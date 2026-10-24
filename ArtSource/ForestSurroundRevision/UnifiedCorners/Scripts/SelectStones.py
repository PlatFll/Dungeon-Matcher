from pathlib import Path
from PIL import Image,ImageDraw
import json
r=Path(__file__).resolve().parents[1]
def col(h):return tuple(bytes.fromhex(h))+(255,)
def components(im):
 points={(x,y)for y in range(im.height)for x in range(im.width)if im.getpixel((x,y))[3]};parts=[]
 while points:
  todo=[points.pop()];part=[]
  while todo:
   x,y=todo.pop();part.append((x,y))
   for dx in [-1,0,1]:
    for dy in [-1,0,1]:
     q=(x+dx,y+dy)
     if q in points:points.remove(q);todo.append(q)
  parts.append(part)
 return sorted(parts,key=len,reverse=True)
assets=[];repairs=[]
for side in ['Left','Right']:
 raw=Image.open(r/'Raw'/f'{side}_2.png').convert('RGBA');before=Image.new('RGBA',(104,136));before.alpha_composite(raw,(12,12));im=before.copy();draw=ImageDraw.Draw(im)
 if side=='Left':
  # Finish the flat left-edge root as a complete curved taper. Native drawing
  # extends into extra canvas, never stretches or trims the original boulder.
  for points,h in [
   ([(2,12),(-2,8),(-5,3),(-7,2),(-8,4),(-7,10),(-5,16),(-2,23),(1,25),(3,21)],'0c1014'),
   ([(1,12),(-3,8),(-5,5),(-6,5),(-5,11),(-3,17),(0,22),(1,21)],'563c26'),
   ([(-5,5),(-4,8),(-2,11),(1,14),(1,12),(-3,7)],'765131')]:
   draw.polygon([(x+12,y+12)for x,y in points],fill=col(h))
  im.alpha_composite(raw,(12,12));reason='Complete the flat cut left root with a hooked taper in expanded native canvas.'
 else:
  # Complete the truncated broad stone face with a rounded five/six-pixel rim.
  for y in range(61,85):
   n=min(6,y-60,85-y)
   for x in range(-n,0):
    c=raw.getpixel((0,y))
    if x==-n:c=col('0c1014')
    elif y<64:c=col('49616d')
    im.putpixel((x+12,y+12),c)
  draw=ImageDraw.Draw(im)
  draw.polygon([(x+12,y+12)for x,y in [(0,89),(-3,88),(-2,91),(-1,94),(1,95),(1,89)]],fill=col('0c1014'))
  draw.line([(11,102),(12,105)],fill=col('563c26'))
  for pts,h in [
   ([(47,35),(49,30),(55,25),(61,19),(65,12),(65,6),(67,4),(69,7),(70,13),(68,20),(63,27),(59,34),(57,40)],'0c1014'),
   ([(50,34),(53,30),(59,24),(64,17),(67,10),(67,7),(68,13),(66,20),(61,27),(57,35),(55,39)],'563c26'),
   ([(50,34),(52,29),(58,23),(63,16),(65,10),(66,9),(65,17),(60,24),(54,31),(52,35)],'765131')]:
   draw.polygon([(x+12,y+12)for x,y in pts],fill=col(h))
  reason='Complete the clipped rock rim and lower root stub; extend the upper wrapping root to join the existing woody connector, keeping the stone body fixed in the scene.'
 parts=components(im);removed=[]
 for part in parts[1:]:
  removed+=part
  for pt in part:im.putpixel(pt,(0,0,0,0))
 edits=[]
 for y in range(im.height):
  for x in range(im.width):
   if im.getpixel((x,y))!=before.getpixel((x,y)):edits.append([x,y,bytes(im.getpixel((x,y))).hex()])
 assets.append(dict(name='Stone_'+side,raw=side+'_2.png',prepad=[12,12,12,12],edits=edits))
 repairs.append(dict(side=side,reason=reason,contour_edits=len(edits),detached_pixels_removed=len(removed),components_before=[len(c)for c in parts]))
 im.crop(im.getbbox()).save(r/'Previews'/(side+'-contour-plan.png'))
assets+=json.loads((r/'Scripts/AccentSelection.json').read_text())
(r/'Scripts/SelectionConfig.json').write_text(json.dumps(dict(assets=assets,judgment='Left 2 and Right 2 from second set: one compact grounded connected stone mass per side, broad stone planes and thick wrapping roots. First tall variants rejected as columns; three small native accents selected instead of oversized Pro studies.',repairs=repairs),indent=2))
print(repairs)
placements=[dict(name='Left main stone formation',file='Stone_Left',x=0,y=579),dict(name='Right main stone formation',file='Stone_Right',x=274,y=575),dict(name='Left pale mushrooms attached to rock',file='Mushrooms_Pale',x=27,y=606),dict(name='Left red mushrooms attached to base',file='Mushrooms_Red',x=45,y=648),dict(name='Left crown moss',file='Moss_Sprig',x=33,y=590),dict(name='Right red mushrooms attached to crevice',file='Mushrooms_Red',x=311,y=625),dict(name='Right pale mushrooms attached to base',file='Mushrooms_Pale',x=292,y=650),dict(name='Right shoulder moss',file='Moss_Sprig',x=334,y=610)]
(r/'Scripts/Placements.json').write_text(json.dumps(dict(pieces=placements),indent=2))
