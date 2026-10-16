"""Capture the current V1 source and measure where its edits were visible."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib,shutil
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for name in ['before','pilot','native','review','logs','evidence']:(HERE/name).mkdir(exist_ok=True)
baseline=HERE/'baseline.json'
if not baseline.exists():
 protected=json.loads((ROOT/'ArtSource/MenuUI/RenderingCleanup/baseline.json').read_text())
 protected={p:sha(ROOT/p) for p in protected}
 for name in ['Hall','HallTall']:
  for ext in ['.aseprite','.png']:shutil.copyfile(ROOT/'ArtSource/MenuUI/native'/(name+ext),HERE/'before'/(name+ext))
 baseline.write_text(json.dumps(protected,indent=2)+'\n')
assert sha(ROOT/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png')=='705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c'
assert sha(ROOT/'Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png')=='59b04929e9fdc15ae9f1d70fd3c85c6ea5362bdac479eb12f45cfcbb0efe4306'
prev=ROOT/'ArtSource/MenuUI/RenderingCleanup'
plan=json.loads((prev/'cleanup-plan.json').read_text())
oldshot=Image.open(prev/'before/runtime/1920-main-menu.png').convert('RGB')
newshot=Image.open(prev/'after/runtime/1920-main-menu.png').convert('RGB')
oldsrc=Image.open(prev/'before/HallTall.png').convert('RGBA');newsrc=Image.open(HERE/'before/HallTall.png').convert('RGBA')
visible=[];hidden=[];counts={}
for x,y,*rgb in plan['pixels']:
 key='visible' if y<640 and oldshot.getpixel((x*3+1,y*3+1))==tuple(rgb[:3]) and newshot.getpixel((x*3+1,y*3+1))==tuple(rgb[3:]) else 'occluded_or_unconfirmed'
 (visible if key=='visible' else hidden).append([x,y])
assert sum(a!=b for a,b in zip(oldsrc.get_flattened_data(),newsrc.get_flattened_data()))==693
audit={'binding':'MainMenuArtwork.Initialize -> GlobalMenuSkin.Art("MenuDungeonArtwork", ..., "HallTall", (360,800), (0,-80))',
 'production':'Assets/_Game/Resources/UI/GlobalMenu/HallTall.png','guid':'8e5d07dcb9fc8ff408651dcfbd4b404a',
 'dimensions':[360,800],'palette':len(set(newsrc.get_flattened_data())),
 'previous_changed_pixels':693,'verified_visible_at_1080x1920':len(visible),'occluded_or_unconfirmed':len(hidden),
 'source_and_production_bytes_equal':sha(HERE/'before/HallTall.png')==sha(ROOT/'Assets/_Game/Resources/UI/GlobalMenu/HallTall.png'),
 'previous_edit_limit':'Only detached 1–14-pixel fragments, <=7px span, surrounded by selected material colors; broader connected noisy shading remained.',
 'animated_layers':'Rattlebones Animator uses UnscaledTime; prior screenshots captured different idle frames. Hall flames and portal are baked static pixels. Logo/UI are separate static layers.',
 'guide_sha256':sha(ROOT/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt'),
 'register_sha256':sha(ROOT/'Docs/ArtDirection/Golden_Reference_Register.md')}
if not (HERE/'evidence/source-audit.json').exists():
 (HERE/'evidence/source-audit.json').write_text(json.dumps(audit,indent=2,ensure_ascii=False)+'\n')
print(json.dumps(audit,indent=2,ensure_ascii=False))
# Inspection-only coordinate sheets; never used as production art.
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',12)
for name,box in [('Wall',(275,210,319,253)),('Stairs',(0,330,142,424)),('Pedestal',(88,294,144,331)),('FloorLeft',(0,420,115,560)),('Arch',(103,145,131,216)),('RightStairs',(218,330,360,424)),('RightFloor',(245,420,360,560)),('UpperLeft',(0,0,32,135))]:
 crop=newsrc.crop(box);z=8;sheet=crop.resize((crop.width*z,crop.height*z),Image.Resampling.NEAREST);d=ImageDraw.Draw(sheet)
 for x in range((box[0]+4)//5*5,box[2],5):
  xx=(x-box[0])*z;d.line((xx,0,xx,sheet.height),fill='#747474');d.text((xx+1,0),str(x),fill='white',font=font)
 for y in range((box[1]+4)//5*5,box[3],5):
  yy=(y-box[1])*z;d.line((0,yy,sheet.width,yy),fill='#747474');d.text((0,yy),str(y),fill='white',font=font)
 sheet.save(HERE/'logs'/(name+'Grid.png'))
