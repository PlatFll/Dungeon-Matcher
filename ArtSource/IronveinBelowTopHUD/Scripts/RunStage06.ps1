param([string]$Label='Stage06')
$lowerProject=(Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$lowerLog=$lowerProject+'/ArtSource/IronveinBelowTopHUD/Stage06/QA/'+$Label+'.log'
$lowerArgs=@('-batchmode','-quit','-projectPath',('"'+$lowerProject+'"'),'-logFile',('"'+$lowerLog+'"'),'-executeMethod','IronveinBelowTopHUDStage06.Build')
$lowerJob=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe' -ArgumentList $lowerArgs -WindowStyle Hidden -PassThru
$lowerJob.WaitForExit();$lowerJob.Refresh()
Select-String -LiteralPath $lowerLog -Pattern 'IRONVEIN_|error CS|AssertionException|Exception:' | Select-Object -Last 12
Write-Output ('Unity exit: '+$lowerJob.ExitCode+'; log: '+$lowerLog)
exit $lowerJob.ExitCode
