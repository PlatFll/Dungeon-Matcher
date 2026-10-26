param([string]$ScriptName='RecolorBackground')
$revisionRoot='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2'
$nativeJob=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$revisionRoot+'/Scripts/Revision01/'+$ScriptName+'.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($revisionRoot+'/QA/Revision01/'+$ScriptName+'.log') -RedirectStandardError ($revisionRoot+'/QA/Revision01/'+$ScriptName+'-error.log')
$nativeJob.WaitForExit();$nativeJob.Refresh()
Get-Content -LiteralPath ($revisionRoot+'/QA/Revision01/'+$ScriptName+'.log') -Tail 5
Get-Content -LiteralPath ($revisionRoot+'/QA/Revision01/'+$ScriptName+'-error.log') -Tail 5
exit $nativeJob.ExitCode
