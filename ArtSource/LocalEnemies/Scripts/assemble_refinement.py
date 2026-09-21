"""Assemble native LibreSprite script text; never edit raster pixels here."""
from pathlib import Path
import json
root=Path(__file__).resolve().parent
palettes=json.loads((root/'palettes.json').read_text())
groups=json.loads((root/'refinement_materials.json').read_text())
maps={name:{color:color for color in colors} for name,colors in palettes.items()}
for name, ramps in groups.items():
    for target, colors in ramps.items():
        assert target in palettes[name]
        maps[name].update({color:target for color in colors.split()})
helpers=(root/'build_native.js').read_text().split('function write(')[0]
script='var REFINEMENT_MAPS='+json.dumps(maps)+';\n'+helpers+'\n'+(root/'refine_native.js').read_text()
(root/'../Review/Refined').mkdir(exist_ok=True,parents=True)
(root/'../Review/RefinementWork.js').write_text(script)
print('Assembled Review/RefinementWork.js for native LibreSprite execution.')
