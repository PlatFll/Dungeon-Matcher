param([int]$Port=8881)
$ErrorActionPreference='Stop'
$python='C:/Users/USER/DungeonMatcherSpriteFoundry/venvs/art/Scripts/python.exe'
$probe=[Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback,$Port)
try{$probe.Start()}finally{$probe.Stop()}
$job=Start-Process -FilePath $python -ArgumentList @('-m','http.server',"$Port",'--bind','127.0.0.1','--directory',('"{0}"' -f $PSScriptRoot)) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $PSScriptRoot 'logs/server.log') -RedirectStandardError (Join-Path $PSScriptRoot 'logs/server-errors.log')
$job.Id | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'logs/server.pid')
Write-Output "Review server PID $($job.Id): http://127.0.0.1:$Port/Review.html"
