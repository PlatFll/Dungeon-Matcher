$ErrorActionPreference='Stop'
$root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2'
$args=@('--script',('"'+$root+'/Scripts/Stage05/AuthorBoard05.js"'))
$job=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList $args -WindowStyle Hidden -PassThru -RedirectStandardOutput ($root+'/QA/Stage05/LibreSprite.txt') -RedirectStandardError ($root+'/QA/Stage05/LibreSprite-errors.txt')
$job.WaitForExit();$job.Refresh();Get-Content -LiteralPath ($root+'/QA/Stage05/LibreSprite.txt');exit $job.ExitCode
