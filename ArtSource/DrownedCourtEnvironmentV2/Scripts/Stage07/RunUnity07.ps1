$ErrorActionPreference = 'Stop'
$project = 'C:\Users\USER\.codex\worktrees\drowned-court-pixellab-v2\Dungeon Matcher'
$log = Join-Path $project 'ArtSource\DrownedCourtEnvironmentV2\QA\Stage07\UnityPreview.log'
$exe = 'C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe'
$arguments = '-batchmode -quit -projectPath "' + $project + '" -executeMethod DrownedCourtV2Stage07Preview.Build -logFile "' + $log + '"'
$process = Start-Process -FilePath $exe -ArgumentList $arguments -PassThru -WindowStyle Hidden
$process.WaitForExit()
Write-Output ('Unity exit code: ' + $process.ExitCode)
Get-Content -LiteralPath $log -Tail 16
exit $process.ExitCode
