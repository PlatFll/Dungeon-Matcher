[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$versionText = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Raw
$version = [regex]::Match($versionText, '(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$unityPath = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe"
if ($env:UNITY_EXE) { $unityPath = $env:UNITY_EXE }
$logPath = Join-Path $projectRoot '.utmp/finalized-visual-import.log'
$arguments = @('-batchmode','-nographics','-quit','-projectPath',('"{0}"' -f $projectRoot),'-logFile',('"{0}"' -f $logPath),'-executeMethod','FinalizedVisualTargetsImporter.Run')
$unityJob = Start-Process -FilePath $unityPath -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru
$unityJob.WaitForExit(); $unityJob.Refresh()
Write-Output "Unity import exit: $($unityJob.ExitCode); log: $logPath"
if ($unityJob.ExitCode -ne 0) { Select-String -LiteralPath $logPath -Pattern 'error CS|Exception|Aborting batchmode'; exit 1 }
