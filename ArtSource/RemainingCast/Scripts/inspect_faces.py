from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase
boxes={'CrossbowGuard':(20,25,39,35),'SpearGuard':(24,25,40,35),'TownMarshal':(22,21,41,33),'SiegeSergeant':(24,21,42,30),'KnightCaptain':(22,23,38,33),'RoyalArcanist':(25,17,42,28),'RoyalMage':(24,20,40,32),'King':(23,19,40,27),'Minotaur':(22,20,44,30)}
for name,(x0,y0,x1,y1) in boxes.items():
 im=read_ase(ROOT/'Recolored'/f'{name}.aseprite')[0][0]
 print('\n'+name+' x='+str(x0)+'..'+str(x1));print('   '+''.join(str(x%10)for x in range(x0,x1+1)))
 for y in range(y0,y1+1):
  line=''
  for x in range(x0,x1+1):
   p=im.getpixel((x,y));c='%02X%02X%02X'%p[:3]
   line+= '.' if not p[3] else '#' if c=='0A0D11' else 'S' if c=='E6B08A' else 's' if c in ['7C5238','B9825D'] else 'B' if c in ['223A6B','2E568F','75BCEF','A9E0FF'] else 'w' if c in ['FDF5E5','FBEA90'] else 'h'
  print(f'{y:02} '+line)
