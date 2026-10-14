param([string]$Filter='ThreeZonePresentationTests;BoardFeel2Tests;BoardFeel2PlayTests;BoardPointerIdentityTests')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$unity='C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe'
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss'
$result=Join-Path $project ".utmp/ThreeZonePresentation/reference-tests-$stamp.xml"
$log=Join-Path $project ".utmp/ThreeZonePresentation/reference-tests-$stamp.log"
New-Item -ItemType Directory -Path (Split-Path $log) -Force | Out-Null
$started=Get-Date
$job=Start-Process -FilePath $unity -ArgumentList @('-batchmode','-projectPath',('"{0}"' -f $project),'-runTests','-testPlatform','EditMode','-testFilter',$Filter,'-testResults',('"{0}"' -f $result),'-logFile',('"{0}"' -f $log)) -WindowStyle Hidden -PassThru
$handle=$job.Handle;$job.WaitForExit();$job.Refresh()
if(!(Test-Path $result) -or (Get-Item $result).LastWriteTime -lt $started){Select-String -LiteralPath $log -Pattern 'error CS|already open|Aborting';throw 'No fresh test result'}
[xml]$report=Get-Content $result
$report.'test-run'|Select-Object result,total,passed,failed,skipped
$report.SelectNodes('//test-case[@result="Failed"]')|ForEach-Object{Write-Output $_.fullname;Write-Output $_.failure.message.InnerText}
Write-Output "Exit: $($job.ExitCode); results: $result; log: $log"
if($job.ExitCode -ne 0 -or [int]$report.'test-run'.failed -gt 0 -or [int]$report.'test-run'.skipped -gt 0){exit 1}
