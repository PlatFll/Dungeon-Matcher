from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,struct,zlib,itertools,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/Stage03';V=R/'Previews/Stage03'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return Image.open(p).convert('RGBA')
def pixels(im):return list(im.get_flattened_data())
def ase(p):
 b=p.read_bytes();w,h,dep=struct.unpack_from('<HHH',b,8);assert dep==32;out=Image.new('RGBA',(w,h));size=struct.unpack_from('<I',b,128)[0];pos=144
 while pos<128+size:
  n,typ=struct.unpack_from('<IH',b,pos)
  if typ==0x2005:
   layer,x,y,op,ct=struct.unpack_from('<HhhBH',b,pos+6);assert layer==0 and op==255 and ct in (0,2)
   cw,ch=struct.unpack_from('<HH',b,pos+22);buf=b[pos+26:pos+n];buf=zlib.decompress(buf) if ct==2 else buf;out.paste(Image.frombytes('RGBA',(cw,ch),buf),(x,y))
  pos+=n
 return out
files=sorted((R/'Exports/Stage03').glob('*.png'));ims={f.stem:load(f) for f in files};rows=[]
assert len(files)==5
for f in files:
 im=ims[f.stem];native=R/'Native/Stage03'/f'{f.stem}.aseprite';cols=sorted(set(pixels(im)));alpha=sorted(set(c[3] for c in cols));opaque=sorted(set(c[:3] for c in cols if c[3]))
 assert im.size==((30 if f.stem=='Floor_Right' else 32),12)
 assert ase(native).tobytes()==im.tobytes();assert set(alpha).issubset({0,255});assert sha(f)==sha(P/'Assets/_Game/Art/DungeonPixelLab/FloorTop'/f.name)
 assert all(im.getpixel((x,2))[3]==255 for x in range(im.width)),(f.stem,'discontinuous contact plane')
 rows.append(dict(name=f.stem,path=f.relative_to(P).as_posix(),native_source=native.relative_to(P).as_posix(),dimensions=list(im.size),opaque_colors=len(opaque),palette=['#%02X%02X%02X'%c for c in opaque],alpha=alpha,bounds=list(im.getbbox()),sha256=sha(f),ase_sha256=sha(native),native_cel_equals_png=True,status='CANDIDATE_PENDING_USER'))
mid=['Floor_Mid_A','Floor_Mid_B','Floor_Mid_C'];pairs=list(itertools.product(mid,repeat=2))+[('Floor_Left',n) for n in mid]+[(n,'Floor_Right') for n in mid]+[('Floor_Left','Floor_Right')]
for a,b in pairs:assert ims[a].crop((ims[a].width-1,0,ims[a].width,12)).tobytes()==ims[b].crop((0,0,1,12)).tobytes(),(a,b)
assert len({ims[n].tobytes() for n in mid})==3
protected=json.loads((D/'ProtectedArtHashes.json').read_text());assert all(sha(P/f)==h for f,h in protected.items())
ref=json.loads((R/'Progress/Stage03_Reference_Audit.json').read_text());assert sha(P/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt')==ref['guide_sha256'];assert sha(P/'Docs/ArtDirection/Golden_Reference_Register.md')==ref['register_sha256']
for r in ref['references']:assert sha(P/r['path'])==r['sha256']
manifest=json.loads((R/'Progress/Stage03FloorPlacement.json').read_text());prior=json.loads((R/'Progress/Stage02RecessPlacement.json').read_text());assert [p for p in manifest['placements'] if not p['name'].startswith('Floor_')]==prior['placements']
expected=Image.new('RGBA',(360,640));floor=Image.new('RGBA',(254,12));cover=Image.new('L',(254,12));cd=ImageDraw.Draw(cover)
for p in sorted(manifest['placements'],key=lambda p:p['layer']):
 n=p['name'];x,y=p['x'],p['y']
 if n.startswith('Wall_'):im=load(R/f'Exports/Stage01/{n}.png')
 elif n.startswith('Recess_'):im=load(R/f'Exports/Stage02/{n}.png')
 elif n.startswith('Player'):im=load(P/f'Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Frames_Player/{n}.png')
 elif n.startswith('Floor_'):
  im=ims[n];assert y==178 and x>=106 and x+im.width<=360
  assert not cover.crop((x-106,0,x-106+im.width,12)).getbbox();cd.rectangle((x-106,0,x-106+im.width-1,11),fill=255);floor.alpha_composite(im,(x-106,0))
 else:raise AssertionError(n)
 assert x>=0 and y>=0 and x+im.width<=360 and y+im.height<=640
 expected.alpha_composite(im,(x,y))
assert cover.getextrema()==(255,255)
marked=expected.copy();dr=ImageDraw.Draw(marked)
for x in [154,234,314]:dr.rectangle((x,164,x,179),fill=(0,255,255,255));dr.rectangle((x-3,179,x+3,179),fill=(0,255,255,255))
screens=[]
for scale in [1,3]:
 for label,ex in [('Normal',expected),('Contact',marked),('Restored',expected)]:
  shot=load(V/f'UnityFloor-{label}-{scale}x.png');pred=ex.resize((360*scale,640*scale),Image.Resampling.NEAREST);assert shot.size==pred.size
  diff=sum(a!=b for a,b in zip(pixels(shot),pixels(pred)));assert diff==0,(label,scale,diff)
  screens.append(dict(label=label,scale=scale,dimensions=list(shot.size),rgba_differences=diff))
test=ET.parse(D/'UnityTests.xml').getroot();assert test.attrib['result']=='Passed' and int(test.attrib['failed'])==0
checks=dict(assets=rows,legal_pairs=[dict(left=a,right=b,exact_profile_match=True) for a,b in pairs],same_floor_contact_row=180,placed_modules=8,floor_native_rect=[106,178,254,12],gaps=0,overlaps=0,clipped_sprite_bounds=0,protected_art_hashes=len(protected),prior_placements_preserved=True,reference_hashes_unchanged=True,unity_test=dict(result=test.attrib['result'],passed=test.attrib['passed'],failed=test.attrib['failed']),screens=screens,native_resampling='none',production_scene_and_build_settings='unchanged; asserted in Unity test')
(D/'PixelChecks.json').write_text(json.dumps(checks,indent=2)+'\n')
floor.save(V/'FloorStrip_1x.png');floor.resize((1016,48),Image.Resampling.NEAREST).save(V/'FloorStrip_4x.png')
grid=Image.new('RGBA',(280,len(pairs)*20+24),(10,13,17,255));dr=ImageDraw.Draw(grid);dr.text((5,4),'All legal joints / no gaps',fill='#e4e2ed')
for i,(a,b) in enumerate(pairs):
 y=24+i*20;dr.text((5,y),a.replace('Floor_','')+' > '+b.replace('Floor_',''),fill='#e4e2ed');grid.alpha_composite(ims[a],(190,y));grid.alpha_composite(ims[b],(190+ims[a].width,y))
grid.save(V/'LegalPairs_1x.png');grid.resize((840,grid.height*3),Image.Resampling.NEAREST).save(V/'LegalPairs_3x.png')
for name in ['Normal','Contact']:
 crop=load(V/f'UnityFloor-{name}-1x.png').crop((0,12,360,220));crop.save(V/f'Unity_Top_{name}_1x.png');crop.resize((1080,624),Image.Resampling.NEAREST).save(V/f'Unity_Top_{name}_3x.png')
print(json.dumps(dict(assets=len(rows),palette_counts=[r['opaque_colors'] for r in rows],legal_pairs=len(pairs),screens=screens,protected_art=len(protected)),indent=2))
