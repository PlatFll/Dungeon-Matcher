from pathlib import Path
from PIL import Image,ImageDraw,ImageOps
import json,re,uuid,hashlib
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/EnchantedForestStaged/Stage05';a=p/'Assets/_Game/Art/EnchantedForestStaged/Stage05';t=p/'.utmp/enchanted-stage05'
t.mkdir(parents=True,exist_ok=True);(t/'reopened').mkdir(exist_ok=True)
rgba=lambda h:tuple(bytes.fromhex(h))+(255,)
maps={
 'Narrow_00':{'05090b':'0c1014','0c1713':'172920','29271f':'3b2c20','3a3326':'563c26','594931':'765131','717933':'65863e','4e572b':'3c5f33','3e5c2d':'3c5f33','627d38':'65863e','253521':'233f2b','3d4f6b':'3b505a','8598a5':'8a9d9a','2e1109':'4a2820','956337':'96513a','c97b2b':'be7043','b7b184':'d4bb85','6a8760':'52733a','ffffff':'eddbb2'},
 'Narrow_13':{'050a0c':'0c1014','131f19':'172920','2c2b21':'3b2c20','403929':'563c26','5d4a31':'765131','727a33':'65863e','536331':'4c7037','637e37':'65863e','324e27':'233f2b','475228':'3c5f33','3d4f6b':'3b505a','8598a5':'8a9d9a','c57726':'be7043','6d8760':'52733a','2e1109':'4a2820','e3954b':'d89355','936434':'96513a','b5b081':'d4bb85'}}
clean={}
for n,mp in maps.items():
 raw=Image.open(r/'Raw'/(n+'.png')).convert('RGBA');im=Image.new('RGBA',raw.size)
 for y in range(im.height):
  for x in range(im.width):
   c=raw.getpixel((x,y))
   if c[3]:im.putpixel((x,y),rgba(mp[bytes(c[:3]).hex()]))
 clean[n]=im
items={}
# Extend the native root through the lower margin, instead of floating separate foot props.
# Keep generated crown/foot pixels; author the quiet joining curve at the same native scale.
for tag,n,points in [('Left','Narrow_00',[(11,48),(10,60),(7,76),(9,91),(15,107),(14,119),(11,126)]),('Right','Narrow_13',[(12,48),(14,62),(20,78),(19,93),(13,110),(11,120),(11,126)])]:
 im=Image.new('RGBA',(48,148));d=ImageDraw.Draw(im)
 d.line(points,fill=rgba('0c1014'),width=9,joint='curve');d.line(points,fill=rgba('3b2c20'),width=6,joint='curve')
 d.line([(x-1,y)for x,y in points],fill=rgba('563c26'),width=3,joint='curve')
 for seg in [points[:3],points[3:]]:d.line([(x-2,y)for x,y in seg],fill=rgba('765131'),width=1,joint='curve')
 d.line([(8,67),(6,78),(8,89)] if tag=='Left' else [(17,72),(20,82),(18,92)],fill=rgba('3c5f33'),width=3,joint='curve')
 d.line([(7,69),(5,78),(7,84)] if tag=='Left' else [(16,74),(18,82),(17,88)],fill=rgba('65863e'),width=1)
 im.alpha_composite(clean[n].crop((0,0,48,49)),(0,0));foot=clean[n].crop((0,49,48,72))
 # The foot's original tiny mushrooms remain part of the native stone/root group.
 im.alpha_composite(foot,(0,125));items['Root_Stone_'+tag]=im
# Native leafy branch offshoots, with a closed joining stub. They tuck behind approved ivy.
for tag,n,b in [('Left','Narrow_00',(15,13,44,43)),('Right','Narrow_13',(15,12,44,43))]:
 im=clean[n].crop(b);d=ImageDraw.Draw(im)
 # Remove only the trunk side of the native crop, retaining the entire branch and leaves.
 for y in range(im.height):
  for x in range(min(5,im.width)):
   if x<max(0,5-y//6):im.putpixel((x,y),(0,0,0,0))
 # Eliminate tiny disconnected cutout residue.
 seen=set()
 for y in range(im.height):
  for x in range(im.width):
   if not im.getpixel((x,y))[3] or (x,y) in seen:continue
   todo=[(x,y)];seen.add((x,y));cc=[]
   while todo:
    xx,yy=todo.pop();cc.append((xx,yy))
    for dx in [-1,0,1]:
     for dy in [-1,0,1]:
      q=(xx+dx,yy+dy)
      if 0<=q[0]<im.width and 0<=q[1]<im.height and q not in seen and im.getpixel(q)[3]:seen.add(q);todo.append(q)
   if len(cc)<5:
    for q in cc:im.putpixel(q,(0,0,0,0))
 im=im.crop(im.getbbox());pad=Image.new('RGBA',(im.width+4,im.height+4));pad.alpha_composite(im,(2,2));items['Leaf_Sprig_'+tag]=pad
# Extract intact warm mushrooms from the large study; never downscale their native pixels.
raw=Image.open(r/'Raw/LargeStudy_00.png').convert('RGBA')
warm={'aa4d35':'96513a','632d23':'593126','f2c19a':'eddbb2','693d2e':'765131','d4a87a':'c9a477','b48f6e':'a7835e','c47452':'bd7350'}
dark={'03050a','100c0f','241a18'}
for name,b in [('Mushroom_Pair',(63,60,111,106)),('Mushroom_Tall',(74,60,111,106))]:
 selected={(x,y)for y in range(b[1],b[3])for x in range(b[0],b[2])if raw.getpixel((x,y))[3] and bytes(raw.getpixel((x,y))[:3]).hex() in warm}
 if name=='Mushroom_Tall':selected={(x,y)for x,y in selected if x>=84 or y<83}
 contour=set(selected)
 for x,y in selected:
  for dx,dy in [(1,0),(-1,0),(0,1),(0,-1)]:
   q=x+dx,y+dy;c=raw.getpixel(q)
   if c[3] and bytes(c[:3]).hex() in dark:contour.add(q)
 im=Image.new('RGBA',raw.size)
 for q in contour:
  c=raw.getpixel(q);h=bytes(c[:3]).hex();im.putpixel(q,rgba(warm.get(h,'0c1014')))
 im=im.crop(im.getbbox());pad=Image.new('RGBA',(im.width+4,im.height+4));pad.alpha_composite(im,(2,2));items[name]=pad
placements=[dict(name='Left lower root and stone',file='Root_Stone_Left',x=2,y=516),dict(name='Right lower root and stone',file='Root_Stone_Right',x=310,y=516),dict(name='Left middle branch',file='Leaf_Sprig_Left',x=1,y=357),dict(name='Right middle branch',file='Leaf_Sprig_Right',x=326,y=407),dict(name='Left foot mushrooms',file='Mushroom_Pair',x=1,y=629),dict(name='Right foot mushroom',file='Mushroom_Tall',x=320,y=631)]
placements.insert(2,dict(name='Left lower connected leaves',file='Leaf_Sprig_Left',x=5,y=580))
placements.insert(3,dict(name='Right lower connected leaves',file='Leaf_Sprig_Right',x=321,y=587))
for q in placements:assert q['x']+items[q['file']].width<=360 and q['y']+items[q['file']].height<=680,(q,items[q['file']].size)
(r/'Scripts/Placements.json').write_text(json.dumps(dict(pieces=placements),indent=2))
plan=[];manifest=[]
for name,im in items.items():
 Image.new('RGBA',im.size).save(t/(name+'-canvas.png'));im.save(t/(name+'-expected.png'));rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   c=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==c:end+=1
   if c[3]:row.append([x,end-x,bytes(c).hex()])
   x=end
  rows.append(row)
 plan.append(dict(name=name,w=im.width,h=im.height,rows=rows));manifest.append(dict(name=name,size=im.size,bounds=im.getbbox(),colors=len({c for c in im.get_flattened_data() if c[3]})))
 meta=(p/'Assets/_Game/Art/EnchantedForestStaged/Stage03/Lantern_Body.png.meta').read_text();meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/EnchantedForestStaged/Stage05/'+name).hex,meta,flags=re.M);meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/EnchantedForestStaged/Stage05/Sprite/'+name).hex,meta,flags=re.M);(a/(name+'.png.meta')).write_text(meta)
data=json.dumps(plan,separators=(',',':'));(r/'Scripts/NativePixelPlan.json').write_text(data)
js='var pc=app.pixelColor;var items='+data+';\n'+'''items.forEach(function(a){var s=app.open('.utmp/enchanted-stage05/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' native dressing';s.commit();s.saveAs('ArtSource/EnchantedForestStaged/Stage05/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/EnchantedForestStaged/Stage05/'+a.name+'.png',true);});console.log('STAGE05_NATIVE_COMPLETE');'''
(r/'Scripts/AuthorNative.js').write_text(js);(r/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Scripts/ReopenNative.js').write_text('try{'+json.dumps(list(items))+".forEach(function(n){app.open('ArtSource/EnchantedForestStaged/Stage05/Native/'+n+'.aseprite').sprite.saveAs('.utmp/enchanted-stage05/reopened/'+n+'.png',true);});console.log('STAGE05_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Diagnostics/Selection.json').write_text(json.dumps(dict(cost=20,jobs=['a580e155-e745-40f2-b768-909dc5ea59f2','fcbf1348-144e-4d58-876d-6bce3a39e0db'],selected=['Narrow_00','Narrow_13','LargeStudy_00 mushroom cutouts'],palette_maps=maps,mushroom_palette=warm,modules=manifest,notes='Large study used only for complete small mushroom cutouts. All roots are native48x72, no resampling. Side dressing is behind the approved board frame.'),indent=2))
preview=Image.open(p/'ArtSource/EnchantedForestStaged/Stage04/Previews/Unity-Stage04-720x1360.png').convert('RGBA')
for q in placements:
 im=items[q['file']];preview.alpha_composite(im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST),(q['x']*2,q['y']*2))
preview.save(r/'Previews/Layout-Study.png')
sheet=Image.new('RGBA',(720,700),'#22332a');d=ImageDraw.Draw(sheet)
for i,(name,im)in enumerate(items.items()):
 x=(i%3)*240;y=(i//3)*350;d.text((x+6,y+4),name,fill='white');sheet.alpha_composite(im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST),(x+6,y+28));d.text((x+112,y+28),str(im.size),fill='white')
sheet.save(r/'Previews/Modules-2x.png');print('Prepared',len(items),'modules:',[(n,im.size)for n,im in items.items()])
