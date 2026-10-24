from pathlib import Path
from PIL import Image
import json,re,yaml,hashlib
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
R=F/'ArtSource/ForestSurroundRevision/PolishFinal'
A=F/'Assets/_Game/Art/ForestSurroundRevision/PolishFinal'
def scene(path):
 text=path.read_text(encoding='utf-8');chunks=re.split(r'^--- !u!\d+ &(\d+)\s*$',text,flags=re.M)
 return {k:yaml.safe_load(v)for k,v in zip(chunks[1::2],chunks[2::2])}
old=scene(F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_RootJoins.unity')
new=scene(F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_FinalPolish.unity')
assert set(old)<=set(new),'Old scene object removed'
allowed_names={'Continuous lower root joins 2x','Unified lower corner formations 2x'}
changes=[]
for key,b in old.items():
 a=new[key]
 if b==a:continue
 if 'GameObject'in b:
  name=b['GameObject']['m_Name'];assert name in allowed_names,name
  before=b['GameObject'].copy();after=a['GameObject'].copy()
  assert before.pop('m_IsActive')==1 and after.pop('m_IsActive')==0
  assert before==after;changes.append({'id':key,'name':name,'change':'Disabled superseded lower dressing'})
 elif 'RectTransform'in b:
  before=b['RectTransform'].copy();after=a['RectTransform'].copy()
  bc=before.pop('m_Children');ac=after.pop('m_Children');assert before==after
  assert [c for c in ac if c in bc]==bc and len(ac)==len(bc)+3
  changes.append({'id':key,'change':'Three new dressing layers inserted; existing child order preserved'})
 else:raise AssertionError((key,b,a))
assert len(changes)==3,changes
placements=json.loads((R/'Scripts/Placements.json').read_text())['pieces']
def components(im):
 pts={(x,y)for y in range(im.height)for x in range(im.width)if im.getpixel((x,y))[3]};sizes=[]
 while pts:
  todo=[pts.pop()];n=0
  while todo:
   x,y=todo.pop();n+=1
   for dx in [-1,0,1]:
    for dy in [-1,0,1]:
     q=(x+dx,y+dy)
     if q in pts:pts.remove(q);todo.append(q)
  sizes.append(n)
 return sorted(sizes,reverse=True)
joins={}
for side in ['Left','Right']:
 im=Image.new('RGBA',(360,680))
 for name in ['Sweep_'+side,'Growth_'+side,'Corner_'+side]:
  q=next(q for q in placements if q['file']==name)
  im.alpha_composite(Image.open(A/(name+'.png')).convert('RGBA'),(q['x'],q['y']))
 counts=components(im);assert len(counts)==1,(side,counts)
 joins[side]={'combined_components':counts,'single_anchored_formation':True}
expected={'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png':'705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png':'6189bc2cfe01bab84bafada24a7b129a3fd8c2225f629eb8580386f461109e2d','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png':'e1be89f8231483df5db6739dd2cae7bef83e11aaa9b028e4cb1b60ea55aba0ca'}
for p,h in expected.items():assert hashlib.sha256((F/p).read_bytes()).hexdigest()==h,p
report=dict(existing_objects=len(old),new_objects=len(set(new)-set(old)),deleted_objects=0,existing_changes=changes,corner_joins=joins,current_goldens_verified=expected)
(R/'Diagnostics/SceneAudit.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
