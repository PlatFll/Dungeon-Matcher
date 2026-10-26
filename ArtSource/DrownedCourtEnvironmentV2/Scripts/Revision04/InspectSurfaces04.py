from pathlib import Path
from PIL import Image,ImageDraw
import json
R=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2');E=R/'Exports/Revision04';P=R/'Preview/Revision04'
board=Image.new('RGBA',(640,430),'#101f2a');d=ImageDraw.Draw(board)
for i,n in enumerate(['ColumnLeftA','ColumnRightB']):
 src=Image.open(R/f'Exports/Revision02/{n}.png').convert('RGBA'); root=[78,50][i];
 for j,suffix in enumerate(['A','B','C']):
  im=Image.open(E/f'{n}Sway{suffix}.png').convert('RGBA');assert im.size==(31,96)
  assert list(im.crop((0,root-12,31,96)).getdata())==list(src.crop((0,root-12,31,96)).getdata())
  if suffix=='A':assert list(im.getdata())==list(src.getdata())
  x=12+i*320+j*100;board.alpha_composite(im.resize((93,288),Image.Resampling.NEAREST),(x,25));d.text((x,8),n+suffix,fill='white')
old=Image.new('RGBA',(128,32));new=old.copy()
for x,n in [(0,'A'),(64,'B')]:
 src=Image.open(R/f'Exports/Revision01/S03_FloorTop{n}.png').convert('RGBA');old.paste(src,(x,0));old.paste(src,(x,16));
 new.paste(Image.open(E/f'FloorRear{n}.png'),(x,0));new.paste(Image.open(E/f'FloorFront{n}.png'),(x,16))
board.alpha_composite(old.resize((256,64),Image.Resampling.NEAREST),(10,345));board.alpha_composite(new.resize((256,64),Image.Resampling.NEAREST),(330,345));d.text((10,325),'Original double-course seam',fill='white');d.text((330,325),'Joined surface, front lip preserved',fill='white');board.save(P/'Surfaces-Review.png')
im=Image.open(E/'ClamClosed.png').convert('RGBA');im.resize((384,384),Image.Resampling.NEAREST).save(P/'ClamClosed-6x.png')
print('Surface native checks passed. Clam:',im.size,im.getbbox(),len(set(p for p in im.getdata() if p[3])))
