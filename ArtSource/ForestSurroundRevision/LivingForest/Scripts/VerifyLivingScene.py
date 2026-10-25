from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
import json,hashlib,xml.etree.ElementTree as ET,shutil,re,yaml
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/LivingForest';T=F/'.utmp/living-forest';A=F/'Assets/_Game/Art/ForestSurroundRevision/LivingForest'
root=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert root.attrib['result']=='Passed'
e=json.loads((R/'Diagnostics/MotionEvidence.json').read_text());assert e['samples']==288 and e['pauseStable'] and e['reducedMotionStable']
b=Image.open(R/'Previews/Unity-Before.png').convert('RGB');a=Image.open(R/'Previews/Unity-Final-720x1360.png').convert('RGB')
assert b.tobytes()==Image.open(R/'Previews/Before.png').convert('RGB').tobytes(),'Baseline review changed'
large=Image.open(R/'Previews/Unity-Final-1440x2720.png').convert('RGB');assert large.tobytes()==a.resize(large.size,Image.Resampling.NEAREST).tobytes()
allowed=Image.new('L',b.size);draw=ImageDraw.Draw(allowed)
# Track rectangles include all native poses. Drift gets a bounded six-pixel native margin.
for t in e['tracks']:
 rr=t['rect'];x=round(rr['x']);y=1360-round(rr['y']+rr['height']);w=round(rr['width']);h=round(rr['height']);pad=12 if t['kind']=='Light' else 0
 draw.rectangle((x-pad,y-pad,x+w+pad-1,y+h+pad-1),fill=255)
def putmask(mask,x,y):
 im=Image.open(mask).convert('L');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);allowed.paste(im,(x*2,y*2),im)
putmask(R/'Diagnostics/RemovedSprigMask.png',272,500)
putmask(R/'Diagnostics/SeparatedTopMotesMask.png',0,0)
for q in json.loads((F/'ArtSource/ForestSurroundRevision/PolishFinal/Scripts/Placements.json').read_text())['pieces']:
 if q['stage']==5:
  im=Image.open(F/'Assets/_Game/Art/ForestSurroundRevision/PolishFinal'/(q['file']+'.png')).getchannel('A');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST)
  for dy in [-2,0,2]:allowed.paste(im,(q['x']*2,q['y']*2+dy),im)
hud=Image.open(F/'ArtSource/ForestSurroundRevision/UnifiedCorners/Diagnostics/ProtectedHudPixels.png').convert('L')
frame=Image.new('L',b.size)
for q in json.loads((F/'ArtSource/EnchantedForestStaged/Stage04/Diagnostics/Unity-720x1360.json').read_text())['images']:
 if not q['frame']:continue
 rr=q['rect'];x,y=round(rr['x']),1360-round(rr['y']+rr['height']);w,h=round(rr['width']),round(rr['height']);factor=round(q['scale']);src=Image.open(F/q['path']).getchannel('A');src=src.resize((src.width*factor,src.height*factor),Image.Resampling.NEAREST)
 if q['type']=='Tiled':
  tile=Image.new('L',(w,h))
  for yy in range(0,h,src.height):
   for xx in range(0,w,src.width):tile.paste(src,(xx,yy))
 else:assert src.size==(w,h);tile=src
 frame.paste(tile,(x,y),tile)
off=ImageChops.invert(allowed);offrgb=Image.merge('RGB',(off,)*3);hudrgb=Image.merge('RGB',(hud,)*3);framergb=Image.merge('RGB',(frame,)*3)
# Protect each top plaque and skull, not the newly authorized waterfall in the gap below WAVE.
boxes=[(16,16,212,456),(104,528,616,1040),(236,1160,484,1248),(262,1100,458,1136),(220,1278,501,1308),(248,20,455,85),(490,36,637,99),(644,36,703,100),(342,86,367,115)]
def check(im,name):
 diff=ImageChops.difference(b,im)
 assert ImageChops.multiply(diff,offrgb).getbbox()is None,('Outside motion scope',name,ImageChops.multiply(diff,offrgb).getbbox())
 assert ImageChops.multiply(diff,hudrgb).getbbox()is None,('HUD changed',name,ImageChops.multiply(diff,hudrgb).getbbox())
 assert ImageChops.multiply(diff,framergb).getbbox()is None,('Frame changed',name)
 for box in boxes:assert b.crop(box).tobytes()==im.crop(box).tobytes(),('Locked region',name,box)
check(a,'final');check(Image.open(R/'Previews/Unity-ReducedMotion.png').convert('RGB'),'reduced')
frames=sorted((T/'capture').glob('Frame-*.png'));assert len(frames)==288,len(frames)
for p in frames:check(Image.open(p).convert('RGB'),p.name)
prior=json.loads((R/'Diagnostics/PreservedFiles.json').read_text());changed=[p for p,h in prior.items()if hashlib.sha256((F/p).read_bytes()).hexdigest()!=h];assert not changed,changed
allowed.save(R/'Diagnostics/AllowedMotionRegions.png')
def scene(path):
 chunks=re.split(r'^--- !u!\d+ &(\d+)\s*$',path.read_text(encoding='utf-8'),flags=re.M)
 return {k:yaml.safe_load(v)for k,v in zip(chunks[1::2],chunks[2::2])}
old=scene(F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_FinalPolish.unity');new=scene(F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_Living.unity');assert set(old)<=set(new)
changes=[]
for k,v in old.items():
 if v!=new[k]:
  kind=next(iter(v));body=v[kind];go=body.get('m_GameObject',{}).get('fileID');name=body.get('m_Name') or old.get(str(go),{}).get('GameObject',{}).get('m_Name',kind)
  changes.append(dict(id=k,type=kind,name=name,changed_fields=[f for f in body if body[f]!=new[k][kind].get(f)]))
(R/'Diagnostics/SceneAudit.json').write_text(json.dumps(dict(old_objects=len(old),new_objects=len(set(new)-set(old)),removed_objects=0,changes=changes),indent=2))
for n in [0,24,48,96,144,192,240,287]:shutil.copy2(frames[n],R/'Previews'/('Unity-Motion-'+str(n).zfill(3)+'.png'))
comparison=Image.new('RGB',(1448,412),'#17221d');d=ImageDraw.Draw(comparison);d.text((8,8),'BEFORE',fill='white');d.text((736,8),'LIVING FOREST COPY',fill='white');comparison.paste(b.crop((0,980,720,1360)),(0,32));comparison.paste(a.crop((0,980,720,1360)),(728,32));comparison.save(R/'Previews/BeforeAfter-Lower.png')
summary=dict(original_files_preserved=len(prior),all_original_sources_unchanged=True,old_scene_unchanged=True,protected_image_bindings=e['protectedImages'],pixel_doubling_exact=True,checked_unity_frames=len(frames),board_portrait_hud_frame_unchanged=True,changes_outside_motion_regions=0,native_poses=43,paused_and_reduced_motion_stable=True,production_integrated=False,device_test=False)
(R/'Diagnostics/Validation.json').write_text(json.dumps(summary,indent=2));print(json.dumps(summary,indent=2))
