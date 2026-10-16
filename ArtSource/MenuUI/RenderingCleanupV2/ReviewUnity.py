"""Verify controlled captures against the exact native inputs and make sheets."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib,shutil,xml.etree.ElementTree as ET
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',17)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',12)
src=[Image.open(HERE/p/'HallTall.png').convert('RGB') for p in ['before','native']]
changed={(x,y) for y in range(800) for x in range(360) if src[0].getpixel((x,y))!=src[1].getpixel((x,y))}
xml=ET.parse(HERE/'evidence/unity-menu-tests.xml').getroot();assert xml.get('result')=='Passed'
report={'tests':[{k:n.get(k) for k in ['fullname','result','start-time','end-time']} for n in xml.iter('test-case')],
 'unity_version':'6000.3.19f1','native_source_hashes':{n:sha(HERE/n/'HallTall.png') for n in ['before','native']},'viewports':[]}
for height in [1920,2400]:
 images={}
 for mode in ['full','environment']:
  shots=[]
  for i,state in enumerate(['before','after']):
   path=HERE/'captures'/f'{height}-{state}-{mode}.png';shot=Image.open(path).convert('RGB');assert shot.size==(1080,height)
   if mode=='environment':
    expected=src[i].crop((0,0,360,height//3)).resize((1080,height),Image.Resampling.NEAREST)
    assert expected.tobytes()==shot.tobytes(),f'{path}: source/display mismatch'
   shots.append(shot)
  images[mode]=shots
  visible=set();unexpected=[]
  for y in range(height):
   for x in range(1080):
    if shots[0].getpixel((x,y))!=shots[1].getpixel((x,y)):
     q=x//3,y//3
     if q not in changed:unexpected.append((x,y))
     visible.add(q)
  assert not unexpected,(mode,unexpected[:10])
  if mode=='environment':assert visible=={q for q in changed if q[1]<height//3}
  report['viewports'].append({'size':[1080,height],'mode':mode,'visible_changed_native_cells':len(visible),
      'differences_outside_edited_background_pixels':0,'capture_sha256':[sha(HERE/'captures'/f'{height}-{s}-{mode}.png') for s in ['before','after']]})
  sheet=Image.new('RGB',(756,height//3+76),'#25232a');d=ImageDraw.Draw(sheet)
  d.text((12,10),'BEFORE · current V1',font=font,fill='white');d.text((384,10),'AFTER · V2 cleanup',font=font,fill='white')
  for x,im in [(12,shots[0]),(384,shots[1])]:sheet.paste(im.resize((360,height//3),Image.Resampling.NEAREST),(x,38))
  d.text((12,height//3+48),'Actual Unity capture · identical layout and character frame · shown on the native grid',font=small,fill='#c4b9ce')
  sheet.save(HERE/'review'/f'Unity-{height}-{mode}.png')
# Preserve measured full import report at this source version.
shutil.copyfile(ROOT/'ArtSource/MenuUI/validation/native-checks.json',HERE/'evidence/native-import-checks.json')
(HERE/'evidence/unity-pixel-checks.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({'tests_passed':len(report['tests']),'viewports':report['viewports']},indent=2))
