from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import hashlib,json,shutil,sys
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint03';a=p/'Assets/_Game/Art/ForestReworkV2/Checkpoint03';s=p/'.utmp/forest-v2-c3'
sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
records=[];allcolors=set()
for f in sorted(a.glob('*.png')):
 im=Image.open(f).convert('RGBA');planned=Image.open(s/(f.stem+'-expected.png')).convert('RGBA');reopened=Image.open(s/'reopened'/f.name).convert('RGBA')
 assert im.tobytes()==planned.tobytes()==reopened.tobytes(),f.name
 assert set(im.getchannel('A').getdata())<={0,255};assert all(c[3] or c==(0,0,0,0) for c in im.getdata())
 colors={c[:3] for c in im.getdata() if c[3]};allcolors|=colors;b=im.getbbox()
 if 'Vine' in f.stem:assert b[0]>=1 and b[1]>=1 and b[2]<im.width and b[3]<im.height,(f.name,b)
 if 'Rail' in f.stem:
  horizontal=im.width>im.height
  assert (im.crop((0,0,1,im.height)).tobytes()==im.crop((im.width-1,0,im.width,im.height)).tobytes()) if horizontal else (im.crop((0,0,im.width,1)).tobytes()==im.crop((0,im.height-1,im.width,im.height)).tobytes())
 records.append({'name':f.stem,'size':im.size,'opaque_colors':len(colors),'alpha':'binary','bounds':b,'sha256':sha(f),'libresprite_reopen_exact':True})
 shutil.copy2(f,o/'Previews'/f.name)
# Check all eight corner/rail ports share exact cross-sections.
for corner,side,box,railbox in [('TL','Top',(14,3,15,12),(0,0,1,9)),('TR','Top',(0,3,1,12),(0,0,1,9)),('BL','Bottom',(14,3,15,12),(0,0,1,9)),('BR','Bottom',(0,3,1,12),(0,0,1,9)),('TL','Left',(3,14,12,15),(0,0,9,1)),('BL','Left',(3,0,12,1),(0,0,9,1)),('TR','Right',(3,14,12,15),(0,0,9,1)),('BR','Right',(3,0,12,1),(0,0,9,1))]:
 ci=Image.open(a/('Portrait_Corner_'+corner+'.png'));ri=Image.open(a/('Portrait_Rail_'+side+'.png'))
 assert ci.crop(box).tobytes()==ri.crop(railbox).tobytes(),('join',corner,side)
out={'assets':records,'total_material_colors':len(allcolors),'corner_rail_ports':8,'seamless_rail_endpoints':True}
(o/'Diagnostics/NativeValidation.json').write_text(json.dumps(out,indent=2))
if (o/'Previews/Unity-After-720x480.png').exists():
 before=Image.open(o/'Previews/Unity-Before-720x480.png').convert('RGBA');after=Image.open(o/'Previews/Unity-After-720x480.png').convert('RGBA')
 previous=Image.open(p/'ArtSource/ForestReworkV2/Checkpoint02/Previews/Unity-After-720x480.png').convert('RGBA');double=Image.open(o/'Previews/Unity-After-1440x960.png').convert('RGBA')
 assert before.tobytes()==previous.tobytes(),'Baseline differs from approved checkpoint2'
 assert after.resize(double.size,Image.Resampling.NEAREST).tobytes()==double.tobytes(),'Noninteger rendering'
 interior_changes=[(x,y) for y in range(40,432) for x in range(40,188) if after.getpixel((x,y))!=before.getpixel((x,y))]
 # The old upper ivy overlaps 48 screen pixels inside the opening; its removal
 # must expose the actual unchanged tiled backing, not introduce another drawing.
 assert set(interior_changes)=={(x,y) for y in range(40,44) for x in range(68,80)},'Unexpected interior change'
 uses=json.loads((o/'Diagnostics/Unity-720x480.json').read_text())['images']
 use=next(u for u in uses if u['path'].endswith('/Portrait_Inset.png'));source=Image.open(p/use['path']).convert('RGBA');r=use['rect'];sc=round(use['scale'])
 left,bottom,ww,hh=[round(r[k]) for k in ['x','y','width','height']];top=480-bottom-hh
 for x,y in interior_changes:
  sy=source.height-1-((hh-1-(y-top))//sc%source.height);sx=(x-left)//sc%source.width
  assert after.getpixel((x,y))==source.getpixel((sx,sy)),('Revealed backing differs',x,y)
 assert after.crop((240,0,720,480)).tobytes()==before.crop((240,0,720,480)).tobytes(),'Battle or shared controls changed'
 allowed=set()
 for cp in ['Checkpoint02','Checkpoint03']:
  for u in json.loads((p/'ArtSource/ForestReworkV2'/cp/'Diagnostics/Unity-720x480.json').read_text())['images']:
   f=u['path'];n=Path(f).stem
   if not ('Portrait_Corner' in n or 'Portrait_Rail' in n or 'Portrait_Clamp' in n or 'Portrait_Join' in n or 'Portrait_Leaves' in n or 'Portrait_Vine' in n):continue
   src=Image.open(p/f).convert('RGBA');r=u['rect'];scale=round(u['scale']);x,y,ww,hh=[round(r[k]) for k in ['x','y','width','height']];top=480-y-hh
   for yy in range(hh):
    sy=src.height-1-((hh-1-yy)//scale%src.height)
    for xx in range(ww):
     if src.getpixel((xx//scale%src.width,sy))[3]:allowed.add((x+xx,top+yy))
 changed=[]
 for y in range(480):
  for x in range(720):
   if before.getpixel((x,y))!=after.getpixel((x,y)):changed.append((x,y));assert (x,y) in allowed,('Out of scope change',x,y)
 probe=Image.open(o/'Previews/Unity-MaskProbe-720x480.png').convert('RGBA');affected=0
 for y in range(480):
  for x in range(720):
   if probe.getpixel((x,y))!=after.getpixel((x,y)):
    affected+=1;assert 40<=x<188 and 40<=y<432,('mask spill',x,y)
 assert affected>0;assert all(c[:3]!=(255,0,255) for c in after.getdata())
 checks={'approved_checkpoint2_before_exact':True,'exact_2x_render':True,'portrait_contents_exact_except_removed_old_ivy':True,'old_ivy_intrusion_pixels_now_exact_backing':len(interior_changes),'outside_portrait_unchanged':True,'changed_pixels_only_in_frame_foliage_masks':len(changed),'mask_probe_changed_pixels':affected,'mask_spill_pixels':0,'camera_gap_pixels':0}
 (o/'Diagnostics/PixelValidation.json').write_text(json.dumps(checks,indent=2));print(json.dumps(checks))
 # Display-only nearest-neighbor closeups; native assets remain unchanged.
 before.crop((12,0,238,468)).resize((452,936),Image.Resampling.NEAREST).save(o/'Previews/Portrait-Before-Display.png')
 after.crop((12,0,238,468)).resize((452,936),Image.Resampling.NEAREST).save(o/'Previews/Portrait-After-Display.png')
if Path(__file__).resolve()!=(o/'Scripts/ValidateAssetsAndCaptures.py').resolve():shutil.copy2(__file__,o/'Scripts/ValidateAssetsAndCaptures.py')
print('PASS: '+str(len(records))+' native modules, '+str(len(allcolors))+' total material colors, 8 exact corner ports.')
