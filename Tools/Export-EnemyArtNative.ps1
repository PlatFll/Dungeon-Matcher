param([string]$LibreSprite='C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe',[switch]$Frames)
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$sourceRoot=Join-Path $projectRoot 'ArtSource/EnemyArtCohesion'
$jobs=Get-Content (Join-Path $projectRoot '.utmp/EnemyArtCohesion/selected-jobs.json') -Raw | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1') -SourceRoot (Join-Path $sourceRoot 'Native')
foreach($job in $jobs){
 $source=Join-Path $sourceRoot ('Native/'+$job.stem+'.aseprite')
 $bytes=[IO.File]::ReadAllBytes($source);$offset=128
 if([BitConverter]::ToUInt16($bytes,6) -ne $job.sprites.Count){throw "Frame count differs: $($job.stem)"}
 foreach($frame in $job.sprites){
  [BitConverter]::GetBytes([UInt16]$frame.duration).CopyTo($bytes,$offset+8)
  $offset += [BitConverter]::ToUInt32($bytes,$offset)
 }
 [IO.File]::WriteAllBytes($source,$bytes)
 $sheet=Join-Path $sourceRoot ('Sheets/'+$job.stem+'.png')
 $exportProcess=Start-Process -FilePath $LibreSprite -ArgumentList @('-b',('"{0}"' -f $source),'--sheet-type','horizontal','--sheet',('"{0}"' -f $sheet)) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $projectRoot '.utmp/EnemyArtCohesion/export.log') -RedirectStandardError (Join-Path $projectRoot '.utmp/EnemyArtCohesion/export-error.log')
 $exportHandle=$exportProcess.Handle;$exportProcess.WaitForExit();$exportProcess.Refresh()
 if($exportProcess.ExitCode -ne 0){throw "Export failed: $($job.stem) / $($exportProcess.ExitCode)"}
 if($Frames){
  $frameDir=Join-Path $projectRoot ('.utmp/EnemyArtCohesion/Frames/'+$job.stem)
  New-Item -ItemType Directory -Path $frameDir -Force | Out-Null
  $exportProcess=Start-Process -FilePath $LibreSprite -ArgumentList @('-b',('"{0}"' -f $source),'--save-as',('"{0}"' -f (Join-Path $frameDir 'frame-{frame}.png'))) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $projectRoot '.utmp/EnemyArtCohesion/export.log') -RedirectStandardError (Join-Path $projectRoot '.utmp/EnemyArtCohesion/export-error.log')
  $exportHandle=$exportProcess.Handle;$exportProcess.WaitForExit();$exportProcess.Refresh()
  if($exportProcess.ExitCode -ne 0){throw "Frame export failed: $($job.stem)"}
 }
}
Write-Output "EXPORTED_SHEETS $($jobs.Count)"
