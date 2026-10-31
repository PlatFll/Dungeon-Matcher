[CmdletBinding()]
param([string]$OutputDirectory)
$ErrorActionPreference='Stop'
$snowInputRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if(-not $OutputDirectory){$OutputDirectory=Join-Path $env:TEMP ('SnowStage08Native-'+[guid]::NewGuid().ToString())}
$snowBuildRoot=[IO.Path]::GetFullPath($OutputDirectory).Replace('\','/')
if(Test-Path -LiteralPath $snowBuildRoot){throw 'Choose a fresh output directory; existing artwork is preserved.'}
foreach($snowDir in @('Raw','Native','Exports','Roundtrip','QA')){New-Item -ItemType Directory -Path (Join-Path $snowBuildRoot $snowDir) -Force | Out-Null}
Copy-Item -Path (Join-Path $snowInputRoot 'Raw/*') -Destination (Join-Path $snowBuildRoot 'Raw')
$snowSource=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'BuildNative.js') -Raw
$snowSource=[regex]::Replace($snowSource,'(?m)^var root=.*;',("var root='"+$snowBuildRoot+"/';"))
$snowScript=Join-Path $snowBuildRoot 'BuildNative.js'
[IO.File]::WriteAllText($snowScript,$snowSource)
$snowJob=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$snowScript+'"')) -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput ($snowBuildRoot+'/QA/LibreSprite.log') -RedirectStandardError ($snowBuildRoot+'/QA/LibreSprite-error.log')
if($snowJob.ExitCode -ne 0){throw ('Native process failed: '+$snowJob.ExitCode)}
if(-not (Select-String -Path ($snowBuildRoot+'/QA/LibreSprite.log') -Pattern 'SNOW8_LIBRESPRITE_PASS' -Quiet)){throw 'Completion marker missing'}
Write-Output ('LibreSprite exit 0; saved/reopened outputs: '+$snowBuildRoot)
