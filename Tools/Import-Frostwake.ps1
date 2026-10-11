[CmdletBinding()]
param([ValidateSet('Kits','Art','Theme')][string]$Stage='Kits')
$ErrorActionPreference='Stop'
$frostRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$frostVersion=[regex]::Match((Get-Content (Join-Path $frostRoot 'ProjectSettings/ProjectVersion.txt') -Raw),'(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$frostUnity=Join-Path $env:ProgramFiles "Unity/Hub/Editor/$frostVersion/Editor/Unity.exe"
$frostOutput=Join-Path $frostRoot '.utmp/Frostwake'
New-Item -ItemType Directory -Force -Path $frostOutput | Out-Null
$frostLog=Join-Path $frostOutput ('import-'+$Stage+'-'+[Guid]::NewGuid()+'.log')
$frostMethod=if($Stage -eq 'Kits'){'FrostwakeZoneImporter.ImportKits'}elseif($Stage -eq 'Art'){'FrostwakeArtImporter.Run'}else{'FrostwakeThemeImporter.Run'}
$frostArgs=@('-batchmode','-nographics','-quit','-projectPath',('"{0}"' -f $frostRoot),'-executeMethod',$frostMethod,'-logFile',('"{0}"' -f $frostLog))
$frostJob=Start-Process -FilePath $frostUnity -ArgumentList $frostArgs -WindowStyle Hidden -PassThru
$frostJob.WaitForExit();$frostJob.Refresh()
Write-Output "Import exit: $($frostJob.ExitCode); log: $frostLog"
if($frostJob.ExitCode -ne 0){Select-String -LiteralPath $frostLog -Pattern 'error CS','Exception','Aborting';exit 1}
