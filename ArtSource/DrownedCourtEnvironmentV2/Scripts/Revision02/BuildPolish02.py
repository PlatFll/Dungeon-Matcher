from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent;E=R/'Exports/Revision02';P=R/'Preview/Revision02'
names=['ColumnLeftA','ColumnLeftB','ColumnRightA','ColumnRightB','S07_LeftReef','S07_RightReef','PortraitWallA','PortraitWallB','OceanMasonry','MasonryGrowth']+['Bubble'+sz+str(k) for sz in ['Small','Medium'] for k in range(3)]+['Clam'+str(k) for k in range(5)]
assert all((E/(n+'.png')).exists() for n in names),'Missing native exports'
assets=[]
for n in names:
 p=E/(n+'.png');im=Image.open(p).convert('RGBA');assets.append(dict(name=n,width=im.width,height=im.height,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
repl=[dict(name=n,oldAsset=f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/{n}.png') for n in names[:6]]
repl += [dict(name='PortraitWall'+v,oldAsset=f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/S04_Recess{v}.png') for v in ['A','B']]
data=dict(assets=assets,replacements=repl,credit_cost=5,animation_loop_seconds=24,clam_loop_seconds=12,status='CANDIDATE_PENDING_REVIEW')
(R/'Native/Revision02/Manifest.json').write_text(json.dumps(data,indent=2)+'\n')
review=Image.new('RGBA',(37*5,38),(4,37,51,255))
for i in range(5):review.alpha_composite(Image.open(E/f'Clam{i}.png').convert('RGBA'),(i*37,0))
review.save(P/'Clam-Poses-Native.png');review.resize((740,152),Image.Resampling.NEAREST).save(P/'Clam-Poses-4x.png')
for n in ['SetupPolish02.py','PreparePolish02.py','BuildPolish02.py','RunLibrePolish02.ps1']:
 src=W/n;dst=R/'Scripts/Revision02'/n
 if src.resolve()!=dst.resolve():shutil.copy2(src,dst)
print('Prepared',len(assets),'native assets and scoped bindings.')
