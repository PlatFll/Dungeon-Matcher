param([string]$Script = 'AnimateApproved.js')
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$scriptPath = Join-Path $PSScriptRoot $Script
$arguments = @('--script', ('"{0}"' -f $scriptPath))
$gideonProcess = Start-Process -FilePath 'C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe' -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru
$gideonProcess.WaitForExit()
$gideonProcess.Refresh()
exit $gideonProcess.ExitCode
