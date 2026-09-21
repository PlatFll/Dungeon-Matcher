"""Read-only contact sheets for the native refinement outputs."""
from pathlib import Path
import sys
from PIL import Image,ImageDraw
root=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(root.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase
for path in (root/'Review/Refined').glob('*.aseprite'):
    frames,ds=read_ase(path)
    w,h=frames[0].size
    s=4;cols=5;cellw=w*s+12;cellh=h*s+26
    out=Image.new('RGB',(cols*cellw,((len(frames)+cols-1)//cols)*cellh),'#26202f')
    d=ImageDraw.Draw(out)
    for f,im in enumerate(frames):
        x=(f%cols)*cellw;y=(f//cols)*cellh
        d.text((x+4,y+3),f'{f+1}: {ds[f]}ms',fill='white')
        large=im.resize((w*s,h*s),Image.Resampling.NEAREST)
        out.paste(large,(x+4,y+22),large)
    out.save(root/'Review'/f'{path.stem}_refined.png')
    print(path.stem,len(frames),frames[0].size,[im.getbbox() for im in frames])
    if '--face' in sys.argv and path.stem=='Miner_Idle':
        symbols={(10,13,17,255):'#',(230,176,138,255):'.',(185,130,93,255):'+',(124,82,56,255):'s'}
        for n,im in enumerate(frames):
            print('FRAME',n+1,'\n   '+''.join(str(x%10) for x in range(29,61)))
            for y in range(35,55):
                print(str(y).rjust(2),' '+''.join(symbols.get(im.getpixel((x,y)),' ' if im.getpixel((x,y))[3]==0 else 'o') for x in range(29,61)))
