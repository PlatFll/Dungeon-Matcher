param([ValidateSet('pilot','native')][string]$Stage='pilot')
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path (Join-Path $PSScriptRoot 'logs'),(Join-Path $PSScriptRoot $Stage) | Out-Null
$script=((Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ShadingPlan.js') -Raw)+"`n"+(Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Edit.js') -Raw)).Replace('__ROOT__',$PSScriptRoot.Replace('\','/')+'/').Replace('__STAGE__',$Stage)
$file=Join-Path $PSScriptRoot 'logs/Edit.js';[IO.File]::WriteAllText($file,$script)
$out=Join-Path $PSScriptRoot ('logs/'+$Stage+'-libresprite.log')
$job=Start-Process 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"{0}"' -f $file)) -WindowStyle Hidden -PassThru -RedirectStandardOutput $out -RedirectStandardError (Join-Path $PSScriptRoot 'logs/libresprite-error.log')
$handle=$job.Handle
if(!$job.WaitForExit(60000)){Stop-Process -Id $job.Id;throw 'Owned LibreSprite job timed out'}
$job.Refresh()
if($job.ExitCode -ne 0 -or !(Select-String -LiteralPath $out -Pattern 'MENU_CLEANUP_V2_OK' -Quiet)){throw "LibreSprite failed; see $out"}
"LibreSprite $Stage sources saved and re-exported."
