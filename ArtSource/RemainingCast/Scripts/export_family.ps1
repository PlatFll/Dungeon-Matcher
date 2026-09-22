param([string]$LibreSprite='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe')
$ErrorActionPreference='Stop'
$artRoot=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$familySources=@(Get-ChildItem -LiteralPath "$artRoot/RemainingCast/SelectedIdles" -Filter '*_Idle.aseprite')
$familySources+=Get-Item -LiteralPath "$artRoot/GuardIdles/CrossbowGuard_Idle.aseprite"
foreach($name in @('Miner','BasketVillager','BarricadeVillager')){$familySources+=Get-Item -LiteralPath "$artRoot/LocalEnemies/$($name)_Idle.aseprite"}
foreach($source in $familySources){
    $stem=Join-Path $source.DirectoryName $source.BaseName
    rtk proxy $LibreSprite --batch $source.FullName --save-as ($stem+'.gif') --sheet-type horizontal --sheet ($stem+'.png') --format json-array --data ($stem+'.json')
    if($LASTEXITCODE -ne 0){throw "Family export failed: $($source.Name)"}
    $metadata=Get-Content -LiteralPath ($stem+'.json') -Raw | ConvertFrom-Json
    $metadata.meta.image=$source.BaseName+'.png'
    [IO.File]::WriteAllText(($stem+'.json'),($metadata | ConvertTo-Json -Depth 20),[Text.UTF8Encoding]::new($false))
}
