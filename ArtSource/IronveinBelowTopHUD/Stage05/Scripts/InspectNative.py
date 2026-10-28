from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil
def copy_script(source,target):
 if Path(source).resolve()!=Path(target).resolve():shutil.copyfile(source,target)
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage05'
assets=[];allcolors=set();sheet=Image.new('RGBA',(1000,650),'#20232c');d=ImageDraw.Draw(sheet)
for i,p in enumerate(sorted((R/'Exports').glob('*.png'))):
 im=Image.open(p).convert('RGBA');rt=Image.open(R/'Roundtrip'/p.name).convert('RGBA');assert im.tobytes()==rt.tobytes()
 colors={c[:3] for c in im.get_flattened_data() if c[3]};alpha=set(im.getchannel('A').get_flattened_data());assert alpha<={0,255};allcolors|=colors
 x=i%3*330+10;y=i//3*212+10;d.text((x,y),f'{p.stem}: {im.width}x{im.height}, {len(colors)} colors',fill='white');sheet.alpha_composite(im,(x+240,y+28));sheet.alpha_composite(im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST),(x,y+32))
 assets.append({'name':p.stem,'size':im.size,'bounds':im.getbbox(),'opaque_colors':len(colors),'alpha':sorted(alpha),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'libresprite_roundtrip_exact':True})
repeat=Image.open(R/'Exports/TimberRepeat.png');assert repeat.crop((0,0,20,1)).tobytes()==repeat.crop((0,23,20,24)).tobytes()
course=Image.open(R/'Exports/RockCourse.png');assert course.crop((0,0,1,16)).tobytes()==course.crop((63,0,64,16)).tobytes()
sheet.save(R/'Preview/NativeAssets.png');(R/'QA/Assets.json').write_text(json.dumps({'status':'PASS','assets':assets,'timber_and_rock_repeat_edges_match':True,'palette':['#%02x%02x%02x'%c for c in sorted(allcolors)]},indent=2));copy_script(__file__,R/'Scripts/InspectNative.py')
print('9 assets pass binary alpha, exact LibreSprite roundtrip and repeat-edge checks; total colors',len(allcolors))
