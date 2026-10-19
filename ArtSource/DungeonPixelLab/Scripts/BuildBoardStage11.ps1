$ErrorActionPreference='Stop'
$stageRoot=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path.Replace('\','/')
$stageNative=Join-Path $stageRoot 'Native/Stage11'
$stageLogs=Join-Path $stageRoot 'Diagnostics/Stage11'
$stageEditor='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$stageScript=Join-Path $stageLogs 'BuildFrame.run.js'
$stageText=[IO.File]::ReadAllText((Join-Path $stageRoot 'Scripts/BuildBoardStage11.js')).Replace('__ROOT__',$stageRoot)
[IO.File]::WriteAllText($stageScript,"try {"+$stageText+"} catch(e){console.log('STAGE11_FAILED '+e);} app.command.Exit();")
$stageProcess=Start-Process -FilePath $stageEditor -ArgumentList @('--script',('"'+$stageScript+'"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $stageLogs 'LibreSprite.log') -RedirectStandardError (Join-Path $stageLogs 'LibreSprite-errors.log')
$stageHandle=$stageProcess.Handle
if(-not $stageProcess.WaitForExit(30000)){$stageProcess.Kill();throw 'Only the owned native script process exceeded its 30s limit.'}
$stageProcess.WaitForExit();$stageProcess.Refresh()
if($stageProcess.ExitCode -ne 0 -or -not (Select-String -LiteralPath (Join-Path $stageLogs 'LibreSprite.log') -Pattern '^STAGE11_COMPLETE' -Quiet)){Get-Content -LiteralPath (Join-Path $stageLogs 'LibreSprite.log') -Tail 15;throw 'Native edit failed'}
& (Join-Path $stageRoot '../../Tools/Normalize-FinalizedSourceLayers.ps1') -SourceRoot $stageNative
$stageArguments=@('--batch')
foreach($stageFile in Get-ChildItem -LiteralPath $stageNative -Filter '*.aseprite'){
 $stageArguments+=@(('"'+$stageFile.FullName+'"'),'--save-as',('"'+(Join-Path $stageRoot ('Exports/Stage11/'+$stageFile.BaseName+'.png'))+'"'))
}
$stageExport=Start-Process -FilePath $stageEditor -ArgumentList $stageArguments -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $stageLogs 'LibreSprite-export.log') -RedirectStandardError (Join-Path $stageLogs 'LibreSprite-export-errors.log')
$stageExportHandle=$stageExport.Handle
if(-not $stageExport.WaitForExit(30000)){$stageExport.Kill();throw 'Only the owned export process exceeded its 30s limit.'}
$stageExport.WaitForExit();$stageExport.Refresh()
if($stageExport.ExitCode -ne 0){throw 'Native PNG export failed'}
Remove-Item -LiteralPath $stageScript
Get-ChildItem -LiteralPath (Join-Path $stageRoot 'Exports/Stage11') -Filter '*.png' | Select-Object Name,Length
