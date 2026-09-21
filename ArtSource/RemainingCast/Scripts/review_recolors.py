"""Read-only side-by-side inspection and color/alpha measurements."""
from pathlib import Path
import json,sys,re
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase,color_counts
spec=(ROOT/'Scripts/cast_spec.js').read_text()
pairs=re.findall(r"src:'([^']+)',name:'([^']+)'",spec)
report={}
for page in range((len(pairs)+3)//4):
 board=Image.new('RGB',(1100,650),'#191624');d=ImageDraw.Draw(board)
 for k,(src,name) in enumerate(pairs[page*4:page*4+4]):
  old,_=read_ase(ROOT/'Originals'/f'{src}.aseprite')
  new,_=read_ase(ROOT/'Recolored'/f'{name}.aseprite')
  a,b=old[0],new[0]
  assert a.size==b.size and a.getchannel('A').tobytes()==b.getchannel('A').tobytes(),name
  report[name]={'colors':dict(color_counts(new).most_common()),'mask_unchanged':True,'canvas':list(b.size),'bounds':b.getbbox()}
  x=(k%2)*550+8;y=(k//2)*325+35
  d.text((x,y-23),f'{name}  ORIGINAL / RECOLORED',fill='#FDF5E5')
  for q,im in enumerate((a,b)):
   p=im.resize((256,256),Image.Resampling.NEAREST);board.paste(p,(x+q*270,y),p)
  d.text((x,y+266),f'{len(color_counts(old))} -> {len(color_counts(new))} colors; exact alpha mask',fill='#B7A393')
 board.save(ROOT/'Review'/f'Recolors_{page+1}.png')
(ROOT/'Review/RecolorChecks.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS: 17 recolors preserve exact original alpha, dimensions and placement.')
for n,v in report.items():print(n,len(v['colors']))
