[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$forestRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$forestVersion = [regex]::Match((Get-Content (Join-Path $forestRoot 'ProjectSettings/ProjectVersion.txt') -Raw), '(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$forestUnity = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$forestVersion/Editor/Unity.exe"
$forestOutput = Join-Path $forestRoot '.utmp/ForestValidation'
New-Item -ItemType Directory -Force -Path $forestOutput | Out-Null
$forestLog = Join-Path $forestOutput ('import-' + [Guid]::NewGuid() + '.log')
$forestArgs = @('-batchmode','-nographics','-quit','-projectPath',('"{0}"' -f $forestRoot),'-executeMethod','ForestFoundationImporter.Run','-logFile',('"{0}"' -f $forestLog))
$forestJob = Start-Process -FilePath $forestUnity -ArgumentList $forestArgs -WindowStyle Hidden -PassThru
$forestJob.WaitForExit(); $forestJob.Refresh()
Write-Output "Import exit: $($forestJob.ExitCode); log: $forestLog"
if($forestJob.ExitCode -ne 0) { Select-String -LiteralPath $forestLog -Pattern 'error CS','Exception','Aborting'; exit 1 }
