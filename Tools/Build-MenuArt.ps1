[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$menuProject=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$menuRoot=Join-Path $menuProject 'ArtSource/MenuUI'
$menuLibre=Join-Path $env:USERPROFILE 'Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$menuTemp=Join-Path $menuProject '.utmp/MenuUI'
New-Item -ItemType Directory -Force -Path $menuTemp,(Join-Path $menuRoot 'native') | Out-Null
$menuScript=(Get-Content -LiteralPath (Join-Path $menuRoot 'BuildNative.js') -Raw).Replace('__ART_ROOT__',$menuRoot.Replace('\','/')+'/').Replace('__PROJECT_ROOT__',$menuProject.Replace('\','/')+'/')
$menuFile=Join-Path $menuTemp 'BuildNative.js'
[IO.File]::WriteAllText($menuFile,$menuScript)
$menuJob=Start-Process -FilePath $menuLibre -ArgumentList @('--script',('"{0}"' -f $menuFile)) -WindowStyle Hidden -PassThru
$menuJob.WaitForExit();$menuJob.Refresh();if($menuJob.ExitCode -ne 0){throw "LibreSprite build exit $($menuJob.ExitCode)"}
& (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1') -SourceRoot (Join-Path $menuRoot 'native')
$menuExports=Get-ChildItem -LiteralPath (Join-Path $menuRoot 'native') -Filter '*.aseprite'
$menuScript=''
foreach($menuAsset in $menuExports){$menuPath=$menuAsset.FullName.Replace('\','/');$menuPng=[IO.Path]::ChangeExtension($menuPath,'.png');$menuScript+="app.open('$menuPath');app.activeSprite.saveAs('$menuPng',false);app.activeDocument.close();`n"}
$menuScript+='app.command.Exit();'
[IO.File]::WriteAllText($menuFile,$menuScript)
$menuJob=Start-Process -FilePath $menuLibre -ArgumentList @('--script',('"{0}"' -f $menuFile)) -WindowStyle Hidden -PassThru
$menuJob.WaitForExit();$menuJob.Refresh();if($menuJob.ExitCode -ne 0){throw "LibreSprite export exit $($menuJob.ExitCode)"}
Write-Output "LibreSprite authored and exported $($menuExports.Count) native assets."
# The user-approved extraction workflow supersedes the original simple frames.
& (Join-Path $PSScriptRoot 'Build-MenuFrames.ps1')
# Preserve the later, authorized local rendering cleanup after the original
# hall constructor. Its before sources and exact LibreSprite edit recipe live
# alongside these finalized sources; never regenerate the approved composition.
foreach($menuHall in @('Hall','HallTall')){
    foreach($menuExtension in @('.aseprite','.png')){
        Copy-Item -LiteralPath (Join-Path $menuRoot ('RenderingCleanupV2/native/'+$menuHall+$menuExtension)) -Destination (Join-Path $menuRoot ('native/'+$menuHall+$menuExtension)) -Force
    }
}
