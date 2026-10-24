from pathlib import Path
from PIL import Image,ImageDraw
import json,uuid,re,hashlib
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/EnchantedForestStaged/Stage03';art=p/'Assets/_Game/Art/EnchantedForestStaged/Stage03';t=p/'.utmp/enchanted-stage03'
t.mkdir(parents=True,exist_ok=True);(t/'reopened').mkdir(exist_ok=True)
rgba=lambda h:tuple(bytes.fromhex(h))+ (255,)
raw=Image.open(r/'Raw/Lantern_02.png').convert('RGBA');lantern=raw.copy()
for y in range(lantern.height):
 for x in range(lantern.width):
  if lantern.getpixel((x,y))[3]==0:lantern.putpixel((x,y),(0,0,0,0))
mapping={'020101':'0c1014','010000':'0c1014','030000':'0c1014','170f0a':'0c1014','594332':'563c26','43322a':'3b2c20','402917':'3b2c20','6b553e':'765131','876c47':'9a6d41','30241d':'3b2c20','7b6243':'765131','967d4f':'9a6d41','ad8d53':'bb9061'}
for y in range(raw.height):
 for x in range(raw.width):
  c=raw.getpixel((x,y));h=bytes(c[:3]).hex()
  if c[3] and h in mapping:lantern.putpixel((x,y),rgba(mapping[h]))
# The single hanging leaf keeps its source silhouette but uses the approved olive ramp.
for y in range(16,21):
 for x in range(13,21):
  c=lantern.getpixel((x,y))
  if c[3] and c[:3]!=(12,16,20):lantern.putpixel((x,y),rgba('65863e' if c[0]>=118 else '4c7037'))
items={n:Image.new('RGBA',lantern.size) for n in ['Lantern_Mount','Lantern_Body','Lantern_Light']}
glass={rgba(c) for c in ['fdc555','ec9f35','f7ebc5']}
for y in range(lantern.height):
 for x in range(lantern.width):
  c=lantern.getpixel((x,y))
  if not c[3]:continue
  if y<28 or x<18:items['Lantern_Mount'].putpixel((x,y),c)
  else:
   items['Lantern_Body'].putpixel((x,y),rgba('9a6d41') if c in glass else c)
   if c in glass:items['Lantern_Light'].putpixel((x,y),c)
lantern.save(t/'Lantern-expected-composite.png')
rawr=Image.open(r/'Raw/Rune_15.png').convert('RGBA');rune=rawr.copy()
for y in range(rune.height):
 for x in range(rune.width):
  if rune.getpixel((x,y))[3]==0:rune.putpixel((x,y),(0,0,0,0))
for y in range(18,rune.height):
 for x in range(rune.width):
  if rune.getpixel((x,y))[:3]==(187,144,97):rune.putpixel((x,y),rgba('9a6d41'))
items['Rune_Plaque']=rune.copy();items['Rune_Glyph']=Image.new('RGBA',rune.size)
for y in range(rune.height):
 for x in range(rune.width):
  c=rune.getpixel((x,y))
  if c[3] and c[:3] in [(85,166,162),(84,241,236)]:
   items['Rune_Plaque'].putpixel((x,y),rgba('235b5c'));items['Rune_Glyph'].putpixel((x,y),c)
rune.save(t/'Rune-expected-composite.png')
core=Image.open(p/'Assets/_Game/Art/EnchantedForestStaged/Stage01/Wisp_Near.png').convert('RGBA');items['Wisp_Core']=core
# Hard-pixel, two-tone emission skirt, not filtered alpha or a blur. Existing core stays exact.
occupied={(x,y) for y in range(core.height) for x in range(core.width) if core.getpixel((x,y))[3]};near=set(occupied)
for x,y in occupied:
 for dx,dy in [(-1,0),(1,0),(0,-1),(0,1)]:near.add((x+dx,y+dy))
far=set(near)
for x,y in near:
 for dx,dy in [(-1,0),(1,0),(0,-1),(0,1)]:far.add((x+dx,y+dy))
aura=Image.new('RGBA',core.size)
for x,y in far:
 if 0<=x<core.width and 0<=y<core.height:aura.putpixel((x,y),rgba('55a6a2' if (x,y) in near else '235b5c'))
items['Wisp_Aura']=aura
im=Image.new('RGBA',(8,8));d=ImageDraw.Draw(im)
d.line((3,1,3,5),fill=rgba('9a6d41'));d.line((1,3,5,3),fill=rgba('9a6d41'));d.line((3,2,3,4),fill=rgba('fdc555'));d.line((2,3,4,3),fill=rgba('fdc555'));d.point((3,3),fill=rgba('f7ebc5'));items['Firefly_Mote']=im
vinesource=p/'Assets/_Game/Art/EnchantedForestStaged/Stage02/FoliageRevision/Ivy_CrownRight.png'
cut=Image.open(vinesource).convert('RGBA').crop((34,31,68,74))
# Keep the complete lower leaves; remove only the severed end of the preceding leaf.
for yy in range(4):
 for xx in range(21 if yy<3 else 24,34):cut.putpixel((xx,yy),(0,0,0,0))
vine=Image.new('RGBA',(38,47));vine.alpha_composite(cut,(2,2));items['Hanging_Leaf_Strand']=vine
plan=[];manifest=[]
for name,im in items.items():
 Image.new('RGBA',im.size).save(t/(name+'-canvas.png'));im.save(t/(name+'-expected.png'))
 rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   c=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==c:end+=1
   if c[3]:row.append([x,end-x,bytes(c).hex()])
   x=end
  rows.append(row)
 plan.append(dict(name=name,w=im.width,h=im.height,rows=rows));manifest.append(dict(name=name,size=im.size,bounds=im.getbbox(),colors=len({c for c in im.get_flattened_data() if c[3]})))
 meta=(p/'Assets/_Game/Art/EnchantedForestStaged/Stage02/FoliageRevision/Ivy_CrownRight.png.meta').read_text()
 meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/EnchantedForestStaged/Stage03/'+name).hex,meta,flags=re.M)
 # Single-sprite imports must not carry the template's obsolete multi-sprite table.
 meta=re.sub(r'^  internalIDToNameTable:\n.*?(?=^  externalObjects:)', '  internalIDToNameTable: []\n',meta,flags=re.M|re.S)
 meta=re.sub(r'^    sprites:\n.*?(?=^    outline:)', '    sprites: []\n',meta,flags=re.M|re.S)
 meta=re.sub(r'^    nameFileIdTable:\n(?:      .*\n)*', '    nameFileIdTable: {}\n',meta,flags=re.M)
 meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/EnchantedForestStaged/Stage03/Sprite/'+name).hex,meta,flags=re.M)
 (art/(name+'.png.meta')).write_text(meta)
data=json.dumps(plan,separators=(',',':'));(r/'Scripts/NativePixelPlan.json').write_text(data)
js='var pc=app.pixelColor;var items='+data+';\n'+'''items.forEach(function(a){var s=app.open('.utmp/enchanted-stage03/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;
a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' native modular magic';s.commit();s.saveAs('ArtSource/EnchantedForestStaged/Stage03/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/EnchantedForestStaged/Stage03/'+a.name+'.png',true);});console.log('STAGE03_NATIVE_COMPLETE');'''
(r/'Scripts/AuthorNative.js').write_text(js);(r/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Scripts/ReopenNative.js').write_text('try{'+json.dumps(list(items))+".forEach(function(n){app.open('ArtSource/EnchantedForestStaged/Stage03/Native/'+n+'.aseprite').sprite.saveAs('.utmp/enchanted-stage03/reopened/'+n+'.png',true);});console.log('STAGE03_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();")
placements=[]
for n in ['Lantern_Mount','Lantern_Body','Lantern_Light']:placements.append(dict(name=n,file=n,x=99,y=50))
for n in ['Rune_Plaque','Rune_Glyph']:placements.append(dict(name=n,file=n,x=280,y=62))
for n in ['Wisp_Aura','Wisp_Core']:placements.append(dict(name=n,file=n,x=225,y=92))
placements += [dict(name='Warm firefly',file='Firefly_Mote',x=191,y=82),dict(name='Hanging leaves',file='Hanging_Leaf_Strand',x=316,y=49)]
(r/'Scripts/Placements.json').write_text(json.dumps(dict(pieces=placements),indent=2))
selection={'lantern':{'job':'687faea8-a616-4719-99a6-e37be543f19b','selected_index':2,'source_sha256':hashlib.sha256((r/'Raw/Lantern_02.png').read_bytes()).hexdigest(),'cleanup_map':mapping,'light_underlay':'#9a6d41','sway_hinge_png_top_left':[28,28]},'rune':{'job':'1c4721d2-0801-4e2c-8b1a-df015555ca4d','selected_index':15,'source_sha256':hashlib.sha256((r/'Raw/Rune_15.png').read_bytes()).hexdigest(),'cleanup':'plate highlight #bb9061 to #9a6d41 below row18; retain rope highlights; separate glyph over carved shadow #235b5c'},'wisp':'Stage01/Wisp_Near.png native core preserved, separate hard-pixel emission skirt','vine':{'source':str(vinesource.relative_to(p)),'crop':[34,31,68,74],'padding':2,'cleanup':'remove cropped end of preceding upper leaf; retain complete lower leaf shapes'},'mote':'8x8 native authoring using selected lantern glass ramp','generated_cost':20,'modules':manifest}
(r/'Diagnostics/Selection.json').write_text(json.dumps(selection,indent=2))
# Compact static study, every display enlargement nearest-neighbor only.
study=Image.new('RGBA',(252,90),(20,29,30,255));d=ImageDraw.Draw(study)
groups=[('Lantern',['Lantern_Mount','Lantern_Body','Lantern_Light'],(6,16)),('Rune',['Rune_Plaque','Rune_Glyph'],(64,16)),('Wisp',['Wisp_Aura','Wisp_Core'],(112,28)),('Mote',['Firefly_Mote'],(146,38)),('Leaf strand',['Hanging_Leaf_Strand'],(181,16))]
for label,names,xy in groups:
 for name in names:study.alpha_composite(items[name],xy)
 d.text((xy[0],2),label,fill='#dbe3ca')
study.save(r/'Previews/Set-Native.png');study.resize((1008,360),Image.Resampling.NEAREST).save(r/'Previews/Set-4x.png')
print(json.dumps(manifest))
