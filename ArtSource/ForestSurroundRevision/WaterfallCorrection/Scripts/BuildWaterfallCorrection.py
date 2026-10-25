from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import hashlib,json,uuid,re,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
R=F/'ArtSource/ForestSurroundRevision/WaterfallCorrection';A=F/'Assets/_Game/Art/ForestSurroundRevision/WaterfallCorrection';T=F/'.utmp/forest-waterfall'
for p in [R/'Diagnostics',R/'Native',R/'Exports',R/'Previews',R/'Scripts',A,T/'reopened',T/'before',T/'after']:p.mkdir(parents=True,exist_ok=True)
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
old=F/'ArtSource/ForestSurroundRevision/LivingForest'
preserved=json.loads((old/'Diagnostics/PreservedFiles.json').read_text())
for base in [old,F/'Assets/_Game/Art/ForestSurroundRevision/LivingForest']:
 for p in base.rglob('*'):
  if p.is_file() and p.suffix not in ['.log','.err']:preserved[p.relative_to(F).as_posix()]=sha(p)
for path in ['Assets/_Game/Scenes/ArtPreview_ForestSurround_Living.unity','Assets/_Game/Scripts/ArtPreview/LivingForestReviewMotion.cs','Assets/_Game/Scripts/Editor/LivingForestReviewTests.cs']:
 preserved[path]=sha(F/path)
for path,digest in preserved.items():assert sha(F/path)==digest,path
snap=R/'Diagnostics/PreservedFiles.json'
if snap.exists():assert json.loads(snap.read_text())==preserved
else:snap.write_text(json.dumps(preserved,indent=2))
shutil.copy2(old/'Diagnostics/ReferenceHashes.json',R/'Diagnostics/ReferenceHashes.json')
grove=Image.open(F/'Assets/_Game/Art/ForestSurroundRevision/LivingForest/Grove_Motion_Copy_00.png').convert('RGBA')
mask=Image.new('L',grove.size);d=ImageDraw.Draw(mask)
d.polygon([(185,36),(202,36),(202,97),(181,97)],fill=255)
d.polygon([(183,110),(199,110),(198,137),(194,149),(180,149)],fill=255)
# The existing bright water plane is the authority; never paint foliage, bridge or bank pixels.
for y in range(mask.height):
 for x in range(mask.width):
  if grove.getpixel((x,y))!=(131,184,176,255):mask.putpixel((x,y),0)
mask.save(R/'Diagnostics/WaterMask.png')
frames=[];palette=[(161,210,199,255),(143,196,187,255),(106,168,160,255)]
# Deliberate connected streams. A 24-pixel vertical period gives exact one-pixel downward motion and loop closure.
streams=[(186,2,11,3,0),(193,2,9,14,0),(197,1,16,8,1),(190,2,13,7,2)]
def pose(f):
 im=Image.new('RGBA',(40,144))
 for x,width,length,phase,tone in streams:
  for y in range(36,150):
   q=(y-f-phase)%24
   if q>=length:continue
   # Taper the leading/trailing ends of broader water ribbons by one native pixel.
   span=1 if q in [0,length-1] else width
   for dx in range(span):
    if mask.getpixel((x+dx,y)):im.putpixel((x+dx-174,y-8),palette[tone])
 return im
for f in range(24):
 im=pose(f);im.save(T/f'Waterfall_Flow_{f:02}-plan.png');frames.append(im)
assert pose(24).tobytes()==frames[0].tobytes()
assert len({im.tobytes() for im in frames})==24
Image.new('RGBA',(40,144)).save(T/'canvas.png')
Image.new('RGBA',(2,2)).save(T/'primer.png')
sheet=Image.new('RGBA',(40*24,144))
composites=[]
for f,im in enumerate(frames):
 sheet.alpha_composite(im,(40*f,0));composite=grove.copy();composite.alpha_composite(im,(174,8));composites.append(composite)
sheet.save(R/'Previews/Waterfall-Strip.png')
phase=Image.new('RGB',(240*3,510),'#152c2c');pd=ImageDraw.Draw(phase)
for i,f in enumerate([0,8,16]):
 pd.text((i*240+8,6),f'FLOW POSE {f}',fill='white');phase.paste(composites[f].crop((160,30,220,151)).resize((240,484),Image.Resampling.NEAREST),(i*240,26))
phase.save(R/'Previews/Flow-Phases.png')
# Native source GIF contains only exact pixel art, not a filtered shader or screen-space distortion.
gif=[im.crop((160,30,220,151)).resize((240,484),Image.Resampling.NEAREST) for im in composites]
gif[0].save(R/'Previews/Waterfall-Design.gif',save_all=True,append_images=gif[1:],duration=[80,80,90]*8,loop=0,disposal=2)
encoded=[]
for im in frames:
 rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   c=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==c:end+=1
   if c[3]:row.append([x,end-x,bytes(c).hex()])
   x=end
  rows.append(row)
 encoded.append(rows)
relative=R.relative_to(F).as_posix();art=A.relative_to(F).as_posix();scratch=T.relative_to(F).as_posix()
native=f'''app.open('{scratch}/primer.png').sprite.saveAs('{scratch}/primer.aseprite',false);var rowsAll={json.dumps(encoded,separators=(',',':'))};var pc=app.pixelColor;var s=app.open('{scratch}/canvas.png').sprite;
rowsAll.forEach(function(rows,f){{if(f>0)app.command.NewFrame();var im=app.activeImage;for(var y=0;y<144;y++)for(var x=0;x<40;x++)im.putPixel(x,y,0);rows.forEach(function(row,y){{row.forEach(function(r){{var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);}});}});s.commit();}});s.layer(0).name='Waterfall downward flow';s.saveAs('{relative}/Native/Waterfall_Flow.aseprite',false);console.log('POSES '+s.layer(0).celCount);console.log('WATERFALL_COMPLETE');'''
(R/'Scripts/AuthorNative.js').write_text('try{'+native+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
export=f'''var s=app.open('{relative}/Native/Waterfall_Flow.aseprite').sprite;for(var f=0;f<24;f++){{var source=s.layer(0).cel(f).image;var out=app.open('{scratch}/canvas.png').sprite;var im=out.layer(0).cel(0).image;for(var y=0;y<144;y++)for(var x=0;x<40;x++)im.putPixel(x,y,source.getPixel(x,y));out.commit();var name='Waterfall_Flow_'+('0'+f).slice(-2)+'.png';out.saveAs('{art}/'+name,true);out.saveAs('{relative}/Exports/'+name,true);out.saveAs('{scratch}/reopened/'+name,false);}}console.log('WATERFALL_COMPLETE');'''
(R/'Scripts/ExportNative.js').write_text('try{'+export+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
template=(F/'Assets/_Game/Art/ForestSurroundRevision/LivingForest/Waterfall_00.png.meta').read_text()
for f in range(24):
 meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/WaterfallCorrection/'+str(f)).hex,template,flags=re.M)
 meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/WaterfallCorrection/Sprite/'+str(f)).hex,meta,flags=re.M)
 (A/f'Waterfall_Flow_{f:02}.png.meta').write_text('\n'.join(s.rstrip()for s in meta.splitlines())+'\n')
(A.parent/'WaterfallCorrection.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/WaterfallCorrection').hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
launcher=(old/'Scripts/RunNative.ps1').read_text().replace('LivingForest','WaterfallCorrection').replace('LIVING_FOREST_COMPLETE','WATERFALL_COMPLETE')
(R/'Scripts/RunNative.ps1').write_text(launcher)
(R/'Diagnostics/NativeManifest.json').write_text(json.dumps(dict(dimensions=[40,144],frames=24,fps=12,duration_seconds=2,palette=[bytes(c[:3]).hex()for c in palette],origin=[174,8],binary_alpha=True,loop_closes=True,generations_spent=0),indent=2))
shutil.copy2(__file__,R/'Scripts/BuildWaterfallCorrection.py')
print('Prepared 24 native water poses; prior files protected:',len(preserved))
