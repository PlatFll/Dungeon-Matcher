from pathlib import Path
import hashlib, importlib.util, json
from PIL import Image

HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
spec=importlib.util.spec_from_file_location('menu_validator',ROOT/'Tools/Validate-MenuArt.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
manifest=json.loads((HERE/'manifest.json').read_text())
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for row in manifest['assets']:
    png=ROOT/row['native'];ase=png.with_suffix('.aseprite');runtime=ROOT/row['runtime']
    assert sha(png)==sha(runtime)==row['pngSha256'],row['name']
    assert sha(ase)==row['aseSha256'],row['name']
    a=Image.open(png).convert('RGBA'); b=module.ase_pixels(ase)
    assert a.size==b.size and a.tobytes()==b.tobytes(),row['name']
    assert set(a.getchannel('A').tobytes()) <= {0,255},row['name']
    meta=runtime.with_suffix('.png.meta').read_text()
    for field in ('enableMipMap: 0','filterMode: 0','nPOTScale: 0','spritePixelsToUnits: 100','textureCompression: 0'):
        assert field in meta,(row['name'],field)
    assert 'overridden: 1' not in meta,row['name']
assert len(manifest['assets'])==37
print('PASS: 37 immutable approved PNG/ASE sources, binary alpha, exact runtime copies and native imports.')
