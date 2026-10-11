"""Read-only masked-repair checks and before/after review layouts."""
from pathlib import Path
from PIL import Image,ImageDraw
import json
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'.utmp/EnemyArtCohesion'
rows=json.loads((ROOT/'ArtSource/EnemyArtCohesion/smith-repair-jobs.json').read_text())
result=[]
for row in rows:
 if not row.get('output'):continue
 a=Image.open(ROOT/row['input']).convert('RGBA');b=Image.open(ROOT/row['output']).convert('RGBA');p=row['args'];x,y,w,h=[p['mask_'+s] for s in ['x','y','width','height']]
 assert a.size==b.size,row['id']
 changed=[(xx,yy) for yy in range(a.height) for xx in range(a.width) if a.getpixel((xx,yy))!=b.getpixel((xx,yy))]
 outside=[q for q in changed if not(x<=q[0]<x+w and y<=q[1]<y+h)]
 assert not outside,(row['id'],outside)
 alpha=sorted(set(b.getchannel('A').getdata()))
 result.append(dict(id=row['id'],changed=len(changed),outside_mask=len(outside),alpha=alpha,bounds=b.getbbox()))
 canvas=Image.new('RGB',(a.width*8,a.height*4+24),(48,49,60));d=ImageDraw.Draw(canvas);d.text((4,4),row['id']+' | original then masked repair',fill='white')
 for image,xx in [(a,0),(b,a.width*4)]:
  large=image.resize((image.width*4,image.height*4),Image.Resampling.NEAREST);canvas.paste(large,(xx,24),large)
 canvas.save(OUT/(row['id']+'-repair-review.png'))
(OUT/'smith-repair-checks.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))
