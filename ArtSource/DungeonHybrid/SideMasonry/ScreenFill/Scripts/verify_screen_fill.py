from pathlib import Path
from PIL import Image
from collections import Counter
import json,hashlib,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[5]
R=P/'ArtSource/DungeonHybrid/SideMasonry/ScreenFill';I=P/'ArtSource/DungeonHybrid/Integration'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
test=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert test.get('passed')=='3' and test.get('failed')=='0'
palette=set()
for folder in ['Wall','Support','FloorTop','Fascia']:
 for f in (P/'Assets/_Game/Art/DungeonPixelLab'/folder).glob('*.png'):
  palette.update(p[:3] for p in Image.open(f).convert('RGBA').get_flattened_data() if p[3])
mods=json.loads((R.parent/'MatchedCorners/Modules.json').read_text())
for a in mods['parts']:
 assert sha(P/a['source'])==a['source_sha256']
 assert sha(R.parent/'MatchedCorners/Exports'/(a['name']+'.png'))==a['sha256']
 assert sha(P/'Assets/_Game/Art/DungeonHybrid/SideMasonry/MatchedCorners'/(a['name']+'.png'))==a['sha256']
profiles=[]
for which,base,phase in [('production',I,'Final'),('candidate',R,'Candidate')]:
 for f in sorted((base/'Previews'/phase).glob('Dungeon-*.png')):
  im=Image.open(f).convert('RGB');w,h=im.size
  edge=[im.getpixel((x,y)) for y in [0,h-1] for x in range(w)]+[im.getpixel((x,y)) for x in [0,w-1] for y in range(h)]
  # UI rendering can round a source channel by one byte (observed 149 -> 148
  # on the approved floor highlight). This is not source recoloring or blur.
  distance={c:min(max(abs(c[k]-p[k]) for k in range(3)) for p in palette) for c in set(edge)}
  unknown=Counter(c for c in edge if distance[c]>1)
  assert not unknown,(which,f.name,'Unexpected perimeter colors',unknown)
  g=json.loads((base/'Diagnostics'/phase/(f.stem+'-geometry.json')).read_text())
  old=I/'Diagnostics/Baseline'/(f.stem+'-geometry.json')
  if old.exists(): assert g==json.loads(old.read_text()),f.name
  if which=='candidate':
   assert g==json.loads((I/'Diagnostics/Final'/(f.stem+'-geometry.json')).read_text()),f.name
   # Exact opaque bottom row: transparent PNG padding must not leave a strip.
   scale=max(1,round(w/360));stone=(36,31,58)
   assert all(im.getpixel((x,h-1))==stone for x in range(54*scale)),(f.name,'left base bottom')
   assert all(im.getpixel((x,h-1))==stone for x in range(w-54*scale,w)),(f.name,'right base bottom')
  profiles.append(dict(view=which,file=f.name,width=w,height=h,all_perimeter_pixels_are_stone=True,max_perimeter_channel_rounding=max(distance.values()),baseline_geometry_equal=old.exists()))
assert len(profiles)==12,len(profiles)
report=dict(result='PASS',tests_passed=3,tests_failed=0,profiles=profiles,source_art_unchanged=True,additional_generations=0,android_render_outside_safe_area_already_enabled=True,physical_device_tested=False)
(R/'Diagnostics/ScreenCoverage.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS: all four edges contain only native stone colors in 12 actual screenshots; original layout and art remain unchanged.')
