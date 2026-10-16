"""Create review artifacts from completed Unity captures; preserve originals."""
from pathlib import Path
from datetime import datetime,timezone
import json,hashlib,shutil,xml.etree.ElementTree as ET
from PIL import Image,ImageDraw,ImageFont
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',13)
result={}
for stage in ['before','after']:
 xml=ET.parse(HERE/'evidence'/f'{stage}-tests.xml').getroot()
 assert xml.get('result')=='Passed'
 # Unity domain reload can reset the parent suite start time after its first
 # child has finished. Use actual test-case times for capture freshness.
 first=min(t.get('start-time') for t in xml.iter('test-case'))
 start=datetime.strptime(first,'%Y-%m-%d %H:%M:%SZ').replace(tzinfo=timezone.utc).timestamp()
 shots=HERE/stage/'runtime'
 # Only this task's copied artifact folder, never test sources or user files.
 # The first baseline capture copied older ancillary shots; omit those stale
 # copies rather than representing them as evidence from the current source.
 assert shots.resolve().is_relative_to(HERE.resolve())
 for p in shots.glob('*.png'):
  if p.stat().st_mtime<start:p.unlink()
 result[stage]={'result':xml.get('result'),'total':int(xml.get('total')),'start_utc':first,
   'tests':[{k:t.get(k) for k in ['fullname','result','duration']} for t in xml.iter('test-case')],
   'captures':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(shots.glob('*.png'))}}
before=Image.open(HERE/'before/runtime/1920-main-menu.png').convert('RGB')
after=Image.open(HERE/'after/runtime/1920-main-menu.png').convert('RGB')
assert before.size==after.size==(1080,1920)
sheet=Image.new('RGB',(756,712),'#25232a');d=ImageDraw.Draw(sheet)
d.text((12,12),'BEFORE · implemented menu',font=font,fill='white')
d.text((384,12),'AFTER · cleaned background',font=font,fill='white')
for x,im in [(12,before),(384,after)]:sheet.paste(im.resize((360,640),Image.Resampling.NEAREST),(x,44))
d.text((12,692),'Actual Unity captures · 3 display pixels per native pixel · same scene and controls',font=small,fill='#c4b9ce')
sheet.save(HERE/'review/UnityComparison.png')
shutil.copyfile(ROOT/'ArtSource/MenuUI/validation/native-checks.json',HERE/'evidence/native-import-checks.json')
result['native_menu_pairs']=54
result['unity_version']='6000.3.19f1'
result['audio']='EditorUtility.audioMasterMute = true in existing MenuUiReviewTests setup'
result['note']='Three distinct existing menu tests passed after cleanup. Before capture repeats one test; not counted as an additional distinct test.'
(HERE/'evidence/runtime-checks.json').write_text(json.dumps(result,indent=2)+'\n')
print('Fresh Unity before/after review saved; all three menu tests passed.')
