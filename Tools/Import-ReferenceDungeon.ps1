$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$unity='C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe'
$log=Join-Path $project '.utmp/ThreeZonePresentation/reference-import.log'
New-Item -ItemType Directory -Path (Split-Path $log) -Force | Out-Null
$job=Start-Process -FilePath $unity -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"{0}"' -f $project),'-logFile',('"{0}"' -f $log),'-executeMethod','BattlePresentationImporter.ImportApprovedReferenceKits') -WindowStyle Hidden -PassThru
$handle=$job.Handle;$job.WaitForExit();$job.Refresh()
Write-Output "Unity reference import exit: $($job.ExitCode); log: $log"
if($job.ExitCode -ne 0){Select-String -LiteralPath $log -Pattern 'error CS|Exception|Aborting';exit 1}
