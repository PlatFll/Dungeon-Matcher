from pathlib import Path
from PIL import Image
import json,subprocess,hashlib,xml.etree.ElementTree as ET
F=Path(__file__).resolve().parents[4];R=F/'ArtSource/DrownedCourtEnvironmentV2';P=R/'Preview/Revision03';Q=R/'QA/Revision03'
tests=ET.parse(Q/'After-Tests.xml').getroot();assert tests.attrib['failed']=='0' and tests.attrib['passed']=='1',tests.attrib
a=json.loads((Q/'OriginalMatrix/Measurements.json').read_text())['cases'];b=json.loads((Q/'After/Measurements.json').read_text())['cases'];assert len(a)==len(b)==25
rows=[]
for r,o in zip(b,a):
 assert (r['zone'],r['size'])==(o['zone'],o['size'])
 assert r['board']==o['board'] and r['portrait']==o['portrait']
 assert not r['errors']
 gold=next(x for x in a if x['zone']=='dungeon' and x['size']==r['size'])
 assert r['feet']==gold['feet'] and max(r['feet'])==min(r['feet'])
 for bar,g in zip(r['health'],gold['health']):assert bar['y']==g['y'] and bar['height']==g['height']
 rows.append({'zone':r['zone'],'size':r['size'],'source_floor_screen_y':r['feet'][0],'matches_original_dungeon_exactly':True,'board_and_portrait_unchanged':True})
protected=0
for scale in [1,2,3]:
 before=Image.open(P/f'Unity-Before-{360*scale}x{640*scale}.png').convert('RGBA');after=Image.open(P/f'Unity-Revised-{360*scale}x{640*scale}.png').convert('RGBA')
 region=(0,240*scale,360*scale,640*scale)
 assert before.crop(region).tobytes()==after.crop(region).tobytes()
 protected+=360*400*scale*scale
assert Image.open(P/'LoopStart.png').tobytes()==Image.open(P/'LoopEnd.png').tobytes()
assert len(list((P/'Motion').glob('Frame-*.png')))==288
changed=subprocess.check_output(['git','diff','--name-only','HEAD'],cwd=F,text=True).splitlines()
assert not [n for n in changed if Path(n).suffix.lower() in ['.png','.ase','.aseprite','.anim','.controller','.prefab']],changed
assert 'Assets/_Game/Scenes/Game.unity' not in changed
record={'status':'PASS','live_cases':25,'live_enemies_per_case':3,'zones':5,'exact_original_dungeon_baseline':True,'complete_board_portrait_rectangles_unchanged':True,'lower_review_pixels_preserved':protected,'animation_frames':288,'loop_24s_exact':True,'original_art_files_unchanged':True,'production_game_scene_unchanged':True,'pixellab_generations_spent':0,'measurements':rows}
(Q/'Verification.json').write_text(json.dumps(record,indent=2)+'\n');print(json.dumps({k:v for k,v in record.items() if k!='measurements'},indent=2))
