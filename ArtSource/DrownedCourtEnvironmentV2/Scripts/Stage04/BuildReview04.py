from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib
F=Path('C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';E=R/'Exports/Stage04';P=R/'Preview/Stage04'
assets=[]
for p in sorted(E.glob('*.png')):
 im=Image.open(p).convert('RGBA');assets.append(dict(name=p.stem,width=im.width,height=im.height,colors=len(set(c[:3] for c in im.get_flattened_data() if c[3])),alpha=sorted(set(im.getchannel('A').get_flattened_data())),bbox=im.getbbox(),sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
assert len(assets)==16
decor=[dict(name='Barnacles',x=-1,y=0),dict(name='Barnacles',x=87,y=187),dict(name='AlgaeLeft',x=-4,y=45),dict(name='AlgaeRight',x=89,y=110),dict(name='CoralSprig',x=-3,y=172)]
m=dict(stage='04',status='CANDIDATE_PENDING_USER_REVIEW',ppu=64,assets=assets,portrait_reference_rect=[21,825,195,406],native_layout_size=[97.5,203],unit=2,corner=15,rail_thickness=7,connector_pitch=45,interior_inset_reference=20,decorations=decor,notes='Same outer PlayerSection and actor/UI positions. Existing ResponsiveModularFrameFitter rounds the native decorative shell width to 98, as its shared implementation specifies; no artwork is stretched. Recess overfills an inset mask that overlaps beneath opaque rails.')
(R/'Native/Stage04/Manifest.json').write_text(json.dumps(m,indent=2)+'\n')
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12)
sheet=Image.new('RGBA',(480,410),(9,20,29,255));d=ImageDraw.Draw(sheet)
for i,a in enumerate(assets):
 x=12+(i%4)*120;y=10+(i//4)*100;d.text((x,y),a['name'],font=font,fill='white');sheet.alpha_composite(Image.open(E/(a['name']+'.png')).convert('RGBA'),(x,y+20))
sheet.save(P/'Native-Contact.png');sheet.resize((960,820),Image.Resampling.NEAREST).save(P/'Native-Contact-2x.png')
def tile(dst,name,box):
 im=Image.open(E/(name+'.png')).convert('RGBA');x,y,w,h=box
 for yy in range(0,h,im.height):
  for xx in range(0,w,im.width):dst.alpha_composite(im.crop((0,0,min(im.width,w-xx),min(im.height,h-yy))),(x+xx,y+yy))
canvas=Image.new('RGBA',(112,215));frame=Image.new('RGBA',(98,203));tile(frame,'RecessA',(10,10,78,183))
for name,box in [('RailTop',(7,4,84,7)),('RailBottom',(7,192,84,7)),('RailLeft',(4,7,7,189)),('RailRight',(87,7,7,189))]:tile(frame,name,box)
for name,x,y in [('CornerTL',0,0),('CornerTR',83,0),('CornerBL',0,188),('CornerBR',83,188)]:frame.alpha_composite(Image.open(E/(name+'.png')).convert('RGBA'),(x,y))
clamp=Image.open(E/'Clamp.png').convert('RGBA')
for x,y in [(48,7),(48,195),(7,54),(90,54),(7,101),(90,101),(7,148),(90,148)]:frame.alpha_composite(clamp,(x-4,y-4))
canvas.alpha_composite(frame,(7,6))
for p in decor:canvas.alpha_composite(Image.open(E/(p['name']+'.png')).convert('RGBA'),(p['x']+7,p['y']+6))
canvas.save(P/'Portrait-Native.png');canvas.resize((448,860),Image.Resampling.NEAREST).save(P/'Portrait-4x.png')
repeat=Image.new('RGBA',(256,128))
for y in range(2):
 for x in range(4):repeat.alpha_composite(Image.open(E/('Recess'+('A' if (x+y)%2==0 else 'B')+'.png')).convert('RGBA'),(x*64,y*64))
repeat.save(P/'Recess-Joins-Native.png');repeat.resize((768,384),Image.Resampling.NEAREST).save(P/'Recess-Joins-3x.png')
raw=Image.new('RGBA',(288,170),(9,20,29,255));d=ImageDraw.Draw(raw)
for i,(r,n) in enumerate([('CornerMotif','CornerMotif'),('RecessSeed','RecessA')]):
 d.text((8,4+i*84),r+' raw / clean',font=font,fill='white');raw.alpha_composite(Image.open(R/f'RawPixelLab/Stage04/{r}.png').convert('RGBA'),(8,20+i*84));raw.alpha_composite(Image.open(E/(n+'.png')).convert('RGBA'),(100,20+i*84))
raw.save(P/'Source-Comparison-Native.png');raw.resize((864,510),Image.Resampling.NEAREST).save(P/'Source-Comparison-3x.png')
print('16 native assets measured; clean modular assembly and source review sheets created')
