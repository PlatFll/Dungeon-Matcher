[CmdletBinding()]
param([string]$LibreSpritePath = 'libresprite')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$script=Join-Path $PSScriptRoot 'SmallHold-WhiteHold.js'
New-Item -ItemType Directory -Path (Join-Path $root '.utmp') -Force | Out-Null
$log=Join-Path $root '.utmp/SmallHold-WhiteHold.log'
$arguments=@('-b','--script',('"{0}"' -f $script))
$process=Start-Process -FilePath $LibreSpritePath -ArgumentList $arguments -WorkingDirectory $root -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput $log
Get-Content -LiteralPath $log
if (Select-String -LiteralPath $log -Pattern '^Error:' -Quiet) { throw 'LibreSprite script reported an error' }
if($process.ExitCode -ne 0){throw "LibreSprite white hold preparation exited $($process.ExitCode)"}
