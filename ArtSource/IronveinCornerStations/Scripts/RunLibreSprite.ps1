$cornerRoot=(Split-Path -Parent $PSScriptRoot)
$cornerJob=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$cornerRoot+'/Scripts/Cleanup.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($cornerRoot+'/QA/LibreSprite.log') -RedirectStandardError ($cornerRoot+'/QA/LibreSprite-error.log')
$cornerJob.WaitForExit();$cornerJob.Refresh()
Get-Content ($cornerRoot+'/QA/LibreSprite.log') -Tail 15
Get-Content ($cornerRoot+'/QA/LibreSprite-error.log') -Tail 5
exit $cornerJob.ExitCode
