param([string]$LibreSprite='C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$art=Join-Path $root 'ArtSource/RankVisibility'
$script=Join-Path $root '.utmp/RecolorBadges.js'
[IO.File]::WriteAllText($script,(Get-Content (Join-Path $art 'RecolorBadges.js') -Raw).Replace('__ROOT__',$root.Replace('\','/')+'/'))
$job=Start-Process -FilePath $LibreSprite -ArgumentList @('--script',('"{0}"' -f $script)) -WindowStyle Hidden -PassThru
$job.WaitForExit();$job.Refresh();if($job.ExitCode -ne 0){throw 'Badge recolor failed'}
& (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1') -SourceRoot $art
foreach($rank in @('Normal','Special','Miniboss')){
 $source=Join-Path $art ($rank+'Badge.aseprite');$png=[IO.Path]::ChangeExtension($source,'.png')
 $job=Start-Process -FilePath $LibreSprite -ArgumentList @('-b',('"{0}"' -f $source),'--save-as',('"{0}"' -f $png)) -WindowStyle Hidden -PassThru
 $job.WaitForExit();$job.Refresh();if($job.ExitCode -ne 0){throw 'Badge export failed'}
 Copy-Item -LiteralPath $png -Destination (Join-Path $root ('Assets/_Game/Resources/UI/Finalized/'+$rank+'Badge.png')) -Force
 Copy-Item -LiteralPath $png -Destination (Join-Path $root ('ArtSource/FinalizedVisuals/UI/'+$rank+'Badge.png')) -Force
 Copy-Item -LiteralPath $source -Destination (Join-Path $root ('ArtSource/FinalizedVisuals/UI/'+$rank+'Badge.aseprite')) -Force
}
Write-Output 'Recolored three rank badges; boss art preserved.'
