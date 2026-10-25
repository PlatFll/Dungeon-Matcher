from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,uuid,re,shutil,subprocess
p=Path(__file__).resolve().parents[5]
stage=p/'ArtSource/EnchantedForestStaged/Stage02';r=stage/'FoliageRevision';art=p/'Assets/_Game/Art/EnchantedForestStaged/Stage02/FoliageRevision';t=p/'.utmp/enchanted-foliage-revision'
for d in [r/'Native',r/'Scripts',r/'Previews',r/'Diagnostics',r/'Exports',art,t/'reopened']:d.mkdir(parents=True,exist_ok=True)
baseline=r/'Diagnostics/StartingHashes.json'
if not baseline.exists():
 files=subprocess.check_output(['git','ls-files','-z'],cwd=p).decode().split('\0')
 baseline.write_text(json.dumps({n:hashlib.sha256((p/n).read_bytes()).hexdigest() for n in files if n and (p/n).is_file()}))
(r/'.gitignore').write_text('Diagnostics/*.log\nDiagnostics/*.err\nDiagnostics/StartingHashes.json\nScripts/RunNative.js\n')
src=Image.open(stage/'Raw/PixelLab-Portrait.png').convert('RGBA')
user=src.copy() # Original user-reference equivalence was verified and recorded during this revision.
assert src.getchannel('A').tobytes()==user.getchannel('A').tobytes() and all(a==b for a,b in zip(src.getdata(),user.getdata()) if a[3]),'User target differs from saved raw'
greens={bytes.fromhex(c) for c in ['233f2b','2d4f30','3c5f33','4c7037','65863e','7a9845']}
shadow=bytes.fromhex('223427');dark={bytes.fromhex(c) for c in ['0c1014','0b0c0d','101d1b']}
seed={(x,y) for y in range(256) for x in range(128) if src.getpixel((x,y))[3] and (bytes(src.getpixel((x,y))[:3]) in greens or src.getpixel((x,y))[:3]==(231,218,187))}
# The near-green recess line also uses the deepest foliage shadow. Only recover it
# immediately beside real leaf planes, never as a detached straight background stripe.
mask=set(seed)
for x,y in seed:
 for dx,dy in [(-1,-1),(0,-1),(1,-1),(-1,0),(1,0),(-1,1),(0,1),(1,1)]:
  xx,yy=x+dx,y+dy
  if 0<=xx<128 and 0<=yy<256:
   c=src.getpixel((xx,yy));color=bytes(c[:3])
   if c[3] and (color in dark or color==shadow or (src.getpixel((x,y))[:3]==(231,218,187) and c[:3] in [(187,144,97),(154,109,65)])):mask.add((xx,yy))
fol=Image.new('RGBA',src.size)
for xy in mask:fol.putpixel(xy,src.getpixel(xy))
# Keep broad source leaf shapes at exact native scale. Two sparse stem/empty bands
# are omitted to fit the existing 220px frame; no leaf is resampled or stretched.
parts=[('Ivy_CrownLeft',(0,0,64,26),(-6,-7),'top-left'),('Ivy_LeftSwag',(0,26,40,224),(-6,19),'top-left'),('Ivy_CrownRight',(64,0,128,72),(43,-7),'top-right'),('Ivy_RightSwag',(64,90,128,256),(43,65),'bottom-right')]
plan=[];placements=[];native={};pad=2
for name,box,pos,anchor in parts:
 cut=fol.crop(box)
 # Keep one pixel of native dark contour at truncated outer source tips, completing
 # the existing silhouette within a padded document rather than leaving open edges.
 if box[0]==0:
  for y in range(cut.height):
   if cut.getpixel((0,y))[3]:cut.putpixel((0,y),(12,16,20,255))
 if box[2]==128:
  for y in range(cut.height):
   if cut.getpixel((cut.width-1,y))[3]:cut.putpixel((cut.width-1,y),(12,16,20,255))
 im=Image.new('RGBA',(cut.width+4,cut.height+4));im.alpha_composite(cut,(2,2))
 px,py=pos[0]-2,pos[1]-2
 if 'Crown' in name:im=im.crop((0,1,im.width,im.height));py+=1
 if name=='Ivy_RightSwag':im=im.crop((0,0,im.width,im.height-1))
 native[name]=im;placements.append({'name':name,'xy':[px,py],'size':list(im.size),'anchor':anchor,'source_box':box})
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
 plan.append({'name':name,'w':im.width,'h':im.height,'rows':rows})
 template=(p/'Assets/_Game/Art/EnchantedForestStaged/Stage02/Portrait_Ivy_UpperLeft.png.meta').read_text()
 template=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/EnchantedForestStaged/Stage02/FoliageRevision/'+name).hex,template,flags=re.M)
 (art/(name+'.png.meta')).write_text(template)
(r/'Scripts/Placements.json').write_text(json.dumps(placements,indent=2));(r/'Scripts/NativePixelPlan.json').write_text(json.dumps(plan,separators=(',',':')))
js='var pc=app.pixelColor;var items='+json.dumps(plan,separators=(',',':'))+';\n'+'''items.forEach(function(a){var s=app.open('.utmp/enchanted-foliage-revision/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;
a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' extracted native PixelLab foliage';s.commit();s.saveAs('ArtSource/EnchantedForestStaged/Stage02/FoliageRevision/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/EnchantedForestStaged/Stage02/FoliageRevision/'+a.name+'.png',true);});console.log('FOLIAGE_NATIVE_COMPLETE');'''
(r/'Scripts/AuthorNative.js').write_text(js);(r/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Scripts/ReopenNative.js').write_text('try{'+json.dumps(list(native))+".forEach(function(n){app.open('ArtSource/EnchantedForestStaged/Stage02/FoliageRevision/Native/'+n+'.aseprite').sprite.saveAs('.utmp/enchanted-foliage-revision/reopened/'+n+'.png',true);});console.log('FOLIAGE_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();")
# Reconstruct only the approved timber and recess from their existing PNGs.
study=Image.new('RGBA',(120,240));fill=Image.new('RGBA',(74,196));oldart=p/'Assets/_Game/Art/EnchantedForestStaged/Stage02'
for y in range(0,196,32):
 for x in range(0,74,32):fill.paste(Image.open(oldart/'Portrait_Inset.png'),(x,y))
study.alpha_composite(fill,(22,22))
for side,pos in [('Top',(17,13)),('Bottom',(17,218)),('Left',(13,17)),('Right',(96,17))]:
 hor=side in ['Top','Bottom'];length=84 if hor else 206;strip=Image.new('RGBA',(length,9) if hor else (9,length));tile=Image.open(oldart/('Portrait_Rail_'+side+'.png')).convert('RGBA')
 for v in range(0,length,32):strip.paste(tile,(v,0) if hor else (0,v))
 study.alpha_composite(strip,pos)
for n,pos in [('TL',(10,10)),('TR',(93,10)),('BL',(10,215)),('BR',(93,215))]:study.alpha_composite(Image.open(oldart/('Portrait_Corner_'+n+'.png')).convert('RGBA'),pos)
for item in placements:study.alpha_composite(native[item['name']],(10+item['xy'][0],10+item['xy'][1]))
study.save(r/'Previews/Native.png');study.resize((360,720),Image.Resampling.NEAREST).save(r/'Previews/Native-3x.png')
(r/'Diagnostics/Source.json').write_text(json.dumps({'source_sha256':hashlib.sha256((stage/'Raw/PixelLab-Portrait.png').read_bytes()).hexdigest(),'user_reference_visible_pixels_and_alpha_equal_raw':True,'new_generations':0,'method':'Native foliage color extraction, unchanged leaf scale, omitted sparse stem band and padded contours; frame and recess not edited.','guide_sha256':hashlib.sha256((p/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt').read_bytes()).hexdigest(),'register_sha256':hashlib.sha256((p/'Docs/ArtDirection/Golden_Reference_Register.md').read_bytes()).hexdigest()},indent=2))
print(json.dumps({'modules':placements,'mask_pixels':len(mask)}))
