[CmdletBinding()]
param([string]$UnityPath = $env:UNITY_EXE)
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$versionText = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Raw
$version = [regex]::Match($versionText, '(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
if (-not $UnityPath) { $UnityPath = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe" }
if (-not (Test-Path -LiteralPath $UnityPath)) { throw "Install Unity $version or provide -UnityPath." }
$outputRoot = Join-Path $projectRoot '.utmp'
New-Item -Path $outputRoot -ItemType Directory -Force | Out-Null
$resultPath = Join-Path $outputRoot 'balance-editmode.xml'
$logPath = Join-Path $outputRoot 'balance-editmode.log'
$testFilter = 'DesignV2ContinuationTests;DesignV2Tests;BalanceV1Tests;BalanceLifecyclePlayTests;BalanceDisruptionPlayTests;BalanceFreshShapesPlayTests;RunUpgradeSystemTests;GameOverRecoveryTests;EnemyDamageResultTests;CrystalChainAttributionTests;CrystalBoardValidityTests;ReshufflePinReservationTests;WaveDeathLifecycleTests;EnemySpecialExecutionGuardTests;BarricadeBannerGravityTests;RoyalAssaultParticipantLifetimeTests;TownMarshalStaggerTests;BoardPointerIdentityTests'
$started = Get-Date
$arguments = @('-batchmode', '-nographics', '-projectPath', ('"{0}"' -f $projectRoot), '-runTests', '-testPlatform', 'EditMode', '-testFilter', $testFilter, '-testResults', ('"{0}"' -f $resultPath), '-logFile', ('"{0}"' -f $logPath))
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru
$process.WaitForExit()
$process.Refresh()
Write-Output "Unity process exit: $($process.ExitCode); wall time: $([Math]::Round(((Get-Date)-$started).TotalSeconds,1)) seconds"
if (-not (Test-Path -LiteralPath $resultPath) -or (Get-Item -LiteralPath $resultPath).LastWriteTime -lt $started) {
    Select-String -LiteralPath $logPath -Pattern 'error CS|already open|Aborting batchmode'
    throw "Unity produced no new test result. See $logPath"
}
[xml]$result = Get-Content -LiteralPath $resultPath
$result.'test-run' | Select-Object result,total,passed,failed,skipped
$result.SelectNodes('//test-case[@result="Failed"]') | ForEach-Object { Write-Output $_.fullname; Write-Output $_.failure.message.InnerText }
if ($process.ExitCode -ne 0 -or [int]$result.'test-run'.failed -gt 0 -or [int]$result.'test-run'.skipped -gt 0) { exit 1 }
Write-Output "Focused regression suite passed. Results: $resultPath"
