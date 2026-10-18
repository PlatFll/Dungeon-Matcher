param([ValidateSet('Floor','Kit','All')][string]$Phase='All',[string]$Only='')
$ErrorActionPreference='Stop'
$kit=$PSScriptRoot
$project=[IO.Path]::GetFullPath((Join-Path $kit '../../..')).Replace('\','/')
$libre='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$groups=if($Phase -eq 'Floor'){@('FloorTiles')}elseif($Phase -eq 'Kit'){@('WallTiles','BoardFrames','CharacterFrames','HudFrames','Props','BackgroundLayers')}else{@('FloorTiles','WallTiles','BoardFrames','CharacterFrames','HudFrames','Props','BackgroundLayers')}
foreach($g in $groups){foreach($d in @('Editable','Exports')){New-Item -ItemType Directory -Force -Path (Join-Path $kit "$d/$g") | Out-Null}}
New-Item -ItemType Directory -Force -Path (Join-Path $kit 'logs') | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $kit 'Sources') | Out-Null
for($cell=0;$cell -lt 3;$cell++) { Copy-Item -LiteralPath (Join-Path $project "ArtSource/BattlePresentation/DungeonMockupRevision/native/EmptyCell$cell.aseprite") -Destination (Join-Path $kit ("Sources/EmptyCell"+[char](65+$cell)+'.aseprite')) }
foreach($stage in $(if($Phase -eq 'All'){@('Floor','Kit')}else{@($Phase)})){
 $text=if($stage -eq 'Floor'){[IO.File]::ReadAllText((Join-Path $kit 'FloorBuild.js'))}else{[IO.File]::ReadAllText((Join-Path $kit 'Native.js'))+"`n"+[IO.File]::ReadAllText((Join-Path $kit 'KitBuild.js'))}
 if($Only){$prefixes=ConvertTo-Json -InputObject @($Only.Split(',')) -Compress; $text=$text.Replace('function save(a,group,name){',('function save(a,group,name){var prefixes='+$prefixes+';if(!prefixes.some(function(p){return name.indexOf(p)===0;}))return;'))}
 $script=Join-Path $kit "logs/$stage.js"; $log=Join-Path $kit "logs/$stage.log"
 [IO.File]::WriteAllText($script,"try {`n"+$text.Replace('__PROJECT__',$project)+"`n} catch(e) {console.log('BUILD_FAILED '+e);} app.command.Exit();")
 $p=Start-Process -FilePath $libre -ArgumentList @('--script',('"{0}"' -f $script)) -WindowStyle Hidden -PassThru -RedirectStandardOutput $log -RedirectStandardError (Join-Path $kit "logs/$stage-errors.log")
 $handle=$p.Handle
 if(-not $p.WaitForExit(600000)){$p.Kill();throw 'Owned LibreSprite process exceeded its ten-minute batch limit; inspect logs.'}
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
  if($Only -and -not (@($Only.Split(',') | Where-Object { $file.BaseName.StartsWith($_) }).Count)){continue}
  $out=Join-Path $kit ("Exports/$g/"+$file.BaseName+'.png')
  $p=Start-Process -FilePath $libre -ArgumentList @('--batch',('"{0}"' -f $file.FullName),'--save-as',('"{0}"' -f $out)) -WindowStyle Hidden -PassThru -Wait -RedirectStandardOutput (Join-Path $kit 'logs/export.log') -RedirectStandardError (Join-Path $kit 'logs/export-errors.log')
  if($p.ExitCode -ne 0){throw "Native export failed: $out"}
 }
}
Write-Output "LibreSprite $Phase authoring and native exports complete."

# Current visual correction is applied last, using archived source candidates.
# This never starts a generation request or spends credits.
$corrections=if($Phase -eq 'Floor'){@('FloorPolish')}elseif($Phase -eq 'Kit'){@('Wall','Decoration','Health')}else{@('FloorPolish','Wall','Decoration','Health')}
foreach($stage in $corrections){
 $correctionGroups=if($stage -eq 'FloorPolish'){'FloorTiles'}elseif($stage -eq 'Wall'){'WallTiles,Props'}elseif($stage -eq 'Health'){'CharacterFrames'}else{'HudFrames,Props'}
 & (Join-Path $kit 'Correction/RunNative.ps1') -Stage $stage -Groups $correctionGroups -Only $Only
}
& (Join-Path $kit 'Correction/RunNative.ps1') -Stage Lighting -Groups 'WallTiles,FloorTiles,Props' -Only 'BattleWall,Floor,TorchLitMasonry,RightTopPillar'
& (Join-Path $kit 'Correction/RunNative.ps1') -Stage Flagstone -Groups 'FloorTiles' -Only 'Floor'
& 'C:/Users/USER/DungeonMatcherSpriteFoundry/venvs/art/Scripts/python.exe' (Join-Path $kit 'Correction/UpdateManifest.py')
if($LASTEXITCODE -ne 0){throw 'Correction manifest update failed.'}
