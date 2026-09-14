[CmdletBinding()]
param(
    [string]$LibreSpritePath = 'libresprite'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$source = Join-Path $projectRoot 'ArtSource\SmallHoldGames\SmallHold_Games_Ident.aseprite'
$atlas = Join-Path $projectRoot 'Assets\_Game\Art\Branding\SmallHold_Games_Atlas.png'

# Fixed-size rows retain all 30 full 480x270 frames, including the final hold.
# Do not trim, scale, pack, or split layers: the runtime uses these exact cells.
$logDirectory = Join-Path $projectRoot '.utmp'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$exportLog = Join-Path $logDirectory 'SmallHold-Export.log'
$executable = (Get-Command $LibreSpritePath -ErrorAction Stop).Source
$exportArguments = @('-b', ('"{0}"' -f $source), '--sheet-type', 'rows',
    '--sheet-width', '2400', '--sheet-height', '1620', '--sheet', ('"{0}"' -f $atlas))
$process = Start-Process -FilePath $executable -ArgumentList $exportArguments `
    -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput $exportLog
if ($process.ExitCode -ne 0) { throw "LibreSprite export failed with exit code $($process.ExitCode). See $exportLog" }
if (-not (Test-Path -LiteralPath $atlas)) { throw 'LibreSprite did not create the ident atlas.' }
Write-Host "Exported SmallHold ident atlas: $atlas"
