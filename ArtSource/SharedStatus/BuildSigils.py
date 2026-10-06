"""Native slot identities: direct pixels, two colors, no generation."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
import re,uuid
repo=Path(__file__).resolve().parents[2];out=repo/'Assets/_Game/Resources/UI/CasterSigils';out.mkdir(exist_ok=True)
template=(repo/'Assets/_Game/Resources/UI/PlayerStatuses/Fear.png.meta').read_text()
for name in ['Triangle','Square','Ring','TargetOutline']:
    size=64 if name=='TargetOutline' else 12
    mask=Image.new('L',(size,size));d=ImageDraw.Draw(mask)
    if name=='Triangle':d.line([(5,2),(9,9),(2,9),(5,2)],fill=255,width=2)
    elif name=='Square':d.rectangle((2,2,9,9),outline=255,width=2)
    elif name=='Ring':d.ellipse((2,2,9,9),outline=255,width=2)
    else:
        for x,y,sx,sy in [(5,5,1,1),(58,5,-1,1),(5,58,1,-1),(58,58,-1,-1)]:
            d.line([(x+sx*7,y),(x,y),(x,y+sy*7)],fill=255,width=1)
    image=Image.new('RGBA',(size,size));image.paste((10,13,17,255),mask=mask.filter(ImageFilter.MaxFilter(3)));image.paste((235,237,231,255),mask=mask)
    p=out/(name+'.png');image.save(p)
    meta=Path(str(p)+'.meta')
    if not meta.exists():meta.write_text(re.sub(r'guid: [0-9a-f]+','guid: '+uuid.uuid4().hex,template,count=1))
folder=Path(str(out)+'.meta')
if not folder.exists():folder.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n')
proof=Image.new('RGB',(48,16),(84,84,84))
for i,name in enumerate(['Triangle','Square','Ring']):
    im=Image.open(out/(name+'.png'));proof.paste(im,(i*16+2,2),im)
proof.save(Path(__file__).parent/'NativeSigils.png')
print('3 native 12x12 slot sigils and neutral 64x64 corner outline; binary alpha, two colors')
