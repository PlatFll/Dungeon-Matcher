param([string]$Method='IronveinStagedPreview.Build',[string]$Label='Assembly',[string]$Test='')
$ironProject=(Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$ironOutput=$ironProject+'/ArtSource/IronveinStaged/QA'
$ironLog=$ironOutput+'/'+$Label+'.log'
$ironArgs=@('-batchmode','-projectPath',('"'+$ironProject+'"'),'-logFile',('"'+$ironLog+'"'))
if($Test){$ironArgs+=@('-runTests','-testPlatform','EditMode','-testFilter',$Test,'-testResults',('"'+$ironOutput+'/'+$Label+'.xml"'))}
else{$ironArgs+=@('-quit','-executeMethod',$Method)}
$ironJob=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe' -ArgumentList $ironArgs -WindowStyle Hidden -PassThru
$ironJob.WaitForExit();$ironJob.Refresh()
Select-String -LiteralPath $ironLog -Pattern 'IRONVEIN_|error CS|AssertionException|Exception:|Test run|TEST RUN' | Select-Object -Last 12
Write-Output ('Unity exit: '+$ironJob.ExitCode+'; log: '+$ironLog)
exit $ironJob.ExitCode
