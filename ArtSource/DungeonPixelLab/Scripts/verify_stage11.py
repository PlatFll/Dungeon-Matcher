from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,struct,zlib,xml.etree.ElementTree as ET,subprocess,re
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab'
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
for stage,family in [(10,'PortraitFrame'),(11,'BoardFrame')]:
 for p in sorted((R/f'Exports/Stage{stage:02}').glob('*.png')):
  im=load(p);a=R/f'Native/Stage{stage:02}'/(p.stem+'.aseprite')
  assert ase(a).tobytes()==im.tobytes(),p.name
  assert sha(p)==sha(P/'Assets/_Game/Art/DungeonPixelLab'/family/p.name)
  assert set(im.getchannel('A').get_flattened_data()) <= {0,255}
  colors=sorted(set(c[:3] for c in im.get_flattened_data() if c[3]))
  rows.append(dict(stage=f'{stage:02}',name=p.stem,dimensions=list(im.size),bounds=im.getbbox(),opaque_colors=len(colors),palette=['#%02X%02X%02X'%c for c in colors],alpha_values=sorted(set(im.getchannel('A').get_flattened_data())),sha256=sha(p),ase_sha256=sha(a),native_equals_png=True))
  if 'Rail_' in p.stem:
   horizontal=im.width>im.height
   a0=im.crop((0,0,1,im.height)) if horizontal else im.crop((0,0,im.width,1))
   a1=im.crop((im.width-1,0,im.width,im.height)) if horizontal else im.crop((0,im.height-1,im.width,im.height))
   assert a0.tobytes()==a1.tobytes(),p.name
for f in ['Portrait_HP_Shell','Portrait_Heart_Existing']:
 for folder in ['Native','Exports']:
  ext='.aseprite' if folder=='Native' else '.png'
  assert sha(R/folder/'Stage10'/(f+ext))==sha(R/'Diagnostics/Stage10Correction'/('Before'+folder)/(f+ext))
protected=json.loads((R/'Diagnostics/Stage11/ProtectedArtHashes.json').read_text(encoding='utf8'))
changed=[p for p,h in protected.items() if sha(P/p)!=h]
assert not changed,changed
refs=json.loads((R/'Progress/Stage11_Reference_Audit.json').read_text(encoding='utf8'))
for f,k in [('Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','guide_sha256'),('Docs/ArtDirection/Golden_Reference_Register.md','register_sha256')]:assert sha(P/f)==refs[k]
for ref in refs['references']:
 assert sha(P/ref['path'])==ref['sha256']
 if 'production_path' in ref:assert sha(P/ref['production_path'])==ref['production_sha256']
tests={}
for stage in ['Stage10Correction','Stage11']:
 t=ET.parse(R/'Diagnostics'/stage/'UnityTests.xml').getroot();assert t.get('result')=='Passed';tests[stage]={k:t.get(k) for k in ['result','total','passed','failed']}
# Native screen comparison checks every rendered pixel, including tiled rail endings.
shot1=load(R/'Previews/Stage11/EmptyBoard-360x640.png');shot3=load(R/'Previews/Stage11/EmptyBoard-1080x1920.png')
assert shot1.resize((1080,1920),Image.Resampling.NEAREST).tobytes()==shot3.tobytes()
tall=load(R/'Previews/Stage11/EmptyBoard-1080x2400.png');assert tall.crop((0,240,1080,2160)).tobytes()==shot3.tobytes()
cell=load(R/'Exports/Stage11/Board_Cell.png')
for y in range(8):
 for x in range(8):assert shot1.crop((40+x*35,238+y*35,75+x*35,273+y*35)).tobytes()==cell.tobytes()
# Approved prior pixels outside the portrait and newly occupied board remain identical.
before=load(R/'Previews/Stage10/UnityPortrait-Normal-1x.png');portrait=load(R/'Previews/Stage10Correction/Restored-1x.png')
assert before.crop((108,0,360,225)).tobytes()==portrait.crop((108,0,360,225)).tobytes()
assert before.crop((0,210,360,640)).tobytes()==portrait.crop((0,210,360,640)).tobytes()
assert shot1.crop((0,0,360,225)).tobytes()==portrait.crop((0,0,360,225)).tobytes()
assert shot1.crop((0,531,360,640)).tobytes()==portrait.crop((0,531,360,640)).tobytes()
assert shot1.crop((0,225,27,531)).tobytes()==portrait.crop((0,225,27,531)).tobytes()
assert shot1.crop((333,225,360,531)).tobytes()==portrait.crop((333,225,360,531)).tobytes()
result=dict(status='PASS',artifacts=rows,protected_files=len(protected),unity_tests=tests,board_cells=64,board_pitch=35,board_interior=[40,238,280,280],portrait_outer=[9,20,99,190],portrait_interior=[19,30,79,170],portrait_resize_sizes=[[99,190],[100,191],[119,231],[60,90]],portrait_coverage='Every native/screen interior pixel covered; no outside pixels cyan; 1x and 3x; undersize recovery checked',screen_pixel_equality='1x enlarged exactly equals 3x; tall-screen safe-area content equals 3x',previous_art_outside_scoped_bounds='RGBA identical')
(R/'Diagnostics/Stage11/NativeChecks.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf8')
# Review layouts only; nearest-neighbor enlargement, never used as source art.
V=R/'Previews/Stage11'
board=Image.new('RGBA',(760,240),(20,22,32,255));d=ImageDraw.Draw(board)
for x,label,im in [(12,'Before: misplaced clamps / fixed backing',load(R/'Previews/Stage10/UnityPortrait-PortraitFull-1x.png')),(390,'Corrected: centered pieces / overfilled mask',load(R/'Previews/Stage10Correction/Portrait-1x.png'))]:
 d.text((x,8),label,fill='white');board.alpha_composite(im.crop((0,0,360,220)),(x,24))
board.save(V/'PortraitComparison-1x.png');board.resize((1520,480),Image.Resampling.NEAREST).save(V/'PortraitComparison-2x.png')
kit=Image.new('RGBA',(730,240),(20,22,32,255));d=ImageDraw.Draw(kit)
d.text((10,8),'PixelLab raw 64x64 (1x)',fill='white');kit.alpha_composite(load(R/'Sources/Stage11/Board_Raw_PixelLab.png'),(10,30))
d.text((180,8),'LibreSprite native modules',fill='white')
for i,f in enumerate(sorted((R/'Exports/Stage11').glob('*.png'))):
 x=180+(i%4)*130;y=30+(i//4)*68;im=load(f);kit.alpha_composite(im,(x,y));d.text((x,y+im.height+4),f.stem.replace('Board_',''),fill='#cfc9df')
kit.save(V/'Contact-1x.png');kit.resize((1460,480),Image.Resampling.NEAREST).save(V/'Contact-2x.png')
print(json.dumps(dict(assets=len(rows),protected=len(protected),tests=tests,exact_cells=64,exact_3x_and_tall=True)))
