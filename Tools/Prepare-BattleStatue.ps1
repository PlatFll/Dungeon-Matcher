$ErrorActionPreference='Stop'
$bpRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$bpOutput=Join-Path $bpRoot 'ArtSource/BattlePresentation/generation/native'
New-Item -ItemType Directory -Force -Path $bpOutput | Out-Null
$bpSource=Get-Content (Join-Path $bpRoot 'ArtSource/BattlePresentation/Build.js') -Raw
$bpHelpers=$bpSource.Substring(0,$bpSource.IndexOf("if(ONLY.indexOf('Shared/')"))
$bpArt=@'
var input=blank(128,160);rect(input,0,0,128,160,'164d5d');
poly(input,[[52,56],[86,58],[96,129],[35,129]],'1c6474');
poly(input,[[63,62],[75,61],[76,128],[47,128]],'2c7e8a');
rect(input,28,129,78,9,'103947');rect(input,23,138,88,11,'1c6474');
copy(input,extract(c,491,133,167,176,84,88),24,4);
save(input,'CourtStatueInput');console.log('STATUE_INPUT_OK');app.command.Exit();
'@
$bpScript=($bpHelpers+$bpArt).Replace('__ROOT__',($bpRoot.Replace('\','/')+'/ArtSource/BattlePresentation/')).Replace('__PROJECT__',($bpRoot.Replace('\','/')+'/')).Replace('__ONLY__','CourtStatueInput')
$bpScript=$bpScript.Replace("ROOT+'native/'","ROOT+'generation/native/'")
$bpFile=Join-Path $bpRoot '.utmp/ThreeZonePresentation/build/StatueInput.js'
[IO.File]::WriteAllText($bpFile,$bpScript)
$bpLibre=Join-Path $env:USERPROFILE 'Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$bpJob=Start-Process $bpLibre -ArgumentList @('--script',('"{0}"' -f $bpFile)) -WindowStyle Hidden -PassThru -Wait
if($bpJob.ExitCode -ne 0){throw 'Input preparation failed'}
& (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1') -SourceRoot $bpOutput
$bpAse=Join-Path $bpOutput 'CourtStatueInput.aseprite'
Start-Process $bpLibre -ArgumentList @('--batch',('"{0}"' -f $bpAse),'--save-as',('"{0}"' -f ([IO.Path]::ChangeExtension($bpAse,'.png')))) -WindowStyle Hidden -Wait
