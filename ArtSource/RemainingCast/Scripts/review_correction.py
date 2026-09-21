"""Read-only part and correction review. Never writes sprite pixels."""
from pathlib import Path
import sys,json,importlib.util
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase
files=sorted((ROOT/'Review/Correction/RigParts').glob('*.aseprite'))
for page in range((len(files)+3)//4):
 board=Image.new('RGB',(1150,1000),'#CCC6BB');d=ImageDraw.Draw(board)
 for row,path in enumerate(files[page*4:page*4+4]):
  fs,_=read_ase(path);y=row*245+24;d.text((10,y),path.stem,fill='#201825')
  for col,(frame,label) in enumerate([(0,'Ready'),(1,'Connected body'),(2,'Head'),(3,'Whole prop'),(4,'Shield')]):
   im=fs[frame].resize((192,192),Image.Resampling.NEAREST);x=150+col*198
   board.paste(im,(x,y),im);d.text((x,y+196),label,fill='#201825')
 board.save(ROOT/'Review/Correction'/f'Parts_{page+1}.png')

# The user's two demonstrated faults, before and after every compressed pose.
for name in ['King','RoyalArbalist','RoyalLancer']:
 before,_=read_ase(ROOT/'BeforeCorrection/Idles'/f'{name}_Idle.aseprite')
 after,_=read_ase(ROOT/'Idles'/f'{name}_Idle.aseprite')
 board=Image.new('RGB',(1400,660),'#CCC6BB');d=ImageDraw.Draw(board)
 for row,(fs,label) in enumerate([(before,'BEFORE'),(after,'CORRECTED')]):
  for col,f in enumerate([0,1,3,4,5]):
   x=col*278+10;y=row*328+42;im=fs[f].resize((256,256),Image.Resampling.NEAREST);board.paste(im,(x,y),im)
   d.text((x,y-23),f'{name} {label} frame {f+1}',fill='#201825')
 board.save(ROOT/'Review/Correction'/f'{name}_comparison.png')
print('Rendered native parts and before/after correction evidence.')

# All nine exposures, with the same reference above each group.
reference=ROOT/'Originals/RattleBones_FluidIdle.aseprite'
for page in range((len(files)+3)//4):
 board=Image.new('RGB',(1330,860),'#CCC6BB');d=ImageDraw.Draw(board)
 rows=[('Rattlebones reference',reference)]+[(p.stem,ROOT/'Idles'/f'{p.stem}_Idle.aseprite') for p in files[page*4:page*4+4]]
 for row,(name,path) in enumerate(rows):
  fs,_=read_ase(path);y=row*170+20;d.text((8,y+44),name,fill='#201825')
  for f,im in enumerate(fs):
   x=165+f*129;p=im.resize((128,128),Image.Resampling.NEAREST);board.paste(p,(x,y),p)
   d.text((x,y+133),f'{f+1} / 130ms',fill='#201825')
 board.save(ROOT/'Review/Correction'/f'AllFrames_{page+1}.png')
