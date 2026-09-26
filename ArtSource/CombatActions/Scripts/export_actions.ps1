param([switch]$Review, [string]$LibreSprite='C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe')
$ErrorActionPreference='Stop'
$actionRoot=Split-Path -Parent $PSScriptRoot
if ($Review) { $actionRoot=Join-Path $actionRoot 'Review' }
Push-Location -LiteralPath $actionRoot
try {
    foreach ($name in @('Farmer_AutoAttack','PanVillager_AutoAttack','Rattlebones_Ability','Bardley_Ability')) {
        rtk proxy $LibreSprite --batch ($name+'.aseprite') --save-as ($name+'.gif') --sheet-type horizontal --sheet ($name+'.png') --format json-array --data ($name+'.json')
        if ($LASTEXITCODE -ne 0) { throw "LibreSprite export failed for $name" }
    }
} finally { Pop-Location }
