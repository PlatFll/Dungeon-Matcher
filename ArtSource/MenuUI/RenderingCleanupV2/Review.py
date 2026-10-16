"""Read-only comparisons and pixel measurements of LibreSprite output."""
from pathlib import Path
import json,sys
from PIL import Image,ImageDraw,ImageFont
HERE=Path(__file__).resolve().parent
stage=sys.argv[1] if len(sys.argv)>1 else 'pilot'
a=Image.open(HERE/'before/HallTall.png').convert('RGBA');b=Image.open(HERE/stage/'HallTall.png').convert('RGBA')
assert a.size==b.size==(360,800)
diff=[(x,y) for y in range(800) for x in range(360) if a.getpixel((x,y))!=b.getpixel((x,y))]
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
regions=[('Pier face',(0,339,24,370)),('Stair faces',(21,341,112,360)),('Pedestal stone',(88,300,104,319))]
if stage=='native':regions += [('Left paving',(0,424,62,543)),('Right paving',(300,424,360,543)),('Arch',(103,160,122,201))]
sheet=Image.new('RGB',(800,2100),'#25232a');d=ImageDraw.Draw(sheet);yy=10;measure=[]
for name,box in regions:
 ca=a.crop(box);cb=b.crop(box)
 d.text((12,yy),name+' / before',font=font,fill='white');d.text((410,yy),name+' / after',font=font,fill='white');yy+=26
 for x,im in [(12,ca),(410,cb)]:sheet.paste(im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST),(x,yy))
 local=Image.new('RGB',(800,ca.height*4+40),'#25232a');ld=ImageDraw.Draw(local)
 ld.text((12,6),name+' / before',font=font,fill='white');ld.text((410,6),name+' / after',font=font,fill='white')
 for x,im in [(12,ca),(410,cb)]:local.paste(im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST),(x,32))
 local.save(HERE/'review'/(stage+'-'+name.lower().replace(' ','-')+'-4x.png'))
 yy+=ca.height*4+22
 measure.append({'region':name,'crop':box,'changed':sum(p!=q for p,q in zip(ca.get_flattened_data(),cb.get_flattened_data()))})
sheet.crop((0,0,800,yy)).save(HERE/'review'/(stage+'-samples-4x.png'))
overlay=a.copy();d=ImageDraw.Draw(overlay)
for xy in diff:d.point(xy,fill='#00ffb0')
overlay.save(HERE/'review'/(stage+'-difference.png'))
for name in ['Hall','HallTall']:
 for side,folder in [('before','before'),('after',stage)]:
  image=Image.open(HERE/folder/(name+'.png'))
  image.resize((image.width*4,image.height*4),Image.Resampling.NEAREST).save(HERE/'review'/(stage+'-'+name+'-'+side+'-4x.png'))
(HERE/'evidence'/(stage+'-region-measurements.json')).write_text(json.dumps(measure,indent=2)+'\n')
print(json.dumps({'stage':stage,'changed':len(diff),'samples':measure},indent=2))
