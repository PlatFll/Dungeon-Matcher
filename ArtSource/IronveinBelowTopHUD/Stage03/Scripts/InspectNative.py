from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
F=Path('C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage03'
items=[];sheet=Image.new('RGBA',(780,410),'#20222d');d=ImageDraw.Draw(sheet)
for i,p in enumerate(sorted((R/'Exports').glob('*.png'))):
 im=Image.open(p).convert('RGBA'); rt=Image.open(R/'Roundtrip'/p.name).convert('RGBA'); assert im.tobytes()==rt.tobytes()
 x=10+(i%5)*155;y=10+(i//5)*200
 d.text((x,y),p.stem.replace('Board',''),fill='white');d.text((x,y+16),f'{im.width}x{im.height}',fill='#a9b7c5');sheet.alpha_composite(im,(x,y+38));sheet.alpha_composite(im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST),(x,y+78))
 alpha=sorted(set(im.getchannel('A').get_flattened_data()));assert set(alpha)<={0,255}
 if 'Rail' in p.stem:
  if im.width>im.height: assert list(im.crop((0,0,1,im.height)).get_flattened_data())==list(im.crop((im.width-1,0,im.width,im.height)).get_flattened_data())
  else: assert list(im.crop((0,0,im.width,1)).get_flattened_data())==list(im.crop((0,im.height-1,im.width,im.height)).get_flattened_data())
 items.append({'name':p.stem,'size':im.size,'opaque_colors':len({c[:3] for c in im.get_flattened_data() if c[3]}),'alpha':alpha,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'roundtrip_exact':True})
sheet.save(R/'Preview/NativeAssets.png');(R/'QA/Assets.json').write_text(json.dumps({'status':'PASS','assets':items,'native_edges_match':True,'palette':sorted({'#%02x%02x%02x'%c[:3] for p in (R/'Exports').glob('*.png') for c in Image.open(p).convert('RGBA').get_flattened_data() if c[3]})},indent=2));print('Native QA PASS',len(items),'assets')
