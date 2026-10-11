param([string]$UnityExe='C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe')
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$output=Join-Path $projectRoot '.utmp/EnemyArtCohesion/Unity'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$log=Join-Path $output 'editor.log'
$artProcess=Start-Process -FilePath $UnityExe -ArgumentList @('-batchmode','-projectPath',('"{0}"' -f $projectRoot),'-executeMethod','EnemyArtCohesionValidation.Run','-logFile',('"{0}"' -f $log)) -WindowStyle Hidden -PassThru
$artHandle=$artProcess.Handle
$artProcess.WaitForExit();$artProcess.Refresh()
if($artProcess.ExitCode -ne 0){throw "Enemy art validation failed ($($artProcess.ExitCode)); see $log"}
Get-Content -LiteralPath (Join-Path $output 'assets.txt')
Get-Content -LiteralPath (Join-Path $output 'presentation.txt')
