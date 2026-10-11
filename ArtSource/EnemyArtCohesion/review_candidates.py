"""Compare native candidate exports with their original imported cells."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,sys
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'.utmp/EnemyArtCohesion'
jobs=json.loads((ROOT/'ArtSource/EnemyArtCohesion/jobs.json').read_text())
for actor in sys.argv[1:]:
 rows=[]
 for j in jobs:
  path=ROOT/f'ArtSource/EnemyArtCohesion/Sheets/{j["stem"]}.png'
  if j['actor']!=actor or not path.exists():continue
  source=Image.open(ROOT/j['original']).convert('RGBA');cand=Image.open(path).convert('RGBA');w,h=j['size']
  for i,r in enumerate(j['sprites']):
   before=source.crop(r['source']['crop_top_left']);after=cand.crop((i*w,0,(i+1)*w,h))
   rows.append((j,r,before,after))
 for page,start in enumerate(range(0,len(rows),12)):
  chunk=rows[start:start+12];cw=max(r[3].width for r in chunk)*4+20;ch=max(r[3].height for r in chunk)*2+24
  board=Image.new('RGB',(cw*2,ch*6),(35,38,51));d=ImageDraw.Draw(board)
  for k,(j,r,b,a) in enumerate(chunk):
   x,y=k%2*cw,k//2*ch
   if k%2:d.rectangle((x,y,x+cw-1,y+ch-1),fill=(219,215,203))
   d.text((x+4,y+4),' / '.join(f'{s} {i+1}' for s,i in r['states']),fill='white' if not k%2 else 'black')
   for im,xx,yy in [(b,x+4+j['padding'][0]*2,y+22+j['padding'][1]*2),(a,x+cw//2+4,y+22)]:
    im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);board.paste(im,(xx,yy),im)
  board.save(OUT/f'{actor}-candidate-{page:02}.png')
 print(actor,len(rows))
