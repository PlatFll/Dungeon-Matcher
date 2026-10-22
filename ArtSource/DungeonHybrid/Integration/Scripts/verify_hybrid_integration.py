from pathlib import Path
from PIL import Image
import hashlib,json,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/Integration';M=Path('C:/UnityProjects/Dungeon Matcher')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
protected=json.loads((R/'Diagnostics/ProtectedHashes.json').read_text())
changed=[p for p,h in protected.items() if not (P/p).exists() or sha(P/p)!=h]
start=json.loads((R/'Diagnostics/StartingState.json').read_text())
mainchanged=[p for p,h in start['main_user_files'].items() if sha(M/p)!=h]
print('Changed protected:',changed,'Main user files changed:',mainchanged)
assert not changed and not mainchanged
results=ET.parse(R/'Diagnostics/Final/RegressionTests.xml').getroot()
assert results.get('result')=='Passed' and int(results.get('passed'))>1 and results.get('failed')=='0'
captures=[]
for b in sorted((R/'Diagnostics/Baseline').glob('*-geometry.json')):
 f=R/'Diagnostics/Final'/b.name
 base=json.loads(b.read_text());final=json.loads(f.read_text())
 assert base==final,('Layout changed',b.name,base,final)
 label=b.name.replace('-geometry.json','')
 im=Image.open(R/f'Previews/Final/{label}.png').convert('RGB');w,h=im.size
 r=final['board'];x,y,bw,bh=[r[k] for k in ('x','y','width','height')]
 box=(int(x+bw*.08),int(h-y-bh+bh*.08),int(x+bw*.92),int(h-y-bh*.08))
 pixels=list(im.crop(box).getdata());ratio=sum(max(c)>90 and max(c)-min(c)>55 for c in pixels)/len(pixels)
 assert ratio>.15,(label,'gem visibility',ratio)
 captures.append(dict(name=label,width=w,height=h,geometry_unchanged=True,visible_gem_fraction=round(ratio,4)))
assert len(captures)==5
out=dict(result='PASS',protected_files_unchanged=len(protected),main_user_files_unchanged=len(start['main_user_files']),
 regression_passed=int(results.get('passed')),regression_failed=0,captures=captures,
 approved_art_unchanged=True,game_scene_serialization_unchanged=True,gameplay_source_unchanged=True,
 new_art_files=0,generation_cost=0,scope='Actual Game scene, live three-enemy disposable fixture, five screen/safe-area profiles. No merge; no real-device or Android build claim.')
(R/'Diagnostics/PreservationAndPixels.json').write_text(json.dumps(out,indent=2)+'\n')
print(json.dumps(out,indent=2))
