param([string]$Stage='Floor',[string]$Groups='FloorTiles',[string]$Only='')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..')).Replace('\','/')
$kit=Join-Path $project 'ArtSource/DungeonEnvironment/ModularRebuild'
$libre='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$text=[IO.File]::ReadAllText((Join-Path $kit 'Native.js'))+"`n"+[IO.File]::ReadAllText((Join-Path $kit "Correction/${Stage}Correction.js"))
$script=Join-Path $kit "Correction/logs/$Stage.js"
$log=Join-Path $kit "Correction/logs/$Stage.log"
[IO.File]::WriteAllText($script,"try {`n"+$text.Replace('__PROJECT__',$project)+"`n} catch(e) {console.log('BUILD_FAILED '+e);} app.command.Exit();")
$owned=Start-Process -FilePath $libre -ArgumentList @('--script',('"{0}"' -f $script)) -WindowStyle Hidden -PassThru -RedirectStandardOutput $log -RedirectStandardError (Join-Path $kit "Correction/logs/$Stage-errors.log")
$handle=$owned.Handle
if(-not $owned.WaitForExit(240000)){$owned.Kill();throw 'Owned native author exceeded four minutes; inspect correction logs.'}
$owned.WaitForExit();$owned.Refresh()
if($owned.ExitCode -ne 0 -or -not (Select-String -LiteralPath $log -Pattern '^CORRECTION_COMPLETE' -Quiet)){throw "Native $Stage failed: $log"}
foreach($g in $Groups.Split(',')){
 foreach($file in Get-ChildItem -LiteralPath (Join-Path $kit "Editable/$g") -Filter '*.aseprite'){
  if($Only -and -not (@($Only.Split(',')|Where-Object {$file.BaseName.StartsWith($_)}).Count)){continue}
  & (Join-Path $project 'Tools/Normalize-FinalizedSourceLayers.ps1') -SourceRoot $file.FullName
  $out=Join-Path $kit ("Exports/$g/"+$file.BaseName+'.png')
  $exp=Start-Process -FilePath $libre -ArgumentList @('--batch',('"{0}"' -f $file.FullName),'--save-as',('"{0}"' -f $out)) -WindowStyle Hidden -PassThru -Wait -RedirectStandardOutput (Join-Path $kit 'Correction/logs/export.log') -RedirectStandardError (Join-Path $kit 'Correction/logs/export-errors.log')
  if($exp.ExitCode -ne 0){throw "Export failed: $out"}
 }
}
Write-Output "LibreSprite $Stage correction authored and exported."
