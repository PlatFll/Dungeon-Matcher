from pathlib import Path
from PIL import Image,ImageDraw
import sys,shutil,json,hashlib,subprocess
R=Path(sys.argv[1]);S=Path(__file__).parent;A=R/'ArtSource/StoneAbilityUI/LightGrayRevision';M=R/'ArtSource/StoneAbilityUI/MaterialRevision';D=R/'Assets/_Game/Resources/UI/BattlePresentation/Stone'
for d in ['Before','Native','Exports','Roundtrip','Scripts','QA','Preview/After']:(A/d).mkdir(parents=True,exist_ok=True)
protected=['Assets/Settings/UniversalRP.asset','Assets/UniversalRenderPipelineGlobalSettings.asset','Assets/_Game/Resources/BattleEnvironments/DrownedCourt.prefab',*[f'Assets/_Game/Resources/UI/Consumables/{x}.png.meta' for x in ['SlotDisabled','SlotHighlighted','SlotPressed']],'ProjectSettings/TimeManager.asset','ProjectSettings/GraphicsSettings.asset','ProjectSettings/ProjectSettings.asset']
for p in protected:
 dst=S/'Initial'/p;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(R/p,dst)
shutil.copytree(M/'Preview/After',S/'PreviousAfter',dirs_exist_ok=True)
refs=['Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','Docs/ArtDirection/Golden_Reference_Register.md','Docs/ArtDirection/Audit2026-10/media/skeleton-still.png','Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png']
refs += [f'ArtSource/StoneAbilityUI/MaterialRevision/Inputs/{n}.png' for n in ['AbilityPlate','Settings','Supply','WaveBody']]
(A/'QA/ReferenceLineage.json').write_text(json.dumps([{'path':p,'sha256':hashlib.sha256((R/p).read_bytes()).hexdigest()} for p in refs],indent=2))
rgb=lambda h:tuple(bytes.fromhex(h))
# The prior lighter cool-gray ramp, assigned to the approved new stone planes.
colors=dict(zip(['0a0d11','202129','343438','484646','595955','6f6d66','7b7871','8d867c','aaa397'],['0a0d11','262333','3c374a','524d61','6f6b7e','838091','948fa4','a7a1b6','c4bdd4']))
# Restore the former ability face's exact dominant gray, while keeping each
# distinct chiseled plane. No new clusters or silhouette edits.
plateColors=dict(zip(['0a0d11','202129','343438','484646','595955','6f6d66','7b7871','8d867c','aaa397'],['0a0d11','151423','262333','3c374a','524d61','6f6b7e','838091','a7a1b6','c4bdd4']))
names=[x['name'] for x in json.loads((M/'QA/NativeValidation.json').read_text())];expected={};manifest=[];js='var root='+json.dumps(A.as_posix()+'/')+';\n'
for name in names:
 src=Image.open(D/(name+'.png')).convert('RGBA');shutil.copy2(D/(name+'.png'),A/'Before'/(name+'.png'))
 engraving=name.startswith(('CrownedSkull','Bull','Revolver','Flute'))
 mappings={rgb(k):rgb(v) for k,v in (plateColors if name=='AbilityPlate' else colors).items()}
 if name=='WaveBody':mappings[rgb('202129')]=rgb('1a1c28') # original dark label well
 if engraving:
  previous=Image.open(M/'Inputs'/(name+'.png')).convert('RGBA')
  assert previous.getchannel('A').tobytes()==src.getchannel('A').tobytes()
  mappings={}
  for a,b in zip(src.get_flattened_data(),previous.get_flattened_data()):
   if a[3]:
    if a[:3] in mappings:assert mappings[a[:3]]==b[:3]
    mappings[a[:3]]=b[:3]
 # Supply recess is its own material plane. Restore the old lighter solid face,
 # retaining the dark carved inset surrounding it.
 center=set()
 if name=='Supply':
  todo=[(32,32)];c=src.getpixel((32,32))
  while todo:
   x,y=todo.pop()
   if (x,y) in center or not(0<=x<src.width and 0<=y<src.height) or src.getpixel((x,y))!=c:continue
   center.add((x,y));todo.extend([(x-1,y),(x+1,y),(x,y-1),(x,y+1)])
  assert all(10<x<54 and 10<y<54 for x,y in center)
 out=Image.new('RGBA',src.size)
 for y in range(src.height):
  for x in range(src.width):
   p=src.getpixel((x,y));out.putpixel((x,y),(*((131,128,145) if (x,y) in center else mappings[p[:3]]),255) if p[3] else (0,0,0,0))
 expected[name]=out
 table={','.join(map(str,k)):list(v) for k,v in mappings.items()};coords={f'{x},{y}':True for x,y in center}
 js+=f"app.open(root+'Before/{name}.png');var spr=app.activeSprite;var im=spr.layer(0).cel(0).image;var pc=app.pixelColor;var map="+json.dumps(table)+";var center="+json.dumps(coords)+";\n"
 js+="for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++){var p=im.getPixel(x,y);if(pc.rgbaA(p)==0){im.putPixel(x,y,pc.rgba(0,0,0,0));continue;}var k=pc.rgbaR(p)+','+pc.rgbaG(p)+','+pc.rgbaB(p);var c=center[x+','+y]?[131,128,145]:map[k];if(!c)throw new Error('Unmapped color '+k);im.putPixel(x,y,pc.rgba(c[0],c[1],c[2],255));}\n"
 js+=f"spr.layer(0).name='Approved stone geometry - restored lighter gray';spr.commit();spr.saveAs(root+'Native/{name}.ase',true);spr.saveAs(root+'Exports/{name}.png',true);app.activeDocument.close();app.open(root+'Native/{name}.ase');app.activeSprite.saveAs(root+'Roundtrip/{name}.png',true);app.activeDocument.close();\n"
 manifest.append({'name':name,'palette_map':{'#%02x%02x%02x'%k:'#%02x%02x%02x'%v for k,v in mappings.items()},'solid_supply_face_pixels':len(center),'before_sha256':hashlib.sha256((D/(name+'.png')).read_bytes()).hexdigest()})
js+="console.log('LIGHT_GRAY_ROUNDTRIP_COMPLETE');app.command.Exit();"
(A/'Scripts/Recolor.js').write_text(js)
si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
run=subprocess.run([r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe','--script',str(A/'Scripts/Recolor.js')],startupinfo=si,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=60)
(A/'QA/LibreSprite.txt').write_bytes(run.stdout);assert run.returncode==0 and b'LIGHT_GRAY_ROUNDTRIP_COMPLETE' in run.stdout,run.stdout
for row in manifest:
 name=row['name'];p=A/'Exports'/(name+'.png');im=Image.open(p).convert('RGBA');old=Image.open(A/'Before'/(name+'.png')).convert('RGBA');rt=Image.open(A/'Roundtrip'/p.name).convert('RGBA')
 assert im.tobytes()==rt.tobytes()==expected[name].tobytes()
 assert im.size==old.size and im.getchannel('A').tobytes()==old.getchannel('A').tobytes()
 row.update(size=im.size,colors=len({p[:3] for p in im.get_flattened_data() if p[3]}),binary_alpha=True,mask_exact=True,roundtrip_exact=True,sha256=hashlib.sha256(p.read_bytes()).hexdigest())
 shutil.copy2(p,D/p.name)
(A/'QA/NativeValidation.json').write_text(json.dumps(manifest,indent=2))
board=Image.new('RGBA',(420,420),(26,28,37,255));draw=ImageDraw.Draw(board)
for col,title in enumerate(['APPROVED SHAPES','LIGHTER GRAY']):draw.text((col*210+8,8),title,fill='white')
for row,name in enumerate(['WaveBody','Settings','Supply','AbilityPlate','CrownedSkull']):
 y=row*77+35
 for col,folder in enumerate(['Before','Exports']):
  x=col*210+8;im=Image.open(A/folder/(name+'.png')).convert('RGBA')
  if name=='CrownedSkull':board.alpha_composite(Image.open(A/folder/'AbilityPlate.png').convert('RGBA'),(x,y));board.alpha_composite(im,(x+64,y+13))
  else:board.alpha_composite(im,(x,y))
board.save(A/'Preview/ControlsNative.png');board.resize((840,840),Image.Resampling.NEAREST).save(A/'Preview/Controls2x.png')
(A/'Scripts/Recolor.py').write_bytes(Path(__file__).read_bytes())
print('13 LibreSprite recolors; exact dimensions and alpha masks; no PixelLab spending')
