"""Read-only sprite inspection and lossless upload packing; never paints sprite pixels."""
from pathlib import Path
from PIL import Image, ImageDraw
import json, hashlib
root=Path(__file__).resolve().parents[1]
base=root/'ArtSource/FrostwakePlayables'
out=base/'eye-revision';out.mkdir(exist_ok=True)
files=[base/'locked/Minotaur.png']
for clip in ['Idle','Jump','Hit','Death']:
    files+=sorted((base/f'motion/Minotaur_{clip}/palette-locked').glob('*.png'))
sheet=Image.new('RGBA',(96*6,96*6))
review=Image.new('RGB',(96*3*6,112*3*6),'#303238');draw=ImageDraw.Draw(review)
items=[]
for i,path in enumerate(files):
    im=Image.open(path).convert('RGBA');x=(i%6)*96;y=(i//6)*96
    sheet.paste(im,(x,y))
    shown=im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST)
    review.paste(shown,((i%6)*288,(i//6)*336),shown)
    draw.text(((i%6)*288,(i//6)*336+290),f'{i}: {path.parent.parent.name}/{path.name}',fill='white')
    gold={(px,py) for py in range(im.height) for px in range(im.width) if im.getpixel((px,py))==(245,200,65,255)}
    groups=[]
    while gold:
        todo=[gold.pop()];cluster=[]
        while todo:
            px,py=todo.pop();cluster.append([px,py])
            for p in [(px-1,py),(px+1,py),(px,py-1),(px,py+1)]:
                if p in gold:gold.remove(p);todo.append(p)
        groups.append(cluster)
    items.append(dict(index=i,path=str(path.relative_to(root)),sha256=hashlib.sha256(path.read_bytes()).hexdigest(),size=im.size,offset=[x,y],gold=groups))
sheet.save(out/'source-sheet.png');review.save(out/'source-review.png')
(out/'source-manifest.json').write_text(json.dumps(items,indent=2))
print(f'Packed {len(files)} immutable source cels. No pixel edits.')
