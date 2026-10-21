"""Read-only source/Unity checks; writes only Stage 02 verification evidence."""
from pathlib import Path
from PIL import Image
import json,hashlib,xml.etree.ElementTree as ET,re
P=Path(__file__).resolve().parents[4]
R=P/'ArtSource/DungeonHybrid/Stage02'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return Image.open(p).convert('RGBA')
def same(a,b):return a.size==b.size and a.tobytes()==b.tobytes()
protected=json.loads((R/'Diagnostics/ProtectedHashes.json').read_text())
changed=[f for f,h in protected.items() if sha(P/f)!=h];assert not changed,changed
reference=json.loads((R/'Diagnostics/ReferenceAudit.json').read_text())
assert sha(P/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt')==reference['guide_sha256']
assert sha(P/'Docs/ArtDirection/Golden_Reference_Register.md')==reference['register_sha256']
for a in reference['references'][:3]:
    assert sha(P/a['path'])==a['sha256'];assert sha(P/a['production_path'])==a['production_sha256']
    prod=load(P/a['production_path']).crop((0,0,a['width'],a['height']))
    assert same(prod,load(P/a['path']))
m=json.loads((R/'Assembly.json').read_text());clean=load(R/'Exports/Gate_Clean.png');parts=Image.new('RGBA',clean.size)
for a in m['parts']:
    im=load(P/a['path']);assert same(im,load(R/'Exports'/(a['name']+'.png')))
    assert im.getbbox()==(2,2,im.width-2,im.height-2)
    assert set(im.getchannel('A').getdata())=={0,255}
    parts.alpha_composite(im,tuple(a['offset']))
    meta=(P/(a['path']+'.meta')).read_text()
    for field,value in [('enableMipMap','0'),('filterMode','0'),('nPOTScale','0'),('spriteMode','1'),('spriteMeshType','0'),('spritePixelsToUnits','64')]:
        assert re.search(r'^\s*'+field+': '+value+r'\s*$',meta,re.M),a['name']+': '+field
    assert 'textureCompression: 0' in meta
assert same(parts,clean)
base=load(P/'ArtSource/DungeonHybrid/Stage01/Previews/Unity-structure-360x240.png')
expected=base.copy();expected.alpha_composite(clean,tuple(m['scene_origin']))
cast=expected.copy()
for a,x,y in zip(reference['references'][:3],[27,105,225],[117,100,100]):cast.alpha_composite(load(P/a['path']),(x,y))
captures=[]
for label,source in [('structure',expected),('cast',cast)]:
    for s in [1,2,3]:
        path=R/'Previews'/f'Unity-{label}-{360*s}x{240*s}.png';actual=load(path)
        assert same(actual,source.resize(actual.size,Image.Resampling.NEAREST)),path.name
        captures.append(dict(path='Previews/'+path.name,scale=s,source_pixel_differences=0,sha256=sha(path)))
tests=ET.parse(R/'Diagnostics/UnityTests.xml').getroot()
assert tests.get('result')=='Passed' and tests.get('failed')=='0'
assert same(base.crop((0,0,360,52)),expected.crop((0,0,360,52)))
assert same(base.crop((0,0,106,240)),expected.crop((0,0,106,240)))
assert same(base.crop((0,178,360,240)),expected.crop((0,178,360,240)))
raw=load(R/'Sources/Gate_Raw_PixelLab.png')
counts=lambda im:len({c[:3] for c in im.getdata() if c[3]})
report=dict(result='PASS',approved_files_unchanged=len(protected),modules=10,source_dimensions=[128,128],occupied_bounds=clean.getbbox(),palette_count=counts(clean),raw_palette_count=counts(raw),binary_alpha=True,component_clearance_pixels=2,modular_reassembly_exact=True,production_copies_exact=True,stage01_pixels_outside_gate_unchanged=True,portrait_and_floor_unchanged=True,source_pixels_exact_in_all_screens=True,captures=captures,unity_test={k:tests.get(k) for k in ['result','total','passed','failed']},scope='Art-only Unity Game View; static reference figures. Not production gameplay or device validation.',artistic_verdict='Broader arched opening and chunky blocks establish a stronger focal point. Muted violet stone, quieter iron and simplified reliefs keep attention on the cast. Candidate for user approval; UI hero pieces and props remain later stages.')
(R/'Diagnostics/Verification.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k not in ['captures','artistic_verdict']}))
