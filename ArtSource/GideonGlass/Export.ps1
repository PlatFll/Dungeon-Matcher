param([string[]]$Names = @('Gideon_Cast','Gideon_Hold','Gideon_Recovery'))
$ErrorActionPreference='Stop'
Push-Location -LiteralPath $PSScriptRoot
try {
    foreach ($name in $Names) {
        rtk proxy 'C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe' --batch ($name+'.aseprite') --save-as ($name+'.gif') --sheet-type horizontal --sheet ($name+'.png') --format json-array --data ($name+'.json')
        if($LASTEXITCODE -ne 0) { throw "Export failed: $name" }
    }
} finally { Pop-Location }
