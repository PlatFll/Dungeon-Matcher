$ErrorActionPreference='Stop'
$menuProject=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$menuVersion=[regex]::Match((Get-Content -LiteralPath (Join-Path $menuProject 'ProjectSettings/ProjectVersion.txt') -Raw),'(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$menuUnity=Join-Path $env:ProgramFiles "Unity/Hub/Editor/$menuVersion/Editor/Unity.exe"
$menuLog=Join-Path $menuProject '.utmp/MenuUI/import.log'
$menuArgs=@('-batchmode','-nographics','-quit','-projectPath',('"{0}"' -f $menuProject),'-logFile',('"{0}"' -f $menuLog),'-executeMethod','GlobalMenuArtImporter.Run')
$menuJob=Start-Process -FilePath $menuUnity -ArgumentList $menuArgs -WindowStyle Hidden -PassThru
$menuJob.WaitForExit();$menuJob.Refresh()
Write-Output "Unity import exit: $($menuJob.ExitCode); log: $menuLog"
if($menuJob.ExitCode -ne 0){Select-String -LiteralPath $menuLog -Pattern 'error CS|Exception|Aborting';exit 1}
