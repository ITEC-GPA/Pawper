param([int]$MinimumExample=1)
$ErrorActionPreference='Stop'
$root=(Resolve-Path "$PSScriptRoot/../examples").Path
$out=Join-Path $PSScriptRoot '../qa/word'
New-Item -ItemType Directory -Force -Path $out | Out-Null
$report=@()
foreach($file in Get-ChildItem -LiteralPath $root -Recurse -Filter '*.docx' | Sort-Object FullName) {
    if($file.Directory.Name -match '^(\d+)-' -and [int]$Matches[1] -lt $MinimumExample){continue}
    for($attempt=1;$attempt -le 2;$attempt++) {
        $word=$null; $doc=$null; $step='start'; $success=$false; $retry=$false
        try {
            $word=New-Object -ComObject Word.Application
            $word.Visible=$false; $word.DisplayAlerts=0; $word.AutomationSecurity=3
            $step='open'; $doc=$word.Documents.Open($file.FullName,$false,$true,$false)
            $step='equations'; $math=$doc.OMaths.Count
            $step='fields'; $null=$doc.Fields.Update()
            $step='paginate'; $doc.Repaginate()
            $step='export'; $pdf=Join-Path $out ($file.Directory.Name+'-'+$file.BaseName+'.pdf'); $doc.ExportAsFixedFormat([IO.Path]::GetFullPath($pdf),17)
            $report += [pscustomobject]@{File=$file.FullName.Substring($root.Length+1);Success=$true;EditableEquations=$math}
            Write-Output ('WORD OK '+$file.Directory.Name+'/'+$file.Name)
            $success=$true
        } catch {
            $message=$_.Exception.Message
            $retry=($message -match 'disconness|RPC|80010108|800706BA') -and $attempt -lt 2
            Write-Output ('WORD FAIL '+$file.Directory.Name+'/'+$file.Name+' at '+$step+': '+$message)
            if(-not $retry){$report += [pscustomobject]@{File=$file.FullName.Substring($root.Length+1);Success=$false;Step=$step;Error=$message}}
        } finally {
            if($null -ne $doc){try{$doc.Close(0)}catch{};try{[Runtime.InteropServices.Marshal]::ReleaseComObject($doc)|Out-Null}catch{}}
            if($null -ne $word){try{$word.Quit()}catch{};try{[Runtime.InteropServices.Marshal]::ReleaseComObject($word)|Out-Null}catch{}}
        }
        if($success -or -not $retry){break}
        Write-Output 'Attendo 60 secondi prima di riconnettere Word.'
        Start-Sleep -Seconds 60
    }
    $report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $out ('word-isolated-'+$MinimumExample+'.json')) -Encoding utf8
}
if($report | Where-Object {-not $_.Success}){exit 1}
