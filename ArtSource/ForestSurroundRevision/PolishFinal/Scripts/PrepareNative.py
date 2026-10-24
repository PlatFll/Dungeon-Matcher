from pathlib import Path
from PIL import Image
import json,re,uuid,hashlib,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/PolishFinal';A=F/'Assets/_Game/Art/ForestSurroundRevision/PolishFinal';T=F/'.utmp/forest-polish-final';A.mkdir(parents=True,exist_ok=True);(T/'reopened').mkdir(parents=True,exist_ok=True)
plan=[];stats=[]
names=['Corner_Left','Corner_Right','Sweep_Left','Sweep_Right','Vine','Vine_Right','Growth_Left','Growth_Right','Wisp']
Image.open(T/'Vine-plan.png').transpose(Image.Transpose.FLIP_LEFT_RIGHT).save(T/'Vine_Right-plan.png')
for name in names:
 im=Image.open(T/(name+'-plan.png')).convert('RGBA');Image.new('RGBA',im.size).save(T/(name+'-canvas.png'));rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   c=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==c:end+=1
   if c[3]:row.append([x,end-x,bytes(c).hex()])
   x=end
  rows.append(row)
 plan.append(dict(name=name,rows=rows))
 meta=(F/'Assets/_Game/Art/ForestSurroundRevision/PolishStage02/Root_Left.png.meta').read_text()
 meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestFinal/'+name).hex,meta,flags=re.M)
 meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestFinal/Sprite/'+name).hex,meta,flags=re.M)
 (A/(name+'.png.meta')).write_text(meta)
 stats.append(dict(name=name,dimensions=im.size,bounds=im.getbbox(),colors=len({c for c in im.get_flattened_data()if c[3]}),alpha=sorted(set(im.getchannel('A').get_flattened_data()))))
data=json.dumps(plan,separators=(',',':'));(R/'Scripts/NativePixelPlan.json').write_text(data+'\n')
js='var pc=app.pixelColor;var items='+data+';\n'+"""items.forEach(function(a){var s=app.open('.utmp/forest-polish-final/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' final native cleanup';s.commit();s.saveAs('ArtSource/ForestSurroundRevision/PolishFinal/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/ForestSurroundRevision/PolishFinal/'+a.name+'.png',true);});console.log('FOREST_FINAL_NATIVE_COMPLETE');"""
(R/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();\n")
(R/'Scripts/ReopenNative.js').write_text('try{'+json.dumps(names)+".forEach(function(n){app.open('ArtSource/ForestSurroundRevision/PolishFinal/Native/'+n+'.aseprite').sprite.saveAs('.utmp/forest-polish-final/reopened/'+n+'.png',true);});console.log('FOREST_FINAL_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();\n")
(R/'Diagnostics/NativeModules.json').write_text(json.dumps(stats,indent=2))
pieces=[dict(name='Left integrated corner',file='Corner_Left',x=0,y=500,stage=3),dict(name='Right integrated corner',file='Corner_Right',x=272,y=500,stage=3),dict(name='Left grounded root sweep',file='Sweep_Left',x=0,y=584,stage=4),dict(name='Right grounded root sweep',file='Sweep_Right',x=202,y=584,stage=4),dict(name='Left hanging vine',file='Vine',x=42,y=525,stage=4),dict(name='Right hanging vine',file='Vine_Right',x=246,y=525,stage=4),dict(name='Left board spirit',file='Wisp',x=5,y=350,stage=5),dict(name='Right board spirit',file='Wisp',x=330,y=435,stage=5),dict(name='Left lower spirit',file='Wisp',x=9,y=550,stage=5),dict(name='Right corner spirit',file='Wisp',x=328,y=615,stage=5)]
for q in pieces:
 if q['file'].startswith('Sweep_'):q['y']=588
 if q['file']=='Sweep_Right':q['x']=203
 if q['file'].startswith('Vine'):q['y']=525
pieces += [dict(name='Left rooted fern and flowers',file='Growth_Left',x=66,y=606,stage=4),dict(name='Right rooted fern and flowers',file='Growth_Right',x=254,y=606,stage=4)]
(R/'Scripts/Placements.json').write_text(json.dumps(dict(pieces=pieces),indent=2))
launcher=(F/'ArtSource/ForestSurroundRevision/PolishStage02/Scripts/RunNative.ps1').read_text().replace('PolishStage02','PolishFinal').replace('FOREST_ROOT_JOIN_NATIVE_COMPLETE','FOREST_FINAL_NATIVE_COMPLETE')
(R/'Scripts/RunNative.ps1').write_text(launcher)
if Path(__file__).resolve()!=(R/'Scripts/PrepareNative.py').resolve():shutil.copy2(__file__,R/'Scripts/PrepareNative.py')
print('Prepared nine native LibreSprite modules and twelve modular placements.')
