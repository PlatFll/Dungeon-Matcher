"""Inspection layouts from raw native files; layout scaling never changes source assets."""
from pathlib import Path
from PIL import Image,ImageDraw
import json
ROOT=Path(__file__).resolve().parents[1];ART=ROOT/'ArtSource/Forest/RosterProduction'
OUT=ART/'Review';OUT.mkdir(exist_ok=True)
for folder in (ART/'Raw').glob('*/*'):
    files=sorted(folder.glob('*.png'))
    if not files:continue
    images=[Image.open(p).convert('RGBA') for p in files]
    w,h=images[0].size;s=3;cols=min(5,len(images))
    plate=Image.new('RGB',(w*s*cols,((len(images)+cols-1)//cols)*(h*s+20)),'#777777');draw=ImageDraw.Draw(plate)
    for i,im in enumerate(images):
        x=(i%cols)*w*s;y=(i//cols)*(h*s+20)
        draw.text((x+2,y+2),f'{i} / {folder.parent.name} {folder.name}',fill='white')
        zoom=im.resize((w*s,h*s),Image.Resampling.NEAREST);plate.paste(zoom,(x,y+20),zoom)
    plate.save(OUT/f'{folder.parent.name}_{folder.name}.png')
print(json.dumps(dict(plates=len(list(OUT.glob('*.png'))))))
