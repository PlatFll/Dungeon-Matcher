"""Assemble text-only commands for native LibreSprite editing."""
from pathlib import Path
root=Path(__file__).resolve().parents[1]
script='var MAPS='+(root/'Scripts/material_map.json').read_text()+';\n'
script+='var PALETTES='+(root/'Scripts/palettes.json').read_text()+';\n'
script+=(root/'Scripts/build_native.js').read_text()
(root/'Scripts/LocalEnemyWork.js').write_text(script,encoding='utf-8')
Path('C:/Users/USER/Downloads/libresprite-development-windows-x86_64/data/scripts/LocalEnemyWork.js').write_text(script,encoding='utf-8')
print('Native LibreSprite commands assembled.')
