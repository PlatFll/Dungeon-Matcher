param([string]$OutputPath)
$ErrorActionPreference='Stop'
if(-not $OutputPath){$OutputPath=Join-Path (Split-Path $PSScriptRoot -Parent) 'Review/RefineFamily.js'}
$helperText=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'animate_native.js'))
$lastLoop=$helperText.LastIndexOf('for(var n=0;n<CAST.length;n++)')
if($lastLoop -lt 0){throw 'Expected anatomy-helper boundary missing'}
# Keep the inspected part selections and native file helpers. The old top-level
# animation run is excluded; refine_family_native supplies the accepted motion.
$script=$helperText.Substring(0,$lastLoop)+[Environment]::NewLine+[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'refine_family_native.js'))
[IO.File]::WriteAllText($OutputPath,$script,[Text.UTF8Encoding]::new($false))
Write-Output "Native LibreSprite script assembled: $OutputPath"
