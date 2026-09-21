param([string]$LibreSprite='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe')
$ErrorActionPreference='Stop'
foreach($guardName in @('CrossbowGuard','BarricadeGuard','SpearGuard','SiegeSergeant')) {
    $stem=Join-Path $PSScriptRoot ($guardName+'_Idle')
    rtk proxy $LibreSprite --batch ($stem+'.aseprite') --save-as ($stem+'.gif') --sheet-type horizontal --sheet ($stem+'.png') --format json-array --data ($stem+'.json')
    if($LASTEXITCODE -ne 0){throw "Guard export failed: $guardName"}
    $metadata=Get-Content -LiteralPath ($stem+'.json') -Raw | ConvertFrom-Json
    $metadata.meta.image=$guardName+'_Idle.png'
    [IO.File]::WriteAllText(($stem+'.json'),($metadata | ConvertTo-Json -Depth 20),[Text.UTF8Encoding]::new($false))
}
