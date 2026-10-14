[CmdletBinding()]
param([string[]]$Only, [switch]$ExportOnly)
$ErrorActionPreference='Stop'
$bpProject=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$bpRoot=Join-Path $bpProject 'ArtSource/BattlePresentation'
$bpTemp=Join-Path $bpProject '.utmp/ThreeZonePresentation/build'
$bpLibre=Join-Path $env:USERPROFILE 'Downloads/libresprite-development-windows-x86_64/libresprite.exe'
New-Item -ItemType Directory -Force -Path $bpTemp | Out-Null
$bpNames=@()
foreach($family in @('Guide','Supply','Ability')) {foreach($state in @('','Hover','Pressed','Disabled')){$bpNames+="Shared/$family$state"}}
$bpNames+=@('Shared/Energy','Shared/EnergyFill','Shared/Corner','Shared/Edge','Court/Statue')
foreach($zone in @('Dungeon','Forest','Court')){foreach($part in @('Edge','Corner','Plaque','Panel','General','BottomPanel','CellA','CellB','CellC','PlayerTop','PlayerMiddle','PlayerBottom','Banner','SideDressing')){$bpNames+="$zone/$part"};$bpNames+="Scene/$zone"}
$bpNames+=@('Forest/Foliage','Court/Kelp','Motion/Torch','Motion/Lantern','Motion/Rune','Motion/Bubble','Motion/Waterfall')
if($Only){$bpNames=$Only}
$bpScript=Get-Content -LiteralPath (Join-Path $bpRoot 'Build.js') -Raw
$bpRevision=$bpScript.Substring(0,$bpScript.IndexOf('var d=read'))+(Get-Content -LiteralPath (Join-Path $bpRoot 'ReferenceRevision.js') -Raw)
foreach($name in $bpNames){
 $native=Join-Path $bpRoot "native/$name.aseprite"
 New-Item -ItemType Directory -Force -Path (Split-Path $native) | Out-Null
 $tag=$name.Replace('/','-');$log=Join-Path $bpTemp "$tag.log"
 if(-not $ExportOnly){
  $selectedScript=if($name -match '^(Scene/|Motion/(?:Rune|Torch|Waterfall)$|(?:Dungeon|Forest|Court)/(?:Edge|Corner|Plaque|Panel|General|BottomPanel|Cell[A-C]|PlayerTop|PlayerMiddle|PlayerBottom|Banner|SideDressing)$)'){$bpRevision}else{$bpScript}
  $script=("try {`n"+$selectedScript+"`n} catch(e) { console.log('BATTLE_ASSET_FAILED '+e); } app.command.Exit();").Replace('__ROOT__',$bpRoot.Replace('\','/')+'/').Replace('__PROJECT__',$bpProject.Replace('\','/')+'/').Replace('__ONLY__',$name)
  $scriptFile=Join-Path $bpTemp "$tag.js";[IO.File]::WriteAllText($scriptFile,$script)
  $job=Start-Process -FilePath $bpLibre -ArgumentList @('--script',('"{0}"' -f $scriptFile)) -WindowStyle Hidden -PassThru -RedirectStandardOutput $log -RedirectStandardError (Join-Path $bpTemp "$tag-errors.log")
  $handle=$job.Handle;$job.WaitForExit();$job.Refresh()
  if($job.ExitCode -ne 0 -or -not (Select-String -LiteralPath $log -Pattern 'BATTLE_ASSET_OK' -Quiet)){throw "LibreSprite failed $name; see $log"}
 }
 & (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1') -SourceRoot (Split-Path $native)
 $job=Start-Process -FilePath $bpLibre -ArgumentList @('--batch',('"{0}"' -f $native),'--save-as',('"{0}"' -f ([IO.Path]::ChangeExtension($native,'.png')))) -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput (Join-Path $bpTemp 'export.log') -RedirectStandardError (Join-Path $bpTemp 'export-errors.log')
 if($job.ExitCode -ne 0){throw "Export failed $name"}
 Write-Output "Verified/exported $name"
}
