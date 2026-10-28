from pathlib import Path
from PIL import Image,ImageDraw,ImageOps
import json,hashlib,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage06'
assets=[];colors=set();sheet=Image.new('RGBA',(900,390),'#20232c');d=ImageDraw.Draw(sheet)
for i,p in enumerate(sorted((R/'Exports').glob('*.png'))):
 im=Image.open(p).convert('RGBA');rt=Image.open(R/'Roundtrip'/p.name).convert('RGBA');assert im.tobytes()==rt.tobytes()
 cs={c[:3] for c in im.get_flattened_data() if c[3]};alpha=set(im.getchannel('A').get_flattened_data());assert alpha<={0,255};colors|=cs
 x=i%3*300+10;y=i//3*190+10;d.text((x,y),f'{p.stem}: {im.width}x{im.height}, {len(cs)} colors',fill='white');sheet.alpha_composite(im,(x+240,y+28));sheet.alpha_composite(im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST),(x,y+32))
 assets.append({'name':p.stem,'size':im.size,'bounds':im.getbbox(),'opaque_colors':len(cs),'alpha':sorted(alpha),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'libresprite_roundtrip_exact':True})
assert len(assets)==5
rail=Image.open(R/'Exports/SeatRail.png');assert rail.crop((0,0,1,7)).tobytes()==rail.crop((23,0,24,7)).tobytes()
post=Image.open(R/'Exports/SeatPost.png');assert post.crop((0,0,7,1)).tobytes()==post.crop((0,23,7,24)).tobytes()
left=Image.open(R/'Exports/CorbelLeft.png');right=Image.open(R/'Exports/CorbelRight.png');assert ImageOps.mirror(left).getchannel('A').tobytes()==right.getchannel('A').tobytes()
sheet.save(R/'Preview/NativeAssets.png');(R/'QA/Assets.json').write_text(json.dumps({'status':'PASS','assets':assets,'rail_and_post_repeat_edges_match':True,'corbel_silhouettes_mirrored':True,'palette':['#%02x%02x%02x'%c for c in sorted(colors)]},indent=2))
target=R/'Scripts/InspectNative.py'
if Path(__file__).resolve()!=target.resolve():shutil.copyfile(__file__,target)
print('5 assets pass binary alpha, exact LibreSprite roundtrip and repeat-edge checks; total colors',len(colors))
