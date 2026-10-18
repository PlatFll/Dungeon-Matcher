param([ValidateSet('Floor','Kit','All')][string]$Phase='All')
$ErrorActionPreference='Stop'
$kit=$PSScriptRoot
$project=[IO.Path]::GetFullPath((Join-Path $kit '../../..')).Replace('\','/')
$libre='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$groups=if($Phase -eq 'Floor'){@('FloorTiles')}elseif($Phase -eq 'Kit'){@('WallTiles','BoardFrames','CharacterFrames','HudFrames','Props','BackgroundLayers')}else{@('FloorTiles','WallTiles','BoardFrames','CharacterFrames','HudFrames','Props','BackgroundLayers')}
foreach($g in $groups){foreach($d in @('Editable','Exports')){New-Item -ItemType Directory -Force -Path (Join-Path $kit "$d/$g") | Out-Null}}
New-Item -ItemType Directory -Force -Path (Join-Path $kit 'logs') | Out-Null
foreach($stage in $(if($Phase -eq 'All'){@('Floor','Kit')}else{@($Phase)})){
 $text=if($stage -eq 'Floor'){[IO.File]::ReadAllText((Join-Path $kit 'FloorBuild.js'))}else{[IO.File]::ReadAllText((Join-Path $kit 'Native.js'))+"`n"+[IO.File]::ReadAllText((Join-Path $kit 'KitBuild.js'))}
 $script=Join-Path $kit "logs/$stage.js"; $log=Join-Path $kit "logs/$stage.log"
 [IO.File]::WriteAllText($script,"try {`n"+$text.Replace('__PROJECT__',$project)+"`n} catch(e) {console.log('BUILD_FAILED '+e);} app.command.Exit();")
 $p=Start-Process -FilePath $libre -ArgumentList @('--script',('"{0}"' -f $script)) -WindowStyle Hidden -PassThru -RedirectStandardOutput $log -RedirectStandardError (Join-Path $kit "logs/$stage-errors.log")
 $handle=$p.Handle
 if(-not $p.WaitForExit(60000)){$p.Kill();throw 'Owned LibreSprite process exceeded one minute; inspect logs.'}
 $p.WaitForExit();$p.Refresh()
 if($p.ExitCode -ne 0 -or -not (Select-String -LiteralPath $log -Pattern ('^'+$stage.ToUpper()+'_COMPLETE') -Quiet)){throw "Native $stage build failed; inspect $log"}
 if($stage -eq 'Kit'){
  $manifest=(Select-String -LiteralPath $log -Pattern '^KIT_MANIFEST ').Line.Substring(13)
  [IO.File]::WriteAllText((Join-Path $kit 'AssetManifest.json'),$manifest)
 }
}
foreach($g in $groups){
 & (Join-Path $project 'Tools/Normalize-FinalizedSourceLayers.ps1') -SourceRoot (Join-Path $kit "Editable/$g")
 foreach($file in Get-ChildItem -LiteralPath (Join-Path $kit "Editable/$g") -Filter '*.aseprite'){
  $out=Join-Path $kit ("Exports/$g/"+$file.BaseName+'.png')
  $p=Start-Process -FilePath $libre -ArgumentList @('--batch',('"{0}"' -f $file.FullName),'--save-as',('"{0}"' -f $out)) -WindowStyle Hidden -PassThru -Wait -RedirectStandardOutput (Join-Path $kit 'logs/export.log') -RedirectStandardError (Join-Path $kit 'logs/export-errors.log')
  if($p.ExitCode -ne 0){throw "Native export failed: $out"}
 }
}
Write-Output "LibreSprite $Phase authoring and native exports complete."
