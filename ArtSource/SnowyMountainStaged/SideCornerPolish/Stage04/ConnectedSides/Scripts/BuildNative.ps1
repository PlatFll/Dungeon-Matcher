param([Parameter(Mandatory=$true)][string]$OutputDirectory)
$ErrorActionPreference='Stop'
$snowInputs=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$snowRoot=[IO.Path]::GetFullPath($OutputDirectory).Replace('\','/')
if(Test-Path -LiteralPath $snowRoot){throw 'Use a fresh output folder to preserve all existing art.'}
foreach($snowFolder in @('Inputs','Exports','Native','Roundtrip','QA','Scripts')){New-Item -ItemType Directory -Path ($snowRoot+'/'+$snowFolder) -Force | Out-Null}
Copy-Item -Path ($snowInputs+'/Inputs/*') -Destination ($snowRoot+'/Inputs')
$snowSource=Get-Content -LiteralPath ($PSScriptRoot+'/BuildNative.js') -Raw
$snowSpecs=Get-Content -LiteralPath ($snowRoot+'/Inputs/Layouts.json') -Raw
$snowSource=$snowSource.Replace('__ROOT__',("'"+$snowRoot+"/'")).Replace('__SPECS__',$snowSpecs)
[IO.File]::WriteAllText(($snowRoot+'/Scripts/Build.js'),$snowSource)
$snowEditor='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$snowProc=Start-Process -FilePath $snowEditor -ArgumentList @('--script',('"'+$snowRoot+'/Scripts/Build.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($snowRoot+'/QA/LibreSprite.log') -RedirectStandardError ($snowRoot+'/QA/LibreSprite-error.log')
$snowHandle=$snowProc.Handle
if(-not $snowProc.WaitForExit(60000)){throw 'LibreSprite build exceeded bounded wait; inspect owned process.'}
if($snowProc.ExitCode -ne 0){throw 'LibreSprite failed'}
if(-not (Select-String -LiteralPath ($snowRoot+'/QA/LibreSprite.log') -Pattern 'SNOW_CONNECTED_PASS' -Quiet)){throw 'Native completion marker missing'}
$snowArgs=@('--batch')
foreach($snowFile in Get-ChildItem -LiteralPath ($snowRoot+'/Native') -Filter '*.ase'){$snowArgs+=@(('"'+$snowFile.FullName+'"'),'--save-as',('"'+$snowRoot+'/Roundtrip/'+$snowFile.BaseName+'.png"'))}
$snowProc=Start-Process -FilePath $snowEditor -ArgumentList $snowArgs -WindowStyle Hidden -PassThru -RedirectStandardOutput ($snowRoot+'/QA/Reopen.log') -RedirectStandardError ($snowRoot+'/QA/Reopen-error.log')
$snowHandle=$snowProc.Handle
if(-not $snowProc.WaitForExit(60000)){throw 'LibreSprite round trip exceeded bounded wait'}
if($snowProc.ExitCode -ne 0){throw 'LibreSprite round trip failed'}
Write-Output 'LibreSprite authored and reopened all ten continuous side/corner assets.'
