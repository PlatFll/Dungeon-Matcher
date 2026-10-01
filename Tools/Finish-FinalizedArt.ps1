[CmdletBinding()]
param([string]$Script = 'FinishTiles.js', [string]$LibreSprite = $env:LIBRESPRITE_EXE)
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$artRoot = Join-Path $projectRoot 'ArtSource/FinalizedVisuals'
for ($tileIndex = 0; $tileIndex -lt 16; $tileIndex++) {
    Copy-Item -LiteralPath (Join-Path $artRoot "Candidates/Tiles_01/tile_$tileIndex.png") -Destination (Join-Path $artRoot ("Candidates/Tiles_01/Tile{0}.png" -f [char](65 + $tileIndex)))
}
New-Item -ItemType Directory -Force -Path (Join-Path $artRoot 'Environment/Tiles') | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $artRoot 'Environment/Props'),(Join-Path $artRoot 'Environment/Baked') | Out-Null
$scriptPath = Join-Path $artRoot $Script
$buildRoot = Join-Path $projectRoot '.utmp/FinalizedArtBuild'
New-Item -ItemType Directory -Force -Path $buildRoot | Out-Null
$scriptText = Get-Content -LiteralPath $scriptPath -Raw
$scriptText = $scriptText.Replace('__ART_ROOT__', ($artRoot.Replace('\','/') + '/'))
$scriptPath = Join-Path $buildRoot $Script
[IO.File]::WriteAllText($scriptPath, $scriptText)
$arguments = @('--script', ('"{0}"' -f $scriptPath))
if (!$LibreSprite) { $LibreSprite = (Get-Command libresprite -ErrorAction Stop).Source }
$artProcess = Start-Process -FilePath $LibreSprite -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru
$artProcess.WaitForExit(); $artProcess.Refresh()
if ($artProcess.ExitCode -ne 0) { exit $artProcess.ExitCode }
& (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1')
exit $artProcess.ExitCode
