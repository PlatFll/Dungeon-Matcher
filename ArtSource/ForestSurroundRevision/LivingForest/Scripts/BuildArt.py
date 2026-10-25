from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
from collections import Counter
import json,math,shutil,uuid,re
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/LivingForest';A=F/'Assets/_Game/Art/ForestSurroundRevision/LivingForest';T=F/'.utmp/living-forest';W=Path(__file__).parent
A.mkdir(parents=True,exist_ok=True);(T/'reopened').mkdir(parents=True,exist_ok=True)
def rgba(h):return tuple(bytes.fromhex(h))+(255,)
series={};sources={}
def add(name,frames,source):
 series[name]=frames;sources[name]=source
 for i,im in enumerate(frames):im.save(T/(name+'_'+str(i).zfill(2)+'-plan.png'))
strip=Image.open(F/'Assets/_Game/Art/ForestSurroundRevision/PolishFinal/Corner_Right.png').convert('RGBA');clean=strip.copy()
mask=Image.new('L',strip.size);d=ImageDraw.Draw(mask);d.polygon([(72,110),(87,110),(87,129),(73,129),(73,121),(70,121),(70,115)],fill=255)
clean.paste((0,0,0,0),(0,0,88,180),mask)
add('Corner_Right_NoSprig',[clean],'Assets/_Game/Art/ForestSurroundRevision/PolishFinal/Corner_Right.png')
mask.save(R/'Diagnostics/RemovedSprigMask.png')
comparison=Image.new('RGBA',(88*2,180));comparison.alpha_composite(strip);comparison.alpha_composite(clean,(88,0));comparison.resize((704,720),Image.Resampling.NEAREST).save(R/'Previews/Sprig-BeforeAfter-4x.png')
palette=[rgba(h)for h in ['21636a','3bb8b5','a1ecdf','effff5']]
for name in ['Mote','Curl','Comet']:
 rawname='Seed' if name=='Mote' else name
 raw=Image.open(R/'Raw'/(rawname+'.png')).convert('RGBA');mapping={c:(0,0,0,0)if c[3]<128 else min(palette,key=lambda p:sum((p[k]-c[k])**2 for k in range(3)))for c in raw.get_flattened_data()}
 im=Image.new('RGBA',raw.size);im.putdata([mapping[c]for c in raw.get_flattened_data()]);b=im.getbbox();im=im.crop(b)
 # New native dimensions are content bounds plus generous transparent margins, never scaling.
 base=Image.new('RGBA',(im.width+8,im.height+8));base.alpha_composite(im,(4,4));frames=[]
 for shift in [0,1,0,-1]:
  frame=Image.new('RGBA',base.size)
  for y in range(base.height):
   ox=round(shift*max(0,1-y/(base.height*.65))) if name!='Mote' else 0
   for x in range(base.width):
    if base.getpixel((x,y))[3]:frame.putpixel((x+ox,y),base.getpixel((x,y)))
  if name=='Mote'and shift:
   # Sparkle cores alternate within their existing light cluster without adding noise.
   for y in range(frame.height):
    for x in range(frame.width):
     c=frame.getpixel((x,y))
     if c==palette[3] and (x+y)%3==0:frame.putpixel((x,y),palette[2])
  frames.append(frame)
 add('Wisp_'+name,frames,'Raw/'+rawname+'.png')
for name,p in [('Vine_Left','Assets/_Game/Art/ForestSurroundRevision/PolishFinal/Vine.png'),('Vine_Right','Assets/_Game/Art/ForestSurroundRevision/PolishFinal/Vine_Right.png'),('Upper_Leaves','Assets/_Game/Art/EnchantedForestStaged/Stage03/Hanging_Leaf_Strand.png')]:
 im=Image.open(F/p).convert('RGBA');base=Image.new('RGBA',(im.width+8,im.height+8));base.alpha_composite(im,(4,4));frames=[]
 # Stem stays fixed. Lower leaf tips bend at integer source-pixel increments.
 for amount in [0,1,2,-1,-2]:
  out=Image.new('RGBA',base.size);anchor=14 if name.startswith('Vine') else 9
  for y in range(base.height):
   shift=round(amount*max(0,(y-anchor)/(base.height-anchor))**1.4)
   for x in range(base.width):
    if base.getpixel((x,y))[3]:out.putpixel((x+shift,y),base.getpixel((x,y)))
  frames.append(out)
 add(name,frames,p)
grove=Image.open(F/'Assets/_Game/Art/EnchantedForestStaged/Stage01/Grove_Background.png').convert('RGBA')
back=grove.copy();removed=Image.new('L',grove.size)
for name,box in [('Upper_Mote_A',(247,87,258,100)),('Upper_Mote_B',(298,100,304,108))]:
 x0,y0,x1,y1=box;light=Image.new('RGBA',(x1-x0+6,y1-y0+6))
 for y in range(y0,y1):
  for x in range(x0,x1):
   c=grove.getpixel((x,y))
   if c in [rgba(h)for h in ['54f1ec','3c8a7f','83b8b0','55a6a2']]:
    light.putpixel((x-x0+3,y-y0+3),c);back.putpixel((x,y),rgba('042f36'));removed.putpixel((x,y),255)
 assert light.getbbox(),name
 add(name,[light],'Assets/_Game/Art/EnchantedForestStaged/Stage01/Grove_Background.png')
add('Grove_Motion_Copy',[back],'Assets/_Game/Art/EnchantedForestStaged/Stage01/Grove_Background.png')
removed.save(R/'Diagnostics/SeparatedTopMotesMask.png')
print('Water palette',Counter(grove.crop((184,40,204,94)).get_flattened_data()).most_common(7))
watercolors={rgba(h)for h in ['83b8b0','55a6a2','3c8a7f']};watermask=Image.new('L',grove.size);md=ImageDraw.Draw(watermask)
md.polygon([(184,39),(202,39),(202,92),(181,92)],fill=255);md.polygon([(184,111),(199,111),(193,146),(181,146)],fill=255)
for y in range(grove.height):
 for x in range(grove.width):
  if grove.getpixel((x,y))not in watercolors:watermask.putpixel((x,y),0)
watermask.save(R/'Diagnostics/WaterfallAllowedMask.png');waterframes=[]
for f in range(12):
 im=Image.new('RGBA',(40,144))
 # Low-contrast streaks descend through the water only; bridge/shore never moves.
 for x,y,length in [(187,43,7),(197,59,5),(190,77,8),(186,115,6),(191,131,5)]:
  yy=y+f%12
  for dy in range(length):
   if yy+dy<200 and watermask.getpixel((x,yy+dy)):
    old=grove.getpixel((x,yy+dy));target=tuple(round(old[k]*.76+rgba('a1c9be')[k]*.24)for k in range(3))+(255,)
    im.putpixel((x-174,yy+dy-8),target)
 waterframes.append(im)
add('Waterfall',waterframes,'Assets/_Game/Art/EnchantedForestStaged/Stage01/Grove_Background.png')
manifest=[];items=[];template=(F/'Assets/_Game/Art/ForestSurroundRevision/PolishFinal/Wisp.png.meta').read_text()
for name,frames in series.items():
 width,height=frames[0].size;Image.new('RGBA',(width,height)).save(T/(name+'-canvas.png'));encoded=[]
 for fi,im in enumerate(frames):
  rows=[]
  for y in range(height):
   row=[];x=0
   while x<width:
    c=im.getpixel((x,y));end=x+1
    while end<width and im.getpixel((end,y))==c:end+=1
    if c[3]:row.append([x,end-x,bytes(c).hex()])
    x=end
   rows.append(row)
  encoded.append(rows)
  meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/LivingForest/'+name+'/'+str(fi)).hex,template,flags=re.M)
  meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/LivingForest/Sprite/'+name+'/'+str(fi)).hex,meta,flags=re.M)
  (A/(name+'_'+str(fi).zfill(2)+'.png.meta')).write_text('\n'.join(s.rstrip()for s in meta.splitlines())+'\n')
 sheet=Image.new('RGBA',(width*len(frames),height))
 for i,im in enumerate(frames):sheet.alpha_composite(im,(i*width,0))
 sheet.save(R/'Previews'/(name+'-Strip.png'))
 frames[0].resize((width*3,height*3),Image.Resampling.NEAREST).save(R/'Previews'/(name+'-3x.png'))
 manifest.append(dict(name=name,width=width,height=height,frames=len(frames),source=sources[name],bounds=[im.getbbox()for im in frames],colors=len({c for im in frames for c in im.get_flattened_data()if c[3]})))
 items.append(dict(name=name,width=width,height=height,frames=encoded))
(R/'Diagnostics/NativeManifest.json').write_text(json.dumps(manifest,indent=2));(R/'Scripts/NativePlan.json').write_text(json.dumps(items,separators=(',',':')))
js='var pc=app.pixelColor;var items='+json.dumps(items,separators=(',',':'))+';\n'+'''items.forEach(function(a){var s=app.open('.utmp/living-forest/'+a.name+'-canvas.png').sprite;a.frames.forEach(function(rows,fi){if(fi>0)app.command.NewFrame();var im=app.activeImage;for(var y=0;y<a.height;y++)for(var x=0;x<a.width;x++)im.putPixel(x,y,0);rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.commit();});s.layer(0).name=a.name+' native motion';s.saveAs('ArtSource/ForestSurroundRevision/LivingForest/Native/'+a.name+'.aseprite',false);console.log('TIMELINE '+a.name+' '+s.layer(0).celCount);});console.log('LIVING_FOREST_COMPLETE');'''
(R/'Scripts/AuthorNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();\n")
# Export individual poses from their native timelines using independent canvases.
reopen='var pc=app.pixelColor;var all='+json.dumps([dict(name=m['name'],width=m['width'],height=m['height'],frames=m['frames'])for m in manifest])+';\n'+'''all.forEach(function(a){var s=app.open('ArtSource/ForestSurroundRevision/LivingForest/Native/'+a.name+'.aseprite').sprite;for(var fi=0;fi<a.frames;fi++){var input=s.layer(0).cel(fi).image;var out=app.open('.utmp/living-forest/'+a.name+'-canvas.png').sprite,im=out.layer(0).cel(0).image;for(var y=0;y<a.height;y++)for(var x=0;x<a.width;x++)im.putPixel(x,y,input.getPixel(x,y));out.commit();var tag=a.name+'_'+('0'+fi).slice(-2);out.saveAs('Assets/_Game/Art/ForestSurroundRevision/LivingForest/'+tag+'.png',true);out.saveAs('ArtSource/ForestSurroundRevision/LivingForest/Exports/'+tag+'.png',true);out.saveAs('.utmp/living-forest/reopened/'+tag+'.png',false);}});console.log('LIVING_FOREST_COMPLETE');'''
(R/'Scripts/ExportNative.js').write_text('try{'+reopen+"}catch(e){console.log('FAILED '+e);}app.command.Exit();\n")
(A.parent/'LivingForest.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/LivingForest').hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
launcher=(F/'ArtSource/ForestSurroundRevision/PolishFinal/Scripts/RunNative.ps1').read_text().replace('PolishFinal','LivingForest').replace("'RunNative.js'","'AuthorNative.js'").replace('FOREST_FINAL_NATIVE_COMPLETE','LIVING_FOREST_COMPLETE')
(R/'Scripts/RunNative.ps1').write_text(launcher)
shutil.copy2(__file__,R/'Scripts/BuildArt.py')
print('Prepared',len(manifest),'native timelines,',sum(m['frames']for m in manifest),'poses.')
