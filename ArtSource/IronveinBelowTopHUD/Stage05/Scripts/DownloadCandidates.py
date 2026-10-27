from pathlib import Path
import urllib.request,json,hashlib,concurrent.futures,shutil
from PIL import Image,ImageDraw
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage05'
jobs={'Timber':('9ba90d6e-1abf-48f1-9cf3-a04e2289425f',25),'Rock':('b1b1279c-3f49-46c2-8b86-2457e035bf32',25),'Crystal':('358ad8cf-02ac-42e7-96f4-28b41d002d6e',1),'Course':('62015827-61a3-42bd-9b91-2940e8c0e81e',1)}
def download(item):
 name,job,i=item;p=R/'Raw'/f'{name}-{i:02}.png'
 if not p.exists():urllib.request.urlretrieve(f'https://api.pixellab.ai/mcp/images/{job}/download?index={i}',p)
 return p
items=[(name,job,i) for name,(job,n) in jobs.items() for i in range(n)]
with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool:list(pool.map(download,items))
stats=[]
for name,(_,n) in jobs.items():
 sheet=Image.new('RGBA',(680,238*((n+4)//5)),'#20232c');d=ImageDraw.Draw(sheet)
 for i in range(n):
  p=R/'Raw'/f'{name}-{i:02}.png';im=Image.open(p).convert('RGBA');x=i%5*136+4;y=i//5*238+4
  d.text((x,y),str(i),fill='white');sheet.alpha_composite(im,(x+68,y));sheet.alpha_composite(im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST),(x,y+36))
  stats.append({'name':name,'index':i,'size':im.size,'bounds':im.getbbox(),'colors':len({c[:3] for c in im.get_flattened_data() if c[3]}),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
 sheet.save(R/f'Preview/{name}Candidates.png')
(R/'QA/RawCandidates.json').write_text(json.dumps(stats,indent=2))
if Path(__file__).resolve()!=(R/'Scripts/DownloadCandidates.py').resolve():shutil.copyfile(__file__,R/'Scripts/DownloadCandidates.py')
print('Saved 52 native candidates; existing generation receipts preserved')
