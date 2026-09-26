"""Read-only inspection: exact source pixels, palettes and masks. No art edits."""
from pathlib import Path
import json, sys, hashlib
from collections import Counter
from PIL import Image, ImageDraw
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase
files=sorted((ROOT/'Originals').glob('*.aseprite'))
report={}
for page in range((len(files)+5)//6):
    board=Image.new('RGB',(936,660),'#171522')
    draw=ImageDraw.Draw(board)
    for k,path in enumerate(files[page*6:page*6+6]):
        frames,durations=read_ase(path)
        im=frames[0]; x=(k%3)*312+25; y=(k//3)*330+38
        board.paste(im.resize((256,256),Image.Resampling.NEAREST),(x,y),im.resize((256,256),Image.Resampling.NEAREST))
        draw.text((x,y-24),path.stem,fill='#FDF5E5')
        colors=Counter('#%02X%02X%02X'%p[:3] for p in im.get_flattened_data() if p[3])
        draw.text((x,y+264),f'{im.size} | {len(colors)} colors | {len(frames)} frames',fill='#B7A393')
        report[path.stem]={'file':str(path.relative_to(ROOT)),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'canvas':list(im.size),'frames':len(frames),'durations_ms':durations,'bounds':im.getbbox(),'alpha':sorted(set(im.getchannel('A').get_flattened_data())),'colors':dict(colors.most_common())}
    board.save(ROOT/'Review'/f'Originals_{page+1}.png')
(ROOT/'SourceManifest.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:{f:v[f] for f in ['canvas','frames','bounds','alpha']}|{'colors':len(v['colors'])} for k,v in report.items()},indent=2))
