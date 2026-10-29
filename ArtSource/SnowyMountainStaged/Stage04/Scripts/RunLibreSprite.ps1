$snowRoot='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/SnowyMountainStaged/Stage04'
$snowJob=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$snowRoot+'/Scripts/Cleanup.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($snowRoot+'/QA/LibreSprite.log') -RedirectStandardError ($snowRoot+'/QA/LibreSprite-error.log')
$snowJob.WaitForExit();$snowJob.Refresh()
Get-Content ($snowRoot+'/QA/LibreSprite.log') -Tail 10
Get-Content ($snowRoot+'/QA/LibreSprite-error.log') -Tail 5
exit $snowJob.ExitCode
