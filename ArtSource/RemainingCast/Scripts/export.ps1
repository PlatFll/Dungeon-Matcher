param([string]$LibreSprite='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe')
$ErrorActionPreference='Stop'
$artRoot=Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $artRoot
try {
 foreach($source in Get-ChildItem -LiteralPath 'Recolored' -Filter '*.aseprite') {
  rtk proxy $LibreSprite --batch $source.FullName --save-as (Join-Path $source.DirectoryName ($source.BaseName+'.png'))
  if($LASTEXITCODE -ne 0){throw "Static export failed: $($source.Name)"}
 }
 foreach($source in Get-ChildItem -LiteralPath 'Idles' -Filter '*.aseprite') {
  $base=Join-Path $source.DirectoryName $source.BaseName
  rtk proxy $LibreSprite --batch $source.FullName --save-as ($base+'.gif') --sheet-type horizontal --sheet ($base+'.png') --format json-array --data ($base+'.json')
  if($LASTEXITCODE -ne 0){throw "Idle export failed: $($source.Name)"}
  $jsonPath=$base+'.json'
  $metadata=Get-Content -LiteralPath $jsonPath -Raw | ConvertFrom-Json
  $metadata.meta.image=$source.BaseName+'.png'
  [IO.File]::WriteAllText($jsonPath,($metadata | ConvertTo-Json -Depth 20),[Text.UTF8Encoding]::new($false))
 }
 rtk proxy $LibreSprite --batch 'Originals/RattleBones_FluidIdle.aseprite' --save-as 'Review/Rattlebones_Reference.gif' --sheet-type horizontal --sheet 'Review/Rattlebones_Reference.png' --format json-array --data 'Review/Rattlebones_Reference.json'
 if($LASTEXITCODE -ne 0){throw 'Reference export failed'}
 $referencePath=Join-Path $artRoot 'Review/Rattlebones_Reference.json'
 $metadata=Get-Content -LiteralPath $referencePath -Raw | ConvertFrom-Json
 $metadata.meta.image='Rattlebones_Reference.png'
 [IO.File]::WriteAllText($referencePath,($metadata | ConvertTo-Json -Depth 20),[Text.UTF8Encoding]::new($false))
} finally {Pop-Location}
