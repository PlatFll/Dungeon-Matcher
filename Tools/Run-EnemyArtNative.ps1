param([string]$LibreSprite='C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe')
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$artRoot=Join-Path $projectRoot '.utmp/EnemyArtCohesion'
$scriptPath=Join-Path $artRoot 'RunNative.js'
$artProcess=Start-Process -FilePath $LibreSprite -ArgumentList @('--script',('"{0}"' -f $scriptPath)) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $artRoot 'native.log') -RedirectStandardError (Join-Path $artRoot 'native-error.log')
$artHandle=$artProcess.Handle
$artProcess.WaitForExit();$artProcess.Refresh()
if($artProcess.ExitCode -ne 0){throw "LibreSprite failed: $($artProcess.ExitCode)"}
$log=Get-Content -LiteralPath (Join-Path $artRoot 'native.log') -Raw
if($log -match 'NATIVE_ERROR' -or $log -notmatch 'NATIVE_SUCCESS'){throw "Native edits did not complete. See $artRoot/native.log"}
$log -split "`n" | Where-Object {$_ -match 'NATIVE_(SUCCESS|ERROR)'}
