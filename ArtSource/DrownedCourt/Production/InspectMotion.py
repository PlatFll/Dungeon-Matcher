from pathlib import Path
from PIL import Image,ImageDraw
r=Path(__file__).resolve().parent
for path in (r/'MotionRaw').iterdir():
 fs=sorted(path.glob('*.png'));sz=Image.open(fs[0]).size;scale=2;cw=sz[0]*scale+8;ch=sz[1]*scale+22;canvas=Image.new('RGB',(9*cw,((len(fs)+8)//9)*ch),(96,96,96));d=ImageDraw.Draw(canvas)
 for i,p in enumerate(fs):
  im=Image.open(p).convert('RGBA');v=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST);x=(i%9)*cw;y=(i//9)*ch;canvas.paste(v,(x,y),v);d.text((x,y+sz[1]*scale+2),str(i),fill='white')
 canvas.save(r/(path.name+'_inspection.png'))
