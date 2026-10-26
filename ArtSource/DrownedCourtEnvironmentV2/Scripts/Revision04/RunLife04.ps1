$lifeProject='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher'
$lifeLog=$lifeProject+'/ArtSource/DrownedCourtEnvironmentV2/QA/Revision04/UnityBuild.log'
$lifeJob=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe' -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$lifeProject+'"'),'-executeMethod','DrownedCourtV2Life04Preview.Build','-logFile',('"'+$lifeLog+'"')) -WindowStyle Hidden -PassThru
$lifeJob.WaitForExit();$lifeJob.Refresh()
Select-String -LiteralPath $lifeLog -Pattern 'DROWNED_COURT_LIFE04_PASS|error CS|AssertionException|Exception' | Select-Object -Last 12
exit $lifeJob.ExitCode
