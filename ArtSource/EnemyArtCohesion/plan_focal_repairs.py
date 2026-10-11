"""Read-only registration study for restoring the mage's face under spell pixels."""
from pathlib import Path
from PIL import Image
import json,numpy as np
ROOT=Path(__file__).resolve().parents[2];BASE=Path(__file__).resolve().parent
jobs=json.loads((BASE/'jobs.json').read_text())
rows=[]
for j in jobs:
 if j['actor']!='court_mage':continue
 im=Image.open(BASE/'Sheets'/(j['stem']+'.png')).convert('RGBA');w,h=j['size']
 for f,r in enumerate(j['sprites']):
  a=np.array(im.crop((f*w,0,(f+1)*w,h)))
  skin=np.all(a[:,:,:3]==[230,176,138],axis=2)&(a[:,:,3]>0)
  ys,xs=np.where(skin)
  # Face is the highest skin mass; hands sit below it.
  top=int(ys.min());yy,xx=np.where(skin&(np.indices(skin.shape)[0]<top+14))
  rows.append(dict(id=j['stem']+'-'+str(f),states=r['states'],array=a,face=[int(xx.min())-3,top-4,int(xx.max())+4,top+16]))
donors=[r for r in rows if any(s=='Idle' for s,i in r['states'])]
out=[]
for r in rows:
 if not any(s=='Ability' and 5<=i<=10 for s,i in r['states']):continue
 a=r['array'];x,y,X,Y=r['face'];piece=a[y:Y,x:X];valid=(piece[:,:,3]>0)&~((piece[:,:,1]>150)&(piece[:,:,2]>160)&(piece[:,:,0]<140))
 best=None
 for d in donors:
  for dy in range(-8,9):
   for dx in range(-12,13):
    if y+dy<0 or x+dx<0 or Y+dy>d['array'].shape[0] or X+dx>d['array'].shape[1]:continue
    b=d['array'][y+dy:Y+dy,x+dx:X+dx];score=int(np.sum(np.all(piece==b,axis=2)&valid))
    if best is None or score>best['matching']:best=dict(donor=d['id'],dx=dx,dy=dy,matching=score,compared=int(valid.sum()))
 out.append(dict(id=r['id'],states=r['states'],face=r['face'],best=best))
(ROOT/'.utmp/EnemyArtCohesion/focal-registration.json').write_text(json.dumps(out,indent=2))
print(json.dumps(out,indent=2))
