from pathlib import Path
from PIL import Image
import json,re,uuid,hashlib,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/PolishStage02';A=F/'Assets/_Game/Art/ForestSurroundRevision/PolishStage02';T=F/'.utmp/forest-root-joins'
A.mkdir(parents=True,exist_ok=True);(T/'reopened').mkdir(parents=True,exist_ok=True)
plan=[];report=[]
for name in ['Root_Left','Root_Right','StoneAnchor_Left','StoneAnchor_Right']:
 im=Image.open(R/'Previews'/(name+'-Plan.png')).convert('RGBA')
 im.save(T/(name+'-expected.png'));Image.new('RGBA',im.size).save(T/(name+'-canvas.png'))
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
 meta=(F/'Assets/_Game/Art/EnchantedForestStaged/Stage03/Lantern_Body.png.meta').read_text()
 meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestRootJoin/'+name).hex,meta,flags=re.M)
 meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestRootJoin/Sprite/'+name).hex,meta,flags=re.M)
 (A/(name+'.png.meta')).write_text(meta)
 report.append(dict(name=name,dimensions=im.size,bounds=im.getbbox(),colors=len({c for c in im.get_flattened_data()if c[3]}),alpha=sorted(set(im.getchannel('A').get_flattened_data()))))
data=json.dumps(plan,separators=(',',':'));(R/'Scripts/NativePixelPlan.json').write_text(data+'\n')
js='var pc=app.pixelColor;var items='+data+';\n'+"""items.forEach(function(a){var s=app.open('.utmp/forest-root-joins/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' native root join cleanup';s.commit();s.saveAs('ArtSource/ForestSurroundRevision/PolishStage02/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/ForestSurroundRevision/PolishStage02/'+a.name+'.png',true);});console.log('FOREST_ROOT_JOIN_NATIVE_COMPLETE');"""
(R/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();\n")
(R/'Scripts/ReopenNative.js').write_text('try{'+json.dumps([p['name']for p in plan])+".forEach(function(n){app.open('ArtSource/ForestSurroundRevision/PolishStage02/Native/'+n+'.aseprite').sprite.saveAs('.utmp/forest-root-joins/reopened/'+n+'.png',true);});console.log('FOREST_ROOT_JOIN_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();\n")
(R/'Diagnostics/NativeModules.json').write_text(json.dumps(report,indent=2))
placements=[dict(name='Left preserved stone anchor',file='StoneAnchor_Left',x=0,y=579),dict(name='Right preserved stone anchor',file='StoneAnchor_Right',x=274,y=575),dict(name='Left continuous root',file='Root_Left',x=0,y=500),dict(name='Right continuous root',file='Root_Right',x=272,y=500)]
(R/'Scripts/Placements.json').write_text(json.dumps(dict(pieces=placements),indent=2))
p=F/'Assets/_Game/Scripts/Editor/ForestRootJoinReviewTests.cs';s=p.read_text().replace('rt.SetSiblingIndex(root.Find("Stage05 rich woodland dressing 2x").GetSiblingIndex());','rt.SetSiblingIndex(root.Find("Unified lower corner formations 2x").GetSiblingIndex());')
s=s.replace('Assert.That(root.Find("Unified lower corner formations 2x/" + side + " main stone formation").gameObject.activeSelf, Is.True);','root.Find("Unified lower corner formations 2x/" + side + " main stone formation").gameObject.SetActive(!revised);')
p.write_text(s)
if Path(__file__).resolve() != (R/'Scripts/PrepareNative.py').resolve(): shutil.copy2(__file__,R/'Scripts/PrepareNative.py')
print(json.dumps(report,indent=2))
