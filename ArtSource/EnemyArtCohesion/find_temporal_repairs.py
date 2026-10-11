"""Propose exact neighboring-pose continuations. Read-only; artist reviews results."""
from pathlib import Path
import json,hashlib
import numpy as np
from PIL import Image
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'.utmp/EnemyArtCohesion'
jobs=json.loads((ROOT/'ArtSource/EnemyArtCohesion/jobs.json').read_text())
plans=json.loads((OUT/'edge-repair-plan.json').read_text());actors={};lookup={}
for j in jobs:
 im=Image.open(ROOT/j['original']).convert('RGBA')
 for index,row in enumerate(j['sprites']):
  a=np.array(im.crop(row['source']['crop_top_left']));a[a[:,:,3]==0]=0
  k=hashlib.sha256(a.tobytes()).hexdigest();d=dict(job=j,row=row,index=index,a=a)
  actors.setdefault(j['actor'],{}).setdefault(k,d);lookup[j['stem'],index]=d
results=[]
for plan in plans:
 if plan.get('duplicate_of'):continue
 target=lookup[plan['stem'],plan['frame']];a=target['a'];h,w=a.shape[:2]
 for side,info in plan['sides'].items():
  if side=='left':ys=np.where(a[:,0,3]>0)[0];box=[0,max(0,int(ys.min())-2),8,min(h,int(ys.max())+3)]
  elif side=='right':ys=np.where(a[:,-1,3]>0)[0];box=[w-8,max(0,int(ys.min())-2),w,min(h,int(ys.max())+3)]
  else:xs=np.where(a[0,:,3]>0)[0];box=[max(0,int(xs.min())-2),0,min(w,int(xs.max())+3),8]
  x0,y0,x1,y1=box;patch=a[y0:y1,x0:x1];ph,pw=patch.shape[:2]
  opaque=np.argwhere(patch[:,:,3]>0)
  if len(opaque)<10:continue
  # Deterministic sample rejects unrelated positions before full comparisons.
  sample=opaque[np.linspace(0,len(opaque)-1,min(8,len(opaque)),dtype=int)]
  best=None
  for source in actors[plan['actor']].values():
   b=source['a'];bh,bw=b.shape[:2]
   if ph>bh or pw>bw:continue
   candidates=np.ones((bh-ph+1,bw-pw+1),dtype=bool)
   for py,px in sample:
    candidates &= np.all(b[py:py+bh-ph+1,px:px+bw-pw+1]==patch[py,px],axis=2)
   for sy,sx in np.argwhere(candidates):
    if side=='left' and sx<2 or side=='right' and sx+pw>bw-2 or side=='top' and sy<2:continue
    match=np.all(b[sy:sy+ph,sx:sx+pw]==patch,axis=2)
    score=float(match.sum()/match.size)
    if score<0.90 or best and score<=best['score']:continue
    dx=x0-int(sx);dy=y0-int(sy)
    # Require visible continuation outside the old cell, not a blank match.
    yy,xx=np.indices(b.shape[:2]);tx=xx+dx;ty=yy+dy
    outside=(tx<0 if side=='left' else tx>=w if side=='right' else ty<0)
    near=(tx>=-20)&(tx<w+20)&(ty>=-16)&(ty<h)
    if side=='top':near&=(tx>=x0-2)&(tx<x1+2)
    else:near&=(ty>=y0-2)&(ty<y1+2)
    added=int(((b[:,:,3]>0)&outside&near).sum())
    if not 0<added<250:continue
    best=dict(score=score,added_pixels=added,source_stem=source['job']['stem'],source_frame=source['index'],source_original=source['job']['original'],source_crop=source['row']['source']['crop_top_left'],translation=[dx,dy],target_box=box)
  if best:results.append(dict(id=plan['id'],actor=plan['actor'],side=side,**best))
(OUT/'temporal-repair-proposals.json').write_text(json.dumps(results,indent=2))
print(json.dumps(dict(proposals=len(results),actors=sorted(set(r['actor'] for r in results)),examples=results[:5]),indent=2))
