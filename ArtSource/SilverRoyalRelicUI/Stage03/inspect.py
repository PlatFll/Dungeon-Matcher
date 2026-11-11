from pathlib import Path
from PIL import Image
from collections import Counter
root=Path(__file__).resolve().parent
for n in ('Ability','Supply','Energy'):
 im=Image.open(root/'Raw'/f'{n}.png').convert('RGBA')
 print(n,im.size,im.getbbox(),Counter(im.getchannel('A').get_flattened_data()),Counter(c for c in im.get_flattened_data() if c[3]))
 im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST).save(root/'Preview'/f'Raw-{n}-4x.png')
 # Enumerate connected opaque components and row spans.
 pixels={(x,y) for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))[3]};parts=[]
 while pixels:
  seed=pixels.pop();todo=[seed];part=[seed]
  while todo:
   x,y=todo.pop()
   for p in ((x+1,y),(x-1,y),(x,y+1),(x,y-1)):
    if p in pixels: pixels.remove(p);todo.append(p);part.append(p)
  parts.append((len(part),[min(x for x,y in part),min(y for x,y in part),max(x for x,y in part)+1,max(y for x,y in part)+1]))
 print('COMPONENTS', sorted(parts,reverse=True)[:20])
