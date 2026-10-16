$ErrorActionPreference='Stop'
$exe='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$script=Join-Path $PSScriptRoot 'logs/ApplyCleanup.js'
$job=Start-Process $exe -ArgumentList @('--script',('"{0}"' -f $script)) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $PSScriptRoot 'logs/libresprite-out.log') -RedirectStandardError (Join-Path $PSScriptRoot 'logs/libresprite-err.log')
$handle=$job.Handle
if(!$job.WaitForExit(60000)){
    Stop-Process -Id $job.Id -ErrorAction SilentlyContinue
    throw "LibreSprite exceeded 60 seconds; stopped owned PID $($job.Id). See logs/."
}
$job.Refresh()
if($job.ExitCode -ne 0){throw "LibreSprite exited $($job.ExitCode)"}
foreach($name in @('Hall','HallTall')){
 if(!(Test-Path (Join-Path $PSScriptRoot ('native/'+$name+'.png')))){throw "Missing native output $name"}
}
'LibreSprite edit, save and reopen/export completed.'
