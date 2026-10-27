from pathlib import Path
from PIL import Image,ImageDraw
import json,urllib.request,shutil,hashlib,concurrent.futures
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage06'
job='5176647b-3c64-49d1-b639-af022b6aed08'
def fetch(i):
 p=R/'Raw'/f'Bracket-{i:02}.png'
 if not p.exists():
  with urllib.request.urlopen(f'https://api.pixellab.ai/mcp/images/{job}/download?index={i}',timeout=120) as r:p.write_bytes(r.read())
 with Image.open(p) as im:assert im.size==(32,32)
 return {'index':i,'file':p.relative_to(R).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool:items=list(pool.map(fetch,range(64)))
sheet=Image.new('RGBA',(800,800),'#242938');d=ImageDraw.Draw(sheet)
for i in range(64):
 x=i%8*100;y=i//8*100;d.text((x+3,y+2),str(i),fill='white');im=Image.open(R/'Raw'/f'Bracket-{i:02}.png').convert('RGBA');sheet.alpha_composite(im.resize((64,64),Image.Resampling.NEAREST),(x+15,y+24))
sheet.save(R/'Preview/BracketCandidates.png')
gpath=R/'Generation.json'
if not gpath.exists():shutil.copyfile(Path(r'C:/Users/USER/.codex/visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a/IronveinStagePack20261027')/'stage06-generation.json',gpath)
g=json.loads(gpath.read_text());g['raw_assets']=items;gpath.write_text(json.dumps(g,indent=2))
target=R/'Scripts/DownloadCandidates.py'
if Path(__file__).resolve()!=target.resolve():shutil.copyfile(__file__,target)
print('Retained all 64 PixelLab bracket candidates, contact sheet ready')
