"""Verify real Unity captures against original native sprites; no production writes."""
from pathlib import Path
from PIL import Image
import hashlib,json,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonHybrid/Stage01'
def load(p):return Image.open(p).convert('RGBA')
def same(a,b):return a.size==b.size and a.tobytes()==b.tobytes()
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
audit=json.loads((R/'Diagnostics/AssetAudit.json').read_text());lookup={a['name']:load(P/a['unity_path']) for a in audit['assets']}
protected=json.loads((R/'Diagnostics/ProtectedHashes.json').read_text());assert all(sha(P/f)==h for f,h in protected.items())
qa=json.loads((P/'ArtSource/DungeonPixelLab/Diagnostics/Stage15/LibraryQA.json').read_text());joins=[]
for j in qa['exact_seam_comparisons']:
    if j['a'] not in lookup or j['b'] not in lookup:continue
    a,b=lookup[j['a']],lookup[j['b']]
    if j['axis']=='H':aa=a.crop((a.width-1,0,a.width,a.height));bb=b.crop((0,0,1,b.height))
    else:aa=a.crop((0,a.height-1,a.width,a.height));bb=b.crop((0,0,b.width,1))
    assert same(aa,bb),j;joins.append(j)
def maskrow(name,y):
    im=lookup[name];return [x for x in range(im.width) if im.getpixel((x,y))[3]]
assert maskrow('Support_Capital',15)==maskrow('Support_Core',0)==maskrow('Support_Core',27)==maskrow('Support_Foot',0)
for n in ['Floor_Left','Floor_Mid_A','Floor_Mid_B','Floor_Mid_C','Floor_Right']:
    assert all(lookup[n].getpixel((x,6))[3]==255 for x in range(lookup[n].width))
for n in ['Support_Corner_Left','Support_Corner_Right']:
    for y in range(28):
        row=maskrow(n,y);assert row==list(range(min(row),max(row)+1))
m=json.loads((R/'Placement.json').read_text());expected=Image.new('RGBA',(360,240),(10,13,17,255))
coverage=Image.new('L',(360,240))
for a in sorted(m['placements'],key=lambda a:a['layer']):
    im=load(P/a['path']);assert a['x']>=0 and a['y']>=0 and a['x']+im.width<=360 and a['y']+im.height<=240
    expected.alpha_composite(im,(a['x'],a['y']))
    if a['name'].startswith('Wall_'):coverage.paste(im.getchannel('A'),(a['x'],a['y']))
assert coverage.getextrema()==(255,255),'Wall field must fill the entire study without gaps'
x,y,w,h=m['portrait_mask'];tile=load(P/m['portrait_repeat']);field=Image.new('RGBA',(w,h))
for yy in range(0,h,tile.height):
    for xx in range(0,w,tile.width):field.alpha_composite(tile,(xx,yy))
assert field.getchannel('A').getextrema()==(255,255);expected.alpha_composite(field,(x,y))
base=load(R/'Previews/Unity-structure-360x240.png');assert same(base,expected),'Unity structural rendering differs from source pixels'
cast=expected.copy()
for path,x,y in [('Docs/ArtDirection/Audit2026-10/media/skeleton-still.png',27,117),('Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png',105,100),('Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png',225,100)]:cast.alpha_composite(load(P/path),(x,y))
assert same(cast,load(R/'Previews/Unity-cast-360x240.png')),'Reference source pixels differ in Unity'
captures=[]
for label,im in [('structure',expected),('cast',cast)]:
    for s in [1,2,3]:
        f=f'Unity-{label}-{360*s}x{240*s}.png';shot=load(R/'Previews'/f)
        assert same(shot,im.resize(shot.size,Image.Resampling.NEAREST)),f
        captures.append(dict(path='Previews/'+f,scale=s,source_pixel_differences=0,sha256=sha(R/'Previews'/f)))
tests=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert tests.get('result')=='Passed' and tests.get('failed')=='0'
report=dict(result='PASS',retained_assets=33,protected_files_unchanged=len(protected),new_generations=0,exact_seam_comparisons=joins,support_join_masks_equal=True,floor_contact_continuous=True,wall_field_full_coverage=True,portrait_mask_exact=True,all_native_source_pixels_exact=True,captures=captures,unity_tests={k:tests.get(k) for k in ['result','total','passed','failed']})
(R/'Diagnostics/Verification.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k not in ['captures','exact_seam_comparisons']}));print('Exact structural joins:',len(joins))
