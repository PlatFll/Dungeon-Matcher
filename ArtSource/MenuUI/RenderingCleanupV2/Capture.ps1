$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$log=Join-Path $PSScriptRoot 'evidence/unity-menu-tests.log'
$results=Join-Path $PSScriptRoot 'evidence/unity-menu-tests.xml'
$job=Start-Process 'C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe' -ArgumentList @('-batchmode','-projectPath',('"{0}"' -f $project),'-runTests','-testPlatform','EditMode','-testFilter','MenuUiReviewTests','-testResults',('"{0}"' -f $results),'-logFile',('"{0}"' -f $log)) -WindowStyle Hidden -PassThru
$handle=$job.Handle;$job.WaitForExit();$job.Refresh()
if($job.ExitCode -ne 0 -or !(Test-Path $results)){throw "Unity capture failed: $log"}
[xml]$report=Get-Content $results
if($report.'test-run'.result -ne 'Passed'){throw "Unity test failed: $results"}
$report.'test-run' | Select-Object result,total,passed,failed,skipped
