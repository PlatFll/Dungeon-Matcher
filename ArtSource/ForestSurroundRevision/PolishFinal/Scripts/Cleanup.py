from pathlib import Path
from PIL import Image,ImageDraw
from collections import Counter
import json,colorsys,shutil,math
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/PolishFinal';old=F/'ArtSource/ForestSurroundRevision/PolishStage02';T=F/'.utmp/forest-polish-final';T.mkdir(parents=True,exist_ok=True)
def rgb(h):return tuple(bytes.fromhex(h))+(255,)
pal=[rgb(h)for h in ['0c1014','241e1a','3b2c20','563c26','765131','997344','b08a57','172920','233f2b','3c5f33','4c7037','718b45','94ac57','23313a','314651','49616d','739096','663b34','975044','c46c58','e69b7a','9d805b','c9af7f','e7d9b5','414465','70669b','a89cc9','c8c6e1']]
def connected(im):
 pts={(x,y)for y in range(im.height)for x in range(im.width)if im.getpixel((x,y))[3]};parts=[]
 while pts:
  todo=[pts.pop()];part=[]
  while todo:
   x,y=todo.pop();part.append((x,y))
   for dx in [-1,0,1]:
    for dy in [-1,0,1]:
     q=(x+dx,y+dy)
     if q in pts:pts.remove(q);todo.append(q)
  parts.append(part)
 return sorted(parts,key=len,reverse=True)
def clean(im,palette=pal):
 im=im.convert('RGBA');out=Image.new('RGBA',im.size);mapping={c:(0,0,0,0)if c[3]<128 else min(palette,key=lambda p:sum((p[i]-c[i])**2 for i in range(3)))for c in im.get_flattened_data()};out.putdata([mapping[c]for c in im.get_flattened_data()])
 for part in connected(out)[1:]:
  if len(part)<8:
   for q in part:out.putpixel(q,(0,0,0,0))
 return out
def flower(d,x,y,kind):
 shadow=rgb('233f2b');stem=rgb('4c7037');d.line([(x,y+2),(x,y+6)],fill=stem,width=1);d.line([(x,y+5),(x+2,y+4)],fill=stem,width=1)
 tones=['70669b','a89cc9','c8c6e1']if kind=='blue'else['c9af7f','e7d9b5','e7d9b5']
 for dx,dy in [(-1,0),(1,0),(0,-1),(0,1)]:d.point((x+dx,y+dy),fill=rgb(tones[1 if dy<=0 else 0]))
 d.point((x,y),fill=rgb('b08a57'))
records=[]
for side in ['Left','Right']:
 raw=Image.open(R/'Raw'/(side+'Corner.png'));body=clean(raw);im=Image.new('RGBA',(88,180));im.alpha_composite(body,(0,56))
 # Keep the approved root/crown exactly through the native joining section.
 original=Image.open(old/'Previews'/(side+'-Native-Assembly.png')).convert('RGBA')
 im.paste(original.crop((0,0,88,102)),(0,0))
 # Reject the model's cream-painted stone highlights; retain the established cool rock material.
 if side=='Right':
  for y in range(102,153):
   for x in range(88):
    if (y<124 or x<26) and im.getpixel((x,y)) in [rgb('c9af7f'),rgb('e7d9b5')]:im.putpixel((x,y),rgb('739096'))
 d=ImageDraw.Draw(im)
 # Small stem-ground pockets and flowers sit on existing connected moss/stone planes.
 if side=='Left':
  flower(d,24,142,'blue');flower(d,29,145,'blue');flower(d,68,163,'cream')
 else:
  flower(d,67,137,'blue');flower(d,72,140,'blue');flower(d,24,153,'cream')
 # Remove generated isolated specks; keep the main connected corner silhouette.
 parts=connected(im)
 for part in parts[1:]:
  for q in part:im.putpixel(q,(0,0,0,0))
 name='Corner_'+side;im.save(T/(name+'-plan.png'));im.resize((264,540),Image.Resampling.NEAREST).save(R/'Previews'/(name+'-3x.png'))
 records.append(dict(name=name,dimensions=im.size,bounds=im.getbbox(),colors=len({c for c in im.get_flattened_data()if c[3]}),components=[len(p)for p in connected(im)]))
for name,source in [('Vine','VineV2'),('Sweep_Left','SweepLeftV2'),('Sweep_Right','SweepRightV2'),('Growth_Left','GrowthLeft'),('Growth_Right','GrowthRight')]:
 p=R/'Raw'/(source+'.png')
 if not p.exists():continue
 im=clean(Image.open(p));b=im.getbbox();im=im.crop(b);padded=Image.new('RGBA',(im.width+8,im.height+8));padded.alpha_composite(im,(4,4));im=padded
 if name.startswith('Sweep_'):
  # Use the stronger PixelLab S-curve study. Complete both cut study ends with
  # native bark caps, then derive the opposite-side variant without resampling.
  im=clean(Image.open(R/'Raw/SweepRightV2.png'));d=ImageDraw.Draw(im)
  for pts,width,tone in [([(21,82),(15,88)],13,'0c1014'),([(15,88),(10,94)],7,'0c1014'),([(10,94),(8,96)],3,'0c1014'),([(21,82),(15,88)],10,'3b2c20'),([(15,88),(10,93)],4,'3b2c20'),([(21,80),(14,87)],5,'563c26'),([(21,79),(15,84),(12,89)],2,'765131')]:d.line(pts,fill=rgb(tone),width=width)
  # The upper end is buried in the corner rock; a closed contour avoids a cut-flat edge.
  d.ellipse((132,22,153,48),fill=rgb('0c1014'));d.ellipse((132,23,151,46),fill=rgb('3b2c20'));d.ellipse((131,24,149,39),fill=rgb('563c26'));d.ellipse((131,24,146,31),fill=rgb('765131'))
  # Merge isolated bark speckle into connected planes; preserve contours, moss and flowers.
  woodset={rgb(h)for h in ['241e1a','3b2c20','563c26','765131','997344','b08a57']}
  prev=im.copy()
  for y in range(1,im.height-1):
   for x in range(1,im.width-1):
    c=prev.getpixel((x,y))
    if c not in woodset:continue
    ns=[prev.getpixel(q)for q in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)]];common,n=Counter(ns).most_common(1)[0]
    if n>=3 and common in woodset:im.putpixel((x,y),common)
  b=im.getbbox();im=im.crop(b);padded=Image.new('RGBA',(im.width+8,im.height+8));padded.alpha_composite(im,(4,4));im=padded
  im=im.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
  # Curl the inner tip below the shared text margin instead of crossing behind it.
  d=ImageDraw.Draw(im);d.rectangle((94,0,154,88),fill=(0,0,0,0))
  for pts,width,tone in [([(92,51),(97,56)],17,'0c1014'),([(97,56),(100,63)],12,'0c1014'),([(100,63),(103,70)],7,'0c1014'),([(103,70),(104,74)],3,'0c1014'),([(92,51),(97,56)],14,'3b2c20'),([(97,56),(100,63)],9,'3b2c20'),([(100,63),(103,70)],4,'3b2c20'),([(92,48),(96,53),(99,60)],7,'563c26'),([(99,60),(102,67)],4,'563c26'),([(92,46),(96,51),(98,56)],3,'765131')]:d.line(pts,fill=rgb(tone),width=width)
  d.line([(91,45),(95,47),(97,49)],fill=rgb('4c7037'),width=2)
  if name.endswith('Left'):
   # Flower accents vary while the broad top-lit bark planes remain consistent.
   im.putdata([rgb('a89cc9')if c==rgb('c9af7f')else rgb('c8c6e1')if c==rgb('e7d9b5')else c for c in im.get_flattened_data()])
  else:im=im.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
  for part in connected(im)[1:]:
   for q in part:im.putpixel(q,(0,0,0,0))
 im.save(T/(name+'-plan.png'));im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST).save(R/'Previews'/(name+'-3x.png'))
 records.append(dict(name=name,dimensions=im.size,bounds=im.getbbox(),colors=len({c for c in im.get_flattened_data()if c[3]}),components=[len(p)for p in connected(im)],native_crop=b,padding=4))
wisp=clean(Image.open(R/'Raw/Wisp.png'),[rgb(h)for h in ['21636a','3bb8b5','a1ecdf','effff5']]);wisp.save(T/'Wisp-plan.png');wisp.resize((144,144),Image.Resampling.NEAREST).save(R/'Previews/Wisp-6x.png')
records.append(dict(name='Wisp',dimensions=wisp.size,bounds=wisp.getbbox(),colors=len({c for c in wisp.get_flattened_data()if c[3]}),components=[len(p)for p in connected(wisp)]))
(R/'Diagnostics/NativePlan.json').write_text(json.dumps(records,indent=2));print(json.dumps(records,indent=2))
if Path(__file__).resolve()!=(R/'Scripts/Cleanup.py').resolve():shutil.copy2(__file__,R/'Scripts/Cleanup.py')
