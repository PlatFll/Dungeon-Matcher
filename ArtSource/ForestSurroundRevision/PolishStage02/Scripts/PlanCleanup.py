from pathlib import Path
from PIL import Image,ImageDraw
from collections import Counter
import json,math,hashlib,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/PolishStage02'
def rgb(h):return tuple(bytes.fromhex(h))+(255,)
wood=['241e1a','3b2c20','563c26','765131','997344','b08a57']
green=['233f2b','3c5f33','4c7037','718b45','94ac57']
outline=rgb('0c1014');stonecolors={rgb(h)for h in ['23313a','314651','49616d','739096']}
def woodlike(c):
 r,g,b,a=c;return a and r>g*1.08 and r>b*1.4 and r>30
def greenlike(c):
 r,g,b,a=c;return a and g>=r*.94 and g>b*1.25 and g>32 and not (b>r*1.08)
def nearest(c,pal):return min([rgb(h)for h in pal],key=lambda q:sum((c[i]-q[i])**2 for i in range(3)))
def components(points):
 points=set(points);parts=[]
 while points:
  todo=[points.pop()];part=[]
  while todo:
   p=todo.pop();part.append(p)
   for dx in [-1,0,1]:
    for dy in [-1,0,1]:
     q=(p[0]+dx,p[1]+dy)
     if q in points:points.remove(q);todo.append(q)
  parts.append(part)
 return sorted(parts,key=len,reverse=True)
records=[]
for side,bough,bp,sp in [('Left','Bough_D',(0,16),(0,79)),('Right','Bough_B',(32,16),(2,75))]:
 raw=Image.open(R/'Raw'/(side+'-Edit.png')).convert('RGBA')
 originalStone=Image.open(F/'Assets/_Game/Art/ForestSurroundRevision/UnifiedCorners'/('Stone_'+side+'.png')).convert('RGBA')
 originalBough=Image.open(F/'Assets/_Game/Art/EnchantedForestStaged/Stage05/Revision02'/(bough+'.png')).convert('RGBA')
 body=originalStone.copy();woodpixels={(x,y)for y in range(body.height)for x in range(body.width)if woodlike(body.getpixel((x,y)))}
 # Tiny warm flower centers are not roots. Only coherent wooden branches are removed.
 oldwood={p for part in components(woodpixels)if len(part)>=24 or not any(body.getpixel(q)[0]>185 and body.getpixel(q)[1]>160 and body.getpixel(q)[2]>110 for q in part) for p in part}
 removed=set(oldwood)
 # Remove outline pixels belonging solely to the old wooden spur/root. Preserve
 # outlines that also border stone or moss; the stone material pixels stay exact.
 for y in range(body.height):
  for x in range(body.width):
   c=body.getpixel((x,y))
   if not c[3] or max(c[:3])>35:continue
   ns=[(xx,yy)for xx in range(max(0,x-1),min(body.width,x+2))for yy in range(max(0,y-1),min(body.height,y+2))]
   if any(p in oldwood for p in ns) and not any(body.getpixel(p) in stonecolors or greenlike(body.getpixel(p))for p in ns):removed.add((x,y))
 for p in removed:body.putpixel(p,(0,0,0,0))
 # Drop ghost outlines left by the removed spurs, retaining every colored stone/moss pixel.
 for part in components({(x,y)for y in range(body.height)for x in range(body.width)if body.getpixel((x,y))[3]}):
  if all(max(body.getpixel(p)[:3])<45 for p in part):
   for p in part:body.putpixel(p,(0,0,0,0));removed.add(p)
 fills=[]
 if side=='Right':
  # Restore the stone plane exposed by removal of the disconnected diagonal root.
  # Existing stone/moss pixels are immutable; only former wood/empty recess pixels change.
  fillmask=Image.new('1',body.size);ImageDraw.Draw(fillmask).polygon([(29,27),(40,24),(49,29),(57,39),(46,39),(36,32)],fill=1)
  for y in range(body.height):
   for x in range(body.width):
    if fillmask.getpixel((x,y)) and body.getpixel((x,y))not in stonecolors and not greenlike(body.getpixel((x,y))):
     value=rgb('739096' if y<34 else '49616d');body.putpixel((x,y),value);fills.append([x,y,bytes(value).hex()])
 # Edited branch/root wood, its original native contour, and attached branch moss.
 root=Image.new('RGBA',(88,180));mask={(x,y)for y in range(180)for x in range(88)if woodlike(raw.getpixel((x,y)))or(y<100 and greenlike(raw.getpixel((x,y))))}
 for x,y in mask:
  c=raw.getpixel((x,y));root.putpixel((x,y),nearest(c,wood if woodlike(c)else green))
 for y in range(180):
  for x in range(88):
   c=raw.getpixel((x,y))
   if c[3] and max(c[:3])<45 and any((xx,yy)in mask for xx in range(max(0,x-1),min(88,x+2))for yy in range(max(0,y-1),min(180,y+2))):root.putpixel((x,y),outline)
 # Preserve the approved upper leaf crown and its exact registration.
 for y in range(60):
  for x in range(88):
   ox,oy=x-bp[0],y-bp[1]
   root.putpixel((x,y),originalBough.getpixel((ox,oy))if 0<=ox<originalBough.width and 0<=oy<originalBough.height else (0,0,0,0))
 # Reject detached generated bark flecks and isolated twig scraps.
 parts=components({(x,y)for y in range(root.height)for x in range(root.width)if root.getpixel((x,y))[3]})
 for part in parts[1:]:
  for p in part:root.putpixel(p,(0,0,0,0))
 body.save(R/'Previews'/('StoneAnchor_'+side+'-Plan.png'));root.save(R/'Previews'/('Root_'+side+'-Plan.png'))
 assembled=Image.new('RGBA',(88,180));assembled.alpha_composite(body,sp);assembled.alpha_composite(root)
 assembled.resize((264,540),Image.Resampling.NEAREST).save(R/'Previews'/(side+'-Plan-3x.png'))
 print(side,'root',root.getbbox(),'colors',len({c for c in root.get_flattened_data()if c[3]}),'removed old wood',len(removed))
 records.append(dict(side=side,body_offset=sp,root_offset=[0 if side=='Left' else 272,500],old_wood_pixels=[list(p)for p in sorted(removed)],exposed_stone_fills=fills,source_stone_sha256=hashlib.sha256((F/'Assets/_Game/Art/ForestSurroundRevision/UnifiedCorners'/('Stone_'+side+'.png')).read_bytes()).hexdigest()))
(R/'Diagnostics/SeparationPlan.json').write_text(json.dumps(records,indent=2))
if Path(__file__).resolve() != (R/'Scripts/PlanCleanup.py').resolve(): shutil.copy2(__file__,R/'Scripts/PlanCleanup.py')
