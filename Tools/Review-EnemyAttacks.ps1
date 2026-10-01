param([switch]$ImportOnly,[switch]$SkipImport)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$unity='C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe'
$log=Join-Path $root '.utmp/enemy-attacks-import.log'
if(!$SkipImport){
$job=Start-Process -FilePath $unity -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"{0}"' -f $root),'-executeMethod','CombatActionImporter.ImportPixelLabEnemies','-logFile',('"{0}"' -f $log)) -WorkingDirectory $root -WindowStyle Hidden -PassThru
$processHandle=$job.Handle
$job.WaitForExit();$job.Refresh();if($job.ExitCode -ne 0){Select-String -LiteralPath $log -Pattern 'error CS|Exception|Aborting';exit 1}
}
if($ImportOnly){Write-Output "Import passed: $log";exit 0}
$result=Join-Path $root '.utmp/enemy-attacks-tests.xml';$log=Join-Path $root '.utmp/enemy-attacks-tests.log';$started=Get-Date
$job=Start-Process -FilePath $unity -ArgumentList @('-batchmode','-projectPath',('"{0}"' -f $root),'-runTests','-testPlatform','EditMode','-testFilter','PixelLabAttackPlayTests','-testResults',('"{0}"' -f $result),'-logFile',('"{0}"' -f $log)) -WorkingDirectory $root -WindowStyle Hidden -PassThru
$processHandle=$job.Handle
$job.WaitForExit();$job.Refresh()
if(!(Test-Path $result) -or (Get-Item $result).LastWriteTime -lt $started){Select-String -LiteralPath $log -Pattern 'error CS|Exception|Aborting';throw 'No fresh test results'}
[xml]$report=Get-Content $result
$report.'test-run'|Select-Object result,total,passed,failed
$report.SelectNodes('//test-case[@result="Failed"]')|ForEach-Object{Write-Output $_.failure.message.InnerText;Write-Output $_.failure.'stack-trace'.InnerText}
Write-Output "Exit: $($job.ExitCode); $result; $log"
if($job.ExitCode -ne 0 -or [int]$report.'test-run'.failed -gt 0){exit 1}
