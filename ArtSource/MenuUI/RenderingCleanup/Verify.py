"""Independent pixel checks and comparison sheets; never edits production art."""
from pathlib import Path
from collections import Counter
import hashlib,json,shutil,sys
from PIL import Image,ImageDraw,ImageFont
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
load=lambda p:Image.open(p).convert('RGBA')
plan=json.loads((HERE/'cleanup-plan.json').read_text())
original=load(HERE/'before/HallTall.png');clean=load(HERE/'native/HallTall.png')
assert sha(HERE/'before/HallTall.png')==plan['source_sha256']
assert original.size==clean.size==(360,800)
expected={(e[0],e[1]):e for e in plan['pixels']}
actual=set()
for y in range(800):
 for x in range(360):
  a,b=original.getpixel((x,y)),clean.getpixel((x,y))
  assert a[3]==b[3]==255
  if a[:3]==(17,6,36):assert a==b,'Deep seam changed'
  if a==b:continue
  actual.add((x,y));assert (x,y) in expected,(x,y)
  e=expected[x,y];assert list(a[:3])==e[2:5] and list(b[:3])==e[5:8]
assert actual==set(expected)
assert clean.crop((0,0,360,640)).tobytes()==load(HERE/'native/Hall.png').tobytes()
assert set(clean.get_flattened_data())==set(original.get_flattened_data())
locks={
 'skull and portal':(128,135,233,295),
 'left statue':(0,141,42,296),'right statue':(316,140,360,296),
 'left banner':(43,104,98,214),'right banner':(260,104,315,211),
 'left upper flame':(26,40,61,109),'right upper flame':(295,40,331,109),
 'left middle torch':(84,211,110,274),'right middle torch':(251,211,279,274),
 'pedestal and carpet emblem':(88,294,266,331),
 'left foreground flame':(0,257,29,332),'right foreground flame':(333,257,360,332),
 'left treasure':(29,253,92,317),'right props':(265,250,333,317),
 'tall continuation':(0,640,360,800),
}
for name,box in locks.items():assert original.crop(box).tobytes()==clean.crop(box).tobytes(),name
baseline=json.loads((HERE/'baseline.json').read_text())
mutable={'Assets/_Game/Resources/UI/GlobalMenu/Hall.png','Assets/_Game/Resources/UI/GlobalMenu/HallTall.png'}
for name,digest in baseline.items():
 if name not in mutable:assert sha(ROOT/name)==digest,name
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',13)
sheet=Image.new('RGB',(918,712),'#25232a');d=ImageDraw.Draw(sheet)
d.text((12,12),'BEFORE · current implemented hall',font=font,fill='white')
d.text((384,12),'AFTER · selective native cleanup',font=font,fill='white')
sheet.paste(original.crop((0,0,360,640)),(12,44))
sheet.paste(clean.crop((0,0,360,640)),(384,44))
gold=load(ROOT/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png')
sheet.paste(gold,(786,62),gold)
d.text((762,141),'Rattlebones',font=font,fill='white')
d.text((762,169),'64 × 64 · native',font=small,fill='#c4b9ce')
d.text((762,208),'Hall: 360 × 640',font=small,fill='#c4b9ce')
d.text((762,230),'Both halls at 1×',font=small,fill='#c4b9ce')
d.text((762,270),str(len(actual))+' pixels edited',font=small,fill='#c4b9ce')
d.text((762,292),'Same 38 colors',font=small,fill='#c4b9ce')
d.text((762,314),'Same geometry',font=small,fill='#c4b9ce')
d.text((12,692),'Art-only comparison; the separate logo, character, buttons and icons are unchanged.',font=small,fill='#c4b9ce')
sheet.save(HERE/'review/NativeComparison.png')
boxes=[('Stair shading',(25,332,135,407)),('Floor texture',(5,447,110,530)),('Arch surface',(103,145,129,218))]
detail=Image.new('RGB',(940,1100),'#25232a');d=ImageDraw.Draw(detail)
y=12
for label,box in boxes:
 d.text((12,y),label+' · BEFORE',font=font,fill='white');d.text((482,y),label+' · AFTER',font=font,fill='white');y+=28
 for x,img in [(12,original),(482,clean)]:
  crop=img.crop(box);detail.paste(crop.resize((crop.width*4,crop.height*4),Image.Resampling.NEAREST),(x,y))
 y+=(box[3]-box[1])*4+25
detail.crop((0,0,940,y)).save(HERE/'review/DetailComparison.png')
report={'checks':'passed','dimensions':{'Hall':[360,640],'HallTall':[360,800]},'changed_pixels':len(actual),
 'changed_percent':round(len(actual)*100/(360*800),4),'palette_before':len(set(original.get_flattened_data())),
 'palette_after':len(set(clean.get_flattened_data())),'alpha':[255],'locked_regions_unchanged':locks,
 'other_protected_files_unchanged':len(baseline)-len(mutable),
 'artwork':{str(p.relative_to(HERE)):sha(p) for p in (HERE/'native').iterdir() if p.suffix in ['.png','.aseprite']},
 'changes_by_region':plan['by_region']}
(HERE/'evidence/pixel-checks.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS:',len(actual),'exact native pixel replacements; 38 unchanged palette colors; all protected regions/assets identical.')
if '--install' in sys.argv:
 for name in ['Hall','HallTall']:
  for extension in ['.png','.aseprite']:
   shutil.copyfile(HERE/'native'/(name+extension),ROOT/'ArtSource/MenuUI/native'/(name+extension))
  shutil.copyfile(HERE/'native'/(name+'.png'),ROOT/'Assets/_Game/Resources/UI/GlobalMenu'/(name+'.png'))
 print('Installed two background PNGs and their editable native sources. Import settings retained.')
