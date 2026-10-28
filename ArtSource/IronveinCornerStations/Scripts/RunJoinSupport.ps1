$cornerRoot=(Split-Path -Parent $PSScriptRoot)
$cornerJob=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$cornerRoot+'/Scripts/JoinSupport.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($cornerRoot+'/QA/JoinSupport.log') -RedirectStandardError ($cornerRoot+'/QA/JoinSupport-error.log')
$cornerJob.WaitForExit();$cornerJob.Refresh()
Get-Content ($cornerRoot+'/QA/JoinSupport.log') -Tail 15
Get-Content ($cornerRoot+'/QA/JoinSupport-error.log') -Tail 5
exit $cornerJob.ExitCode
