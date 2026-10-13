[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$frameProject=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$frameRoot=Join-Path $frameProject 'ArtSource/MenuUI'
$frameLibre=Join-Path $env:USERPROFILE 'Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$frameTemp=Join-Path $frameProject '.utmp/MenuFrames'
New-Item -ItemType Directory -Force -Path $frameTemp | Out-Null
$frameNames=@('Play','Secondary','Challenge','Affirmative','Neutral','Destructive') | ForEach-Object { $family=$_; '', 'Hover','Pressed','Disabled' | ForEach-Object { "$family$_" } }
$frameNames+=@('IconPlate','SettingsRow','ToggleOn','ToggleOff','Panel','HeaderRule','CornerJewel','SettingsCrest','Divider')
$frameScript=(Get-Content -LiteralPath (Join-Path $frameRoot 'BuildFrames.js') -Raw).Replace('__ART_ROOT__',$frameRoot.Replace('\','/')+'/').Replace('__PROJECT_ROOT__',$frameProject.Replace('\','/')+'/')
$frameFile=Join-Path $frameTemp 'BuildFrames.js'
[IO.File]::WriteAllText($frameFile,$frameScript)
$frameJob=Start-Process -FilePath $frameLibre -ArgumentList @('--script',('"{0}"' -f $frameFile)) -WindowStyle Hidden -PassThru
$frameJob.WaitForExit();$frameJob.Refresh();if($frameJob.ExitCode -ne 0){throw "LibreSprite build exit $($frameJob.ExitCode)"}
& (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1') -SourceRoot (Join-Path $frameRoot 'native')
$frameScript=''
foreach($frameName in $frameNames){$framePath=(Join-Path $frameRoot "native/$frameName.aseprite").Replace('\','/');$framePng=[IO.Path]::ChangeExtension($framePath,'.png');$frameScript+="app.open('$framePath');app.activeSprite.saveAs('$framePng',false);app.activeDocument.close();`n"}
$frameScript+='app.command.Exit();'
[IO.File]::WriteAllText($frameFile,$frameScript)
$frameJob=Start-Process -FilePath $frameLibre -ArgumentList @('--script',('"{0}"' -f $frameFile)) -WindowStyle Hidden -PassThru
$frameJob.WaitForExit();$frameJob.Refresh();if($frameJob.ExitCode -ne 0){throw "LibreSprite export exit $($frameJob.ExitCode)"}
Write-Output "LibreSprite rebuilt/exported $($frameNames.Count) button and frame assets. Background, logo and character assets were not rebuilt."
