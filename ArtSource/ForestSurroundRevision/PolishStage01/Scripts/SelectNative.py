from pathlib import Path
from PIL import Image,ImageDraw
import json,math,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
R=F/'ArtSource/ForestSurroundRevision/PolishStage01'
raw=Image.open(R/'Raw/Mudstone_Pixen.png').convert('RGBA')
mapping={}
for c in set(raw.get_flattened_data()):
 if not c[3]:continue
 red,green,blue=c[:3]
 if max(c[:3])<65:target='171e1c'
 elif red>green and blue<60:target='3c5f33'
 elif red>=160:target='838463' if red>=195 else '6a6c50'
 elif green>=120:target='535641'
 elif green>=90:target='3c4135'
 else:target='293029'
 mapping[bytes(c[:3]).hex()]=target
rows=list(range(5,17))+list(range(22,26))
columns=[list(range(20))+list(range(44,64)),list(range(12))+list(range(24,40))+list(range(52,64)),list(range(4))+list(range(20,52))+list(range(60,64))]
pieces=[];previews=[]
def color(h):return tuple(bytes.fromhex(h))+(255,)
for i,cols in enumerate(columns):
 mapped=Image.new('RGBA',(40,16),color('171e1c'))
 for y,sy in enumerate(rows):
  for x,sx in enumerate(cols):
   c=raw.getpixel((sx,sy))
   if c[3]:mapped.putpixel((x,y),color(mapping[bytes(c[:3]).hex()]))
 im=mapped.copy();d=ImageDraw.Draw(im)
 # A single solid stone face replaces the source's two-course internal mortar.
 # Keep the source top bevel fragments and lower contour, then join broad planes.
 d.rectangle((2,5,37,12),fill=color('535641'))
 d.rectangle((0,0,39,1),fill=color('171e1c'))
 d.rectangle((2,4,37,4),fill=color('535641'))
 d.rectangle((2,12,36,13),fill=color('3c4135'))
 d.line((2,14,36,14),fill=color('293029'))
 d.line((39,0,39,15),fill=color('171e1c'))
 d.line((0,15,39,15),fill=color('171e1c'))
 # Construct the clean squared top lip; a few native chips distinguish the blocks.
 d.rectangle((2,2,37,3),fill=color('6a6c50'))
 d.line((3,2,20,2),fill=color('838463'))
 d.line((1,4,1,12),fill=color('6a6c50'))
 d.line((38,4,38,12),fill=color('293029'))
 if i==0:
  d.line((6,4,11,4),fill=color('6a6c50'));d.line((28,11,33,11),fill=color('3c4135'))
  # Moss belongs to the top lip; connected to the edge rather than scattered dots.
  d.rectangle((3,1,9,2),fill=color('3c5f33'));d.line((4,0,7,0),fill=color('3c5f33'))
  d.line((5,1,8,1),fill=color('718b45'));d.line((3,2,3,3),fill=color('3c5f33'))
 elif i==1:
  d.line((22,3,25,3),fill=color('535641'));d.line((3,13,8,13),fill=color('535641'))
  # Clear residual source moss on this quieter variation.
  d.rectangle((0,0,38,1),fill=color('171e1c'))
  d.line((4,2,18,2),fill=color('838463'));d.line((31,5,35,5),fill=color('6a6c50'))
 else:
  d.rectangle((0,0,38,1),fill=color('171e1c'))
  d.rectangle((27,1,33,2),fill=color('3c5f33'));d.line((29,1,32,1),fill=color('718b45'))
  d.line((12,12,17,12),fill=color('535641'));d.line((2,4,7,4),fill=color('6a6c50'))
 edits=[[x,y,bytes(im.getpixel((x,y))).hex()]for y in range(16)for x in range(40)if im.getpixel((x,y))!=mapped.getpixel((x,y))]
 name='Mudstone_'+chr(65+i)
 pieces.append(dict(name=name,source='Mudstone_Pixen.png',source_rows=rows,source_columns=cols,color_map=mapping,edits=edits,reason='Reassemble source outer contours at native scale, remove the double-course interior mortar, connect broad face planes, normalize edge seams and vary attached top moss. No resampling.'))
 previews.append(im)
 im.save(R/'Previews'/(name+'-Plan.png'))
config=dict(pieces=pieces,judgment='The gray study was too tall and cracked. The olive mudstone source better relates to the clearing and wooden frame. Its double-course geometry is rebuilt into three broad single-block pieces with connected flat faces, native top bevels, shallow wear and occasional attached moss. Nine blocks make a deliberate full-width rhythm; no extra decoration is added.')
(R/'Scripts/Selection.json').write_text(json.dumps(config,indent=2))
out=Image.new('RGBA',(360,16))
for i,v in enumerate([0,1,2,1,0,2,0,1,2]):out.alpha_composite(previews[v],(40*i,0))
out.resize((1080,48),Image.Resampling.NEAREST).save(R/'Previews/Divider-Plan-3x.png')
screen=Image.open(R/'Previews/Before.png').convert('RGBA');screen.alpha_composite(out.resize((720,32),Image.Resampling.NEAREST),(0,448))
screen.crop((0,390,720,550)).save(R/'Previews/Planning-Composite-NOT-Unity.png')
shutil.copy2(__file__,R/'Scripts/SelectNative.py')
print('Prepared three 40x16 modules from the completed Pixen source; 8-color authored palette.')
