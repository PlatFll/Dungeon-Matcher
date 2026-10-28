param()
$stageRoot=(Split-Path -Parent $PSScriptRoot)
$nativeJob=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$stageRoot+'/Scripts/Cleanup.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($stageRoot+'/QA/LibreSprite.log') -RedirectStandardError ($stageRoot+'/QA/LibreSprite-error.log')
$nativeJob.WaitForExit();$nativeJob.Refresh()
Get-Content ($stageRoot+'/QA/LibreSprite.log') -Tail 12
Get-Content ($stageRoot+'/QA/LibreSprite-error.log') -Tail 5
exit $nativeJob.ExitCode
