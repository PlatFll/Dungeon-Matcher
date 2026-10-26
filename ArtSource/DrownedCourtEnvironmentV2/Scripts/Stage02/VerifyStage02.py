"""Verify Stage 02 native assets, preserved source bytes, module joins and Unity camera pixels."""
from pathlib import Path
from PIL import Image
import hashlib,json
R=Path(__file__).resolve().parents[2];F=R.parents[1];Q=R/'QA/Stage02';E=R/'Exports/Stage02';P=R/'Preview/Stage02'
protected=json.loads((R/'QA/ProtectedBefore.json').read_text());accepted=json.loads((Q/'AcceptedStage01Hashes.json').read_text())
changed=[p for p,v in protected.items() if not (F/p).exists() or hashlib.sha256((F/p).read_bytes()).hexdigest()!=v['sha256']]
changed+= [p for p,v in accepted.items() if not (F/p).exists() or hashlib.sha256((F/p).read_bytes()).hexdigest()!=v]
assert not changed,changed
data=json.loads((R/'Native/Stage02/Manifest.json').read_text());colors=set()
for a in data['assets']:
 n=a['name'];p=E/(n+'.png');im=Image.open(p).convert('RGBA');out=Image.open(Q/(n+'-roundtrip.png')).convert('RGBA')
 assert im.tobytes()==out.tobytes() and im.size==out.size,n+' ASE round trip'
 assert p.read_bytes()==(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage02/{n}.png').read_bytes(),n+' Unity PNG'
 assert hashlib.sha256(p.read_bytes()).hexdigest()==a['sha256'],n+' source hash'
 assert set(im.getchannel('A').getdata())<={0,255},n+' alpha'
 raw=R/f'RawPixelLab/Stage02/{n}-raw.png'
 if raw.exists():assert im.getchannel('A').tobytes()==Image.open(raw).convert('RGBA').getchannel('A').tobytes(),n+' raw silhouette'
 colors.update(p[:3] for p in im.getdata() if p[3]);print(f'{n}: {im.width}x{im.height}, {a["colors"]} colors; exact ASE and Unity PNG')
def row(n,y):return Image.open(E/(n+'.png')).convert('RGBA').crop((0,y,64,y+1)).tobytes()
for n in ['PillarShaft','PillarShaftWorn']:assert row(n,0)==row(n,23)==row('PillarCapital',19)==row('PillarBase',0)
ledge=Image.open(E/'RuinedLedge.png');assert ledge.crop((0,0,1,16)).tobytes()==ledge.crop((63,0,64,16)).tobytes()
for p in data['placements']:
 a=next(a for a in data['assets'] if a['name']==p['name']);assert 0<=p['x']<=360-a['width'] and 0<=p['y']<=240-a['height'],p
native=Image.open(P/'LayerStudy-Native.png').convert('RGBA');actual=Image.open(P/'Unity-Layers-720x1280.png').convert('RGBA')
expected=Image.new('RGBA',actual.size);expected.paste(native.resize((720,480),Image.Resampling.NEAREST),(0,4));area=(216,150,720,400)
assert expected.crop(area).tobytes()==actual.crop(area).tobytes(),'Native pixels / Unity mismatch'
normal=Image.open(P/'Unity-Actors-720x1280.png');tall=Image.open(P/'Unity-Actors-720x1600.png')
assert normal.crop((0,0,720,484)).tobytes()==tall.crop((0,0,720,484)).tobytes(),'Tall upper placement drift'
prior=Image.open(R/'Preview/Stage01/Unity-Actors-720x1280.png').convert('RGBA');before=Image.open(P/'Unity-Before-720x1280.png').convert('RGBA')
assert prior.tobytes()==before.tobytes(),'Before render differs from accepted Stage01'
assert json.loads((Q/'UnityPreview.json').read_text())['status']=='PASS'
print(f'PASS: {len(protected)} production/settings and {len(accepted)} accepted Stage01 files unchanged; {len(data["assets"])} sources round-trip; module joins exact; all placements in canvas; native camera pixels exact; normal/tall upper views exact; {len(colors)} combined colors.')
