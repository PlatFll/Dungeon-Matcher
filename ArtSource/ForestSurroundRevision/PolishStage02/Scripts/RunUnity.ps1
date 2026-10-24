$ErrorActionPreference='Stop'
$project='C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher'
$root=Join-Path $project 'ArtSource/ForestSurroundRevision/PolishStage02'
$p=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe' -ArgumentList @('-batchmode','-projectPath',('"'+$project+'"'),'-runTests','-testPlatform','EditMode','-testFilter','ForestRootJoinReviewTests.RootJoinReview','-testResults',('"'+$root+'/Diagnostics/UnityTests.xml"'),'-logFile',('"'+$root+'/Diagnostics/UnityTests.log"')) -WorkingDirectory $project -WindowStyle Hidden -PassThru
$handle=$p.Handle
$p.WaitForExit();$p.Refresh()
Write-Output ('UNITY_EXIT='+$p.ExitCode)
if($p.ExitCode -ne 0){throw 'Root-join Unity review failed; inspect saved diagnostics.'}
[xml]$results=Get-Content ($root+'/Diagnostics/UnityTests.xml')
if($results.'test-run'.result -ne 'Passed'){throw 'Unity test XML did not pass'}
Write-Output ('TEST_RESULT='+$results.'test-run'.result)
