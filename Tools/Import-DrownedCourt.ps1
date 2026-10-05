[CmdletBinding()]
param([string]$Method='DrownedCourtImporter.ImportKits')
$ErrorActionPreference='Stop'
$courtRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$courtVersion=[regex]::Match((Get-Content (Join-Path $courtRoot 'ProjectSettings/ProjectVersion.txt') -Raw),'(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$courtUnity=Join-Path $env:ProgramFiles "Unity/Hub/Editor/$courtVersion/Editor/Unity.exe"
$courtOutput=Join-Path $courtRoot '.utmp/DrownedCourtValidation'
New-Item -ItemType Directory -Force -Path $courtOutput | Out-Null
$courtLog=Join-Path $courtOutput ('import-'+[Guid]::NewGuid()+'.log')
$courtArgs=@('-batchmode','-nographics','-quit','-projectPath',('"{0}"' -f $courtRoot),'-executeMethod',$Method,'-logFile',('"{0}"' -f $courtLog))
$courtJob=Start-Process -FilePath $courtUnity -ArgumentList $courtArgs -WindowStyle Hidden -PassThru
$courtJob.WaitForExit();$courtJob.Refresh()
Write-Output "Import exit: $($courtJob.ExitCode); log: $courtLog"
if($courtJob.ExitCode -ne 0){Select-String -LiteralPath $courtLog -Pattern 'error CS','Exception','Aborting';exit 1}
