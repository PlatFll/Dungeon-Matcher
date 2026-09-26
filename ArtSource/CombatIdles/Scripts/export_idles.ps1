param([string]$LibreSprite = 'C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe')
$ErrorActionPreference = 'Stop'
$idleRoot = Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $idleRoot
try {
    foreach ($character in @('Rattlebones', 'Farmer', 'PanVillager', 'Bardley')) {
        # Relative export names keep the JSON metadata portable with the sprite sheet.
        $stem = $character + '_Idle'
        rtk proxy $LibreSprite --batch ($stem + '.aseprite') --save-as ($stem + '.gif') --sheet-type horizontal --sheet ($stem + '.png') --format json-array --data ($stem + '.json')
        if ($LASTEXITCODE -ne 0) { throw "LibreSprite export failed for $character" }
        Write-Output "Exported $character"
    }
} finally { Pop-Location }
