"""Read-only contact sheets of exact native candidate cells, with full canvases."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,sys
ROOT=Path(__file__).resolve().parents[2];BASE=Path(__file__).resolve().parent;OUT=ROOT/'.utmp/EnemyArtCohesion'
jobs=json.loads((BASE/'jobs.json').read_text(encoding='utf-8'));audit=json.loads((ROOT/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text(encoding='utf-8'))
lookup={}
for j in jobs:
 for i,r in enumerate(j['sprites']):lookup[j['path'],str(r['source']['fileID'])]=(j,i)
actor=next(a for a in audit['actors'] if a['id']==sys.argv[1])
states=[s for s in actor['states'] if len(sys.argv)<3 or s['name'] in sys.argv[2:]]
for state in states:
 rows=[]
 for f in state['frames']:
  j,i=lookup[f['sprite']['path'],str(f['sprite']['fileID'])];w,h=j['size'];im=Image.open(BASE/'Sheets'/(j['stem']+'.png')).convert('RGBA').crop((i*w,0,(i+1)*w,h));rows.append((im,f['duration']))
 w=max(im.width for im,t in rows)*2+16;h=max(im.height for im,t in rows)*2+32
 for page,start in enumerate(range(0,len(rows),12)):
  chunk=rows[start:start+12];out=Image.new('RGB',(w*2,h*((len(chunk)+1)//2)),(35,38,51));d=ImageDraw.Draw(out)
  for k,(im,duration) in enumerate(chunk):
   x,y=(k%2)*w,(k//2)*h
   if k%2:d.rectangle((x,y,x+w-1,y+h-1),fill=(219,215,203))
   d.text((x+4,y+4),f'{actor["name"]} / {state["name"]} / {start+k+1} / {duration*1000:.0f} ms',fill='black' if k%2 else 'white')
   big=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);out.paste(big,(x+8,y+24),big)
  out.save(OUT/f'state-{actor["id"]}-{state["name"]}-{page:02}.png')
 print(actor['id'],state['name'],len(rows))
