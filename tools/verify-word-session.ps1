param([string]$ResumeReport='')
$ErrorActionPreference='Stop'
$root=(Resolve-Path "$PSScriptRoot/../examples").Path
$out=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../qa/word'))
New-Item -ItemType Directory -Force -Path $out | Out-Null
$report=@()
if($ResumeReport){$report=@(Get-Content -LiteralPath $ResumeReport -Raw | ConvertFrom-Json | Where-Object Success)}
$word=$null
function Release-Com($value){if($null -ne $value -and [Runtime.InteropServices.Marshal]::IsComObject($value)){[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($value)}}
try {
    foreach($file in Get-ChildItem -LiteralPath $root -Recurse -Filter '*.docx' | Sort-Object FullName){
        $relative=$file.FullName.Substring($root.Length+1)
        if($report.File -contains $relative){continue}
        for($attempt=1;$attempt -le 2;$attempt++){
            $doc=$null; $docs=$null; $maths=$null; $fields=$null; $retry=$false; $ok=$false
            try {
                if($null -eq $word){$word=New-Object -ComObject Word.Application;$word.Visible=$false;$word.DisplayAlerts=0;$word.AutomationSecurity=3}
                $docs=$word.Documents
                $doc=$docs.Open($file.FullName,$false,$true,$false)
                $maths=$doc.OMaths;$mathCount=$maths.Count;Release-Com $maths;$maths=$null
                $fields=$doc.Fields;$null=$fields.Update();Release-Com $fields;$fields=$null
                $doc.Repaginate()
                $pdf=Join-Path $out ($file.Directory.Name+'-'+$file.BaseName+'.pdf')
                $doc.ExportAsFixedFormat($pdf,17)
                $report += [pscustomobject]@{File=$relative;Success=$true;EditableEquations=$mathCount}
                Write-Output ('WORD OK '+$relative)
                $ok=$true
            } catch {
                $message=$_.Exception.Message
                $retry=$attempt -lt 2 -and $message -match 'disconness|RPC|80010108|800706BA'
                if(-not $retry){$report += [pscustomobject]@{File=$relative;Success=$false;Error=$message}}
                Write-Output ('WORD FAIL '+$relative+': '+$message)
            } finally {
                Release-Com $maths;Release-Com $fields
                if($null -ne $doc){try{$doc.Close(0)}catch{};Release-Com $doc}
                Release-Com $docs
            }
            $report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $out 'word-pawper.json') -Encoding utf8
            if($ok -or -not $retry){break}
            try{$word.Quit()}catch{};Release-Com $word;$word=$null
            Write-Output 'Nuovo collegamento Word tra 60 secondi.'
            Start-Sleep -Seconds 60
        }
    }
} finally {
    if($null -ne $word){try{$word.Quit()}catch{};Release-Com $word}
}
if($report | Where-Object {-not $_.Success}){exit 1}
Write-Output ('Verificati '+$report.Count+' documenti.')
exit 0
