from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil
F=Path('C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage04'
assets=[];sheet=Image.new('RGBA',(950,430),'#20232c');d=ImageDraw.Draw(sheet)
for i,p in enumerate(sorted((R/'Exports').glob('*.png'))):
 im=Image.open(p).convert('RGBA');rt=Image.open(R/'Roundtrip'/p.name).convert('RGBA');assert im.tobytes()==rt.tobytes();alpha=sorted(set(im.getchannel('A').get_flattened_data()));assert set(alpha)<={0,255}
 x=12+(i%4)*235;y=10+(i//4)*210;d.text((x,y),p.stem,fill='white');d.text((x,y+16),f'{im.width}x{im.height}',fill='#b7c5d4');sheet.alpha_composite(im,(x,y+35));sheet.alpha_composite(im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST),(x,y+82))
 assets.append({'name':p.stem,'size':im.size,'bounds':im.getbbox(),'opaque_colors':len({c[:3] for c in im.get_flattened_data() if c[3]}),'alpha':alpha,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'libresprite_roundtrip_exact':True})
sheet.save(R/'Preview/NativeAssets.png')
assemblies={}
for side in ['Left','Right']:
 im=Image.new('RGBA',(40,36));pieces=[('MountLeft',0,0),('MotorLeft',7,4),('NoseRight',23,9),('PowerBlock',13,13)] if side=='Left' else [('MountRight',2,0),('MotorRight',15,4),('NoseLeft',1,9),('PowerBlock',18,13)]
 for n,x,y in pieces:im.alpha_composite(Image.open(R/'Exports'/(n+'.png')).convert('RGBA'),(x,y))
 im.save(R/'Preview'/f'Assembly{side}.png');assemblies[side]=im
 # One continuous silhouette: no loose drill fragments or floating power blocks.
 mask={(x,y) for y in range(36) for x in range(40) if im.getpixel((x,y))[3]};todo=[next(iter(mask))];seen=set(todo)
 while todo:
  x,y=todo.pop()
  for p in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)]:
   if p in mask and p not in seen:seen.add(p);todo.append(p)
 assert seen==mask,(side,'disconnected opaque islands',len(mask-seen))
assembly=Image.new('RGBA',(490,290),'#20232c');d=ImageDraw.Draw(assembly)
for i,(side,im) in enumerate(assemblies.items()):
 x=10+i*240;d.text((x,5),side+' board-side assembly',fill='white');assembly.alpha_composite(im,(x,26));assembly.alpha_composite(im.resize((200,180),Image.Resampling.NEAREST),(x,83))
assembly.save(R/'Preview/Assemblies.png');assert assemblies['Left'].getchannel('A').transpose(Image.Transpose.FLIP_LEFT_RIGHT).tobytes()==assemblies['Right'].getchannel('A').tobytes(),'Mirrored assembly silhouette'
(R/'QA/Assets.json').write_text(json.dumps({'status':'PASS','assets':assets,'connected_assemblies':True,'mirrored_silhouettes':True,'native_assembly_canvas':[40,36],'palette':sorted({'#%02x%02x%02x'%c[:3] for p in (R/'Exports').glob('*.png') for c in Image.open(p).convert('RGBA').get_flattened_data() if c[3]})},indent=2))
shutil.copyfile(__file__,R/'Scripts/InspectNative.py');print('Native QA PASS',len(assets),'modular assets')
