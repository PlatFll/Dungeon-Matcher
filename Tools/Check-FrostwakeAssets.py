"""Verify native export bytes, palettes, alpha and Unity slice registration."""
from pathlib import Path
from PIL import Image
import hashlib,json,re
root=Path(__file__).resolve().parents[1]
source=root/'ArtSource/Frostwake'
clips=json.loads((source/'production/animation-manifest.json').read_text())['clips']
records=[]
for spec in clips:
    name,state=spec['name'],spec['state']
    png=source/'production'/name/(state+'.png')
    imported=root/'Assets/_Game/Art/Frostwake/Motion'/(name+'_'+state+'.png')
    assert png.read_bytes()==imported.read_bytes(),str(imported)
    im=Image.open(png).convert('RGBA')
    assert im.size==(128*spec['frameCount'],104)
    assert set(im.getchannel('A').get_flattened_data())<={0,255}
    palette={c for c in Image.open(source/'approved'/(name+'.png')).convert('RGBA').get_flattened_data() if c[3]}
    assert {c for c in im.get_flattened_data() if c[3]}<=palette,name
    meta=imported.with_suffix('.png.meta').read_text()
    pivots=re.findall(r'      pivot: \{x: ([\d.]+), y: ([\d.]+)\}',meta)
    assert len(pivots)==spec['frameCount'],name+' '+state
    assert all(abs(float(x)-.5)<1e-6 and abs(float(y)*104-4)<1e-5 for x,y in pivots)
    assert 'filterMode: 0' in meta and 'enableMipMap: 0' in meta
    records.append(dict(name=name,state=state,exposures=spec['frameCount'],sha256=hashlib.sha256(png.read_bytes()).hexdigest()))
assert len(records)==70 and sum(r['exposures'] for r in records)==501
eyes=json.loads((root/'ArtSource/FrostwakePlayables/eye-revision/verification.json').read_text())
for item in eyes['outputs']:
    assert hashlib.sha256((root/item['path']).read_bytes()).hexdigest()==item['sha256']
ledger=root/'.utmp/FrostwakePlayables/user-changes.json'
preserved=json.loads(ledger.read_text()) if ledger.exists() else {}
for path,expected in preserved.items():
    assert hashlib.sha256((root/path).read_bytes()).hexdigest()==expected,path
report=dict(clips=70,exposures=501,canvas=[128,104],floor_pixels=4,
    exact_unity_exports=True,approved_palettes=True,binary_alpha=True,
    minotaur_outputs=len(eyes['outputs']),user_files_preserved=len(preserved),assets=records)
(source/'production/import-verification.json').write_text(json.dumps(report,indent=2))
print('PASS: 70 exact Unity sheets, 501 native slices, approved palettes/binary alpha, four-pixel floors; Minotaur hashes verified; preserved user files:',len(preserved))
