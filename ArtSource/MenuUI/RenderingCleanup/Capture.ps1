param([ValidateSet('before','after')][string]$Stage='before')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$evidence=Join-Path $PSScriptRoot 'evidence'
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
$log=Join-Path $evidence ($Stage+'-unity.log')
$results=Join-Path $evidence ($Stage+'-tests.xml')
$filter=if($Stage -eq 'before'){'MenuUiReviewTests.CaptureMenusAndPreservePause'}else{'MenuUiReviewTests'}
$started=Get-Date
$job=Start-Process 'C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe' -ArgumentList @('-batchmode','-projectPath',('"{0}"' -f $project),'-runTests','-testPlatform','EditMode','-testFilter',$filter,'-testResults',('"{0}"' -f $results),'-logFile',('"{0}"' -f $log)) -WindowStyle Hidden -PassThru
$handle=$job.Handle;$job.WaitForExit();$job.Refresh()
if($job.ExitCode -ne 0 -or !(Test-Path $results)){throw "Unity capture failed: $log"}
[xml]$report=Get-Content $results
if($report.'test-run'.result -ne 'Passed'){throw "Unity test failed: $results"}
$destination=Join-Path $PSScriptRoot ($Stage+'/runtime')
New-Item -ItemType Directory -Force -Path $destination | Out-Null
Get-ChildItem (Join-Path $project '.utmp/MenuUI/after') -Filter '*.png' | Where-Object {$_.LastWriteTime -ge $started} | Copy-Item -Destination $destination -Force
$report.'test-run' | Select-Object result,total,passed,failed,skipped
