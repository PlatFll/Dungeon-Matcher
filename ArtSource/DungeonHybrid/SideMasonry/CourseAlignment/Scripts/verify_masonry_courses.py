from pathlib import Path
from PIL import Image
import json,hashlib,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[5]
R=P/'ArtSource/DungeonHybrid/SideMasonry/CourseAlignment';I=P/'ArtSource/DungeonHybrid/Integration'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
t=ET.parse(R/'Diagnostics/UnityTests.xml').getroot()
assert t.get('result')=='Passed' and t.get('passed')=='3' and t.get('failed')=='0'
modules=json.loads((R.parent/'MatchedCorners/Modules.json').read_text())
for a in modules['parts']:
 assert sha(P/a['source'])==a['source_sha256']
 assert sha(P/'Assets/_Game/Art/DungeonHybrid/SideMasonry/MatchedCorners'/(a['name']+'.png'))==a['sha256']
protected=json.loads((I/'Diagnostics/ProtectedHashes.json').read_text())
assert all(sha(P/f)==h for f,h in protected.items()),'Protected source changed'
main=json.loads((I/'Diagnostics/StartingState.json').read_text())['main_user_files']
assert all(sha(Path('C:/UnityProjects/Dungeon Matcher')/f)==h for f,h in main.items()),'Pre-existing user change modified'
profiles=[]
def check_row(im,row,side,y,scale):
 w,h=im.size;start=0 if side=='Left' else w-len(row)*scale
 compared=0;maximum=0
 for sx,c in enumerate(row):
  if c[3]!=255: continue
  for sub in range(scale):
   actual=im.getpixel((start+sx*scale+sub,y))
   maximum=max(maximum,max(abs(actual[k]-c[k]) for k in range(3)))
   compared+=1
 return compared,maximum
for which,base,phase in [('production',I,'Final'),('candidate',R,'Candidate')]:
 for f in sorted((base/'Previews'/phase).glob('Dungeon-*.png')):
  im=Image.open(f).convert('RGB');w,h=im.size;scale=max(1,round(w/360))
  g=json.loads((base/'Diagnostics'/phase/(f.stem+'-geometry.json')).read_text())
  baseline=I/'Diagnostics/Baseline'/(f.stem+'-geometry.json')
  if baseline.exists(): assert g==json.loads(baseline.read_text()),f.name
  if which=='candidate': assert g==json.loads((I/'Diagnostics/Final'/(f.stem+'-geometry.json')).read_text()),f.name
  # Compare every opaque side-stone pixel along the board/HUD against the
  # unchanged native source, anchored to the physical bottom and outside edges.
  # One byte of GPU channel rounding is allowed, as measured in the prior pass.
  limit=int(g['board']['y']+g['board']['height']);count=0;max_delta=0
  for side in ['Left','Right']:
   tile=Image.open(P/'Assets/_Game/Art/DungeonPixelLab/Support'/('Support_Corner_'+side+'.png')).convert('RGBA')
   for d in range(84*scale if which=='candidate' else 0,limit):
    row=[tile.getpixel((x,27-(d//scale)%28)) for x in range(28)]
    n,delta=check_row(im,row,side,h-1-d,scale)
    assert delta<=1,(which,f.name,side,d,'side course or edge mismatch',delta)
    count+=n;max_delta=max(max_delta,delta)
   if which=='candidate':
    corner=Image.open(R.parent/'MatchedCorners/Exports'/('Masonry_Corner_'+side+'.png')).convert('RGBA')
    for d in range(84*scale):
     row=[corner.getpixel((x,85-d//scale)) for x in range(58)]
     n,delta=check_row(im,row,side,h-1-d,scale)
     assert delta<=1,(which,f.name,side,d,'corner placement mismatch',delta)
     count+=n;max_delta=max(max_delta,delta)
  profiles.append(dict(view=which,file=f.name,opaque_pixels_compared=count,max_channel_rounding=max_delta,side_edges_flush=True,courses_contiguous=True,layout_equal=True))
assert len(profiles)==12
report=dict(result='PASS',current_unity_tests=3,profiles=profiles,protected_files_unchanged=len(protected),main_user_files_unchanged=len(main),art_unchanged=True,generations_spent=0,physical_device_tested=False)
(R/'Diagnostics/PixelJoins.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS: native side/corner pixels, physical edges, shared course grid and unchanged UI in all 12 captures.')
