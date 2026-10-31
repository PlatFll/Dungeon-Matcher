from pathlib import Path
import json,hashlib
from PIL import Image,ImageChops
R=Path(__file__).resolve().parents[4]
S=R/'ArtSource/SnowyMountainStaged/Stage08'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
baseline=json.loads((S/'QA/StartingHashes.json').read_text())
changed=[p for p,h in baseline.items() if not (R/p).exists() or sha(R/p)!=h]
assert not changed,changed
for a in json.loads((S/'Manifest.json').read_text())['assets']:
 p=S/'Exports'/(a['name']+'.png');im=Image.open(p).convert('RGBA');back=Image.open(S/'Roundtrip'/p.name).convert('RGBA')
 assert sha(p)==a['sha256'] and im.size==(8,8) and im.tobytes()==back.tobytes()
 assert set(im.getchannel('A').getdata())<={0,255}
records=[]
measure=json.loads((S/'Preview/Measurements.json').read_text())['items']
def region(rect,w,h):return (round(rect['x']),h-round(rect['y']+rect['height']),round(rect['x']+rect['width']),h-round(rect['y']))
def check(a,b,m,label):
 w,h=a.size;diff=ImageChops.difference(a,b)
 for key,rect in [('board frame and grid',m['boardFrame']),('portrait',m['geometry']['portrait'])]:
  assert diff.crop(region(rect,w,h)).getbbox() is None,(label,key)
 for rect in m['geometry']['health']:assert diff.crop(region(rect,w,h)).getbbox() is None,(label,'health bars')
 for control in m['geometry']['controls']:assert diff.crop(region(control['rect'],w,h)).getbbox() is None,(label,control['name'])
 bottom=m['geometry']['bottom'];central=(round(w*.25),h-round(bottom['y']+bottom['height']),round(w*.75),h)
 assert diff.crop(central).getbbox() is None,(label,'central bottom text and energy')
 changed=sum(p!=(0,0,0) for p in diff.getdata())
 assert 0<changed<w*h*.005,(label,'effects sparse and visibly present',changed)
 return changed
for m in measure:
 key=m['size'];a=Image.open(S/f'Preview/Before/{key}.png').convert('RGB');b=Image.open(S/f'Preview/After/{key}.png').convert('RGB');reduced=Image.open(S/f'Preview/Reduced/{key}.png').convert('RGB')
 assert a.tobytes()==reduced.tobytes(),(key,'Reduced Motion exact Stage 7 scene')
 records.append({'viewport':key,'changed_pixels':check(a,b,m,key),'protected_board_portrait_controls_health_text':'exact','reduced_motion':'exact baseline'})
frames=sorted((S/'Preview/Motion').glob('*.png'));assert len(frames)==96,len(frames)
m=next(v for v in measure if v['size']=='720x1280');a=Image.open(S/'Preview/Before/720x1280.png').convert('RGB')
counts=[check(a,Image.open(p).convert('RGB'),m,p.name) for p in frames]
distinct=len({sha(p) for p in frames});assert distinct>=80,distinct
report={'status':'passed','protected_files':len(baseline),'changed_protected_files':changed,'native_frames':9,'views':records,'motion':{'frames':96,'duration_seconds':12,'distinct_frames':distinct,'protected_region_comparison':'every frame exact','changed_pixels_min':min(counts),'changed_pixels_max':max(counts)},'unity':'See UnityTestResults.xml and UnityCompilation.txt. Editor evidence only.'}
old=S/'QA/Validation.json'
if old.exists() and isinstance(json.loads(old.read_text()).get('unity'),dict):report['unity']=json.loads(old.read_text())['unity']
(S/'QA/Validation.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
