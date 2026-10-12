[CmdletBinding()]
param([switch]$Verify, [switch]$Definitions)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$projectRoot = [IO.Path]::GetFullPath($PSScriptRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
$releaseDirectory = Join-Path $projectRoot 'bin'
$buildDirectory = Join-Path $projectRoot ('.pawper-build-' + [Guid]::NewGuid().ToString('N'))
$releaseFiles = @('Pawper.Grasshopper.gha', 'Pawper.Core.dll', 'DocumentFormat.OpenXml.dll', 'DocumentFormat.OpenXml.Framework.dll', 'System.IO.Packaging.dll')

function Assert-ProjectPath([string]$Path) {
    $resolved = [IO.Path]::GetFullPath($Path)
    if (-not $resolved.StartsWith($projectRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Operazione fuori dalla cartella del progetto: $resolved"
    }
    $current = $resolved
    while ($current -and $current -ne $projectRoot) {
        if (Test-Path -LiteralPath $current) {
            if ((Get-Item -LiteralPath $current -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw "Operazione su collegamento o junction non consentita: $current"
            }
        }
        $current = [IO.Path]::GetDirectoryName($current)
    }
}

function Remove-ProjectDirectory([string]$Path) {
    Assert-ProjectPath $Path
    if (Test-Path -LiteralPath $Path) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Remove-PreviousBuildOutputs {
    foreach ($area in @('src', 'tools')) {
        $folders = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot $area) -Directory -Recurse -Force |
            Where-Object { $_.Name -in @('bin', 'obj') } | Sort-Object { $_.FullName.Length })
        foreach ($folder in $folders) { Remove-ProjectDirectory $folder.FullName }
    }
    foreach ($name in @('obj', 'artifacts', '.artifacts', 'dist')) {
        Remove-ProjectDirectory (Join-Path $projectRoot $name)
    }
    foreach ($folder in Get-ChildItem -LiteralPath $projectRoot -Directory -Force |
        Where-Object { $_.Name -like '.pawper-build-*' -and $_.FullName -ne $buildDirectory }) {
        Remove-ProjectDirectory $folder.FullName
    }
}

# Prevent two builds from publishing or cleaning each other's temporary output.
$mutexHash = [Security.Cryptography.SHA256]::Create()
try { $mutexKey = [BitConverter]::ToString($mutexHash.ComputeHash([Text.Encoding]::UTF8.GetBytes($projectRoot.ToLowerInvariant()))).Replace('-', '') }
finally { $mutexHash.Dispose() }
$mutex = [Threading.Mutex]::new($false, 'Local\PawperBuild-' + $mutexKey)
$acquired = $false
try {
    try { $acquired = $mutex.WaitOne(0) } catch [Threading.AbandonedMutexException] { $acquired = $true }
    if (-not $acquired) { throw 'Una compilazione Pawper è già in corso in questa cartella.' }
    Assert-ProjectPath $buildDirectory
    Assert-ProjectPath $releaseDirectory
    New-Item -ItemType Directory -Path $buildDirectory | Out-Null
    Push-Location $projectRoot
    try {
        $properties = @("-p:PawperBuildRoot=$buildDirectory", '-p:Configuration=Release', '-p:UseSharedCompilation=false')
        $restored = $false
        for ($attempt = 1; $attempt -le 5; $attempt++) {
            & dotnet restore Pawper.sln @properties --disable-parallel
            if ($LASTEXITCODE -eq 0) { $restored = $true; break }
            if ($attempt -lt 5) {
                Write-Host 'Ripristino dipendenze fallito. Nuovo tentativo tra 60 secondi.'
                Start-Sleep -Seconds 60
            }
        }
        if (-not $restored) { throw 'Ripristino dipendenze fallito; bin precedente conservata.' }
        & dotnet build Pawper.sln -c Release --no-restore --disable-build-servers @properties
        if ($LASTEXITCODE -ne 0) { throw 'Compilazione fallita; bin precedente conservata.' }

        # Test the real Grasshopper startup callback before publishing any build.
        & dotnet (Join-Path $buildDirectory 'bin/Pawper.Definitions/Release/net8.0-windows/Pawper.Definitions.dll') examples --loading-check
        if ($LASTEXITCODE -ne 0) { throw 'Registrazione Grasshopper fallita; bin precedente conservata.' }

        if ($Verify) {
            & dotnet (Join-Path $buildDirectory 'bin/Pawper.Verify/Release/net8.0/Pawper.Verify.dll') examples
            if ($LASTEXITCODE -ne 0) { throw 'Verifiche motore fallite; bin precedente conservata.' }
        }
        if ($Definitions) {
            & dotnet (Join-Path $buildDirectory 'bin/Pawper.Definitions/Release/net8.0-windows/Pawper.Definitions.dll') examples
            if ($LASTEXITCODE -ne 0) { throw 'Verifiche Grasshopper fallite; bin precedente conservata.' }
        }

        $compiled = Join-Path $buildDirectory 'bin/Pawper.Grasshopper/Release/net8.0-windows'
        $staged = Join-Path $buildDirectory 'release'
        $previous = Join-Path $buildDirectory 'previous-release'
        New-Item -ItemType Directory -Path $staged | Out-Null
        foreach ($name in $releaseFiles) {
            $source = Join-Path $compiled $name
            if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "File Release mancante: $name" }
            Copy-Item -LiteralPath $source -Destination (Join-Path $staged $name)
        }
        foreach ($path in @($staged, $previous, $releaseDirectory)) { Assert-ProjectPath $path }
        # Fail before moving a release that Rhino is still using.
        foreach ($name in $releaseFiles) {
            $installedFile = Join-Path $releaseDirectory $name
            if (Test-Path -LiteralPath $installedFile -PathType Leaf) {
                try {
                    $probe = [IO.File]::Open($installedFile, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
                    $probe.Dispose()
                }
                catch { throw "File Release in uso o non accessibile: $installedFile. Chiudere Rhino prima di compilare. La bin precedente è conservata." }
            }
        }
        if (Test-Path -LiteralPath $releaseDirectory) { [IO.Directory]::Move($releaseDirectory, $previous) }
        try { [IO.Directory]::Move($staged, $releaseDirectory) }
        catch {
            if (Test-Path -LiteralPath $previous) { [IO.Directory]::Move($previous, $releaseDirectory) }
            throw
        }
        Remove-PreviousBuildOutputs
        Write-Host "Release pronta in $releaseDirectory ($($releaseFiles.Count) file)."
    }
    finally {
        Pop-Location
        Remove-ProjectDirectory $buildDirectory
    }
}
finally {
    if ($acquired) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}

