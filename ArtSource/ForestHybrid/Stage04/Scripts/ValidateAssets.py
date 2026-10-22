from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import json,hashlib,shutil
F=Path('C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestHybrid/Stage04';A=F/'Assets/_Game/Art/ForestHybrid/Stage04'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
plan=json.loads((R/'Scripts/NativePixelPlan.json').read_text());results=[]
board=Image.new('RGB',(800,370),'#172024');d=ImageDraw.Draw(board)
d.text((16,12),'Stage 4 / raw PixelLab (left) and LibreSprite cleanup (right)',fill='#eee7d9')
for idx,a in enumerate(plan['assets']):
 n=a['name'];im=Image.open(A/(n+'.png')).convert('RGBA');reopened=Image.open(F/'.utmp/forest-stage04/reopened'/(n+'.png')).convert('RGBA')
 assert im.tobytes()==reopened.tobytes(),n+' reopen'
 expected=Image.new('RGBA',(a['w'],a['h']))
 for y,row in enumerate(a['rows']):
  for start,count,h in row:
   for x in range(start,start+count):expected.putpixel((x,y),tuple(bytes.fromhex(h)))
 assert im.tobytes()==expected.tobytes(),n+' exact editor export'
 pixels=list(im.get_flattened_data());alpha=sorted({p[3] for p in pixels});assert alpha==[0,255],n
 assert all(p==(0,0,0,0) for p in pixels if not p[3]),n+' hidden RGB'
 b=im.getbbox();assert b[0]>=2 and b[1]>=2 and b[2]<=im.width-2 and b[3]<=im.height-2,n+' padding'
 raw=Image.open(R/'Previews'/('Raw-'+n+'.png')).convert('RGBA')
 colors=sorted({bytes(p[:3]).hex() for p in pixels if p[3]})
 results.append({'name':n,'size':list(im.size),'occupied_bounds':b,'opaque_colors':len(colors),'palette':colors,'alpha':alpha,'reopened_pixels_equal':True,'source_scale_preserved':True,'sha256':sha(A/(n+'.png')),'changed_pixels':sum(a!=b for a,b in zip(pixels,raw.get_flattened_data()))})
 x=16+idx*264;d.text((x,36),n,fill='#eee7d9');d.text((x,52),str(im.width)+'x'+str(im.height)+' / '+str(len(colors))+' colors',fill='#acbcb6')
 board.paste(raw.resize((raw.width*3,raw.height*3),Image.Resampling.NEAREST),(x,76),raw.resize((raw.width*3,raw.height*3),Image.Resampling.NEAREST))
 board.paste(im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST),(x+112,76),im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST))
 board.paste(im,(x+112,286),im);d.text((x,306),'1x ->',fill='#acbcb6')
 shutil.copyfile(A/(n+'.png'),R/'Previews'/(n+'.png'))
 im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST).save(R/'Previews'/(n+'-4x.png'))
board.save(R/'Previews/PropSheet.png')
out={'assets':results,'generation_charge':5,'forest_total_generations':26,'native_editor':'LibreSprite batch scripts; all documents reopened and exported pixel-identical','preserved':None}
if (R/'Previews/Unity-Board-NoProps.png').exists():
 for study,w,h,old in [('Upper',470,288,'Stage02/Previews/Unity-portrait-470x288.png'),('Board',608,608,'Stage03/Previews/Unity-board-608x608.png')]:
  native=Image.open(R/'Previews'/('Unity-'+study+'-'+str(w)+'x'+str(h)+'.png')).convert('RGB')
  off=Image.open(R/'Previews'/('Unity-'+study+'-NoProps.png')).convert('RGB')
  approved=Image.open(F/'ArtSource/ForestHybrid'/old).convert('RGB')
  assert off.tobytes()==approved.tobytes(),study+' approved assembly changed with props off'
  for scale in [2,3]:
   big=Image.open(R/'Previews'/('Unity-'+study+'-'+str(w*scale)+'x'+str(h*scale)+'.png')).convert('RGB')
   assert big.tobytes()==native.resize(big.size,Image.Resampling.NEAREST).tobytes(),study+' integer scale'
  # Independent composition of exported PNGs proves no scaling, hidden crop or unintended base changes.
  composed=off.convert('RGBA')
  for p in json.loads((R/'Placements.json').read_text())['placements']:
   if p['study']==study:composed.alpha_composite(Image.open(A/(p['asset']+'.png')).convert('RGBA'),(p['x'],p['y']))
  assert composed.convert('RGB').tobytes()==native.tobytes(),study+' source-pixel composition'
  protected=(48,48,560,560) if study=='Board' else (20,36,131,270)
  assert off.crop(protected).tobytes()==native.crop(protected).tobytes(),study+' protected interior changed'
  if study=='Upper':
   for b in [(213,174,325,262),(320,174,432,262)]:
    assert off.crop(b).tobytes()==native.crop(b).tobytes(),'cast area changed'
  out[study]={'baseline_capture_identical':True,'integer_2x_3x_exact':True,'native_source_composite_exact':True,'protected_interiors_unchanged':True}
 protected=json.loads((R/'Diagnostics/StartingState.json').read_text())['protected']
 changed=[p for p,h in protected.items() if sha(F/p)!=h]
 out['preserved']={'count':len(protected),'changed':changed};assert not changed,changed
(R/'Diagnostics/Validation.json').write_text(json.dumps(out,indent=2))
shutil.copyfile(__file__,R/'Scripts/ValidateAssets.py')
print(json.dumps(out,indent=2))
