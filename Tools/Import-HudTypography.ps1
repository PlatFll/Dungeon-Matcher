[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$version = [regex]::Match((Get-Content (Join-Path $root 'ProjectSettings/ProjectVersion.txt') -Raw), '(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$unityPath = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe"
if ($env:UNITY_EXE) { $unityPath = $env:UNITY_EXE }
$logPath = Join-Path $root '.utmp/hud-typography-import.log'
$job = Start-Process -FilePath $unityPath -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"{0}"' -f $root),'-logFile',('"{0}"' -f $logPath),'-executeMethod','HudTypographyImporter.Run') -WindowStyle Hidden -PassThru
$job.WaitForExit(); $job.Refresh()
Write-Output "HUD import exit: $($job.ExitCode); log: $logPath"
if ($job.ExitCode -ne 0) { Select-String -LiteralPath $logPath -Pattern 'error CS|Exception|Aborting batchmode'; exit 1 }
