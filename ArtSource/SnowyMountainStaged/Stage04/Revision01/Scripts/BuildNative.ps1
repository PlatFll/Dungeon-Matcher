[CmdletBinding()]
param([string]$OutputDirectory)
$ErrorActionPreference='Stop'
$snowInputRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if(-not $OutputDirectory) { $OutputDirectory=Join-Path $env:TEMP ('SnowyMountainNative-'+[guid]::NewGuid().ToString()) }
$snowBuildRoot=[IO.Path]::GetFullPath($OutputDirectory).Replace('\','/')
if(Test-Path -LiteralPath $snowBuildRoot) { throw 'Choose a new output directory; existing artwork will not be overwritten.' }
foreach($snowDir in @('Raw','Native','Exports','Roundtrip')) { New-Item -ItemType Directory -Path (Join-Path $snowBuildRoot $snowDir) -Force | Out-Null }
foreach($snowFile in @('OriginalPixelLab.png','Blank64x84.png','Blank154x52.png')) { Copy-Item -LiteralPath (Join-Path $snowInputRoot ('Raw/'+$snowFile)) -Destination (Join-Path $snowBuildRoot ('Raw/'+$snowFile)) }
$snowSource=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RestoreOriginal.js') -Raw
$snowSource=[regex]::Replace($snowSource,'(?m)^var root=.*;',("var root='"+$snowBuildRoot+"/';"))
$snowScript=Join-Path $snowBuildRoot 'RestoreOriginal.js'
[IO.File]::WriteAllText($snowScript,$snowSource)
$snowJob=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$snowScript+'"')) -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput ($snowBuildRoot+'/LibreSprite.log') -RedirectStandardError ($snowBuildRoot+'/LibreSprite-error.log')
Write-Output ('LibreSprite exit: '+$snowJob.ExitCode)
if($snowJob.ExitCode -ne 0) { throw ('Native export failed; inspect '+$snowBuildRoot+'/LibreSprite.log') }
if(-not (Select-String -Path ($snowBuildRoot+'/LibreSprite.log') -Pattern 'SNOW_REVISION_LIBRESPRITE_PASS' -Quiet)) { throw 'Native completion marker is missing.' }
foreach($snowName in @('Ledge_0','Ledge_1','Ledge_2','Ledge_3','Ledge_4','CobbleWall')) {
    foreach($snowSuffix in @(('Native/'+$snowName+'.ase'),('Exports/'+$snowName+'.png'),('Roundtrip/'+$snowName+'.png'))) {
        if(-not (Test-Path -LiteralPath (Join-Path $snowBuildRoot $snowSuffix))) { throw ('Missing native output '+$snowSuffix) }
    }
}
Write-Output ('Fresh native source and roundtrip exports: '+$snowBuildRoot)
