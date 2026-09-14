[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$Method, [string]$Name = 'scenario')
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$version = [regex]::Match((Get-Content (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Raw), '(?m)^m_EditorVersion:\s*(\S+)').Groups[1].Value
$unityExe = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe"
$output = Join-Path $projectRoot '.utmp/DesignV2'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$log = Join-Path $output ($Name + '.log')
$arguments = @('-batchmode','-projectPath',('"{0}"' -f $projectRoot),'-executeMethod',$Method,'-logFile',('"{0}"' -f $log))
$started = Get-Date
$scenarioProcess = Start-Process -FilePath $unityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru
$scenarioProcess.WaitForExit()
$scenarioProcess.Refresh()
Write-Output "Unity scenario $Method exited $($scenarioProcess.ExitCode) after $([Math]::Round(((Get-Date)-$started).TotalSeconds,1))s. Log: $log"
if ($scenarioProcess.ExitCode -ne 0) {
    Select-String -LiteralPath $log -Pattern 'error CS|Exception|FAILED|validation:' | Select-Object -Last 30
    exit 1
}
