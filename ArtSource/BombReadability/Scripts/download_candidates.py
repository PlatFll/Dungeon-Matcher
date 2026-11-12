from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
import urllib.request,json
from PIL import Image,ImageDraw
root=Path(__file__).resolve().parents[1]/'Candidates'
jobs={'Poison':'883e0802-db5a-4d84-aded-6a87dcfc5d94','Healing':'dcf5f97a-457a-4754-87ba-3e60a9bde5b6','Directional':'7f8a5550-2fa1-42ec-9ec7-0260f5cb01a5','Star':'2483e3f1-1e66-4e3b-9cbe-d21232edcd80'}
def fetch(item):
    name,job,i=item;p=root/'Raw'/name/f'{i:02}.png';p.parent.mkdir(parents=True,exist_ok=True)
    if not p.exists():urllib.request.urlretrieve(f'https://api.pixellab.ai/mcp/images/{job}/download?index={i}',p)
    im=Image.open(p);assert im.size==(64,64),(p,im.size)
with ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(fetch,[(n,j,i) for n,j in jobs.items() for i in range(16)]))
for name in jobs:
    sheet=Image.new('RGB',(768,832),'#20232e');d=ImageDraw.Draw(sheet)
    for i in range(16):
        x=i%4*192;y=i//4*208;d.text((x+8,y+3),f'{name} {i:02}',fill='white');im=Image.open(root/'Raw'/name/f'{i:02}.png').convert('RGBA');sheet.paste(im.resize((192,192),Image.Resampling.NEAREST),(x,y+16),im.resize((192,192),Image.Resampling.NEAREST))
    sheet.save(root/f'{name}-all.png')
(root/'jobs.json').write_text(json.dumps({'jobs':jobs,'generations_each':10,'total_generations':40,'reference_Rattlebones_sha256':'705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c','reference_ShieldBomb_sha256':'3992f944830f2827dc16fd105695fe4112c0c5c1953d5bd6e467523e8ab49d40','status':'candidates, not approved'},indent=2))
print('Downloaded and inspected 64 native candidates; 40 generations charged.')
