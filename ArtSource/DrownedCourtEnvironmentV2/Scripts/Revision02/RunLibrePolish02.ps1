param([string]$ScriptName='AuthorPolish02')
$revisionRoot='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2'
$nativeJob=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$revisionRoot+'/Scripts/Revision02/'+$ScriptName+'.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($revisionRoot+'/QA/Revision02/'+$ScriptName+'.log') -RedirectStandardError ($revisionRoot+'/QA/Revision02/'+$ScriptName+'-error.log')
$nativeJob.WaitForExit();$nativeJob.Refresh()
Get-Content -LiteralPath ($revisionRoot+'/QA/Revision02/'+$ScriptName+'.log') -Tail 5
Get-Content -LiteralPath ($revisionRoot+'/QA/Revision02/'+$ScriptName+'-error.log') -Tail 5
exit $nativeJob.ExitCode
