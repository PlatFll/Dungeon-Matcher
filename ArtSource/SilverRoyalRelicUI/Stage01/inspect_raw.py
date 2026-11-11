from pathlib import Path
from PIL import Image
from collections import Counter
import json
HERE=Path(__file__).resolve().parent
report={}
for p in sorted((HERE/'Raw').glob('*.png')):
    im=Image.open(p).convert('RGBA');colors=Counter(im.get_flattened_data());alpha=im.getchannel('A')
    bands=[];start=None
    for y in range(im.height+1):
        visible=y<im.height and alpha.crop((0,y,im.width,y+1)).getbbox() is not None
        if visible and start is None:start=y
        if not visible and start is not None:bands.append([start,y]);start=None
    report[p.stem]={'size':list(im.size),'bounds':im.getbbox(),'alpha':sorted(set(alpha.get_flattened_data())),
        'colors':{'#%02X%02X%02X'%c[:3]:n for c,n in colors.items() if c[3]},'bands':bands}
    im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST).save(HERE/'Preview'/f'Raw-{p.stem}-4x.png')
(HERE/'QA'/'RawPixels.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
