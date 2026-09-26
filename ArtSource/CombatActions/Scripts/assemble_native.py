"""Assemble text-only native LibreSprite commands. Does not edit image files."""
from pathlib import Path
import json
root=Path(__file__).resolve().parents[1]
preamble='var ROOT='+json.dumps(root.as_posix()+'/')+';\nvar IDLES='+json.dumps((root.parent/'CombatIdles').as_posix()+'/')+';\nvar MAPS='+(root/'Scripts/material_map.json').read_text()+';\nvar PALETTES='+(root/'Scripts/palettes.json').read_text()+';\n'
preamble+='if(app.activeSprite && app.activeSprite.filename.indexOf(ROOT+"Review/")===0)app.activeDocument.close();\n'
script=preamble+(root/'Scripts/build_actions_body.js').read_text()
(root/'Scripts/ActionFamilyWork.js').write_text(script,encoding='utf-8')
Path(r'C:\Users\USER\Downloads\libresprite-development-windows-x86_64\data\scripts\ActionFamilyWork.js').write_text(script,encoding='utf-8')
print('Native script assembled.')
