[CmdletBinding()]
param([ValidateSet('Import','Tests','Visuals')][string]$Mode = 'Tests', [string]$TestFilter = 'ChronoShutterTests')
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$versionText = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Raw
$version = [regex]::Match($versionText, '(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$unityPath = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe"
if ($env:UNITY_EXE) { $unityPath = $env:UNITY_EXE }
New-Item -ItemType Directory -Force -Path (Join-Path $projectRoot '.utmp') | Out-Null
$logPath = Join-Path $projectRoot ('.utmp/gideon-'+$Mode.ToLower()+'.log')
$resultPath = Join-Path $projectRoot '.utmp/gideon-tests.xml'
$arguments = @('-batchmode','-projectPath',('"{0}"' -f $projectRoot),'-logFile',('"{0}"' -f $logPath))
if ($Mode -ne 'Visuals') { $arguments += '-nographics' }
if ($Mode -eq 'Import') { $arguments += @('-quit','-executeMethod','GideonGlassImporter.Run') }
elseif ($Mode -eq 'Visuals') { $arguments += @('-executeMethod','GideonGlassVisualValidation.Run') }
else {
    $arguments += @('-runTests','-testPlatform','EditMode','-testResults',('"{0}"' -f $resultPath))
    if ($TestFilter -ne 'All') { $arguments += @('-testFilter',$TestFilter) }
}
$started = Get-Date
$unityJob = Start-Process -FilePath $unityPath -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru
$unityJob.WaitForExit(); $unityJob.Refresh()
Write-Output "Unity exit: $($unityJob.ExitCode); log: $logPath"
if ($unityJob.ExitCode -ne 0) {
    Select-String -LiteralPath $logPath -Pattern 'error CS|already open|Aborting batchmode'
}
if ($Mode -eq 'Tests') {
    if (!(Test-Path -LiteralPath $resultPath) -or (Get-Item -LiteralPath $resultPath).LastWriteTime -lt $started) { throw 'No fresh test results.' }
    [xml]$result = Get-Content -LiteralPath $resultPath
    $result.'test-run' | Select-Object result,total,passed,failed,skipped
    $result.SelectNodes('//test-case[@result="Failed"]') | ForEach-Object { Write-Output $_.fullname; Write-Output $_.failure.message.InnerText }
    if ([int]$result.'test-run'.failed -gt 0 -or [int]$result.'test-run'.skipped -gt 0) { exit 1 }
}
if ($unityJob.ExitCode -ne 0) { exit 1 }
