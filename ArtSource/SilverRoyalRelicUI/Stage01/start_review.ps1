param([int]$Port = 61347)
$ErrorActionPreference = 'Stop'
$reviewFolder = $PSScriptRoot
$reviewUrl = "http://127.0.0.1:$Port/Review.html"
foreach ($candidateUrl in @($reviewUrl, "http://127.0.0.1:$Port/ArtSource/SilverRoyalRelicUI/Stage01/Review.html")) {
    try {
        $existing = Invoke-WebRequest -Uri $candidateUrl -UseBasicParsing -TimeoutSec 2
        if ($existing.Content -match 'Silver Royal Relic') { Write-Output $candidateUrl; exit 0 }
    } catch [System.Net.WebException] { }
}
if (Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue) {
    throw "Port $Port already serves a different review. Choose another port."
}
$reviewProcess = Start-Process -FilePath 'rtk.exe' -ArgumentList @(
    'C:/Users/USER/DungeonMatcherSpriteFoundry/venvs/art/Scripts/python.exe',
    '-m', 'http.server', "$Port", '--bind', '127.0.0.1', '--directory', ('"' + $reviewFolder + '"')
) -WorkingDirectory $reviewFolder -WindowStyle Hidden -PassThru `
  -RedirectStandardOutput "$reviewFolder/QA/ReviewServer.out.log" `
  -RedirectStandardError "$reviewFolder/QA/ReviewServer.err.log"
$reviewProcess.Id | Set-Content -LiteralPath "$reviewFolder/QA/ReviewServer.pid"
Write-Output "Review server PID $($reviewProcess.Id): $reviewUrl"
