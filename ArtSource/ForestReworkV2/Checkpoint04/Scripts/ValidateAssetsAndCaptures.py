from pathlib import Path
from PIL import Image
import hashlib,json,shutil,sys
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint04';a=p/'Assets/_Game/Art/ForestReworkV2/Checkpoint04';s=p/'.utmp/forest-v2-c4'
sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
records=[];colors=set()
for f in sorted(a.glob('*.png')):
 im=Image.open(f).convert('RGBA');planned=Image.open(s/(f.stem+'-expected.png')).convert('RGBA');reopened=Image.open(s/'reopened'/f.name).convert('RGBA')
 assert im.tobytes()==planned.tobytes()==reopened.tobytes(),f.name
 assert set(im.getchannel('A').getdata())<={0,255};assert all(c[3] or c==(0,0,0,0) for c in im.getdata())
 palette={c[:3] for c in im.getdata() if c[3]};colors|=palette;b=im.getbbox()
 if 'Foliage' in f.stem:assert b[0]>=2 and b[1]>=2 and b[2]<=im.width-2 and b[3]<=im.height-2,(f.name,b)
 if 'Rail' in f.stem:
  if im.width>im.height:assert im.crop((0,0,1,im.height)).tobytes()==im.crop((im.width-1,0,im.width,im.height)).tobytes()
  else:assert im.crop((0,0,im.width,1)).tobytes()==im.crop((0,im.height-1,im.width,im.height)).tobytes()
 records.append({'name':f.stem,'size':im.size,'opaque_colors':len(palette),'alpha':'binary','bounds':b,'sha256':sha(f),'libresprite_reopen_exact':True});shutil.copy2(f,o/'Previews'/f.name)
for cn,side,cb,rb in [('TL','Top',(22,6,23,17),(0,0,1,11)),('TR','Top',(0,6,1,17),(0,0,1,11)),('BL','Bottom',(22,6,23,17),(0,0,1,11)),('BR','Bottom',(0,6,1,17),(0,0,1,11)),('TL','Left',(6,22,17,23),(0,0,11,1)),('BL','Left',(6,0,17,1),(0,0,11,1)),('TR','Right',(6,22,17,23),(0,0,11,1)),('BR','Right',(6,0,17,1),(0,0,11,1))]:
 assert Image.open(a/('Board_Corner_'+cn+'.png')).crop(cb).tobytes()==Image.open(a/('Board_Rail_'+side+'.png')).crop(rb).tobytes(),('port',cn,side)
(o/'Diagnostics/NativeValidation.json').write_text(json.dumps({'assets':records,'material_colors_total':len(colors),'exact_corner_ports':8,'repeat_endpoints_match':True},indent=2))
if (o/'Previews/Unity-After-720x704.png').exists():
 before=Image.open(o/'Previews/Unity-Before-720x704.png').convert('RGBA');after=Image.open(o/'Previews/Unity-After-720x704.png').convert('RGBA')
 double=Image.open(o/'Previews/Unity-After-1440x1408.png').convert('RGBA');empty=Image.open(o/'Previews/Unity-Empty-720x704.png').convert('RGBA');no=Image.open(o/'Previews/Unity-NoFrame-720x704.png').convert('RGBA')
 old=Image.open(p/'ArtSource/ForestHybrid/Stage05/Previews/Unity-Full-720x1360.png').convert('RGBA')
 box=(104,96,616,608);board=after.crop(box)
 assert before.crop(box).tobytes()==board.tobytes()==no.crop(box).tobytes()==old.crop((104,528,616,1040)).tobytes(),'Existing board differs/covered'
 assert after.resize(double.size,Image.Resampling.NEAREST).tobytes()==double.tobytes(),'Not exact 2x'
 uses=json.loads((o/'Diagnostics/Unity-720x704.json').read_text())['images'];expected=Image.new('RGBA',(512,512));emptyExpected=expected.copy()
 for u in uses:
  if not(u['name'].startswith('Cell ') or u['name'].startswith('Gem ')):continue
  im=Image.open(p/u['path']).convert('RGBA');assert im.size==(64,64);assert u['scale']==1
  r=u['rect'];x=round(r['x'])-104;y=704-round(r['y'])-round(r['height'])-96
  if u['name'].startswith('Cell '):emptyExpected.alpha_composite(im,(x,y))
 # Source ordering from FindObjects is not layer order. Compose cells first, then gems.
 expected=emptyExpected.copy()
 for u in uses:
  if u['name'].startswith('Gem '):
   r=u['rect'];expected.alpha_composite(Image.open(p/u['path']).convert('RGBA'),(round(r['x'])-104,704-round(r['y'])-round(r['height'])-96))
 assert expected.tobytes()==board.tobytes(),'Independent gem/cell composite mismatch';assert emptyExpected.tobytes()==empty.crop(box).tobytes()
 for u in uses:
  if 'connected foliage' not in u['name']:continue
  im=Image.open(p/u['path']).convert('RGBA');r=u['rect'];factor=round(u['scale']);left=round(r['x']);top=704-round(r['y'])-round(r['height'])
  for y in range(im.height):
   for x in range(im.width):
    if im.getpixel((x,y))[3]:
     sx=left+x*factor;sy=top+y*factor
     assert not(sx<616 and sx+factor>104 and sy<608 and sy+factor>96),('foliage enters board',u['name'],x,y)
 assert all(c[:3]!=(255,0,255) for c in after.getdata()),'camera gap'
 checks={'dimensions':[720,704],'exact_2x':True,'board_512x512_matches_prior_unity_and_independent_original_png_composite':True,'native_cell_and_gem_size':[64,64],'frame_on_off_board_intrusion_pixels':0,'empty_cells_exact':True,'foliage_alpha_does_not_enter_grid':True,'camera_gap_pixels':0}
 (o/'Diagnostics/PixelValidation.json').write_text(json.dumps(checks,indent=2));print(json.dumps(checks))
if Path(__file__).resolve()!=(o/'Scripts/ValidateAssetsAndCaptures.py').resolve():shutil.copy2(__file__,o/'Scripts/ValidateAssetsAndCaptures.py')
print('PASS: native modules',len(records),'material colors',len(colors),'eight exact frame ports.')
