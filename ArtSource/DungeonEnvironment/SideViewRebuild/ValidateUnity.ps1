param([ValidateSet('Build','Test')][string]$Mode='Build')
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path.Replace('\','/')
$kit=Join-Path $project 'ArtSource/DungeonEnvironment/SideViewRebuild'
New-Item -ItemType Directory -Force -Path (Join-Path $kit 'logs') | Out-Null
$unity='C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe'
$args=@('-batchmode','-projectPath',('"{0}"' -f $project),'-logFile',('"{0}"' -f (Join-Path $kit "logs/Unity$Mode.log")))
if($Mode -eq 'Build'){$args+=@('-executeMethod','DungeonSideViewPreviewTools.BuildBatch','-quit')}
else{$args+=@('-runTests','-testPlatform','EditMode','-testFilter','DungeonSideViewPreviewTests','-testResults',('"{0}"' -f (Join-Path $kit 'Evidence/UnityTests.xml')))}
$owned=Start-Process -FilePath $unity -ArgumentList $args -WindowStyle Hidden -PassThru
$handle=$owned.Handle
if(-not $owned.WaitForExit(600000)){$owned.Kill();throw "Owned Unity $Mode exceeded ten minutes; see its log."}
$owned.WaitForExit();$owned.Refresh()
if($owned.ExitCode -ne 0){throw "Unity $Mode failed with $($owned.ExitCode); see logs/Unity$Mode.log"}
if($Mode -eq 'Build' -and -not(Select-String -LiteralPath (Join-Path $kit 'logs/UnityBuild.log') -Pattern 'DUNGEON_SIDEVIEW_BUILD_PASSED' -Quiet)){throw 'Missing Unity build completion marker'}
Write-Output "Unity $Mode completed with exit $($owned.ExitCode)."
