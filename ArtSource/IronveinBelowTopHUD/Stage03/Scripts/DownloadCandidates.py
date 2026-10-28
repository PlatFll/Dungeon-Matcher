from pathlib import Path
import json,urllib.request,hashlib
from PIL import Image,ImageDraw
F=Path('C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher')
R=F/'ArtSource/IronveinBelowTopHUD/Stage03'
job='d565cd16-d75b-450b-9888-31a693c097b0'
board=Image.new('RGB',(800,440),'#20222d');d=ImageDraw.Draw(board);stats=[]
for i in range(4):
 p=R/'Raw'/f'FrameKit-{i}.png'
 if not p.exists(): urllib.request.urlretrieve(f'https://api.pixellab.ai/mcp/images/{job}/download?index={i}',p)
 im=Image.open(p).convert('RGBA');x=i*200+4
 board.paste(im,(x,25),im);big=im.resize((192,192),Image.Resampling.NEAREST);board.paste(big,(x,150),big)
 d.text((x,5),f'Candidate {i} - native / 2x',fill='white')
 stats.append({'index':i,'size':im.size,'colors':len({c[:3] for c in im.getdata() if c[3]}),'alpha':sorted({c[3] for c in im.getdata()}),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
board.save(R/'Preview/RawCandidates.png');(R/'QA/RawStats.json').write_text(json.dumps(stats,indent=2));print(json.dumps(stats))
