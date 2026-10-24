from pathlib import Path
from PIL import Image,ImageDraw
from collections import Counter
import json,re,uuid
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/ForestSurroundRevision/Stage02';a=p/'Assets/_Game/Art/ForestSurroundRevision/Stage02';t=p/'.utmp/forest-surround-stage02'
t.mkdir(exist_ok=True,parents=True);(t/'reopened').mkdir(exist_ok=True)
def col(h):return tuple(bytes.fromhex(h))+(255,)
wood={col(h)for h in ['241e1a','3b2c20','563c26','765131','997344','b08a57']};outline=col('0c1014');mid=col('765131');light=col('997344')
items={};records=[]
for name,rawname in [('Lower_Left','Left_0'),('Lower_Right','Right_2')]:
 raw=Image.open(r/'Raw'/(rawname+'.png')).convert('RGBA');clean=raw.copy();changes=[]
 # Merge only isolated interior wood speckles surrounded on three sides by one
 # other wood tone. Long bark ridges, silhouettes, plants and stones are retained.
 for y in range(1,raw.height-1):
  for x in range(1,raw.width-1):
   c=raw.getpixel((x,y))
   if c not in wood:continue
   ns=[raw.getpixel(q)for q in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)]];tone,n=Counter(ns).most_common(1)[0]
   if n>=3 and tone in wood and tone!=c:clean.putpixel((x,y),tone);changes.append([x,y,bytes(c).hex(),bytes(tone).hex()])
 # Complete the generated flat-ended trunk tips in native pixels, with enough
 # canvas above them for an outlined taper. No original silhouette is removed.
 work=Image.new('RGBA',(116,168));ox=10;oy=28;work.alpha_composite(clean,(ox,oy));top=raw.getbbox()[1];xs=[x for x in range(96)if raw.getpixel((x,top))[3]];runs=[]
 for x in xs:
  if not runs or x>runs[-1][-1]+1:runs.append([x])
  else:runs[-1].append(x)
 caps=[]
 for run in runs:
  lo,hi=run[0],run[-1];length=0
  if name=='Lower_Left' and len(run)>=16:
   # Hook the broad trunk inward rather than forming a symmetric arrow tip.
   for step,(l,h) in enumerate([(11,26),(13,26),(15,27),(17,27),(19,27),(20,27),(22,26),(23,25),(24,25)],1):
    for x in range(l,h+1):work.putpixel((x+ox,top+oy-step),outline if x in [l,h] else (light if x<l+2 else mid))
   caps.append(dict(source_top_x=[run[0],run[-1]],extension=9,shape='inward curved taper'));continue
  for step in range(1,18):
   lo+=1;hi-=1
   if hi<lo:break
   for x in range(lo,hi+1):work.putpixel((x+ox,top+oy-step),outline if x in [lo,hi] else (light if x<(lo+hi)/2 else mid))
   length=step
  work.putpixel((round((lo+hi)/2)+ox,top+oy-length-1),outline);caps.append(dict(source_top_x=[run[0],run[-1]],extension=length+1))
 bounds=work.getbbox();im=work.crop(bounds);result=Image.new('RGBA',(im.width+4,im.height+4));result.alpha_composite(im,(2,2));items[name]=result
 Image.new('RGBA',result.size).save(t/(name+'-canvas.png'));result.save(t/(name+'-expected.png'))
 records.append(dict(name=name,raw=rawname+'.png',dimensions=result.size,opaque_colors=len({c for c in result.get_flattened_data()if c[3]}),wood_speckle_edits=changes,tip_caps=caps,source_body_offset=[ox-bounds[0]+2,oy-bounds[1]+2]))
 meta=(p/'Assets/_Game/Art/EnchantedForestStaged/Stage03/Lantern_Body.png.meta').read_text();meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestSurround/Stage02/'+name).hex,meta,flags=re.M);meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestSurround/Stage02/Sprite/'+name).hex,meta,flags=re.M);(a/(name+'.png.meta')).write_text(meta)
placements=[]
for name,im in items.items():
 q=dict(name=name+' root-and-stone anchor',file=name,x=0 if name.endswith('Left')else 360-im.width,y=680-im.height)
 assert q['y']>=540,(name,im.size);placements.append(q)
(r/'Scripts/Placements.json').write_text(json.dumps(dict(pieces=placements),indent=2))
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
js='var pc=app.pixelColor;var items='+data+';\n'+'''items.forEach(function(a){var s=app.open('.utmp/forest-surround-stage02/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' native root stone and mushroom cleanup';s.commit();s.saveAs('ArtSource/ForestSurroundRevision/Stage02/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/ForestSurroundRevision/Stage02/'+a.name+'.png',true);});console.log('FOREST_LOWER_NATIVE_COMPLETE');'''
(r/'Scripts/AuthorNative.js').write_text(js);(r/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Scripts/ReopenNative.js').write_text('try{'+json.dumps(list(items))+".forEach(function(n){app.open('ArtSource/ForestSurroundRevision/Stage02/Native/'+n+'.aseprite').sprite.saveAs('.utmp/forest-surround-stage02/reopened/'+n+'.png',true);});console.log('FOREST_LOWER_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Diagnostics/Selection.json').write_text(json.dumps(dict(modules=records,cost_generations=20,selected='Left 0: broad warm root with two stone masses. Right 2: organic crook, strong stone foot and visible red/pale mushroom accent.',unselected='Other variants retained as studies; Left 2/Right 3 have artificial pale top caps. No regeneration.',palette='25 named forest colors supplied to PixelLab; selected exports use 19 each.'),indent=2))
sheet=Image.new('RGBA',(720,560),'#21302a');d=ImageDraw.Draw(sheet)
for i,(name,im)in enumerate(items.items()):
 d.text((i*360+10,8),name+' native 1x / 3x',fill='white');sheet.alpha_composite(im,(i*360+10,30));sheet.alpha_composite(im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST),(i*360+10,175))
sheet.save(r/'Previews/NativeClusters.png');print([(m['name'],m['dimensions'],len(m['wood_speckle_edits']),m['tip_caps'])for m in records])
