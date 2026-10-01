# LibreSprite scripting preserves RGBA cels but its LayerFromBackground command
# can leave background flags set. Normalize layer metadata only, before exporting
# through LibreSprite. No pixels, palette, dimensions or cel bytes are changed.
param([string]$SourceRoot = (Join-Path $PSScriptRoot '../ArtSource/FinalizedVisuals'))
$ErrorActionPreference = 'Stop'
foreach ($sourceFile in Get-ChildItem -LiteralPath $sourceRoot -Filter '*.aseprite' -Recurse) {
    $bytes = [IO.File]::ReadAllBytes($sourceFile.FullName)
    if ([BitConverter]::ToUInt16($bytes,4) -ne 0xA5E0) { throw "Invalid ASE: $($sourceFile.Name)" }
    $frameOffset = 128
    $changed = $false
    for ($frameIndex=0; $frameIndex -lt [BitConverter]::ToUInt16($bytes,6); $frameIndex++) {
        $frameSize = [BitConverter]::ToUInt32($bytes,$frameOffset)
        $chunkOffset = $frameOffset + 16
        while ($chunkOffset -lt $frameOffset + $frameSize) {
            $chunkSize = [BitConverter]::ToUInt32($bytes,$chunkOffset)
            if ($chunkSize -lt 6 -or $chunkOffset + $chunkSize -gt $bytes.Length) { throw 'Invalid ASE chunk' }
            if ([BitConverter]::ToUInt16($bytes,$chunkOffset+4) -eq 0x2004) {
                $flags = [BitConverter]::ToUInt16($bytes,$chunkOffset+6)
                $updated = $flags -band 0xFFF3
                if ($updated -ne $flags) {
                    [BitConverter]::GetBytes([UInt16]$updated).CopyTo($bytes,$chunkOffset+6)
                    $changed = $true
                }
            }
            $chunkOffset += $chunkSize
        }
        $frameOffset += $frameSize
    }
    if ($changed) { [IO.File]::WriteAllBytes($sourceFile.FullName,$bytes) }
}
