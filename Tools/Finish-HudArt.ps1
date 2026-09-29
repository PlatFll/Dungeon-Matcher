[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$LibreSprite)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$art = Join-Path $root 'ArtSource/HudTypography'
$scriptPath = Join-Path $root '.utmp/FinishHud.js'
[IO.File]::WriteAllText($scriptPath, (Get-Content (Join-Path $art 'FinishHud.js') -Raw).Replace('__ART_ROOT__', $art.Replace('\','/')+'/'))
$job = Start-Process -FilePath $LibreSprite -ArgumentList @('--script',('"{0}"' -f $scriptPath)) -WindowStyle Hidden -PassThru
$job.WaitForExit(); $job.Refresh(); if ($job.ExitCode -ne 0) { throw "LibreSprite finish failed: $($job.ExitCode)" }
& (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1') -SourceRoot $art
foreach ($source in Get-ChildItem (Join-Path $art 'UI') -Filter '*.aseprite') {
    $output = [IO.Path]::ChangeExtension($source.FullName,'.png')
    $job = Start-Process -FilePath $LibreSprite -ArgumentList @('-b',('"{0}"' -f $source.FullName),'--save-as',('"{0}"' -f $output)) -WindowStyle Hidden -PassThru
    $job.WaitForExit(); $job.Refresh(); if ($job.ExitCode -ne 0) { throw "LibreSprite export failed: $($source.Name)" }
}
