from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import json
root=Path(__file__).resolve().parent
font=ImageFont.truetype(str(root.parents[2]/'Assets/_Game/Fonts/Thaleah/ThaleahFat.ttf'),20)
files=sorted((root/'Raw').glob('*.png'))
sheet=Image.new('RGB',(1600,1120),(105,105,105));d=ImageDraw.Draw(sheet)
checks=[]
for i,p in enumerate(files):
    im=Image.open(p).convert('RGBA');x=(i%5)*320;y=(i//5)*560
    scale=3 if im.width==96 else 4
    view=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST)
    sheet.paste(view,(x+8,y+20),view)
    d.text((x+8,y+320),p.stem.replace('_',' '),font=font,fill='white')
    d.text((x+8,y+345),f'{im.width} x {im.height}, view {scale}x',font=font,fill='white')
    sheet.paste(im,(x+8,y+390),im)
    checks.append(dict(id=p.stem,size=im.size,bbox=im.getbbox(),colors=len(set(q[:3] for q in im.getdata() if q[3])),alpha=sorted(set(im.getchannel('A').getdata()))))
sheet.save(root/'StillInspection.png')
(root/'StillChecks.json').write_text(json.dumps(checks,indent=2))
print(json.dumps(checks))
