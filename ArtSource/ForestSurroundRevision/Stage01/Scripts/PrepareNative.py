from pathlib import Path
from PIL import Image,ImageDraw
from collections import Counter
import json,uuid,re,hashlib
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/ForestSurroundRevision/Stage01';a=p/'Assets/_Game/Art/ForestSurroundRevision/Stage01';t=p/'.utmp/forest-surround-stage01'
t.mkdir(exist_ok=True,parents=True);(t/'reopened').mkdir(exist_ok=True)
# Explicit material ramps: merge the many nearly identical generated browns.
groups={
'0c1014':'020101 080705 050101 150e0c 030303 020301 010000 0a0605 140d0b 030101 090605 160f0c 050201 070605 140d0a',
'241e1a':'281c18 221716 301f17 2e221f 261a17 2a1e1a 241814 231815 2a1f1c',
'3b2c20':'432f22 34251d 38271e 403025 452e1d 3a271c 432c1b 34241c 423226',
'563c26':'63472d 593c25 533c2b 5c4028 65482e 513825 553921 4f3929 5e422b 68482b 6a4a2d 5e432c 543a27',
'765131':'6e4d2e 8a653c 7a5835 8e673c 6f4e2f 835f38 765331 825e39 8d663c 775635 8c663c',
'997344':'966e40 a47a4a b18651 966e42 a57c4c b28753 987042 af8551 a37846 b08552 997042',
'b08a57':'bb9058 c29459 c09358 c4965c bc9057 c2955b',
'233f2b':'37472a 3b512a',
'4c7037':'4e6328 595333',
'718b45':'637636 7e7a46 677638 627535 7e7c48 657637 768427'}
mapping={c:tuple(bytes.fromhex(dst))+(255,)for dst,colors in groups.items()for c in colors.split()}
outline=tuple(bytes.fromhex('0c1014'))+(255,);wood=tuple(bytes.fromhex('765131'))+(255,);light=tuple(bytes.fromhex('997344'))+(255,)
items={};records=[]
for name,index in [('Root_A',0),('Root_B',2),('Root_C',3)]:
 raw=Image.open(r/'Raw'/f'Root_{index}.png').convert('RGBA');im=Image.new('RGBA',(84,164));ox=oy=10
 for y in range(raw.height):
  for x in range(raw.width):
   c=raw.getpixel((x,y))
   if c[3]:im.putpixel((x+ox,y+oy),mapping[bytes(c[:3]).hex()])
 # The raw studies touch their vertical canvas edges. Extend the terminal wood
 # into short tapered outlined caps; never merely pad an amputated square end.
 for y,direction in [(0,-1),(143,1)]:
  occupied=[x for x in range(64) if raw.getpixel((x,y))[3]];lo,hi=min(occupied),max(occupied)
  n=1
  while hi-lo>=1:
   lo+=1;hi-=1
   if hi<lo:break
   for x in range(lo,hi+1):im.putpixel((x+ox,y+oy+direction*n),outline if x in [lo,hi] else (light if x<(lo+hi)/2 else wood))
   n+=1
  im.putpixel((round((lo+hi)/2)+ox,y+oy+direction*n),outline)
 bounds=im.getbbox();im=im.crop(bounds);result=Image.new('RGBA',(im.width+4,im.height+4));result.alpha_composite(im,(2,2));items[name]=result
 result.save(t/(name+'-expected.png'));Image.new('RGBA',result.size).save(t/(name+'-canvas.png'))
 records.append(dict(name=name,raw=f'Root_{index}.png',size=result.size,colors=len(set(c for c in result.get_flattened_data()if c[3])),repair='Native tapered top and bottom tip extensions; source body retained exactly apart from material recolor',raw_offset=[ox-bounds[0]+2,oy-bounds[1]+2]))
 meta=(p/'Assets/_Game/Art/EnchantedForestStaged/Stage03/Lantern_Body.png.meta').read_text();meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestSurround/Stage01/'+name).hex,meta,flags=re.M);meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestSurround/Stage01/Sprite/'+name).hex,meta,flags=re.M);(a/(name+'.png.meta')).write_text(meta)
placements=[dict(name='Left upper root',file='Root_A',x=5,y=244),dict(name='Left lower root',file='Root_B',x=0,y=380),dict(name='Right upper root',file='Root_C',x=360-items['Root_C'].width,y=244),dict(name='Right lower root',file='Root_A',x=360-items['Root_A'].width-4,y=380)]
for q in placements:assert q['y']+items[q['file']].height<=540
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
js='var pc=app.pixelColor;var items='+data+';\n'+'''items.forEach(function(a){var s=app.open('.utmp/forest-surround-stage01/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' native bark and cap repair';s.commit();s.saveAs('ArtSource/ForestSurroundRevision/Stage01/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/ForestSurroundRevision/Stage01/'+a.name+'.png',true);});console.log('FOREST_ROOT_NATIVE_COMPLETE');'''
(r/'Scripts/AuthorNative.js').write_text(js);(r/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Scripts/ReopenNative.js').write_text('try{'+json.dumps(list(items))+".forEach(function(n){app.open('ArtSource/ForestSurroundRevision/Stage01/Native/'+n+'.aseprite').sprite.saveAs('.utmp/forest-surround-stage01/reopened/'+n+'.png',true);});console.log('FOREST_ROOT_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Diagnostics/Selection.json').write_text(json.dumps(dict(modules=records,palette=groups,rejected={'Root_1':'Busier multiple junctions and clipped horizontal offshoot; prefer broad simpler curves.'},job='ea44142f-325e-425d-a0cb-b998eff12784',cost_generations=10),indent=2))
sheet=Image.new('RGBA',(720,690),'#21302a');d=ImageDraw.Draw(sheet)
for i,(name,im)in enumerate(items.items()):
 d.text((i*240+8,8),name+' native 1x / 3x',fill='white');sheet.alpha_composite(im,(i*240+8,28));sheet.alpha_composite(im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST),(i*240+8,200))
sheet.save(r/'Previews/NativeRoots.png');print(records)
