from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,re,shutil,sys,uuid
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint04';a=p/'Assets/_Game/Art/ForestReworkV2/Checkpoint04';s=p/'.utmp/forest-v2-c4'
for f in [a,s/'reopened']:f.mkdir(parents=True,exist_ok=True)
O='#172326';D='#2b2d27';S='#414035';B='#69523a';L='#8a6845'
ramps={'Top':[O,L,B,B,B,B,S,S,S,D,O],'Left':[O,L,B,B,B,B,S,S,S,D,O],
       'Bottom':[O,D,S,S,B,B,S,S,D,D,O],'Right':[O,D,S,S,B,B,S,S,D,D,O]}
items={}
for side in ramps:
 h=side in ('Top','Bottom');im=Image.new('RGBA',(32,11) if h else (11,32));d=ImageDraw.Draw(im)
 for z,c in enumerate(ramps[side]):
  k=10-z if side in ('Bottom','Right') else z
  d.line((0,k,31,k) if h else (k,0,k,31),fill=c)
 d.line((10,5,22,5) if h else (5,10,5,22),fill=S)
 items['Board_Rail_'+side]=im
frame=Image.new('RGBA',(80,80));px=frame.load()
for y in range(6,74):
 for x in range(6,74):
  ds={'Top':y-6,'Left':x-6,'Bottom':73-y,'Right':73-x};side=min(ds,key=ds.get);v=ds[side]
  if v<=10:
   c=ramps[side][v]
   if (ds['Top']==ds['Left'] or ds['Top']==ds['Right']) and 2<=v<=8:c=S
   elif (ds['Bottom']==ds['Left'] or ds['Bottom']==ds['Right']) and 2<=v<=8:c=D
   px[x,y]=(*bytes.fromhex(c[1:]),255)
for name,box in [('TL',(0,0,23,23)),('TR',(57,0,80,23)),('BL',(0,57,23,80)),('BR',(57,57,80,80))]:items['Board_Corner_'+name]=frame.crop(box)
# Foliage plans are prepared only after the actual PixelLab result is inspected.
for name in ['Board_Foliage_UL','Board_Foliage_LR']:
 f=s/(name+'-cleaned.png')
 if f.exists():items[name]=Image.open(f).convert('RGBA')
plans=[]
for name,im in items.items():
 im.save(s/(name+'-expected.png'));Image.new('RGBA',im.size).save(s/(name+'-canvas.png'));rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   c=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==c:end+=1
   if c[3]:row.append([x,end-x,bytes(c).hex()])
   x=end
  rows.append(row)
 plans.append({'name':name,'w':im.width,'h':im.height,'rows':rows})
 meta=(p/'Assets/_Game/Art/ForestHybrid/Stage03/Board_Corner_TL.png.meta').read_text()
 meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestReworkV2/Checkpoint04/'+name).hex,meta,flags=re.M)
 meta=meta.replace('Board_Corner_TL_0',name+'_0').replace('width: 23','width: '+str(im.width)).replace('height: 23','height: '+str(im.height))
 (a/(name+'.png.meta')).write_text('\n'.join(t.rstrip() for t in meta.splitlines())+'\n')
plan=json.dumps(plans,separators=(',',':'));(o/'Scripts/NativePixelPlan.json').write_text(plan)
(o/'Scripts/AuthorNative.js').write_text('var items='+plan+';var pc=app.pixelColor;\n'+'''items.forEach(function(a){
var s=app.open('.utmp/forest-v2-c4/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;
a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});
s.layer(0).name=a.name+' native cleaned board module';s.commit();s.saveAs('ArtSource/ForestReworkV2/Checkpoint04/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/ForestReworkV2/Checkpoint04/'+a.name+'.png',true);
});''')
(o/'Scripts/ReopenNative.js').write_text('var names='+json.dumps(list(items))+';names.forEach(function(n){app.open("ArtSource/ForestReworkV2/Checkpoint04/Native/"+n+".aseprite").sprite.saveAs(".utmp/forest-v2-c4/reopened/"+n+".png",true);});')
if Path(__file__).resolve()!=(o/'Scripts/PrepareNative.py').resolve():shutil.copy2(__file__,o/'Scripts/PrepareNative.py')
print('Prepared',len(items),'native board modules. Previous art untouched.')
