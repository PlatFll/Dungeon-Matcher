$revisionProject='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher'
$revisionLog=$revisionProject+'/ArtSource/DrownedCourtEnvironmentV2/QA/Revision02/UnityBuild.log'
$unityJob=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe' -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$revisionProject+'"'),'-executeMethod','DrownedCourtV2Polish02Preview.Build','-logFile',('"'+$revisionLog+'"')) -PassThru -WindowStyle Hidden
$unityJob.WaitForExit();$unityJob.Refresh()
Select-String -LiteralPath $revisionLog -Pattern 'DROWNED_COURT_POLISH02_PASS|error CS|Exception|AssertionException|error:' | Select-Object -Last 12
exit $unityJob.ExitCode
