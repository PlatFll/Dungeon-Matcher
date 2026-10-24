param([string]$ScriptName='RunNative.js')
$ErrorActionPreference='Stop'
$project='C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher'
$root=Join-Path $project 'ArtSource/ForestSurroundRevision/PolishStage01'
$p=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$root+'/Scripts/'+$ScriptName+'"')) -WorkingDirectory $project -WindowStyle Hidden -PassThru -RedirectStandardOutput ($root+'/Diagnostics/'+$ScriptName+'.log') -RedirectStandardError ($root+'/Diagnostics/'+$ScriptName+'.err')
$handle=$p.Handle
if(-not $p.WaitForExit(45000)){throw 'Owned separator native task exceeded bounded wait; inspect before retry.'}
$p.WaitForExit();$p.Refresh()
Get-Content ($root+'/Diagnostics/'+$ScriptName+'.log')
Get-Content ($root+'/Diagnostics/'+$ScriptName+'.err')
if($p.ExitCode -ne 0 -or -not (Select-String -Path ($root+'/Diagnostics/'+$ScriptName+'.log') -Pattern 'FOREST_SEPARATOR_NATIVE_COMPLETE' -Quiet)){throw 'Native export failed'}
