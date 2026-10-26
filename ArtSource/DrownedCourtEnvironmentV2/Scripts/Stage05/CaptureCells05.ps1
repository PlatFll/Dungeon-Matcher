$ErrorActionPreference='Stop'
$project='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher'
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'DrownedCourtV2Stage05CellCapture.cs') -Destination ($project+'/Assets/_Game/Scripts/Editor/DrownedCourtV2Stage05CellCapture.cs')
$arguments=@('-batchmode','-projectPath',('"'+$project+'"'),'-runTests','-testPlatform','EditMode','-testFilter','DrownedCourtV2Stage05CellCapture.CaptureCells','-testResults',('"'+$project+'/ArtSource/DrownedCourtEnvironmentV2/QA/Stage05/CellSnapshotTests.xml"'),'-logFile',('"'+$project+'/ArtSource/DrownedCourtEnvironmentV2/QA/Stage05/CellSnapshot.log"'))
$job=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe' -ArgumentList $arguments -WindowStyle Hidden -PassThru
$job.WaitForExit();$job.Refresh();Write-Output ('Cell snapshot Unity exit: '+$job.ExitCode);exit $job.ExitCode
