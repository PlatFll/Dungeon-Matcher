"""Preserve the source and sample authoritative pixels; LibreSprite does the edit."""
from pathlib import Path
from PIL import Image
import hashlib,json,shutil
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[3]
source=ROOT/'Assets/_Game/Art/DrownedCourt/reef_netweaver.png'
before=HERE/'Before.png'
if not before.exists():shutil.copy2(HERE.parent/'MarineSnare.png',before)
if not (HERE/'Before.ase').exists():shutil.copy2(HERE.parent/'MarineSnare.ase',HERE/'Before.ase')
shutil.copy2(source,HERE/'ReefNetweaver-reference.png')
im=Image.open(source).convert('RGBA')
roi=im.crop((70,60,87,88))
colors={(10,13,17),(74,44,28),(122,77,46),(176,122,67),(184,200,190)}
assert colors<={c[:3] for c in roi.get_flattened_data() if c[3]}
reference={
    'enemy_id':'reef_netweaver','display_name':'Reef Netweaver',
    'definition':'Assets/_Game/Data/Enemies/DrownedCourt/reef_netweaver.asset',
    'special_ability_kind':'ThornySnare = 24',
    'source':source.relative_to(ROOT).as_posix(),
    'sha256':hashlib.sha256(source.read_bytes()).hexdigest(),
    'guid':'147a8afb737eb0648b696771460020bc','file_id':21300000,
    'canvas':list(im.size),'held_net_sampling_rect':[70,60,87,88],
    'colors':{','.join(map(str,c)):next([x,y] for y in range(60,88) for x in range(70,87) if im.getpixel((x,y))[:3]==c and im.getpixel((x,y))[3]) for c in sorted(colors)},
    'pixel_lab_generations':0,
    'method':'Exact RGB replacement in LibreSprite via recolor.js; source geometry and alpha locked.'}
(HERE/'reference.json').write_text(json.dumps(reference,indent=2)+'\n')
print('Prepared a separate net copy and verified all five colors occur in the held net.')
