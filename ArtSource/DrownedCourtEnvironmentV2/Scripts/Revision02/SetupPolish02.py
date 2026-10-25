from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,subprocess
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
for d in ['Native','Exports','Preview','QA','Scripts','RawPixelLab']:(R/d/'Revision02').mkdir(parents=True,exist_ok=True)
Q=R/'QA/Revision02';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
if not (Q/'PreservedBefore.json').exists():
 files=subprocess.check_output(['git','ls-files'],cwd=F).decode().splitlines();(Q/'PreservedBefore.json').write_text(json.dumps({p:sha(F/p) for p in files if (F/p).is_file() and not p.startswith('ArtSource/DrownedCourtEnvironmentV2/Progress/')},indent=2))
names=['ColumnLeftA','ColumnLeftB','ColumnRightA','ColumnRightB','S07_LeftReef','S07_RightReef'];families=[['572b4a','a75271','db8b97'],['352b50','775789','ab83b1'],['70413f','bd705e','edab80'],['2f203d','63334f','944a68','c5728b']];groups=[{tuple(bytes.fromhex(c)) for c in cs} for cs in families];plan=[]
for n in names:
 p=R/f'Exports/Revision01/{n}.png';im=Image.open(p).convert('RGBA');patch=[];counts=[]
 for colors in groups:
  points={(x,y) for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))[:3] in colors and im.getpixel((x,y))[3]};comps=[]
  while points:
   todo=[points.pop()];comp=[]
   while todo:
    x,y=todo.pop();comp.append((x,y))
    for dx,dy in [(-1,-1),(0,-1),(1,-1),(-1,0),(1,0),(-1,1),(0,1),(1,1)]:
     q=(x+dx,y+dy)
     if q in points:points.remove(q);todo.append(q)
   comps.append(comp)
  counts.extend(len(c) for c in comps)
  for comp in comps:
   if len(comp)>3:continue
   for x,y in comp:
    # Only explicitly detected detached coral flecks; restore the neighboring structural plane.
    adjacent=[im.getpixel((xx,yy)) for xx,yy in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)] if 0<=xx<im.width and 0<=yy<im.height]
    neutral=[c for c in adjacent if c[:3] not in set.union(*groups)]
    target=max(set(neutral),key=neutral.count) if neutral else (0,0,0,0)
    patch.append([x,y,list(im.getpixel((x,y))),list(target)])
 plan.append(dict(name=n,source=f'Exports/Revision01/{n}.png',oldAsset=f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/{n}.png',patches=patch,width=im.width,height=im.height));print(n,'components',sorted(counts,reverse=True),'cleanup pixels',len(patch))
(R/'Native/Revision02/CoralCleanup.json').write_text(json.dumps(plan,indent=2)+'\n')
for n,w,h in [('PortraitWallA',64,32),('PortraitWallB',64,32),('OceanMasonry',64,32),('MasonryGrowth',64,32),('BubbleSmall0',8,8),('BubbleSmall1',8,8),('BubbleSmall2',8,8),('BubbleMedium0',12,12),('BubbleMedium1',12,12),('BubbleMedium2',12,12)]:Image.new('RGBA',(w,h)).save(R/f'Native/Revision02/Work-{n}.png')
board=Image.new('RGB',(740,400),'#062432');d=ImageDraw.Draw(board);x=5
for n in names:
 im=Image.open(R/f'Exports/Revision01/{n}.png').convert('RGBA');scale=2;im=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST);board.paste(im,(x,30),im);d.text((x,7),n,fill='white');x+=im.width+14
board.save(R/'Preview/Revision02/Coral-Before-2x.png')
print('Revision02 source snapshots and native work canvases ready.')
