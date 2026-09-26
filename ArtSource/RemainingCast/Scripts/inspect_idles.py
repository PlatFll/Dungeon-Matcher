"""Read-only animation pose review, connectivity, color and source checks."""
from pathlib import Path
import sys,json,re,hashlib
from collections import deque
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
# Avoid this filename resolving itself.
import importlib.util
sp=importlib.util.spec_from_file_location('ase_reader',ROOT.parent/'CombatIdles/Scripts/inspect_idles.py')
reader=importlib.util.module_from_spec(sp);sp.loader.exec_module(reader)
read_ase=reader.read_ase;color_counts=reader.color_counts
pairs=re.findall(r"src:'([^']+)',name:'([^']+)'",(ROOT/'Scripts/cast_spec.js').read_text())
manifest=json.loads((ROOT/'SourceManifest.json').read_text())
for src,v in manifest.items():assert hashlib.sha256((ROOT/v['file']).read_bytes()).hexdigest()==v['sha256'],src
def components(im):
 points={(x,y)for y in range(64)for x in range(64)if im.getpixel((x,y))[3]};result=[]
 while points:
  seed=points.pop();q=[seed];part=[seed]
  while q:
   x,y=q.pop()
   for dx in (-1,0,1):
    for dy in (-1,0,1):
     p=(x+dx,y+dy)
     if p in points:points.remove(p);q.append(p);part.append(p)
  if len(part)>=3:result.append({'pixels':len(part),'bounds':[min(x for x,y in part),min(y for x,y in part),max(x for x,y in part),max(y for x,y in part)]})
 return sorted(result,key=lambda c:-c['pixels'])
report={}
for page in range((len(pairs)+3)//4):
 board=Image.new('RGB',(1090,900),'#24212B');d=ImageDraw.Draw(board)
 for k,(src,name) in enumerate(pairs[page*4:page*4+4]):
  fs,ds=read_ase(ROOT/'Idles'/f'{name}_Idle.aseprite');ready=read_ase(ROOT/'Recolored'/f'{name}.aseprite')[0][0]
  assert len(fs)==9 and ds==[130]*9,(name,ds)
  assert fs[0].tobytes()==ready.tobytes() and fs[8].tobytes()==ready.tobytes(),(name,'ready')
  allowed=set(color_counts([ready]));extra=set(color_counts(fs))-allowed
  assert not extra,(name,extra)
  assert all(im.size==(64,64) and set(im.getchannel('A').get_flattened_data())<={0,255} for im in fs)
  comps=[components(im) for im in fs]
  report[name]={'durations_ms':ds,'colors':len(allowed),'bounds':[im.getbbox()for im in fs],'components':comps,'unique_poses':len({im.tobytes()for im in fs})}
  y=k*225+22;d.text((8,y),name,fill='white')
  for col,f in enumerate([0,2,4,6]):
   x=285+col*196;im=fs[f].resize((192,192),Image.Resampling.NEAREST);board.paste(im,(x,y),im)
   d.text((x,y+198),f'Frame {f+1}',fill='#D3CBCD')
  trouble=[i+1 for i,c in enumerate(comps)if len(c)>len(comps[0])]
  assert not trouble,(name,'new detached clusters',trouble)
  d.text((8,y+20),f'{len(allowed)} colors | 1170 ms',fill='#D3CBCD')
  d.text((8,y+36),'Extra components: '+str(trouble),fill='#FF98A0' if trouble else '#B7A393')
  reader.sheet(fs,ds,name,ROOT/'Review'/f'{name}_frames.png')
 board.save(ROOT/'Review'/f'Idles_{page+1}.png')
(ROOT/'Review/IdleChecks.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS: 153 native poses; palette membership, binary alpha, fixed canvas, exact ready-pose return, timing and preserved originals.')
for name,v in report.items():
 bad=[i+1 for i,c in enumerate(v['components'])if len(c)>len(v['components'][0])]
 print(name,'unique',v['unique_poses'],'extra components',bad)
