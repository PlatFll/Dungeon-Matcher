param([string]$Test='SharedZoneFrameTests.Baseline',[string]$Label='Baseline')
$ErrorActionPreference='Stop'
$project='C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher'
$quote=[char]34
$taskArgs=@('-batchmode','-projectPath',($quote+$project+$quote),'-runTests','-testPlatform','EditMode','-testFilter',$Test,'-testResults',($quote+$project+'/ArtSource/ForestReworkV2/FrameFit/Diagnostics/'+$Label+'.xml'+$quote),'-logFile',($quote+$project+'/ArtSource/ForestReworkV2/FrameFit/Diagnostics/'+$Label+'.log'+$quote))
$taskProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe' -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
$taskProcess.WaitForExit();$taskProcess.Refresh()
Write-Output ($Label+' Unity exit: '+$taskProcess.ExitCode)
exit $taskProcess.ExitCode
