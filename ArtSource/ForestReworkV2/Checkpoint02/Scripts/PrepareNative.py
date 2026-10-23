from pathlib import Path
from PIL import Image,ImageDraw
from collections import Counter,deque
import json,sys,uuid,re,shutil
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint02';a=p/'Assets/_Game/Art/ForestReworkV2/Checkpoint02';s=p/'.utmp/forest-v2-c2'
raw=Image.open(o/'Raw/FarForest_PixelLab.png').convert('RGBA');far=raw.copy();px=far.load()
assert far.size==(256,160)
visited=set();removed=0
# Deliberate material masks: low moss/rock banks and the left upper canopy.
# The bridge and waterfall, long trunks and all large connected forms stay intact.
allowed=lambda x,y:(92<=y<125 and (x<132 or x>164)) or (y<42 and x<87)
for y in range(160):
 for x in range(256):
  if (x,y) in visited or not allowed(x,y):continue
  col=raw.getpixel((x,y));q=[(x,y)];visited.add((x,y));component=[];neighbors=[]
  while q:
   cx,cy=q.pop();component.append((cx,cy))
   for nx,ny in [(cx-1,cy),(cx+1,cy),(cx,cy-1),(cx,cy+1)]:
    if not(0<=nx<256 and 0<=ny<160):continue
    nc=raw.getpixel((nx,ny))
    if nc!=col:neighbors.append(nc)
    elif (nx,ny) not in visited and allowed(nx,ny):visited.add((nx,ny));q.append((nx,ny))
  if len(component)<=3 and neighbors:
   replacement=Counter(neighbors).most_common(1)[0][0]
   for xy in component:px[xy]=replacement;removed+=1
# Broader coherent ledges in the lower bank, replacing dense point highlights.
d=ImageDraw.Draw(far)
d.polygon([(16,101),(29,98),(40,105),(54,111),(67,120),(35,115)],fill='#182f3b')
d.polygon([(24,101),(29,100),(43,108),(46,111),(34,107)],fill='#284c56')
d.polygon([(174,114),(193,102),(208,97),(222,101),(239,115),(227,121),(197,121)],fill='#1d3e49')
d.polygon([(184,113),(199,104),(210,101),(222,104),(214,105),(201,109),(193,115)],fill='#284c56')
# A handful of connected pale waterfall strokes, not a white halo or texture pass.
d.line([(150,86),(150,94),(149,96),(149,102)],fill='#8eb4ad',width=1)
d.line([(147,111),(147,118)],fill='#619494',width=1)
items={'Far_EnchantedSanctuary':far}
mapping={
 (20,44,53):(18,35,47),(31,65,72):(29,62,73),(43,84,89):(40,76,86),
 (59,100,105):(52,93,100),(81,120,125):(71,119,123),
 (107,140,139):(97,148,148),(145,170,164):(142,180,173),
 (112,132,106):(90,123,85),(77,106,96):(46,75,59)}
for old,new in [('Far_SideExtension','Depth_SideRepeat'),('Far_CanopyRepeat','Depth_CanopyRepeat'),('Understory_Repeat','Depth_UnderstoryRepeat')]:
 im=Image.open(p/'Assets/_Game/Art/ForestHybrid/Stage01'/(old+'.png')).convert('RGBA')
 im.putdata([(*mapping.get(c[:3],c[:3]),c[3]) for c in im.getdata()]);items[new]=im
wisp=Image.new('RGBA',(12,16));d=ImageDraw.Draw(wisp)
d.polygon([(3,7),(5,3),(7,4),(9,7),(8,10),(5,12),(3,10)],fill='#1d3e49')
d.polygon([(4,7),(6,5),(8,7),(6,10),(4,9)],fill='#345d64')
d.line([(5,7),(6,6),(7,7),(6,9)],fill='#619494')
d.point((6,7),fill='#8eb4ad');items['Distant_Wisp']=wisp
plans=[]
for name,im in items.items():
 Image.new('RGBA',im.size).save(s/(name+'-canvas.png'));im.save(s/(name+'-expected.png'))
 rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   color=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==color:end+=1
   if color[3]:row.append([x,end-x,bytes(color).hex()])
   x=end
  rows.append(row)
 plans.append({'name':name,'w':im.width,'h':im.height,'rows':rows})
 # Match the established FullRect/Point/uncompressed/64PPU import; new GUIDs only.
 meta=(p/'Assets/_Game/Art/ForestHybrid/Stage01/Far_Forest.png.meta').read_text()
 meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestReworkV2/Checkpoint02/'+name).hex,meta,flags=re.M)
 meta=meta.replace('Far_Forest_0',name+'_0').replace('width: 256','width: '+str(im.width)).replace('height: 160','height: '+str(im.height))
 (a/(name+'.png.meta')).write_text(meta)
plan=json.dumps(plans,separators=(',',':'));(o/'Scripts/NativePixelPlan.json').write_text(plan)
js='var items='+plan+';var pc=app.pixelColor;\n'+'''items.forEach(function(a){
var s=app.open('.utmp/forest-v2-c2/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;
a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});
s.layer(0).name=a.name+' native cleaned support';s.commit();s.saveAs('ArtSource/ForestReworkV2/Checkpoint02/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/ForestReworkV2/Checkpoint02/'+a.name+'.png',true);
});'''
(o/'Scripts/AuthorNative.js').write_text(js)
(o/'Scripts/ReopenNative.js').write_text('var names='+json.dumps(list(items))+';names.forEach(function(n){app.open("ArtSource/ForestReworkV2/Checkpoint02/Native/"+n+".aseprite").sprite.saveAs(".utmp/forest-v2-c2/reopened/"+n+".png",true);});')
(o/'Diagnostics/CleanupDecisions.json').write_text(json.dumps({'scoped_microcluster_pixels_replaced':removed,'new_assets':list(items),'protected_geometry':'Foreground trees, ground, portrait, actors and all shared UI retained exactly','far_cleanup':'Selected bank/canopy microclusters consolidated; two connected bank planes; restrained waterfall strokes','context_cleanup':'Exact explicit material-color mappings; no geometry changes','wisp':'12x16 native, four opaque colors, padded, opaque clustered light, two distant placements planned','mapping':{str(k):v for k,v in mapping.items()}},indent=2))
shutil.copy2(__file__,o/'Scripts/PrepareNative.py')
print('Prepared five native support sources; removed '+str(removed)+' scoped microcluster pixels. No production overwrites.')
