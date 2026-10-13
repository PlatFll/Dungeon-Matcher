[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$reviewRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../ArtSource/MenuUI'))
$reviewTemp=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../.utmp/MenuFrames'))
$reviewLibre=Join-Path $env:USERPROFILE 'Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$reviewProject=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$reviewScript=(Get-Content -LiteralPath (Join-Path $reviewRoot 'ReviewFrames.js') -Raw).Replace('__ART_ROOT__',$reviewRoot.Replace('\','/')+'/').Replace('__PROJECT_ROOT__',$reviewProject.Replace('\','/')+'/')
$reviewFile=Join-Path $reviewTemp 'ReviewFrames.js'
[IO.File]::WriteAllText($reviewFile,$reviewScript)
$reviewJob=Start-Process -FilePath $reviewLibre -ArgumentList @('--script',('"{0}"' -f $reviewFile)) -WindowStyle Hidden -PassThru
$reviewJob.WaitForExit();$reviewJob.Refresh();if($reviewJob.ExitCode -ne 0){throw "LibreSprite review exit $($reviewJob.ExitCode)"}
Write-Output 'LibreSprite exported two screen comparisons and a native asset sheet.'
