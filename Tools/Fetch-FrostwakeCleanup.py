"""Fetch exact PixelLab edits and unpack without resampling or painting pixels."""
from pathlib import Path
from urllib.request import urlopen
from PIL import Image
import json,hashlib
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake';out=root/'cleanup'
receipts=json.loads((out/'receipts.json').read_text())
for key,receipt in receipts.items():
    path=out/(key+'-clean.png')
    if not path.exists():
        with urlopen(receipt['url'],timeout=120) as response:path.write_bytes(response.read())
    original=Image.open(out/(key+'-source.png')).convert('RGBA');clean=Image.open(path).convert('RGBA')
    assert original.size==clean.size
    changed=sum(a!=b for a,b in zip(original.get_flattened_data(),clean.get_flattened_data()))
    receipt['changedPixels']=changed;receipt['sha256']=hashlib.sha256(path.read_bytes()).hexdigest()
    if key=='hit':
        for item in json.loads((out/'hit-manifest.json').read_text()):
            x,y,w,h=item['rect'];target=root/item['path'];target=target.parent.parent/'clean'/target.name
            target.parent.mkdir(exist_ok=True);clean.crop((x,y,x+w,y+h)).save(target)
    elif key=='champion':
        folder=root/'motion/iceborn_champion/Release/clean';folder.mkdir(exist_ok=True)
        for i in range(9):
            x=(i%4)*128;y=(i//4)*104;clean.crop((x,y,x+128,y+104)).save(folder/(f'{i:02}.png'))
    elif key=='shell':
        for i,name in enumerate(['Frost','HeavyIce','Bifrost']):
            clean.crop((i*64,0,i*64+64,64)).save(out/(name+'.png'))
(out/'receipts.json').write_text(json.dumps(receipts,indent=2));print(json.dumps(receipts,indent=2))
