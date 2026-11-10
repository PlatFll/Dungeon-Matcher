param([int]$Port = 61347)
$ErrorActionPreference = 'Stop'
$reviewRoot = Split-Path $PSScriptRoot -Parent
$reviewUrl = "http://127.0.0.1:$Port/Stage02/Review.html"
try {
    $existing = Invoke-WebRequest -Uri $reviewUrl -UseBasicParsing -TimeoutSec 2
    if ($existing.Content -match 'Royal bevel') { Write-Output $reviewUrl; exit 0 }
} catch [System.Net.WebException] { }
if (Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue) {
    throw "Port $Port is in use by another review. Choose another port."
}
$reviewProcess = Start-Process -FilePath 'rtk.exe' -ArgumentList @(
    'C:/Users/USER/DungeonMatcherSpriteFoundry/venvs/art/Scripts/python.exe',
    '-m', 'http.server', "$Port", '--bind', '127.0.0.1', '--directory', ('"' + $reviewRoot + '"')
) -WorkingDirectory $reviewRoot -WindowStyle Hidden -PassThru `
  -RedirectStandardOutput "$PSScriptRoot/QA/ReviewServer.out.log" `
  -RedirectStandardError "$PSScriptRoot/QA/ReviewServer.err.log"
$reviewProcess.Id | Set-Content -LiteralPath "$PSScriptRoot/QA/ReviewServer.pid"
for ($attempt = 0; $attempt -lt 10; $attempt++) {
    try {
        $response = Invoke-WebRequest -Uri $reviewUrl -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -eq 200) { Write-Output "Review server PID $($reviewProcess.Id): $reviewUrl"; exit 0 }
    } catch [System.Net.WebException] { Start-Sleep -Milliseconds 300 }
}
throw 'Local review server did not respond.'
