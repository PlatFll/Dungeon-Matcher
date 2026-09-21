from pathlib import Path
import sys
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase
files=sorted((ROOT/'Recolored').glob('*.aseprite'))
for page in range((len(files)+3)//4):
 board=Image.new('RGB',(1130,1170),'#24212B');d=ImageDraw.Draw(board)
 for k,p in enumerate(files[page*4:page*4+4]):
  im=read_ase(p)[0][0];x=(k%2)*565+30;y=(k//2)*585+40
  big=im.resize((512,512),Image.Resampling.NEAREST);board.paste(big,(x,y),big)
  d.text((x,y-30),p.stem,fill='white')
  for v in range(0,65,8):
   d.line((x+v*8,y,x+v*8,y+512),fill='#453E49');d.text((x+v*8,y-14),str(v),fill='#AAA3AC')
   d.line((x,y+v*8,x+512,y+v*8),fill='#453E49');d.text((x-20,y+v*8),str(v),fill='#AAA3AC')
 board.save(ROOT/'Review'/f'Anatomy_{page+1}.png')
