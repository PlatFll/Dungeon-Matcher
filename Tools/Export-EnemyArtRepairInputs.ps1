param([string[]]$Actors=@('seismic_smith'),[switch]$All,[string]$LibreSprite='C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe')
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$plans=Get-Content (Join-Path $projectRoot 'ArtSource/EnemyArtCohesion/edge-repair-plan.json') -Raw | ConvertFrom-Json
$output=Join-Path $projectRoot '.utmp/EnemyArtCohesion/RepairInputs'
New-Item -ItemType Directory -Path $output -Force | Out-Null
foreach($plan in $plans | Where-Object {($All -or $_.actor -in $Actors) -and !$_.duplicate_of}){
 $source=Join-Path $projectRoot ('ArtSource/EnemyArtCohesion/Native/'+$plan.stem+'.aseprite')
 $exportProcess=Start-Process -FilePath $LibreSprite -ArgumentList @('-b','--frame-range',("{0},{0}" -f $plan.frame),('"{0}"' -f $source),'--sheet-type','horizontal','--sheet',('"{0}"' -f (Join-Path $projectRoot $plan.input))) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $projectRoot '.utmp/EnemyArtCohesion/repair-export.log') -RedirectStandardError (Join-Path $projectRoot '.utmp/EnemyArtCohesion/repair-export-error.log')
 $exportHandle=$exportProcess.Handle;$exportProcess.WaitForExit();$exportProcess.Refresh()
 if($exportProcess.ExitCode -ne 0){throw "Repair input export failed: $($plan.id)"}
}
Write-Output 'EXPORTED_REPAIR_INPUTS'
