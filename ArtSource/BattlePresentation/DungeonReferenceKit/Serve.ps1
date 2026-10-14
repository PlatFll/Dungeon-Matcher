$ErrorActionPreference='Stop'
$pythonPath='C:/Users/USER/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$reviewRoot=Split-Path $PSScriptRoot
$listener=Get-NetTCPConnection -LocalPort 8879 -State Listen -ErrorAction SilentlyContinue
if($listener){Write-Output 'Port 8879 already has a listener; verify the review URL.';exit 0}
$job=Start-Process -FilePath $pythonPath -ArgumentList @('-m','http.server','8879','--bind','127.0.0.1','--directory',('"{0}"' -f $reviewRoot)) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $PSScriptRoot 'logs/server.log') -RedirectStandardError (Join-Path $PSScriptRoot 'logs/server-errors.log')
Write-Output ('Local review server PID '+$job.Id)
