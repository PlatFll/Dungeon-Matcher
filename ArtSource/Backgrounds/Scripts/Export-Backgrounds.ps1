param([Parameter(Mandatory=$true)][string]$LibreSprite)
$ErrorActionPreference = 'Stop'
$sourceRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
foreach ($native in Get-ChildItem -LiteralPath $sourceRoot -Filter '*.aseprite' -Recurse) {
    $destination = [IO.Path]::ChangeExtension($native.FullName, '.png')
    rtk proxy $LibreSprite --batch $native.FullName --save-as $destination
    if ($LASTEXITCODE -ne 0) { throw "LibreSprite export failed: $($native.FullName)" }
}
