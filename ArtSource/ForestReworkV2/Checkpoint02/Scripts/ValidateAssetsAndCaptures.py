from pathlib import Path
from PIL import Image,ImageChops
import hashlib,json,sys,shutil
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint02';a=p/'Assets/_Game/Art/ForestReworkV2/Checkpoint02';s=p/'.utmp/forest-v2-c2'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
records=[]
for f in sorted(a.glob('*.png')):
 im=Image.open(f).convert('RGBA');ex=Image.open(s/(f.stem+'-expected.png')).convert('RGBA');re=Image.open(s/'reopened'/f.name).convert('RGBA')
 assert im.tobytes()==ex.tobytes()==re.tobytes(),f.name
 alphas=sorted(set(im.getchannel('A').getdata()));assert set(alphas)<={0,255}
 b=im.getchannel('A').getbbox()
 if f.stem=='Distant_Wisp':assert b[0]>=2 and b[1]>=2 and b[2]<=im.width-2 and b[3]<=im.height-2
 rec={'name':f.stem,'dimensions':im.size,'opaque_colors':len({c[:3] for c in im.getdata() if c[3]}),'alpha':alphas,'bounds':b,'sha256':sha(f),'libresprite_export_reopen_exact':True}
 records.append(rec);shutil.copy2(f,o/'Previews'/f.name)
for name,old in [('Depth_SideRepeat','Far_SideExtension'),('Depth_CanopyRepeat','Far_CanopyRepeat'),('Depth_UnderstoryRepeat','Understory_Repeat')]:
 im=Image.open(a/(name+'.png')).convert('RGBA');before=Image.open(p/'Assets/_Game/Art/ForestHybrid/Stage01'/(old+'.png')).convert('RGBA')
 assert im.size==before.size and im.getchannel('A').tobytes()==before.getchannel('A').tobytes()
(o/'Diagnostics/NativeValidation.json').write_text(json.dumps(records,indent=2))
if (o/'Previews/Unity-After-720x480.png').exists():
 after=Image.open(o/'Previews/Unity-After-720x480.png').convert('RGBA')
 before=Image.open(o/'Previews/Unity-Before-720x480.png').convert('RGBA')
 double=Image.open(o/'Previews/Unity-After-1440x960.png').convert('RGBA')
 previous=Image.open(p/'ArtSource/ForestHybrid/Stage05/Previews/Unity-Full-720x1360.png').convert('RGBA').crop((0,0,720,480))
 # The former board's caps and ivy protruded into rows 458..479. They are not
 # part of this top-only checkpoint. All pixels above that overlap stay exact.
 assert before.crop((0,0,720,458)).tobytes()==previous.crop((0,0,720,458)).tobytes(),'Baseline top differs'
 baseline_differences=[(x,y) for y in range(480) for x in range(720) if before.getpixel((x,y))!=previous.getpixel((x,y))]
 assert len(baseline_differences)==872 and all(y>=458 for x,y in baseline_differences),'Unexpected excluded-board footprint'
 assert after.resize(double.size,Image.Resampling.NEAREST).tobytes()==double.tobytes(),'Not exact 2x rendering'
 for name,box in [('portrait_interior',(40,40,188,432)),('ground',(0,400,720,480))]:
  # The unchanged ground area includes actor feet and HP; new far art ends above it.
  assert after.crop(box).tobytes()==before.crop(box).tobytes(),name
 # Frame/control rectangles have transparent holes through which the new forest
 # should show. Protect their actual opaque source pixels, not those holes.
 ui_pixels=set()
 uses=json.loads((o/'Diagnostics/Unity-720x480.json').read_text())['images']
 for use in uses:
  path=use['path']
  if not('/ForestHybrid/Stage02/' in path or '/DungeonHybrid/Stage03/' in path) or use['name']=='OverfilledBacking':continue
  source=Image.open(p/path).convert('RGBA');scale=round(use['scale']);r=use['rect']
  left,bottom,width,height=[round(r[k]) for k in ['x','y','width','height']];top=480-bottom-height
  for yy in range(height):
   sy=source.height-1-((height-1-yy)//scale%source.height)
   for xx in range(width):
    if source.getpixel(((xx//scale)%source.width,sy))[3]==255:
     x,y=left+xx,top+yy
     if 0<=x<720 and 0<=y<480:ui_pixels.add((x,y))
 for xy in ui_pixels:assert after.getpixel(xy)==before.getpixel(xy),('opaque frame/control pixel',xy)
 oldstructure=Image.open(p/'ArtSource/ForestHybrid/Stage05/Previews/Unity-Structure-720x1360.png').convert('RGBA')
 protected_pixels=0
 for y in range(480):
  for x in range(720):
   if previous.getpixel((x,y))!=oldstructure.getpixel((x,y)):
    assert after.getpixel((x,y))==previous.getpixel((x,y)),('actor/stat',x,y)
    protected_pixels+=1
 assert all(c[:3]!=(255,0,255) for c in after.getdata()),'Camera visible'
 probe=Image.open(o/'Previews/Unity-MaskProbe-720x480.png').convert('RGBA');spill=0;affected=0
 for y in range(480):
  for x in range(720):
   if probe.getpixel((x,y))!=after.getpixel((x,y)):
    affected+=1
    if not(40<=x<188 and 40<=y<432):spill+=1
 assert affected>0 and spill==0,('mask spill',spill)
 checks={'baseline_top_458_rows_match_previous_unity':True,'excluded_old_board_cap_ivy_overlap_pixels':len(baseline_differences),'exact_2x_rendering':True,'portrait_shared_controls_and_ground_exact':True,'retained_opaque_frame_control_pixels':len(ui_pixels),'retained_actor_hp_countdown_pixels':protected_pixels,'portrait_mask_spill_pixels':spill,'no_camera_gaps':True,'dimensions':after.size,'scope':'Actual Unity art-only top blockout, static readability samples; not production gameplay or device testing'}
 (o/'Diagnostics/PixelValidation.json').write_text(json.dumps(checks,indent=2));print(json.dumps(checks))
shutil.copy2(__file__,o/'Scripts/ValidateAssetsAndCaptures.py')
print(json.dumps(records))
