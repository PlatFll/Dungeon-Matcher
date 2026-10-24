from pathlib import Path
from PIL import Image,ImageDraw
import json,re,uuid,hashlib
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/EnchantedForestStaged/Stage04';a=p/'Assets/_Game/Art/EnchantedForestStaged/Stage04';t=p/'.utmp/enchanted-stage04'
t.mkdir(exist_ok=True);(t/'reopened').mkdir(exist_ok=True)
raw=Image.open(r/'Raw/Frame_02.png').convert('RGBA');rgba=lambda h:tuple(bytes.fromhex(h))+(255,)
palette={'050e10':'0c1014','050b0a':'0c1014','0a140f':'0c1014','1e1912':'0c1014','2f2119':'3b2c20','38281c':'3b2c20','493323':'563c26','583e28':'563c26','634d33':'765131','77593c':'765131','7a704e':'9a6d41','182a1c':'233f2b','2b402a':'2b402a','354e2e':'3c5f33','455c31':'4c7037','526933':'52733a','617438':'65863e','74833f':'7a9845'}
clean=Image.new('RGBA',raw.size)
for y in range(128):
 for x in range(128):
  c=raw.getpixel((x,y))
  if c[3]:clean.putpixel((x,y),rgba(palette[bytes(c[:3]).hex()]))
clean.save(r/'Exports/Cleaned_Source.png')
# The source's material ramps are placed in the existing 11px rails and 23px corners.
# Rail endpoints and corner ports are exact; no resizing of generated art or board cells.
O='0c1014';D='3b2c20';S='563c26';B='765131';L='9a6d41'
ramps={'Top':[O,L,L,B,B,B,S,S,D,D,O],'Left':[O,L,B,B,B,S,S,S,D,D,O], 'Right':[O,D,S,S,B,B,S,S,D,D,O], 'Bottom':[O,D,D,S,S,B,B,B,S,D,O]}
items={}
for side,ramp in ramps.items():
 hor=side in ['Top','Bottom'];im=Image.new('RGBA',(32,11) if hor else (11,32));d=ImageDraw.Draw(im)
 for k,c in enumerate(ramp):
  z=10-k if side in ['Bottom','Right'] else k
  d.line((0,z,31,z) if hor else (z,0,z,31),fill=rgba(c))
 # Two connected grain marks leave all four ends unmodified for seamless tiling.
 d.line((9,5,22,5) if hor else (5,9,5,22),fill=rgba(S))
 if side in ['Top','Left']:d.line((13,4,19,4) if hor else (4,13,4,19),fill=rgba(L))
 items['Board_Rail_'+side]=im
base=Image.new('RGBA',(70,70))
for y in range(6,64):
 for x in range(6,64):
  ds={'Top':y-6,'Left':x-6,'Bottom':63-y,'Right':63-x};side=min(ds,key=ds.get);v=ds[side]
  if v<=10:
   c=ramps[side][v]
   if 2<=v<=8 and ((ds['Top']==ds['Left']) or(ds['Top']==ds['Right'])):c=S
   if 2<=v<=8 and ((ds['Bottom']==ds['Left']) or(ds['Bottom']==ds['Right'])):c=D
   base.putpixel((x,y),rgba(c))
for side,b in [('TL',(0,0,23,23)),('TR',(47,0,70,23)),('BL',(0,47,23,70)),('BR',(47,47,70,70))]:items['Board_Corner_'+side]=base.crop(b)
# Extract native green material with its immediately adjacent dark contour.
greens=set(['182a1c','2b402a','354e2e','455c31','526933','617438','74833f']);darks=set(['050e10','050b0a','0a140f'])
mask={(x,y) for y in range(128) for x in range(128) if raw.getpixel((x,y))[3] and bytes(raw.getpixel((x,y))[:3]).hex() in greens}
expanded=set(mask)
for x,y in mask:
 for dx,dy in [(1,0),(-1,0),(0,1),(0,-1)]:
  xx,yy=x+dx,y+dy
  if 0<=xx<128 and 0<=yy<128 and raw.getpixel((xx,yy))[3] and bytes(raw.getpixel((xx,yy))[:3]).hex() in darks:expanded.add((xx,yy))
growth=Image.new('RGBA',(128,128))
for xy in expanded:growth.putpixel(xy,clean.getpixel(xy))
placements=[];extractions=[]
spec=[('TL',(0,0,52,60),0,1,-25,-6),('TR',(76,0,128,64),1,1,-21,-6),('BL',(0,90,56,128),0,0,-31,-32),('BR',(78,90,128,128),1,0,-19,-32)]
for side,b,ax,ay,x,y in spec:
 im=growth.crop(b)
 # Omit complete inward looping tendrils, never shrink leaves into the tile opening.
 for yy in range(im.height):
  for xx in range(im.width):
   sx,sy=xx+b[0],yy+b[1]
   omit=(side=='TL' and sx>=40 and sy>=18) or(side=='TR' and sx<82 and sy>=23) or(side=='BL' and sx>=48 and sy<105) or(side=='BR' and sx<81 and sy<105)
   if omit:im.putpixel((xx,yy),(0,0,0,0))
 # Discard only disconnected one-to-three-pixel extraction specks in the new growth.
 seen=set()
 for yy in range(im.height):
  for xx in range(im.width):
   if (xx,yy) in seen or not im.getpixel((xx,yy))[3]:continue
   todo=[(xx,yy)];seen.add((xx,yy));component=[]
   while todo:
    px,py=todo.pop();component.append((px,py))
    for dx in [-1,0,1]:
     for dy in [-1,0,1]:
      q=(px+dx,py+dy)
      if 0<=q[0]<im.width and 0<=q[1]<im.height and q not in seen and im.getpixel(q)[3]:seen.add(q);todo.append(q)
   if len(component)<=3:
    for q in component:im.putpixel(q,(0,0,0,0))
 padded=Image.new('RGBA',(im.width+4,im.height+4));padded.alpha_composite(im,(2,2));name='Board_Ivy_'+side;items[name]=padded
 placements.append(dict(name=name,file=name,anchorX=ax,anchorY=ay,x=x-2,y=y-2));extractions.append(dict(name=name,source_crop=b,padding=2))
# Reuse the complete approved winding ivy so side growth is connected, not pasted-on clumps.
items['Board_Side_Swag']=Image.open(p/'Assets/_Game/Art/EnchantedForestStaged/Stage02/FoliageRevision/Ivy_LeftSwag.png').convert('RGBA')
for name,ax,x,y in [('Left connected ivy',0,-24,52),('Right connected ivy',1,-19,52)]:placements.append(dict(name=name,file='Board_Side_Swag',anchorX=ax,anchorY=1,x=x,y=y))
(r/'Scripts/Placements.json').write_text(json.dumps(dict(pieces=placements),indent=2))
# Compose at the shared native frame geometry, including padded outward decoration.
preview=Image.new('RGBA',(360,352),'#15211d');previewdraw=ImageDraw.Draw(preview)
ox,oy=35,24;fw,fh=290,290
for side in ['Top','Bottom']:
 rail=items['Board_Rail_'+side];strip=Image.new('RGBA',(268,11))
 for x in range(0,268,32):strip.alpha_composite(rail,(x,0))
 preview.alpha_composite(strip,(ox+11,oy+(6 if side=='Top' else fh-17)))
for side in ['Left','Right']:
 rail=items['Board_Rail_'+side];strip=Image.new('RGBA',(11,268))
 for y in range(0,268,32):strip.alpha_composite(rail,(0,y))
 preview.alpha_composite(strip,(ox+(6 if side=='Left' else fw-17),oy+11))
for side,x,y in [('TL',0,0),('TR',fw-23,0),('BL',0,fh-23),('BR',fw-23,fh-23)]:preview.alpha_composite(items['Board_Corner_'+side],(ox+x,oy+y))
decor=Image.new('RGBA',preview.size)
for q in placements:decor.alpha_composite(items[q['file']],(int(ox+q['anchorX']*fw+q['x']),oy+(0 if q['anchorY']==1 else fh)+q['y']))
# This assertion is about opaque art, so transparent canvas padding is unconstrained.
assert decor.crop((ox+17,oy+17,ox+273,oy+273)).getbbox() is None,'Foliage overlaps shared board opening'
preview.alpha_composite(decor);preview.save(r/'Previews/Frame-Native.png');preview.resize((720,704),Image.Resampling.NEAREST).save(r/'Previews/Frame-2x.png')
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
 meta=(p/'Assets/_Game/Art/EnchantedForestStaged/Stage03/Lantern_Body.png.meta').read_text();meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/EnchantedForestStaged/Stage04/'+name).hex,meta,flags=re.M);meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/EnchantedForestStaged/Stage04/Sprite/'+name).hex,meta,flags=re.M);(a/(name+'.png.meta')).write_text(meta)
data=json.dumps(plan,separators=(',',':'));(r/'Scripts/NativePixelPlan.json').write_text(data)
js='var pc=app.pixelColor;var items='+data+';\n'+'''items.forEach(function(a){var s=app.open('.utmp/enchanted-stage04/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' native board';s.commit();s.saveAs('ArtSource/EnchantedForestStaged/Stage04/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/EnchantedForestStaged/Stage04/'+a.name+'.png',true);});console.log('STAGE04_NATIVE_COMPLETE');'''
(r/'Scripts/AuthorNative.js').write_text(js);(r/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Scripts/ReopenNative.js').write_text('try{'+json.dumps(list(items))+".forEach(function(n){app.open('ArtSource/EnchantedForestStaged/Stage04/Native/'+n+'.aseprite').sprite.saveAs('.utmp/enchanted-stage04/reopened/'+n+'.png',true);});console.log('STAGE04_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Diagnostics/Selection.json').write_text(json.dumps(dict(job='e1a19a4d-2393-4fa0-b238-c729f37afaac',candidate=2,source_sha256=hashlib.sha256((r/'Raw/Frame_02.png').read_bytes()).hexdigest(),cost=10,palette_map=palette,native_wood='11px broad-grain rails and 23px mitred corners match existing frame ports',foliage_extractions=extractions,shared_opening=256,shared_frame_target=290,review_scale=2,modules=manifest),indent=2))
print('Prepared',len(items),'native modules; zero foliage pixels in the unchanged 256x256 native opening.')
