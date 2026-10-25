"""Read-only asset/import-output/production preservation checks for Stage 01."""
from pathlib import Path
from PIL import Image, ImageChops
import hashlib, json
root=Path(__file__).resolve().parents[2]
project=root.parents[1]
before=json.loads((root/'QA/ProtectedBefore.json').read_text())
changed=[p for p,v in before.items() if not (project/p).exists() or hashlib.sha256((project/p).read_bytes()).hexdigest()!=v['sha256']]
assert not changed, changed
checks=json.loads((root/'QA/Stage01/NativeChecks.json').read_text())
all_colors=set()
for c in checks:
    n=c['name']; export=root/f'Exports/Stage01/{n}.png'
    a=Image.open(export).convert('RGBA'); b=Image.open(root/f'QA/Stage01/{n}-roundtrip.png').convert('RGBA')
    assert a.size==b.size and a.tobytes()==b.tobytes(), n+' editable source round trip'
    assert set(a.getchannel('A').getdata())<={0,255},n+' alpha'
    assert hashlib.sha256(export.read_bytes()).hexdigest()==c['sha256'],n+' export hash'
    assert export.read_bytes()==(project/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage01/{n}.png').read_bytes(),n+' exact Unity PNG'
    all_colors.update(p[:3] for p in a.getdata() if p[3])
    print(f'{n}: {a.width}x{a.height}, {c["opaque_colors"]} colors, binary alpha, exact ASE round trip / Unity PNG')
native=Image.open(root/'Preview/Stage01/LayerStudy-Native.png').convert('RGBA')
actual=Image.open(root/'Preview/Stage01/Unity-Layers-720x1280.png').convert('RGBA')
# Pixel equivalence on an unobscured stretch of background in the actual Unity camera render.
area=(216,150,720,400)
expected=Image.new('RGBA',actual.size); expected.paste(native.resize((720,480),Image.Resampling.NEAREST),(0,4))
assert expected.crop(area).tobytes()==actual.crop(area).tobytes(),'Unity background rendered pixels differ from native 2x'
normal=Image.open(root/'Preview/Stage01/Unity-Actors-720x1280.png')
tall=Image.open(root/'Preview/Stage01/Unity-Actors-720x1600.png')
assert normal.crop((0,0,720,484)).tobytes()==tall.crop((0,0,720,484)).tobytes(),'Tall upper source mapping changed'
receipt=json.loads((root/'QA/Stage01/UnityPreview.json').read_text()); assert receipt['status']=='PASS'
print(f'{len(before)} protected files exact; rendered background matches 2x native pixels; tall upper view exact; {len(all_colors)} combined opaque layer colors.')
