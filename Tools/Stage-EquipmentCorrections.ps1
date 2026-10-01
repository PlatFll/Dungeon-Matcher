param([string]$LibreSprite='C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
function Run-Libre([string[]]$arguments){
 $job=Start-Process -FilePath $LibreSprite -ArgumentList $arguments -WindowStyle Hidden -PassThru
 $handle=$job.Handle;$job.WaitForExit();$job.Refresh()
 if($job.ExitCode -ne 0){throw "LibreSprite exit $($job.ExitCode)"}
}
$script=Join-Path $root '.utmp/PrepareEquipmentRGBA.js'
$template=Get-Content (Join-Path $root 'ArtSource/EnemyAttacks/Refinements/EquipmentCleanup/PrepareRGBA.js') -Raw
[IO.File]::WriteAllText($script,$template.Replace('__ROOT__',$root.Replace('\','/')+'/'))
Run-Libre @('--script',('"{0}"' -f $script))
foreach($name in @('SpearKnight','RoyalLancer','ShieldKnight')){
 $rgba=Join-Path $root ".utmp/$($name)EquipmentRGBA.aseprite"
 if([BitConverter]::ToUInt16([IO.File]::ReadAllBytes($rgba),12) -ne 32){throw "Expected RGBA source: $name"}
 $folder=Join-Path $root "ArtSource/EnemyAttacks/Refinements/EquipmentCleanup/$name"
 for($i=0;$i -lt 12;$i++){
  Run-Libre @('-b','--frame-range',("$i,$i"),('"{0}"' -f $rgba),'--sheet',('"{0}/{1:00}.png"' -f $folder,$i),'--data',('"{0}"' -f (Join-Path $root '.utmp/equipment-frame.json')))
 }
}
& (Join-Path $PSScriptRoot 'Finish-EnemyAttacks.ps1') -LibreSprite $LibreSprite -Names @('SpearKnight','RoyalLancer','ShieldKnight')
foreach($name in @('SpearKnight','RoyalLancer','ShieldKnight')){
 Copy-Item -LiteralPath (Join-Path $root "ArtSource/EnemyAttacks/$($name)_AutoAttack.png") -Destination (Join-Path $root "Assets/_Game/Art/CombatActions/$($name)_AutoAttack.png") -Force
}
