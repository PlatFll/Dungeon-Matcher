"""Read-only audit and nearest-neighbor review of the user-selected design."""
from PIL import Image,ImageDraw
from collections import Counter
from pathlib import Path
import json
import sys
root=Path(__file__).resolve().parent
name=sys.argv[1] if len(sys.argv)>1 else 'Gideon_Approved_02'
im=Image.open(root/('Candidates/'+name+'.png')).convert('RGBA')
pixels=im.load(); todo={(x,y) for y in range(64) for x in range(64) if pixels[x,y][3]}
components=[]
while todo:
    q=[todo.pop()]; component=[]
    while q:
        x,y=q.pop(); component.append((x,y))
        for v in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)]:
            if v in todo:todo.remove(v);q.append(v)
    components.append(component)
print('Connected components:',[len(c) for c in components])
print('Small components:',[c for c in components if len(c)<20])
review=Image.new('RGB',(768,576),'#24212d');d=ImageDraw.Draw(review)
for i,bg in enumerate(['#161420','#ddd8ce']):
    tile=Image.new('RGBA',(64,64),bg);tile.alpha_composite(im)
    review.paste(tile.resize((384,384),Image.Resampling.NEAREST),(i*384,24))
    review.paste(tile,(i*384+24,440))
    d.text((i*384+24,518),'64 x 64 native / 6x nearest-neighbor',fill='white')
review.save(root/('Candidates/'+name+'_detail.png'))
hexes='0A0D11 755335 B18A4B DAC080 25212D 443949 685565 54283D 894254 24425C 397BAB A9E0FF B7A393 FDF5E5 5B3627 8E7868'.split()
lookup={tuple(bytes.fromhex(h)):chr(65+i) for i,h in enumerate(hexes)}
print('    '+''.join(str(x//10) if x%10==0 else ' ' for x in range(64)))
for y in range(27,39):
    print(f'{y:02}  '+''.join(lookup[pixels[x,y][:3]] if pixels[x,y][3] else '.' for x in range(64)))
