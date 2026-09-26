"""Read-only native source inspection and nearest-neighbor contact sheets."""
from pathlib import Path
import sys,json
from PIL import Image,ImageDraw
root=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(root.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase,color_counts
for p in (root/'Review').glob('*.aseprite'):
    frames,ds=read_ase(p);w,h=frames[0].size
    cols=min(4,len(frames));cw=w*3+16;ch=h*3+28
    board=Image.new('RGB',(cols*cw,((len(frames)+cols-1)//cols)*ch+24),'#292632');draw=ImageDraw.Draw(board)
    draw.text((8,6),p.name,fill='white')
    for i,im in enumerate(frames):
        x=i%cols*cw+8;y=i//cols*ch+48
        draw.text((x,y-18),f'{i+1}: {ds[i]}ms',fill='white')
        big=im.resize((w*3,h*3),Image.Resampling.NEAREST);board.paste(big,(x,y),big)
    board.save(root/'Review'/(p.stem+'_contact.png'))
    print(p.stem,len(frames),frames[0].size,'colors',len(color_counts(frames)),'bounds',[im.getbbox() for im in frames])
