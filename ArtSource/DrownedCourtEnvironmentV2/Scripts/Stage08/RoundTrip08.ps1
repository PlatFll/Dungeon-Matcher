$ErrorActionPreference='Stop'
$root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2'
foreach($source in Get-ChildItem -LiteralPath ($root+'/Native/Stage08') -Filter '*.ase') {
 & 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -b $source.FullName --save-as ($root+'/QA/Stage08/'+$source.BaseName+'-roundtrip.png') | Out-Null
 if($LASTEXITCODE -ne 0){throw ('Roundtrip failed '+$source.Name)}
}
Write-Output '3 native documents re-exported by LibreSprite.'
