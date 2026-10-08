[CmdletBinding()]
param([int]$Port=8891)
$ErrorActionPreference='Stop'
$reviewRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../.utmp/Ironvein/ReviewDelivery'))
if(-not(Test-Path -LiteralPath (Join-Path $reviewRoot 'Review.html'))){throw 'Build the review with Review-IronveinDelivery.py first.'}
$reviewState=Join-Path $reviewRoot '../review-server.json'
if(Test-Path -LiteralPath $reviewState)
{
    $prior=Get-Content -LiteralPath $reviewState -Raw | ConvertFrom-Json
    $existing=Get-CimInstance Win32_Process -Filter ('ProcessId='+$prior.processId) -ErrorAction SilentlyContinue
    if($existing -and $existing.CommandLine -like ('*'+$reviewRoot+'*')){Write-Output $prior.url;exit 0}
}
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
if(-not(Test-Path -LiteralPath $python)){throw 'Configured local Python runtime is unavailable.'}
if(Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue){throw ('Port '+$Port+' is already in use; choose a different port.')}
$args=@('-m','http.server',$Port,'--bind','127.0.0.1','--directory',('"'+$reviewRoot+'"'))
$process=Start-Process -FilePath $python -ArgumentList $args -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput (Join-Path $reviewRoot '../review-server.out.log') `
    -RedirectStandardError (Join-Path $reviewRoot '../review-server.err.log')
$url='http://127.0.0.1:'+$Port+'/Review.html'
$state=@{processId=$process.Id;root=$reviewRoot;url=$url;startedUtc=[DateTime]::UtcNow.ToString('o')}
$state | ConvertTo-Json | Set-Content -LiteralPath $reviewState -Encoding UTF8
for($attempt=0;$attempt -lt 20;$attempt++)
{
    try { $response=Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2;if($response.StatusCode -eq 200){Write-Output $url;exit 0} }
    catch { Start-Sleep -Milliseconds 100 }
}
throw ('Review server did not respond; inspect '+$reviewState)
