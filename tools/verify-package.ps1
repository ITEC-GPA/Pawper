[CmdletBinding()]
param([Parameter(Mandatory)][string]$Path)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Inspect the archive without extracting or executing its content.
$zip = [IO.Compression.ZipFile]::OpenRead([IO.Path]::GetFullPath($Path))
try {
    $entries = @($zip.Entries | Where-Object { $_.Name })
    $roots = @($entries | ForEach-Object { $_.FullName.Split('/')[0] } | Sort-Object -Unique)
    if ($roots.Count -ne 1) { throw 'Il pacchetto deve avere una sola cartella radice.' }
    $prefix = $roots[0] + '/'
    $files = @{}
    foreach ($entry in $entries) {
        $relative = $entry.FullName.Substring($prefix.Length)
        if ($files.ContainsKey($relative)) { throw "Voce duplicata: $relative" }
        if ($relative -match '(^|/)(bin|obj)(/|$)|(^|/)~\$|\.tmp$|(^|/)batch-live-' -or $relative -match '^examples/.*/output\.docx$') {
            throw "File temporaneo o output locale nel pacchetto: $relative"
        }
        $files[$relative] = $entry
    }
    $plugin = @('Pawper.Grasshopper.gha', 'Pawper.Core.dll', 'DocumentFormat.OpenXml.dll', 'DocumentFormat.OpenXml.Framework.dll', 'System.IO.Packaging.dll')
    $actual = @($files.Keys | Where-Object { $_ -like 'plugin/Pawper/*' } | ForEach-Object { $_.Substring('plugin/Pawper/'.Length) })
    if (Compare-Object ($plugin | Sort-Object) ($actual | Sort-Object)) { throw 'Contenuto plugin diverso dai cinque file previsti.' }
    foreach ($required in @('README.md', 'THIRD-PARTY-NOTICES.md', 'Directory.Build.props', 'SHA256SUMS.txt')) {
        if (-not $files.ContainsKey($required)) { throw "File richiesto assente: $required" }
    }
    if (@($files.Keys | Where-Object { $_ -like 'licenses/*' }).Count -eq 0) { throw 'Licenze delle dipendenze assenti.' }
    $definitions = @($files.Keys | Where-Object { $_ -match '^examples/\d{2}-[^/]+/example\.gh$' })
    if ($definitions.Count -ne 20) { throw "Attesi 20 esempi, trovati $($definitions.Count)." }
    foreach ($definition in $definitions) {
        $folder = $definition.Substring(0, $definition.LastIndexOf('/') + 1)
        foreach ($required in @('README.md', 'document.json', 'expected.docx', 'grasshopper-verified.docx')) {
            if (-not $files.ContainsKey($folder + $required)) { throw "Esempio incompleto: $folder$required" }
        }
    }
    $reader = [IO.StreamReader]::new($files['SHA256SUMS.txt'].Open())
    try { $manifest = $reader.ReadToEnd() } finally { $reader.Dispose() }
    $checked = @{}
    foreach ($line in ($manifest -split '\r?\n' | Where-Object { $_ })) {
        if ($line -notmatch '^([a-f0-9]{64})  (.+)$') { throw "Riga manifest non valida: $line" }
        $expected = $Matches[1]; $relative = $Matches[2]
        if (-not $files.ContainsKey($relative) -or $checked.ContainsKey($relative)) { throw "Voce manifest assente o duplicata: $relative" }
        $stream = $files[$relative].Open()
        $sha = [Security.Cryptography.SHA256]::Create()
        try { $hash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '').ToLowerInvariant() }
        finally { $sha.Dispose(); $stream.Dispose() }
        if ($hash -ne $expected) { throw "SHA256 diverso: $relative" }
        $checked[$relative] = $true
    }
    if ($checked.Count -ne $files.Count - 1 -or $checked.ContainsKey('SHA256SUMS.txt')) { throw 'Manifest incompleto.' }
    Write-Output "PACKAGE OK: 20 esempi, 5 file plugin, $($checked.Count) SHA256 verificati."
}
finally { $zip.Dispose() }
