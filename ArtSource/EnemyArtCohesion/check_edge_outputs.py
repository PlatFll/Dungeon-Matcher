"""Read-only pixel invariants and magnified masked-region comparisons."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,sys
ROOT=Path(__file__).resolve().parents[2];BASE=ROOT/'ArtSource/EnemyArtCohesion';OUT=ROOT/'.utmp/EnemyArtCohesion'
records=[];views=[]
unreviewed='--unreviewed' in sys.argv
review=json.loads((BASE/'repair_review.json').read_text()) if unreviewed else {}
for path in sorted((BASE/'RepairReceipts').glob('*.json')):
 r=json.loads(path.read_text());
 if not r.get('output'):continue
 a=Image.open(ROOT/r['input']).convert('RGBA');b=Image.open(ROOT/r['output']).convert('RGBA');p=r['parameters'];x,y,w,h=[p['mask_'+s] for s in ['x','y','width','height']]
 assert a.size==b.size,r['id']
 changed=[(xx,yy) for yy in range(a.height) for xx in range(a.width) if a.getpixel((xx,yy))!=b.getpixel((xx,yy))]
 outside=sum(not(x<=xx<x+w and y<=yy<y+h) for xx,yy in changed)
 alpha=sorted(set(b.getchannel('A').getdata()))
 records.append(dict(id=r['id'],side=r['side'],changed=len(changed),outside_mask=outside,alpha=alpha,bounds=b.getbbox(),input_sha256=hashlib.sha256((ROOT/r['input']).read_bytes()).hexdigest(),output_sha256=hashlib.sha256((ROOT/r['output']).read_bytes()).hexdigest()))
 assert not outside,(r['id'],'outside-mask pixels changed')
 crop=(max(0,x-9),max(0,y-9),min(a.width,x+w+9),min(a.height,y+h+9))
 if not unreviewed or r['id']+'-'+r['side'] not in review:views.append((r,a.crop(crop),b.crop(crop)))
for n,start in enumerate(range(0,len(views),12)):
 chunk=views[start:start+12];cw=max(a.width for r,a,b in chunk)*8+20;ch=max(a.height for r,a,b in chunk)*4+40
 board=Image.new('RGB',(cw*2,ch*6),(38,40,51));draw=ImageDraw.Draw(board)
 for k,(r,a,b) in enumerate(chunk):
  x,y=k%2*cw,k//2*ch
  if k%2:draw.rectangle((x,y,x+cw-1,y+ch-1),fill=(219,215,203))
  draw.text((x+4,y+4),r['id']+' '+r['side'],fill='white' if not k%2 else 'black')
  for im,xx in [(a,x+4),(b,x+cw//2+4)]:
   big=im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST);board.paste(big,(xx,y+26),big)
 board.save(OUT/f'edge-{"unreviewed" if unreviewed else "output-review"}-{n:02}.png')
(OUT/'edge-output-checks.json').write_text(json.dumps(records,indent=2))
print(json.dumps(dict(outputs=len(records),changed=sum(r['changed'] for r in records),outside_mask=sum(r['outside_mask'] for r in records),nonbinary=[r['id'] for r in records if r['alpha']!=[0,255]],pages=(len(views)+11)//12),indent=2))
