$ErrorActionPreference='Stop'
$bpProject=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$bpVersion=[regex]::Match((Get-Content -LiteralPath (Join-Path $bpProject 'ProjectSettings/ProjectVersion.txt') -Raw),'(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$bpUnity=Join-Path $env:ProgramFiles "Unity/Hub/Editor/$bpVersion/Editor/Unity.exe"
$bpLog=Join-Path $bpProject '.utmp/ThreeZonePresentation/import.log'
$bpArgs=@('-batchmode','-nographics','-quit','-projectPath',('"{0}"' -f $bpProject),'-logFile',('"{0}"' -f $bpLog),'-executeMethod','BattlePresentationImporter.Run')
$bpJob=Start-Process -FilePath $bpUnity -ArgumentList $bpArgs -WindowStyle Hidden -PassThru
$bpJob.WaitForExit();$bpJob.Refresh()
Write-Output "Unity import exit: $($bpJob.ExitCode); log: $bpLog"
if($bpJob.ExitCode -ne 0){Select-String -LiteralPath $bpLog -Pattern 'error CS|Exception|Aborting';exit 1}
