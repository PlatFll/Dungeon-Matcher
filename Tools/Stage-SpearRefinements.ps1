param([string]$LibreSprite='C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
function Run-Libre([string[]]$arguments){
 $p=Start-Process -FilePath $LibreSprite -ArgumentList $arguments -WindowStyle Hidden -PassThru
 $handle=$p.Handle;$p.WaitForExit();$p.Refresh()
 if($p.ExitCode -ne 0){throw "LibreSprite exit $($p.ExitCode)"}
}
foreach($name in @('SpearGuard','SpearKnight','RoyalLancer')){
 Run-Libre @('-b',('"{0}"' -f (Join-Path $root "ArtSource/EnemyAttacks/Refinements/$($name)Thrust/00.png")),'--save-as',('"{0}"' -f (Join-Path $root ".utmp/$($name)Candidate.aseprite")))
}
$template=Get-Content (Join-Path $root 'ArtSource/EnemyAttacks/Refinements/StageThrusts.js') -Raw
$script=Join-Path $root '.utmp/StageThrusts.js'
[IO.File]::WriteAllText($script,$template.Replace('__ROOT__',$root.Replace('\','/')+'/'))
Run-Libre @('--script',('"{0}"' -f $script))
& (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1') -SourceRoot (Join-Path $root 'ArtSource/EnemyAttacks/Refinements')
foreach($name in @('SpearGuard','SpearKnight','RoyalLancer')){
 $stem=Join-Path $root "ArtSource/EnemyAttacks/Refinements/$($name)Prepared"
 New-Item -ItemType Directory -Path $stem -Force|Out-Null
 $resolved=[IO.Path]::GetFullPath($stem)
 if(!$resolved.StartsWith((Join-Path $root 'ArtSource/EnemyAttacks/Refinements/'),[StringComparison]::OrdinalIgnoreCase)){throw 'Unexpected staging directory'}
 Get-ChildItem -LiteralPath $resolved -Filter '*.png' -File|ForEach-Object {Remove-Item -LiteralPath $_.FullName}
 for($i=0;$i -lt 12;$i++){
  Run-Libre @('-b','--frame-range',("$i,$i"),('"{0}"' -f ($stem+'.aseprite')),'--sheet',('"{0}/{1:00}.png"' -f $stem,$i),'--data',('"{0}"' -f (Join-Path $root '.utmp/thrust-frame-export.json')))
 }
}
Write-Output 'Finished native two-handed thrust poses with planted boots.'
