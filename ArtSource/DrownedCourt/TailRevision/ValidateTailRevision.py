"""Read-only verification of current source PNGs and Unity exports."""
from pathlib import Path
from PIL import Image
import json,hashlib
here=Path(__file__).resolve().parent;repo=here.parents[2];selected=here.parent/'Production/Selected';art=repo/'Assets/_Game/Art/DrownedCourt'
checks=json.loads((here/'TechnicalChecks.json').read_text())['checks']
for c in checks:
    p=selected/c['file'];old=Image.open(here/'Before'/c['file']).convert('RGBA');new=Image.open(p).convert('RGBA')
    assert hashlib.sha256(p.read_bytes()).hexdigest()==c['sha256'],p
    assert old.size==new.size==tuple(c['size']),p
    assert set(new.getchannel('A').get_flattened_data())=={0,255},p
    assert {x[:3] for x in new.get_flattened_data() if x[3]}=={x[:3] for x in old.get_flattened_data() if x[3]},p
    assert all(a==b for a,b in zip(old.get_flattened_data(),new.get_flattened_data()) if a[3]),p
    if c['file'].startswith('Motion/'):
        _,name,state=Path(c['file']).parts;export=art/(name+'_'+state)
    else:export=art/Path(c['file']).name
    if c['file'].endswith('_wide.png'):continue # Source canvas helper, not an imported still.
    assert export.read_bytes()==p.read_bytes(),export
    meta=Path(str(export)+'.meta').read_text()
    assert 'filterMode: 0' in meta and 'enableMipMap: 0' in meta and 'textureCompression: 0' in meta,export
print(f'{len(checks)} source PNGs verified; 15 Unity exports identical; dimensions/palettes/body pixels/alpha/import settings preserved.')
