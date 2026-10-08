param([ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+(?:-[a-zA-Z0-9.-]+)?$')][string]$Version = '0.1.0')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($PSScriptRoot)
$bin = Join-Path $root 'bin'
$files = @('Pawper.Grasshopper.gha', 'Pawper.Core.dll', 'DocumentFormat.OpenXml.dll', 'DocumentFormat.OpenXml.Framework.dll', 'System.IO.Packaging.dll')
foreach ($name in $files) {
    if (-not (Test-Path -LiteralPath (Join-Path $bin $name) -PathType Leaf)) { throw "Release mancante: eseguire prima ./build.ps1 ($name)." }
}
$dist = Join-Path $root 'dist'
$temporary = Join-Path $dist ('.package-' + [Guid]::NewGuid().ToString('N'))
$stage = Join-Path $temporary "Pawper-$Version"
try {
    New-Item -ItemType Directory -Force -Path "$stage/plugin/Pawper" | Out-Null
    foreach ($name in $files) { Copy-Item -LiteralPath (Join-Path $bin $name) -Destination "$stage/plugin/Pawper/$name" }
    foreach ($name in @('README.md', 'THIRD-PARTY-NOTICES.md', 'Directory.Build.props', 'global.json', 'Pawper.sln', 'build.ps1', 'package.ps1', '.editorconfig', '.gitignore')) {
        Copy-Item -LiteralPath (Join-Path $root $name) -Destination $stage
    }
    foreach ($area in @('src', 'tools', 'examples', 'docs', 'licenses', 'assets')) {
        foreach ($file in Get-ChildItem -LiteralPath (Join-Path $root $area) -Recurse -File) {
            $relative = $file.FullName.Substring($root.Length + 1)
            if ($relative -match '(^|[\\/])(bin|obj)([\\/]|$)' -or $file.Name -like '~$*' -or $file.Name -like '*.tmp' -or $file.Name -like 'batch-live-*') { continue }
            $destination = Join-Path $stage $relative
            New-Item -ItemType Directory -Force -Path (Split-Path $destination) | Out-Null
            Copy-Item -LiteralPath $file.FullName -Destination $destination
        }
    }
    $manifest = foreach ($file in Get-ChildItem -LiteralPath $stage -Recurse -File) {
        (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + $file.FullName.Substring($stage.Length + 1).Replace('\', '/')
    }
    $manifest | Set-Content -LiteralPath "$stage/SHA256SUMS.txt" -Encoding utf8
    Compress-Archive -LiteralPath $stage -DestinationPath "$dist/Pawper-$Version.zip" -Force
    Write-Output "Pacchetto creato: $dist/Pawper-$Version.zip"
}
finally {
    $resolved = [IO.Path]::GetFullPath($temporary)
    if (-not $resolved.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Pulizia pacchetto fuori progetto.' }
    foreach ($path in @($root, $dist, $resolved)) {
        if ((Test-Path -LiteralPath $path) -and ((Get-Item -LiteralPath $path -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw "Pulizia su junction non consentita: $path" }
    }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}