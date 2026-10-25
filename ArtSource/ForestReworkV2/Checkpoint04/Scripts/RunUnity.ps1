$ErrorActionPreference='Stop'
$project='C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher'
$quote=[char]34
$taskArgs=@('-batchmode','-projectPath',($quote+$project+$quote),'-runTests','-testPlatform','EditMode','-testFilter','ForestReworkV2Checkpoint04Tests.BoardReview','-testResults',($quote+$project+'/ArtSource/ForestReworkV2/Checkpoint04/Diagnostics/UnityTests.xml'+$quote),'-logFile',($quote+$project+'/ArtSource/ForestReworkV2/Checkpoint04/Diagnostics/UnityTests.log'+$quote))
$taskProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe' -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
$taskProcess.WaitForExit()
$taskProcess.Refresh()
Write-Output ('Forest v2 checkpoint 4 Unity exit: '+$taskProcess.ExitCode)
exit $taskProcess.ExitCode
