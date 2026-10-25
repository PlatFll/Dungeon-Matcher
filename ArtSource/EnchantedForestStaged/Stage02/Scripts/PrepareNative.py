from pathlib import Path
from PIL import Image,ImageDraw
import json,uuid,re,shutil
r=Path(__file__).resolve().parents[4]; root=r/'ArtSource/EnchantedForestStaged/Stage02'; art=r/'Assets/_Game/Art/EnchantedForestStaged/Stage02'; temp=r/'.utmp/enchanted-stage02'
temp.mkdir(parents=True,exist_ok=True);(temp/'reopened').mkdir(exist_ok=True)
O='#0c1014';D='#3b2c20';S='#563c26';B='#765131';L='#9a6d41';H='#bb9061'
GD='#233f2b';GS='#3c5f33';G='#4c7037';GL='#65863e';GH='#7a9845';F='#e7dabb'
ramps={'Top':[O,H,L,B,B,S,S,D,O],'Left':[O,L,B,B,B,S,S,D,O],
'Right':[O,D,S,B,B,L,S,D,O],'Bottom':[O,D,S,S,B,B,L,L,O]}
items={}
for side,ramp in ramps.items():
 hor=side in ['Top','Bottom'];im=Image.new('RGBA',(32,9) if hor else (9,32));d=ImageDraw.Draw(im)
 for z,c in enumerate(ramp):
  k=8-z if side in ['Bottom','Right'] else z
  d.line((0,k,31,k) if hor else (k,0,k,31),fill=c)
 d.line((10,4,21,4) if hor else (4,10,4,21),fill=S)
 if side in ['Top','Left']:d.line((12,3,18,3) if hor else (3,12,3,18),fill=L)
 items['Portrait_Rail_'+side]=im
frame=Image.new('RGBA',(62,62));px=frame.load()
for y in range(3,59):
 for x in range(3,59):
  ds={'Top':y-3,'Left':x-3,'Bottom':58-y,'Right':58-x};side=min(ds,key=ds.get);v=ds[side]
  if v<=8:
   c=ramps[side][v]
   if 2<=v<=6 and ((ds['Top']==ds['Left'])or(ds['Top']==ds['Right'])):c=S
   elif 2<=v<=6 and ((ds['Bottom']==ds['Left'])or(ds['Bottom']==ds['Right'])):c=D
   px[x,y]=(*bytes.fromhex(c[1:]),255)
for n,b in [('TL',(0,0,15,15)),('TR',(47,0,62,15)),('BL',(0,47,15,62)),('BR',(47,47,62,62))]:items['Portrait_Corner_'+n]=frame.crop(b)
im=Image.new('RGBA',(7,7));d=ImageDraw.Draw(im);d.rectangle((1,1,5,5),fill=O);d.rectangle((2,2,4,4),fill=B);d.line((2,2,4,2),fill=L);d.point((2,3),fill=L);d.line((2,4,4,4),fill=D);items['Portrait_Join']=im
# Repeatable dark vertical recess. Broad plank planes, deliberately quiet behind the gem and skull.
im=Image.new('RGBA',(32,32),'#15211d');d=ImageDraw.Draw(im)
d.rectangle((0,0,1,31),fill='#101d1b');d.line((2,0,2,31),fill='#1c261e')
d.line((17,0,17,31),fill='#101d1b');d.line((18,0,18,31),fill='#1c261e')
d.line((10,8,10,15),fill='#1c261e');d.line((26,20,26,26),fill='#101d1b')
items['Portrait_Inset']=im
# Native clusters trace the broad ivy landmarks in the generated source; clipping and
# micro-stair noise are repaired with explicit polygons. No scaling or rotated images.
def leaf(d,x,y,kind=0):
 if kind==0: # pointed ivy leaf, top-left plane
  out=[(0,6),(2,4),(2,1),(5,3),(7,0),(8,4),(11,4),(9,7),(10,10),(6,9),(4,12),(3,9),(0,9)]
  mid=[(2,6),(4,5),(4,3),(6,5),(7,3),(7,6),(9,5),(7,8),(5,8),(4,10),(4,7),(1,8)]
  light=[(4,4),(5,5),(7,4),(6,6),(4,8),(3,7)]
 elif kind==1: # rounder drooping plane
  out=[(1,0),(5,1),(8,4),(10,9),(9,13),(5,12),(2,9),(0,4)]
  mid=[(2,2),(5,3),(7,5),(8,9),(8,11),(5,10),(3,7),(1,4)]
  light=[(2,2),(4,3),(5,6),(5,9),(3,6)]
 else: # right-reaching ivy
  out=[(0,7),(2,3),(5,2),(8,0),(9,3),(13,4),(11,8),(7,10),(3,9)]
  mid=[(2,7),(4,4),(7,3),(8,2),(8,5),(11,5),(9,7),(6,8)]
  light=[(4,4),(7,3),(8,4),(6,5),(3,7)]
 shift=lambda pts:[(x+a,y+b) for a,b in pts]
 d.polygon(shift(out),fill=O);d.polygon(shift(mid),fill=G);d.polygon(shift(light),fill=GL)
 d.line([(x+a,y+b) for a,b in [(4,6),(6,5)]],fill=GH)
def vine(d,pts):
 d.line(pts,fill=O,width=3);d.line([(x,y-1) for x,y in pts],fill=GD,width=1)
def flower(d,x,y):
 d.polygon([(x+2,y),(x+4,y+2),(x+6,y+2),(x+5,y+4),(x+6,y+6),(x+3,y+5),(x+1,y+7),(x+1,y+4),(x-1,y+3),(x+1,y+2)],fill=O)
 for box in [(x+2,y+1,x+3,y+2),(x,y+3,x+2,y+4),(x+4,y+3,x+5,y+4),(x+2,y+5,x+3,y+6)]:d.rectangle(box,fill=F)
 d.point((x+3,y+3),fill=H)
im=Image.new('RGBA',(43,27));d=ImageDraw.Draw(im)
vine(d,[(8,23),(10,17),(17,13),(25,13),(35,12),(39,10)])
leaf(d,5,10,1);leaf(d,14,3,0);leaf(d,26,4,2);leaf(d,2,2,0)
items['Portrait_Ivy_UpperLeft']=im
im=Image.new('RGBA',(26,50));d=ImageDraw.Draw(im)
vine(d,[(10,3),(12,11),(11,19),(13,28),(11,37),(13,46)])
leaf(d,10,5,2);leaf(d,3,17,1);leaf(d,11,30,0)
items['Portrait_Ivy_UpperRight']=im
im=Image.new('RGBA',(25,44));d=ImageDraw.Draw(im)
vine(d,[(12,3),(10,10),(12,20),(11,29),(14,40)])
leaf(d,3,8,0);leaf(d,12,21,1);flower(d,14,9)
items['Portrait_Ivy_LeftReturn']=im
im=Image.new('RGBA',(31,45));d=ImageDraw.Draw(im)
vine(d,[(18,3),(16,13),(17,23),(13,34),(6,40)])
leaf(d,6,6,1);leaf(d,17,17,0);leaf(d,4,27,2);flower(d,18,30)
items['Portrait_Ivy_LowerRight']=im
plan=[]
for name,im in items.items():
 Image.new('RGBA',im.size).save(temp/(name+'-canvas.png'));im.save(temp/(name+'-expected.png'))
 rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   c=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==c:end+=1
   if c[3]:row.append([x,end-x,bytes(c).hex()])
   x=end
  rows.append(row)
 plan.append(dict(name=name,w=im.width,h=im.height,rows=rows))
 meta=(r/'Assets/_Game/Art/EnchantedForestStaged/Stage01/Wisp_Far_A.png.meta').read_text()
 meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/EnchantedForestStaged/Stage02/'+name).hex,meta,flags=re.M)
 (art/(name+'.png.meta')).write_text(meta)
data=json.dumps(plan,separators=(',',':'))
(root/'Scripts/NativePixelPlan.json').write_text(data)
js="var items="+data+";var pc=app.pixelColor;\n"+"""items.forEach(function(a){
var s=app.open('.utmp/enchanted-stage02/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;
a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});
s.layer(0).name=a.name+' native pixel cleanup';s.commit();
s.saveAs('ArtSource/EnchantedForestStaged/Stage02/Native/'+a.name+'.aseprite',true);
s.saveAs('Assets/_Game/Art/EnchantedForestStaged/Stage02/'+a.name+'.png',true);
});console.log('STAGE02_NATIVE_COMPLETE');
"""
(root/'Scripts/AuthorNative.js').write_text(js)
(root/'Scripts/RunNative.js').write_text("try{"+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(root/'Scripts/ReopenNative.js').write_text("try{"+json.dumps(list(items))+".forEach(function(n){app.open('ArtSource/EnchantedForestStaged/Stage02/Native/'+n+'.aseprite').sprite.saveAs('.utmp/enchanted-stage02/reopened/'+n+'.png',true);});console.log('STAGE02_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();")
# Exact shell study; context/gem remain separate in Unity.
panel=Image.new('RGBA',(120,240));fill=Image.new('RGBA',(74,196))
for y in range(0,196,32):
 for x in range(0,74,32):fill.paste(items['Portrait_Inset'],(x,y))
panel.alpha_composite(fill,(22,22))
for side in ['Top','Bottom','Left','Right']:
 tile=items['Portrait_Rail_'+side];hor=side in ['Top','Bottom'];length=84 if hor else 206
 strip=Image.new('RGBA',(length,9) if hor else(9,length))
 for z in range(0,length,32):strip.paste(tile,(z,0) if hor else(0,z))
 pos={'Top':(17,13),'Bottom':(17,218),'Left':(13,17),'Right':(96,17)}[side];panel.alpha_composite(strip,pos)
for n,pos in [('TL',(10,10)),('TR',(93,10)),('BL',(10,215)),('BR',(93,215))]:panel.alpha_composite(items['Portrait_Corner_'+n],pos)
placements=[('Portrait_Ivy_UpperLeft',-5,-5),('Portrait_Ivy_UpperRight',80,18),('Portrait_Ivy_LeftReturn',-4,77),('Portrait_Ivy_LowerRight',78,172)]
for n,x,y in placements:panel.alpha_composite(items[n],(10+x,10+y))
panel.save(root/'Previews/NativeFrameStudy.png');panel.resize((360,720),Image.Resampling.NEAREST).save(root/'Previews/NativeFrameStudy-3x.png')
(root/'Scripts/FoliagePlacement.json').write_text(json.dumps(placements,indent=2))
print('Prepared',len(items),'native modules; current frame dimensions remain 98x220.')
