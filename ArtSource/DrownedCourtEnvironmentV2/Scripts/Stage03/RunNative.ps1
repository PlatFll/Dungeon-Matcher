$ErrorActionPreference='Stop'
$root=[System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$args=@('--script',('"'+$root+'\Scripts\Stage03\AuthorPlatform03.js"'))
$job=Start-Process -FilePath 'C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe' -ArgumentList $args -WindowStyle Hidden -PassThru -RedirectStandardOutput ($root+'\QA\Stage03\LibreSprite.txt') -RedirectStandardError ($root+'\QA\Stage03\LibreSprite-errors.txt')
$job.WaitForExit();$job.Refresh()
Get-Content -LiteralPath ($root+'\QA\Stage03\LibreSprite.txt')
Get-Content -LiteralPath ($root+'\QA\Stage03\LibreSprite-errors.txt')
exit $job.ExitCode
