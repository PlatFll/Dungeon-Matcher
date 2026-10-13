[CmdletBinding()]
param([switch]$ExportOnly)
$ErrorActionPreference='Stop'
$frameProject=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$frameRoot=Join-Path $frameProject 'ArtSource/MenuUI'
$frameLibre=Join-Path $env:USERPROFILE 'Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$frameTemp=Join-Path $frameProject '.utmp/MenuFrames'
New-Item -ItemType Directory -Force -Path $frameTemp | Out-Null
$frameNames=@('Play','Secondary','Challenge','Affirmative','Neutral','Destructive') | ForEach-Object { $family=$_; '', 'Hover','Pressed','Disabled' | ForEach-Object { "$family$_" } }
$frameNames+=@('IconPlate','SettingsRow','ToggleOn','ToggleOff','Panel','HeaderRule','CornerJewel','SettingsCrest','Divider')
$frameNames+=@('CrownedSkull','Characters','Shop','Mastery','Practice','Challenges','Music','Sfx','Motion','Vibration','Resume','Suspend','Retry','Skull','Gear','GearHover','GearPressed','GearDisabled')
$frameScript=("try {`n"+(Get-Content -LiteralPath (Join-Path $frameRoot 'BuildFrames.js') -Raw)+"`n"+(Get-Content -LiteralPath (Join-Path $frameRoot 'IconDetails.js') -Raw)+"`nconsole.log('MENU_ART_BUILD_OK'); } catch(error) { console.log('MENU_ART_BUILD_FAILED '+error); } app.command.Exit();").Replace('__ART_ROOT__',$frameRoot.Replace('\','/')+'/').Replace('__PROJECT_ROOT__',$frameProject.Replace('\','/')+'/')
# A fresh editor per asset prevents this development build from retaining a
# stale cel across repeated document close/open cycles. Verify before exporting.
if(-not $ExportOnly){foreach($frameName in $frameNames){
    $frameFile=Join-Path $frameTemp "BuildFrames-$frameName.js"
    [IO.File]::WriteAllText($frameFile,"var ONLY='$frameName';`n"+$frameScript)
    $frameLog=Join-Path $frameTemp "$frameName.log"
    $frameJob=Start-Process -FilePath $frameLibre -ArgumentList @('--script',('"{0}"' -f $frameFile)) -WindowStyle Hidden -RedirectStandardOutput $frameLog -RedirectStandardError (Join-Path $frameTemp "$frameName-errors.log") -PassThru
    $frameHandle=$frameJob.Handle
    $frameJob.WaitForExit();$frameJob.Refresh()
    if($frameJob.ExitCode -ne 0){throw "LibreSprite $frameName exit $($frameJob.ExitCode); log: $frameLog"}
    if(-not (Select-String -LiteralPath $frameLog -Pattern 'MENU_ART_BUILD_OK' -Quiet)){throw "LibreSprite source verification failed; see $frameLog"}
    Write-Output "Verified $frameName"
}}
& (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1') -SourceRoot (Join-Path $frameRoot 'native')
# Native batch conversion also isolates document state; no scripted save cache.
foreach($frameName in $frameNames){
    $framePath=Join-Path $frameRoot "native/$frameName.aseprite"
    $framePng=[IO.Path]::ChangeExtension($framePath,'.png')
    $frameJob=Start-Process -FilePath $frameLibre -ArgumentList @('--batch',('"{0}"' -f $framePath),'--save-as',('"{0}"' -f $framePng)) -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput (Join-Path $frameTemp 'export.log') -RedirectStandardError (Join-Path $frameTemp 'export-errors.log')
    if($frameJob.ExitCode -ne 0){throw "LibreSprite export $frameName exit $($frameJob.ExitCode)"}
}
Write-Output "LibreSprite rebuilt/exported $($frameNames.Count) icon, button and frame assets. Background, logo and character assets were not rebuilt."
