[CmdletBinding()]
param([switch]$Animations,[switch]$Blockers)
$ErrorActionPreference='Stop'
$rosterRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$rosterVersion=[regex]::Match((Get-Content (Join-Path $rosterRoot 'ProjectSettings/ProjectVersion.txt') -Raw),'(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$rosterUnity=Join-Path $env:ProgramFiles "Unity/Hub/Editor/$rosterVersion/Editor/Unity.exe"
$rosterOutput=Join-Path $rosterRoot '.utmp/ForestValidation'
New-Item -ItemType Directory -Force -Path $rosterOutput | Out-Null
$rosterLog=Join-Path $rosterOutput ('roster-import-'+[Guid]::NewGuid()+'.log')
$rosterMethod=if($Blockers){'ForestBlockerImporter.Run'}elseif($Animations){'ForestRosterImporter.ImportAnimations'}else{'ForestRosterImporter.ImportKits'}
$rosterArgs=@('-batchmode','-nographics','-quit','-projectPath',('"{0}"' -f $rosterRoot),'-executeMethod',$rosterMethod,'-logFile',('"{0}"' -f $rosterLog))
$rosterJob=Start-Process -FilePath $rosterUnity -ArgumentList $rosterArgs -WindowStyle Hidden -PassThru
$rosterJob.WaitForExit();$rosterJob.Refresh()
Write-Output "Import exit: $($rosterJob.ExitCode); log: $rosterLog"
if($rosterJob.ExitCode -ne 0){Select-String -LiteralPath $rosterLog -Pattern 'error CS','Exception','Aborting';exit 1}
