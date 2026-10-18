param([string]$Only='')
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path.Replace('\','/')
$kit=Join-Path $project 'ArtSource/DungeonEnvironment/SideViewRebuild'
New-Item -ItemType Directory -Force -Path (Join-Path $kit 'logs') | Out-Null
$libre='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$text=[IO.File]::ReadAllText((Join-Path $kit 'BuildNative.js')).Replace('__PROJECT__',$project)
$script=Join-Path $kit 'logs/Build.js';$log=Join-Path $kit 'logs/Build.log'
[IO.File]::WriteAllText($script,"try {`n"+$text+"`n} catch(e){console.log('BUILD_FAILED '+e);} app.command.Exit();")
$owned=Start-Process -FilePath $libre -ArgumentList @('--script',('"{0}"' -f $script)) -WindowStyle Hidden -PassThru -RedirectStandardOutput $log -RedirectStandardError (Join-Path $kit 'logs/Build-errors.log')
$handle=$owned.Handle
if(-not $owned.WaitForExit(240000)){$owned.Kill();throw 'Owned LibreSprite script exceeded four minutes.'}
$owned.WaitForExit();$owned.Refresh()
if($owned.ExitCode -ne 0 -or -not (Select-String -LiteralPath $log -Pattern '^SIDEVIEW_COMPLETE' -Quiet)){throw "Native build failed: $log"}
$manifest=(Select-String -LiteralPath $log -Pattern '^MANIFEST ').Line.Substring(9)
[IO.File]::WriteAllText((Join-Path $kit 'AssetManifest.json'),$manifest)
& (Join-Path $project 'Tools/Normalize-FinalizedSourceLayers.ps1') -SourceRoot (Join-Path $kit 'Editable')
$files=@(Get-ChildItem -LiteralPath (Join-Path $kit 'Editable') -Filter '*.aseprite' -Recurse | Where-Object { -not $Only -or $_.BaseName.StartsWith($Only) })
for($index=0;$index -lt $files.Count;$index+=20){
 $arguments=@('--batch')
 foreach($file in $files[$index..([Math]::Min($index+19,$files.Count-1))]){
  $out=Join-Path $kit ("Exports/"+$file.Directory.Name+'/'+$file.BaseName+'.png')
  $arguments+=@(('"{0}"' -f $file.FullName),'--save-as',('"{0}"' -f $out))
 }
 $exp=Start-Process -FilePath $libre -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait -RedirectStandardOutput (Join-Path $kit 'logs/export.log') -RedirectStandardError (Join-Path $kit 'logs/export-errors.log')
 if($exp.ExitCode -ne 0){throw "Native export batch failed at $index"}
}
Write-Output 'Native LibreSprite side-view library exported.'
