from pathlib import Path
from PIL import Image
import json,uuid,re,hashlib,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
R=F/'ArtSource/ForestSurroundRevision/PolishStage01'
A=F/'Assets/_Game/Art/ForestSurroundRevision/PolishStage01'
T=F/'.utmp/forest-separator'
A.mkdir(parents=True,exist_ok=True);(T/'reopened').mkdir(parents=True,exist_ok=True)
config=json.loads((R/'Scripts/Selection.json').read_text())
records=[];plan=[]
for q in config['pieces']:
 original=Image.open(R/'Raw'/q['source']).convert('RGBA')
 source=Image.new('RGBA',(40,16))
 for y,sy in enumerate(q['source_rows']):
  for x,sx in enumerate(q['source_columns']):source.putpixel((x,y),original.getpixel((sx,sy)))
 palette=q['color_map']
 for y in range(16):
  for x in range(40):
   c=source.getpixel((x,y))
   if c[3]:source.putpixel((x,y),tuple(bytes.fromhex(palette[bytes(c[:3]).hex()]))+(255,))
 im=Image.new('RGBA',source.size,tuple(bytes.fromhex('171e1cff')));im.alpha_composite(source)
 for x,y,h in q.get('edits',[]):im.putpixel((x,y),tuple(bytes.fromhex(h)))
 name=q['name'];im.save(T/(name+'-expected.png'));Image.new('RGBA',im.size).save(T/(name+'-canvas.png'))
 rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   color=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==color:end+=1
   row.append([x,end-x,bytes(color).hex()]);x=end
  rows.append(row)
 plan.append(dict(name=name,rows=rows))
 changes=[[x,y,bytes(im.getpixel((x,y))).hex()]for y in range(16)for x in range(40)if im.getpixel((x,y))!=source.getpixel((x,y))]
 records.append(dict(name=name,source=q['source'],source_sha256=hashlib.sha256((R/'Raw'/q['source']).read_bytes()).hexdigest(),source_rows=q['source_rows'],source_columns=q['source_columns'],color_map=palette,native_changes=changes,reason=q['reason'],colors=len(set(im.get_flattened_data()))))
 meta=(F/'Assets/_Game/Art/EnchantedForestStaged/Stage03/Lantern_Body.png.meta').read_text()
 meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestSeparator/'+name).hex,meta,flags=re.M)
 meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestSeparator/Sprite/'+name).hex,meta,flags=re.M)
 (A/(name+'.png.meta')).write_text(meta)
data=json.dumps(plan,separators=(',',':'));(R/'Scripts/NativePixelPlan.json').write_text(data)
js='var pc=app.pixelColor;var items='+data+';\n'+"""items.forEach(function(a){var s=app.open('.utmp/forest-separator/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' native cleanup';s.commit();s.saveAs('ArtSource/ForestSurroundRevision/PolishStage01/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/ForestSurroundRevision/PolishStage01/'+a.name+'.png',true);});console.log('FOREST_SEPARATOR_NATIVE_COMPLETE');"""
(R/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(R/'Scripts/ReopenNative.js').write_text('try{'+json.dumps([q['name']for q in plan])+".forEach(function(n){app.open('ArtSource/ForestSurroundRevision/PolishStage01/Native/'+n+'.aseprite').sprite.saveAs('.utmp/forest-separator/reopened/'+n+'.png',true);});console.log('FOREST_SEPARATOR_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(R/'Diagnostics/NativeCleanup.json').write_text(json.dumps(records,indent=2))
shutil.copy2(__file__,R/'Scripts/PrepareNative.py')
print([(q['name'],q['colors'],len(q['native_changes']))for q in records])
