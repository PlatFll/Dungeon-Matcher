param([switch]$Review,[string]$LibreSprite='C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe')
$ErrorActionPreference='Stop'
$artRoot=Split-Path -Parent $PSScriptRoot
if($Review){$artRoot=Join-Path $artRoot 'Review'}
Push-Location -LiteralPath $artRoot
try {
    foreach($name in @('Miner_Idle','Miner_AutoAttack','Miner_Ability','BasketVillager_Idle','BasketVillager_AutoAttack','BarricadeVillager_Idle','BarricadeVillager_AutoAttack','BarricadeVillager_Ability')) {
        rtk proxy $LibreSprite --batch ($name+'.aseprite') --save-as ($name+'.gif') --sheet-type horizontal --sheet ($name+'.png') --format json-array --data ($name+'.json')
        if($LASTEXITCODE -ne 0){throw "LibreSprite export failed for $name"}
    }
    $referenceFrames=Join-Path $artRoot 'Review/ReferenceFrames'
    New-Item -ItemType Directory -Force -Path $referenceFrames | Out-Null
    foreach($character in @('Miner','BasketVillager','BarricadeVillager')) {
        # This LibreSprite development build ignores --frame-range. Export the
        # native PNG sequence to scratch and copy its first frame unchanged.
        rtk proxy $LibreSprite --batch ($character+'_Idle.aseprite') --save-as (Join-Path $referenceFrames ($character+'_Reference.png'))
        if($LASTEXITCODE -ne 0){throw "LibreSprite reference export failed for $character"}
        Copy-Item -LiteralPath (Join-Path $referenceFrames ($character+'_Reference1.png')) -Destination ($character+'_Reference.png') -Force
    }
} finally {Pop-Location}
