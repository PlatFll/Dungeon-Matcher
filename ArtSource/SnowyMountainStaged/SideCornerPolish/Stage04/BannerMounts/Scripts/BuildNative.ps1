param([Parameter(Mandatory=$true)][string]$OutputDirectory)
$ErrorActionPreference='Stop'
$snowRoot=[IO.Path]::GetFullPath($OutputDirectory).Replace('\','/')
if(Test-Path -LiteralPath $snowRoot){throw 'Use a fresh output directory'}
foreach($snowFolder in @('Inputs','Exports','Native','Roundtrip','Scripts','QA')){New-Item -ItemType Directory -Path ($snowRoot+'/'+$snowFolder) -Force | Out-Null}
Copy-Item -Path ((Join-Path $PSScriptRoot '../Inputs')+'/*') -Destination ($snowRoot+'/Inputs')
$snowSource=(Get-Content -LiteralPath ($PSScriptRoot+'/BuildNative.js') -Raw).Replace('__ROOT__',("'"+$snowRoot+"/'"))
[IO.File]::WriteAllText(($snowRoot+'/Scripts/Build.js'),$snowSource)
$snowEditor='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
$snowProc=Start-Process -FilePath $snowEditor -ArgumentList @('--script',('"'+$snowRoot+'/Scripts/Build.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($snowRoot+'/QA/LibreSprite.log') -RedirectStandardError ($snowRoot+'/QA/LibreSprite-error.log')
$snowHandle=$snowProc.Handle
if(-not $snowProc.WaitForExit(60000)){throw 'Bounded native build wait exceeded'}
if($snowProc.ExitCode -ne 0 -or -not (Select-String -LiteralPath ($snowRoot+'/QA/LibreSprite.log') -Pattern 'SNOW_BANNER_MOUNTS_PASS' -Quiet)){throw 'Native build failed'}
$snowArgs=@('--batch')
foreach($snowFile in Get-ChildItem -LiteralPath ($snowRoot+'/Native') -Filter '*.ase'){$snowArgs+=@(('"'+$snowFile.FullName+'"'),'--save-as',('"'+$snowRoot+'/Roundtrip/'+$snowFile.BaseName+'.png"'))}
$snowProc=Start-Process -FilePath $snowEditor -ArgumentList $snowArgs -WindowStyle Hidden -PassThru -RedirectStandardOutput ($snowRoot+'/QA/Reopen.log') -RedirectStandardError ($snowRoot+'/QA/Reopen-error.log')
$snowHandle=$snowProc.Handle
if(-not $snowProc.WaitForExit(60000) -or $snowProc.ExitCode -ne 0){throw 'Native reopen failed'}
Write-Output 'LibreSprite saved and reopened both mounted flag variants.'
