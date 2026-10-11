"""Read-only full-roster comparisons using the actual bound ready sprites."""
from pathlib import Path
from PIL import Image,ImageDraw
import json
ROOT=Path(__file__).resolve().parents[2];BASE=Path(__file__).resolve().parent;OUT=ROOT/'.utmp/EnemyArtCohesion'
jobs=json.loads((BASE/'jobs.json').read_text(encoding='utf-8'));audit=json.loads((ROOT/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text(encoding='utf-8'));lookup={}
for j in jobs:
 for i,r in enumerate(j['sprites']):lookup[j['path'],str(r['source']['fileID'])]=(j,i)
rows=[]
for actor in audit['actors']:
 if actor['zone']=='Players':continue
 r=actor['still_binding'];j,i=lookup[r['path'],str(r['fileID'])];w,h=j['size']
 before=Image.open(ROOT/j['original']).convert('RGBA').crop(r['crop_top_left']);after=Image.open(BASE/'Sheets'/(j['stem']+'.png')).convert('RGBA').crop((i*w,0,(i+1)*w,h));rows.append((actor,j,before,after))
for page,start in enumerate(range(0,len(rows),8)):
 chunk=rows[start:start+8];cw=max(r[3].width for r in chunk)*4+24;ch=max(r[3].height for r in chunk)*2+36
 out=Image.new('RGB',(cw*2,ch*4),(35,38,51));d=ImageDraw.Draw(out)
 for k,(actor,j,b,a) in enumerate(chunk):
  x,y=k%2*cw,k//2*ch
  if k%2:d.rectangle((x,y,x+cw-1,y+ch-1),fill=(219,215,203))
  d.text((x+5,y+5),actor['name']+' | original / updated',fill='black' if k%2 else 'white')
  for im,xx,yy in [(b,x+8+j['padding'][0]*2,y+26+j['padding'][1]*2),(a,x+cw//2+8,y+26)]:
   big=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);out.paste(big,(xx,yy),big)
 out.save(OUT/f'ready-review-{page:02}.png')
print('Rendered',len(rows),'ready comparisons on',page+1,'pages')
