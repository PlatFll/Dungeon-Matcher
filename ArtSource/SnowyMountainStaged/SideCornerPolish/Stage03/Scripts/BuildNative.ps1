[CmdletBinding()]
param([string]$OutputDirectory)
$ErrorActionPreference='Stop'
$snowInput=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if(-not $OutputDirectory){$OutputDirectory=Join-Path $env:TEMP ('SnowCorner03-'+[guid]::NewGuid().ToString('N').Substring(0,8))}
$snowOutput=[IO.Path]::GetFullPath($OutputDirectory).Replace('\','/')
if(Test-Path -LiteralPath $snowOutput){throw 'Use a fresh output directory; existing artwork is preserved.'}
foreach($snowFolder in @('Raw','Native','Exports','Roundtrip','QA')){New-Item -ItemType Directory -Path ($snowOutput+'/'+$snowFolder) -Force | Out-Null}
# Aliases avoid LibreSprite detecting adjacent numbered candidates as animation.
Copy-Item -LiteralPath ($snowInput+'/Raw/Left_3.png') -Destination ($snowOutput+'/Raw/SelectedLeft.png')
Copy-Item -LiteralPath ($snowInput+'/Raw/RightCargo_0.png') -Destination ($snowOutput+'/Raw/SelectedRight.png')
Copy-Item -Path ($snowInput+'/Raw/Approved*Connector.png') -Destination ($snowOutput+'/Raw')
Copy-Item -Path ($snowInput+'/Raw/Blank*.png') -Destination ($snowOutput+'/Raw')
$snowSource=Get-Content -LiteralPath ($PSScriptRoot+'/BuildNative.js') -Raw
$snowSource=$snowSource.Replace('__ROOT__',("'"+$snowOutput+"/'"))
[IO.File]::WriteAllText(($snowOutput+'/Build.js'),$snowSource)
$snowEditor='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$snowProcess=Start-Process -FilePath $snowEditor -ArgumentList @('--script',('"'+$snowOutput+'/Build.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($snowOutput+'/QA/LibreSprite.log') -RedirectStandardError ($snowOutput+'/QA/LibreSprite-error.log')
$snowHandle=$snowProcess.Handle
if(-not $snowProcess.WaitForExit(60000)){Stop-Process -Id $snowProcess.Id;throw 'Owned LibreSprite build exceeded 60 seconds'}
if($snowProcess.ExitCode -ne 0){throw ('Native export failed: '+$snowProcess.ExitCode)}
if(-not (Select-String -LiteralPath ($snowOutput+'/QA/LibreSprite.log') -Pattern 'SNOW_CORNER_NATIVE_PASS' -Quiet)){throw 'Native completion marker missing'}
$snowArgs=@('--batch')
foreach($snowFile in Get-ChildItem -LiteralPath ($snowOutput+'/Native') -Filter '*.ase'){$snowArgs+=@(('"'+$snowFile.FullName+'"'),'--save-as',('"'+$snowOutput+'/Roundtrip/'+$snowFile.BaseName+'.png"'))}
$snowProcess=Start-Process -FilePath $snowEditor -ArgumentList $snowArgs -WindowStyle Hidden -PassThru -RedirectStandardOutput ($snowOutput+'/QA/Reopen.log') -RedirectStandardError ($snowOutput+'/QA/Reopen-error.log')
$snowHandle=$snowProcess.Handle
if(-not $snowProcess.WaitForExit(60000)){Stop-Process -Id $snowProcess.Id;throw 'Owned round-trip export exceeded 60 seconds'}
if($snowProcess.ExitCode -ne 0){throw 'Round-trip export failed'}
Write-Output ('Native output and round-trip exports: '+$snowOutput)
