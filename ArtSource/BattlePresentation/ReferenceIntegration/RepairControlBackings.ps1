$ErrorActionPreference='Stop'
$out=Join-Path $PSScriptRoot 'native'
$logs=Join-Path $PSScriptRoot 'evidence'
foreach($zone in @('Forest','Court')){New-Item -ItemType Directory -Force -Path (Join-Path $out $zone) | Out-Null}
$source=(Get-Content (Join-Path $PSScriptRoot 'RepairControlBackings.js') -Raw).Replace('__BASE__',(Split-Path $PSScriptRoot).Replace('\','/')).Replace('__OUT__',$out.Replace('\','/'))
$script=Join-Path $logs 'repair-control-backings.js'
[IO.File]::WriteAllText($script,$source)
$job=Start-Process 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"{0}"' -f $script)) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $logs 'repair.log') -RedirectStandardError (Join-Path $logs 'repair-errors.log')
$handle=$job.Handle
if(!$job.WaitForExit(60000)){$job.Kill();throw 'Owned repair exceeded sixty seconds'}
$job.Refresh()
if($job.ExitCode -ne 0 -or !(Select-String (Join-Path $logs 'repair.log') -Pattern '^REPAIR_COMPLETE' -Quiet)){throw 'LibreSprite repair failed'}
Get-Content (Join-Path $logs 'repair.log')
