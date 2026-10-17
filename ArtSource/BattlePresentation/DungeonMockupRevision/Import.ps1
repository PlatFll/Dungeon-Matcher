$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$unity='C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe'
$log=Join-Path $PSScriptRoot 'logs/unity-import.log'
$job=Start-Process -FilePath $unity -ArgumentList @('-batchmode','-quit','-projectPath',('"{0}"' -f $project),'-executeMethod','BattlePresentationImporter.ImportApprovedDungeon','-logFile',('"{0}"' -f $log)) -WindowStyle Hidden -PassThru
$handle=$job.Handle;$job.WaitForExit();$job.Refresh()
if($job.ExitCode -ne 0){Select-String -LiteralPath $log -Pattern 'error CS|Exception|already open|Aborting';throw 'Dungeon-only import failed; see logs/unity-import.log'}
Write-Output "Dungeon-only import exit 0. Log: $log"
