param([string]$ScriptName='NativeCleanup')
$ironRoot=(Split-Path -Parent $PSScriptRoot)
$ironJob=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$ironRoot+'/Scripts/'+$ScriptName+'.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($ironRoot+'/QA/'+$ScriptName+'.log') -RedirectStandardError ($ironRoot+'/QA/'+$ScriptName+'-error.log')
$ironJob.WaitForExit();$ironJob.Refresh()
Get-Content ($ironRoot+'/QA/'+$ScriptName+'.log') -Tail 5
Get-Content ($ironRoot+'/QA/'+$ScriptName+'-error.log') -Tail 4
exit $ironJob.ExitCode
