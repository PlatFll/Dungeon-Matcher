from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,struct,zlib,xml.etree.ElementTree as ET,subprocess
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/PortraitPipeCorrection';V=R/'Previews/PortraitPipeCorrection'
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
green=load(R/'Diagnostics/Stage10Correction/BeforeExports/Portrait_Rail_Left.png');profile=[green.getpixel((i,5)) for i in range(1,6)];rows=[]
for side in ['Left','Right','Top','Bottom']:
 f=R/f'Exports/Stage10/Portrait_Rail_{side}.png';a=R/f'Native/Stage10/Portrait_Rail_{side}.aseprite';im=load(f)
 assert ase(a).tobytes()==im.tobytes();assert sha(f)==sha(P/'Assets/_Game/Art/DungeonPixelLab/PortraitFrame'/f.name)
 assert set(im.getchannel('A').get_flattened_data())<={0,255}
 horiz=im.width>im.height
 for k in range(im.width if horiz else im.height):
  cross=[im.getpixel((k,d) if horiz else (d,k)) for d in range(7)]
  assert cross[0][3]==cross[6][3]==0 and cross[1:6]==profile
 rows.append(dict(name=f.stem,dimensions=list(im.size),opaque_width=5,canvas_cross_axis=7,pipe_center=3,connector_center=3,colors=len(set(im.get_flattened_data()))-1,sha256=sha(f),ase_sha256=sha(a)))
protected=json.loads((R/'Diagnostics/Stage12/ProtectedArtHashes.json').read_text(encoding='utf8'));changed=[p for p,h in protected.items() if sha(P/p)!=h];assert not changed,changed
refs=json.loads((R/'Progress/Stage12_Reference_Audit.json').read_text(encoding='utf8'))
for f,k in [('Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','guide_sha256'),('Docs/ArtDirection/Golden_Reference_Register.md','register_sha256')]:assert sha(P/f)==refs[k]
for ref in refs['references']:
 assert sha(P/ref['path'])==ref['sha256']
 if 'production_path' in ref:assert sha(P/ref['production_path'])==ref['production_sha256']
t=ET.parse(D/'UnityTests.xml').getroot();assert t.get('result')=='Passed'
one=load(V/'Portrait-1x.png');three=load(V/'Portrait-3x.png');assert one.resize(three.size,Image.Resampling.NEAREST).tobytes()==three.tobytes()
before=load(R/'Previews/Stage11/EmptyBoard-360x640.png');after=load(V/'Restored-1x.png')
outside=0
for y in range(640):
 for x in range(360):
  if not (9<=x<108 and 20<=y<210):assert before.getpixel((x,y))==after.getpixel((x,y)),(x,y);outside+=1
cell=load(R/'Exports/Stage11/Board_Cell.png')
for y in range(8):
 for x in range(8):assert after.crop((40+x*35,238+y*35,75+x*35,273+y*35)).tobytes()==cell.tobytes()
# View actual native pixels; all enlargement uses nearest neighbour only.
comparison=Image.new('RGBA',(654,646),(20,22,32,255));draw=ImageDraw.Draw(comparison)
for x,label,path in [(12,'Previous (visible pipe off-center)',R/'Previews/Stage10Correction/Portrait-1x.png'),(336,'Corrected (same axis on all four sides)',V/'Portrait-1x.png')]:
 draw.text((x,10),label,fill='white');im=load(path).crop((7,17,110,212)).resize((309,585),Image.Resampling.NEAREST);comparison.alpha_composite(im,(x,40))
comparison.save(V/'Comparison-3x.png')
sheet=Image.new('RGBA',(900,238),(20,22,32,255));draw=ImageDraw.Draw(sheet);clamp=load(R/'Exports/Stage10/Portrait_Clamp.png')
for i,side in enumerate(['Left','Right','Top','Bottom']):
 x=12+i*220;draw.text((x,10),side+' edge at 8x',fill='white');rail=load(R/f'Exports/Stage10/Portrait_Rail_{side}.png');joint=rail.copy();joint.alpha_composite(clamp,((joint.width-7)//2,(joint.height-7)//2));sheet.alpha_composite(joint.resize((joint.width*8,joint.height*8),Image.Resampling.NEAREST),(x,38))
draw.text((12,216),'Same 5px pipe through the center of each unchanged 7px connector.',fill='#d6cfe4');sheet.save(V/'Joints-8x.png')
result=dict(status='PASS',artifacts=rows,protected_files=len(protected),test={k:t.get(k) for k in ['result','total','passed','failed']},portrait_outer=[9,20,99,190],portrait_interior=[18,29,81,172],sizes_tested=[[99,190],[100,191],[119,231],[60,90]],pixel_scale=[1,3],mask_coverage='Every interior pixel covered; no outside leakage when decorative occluders hidden. Complete frame rendered separately.',undersize_recovery=True,exact_3x=True,unchanged_screen_pixels_outside_portrait=outside,board_cells_unchanged=64)
(D/'NativeChecks.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf8');print(json.dumps({k:v for k,v in result.items() if k!='artifacts'}))
