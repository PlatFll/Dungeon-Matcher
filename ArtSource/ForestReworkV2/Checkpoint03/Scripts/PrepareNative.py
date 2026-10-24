from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json,re,shutil,subprocess,sys,uuid
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint03';a=p/'Assets/_Game/Art/ForestReworkV2/Checkpoint03';s=p/'.utmp/forest-v2-c3'
for f in [o/'Native',o/'Scripts',o/'Previews',o/'Diagnostics',a,s/'reopened']:f.mkdir(parents=True,exist_ok=True)
sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
baseline=o/'Diagnostics/ProtectedHashes.json'
if not baseline.exists():
 files=subprocess.check_output(['rtk','git','-C',str(p),'ls-files'],text=True).splitlines()
 files=[f for f in files if (p/f).is_file() and (f.startswith('Assets/') or f.startswith('ArtSource/') or f.startswith('Docs/ArtDirection/') or f.startswith('ProjectSettings/'))]
 baseline.write_text(json.dumps({f:sha(p/f) for f in files},indent=2))
refs=['Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','Docs/ArtDirection/Golden_Reference_Register.md','Docs/ArtDirection/Audit2026-10/media/skeleton-still.png','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png','ArtSource/ForestHybrid/Stage02/Sources/PortraitFrame_Raw_PixelLab.png','ArtSource/ForestHybrid/Stage02/Sources/PixelLabJob.json']
(o/'Diagnostics/ReferenceHashes.json').write_text(json.dumps({f:sha(p/f) for f in refs},indent=2))
O='#172326';D='#2b2d27';S='#414035';B='#69523a';L='#8a6845';H='#b3915f';G='#486a45';GS='#203c35';GH='#81905b'
# Quiet nine-pixel timber section. Light comes from above/left in every orientation.
ramps={'Top':[O,L,B,B,B,S,S,D,O],'Left':[O,L,B,B,B,S,S,D,O],
       'Bottom':[O,D,S,B,B,S,D,D,O],'Right':[O,D,S,B,B,S,D,D,O]}
items={}
for side in ramps:
 horizontal=side in ('Top','Bottom');im=Image.new('RGBA',(32,9) if horizontal else (9,32));d=ImageDraw.Draw(im)
 for z,c in enumerate(ramps[side]):
  k=8-z if side in ('Bottom','Right') else z
  if horizontal:d.line((0,k,31,k),fill=c)
  else:d.line((k,0,k,31),fill=c)
 # One broad, muted grain groove; clear ends make the repeat seam exact.
 if horizontal:d.line((9,4,21,4),fill=S)
 else:d.line((4,9,4,21),fill=S)
 items['Portrait_Rail_'+side]=im
# Mitred, square timber turns sampled from a single exact frame geometry.
frame=Image.new('RGBA',(62,62));px=frame.load()
for y in range(3,59):
 for x in range(3,59):
  ds={'Top':y-3,'Left':x-3,'Bottom':58-y,'Right':58-x};side=min(ds,key=ds.get);v=ds[side]
  if v<=8:
   c=ramps[side][v]
   # The diagonal joint is restrained and stays inside the broad timber planes.
   if ds['Top']==ds['Left'] and 2<=v<=6:c=S
   elif ds['Top']==ds['Right'] and 2<=v<=6:c=S
   elif ds['Bottom']==ds['Left'] and 2<=v<=6:c=D
   elif ds['Bottom']==ds['Right'] and 2<=v<=6:c=D
   px[x,y]=(*bytes.fromhex(c[1:]),255)
for name,box in [('TL',(0,0,15,15)),('TR',(47,0,62,15)),('BL',(0,47,15,62)),('BR',(47,47,62,62))]:items['Portrait_Corner_'+name]=frame.crop(box)
# Low contrast, centered wood tenon rather than a repeated bright metal stud.
im=Image.new('RGBA',(7,7));d=ImageDraw.Draw(im);d.rectangle((1,1,5,5),fill=O);d.rectangle((2,2,4,4),fill=B);d.line((2,2,4,2),fill=L);d.line((2,4,4,4),fill=S);items['Portrait_Join']=im
def leaf(d,points,light):
 d.polygon(points,fill=O)
 # Native manually placed planes, not a filtered or rotated leaf image.
 d.polygon(light[0],fill=G);d.line(light[1],fill=GH)
# Upper vine curves around the outer timber; three connected, quiet leaf planes.
im=Image.new('RGBA',(30,24));d=ImageDraw.Draw(im)
d.line([(4,17),(5,15),(8,12),(13,10),(19,9),(25,9)],fill=O,width=3)
d.line([(4,17),(5,15),(8,12),(13,10),(19,9),(25,9)],fill=GS,width=1)
leaf(d,[(5,15),(2,11),(2,6),(4,3),(7,5),(9,9),(8,13)],([ (4,5),(6,6),(7,9),(7,12),(5,12),(3,9),(3,6)],[(4,5),(4,8),(5,9)]))
leaf(d,[(10,12),(10,7),(13,3),(17,2),(18,6),(16,10),(13,12)],([(12,7),(14,4),(16,3),(17,5),(15,9),(12,10)],[(14,4),(16,3)]))
leaf(d,[(18,10),(21,6),(25,4),(27,5),(27,8),(24,11),(21,12)],([(20,10),(23,7),(26,6),(26,8),(23,10)],[(23,7),(25,6)]))
items['Portrait_Vine_Upper']=im
# Lower return independently shaded from top left; two leaves, no mirrored light.
im=Image.new('RGBA',(25,27));d=ImageDraw.Draw(im)
d.line([(4,23),(9,23),(14,20),(17,14),(18,6)],fill=O,width=3)
d.line([(4,22),(9,22),(13,19),(16,14),(17,6)],fill=GS)
leaf(d,[(13,18),(10,15),(9,10),(10,6),(13,7),(16,10),(16,15)],([(11,8),(13,9),(14,11),(14,15),(12,15),(10,11)],[(11,8),(11,11)]))
leaf(d,[(17,15),(18,10),(21,7),(23,7),(23,12),(21,15),(18,17)],([(18,14),(20,10),(22,9),(22,12),(20,14)],[(20,10),(21,9)]))
items['Portrait_Vine_Lower']=im
plans=[]
for name,im in items.items():
 im.save(s/(name+'-expected.png'));Image.new('RGBA',im.size).save(s/(name+'-canvas.png'))
 rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   c=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==c:end+=1
   if c[3]:row.append([x,end-x,bytes(c).hex()])
   x=end
  rows.append(row)
 plans.append({'name':name,'w':im.width,'h':im.height,'rows':rows})
 meta=(p/'Assets/_Game/Art/ForestHybrid/Stage02/Portrait_Corner_TL.png.meta').read_text()
 meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestReworkV2/Checkpoint03/'+name).hex,meta,flags=re.M)
 meta=meta.replace('Portrait_Corner_TL_0',name+'_0').replace('width: 15','width: '+str(im.width)).replace('height: 15','height: '+str(im.height))
 (a/(name+'.png.meta')).write_text('\n'.join(t.rstrip() for t in meta.splitlines())+'\n')
plan=json.dumps(plans,separators=(',',':'));(o/'Scripts/NativePixelPlan.json').write_text(plan)
js='var items='+plan+';var pc=app.pixelColor;\n'+'''items.forEach(function(a){
var s=app.open('.utmp/forest-v2-c3/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;
a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});
s.layer(0).name=a.name+' native timber and foliage';s.commit();s.saveAs('ArtSource/ForestReworkV2/Checkpoint03/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/ForestReworkV2/Checkpoint03/'+a.name+'.png',true);
});'''
(o/'Scripts/AuthorNative.js').write_text(js)
(o/'Scripts/ReopenNative.js').write_text('var names='+json.dumps(list(items))+';names.forEach(function(n){app.open("ArtSource/ForestReworkV2/Checkpoint03/Native/"+n+".aseprite").sprite.saveAs(".utmp/forest-v2-c3/reopened/"+n+".png",true);});')
# A native composition study to inspect the joints before the actual Unity build.
im=Image.open(p/'ArtSource/ForestReworkV2/Checkpoint02/Previews/Unity-After-720x480.png').convert('RGBA');panel=Image.new('RGBA',(98,220))
for side in ['Top','Bottom','Left','Right']:
 tile=items['Portrait_Rail_'+side];horizontal=side in ['Top','Bottom'];length=84 if horizontal else 206
 strip=Image.new('RGBA',(length,9) if horizontal else (9,length))
 for z in range(0,length,32):strip.paste(tile,(z,0) if horizontal else (0,z))
 panel.alpha_composite(strip, {'Top':(7,3),'Bottom':(7,208),'Left':(3,7),'Right':(86,7)}[side])
for n,pos in [('TL',(0,0)),('TR',(83,0)),('BL',(0,205)),('BR',(83,205))]:panel.alpha_composite(items['Portrait_Corner_'+n],pos)
# Full Unity scene replaces old images; this study only displays the native frame.
study=Image.new('RGBA',(120,240));study.alpha_composite(panel,(6,10))
study.alpha_composite(items['Portrait_Vine_Upper'],(16,3));study.alpha_composite(items['Portrait_Vine_Lower'],(90,202))
study.save(o/'Previews/NativeFrameStudy.png');study.resize((360,720),Image.Resampling.NEAREST).save(o/'Previews/NativeFrameStudy-3x.png')
if Path(__file__).resolve()!=(o/'Scripts/PrepareNative.py').resolve():shutil.copy2(__file__,o/'Scripts/PrepareNative.py')
print('Prepared',len(items),'native modules. Protected',len(json.loads(baseline.read_text())),'existing files. No generations.')
