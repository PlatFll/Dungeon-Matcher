$ErrorActionPreference = 'Stop'
$draftRoot = Join-Path $PSScriptRoot 'Superseded'
New-Item -ItemType Directory -Force -Path $draftRoot | Out-Null
foreach ($baseName in @('Gideon_Ready','Gideon_Idle','Gideon_Cast','Gideon_Hold','Gideon_Recovery','ChronoShutter_Flow')) {
    foreach ($extension in @('png','aseprite','gif','json')) {
        $sourcePath = Join-Path $PSScriptRoot ($baseName+'.'+$extension)
        $draftPath = Join-Path $draftRoot ($baseName+'.'+$extension)
        if ((Test-Path -LiteralPath $sourcePath) -and !(Test-Path -LiteralPath $draftPath)) {
            Copy-Item -LiteralPath $sourcePath -Destination $draftPath
        }
    }
}
foreach ($extension in @('png','aseprite')) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('Candidates/Gideon_Approved_03.'+$extension)) -Destination (Join-Path $PSScriptRoot ('Gideon_Ready.'+$extension))
}
