"""Read-only edge measurements and provider mask rectangles, not pixel edits."""
from pathlib import Path
from PIL import Image
from collections import Counter
import hashlib,json
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'.utmp/EnemyArtCohesion'
jobs=json.loads((ROOT/'ArtSource/EnemyArtCohesion/jobs.json').read_text())
plans=[];seen={};counts=Counter()
for j in jobs:
 source=Image.open(ROOT/j['original']).convert('RGBA');ow,oh=j['old_size'];w,h=j['size']
 for f,row in enumerate(j['sprites']):
  im=source.crop(row['source']['crop_top_left']);key=hashlib.sha256(im.tobytes()).hexdigest()
  sides={}
  for side,pts in [('left',[(0,y) for y in range(oh)]),('right',[(ow-1,y) for y in range(oh)]),('top',[(x,0) for x in range(ow)])]:
   colored=[(x,y) for x,y in pts if im.getpixel((x,y))[3] and max(im.getpixel((x,y))[:3])>42]
   if not colored:continue
   if side=='top':
    lo=min(x for x,y in colored)+24-6;hi=max(x for x,y in colored)+24+7
    mw=max(32,hi-lo);mx=max(0,min(lo,w-mw));mask=[mx,0,mw,28]
   else:
    lo=min(y for x,y in colored)+20-6;hi=max(y for x,y in colored)+20+7
    mh=max(32,hi-lo);my=max(0,min(lo,h-mh));mask=[0 if side=='left' else w-32,my,32,mh]
   sides[side]=dict(contact_pixels=len(colored),mask=mask)
  if not sides:continue
  ident=j['stem']+'-'+str(f)
  entry=dict(id=ident,actor=j['actor'],stem=j['stem'],frame=f,states=row['states'],source=row['source'],sides=sides,
   input='.utmp/EnemyArtCohesion/RepairInputs/'+ident+'.png',status='needs visual repair decision')
  if key in seen:entry['duplicate_of']=seen[key]
  else:seen[key]=ident;counts[j['actor']]+=len(sides)
  plans.append(entry)
(OUT/'edge-repair-plan.json').write_text(json.dumps(plans,indent=2))
print(json.dumps(dict(total_frames=len(plans),unique_frames=len(seen),unique_edge_regions=sum(counts.values()),by_actor=counts),indent=2))
