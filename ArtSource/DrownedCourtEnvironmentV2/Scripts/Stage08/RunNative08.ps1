$ErrorActionPreference='Stop'
$root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2'
$arguments=@('--script',('"'+$root+'/Scripts/Stage08/AuthorDetails08.js"'))
$job=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList $arguments -WindowStyle Hidden -PassThru -RedirectStandardOutput ($root+'/QA/Stage08/LibreSprite.txt') -RedirectStandardError ($root+'/QA/Stage08/LibreSprite-errors.txt')
$job.WaitForExit();$job.Refresh();Get-Content -LiteralPath ($root+'/QA/Stage08/LibreSprite.txt') -Tail 7;exit $job.ExitCode
