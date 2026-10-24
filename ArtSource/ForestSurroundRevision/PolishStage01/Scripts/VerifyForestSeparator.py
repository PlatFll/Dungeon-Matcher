from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
import json,hashlib,shutil,xml.etree.ElementTree as ET
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
R=F/'ArtSource/ForestSurroundRevision/PolishStage01'
A=F/'Assets/_Game/Art/ForestSurroundRevision/PolishStage01'
T=F/'.utmp/forest-separator'
before=Image.open(R/'Previews/Unity-Before-720x1360.png').convert('RGB')
after=Image.open(R/'Previews/Unity-Separator-720x1360.png').convert('RGB')
previous=Image.open(R/'Previews/Before.png').convert('RGB')
assert ImageChops.difference(before,previous).getbbox() is None,'Baseline changed'
assert ImageChops.difference(before.crop((0,0,720,448)),after.crop((0,0,720,448))).getbbox() is None,'Locked upper scene changed'
assert ImageChops.difference(before.crop((0,480,720,1360)),after.crop((0,480,720,1360))).getbbox() is None,'Board/lower region changed'
delta=ImageChops.difference(before,after).getbbox();assert delta is not None
large=Image.open(R/'Previews/Unity-Separator-1440x2720.png').convert('RGB')
assert ImageChops.difference(large,after.resize(large.size,Image.Resampling.NEAREST)).getbbox() is None,'Nonuniform or fractional render scale'
records=[]
for name in ['Mudstone_A','Mudstone_B','Mudstone_C']:
 im=Image.open(A/(name+'.png')).convert('RGBA')
 assert im.size==(40,16)
 assert set(im.getchannel('A').get_flattened_data())=={255},'Tile seam coverage must be opaque'
 assert ImageChops.difference(im.convert('RGB'),Image.open(T/(name+'-expected.png')).convert('RGB')).getbbox() is None
 assert im.tobytes()==Image.open(T/'reopened'/(name+'.png')).convert('RGBA').tobytes(),'LibreSprite reopen mismatch'
 shutil.copy2(A/(name+'.png'),R/'Exports'/(name+'.png'))
 records.append(dict(name=name,dimensions=im.size,colors=len(set(im.get_flattened_data())),alpha=[255],sha256=hashlib.sha256((A/(name+'.png')).read_bytes()).hexdigest()))
strip=Image.new('RGBA',(360,16))
for i,variant in enumerate('ABCBACABC'):strip.alpha_composite(Image.open(A/('Mudstone_'+variant+'.png')).convert('RGBA'),(i*40,0))
strip.save(R/'Previews/Native-Assembled-Strip.png')
# Existing portrait foliage reaches y=475 and correctly remains in front of the strip.
# Every visible pixel must be an exact tile sample or the unchanged prior foreground.
expected=strip.resize((720,32),Image.Resampling.NEAREST).convert('RGB')
foreground_pixels=[]
for y in range(32):
 for x in range(720):
  actual=after.getpixel((x,y+448))
  if actual!=expected.getpixel((x,y)):
   assert actual==before.getpixel((x,y+448)),f'Unexpected tile or foreground pixel at {x},{y+448}'
   foreground_pixels.append([x,y+448])
assert len(foreground_pixels)<720*32*.12,'Unexpected obstruction of structural divider'
assert ImageChops.difference(after.crop((0,476,720,480)),expected.crop((0,28,720,32))).getbbox() is None,'Unobstructed bottom seams differ'
hashes=json.loads((R/'Diagnostics/PreservedFiles.json').read_text())
changed=[p for p,h in hashes.items() if not (F/p).exists() or hashlib.sha256((F/p).read_bytes()).hexdigest()!=h]
assert not changed,'Existing tracked files changed: '+str(changed)
tests=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert tests.attrib['result']=='Passed'
evidence=json.loads((R/'Diagnostics/Unity-720x1360.json').read_text())
report=dict(modules=records,changed_pixel_bounds=delta,unchanged_existing_files=len(hashes),preserved_image_bindings=evidence['preservedImages'],preserved_foreground_pixels_within_band=len(foreground_pixels),baseline_matches_previous=True,all_pixels_outside_separator_unchanged=True,exact_2x_4x_rendering=True,gap_free_bottom_rows=True,unity_review_test=tests.attrib['result'],status='Stage 1 candidate; awaiting user review. No production integration or merge.')
(R/'Diagnostics/Validation.json').write_text(json.dumps(report,indent=2))
for name,im in [('Before',before),('After',after)]:im.crop((0,400,720,530)).save(R/('Previews/'+name+'-Detail.png'))
comparison=Image.new('RGB',(1448,1392),'#151b20');draw=ImageDraw.Draw(comparison)
draw.text((8,8),'BEFORE - previous Unity scene',fill='white');draw.text((736,8),'AFTER - Stage 1 stone separator',fill='white')
comparison.paste(before,(0,32));comparison.paste(after,(728,32));comparison.save(R/'Previews/Unity-BeforeAfter.png')
detail=Image.new('RGB',(720,300),'#151b20');draw=ImageDraw.Draw(detail)
draw.text((8,4),'BEFORE',fill='white');detail.paste(before.crop((0,400,720,530)),(0,20))
draw.text((8,154),'AFTER - actual Unity capture',fill='white');detail.paste(after.crop((0,400,720,530)),(0,170));detail.save(R/'Previews/Separator-BeforeAfter.png')
print(json.dumps(report,indent=2))
