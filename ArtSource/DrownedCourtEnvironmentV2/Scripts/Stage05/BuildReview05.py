from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import hashlib,json
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';E=R/'Exports/Stage05';P=R/'Preview/Stage05'
assets=[]
for p in sorted(E.glob('*.png')):
 im=Image.open(p).convert('RGBA');assets.append(dict(name=p.stem,width=im.width,height=im.height,colors=len(set(c[:3] for c in im.get_flattened_data() if c[3])),alpha=sorted(set(im.getchannel('A').get_flattened_data())),sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
assert len(assets)==14
data=dict(stage='05',status='CANDIDATE_PENDING_REVIEW',assets=assets,corner=25,rail_thickness=11,connector_pitch=120,source_scale_at_720=2,inner_overlap=2,board_reference_rect=[87.45048522949219,213.45050048828126,545.0989990234375,545.0989990234375],notes='Existing shared fitter follows the unchanged opening. Four native shadow strips overlap below cell edges to seal subpixel gaps; cell images render above frame and shadow. Existing fractional cell mapping is preserved, not certified as fixed.')
(R/'Native/Stage05/Manifest.json').write_text(json.dumps(data,indent=2)+'\n')
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12);sheet=Image.new('RGBA',(560,400),(9,20,29,255));d=ImageDraw.Draw(sheet)
for i,a in enumerate(assets):
 x=12+i%4*140;y=8+i//4*98;d.text((x,y),a['name'],font=font,fill='white');sheet.alpha_composite(Image.open(E/(a['name']+'.png')).convert('RGBA'),(x,y+20))
sheet.save(P/'Native-Contact.png');sheet.resize((1120,800),Image.Resampling.NEAREST).save(P/'Native-Contact-2x.png')
# Pure native join proof: 273x273 opening plus 18px inset each side = 309x309 shell.
frame=Image.new('RGBA',(309,309))
def tile(n,box):
 im=Image.open(E/(n+'.png')).convert('RGBA');x,y,w,h=box
 for yy in range(0,h,im.height):
  for xx in range(0,w,im.width):frame.alpha_composite(im.crop((0,0,min(im.width,w-xx),min(im.height,h-yy))),(x+xx,y+yy))
for n,box in [('RailTop',(12,7,285,11)),('RailBottom',(12,291,285,11)),('RailLeft',(7,12,11,285)),('RailRight',(291,12,11,285))]:tile(n,box)
for n,x,y in [('CornerTL',0,0),('CornerTR',284,0),('CornerBL',0,284),('CornerBR',284,284)]:frame.alpha_composite(Image.open(E/(n+'.png')).convert('RGBA'),(x,y))
j=Image.open(E/'Joint.png').convert('RGBA')
for x,y in [(154,12),(154,296),(12,154),(296,154)]:frame.alpha_composite(j,(x-4,y-4))
frame.save(P/'Frame-Native.png');frame.resize((927,927),Image.Resampling.NEAREST).save(P/'Frame-3x.png')
raw=Image.new('RGBA',(288,95),(9,20,29,255));d=ImageDraw.Draw(raw);d.text((8,4),'Raw PixelLab / LibreSprite seed',font=font,fill='white')
raw.alpha_composite(Image.open(R/'RawPixelLab/Stage05/BoardStoneSeed.png').convert('RGBA'),(8,25));raw.alpha_composite(Image.open(E/'BoardStoneSeed.png').convert('RGBA'),(152,25));raw.save(P/'Source-Comparison.png');raw.resize((864,285),Image.Resampling.NEAREST).save(P/'Source-Comparison-3x.png')
print('Measured 14 native assets; source and exact native frame studies created.')
