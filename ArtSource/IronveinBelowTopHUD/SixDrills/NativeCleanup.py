from pathlib import Path
from PIL import Image,ImageDraw
import struct,zlib,subprocess,json,hashlib,shutil
O=Path(__file__).parent;N=O/'Native';E=O/'Exports';N.mkdir(exist_ok=True);E.mkdir(exist_ok=True)
base=Image.open(O/'RawLoop/00.png').convert('RGBA')
palette=sorted({p[:3] for p in base.getdata() if p[3]})
def tip(im):
 pts=[(x,y) for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))[3]]
 x=max(p[0] for p in pts);ys=[p[1] for p in pts if p[0]==x];return (x,round(sum(ys)/len(ys)))
tx,ty=tip(base);frames=[]
for i in range(8):
 raw=Image.open(O/f'RawLoop/{i:02}.png').convert('RGBA');rx,ry=tip(raw)
 aligned=Image.new('RGBA',base.size);aligned.paste(raw,(tx-rx,ty-ry))
 out=base.copy()
 for y in range(base.height):
  for x in range(base.width):
   if not base.getpixel((x,y))[3]:continue
   # Keep a rigid outline, fixed collar and fixed point. Retain PixelLab's
   # rotating interior ridges; clean their accidental near-duplicate shades.
   boundary=any(not (0<=x+dx<base.width and 0<=y+dy<base.height) or not base.getpixel((x+dx,y+dy))[3] for dx,dy in [(0,1),(0,-1),(1,0),(-1,0)])
   p=aligned.getpixel((x,y))
   if i and x>11 and x<32 and not boundary and p[3]:
    c=min(palette,key=lambda q:sum((q[n]-p[n])**2 for n in range(3)))
    out.putpixel((x,y),(*c,255))
 # Center the spinning axis on the canvas, using whole native pixels only.
 centered=Image.new('RGBA',(40,32));centered.paste(out,(-1,16-ty));frames.append(centered)
def chunk(kind,payload):return struct.pack('<IH',len(payload)+6,kind)+payload
def ase(path,ims):
 name=b'Native spinning drill';layer=chunk(0x2004,struct.pack('<HHHHHHB3xH',1,0,0,0,0,0,255,len(name))+name)
 fs=[]
 for i,im in enumerate(ims):
  cel=chunk(0x2005,struct.pack('<HhhBH7xHH',0,0,0,255,2,im.width,im.height)+zlib.compress(im.tobytes()))
  payload=(layer if i==0 else b'')+cel
  fs.append(struct.pack('<IHHHHI',16+len(payload),0xF1FA,2 if i==0 else 1,60,0,0)+payload)
 header=bytearray(128);struct.pack_into('<IHHHHHI',header,0,128+sum(map(len,fs)),0xA5E0,len(ims),ims[0].width,ims[0].height,32,1);struct.pack_into('<H',header,18,60);header[32]=0;header[34:36]=struct.pack('<H',0);header[36]=header[37]=1
 path.write_bytes(header+b''.join(fs))
ase(N/'RowDrillSpin.ase',frames)
exe=r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
subprocess.run([exe,'--batch',str(N/'RowDrillSpin.ase'),'--sheet',str(E/'RowDrillSpin.png'),'--sheet-type','horizontal','--data',str(N/'RowDrillSpin.json'),'--format','json-array'],check=True,capture_output=True)
export=Image.open(E/'RowDrillSpin.png').convert('RGBA')
assert export.size==(320,32),export.size
for i,im in enumerate(frames):
 got=export.crop((i*40,0,(i+1)*40,32));assert got.tobytes()==im.tobytes()
 got.save(E/f'RowDrillSpin{i:02}.png')
socket=Image.new('RGBA',(12,16));d=ImageDraw.Draw(socket);d.ellipse((1,0,10,15),fill='#0A0D11');d.ellipse((2,1,9,14),fill='#343442');d.ellipse((3,3,9,12),fill='#0A0D11');d.line((3,2,7,2),fill='#696676');ase(N/'EmptySocket.ase',[socket]);subprocess.run([exe,'--batch',str(N/'EmptySocket.ase'),'--save-as',str(E/'EmptySocket.png')],check=True,capture_output=True)
frames[0].save(E/'DrillStill.png');frames[0].save(O/'Spin.gif',save_all=True,append_images=frames[1:],duration=60,loop=0,disposal=2)
export.resize((1280,128),Image.Resampling.NEAREST).save(O/'CleanSpin4x.png')
manifest={'canvas':[40,32],'frames':8,'duration_ms':60,'opaque_colors':len(palette),'alpha':[0,255],'source':'PixelLab c45807dc-bf5f-486e-958d-e9bd664c587f; first attempt retained in Raw','cleanup':'Whole-pixel tip registration, locked outline/collar/point, twelve-color material ramp, native padding. Rotating interior clusters remain PixelLab-generated. LibreSprite ASE reopened and PNG strip compared byte-exact in RGBA.','bounds':[im.getbbox() for im in frames],'distinct_frames':len({im.tobytes() for im in frames}),'generations':12,'files':{str(p.relative_to(O)):hashlib.sha256(p.read_bytes()).hexdigest() for p in E.glob('*.png')}}
assert manifest['distinct_frames']>=6
(O/'ArtQA.json').write_text(json.dumps(manifest,indent=2))
print(json.dumps(manifest))
