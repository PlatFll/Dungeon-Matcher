param([string]$LibreSpritePath = 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$source=Join-Path $root 'ArtSource/SmallHoldGames/SmallHold_Games_Ident.aseprite'
$script=Join-Path $PSScriptRoot 'SmallHold-WhiteHold.js'
$log=Join-Path $root '.utmp/SmallHold-WhiteHold.log'
$arguments=@('-b','--script',('"{0}"' -f $script))
$process=Start-Process -FilePath $LibreSpritePath -ArgumentList $arguments -WorkingDirectory $root -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput $log
Get-Content -LiteralPath $log
if (Select-String -LiteralPath $log -Pattern '^Error:' -Quiet) { throw 'LibreSprite script reported an error' }
if($process.ExitCode -ne 0){throw "LibreSprite white hold preparation exited $($process.ExitCode)"}
