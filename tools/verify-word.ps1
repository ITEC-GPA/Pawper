param([string]$Examples = "$PSScriptRoot/../examples", [string]$Output = "$PSScriptRoot/../qa/word")
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $Examples).Path
$out = [System.IO.Path]::GetFullPath($Output)
New-Item -ItemType Directory -Force -Path $out | Out-Null
$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
$word.AutomationSecurity = 3
$report = @()
try {
    foreach ($file in Get-ChildItem -LiteralPath $root -Recurse -Filter '*.docx' | Sort-Object FullName) {
        $doc = $null
        try {
            $doc = $word.Documents.Open($file.FullName, $false, $true, $false)
            $mathCount = $doc.OMaths.Count
            $null = $doc.Fields.Update()
            $doc.Repaginate()
            $name = $file.Directory.Name + '-' + $file.BaseName
            $pdf = Join-Path $out ($name + '.pdf')
            $doc.ExportAsFixedFormat($pdf, 17)
            $report += [pscustomobject]@{ File = $file.FullName.Substring($root.Length + 1); Success = $true; Pages = $doc.ComputeStatistics(2); EditableEquations = $mathCount; Tables = $doc.Tables.Count; Pdf = $pdf }
            Write-Output ('WORD OK ' + $name)
        } catch {
            $report += [pscustomobject]@{ File = $file.FullName.Substring($root.Length + 1); Success = $false; Error = $_.Exception.Message }
            Write-Output ('WORD FAIL ' + $file.Name + ': ' + $_.Exception.Message)
        } finally {
            if ($null -ne $doc) { $doc.Close(0); [System.Runtime.InteropServices.Marshal]::ReleaseComObject($doc) | Out-Null }
        }
    }
} finally {
    $word.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($word) | Out-Null
    $report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $out 'word-verification.json') -Encoding utf8
}
if ($report | Where-Object { -not $_.Success }) { exit 1 }
