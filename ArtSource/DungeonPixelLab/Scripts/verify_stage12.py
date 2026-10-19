from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,struct,zlib,xml.etree.ElementTree as ET,sys
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/Stage12';V=R/'Previews/Stage12'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return Image.open(p).convert('RGBA')
def ase(p):
 b=p.read_bytes();w,h,dep=struct.unpack_from('<HHH',b,8);assert dep==32;out=Image.new('RGBA',(w,h));size=struct.unpack_from('<I',b,128)[0];pos=144
 while pos<128+size:
  n,typ=struct.unpack_from('<IH',b,pos)
  if typ==0x2005:
   layer,x,y,op,ct=struct.unpack_from('<HhhBH',b,pos+6);assert layer==0 and op==255 and ct in (0,2)
   cw,ch=struct.unpack_from('<HH',b,pos+22);buf=b[pos+26:pos+n];buf=zlib.decompress(buf) if ct==2 else buf;out.paste(Image.frombytes('RGBA',(cw,ch),buf),(x,y))
  pos+=n
 return out
rows=[]
for f in sorted((R/'Exports/Stage12').glob('*.png')):
 im=load(f);a=R/'Native/Stage12'/(f.stem+'.aseprite');assert ase(a).tobytes()==im.tobytes(),f.name
 assert sha(f)==sha(P/'Assets/_Game/Art/DungeonPixelLab/TopPlaques'/f.name)
 alpha=sorted(set(im.getchannel('A').get_flattened_data()));assert set(alpha)<={0,255}
 colors=sorted(set(c[:3] for c in im.get_flattened_data() if c[3]))
 if 'Rail' in f.stem:
  horizontal=im.width>im.height
  first=im.crop((0,0,1,im.height)) if horizontal else im.crop((0,0,im.width,1))
  last=im.crop((im.width-1,0,im.width,im.height)) if horizontal else im.crop((0,im.height-1,im.width,im.height))
  assert first.tobytes()==last.tobytes()
 rows.append(dict(stage='12',name=f.stem,dimensions=list(im.size),bounds=im.getbbox(),opaque_colors=len(colors),palette=['#%02X%02X%02X'%c for c in colors],alpha_values=alpha,sha256=sha(f),ase_sha256=sha(a),native_equals_png=True))
assert len(rows)==11
protected=json.loads((D/'ApprovedPriorHashes.json').read_text(encoding='utf8'));changed=[p for p,h in protected.items() if sha(P/p)!=h];assert not changed,changed
refs=json.loads((R/'Progress/Stage12_Reference_Audit.json').read_text(encoding='utf8'))
for f,k in [('Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','guide_sha256'),('Docs/ArtDirection/Golden_Reference_Register.md','register_sha256')]:assert sha(P/f)==refs[k]
for ref in refs['references']:
 assert sha(P/ref['path'])==ref['sha256']
 if 'production_path' in ref:assert sha(P/ref['production_path'])==ref['production_sha256']
contact=Image.new('RGBA',(660,240),(20,22,32,255));draw=ImageDraw.Draw(contact)
draw.text((10,8),'Raw PixelLab source (native 1x)',fill='white');contact.alpha_composite(load(R/'Sources/Stage12/Plaque_Raw_PixelLab.png'),(10,32))
draw.text((220,8),'LibreSprite native modular components (1x)',fill='white')
for i,row in enumerate(rows):
 x=220+(i%3)*145;y=32+(i//3)*47;im=load(R/'Exports/Stage12'/(row['name']+'.png'));contact.alpha_composite(im,(x,y));draw.text((x,y+18),row['name'].replace('Plaque_',''),fill='#d5cadf')
contact.save(V/'Contact-1x.png');contact.resize((1320,480),Image.Resampling.NEAREST).save(V/'Contact-2x.png')
raw=load(R/'Sources/Stage12/Plaque_Raw_PixelLab.png')
result=dict(status='NATIVE_CHECKS_PASS',assets=rows,protected_files=len(protected),source_sha256=sha(R/'Sources/Stage12/Plaque_Raw_PixelLab.png'),source_dimensions=list(raw.size),source_opaque_colors=len(set(c[:3] for c in raw.get_flattened_data() if c[3])),source_usage='Corners use selected native rows/columns with centered rivet and deliberate border cleanup. Rails use source lower-rail profile with declared highlight/shadow material changes. Socket follows source violet motif; gear separately authored in LibreSprite. No source scaling.')
if '--native-only' not in sys.argv:
 t=ET.parse(D/'UnityTests.xml').getroot();assert t.get('result')=='Passed';result['unity_tests']={k:t.get(k) for k in ['result','total','passed','failed']}
 one=load(V/'Plaques-360x640.png');two=load(V/'Plaques-720x1280.png');three=load(V/'Plaques-1080x1920.png');tall=load(V/'Plaques-1080x2400.png')
 assert one.resize(two.size,Image.Resampling.NEAREST).tobytes()==two.tobytes(),'2x mismatch'
 assert one.resize(three.size,Image.Resampling.NEAREST).tobytes()==three.tobytes(),'3x mismatch'
 assert tall.crop((0,240,1080,2160)).tobytes()==three.tobytes(),'Tall safe area mismatch'
 before=load(R/'Previews/PortraitPipeCorrection/Restored-1x.png');bounds=[(130,16,233,52),(253,23,313,53),(319,23,350,54)];unchanged=0
 for y in range(640):
  for x in range(360):
   if not any(l<=x<r and top<=y<b for l,top,r,b in bounds):assert one.getpixel((x,y))==before.getpixel((x,y)),(x,y);unchanged+=1
 for n in [1,3]:one.crop((115,0,360,75)).resize((245*n,75*n),Image.Resampling.NEAREST).save(V/f'TopHud-{n}x.png')
 result.update(status='PASS',exact_2x_and_3x=True,tall_content_equals_3x=True,all_prior_pixels_outside_plaque_bounds_identical=True,unchanged_screen_pixels=unchanged,plaque_bounds=bounds)
(D/'NativeChecks.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf8');print(json.dumps({k:v for k,v in result.items() if k!='assets'}))
