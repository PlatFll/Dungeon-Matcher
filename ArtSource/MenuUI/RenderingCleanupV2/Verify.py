"""Independent native pixel, protected geometry, alpha and palette checks."""
from pathlib import Path
from collections import Counter
from PIL import Image,ImageDraw,ImageFont
import hashlib,json,re,shutil,struct,zlib,sys
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
stage=sys.argv[1] if len(sys.argv)>1 else 'native'
load=lambda p:Image.open(p).convert('RGBA')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=load(HERE/'before/HallTall.png');b=load(HERE/stage/'HallTall.png')
assert a.size==b.size==(360,800)
changed={(x,y) for y in range(800) for x in range(360) if a.getpixel((x,y))!=b.getpixel((x,y))}
assert a.getchannel('A').tobytes()==b.getchannel('A').tobytes()
assert set(a.get_flattened_data())==set(b.get_flattened_data())
assert b.crop((0,0,360,640)).tobytes()==load(HERE/stage/'Hall.png').tobytes()
# Geometry locks come from the original visible joints and focal object bounds,
# not a mask inferred from the post-edit differences. Opaque alpha alone would
# not prove the preservation of internal silhouettes.
rectangles={
 'portal and skull':(128,132,234,294),'left statue':(0,140,43,294),'right statue':(316,140,360,294),
 'left banner':(43,104,99,214),'right banner':(260,104,316,211),
 'treasure and left props':(0,253,88,330),'right props':(265,250,360,330),
 'pedestal jewel':(105,306,126,322),'pedestal carpet':(145,306,216,340),
 'portal lip':(89,294,265,305),'upper left torch':(25,40,62,110),'upper right torch':(295,40,332,110),
 'left mid torch':(84,211,110,274),'right mid torch':(251,211,279,274),
 'left foreground':(0,545,89,800),'right foreground':(271,545,360,800),
 'tall extension':(0,640,360,800),
 # Primary rim/joint geometry is locked; secondary reflected bands farther
 # inside the bevel are the explicit shading targets, not structural edges.
 'riser 1':(22,343,334,345),'riser 2':(25,358,333,360),
 'riser 3':(25,374,324,376),'riser 4':(27,389,331,392),
 'riser 5':(10,409,348,411),'riser 6':(0,425,360,431),
 'floor joint 1':(0,452,360,456),'floor joint 2':(0,472,360,481),
 'floor joint 3':(0,499,360,502),'floor joint 4':(0,520,360,526)}
locked=Image.new('L',a.size);draw=ImageDraw.Draw(locked)
for name,box in rectangles.items():
 assert a.crop(box).tobytes()==b.crop(box).tobytes(),name
 draw.rectangle((box[0],box[1],box[2]-1,box[3]-1),fill=255)
for y in range(800):
 for x in range(360):
  if a.getpixel((x,y))[:3]==(17,6,36):
   for yy in range(max(0,y-1),min(800,y+2)):
    for xx in range(max(0,x-1),min(360,x+2)):
     locked.putpixel((xx,yy),255);assert a.getpixel((xx,yy))==b.getpixel((xx,yy)),(xx,yy)
locked.save(HERE/'evidence'/(stage+'-protected-geometry.png'))
def ase_pixels(path):
 data=path.read_bytes();magic,frames,w,h,depth=struct.unpack_from('<5H',data,4)
 assert (magic,frames,depth)==(0xa5e0,1,32)
 im=Image.new('RGBA',(w,h));offset=144
 while offset<len(data):
  length,kind=struct.unpack_from('<IH',data,offset)
  if kind==0x2005:
   n=offset+6;layer,x,y,opacity,typ=struct.unpack_from('<HhhBH',data,n);cw,ch=struct.unpack_from('<HH',data,n+16)
   assert (layer,opacity,typ)==(0,255,2)
   im.paste(Image.frombytes('RGBA',(cw,ch),zlib.decompress(data[n+20:offset+length])),(x,y))
  offset+=length
 return im
assets=[]
for name in ['Hall','HallTall']:
 src=HERE/stage/(name+'.png');im=load(src);original=load(HERE/'before'/(name+'.png'))
 assert im.tobytes()==ase_pixels(src.with_suffix('.aseprite')).tobytes()
 entries=[[x,y,*original.getpixel((x,y))[:3],*im.getpixel((x,y))[:3]] for x,y in sorted(changed,key=lambda xy:(xy[1],xy[0])) if y<im.height]
 overlay=original.copy();mask=Image.new('RGBA',im.size,(0,0,0,0))
 for e in entries:overlay.putpixel(tuple(e[:2]),(0,255,176,255));mask.putpixel(tuple(e[:2]),(0,255,176,255))
 overlay.save(HERE/'review'/(stage+'-'+name+'-overlay.png'));mask.save(HERE/'evidence'/(stage+'-'+name+'-difference.png'))
 palette=lambda image: {''.join(f'{v:02x}' for v in c[:3]):n for c,n in sorted(Counter(image.get_flattened_data()).items())}
 assets.append({'asset':name,'dimensions':im.size,'before_sha256':sha(HERE/'before'/(name+'.png')),'after_sha256':sha(src),
  'changed_opaque_pixels':len(entries),'alpha_changes':0,'protected_geometry_changes':0,
  'palette_before':palette(original),'palette_after':palette(im),'source_ase_matches_png':True})
 (HERE/'evidence'/(stage+'-'+name+'-edits.json')).write_text('[\n'+',\n'.join(json.dumps(e) for e in entries)+'\n]\n')
baseline=json.loads((HERE/'baseline.json').read_text())
editable={'Assets/_Game/Resources/UI/GlobalMenu/Hall.png','Assets/_Game/Resources/UI/GlobalMenu/HallTall.png'}
for p,h in baseline.items():
 if p not in editable:assert sha(ROOT/p)==h,p
report={'stage':stage,'result':'passed','assets':assets,'locked_regions':rectangles,'protected_files_unchanged':len(baseline)-2,
 'note':'Alpha equality is verified separately from protected opaque geometry. Visual quality is an art judgment, not a palette-count score.'}
(HERE/'evidence'/(stage+'-pixel-checks.json')).write_text(json.dumps(report,indent=2)+'\n')
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
sheet=Image.new('RGB',(752,686),'#25232a');d=ImageDraw.Draw(sheet)
d.text((10,8),'CURRENT V1',font=font,fill='white');d.text((382,8),'V2 INTERIOR SHADING',font=font,fill='white')
sheet.paste(a.crop((0,0,360,640)),(10,36));sheet.paste(b.crop((0,0,360,640)),(382,36))
sheet.save(HERE/'review'/(stage+'-environment-native.png'))
print(stage,len(changed),'changed pixels; exact palette retained; zero alpha/protected-geometry changes; ASE/PNG agree.')
if '--install' in sys.argv:
 assert stage=='native'
 for name in ['Hall','HallTall']:
  for ext in ['.png','.aseprite']:shutil.copyfile(HERE/stage/(name+ext),ROOT/'ArtSource/MenuUI/native'/(name+ext))
  shutil.copyfile(HERE/stage/(name+'.png'),ROOT/'Assets/_Game/Resources/UI/GlobalMenu'/(name+'.png'))
 print('Installed the verified existing background sources/exports; metadata retained.')
