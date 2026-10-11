"""Contact sheets and pixel metrics; never changes motion sources."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake';q=json.loads((root/'motion-queue.json').read_text());checks=[]
out=root/'motion-review';out.mkdir(exist_ok=True)
for actor in sorted({c['id'] for c in q if c.get('completed')}):
    clips=[c for c in q if c['id']==actor and c.get('completed')];sheet=Image.new('RGB',(128*9,130*len(clips)),'#303238');d=ImageDraw.Draw(sheet)
    for row,c in enumerate(clips):
        files=sorted((root/'motion'/actor/c['state']/'raw').glob('*.png'));d.text((5,row*130+3),actor+' '+c['state'],fill='white')
        for i,path in enumerate(files):
            im=Image.open(path).convert('RGBA');sheet.paste(im,(i*128,row*130+22),im)
            checks.append(dict(id=actor,state=c['state'],frame=i,size=im.size,bounds=im.getbbox(),colors=len({v for v in im.get_flattened_data() if v[3]}),alpha=sorted(set(im.getchannel('A').get_flattened_data())),sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    sheet.save(out/(actor+'.png'))
(out/'raw-checks.json').write_text(json.dumps(checks,indent=2));print(f'Review sheets for {len({c["id"] for c in checks})} actors; {len(checks)} actual cels inspected numerically.')
