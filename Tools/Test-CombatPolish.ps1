param([switch]$Full)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$unity='C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe'
$filter='CombatPolishTests;CombatPolishPlayTests;EnemyDamageResultTests;BalanceV1Tests'
if($Full){$filter+=';DesignV2ContinuationTests;DesignV2Tests;BalanceLifecyclePlayTests;BalanceDisruptionPlayTests;BalanceFreshShapesPlayTests;RunUpgradeSystemTests;GameOverRecoveryTests;CrystalChainAttributionTests;CrystalBoardValidityTests;ReshufflePinReservationTests;WaveDeathLifecycleTests;EnemySpecialExecutionGuardTests;BarricadeBannerGravityTests;RoyalAssaultParticipantLifetimeTests;TownMarshalStaggerTests;BoardPointerIdentityTests'}
$result=Join-Path $root '.utmp/combat-polish-tests.xml';$log=Join-Path $root '.utmp/combat-polish-tests.log';$started=Get-Date
$job=Start-Process -FilePath $unity -ArgumentList @('-batchmode','-nographics','-projectPath',('"{0}"' -f $root),'-runTests','-testPlatform','EditMode','-testFilter',$filter,'-testResults',('"{0}"' -f $result),'-logFile',('"{0}"' -f $log)) -WorkingDirectory $root -WindowStyle Hidden -PassThru
$processHandle=$job.Handle
$job.WaitForExit();$job.Refresh()
if(!(Test-Path $result) -or (Get-Item $result).LastWriteTime -lt $started){Select-String -LiteralPath $log -Pattern 'error CS|already open|Aborting batchmode';throw 'No fresh test results'}
[xml]$report=Get-Content $result
$report.'test-run'|Select-Object result,total,passed,failed,skipped
$report.SelectNodes('//test-case[@result="Failed"]')|ForEach-Object{Write-Output $_.fullname;Write-Output $_.failure.message.InnerText}
Write-Output "Exit: $($job.ExitCode); results: $result; log: $log"
if($job.ExitCode -ne 0 -or [int]$report.'test-run'.failed -gt 0 -or [int]$report.'test-run'.skipped -gt 0){exit 1}
