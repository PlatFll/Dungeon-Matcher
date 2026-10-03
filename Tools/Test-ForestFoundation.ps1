[CmdletBinding()]
param([string]$Filter = 'AcceptedMoveStateTests;ForestFoundationTests;ForestFoundationPlayTests', [switch]$Graphics)
$ErrorActionPreference = 'Stop'
$forestRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$forestVersion = [regex]::Match((Get-Content (Join-Path $forestRoot 'ProjectSettings/ProjectVersion.txt') -Raw), '(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$forestUnity = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$forestVersion/Editor/Unity.exe"
$forestOutput = Join-Path $forestRoot '.utmp/ForestValidation'
New-Item -ItemType Directory -Force -Path $forestOutput | Out-Null
$forestStamp = [Guid]::NewGuid().ToString()
$forestXml = Join-Path $forestOutput "$forestStamp.xml"
$forestLog = Join-Path $forestOutput "$forestStamp.log"
$forestArgs = @('-batchmode','-nographics','-projectPath',('"{0}"' -f $forestRoot),'-runTests','-testPlatform','EditMode','-testFilter',$Filter,'-testResults',('"{0}"' -f $forestXml),'-logFile',('"{0}"' -f $forestLog))
if($Graphics) { $forestArgs=$forestArgs | Where-Object { $_ -ne '-nographics' } }
$forestJob = Start-Process -FilePath $forestUnity -ArgumentList $forestArgs -WindowStyle Hidden -PassThru
$forestJob.WaitForExit()
$forestJob.Refresh()
if (-not (Test-Path -LiteralPath $forestXml)) { throw "No test results. Unity exit $($forestJob.ExitCode); log: $forestLog" }
[xml]$forestResult = Get-Content -LiteralPath $forestXml
$forestResult.'test-run' | Select-Object result,total,passed,failed,skipped
$forestResult.SelectNodes('//test-case[@result="Failed"]') | ForEach-Object { Write-Output $_.fullname; Write-Output $_.failure.message.InnerText }
Write-Output "Results: $forestXml"
Write-Output "Log: $forestLog"
if ($forestJob.ExitCode -ne 0 -or [int]$forestResult.'test-run'.total -eq 0 -or [int]$forestResult.'test-run'.failed -gt 0 -or [int]$forestResult.'test-run'.skipped -gt 0) { exit 1 }
