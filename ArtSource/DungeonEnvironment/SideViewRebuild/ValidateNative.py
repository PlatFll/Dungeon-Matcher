"""Read-only native-source audit and derived review renders, never production art."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,struct,zlib,hashlib
K=Path(__file__).resolve().parent if (Path(__file__).resolve().parent/'AssetManifest.json').exists() else Path(r'C:/Users/USER/.codex/worktrees/dungeon-top-hud-pixellab/Dungeon Matcher/ArtSource/DungeonEnvironment/SideViewRebuild')
def decode(p):
 b=p.read_bytes();w,h=struct.unpack_from('<HH',b,8);i=144
 while i<len(b):
  size,kind=struct.unpack_from('<IH',b,i);s=i+6
  if kind==0x2005:
   x,y=struct.unpack_from('<hh',b,s+2);ct=struct.unpack_from('<H',b,s+7)[0];cw,ch=struct.unpack_from('<HH',b,s+16);raw=b[s+20:i+size]
   if ct==2:raw=zlib.decompress(raw)
   im=Image.new('RGBA',(w,h));im.paste(Image.frombytes('RGBA',(cw,ch),raw),(x,y));return im
  i+=size
 raise ValueError(p)
m=json.loads((K/'AssetManifest.json').read_text());images={};records=[]
for a in m['assets']:
 f=K/'Exports'/a['group']/(a['name']+'.png');im=Image.open(f).convert('RGBA');native=decode(K/'Editable'/a['group']/(a['name']+'.aseprite'))
 assert im.size==(a['width'],a['height']) and im.tobytes()==native.tobytes(),a['name']
 alpha=set(im.getchannel('A').get_flattened_data());assert alpha<={0,255},a['name']
 bounds=im.getbbox();pad=a.get('canvasPadding',0)
 if pad:assert bounds[0]>=pad and bounds[1]>=pad and bounds[2]<=im.width-pad and bounds[3]<=im.height-pad,a['name']
 images[a['name']]=im;records.append(dict(a,sha256=hashlib.sha256(f.read_bytes()).hexdigest(),colors=len({p[:3] for p in im.get_flattened_data() if p[3]}),alpha=sorted(alpha),occupied_bounds=bounds))
out=Image.new('RGBA',(360,640),'#101019')
for p in sorted(m['placements'],key=lambda x:x['layer']):
 im=images[p['name']];x,y=p['x'],p['y']
 if p.get('clip'):
  bx,by,bw,bh=p['clip'];right=min(x+im.width,bx+bw);bottom=min(y+im.height,by+bh);left=max(x,bx);top=max(y,by)
  if right<=left or bottom<=top:continue
  im=im.crop((left-x,top-y,right-x,bottom-y));x,y=left,top
 out.alpha_composite(im,(x,y))
out.save(K/'Review/NativeAssembly.png');out.resize((720,1280),Image.Resampling.NEAREST).save(K/'Review/Assembly2x.png')
board=Image.new('RGB',(752,690),'#151522');d=ImageDraw.Draw(board)
for x,n,im in [(8,'Reference (native sampling)',Image.open(K/'References/NativeReference.png')),(384,'Current modular assembly',out)]:d.text((x,8),n,fill='white');board.paste(im,(x,30))
board.save(K/'Review/ReferenceComparison.png')
before=K/'Evidence/WallCorrection/Before.png'
if before.exists():
 b=Image.new('RGB',(752,690),'#151522');d=ImageDraw.Draw(b)
 for x,n,im in [(8,'Before: clipped flags and patched wall',Image.open(before)),(384,'Corrected: complete contours and continuous wall',out)]:d.text((x,8),n,fill='white');b.paste(im,(x,30))
 b.save(K/'Review/CorrectionComparison.png')
groups={}
for a in records:groups.setdefault(a['group'],[]).append(a)
for group,items in groups.items():
 positions=[];x=8;y=24;rh=0
 for a in items:
  im=images[a['name']];w=max(im.width*2+16,160);h=im.height*2+38
  if x+w>1200:x=8;y+=rh;rh=0
  positions.append((a,x,y));x+=w;rh=max(rh,h)
 b=Image.new('RGB',(1200,y+rh+8),'#20202b');d=ImageDraw.Draw(b)
 for a,x,y in positions:
  im=images[a['name']];d.text((x,y),a['name'],fill='white');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);b.paste(im,(x,y+22),im)
 b.save(K/'Review'/f'{group}.png')
(K/'Evidence/NativeChecks.json').write_text(json.dumps({'native_cel_png_exact':len(records),'binary_alpha':True,'canvas':[360,640],'placements':len(m['placements']),'assets':records},indent=2)+'\n')
print(json.dumps({'assets':len(records),'placements':len(m['placements']),'native_alpha_export':'passed'}))
