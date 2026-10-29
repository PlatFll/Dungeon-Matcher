from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
import json,hashlib,math
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');S=F/'ArtSource/SnowyMountainStaged/Stage03'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
records=[]
for r in json.loads((S/'Preview/Measurements.json').read_text())['items']:
 key=r['size']; a=Image.open(S/f'Preview/Before/{key}.png').convert('RGB');b=Image.open(S/f'Preview/After/{key}.png').convert('RGB')
 diff=ImageChops.difference(a,b);mask=diff.convert('L').point(lambda p:255 if p else 0)
 # Use channel maximum so one-unit channel changes cannot disappear in luminance conversion.
 mask=ImageChops.lighter(ImageChops.lighter(*diff.split()[:2]),diff.split()[2]).point(lambda p:255 if p else 0)
 f=r['frame'];u=r['nativePixelScale'];x,y,w,h=[f[k] for k in ('x','y','width','height')]
 allowed=(math.floor(x-u),math.floor(a.height-y-h-4*u),math.ceil(x+w+u),math.ceil(a.height-y+u))
 outside=mask.copy();ImageDraw.Draw(outside).rectangle(allowed,fill=0)
 n=sum(v>0 for v in outside.get_flattened_data());total=sum(v>0 for v in mask.get_flattened_data())
 records.append(dict(size=key,changedPixels=total,outsidePortraitAndSnow= n,allowedPixelBounds=allowed,changedBounds=mask.getbbox(),outsideBounds=outside.getbbox()))
 if n:outside.save(S/f'QA/UnexpectedDifference-{key}.png')
(S/'QA/ScreenDifferences.json').write_text(json.dumps(records,indent=2))
base=json.loads((F/'ArtSource/SnowyMountainStaged/Stage01/Baseline.json').read_text())
changed=[]
for rel,expected in base['protectedFiles'].items():
 actual=sha(F/rel)
 if actual!=(expected['sha256'] if isinstance(expected,dict) else expected):changed.append(rel)
existing={r:sha(F/r)==e['sha256'] for r,e in base['preexistingEdits'].items()}
starting=json.loads((S/'QA/StartingHashes.json').read_text())
stage2changed=[r for r,e in starting.items() if sha(F/r)!=e]
expected_code_changes={'Assets/_Game/Scripts/UI/GameplayPixelLayoutValidator.cs','Assets/_Game/Scripts/UI/ResponsiveModularFrameFitter.cs'}
result=dict(protectedProductionFilesChecked=len(base['protectedFiles']),changedProductionFiles=changed,expectedCodeCorrections=sorted(expected_code_changes),unchangedProductionFiles=len(base['protectedFiles'])-len(changed),preexistingEditsPreserved=existing,stage2AndStartingFilesChecked=len(starting),stage2AndStartingFilesChanged=stage2changed)
(S/'QA/Preservation.json').write_text(json.dumps(result,indent=2))
print(json.dumps(records,indent=2));print(json.dumps(result,indent=2))
assert set(changed)==expected_code_changes and all(existing.values()) and not stage2changed
assert all(r['outsidePortraitAndSnow']==0 for r in records)
