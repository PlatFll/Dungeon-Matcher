from pathlib import Path
from PIL import Image,ImageDraw
from collections import Counter
import json,re,uuid,hashlib
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/ForestSurroundRevision/UnifiedCorners';a=p/'Assets/_Game/Art/ForestSurroundRevision/UnifiedCorners';t=p/'.utmp/forest-unified-corners'
t.mkdir(exist_ok=True,parents=True);(t/'reopened').mkdir(exist_ok=True)
config=json.loads((r/'Scripts/SelectionConfig.json').read_text())
groups=[{tuple(bytes.fromhex(h))+(255,) for h in values} for values in [
 ['241e1a','3b2c20','563c26','765131','997344','b08a57'],
 ['23313a','314651','49616d','739096']]]
items={};records=[]
for q in config['assets']:
 raw=Image.open(r/'Raw'/q['raw']).convert('RGBA')
 im=raw.crop(q.get('crop',[0,0,raw.width,raw.height]))
 if 'prepad' in q:
  left,top,right,bottom=q['prepad'];expanded=Image.new('RGBA',(im.width+left+right,im.height+top+bottom));expanded.alpha_composite(im,(left,top));im=expanded
 before=im.copy();changes=[]
 assert set(im.getchannel('A').get_flattened_data())<={0,255}
 # Merge only isolated interior wood/rock pixels with three equal neighbors.
 # Preserve outlines, material boundaries, ridges and the original silhouette.
 for y in range(1,im.height-1):
  for x in range(1,im.width-1):
   c=before.getpixel((x,y));group=next((g for g in groups if c in g),None)
   if group is None:continue
   ns=[before.getpixel(pt)for pt in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)]]
   tone,n=Counter(ns).most_common(1)[0]
   if n>=3 and tone in group and tone!=c:
    im.putpixel((x,y),tone);changes.append([x,y,bytes(c).hex(),bytes(tone).hex()])
 for x,y,h in q.get('edits',[]):
  old=im.getpixel((x,y));new=tuple(bytes.fromhex(h));im.putpixel((x,y),new);changes.append([x,y,bytes(old).hex(),h])
 bounds=im.getbbox();im=im.crop(bounds);out=Image.new('RGBA',(im.width+4,im.height+4));out.alpha_composite(im,(2,2))
 name=q['name'];items[name]=out;Image.new('RGBA',out.size).save(t/(name+'-canvas.png'));out.save(t/(name+'-expected.png'))
 rec=dict(name=name,raw=q['raw'],raw_sha256=hashlib.sha256((r/'Raw'/q['raw']).read_bytes()).hexdigest(),source_crop=q.get('crop'),prepad=q.get('prepad'),trim=bounds,dimensions=out.size,palette=sorted({bytes(c[:3]).hex()for c in out.get_flattened_data()if c[3]}),native_edits=changes);records.append(rec)
 meta=(p/'Assets/_Game/Art/EnchantedForestStaged/Stage03/Lantern_Body.png.meta').read_text()
 meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestUnified/'+name).hex,meta,flags=re.M)
 meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestUnified/Sprite/'+name).hex,meta,flags=re.M)
 (a/(name+'.png.meta')).write_text(meta)
plan=[]
for name,im in items.items():
 rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   c=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==c:end+=1
   if c[3]:row.append([x,end-x,bytes(c).hex()])
   x=end
  rows.append(row)
 plan.append(dict(name=name,rows=rows))
data=json.dumps(plan,separators=(',',':'));(r/'Scripts/NativePixelPlan.json').write_text(data)
js='var pc=app.pixelColor;var items='+data+';\n'+"""items.forEach(function(a){var s=app.open('.utmp/forest-unified-corners/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' native cleanup';s.commit();s.saveAs('ArtSource/ForestSurroundRevision/UnifiedCorners/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/ForestSurroundRevision/UnifiedCorners/'+a.name+'.png',true);});console.log('FOREST_UNIFIED_NATIVE_COMPLETE');"""
(r/'Scripts/AuthorNative.js').write_text(js);(r/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Scripts/ReopenNative.js').write_text('try{'+json.dumps(list(items))+".forEach(function(n){app.open('ArtSource/ForestSurroundRevision/UnifiedCorners/Native/'+n+'.aseprite').sprite.saveAs('.utmp/forest-unified-corners/reopened/'+n+'.png',true);});console.log('FOREST_UNIFIED_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Diagnostics/Selection.json').write_text(json.dumps(dict(modules=records,judgment=config['judgment']),indent=2))
print([(q['name'],q['dimensions'],len(q['palette']),len(q['native_edits']))for q in records])
