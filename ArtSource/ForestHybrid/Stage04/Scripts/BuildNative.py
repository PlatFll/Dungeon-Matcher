"""Material-specific cleanup of the selected PixelLab props; native pixels executed in LibreSprite."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,shutil
R=Path('ArtSource/ForestHybrid/Stage04');A=Path('Assets/_Game/Art/ForestHybrid/Stage04');T=Path('.utmp/forest-stage04')
for p in [A,R/'Native',R/'Scripts',R/'Previews',T/'reopened']:p.mkdir(parents=True,exist_ok=True)
raw=Image.open(R/'Sources/Props_Raw_PixelLab.png').convert('RGBA')
def rgba(c):return tuple(bytes.fromhex(c))+(255,)
boxes={'Lantern_Hanging':(9,14,39,76),'Mushroom_Cluster':(48,14,79,45),'Rune_Plaque':(53,50,73,85)}
maps={
 'Lantern_Hanging':{'6b3e35':'414035','b3915f':'8a6845','81905b':'486a45','fff0b0':'f4c86a'},
 'Mushroom_Cluster':{'fff0b0':'d8bb86','d58c3e':'a95d43','81905b':'486a45'},
 'Rune_Plaque':{'6b3e35':'69523a','a95d43':'69523a','b3915f':'8a6845','63a69d':'69523a'}
}
assets={};rawcrops={}
for name,b in boxes.items():
 src=raw.crop(b); im=Image.new('RGBA',src.size)
 for y in range(src.height):
  for x in range(src.width):
   p=src.getpixel((x,y))
   if p[3]:im.putpixel((x,y),rgba(maps[name].get(bytes(p[:3]).hex(),bytes(p[:3]).hex())))
 if name=='Lantern_Hanging':
  # Keep only the center of the flame/window at cream intensity.
  for y in range(43,49):
   for x in range(13,16):
    if im.getpixel((x,y))==rgba('f4c86a'):im.putpixel((x,y),rgba('fff0b0'))
 if name=='Mushroom_Cluster':
  # Consolidate scattered single-pixel foliage flecks, keeping the connected base.
  greens={rgba('486a45'),rgba('81905b')};seen=set()
  for y in range(im.height):
   for x in range(im.width):
    if (x,y) in seen or im.getpixel((x,y)) not in greens:continue
    pending=[(x,y)];group=[];seen.add((x,y))
    while pending:
     q=pending.pop();group.append(q)
     for dx,dy in [(0,1),(0,-1),(1,0),(-1,0)]:
      z=(q[0]+dx,q[1]+dy)
      if 0<=z[0]<im.width and 0<=z[1]<im.height and z not in seen and im.getpixel(z) in greens:seen.add(z);pending.append(z)
    if len(group)<3:
     for q in group:im.putpixel(q,(0,0,0,0))
 if name=='Rune_Plaque':
  # One dim glyph, replacing two competing bright marks; connected two-pixel clusters.
  d=ImageDraw.Draw(im)
  d.line([(10,10),(5,15),(5,18),(10,23),(15,18),(15,16),(11,12)],fill=rgba('63a69d'),width=2)
  d.line([(10,17),(9,18),(11,20)],fill=rgba('63a69d'),width=1)
 padded=Image.new('RGBA',(im.width+4,im.height+4));padded.alpha_composite(im,(2,2))
 assets[name]=padded
 original=Image.new('RGBA',padded.size);original.alpha_composite(src,(2,2));rawcrops[name]=original
def runs(im):
 out=[]
 for y in range(im.height):
  row=[];start=0;last=im.getpixel((0,y))
  for x in range(1,im.width+1):
   v=im.getpixel((x,y)) if x<im.width else None
   if v!=last:row.append([start,x-start,bytes(last).hex()]);start=x;last=v
  out.append(row)
 return out
plan={'assets':[{'name':n,'w':i.width,'h':i.height,'rows':runs(i),'source_crop':boxes[n]} for n,i in assets.items()]}
(R/'Scripts/NativePixelPlan.json').write_text(json.dumps(plan,separators=(',',':')))
for im in assets.values():Image.new('RGBA',im.size).save(T/('Canvas_'+str(im.width)+'x'+str(im.height)+'.png'))
js='// Exact native source cleanup executed in LibreSprite.\nvar items='+json.dumps(plan['assets'],separators=(',',':'))+';\n'
js+="""var pc=app.pixelColor;
items.forEach(function(a){var doc=app.open('.utmp/forest-stage04/Canvas_'+a.w+'x'+a.h+'.png'),s=doc.sprite,im=s.layer(0).cel(0).image;
if(s.width!==a.w||s.height!==a.h)throw Error('Canvas mismatch');
a.rows.forEach(function(row,y){row.forEach(function(run){var h=run[2],v=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=run[0];x<run[0]+run[1];x++)im.putPixel(x,y,v);});});
s.layer(0).name=a.name+' native cleanup';s.commit();s.saveAs('ArtSource/ForestHybrid/Stage04/Native/'+a.name+'.aseprite',true);
s.saveAs('Assets/_Game/Art/ForestHybrid/Stage04/'+a.name+'.png',true);});
"""
(R/'Scripts/AuthorNative.js').write_text(js)
(R/'Scripts/VerifyNativeReopen.js').write_text('var names='+json.dumps(list(assets))+";\nnames.forEach(function(n){var d=app.open('ArtSource/ForestHybrid/Stage04/Native/'+n+'.aseprite');d.sprite.saveAs('.utmp/forest-stage04/reopened/'+n+'.png',true);});\n")
(R/'Diagnostics/PaletteDecisions.json').write_text(json.dumps({'material_maps':maps,'padding':2,'no_resampling':True,'manual_changes':['Lantern cream limited to tiny center light','Mushroom isolated foliage flecks consolidated','Two rune marks replaced by one dim connected spiral'],'source_crops':boxes},indent=2))
for n,im in rawcrops.items():im.save(R/'Previews'/('Raw-'+n+'.png'))
placements=[
 {'asset':'Lantern_Hanging','study':'Upper','x':168,'y':89,'role':'left canopy bracket'},
 {'asset':'Lantern_Hanging','study':'Upper','x':424,'y':97,'role':'right canopy'},
 {'asset':'Mushroom_Cluster','study':'Upper','x':150,'y':223,'role':'left ground edge'},
 {'asset':'Mushroom_Cluster','study':'Upper','x':433,'y':223,'role':'right tree foot'},
 {'asset':'Rune_Plaque','study':'Upper','x':436,'y':177,'role':'right tree carving'},
 {'asset':'Lantern_Hanging','study':'Board','x':4,'y':111,'role':'left post'},
 {'asset':'Lantern_Hanging','study':'Board','x':570,'y':271,'role':'right post'},
 {'asset':'Rune_Plaque','study':'Board','x':14,'y':350,'role':'left post carving'},
 {'asset':'Mushroom_Cluster','study':'Board','x':4,'y':552,'role':'left base'},
 {'asset':'Mushroom_Cluster','study':'Board','x':569,'y':552,'role':'right base'}
]
(R/'Placements.json').write_text(json.dumps({'placements':placements},indent=2))
shutil.copyfile(__file__,R/'Scripts/BuildNative.py')
print('Prepared three padded, source-derived native props.')
