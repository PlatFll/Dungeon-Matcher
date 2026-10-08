"""Inspect final native files and create a nearest-neighbor review sheet. No asset edits."""
from pathlib import Path
import hashlib, json
from PIL import Image, ImageDraw

root=Path(__file__).parent
files=sorted((root/'Selected').glob('*.png'))
rows=[]
sheet=Image.new('RGB',(690,((len(files)+4)//5)*146+120),(49,48,53))
draw=ImageDraw.Draw(sheet)
for n,p in enumerate(files):
    im=Image.open(p).convert('RGBA'); pixels=list(im.getdata())
    alpha=sorted({v[3] for v in pixels}); colors=len({v[:3] for v in pixels if v[3]})
    assert im.size==(24,24),p
    assert alpha==[0,255],(p,alpha)
    assert colors<=12,(p,colors)
    rows.append(dict(id=p.stem,width=24,height=24,alpha=alpha,opaque_palette=colors,
                     bounds=im.getbbox(),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),native_scale=1))
    x=(n%5)*138; y=(n//5)*146
    draw.text((x+4,y+4),p.stem,fill=(253,245,229))
    sheet.paste(im,(x+7,y+25),im)
    large=im.resize((96,96),Image.Resampling.NEAREST)
    sheet.paste(large,(x+37,y+27),large)
    draw.text((x+6,y+128),f'1x / 4x   {colors} colors',fill=(193,187,177))
y=((len(files)+4)//5)*146
draw.text((5,y),'Approved native material references (1x):',fill=(253,245,229))
for i,(name,path) in enumerate([('Rattlebones','Rattlebones.png'),('Farmer','Farmer.png'),('Bardley palette','Bardley_Palette_Only.png')]):
    im=Image.open(Path('ArtSource/DrownedCourt/Phase02/References')/path).convert('RGBA')
    # Reference exports contain horizontal 64px frames. Show the first frame
    # at its actual native scale, without spilling into neighboring labels.
    if im.width>64: im=im.crop((0,0,64,im.height))
    sheet.paste(im,(i*225+8,y+24),im);draw.text((i*225+76,y+40),name,fill=(253,245,229))
sheet.save(root/'NativeReview.png')
(root/'FinalChecks.json').write_text(json.dumps(rows,indent=2)+'\n')
print(f'{len(rows)} icons: 24x24, binary alpha, <=12 opaque colors. NativeReview.png written.')
