"""Retrieve free PixelLab workbench outputs; extract native slices without repainting."""
import hashlib,json,re,urllib.request
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/'ArtSource/Ironvein'
output=SOURCE/'UI/Prepared'
output.mkdir(parents=True,exist_ok=True)
records=[]
for receipt in ['ui-state-edits.json','ui-player-frame-assembly02.json','ui-panel-exact.json','ui-native-frame-correction.json','ui-native-plaque-correction.json','ui-quiet-backing02.json']:
    for row in json.loads((SOURCE/'Production'/receipt).read_text(encoding='utf-8')):
        text='\n'.join(x['text'] for x in row['result'])
        base=re.search(r'download (https://api\.pixellab\.ai/mcp/pixel-tools/[a-f0-9-]+/):',text)[1]
        leaf='image.png' if 'image_id:' in text else 'assets/front/full.png'
        url=base+leaf
        raw=output/(row['name']+'-raw.png')
        if not raw.exists():
            with urllib.request.urlopen(url,timeout=90) as response:raw.write_bytes(response.read())
        im=Image.open(raw).convert('RGBA')
        crop=None
        if row['name']=='WavePlaque':
            assert im.size==(128,48)
            assert im.crop((0,32,128,48)).getbbox() is None
            crop=[0,0,128,32];im=im.crop(crop)
        if row['name'].startswith('Player'):
            assert im.size==(146,64)
            assert im.crop((0,16,146,64)).getbbox() is None
            crop=[0,0,146,16];im=im.crop(crop)
        target=output/(row['name']+'.png');im.save(target)
        if 'changes' in row:
            original=Image.open(SOURCE/'UI'/row['source']/'00.png').convert('RGBA')
            assert im.size==original.size
            assert im.getchannel('A').tobytes()==original.getchannel('A').tobytes()
            mapping=row['changes'][0]['colors']
            for before,after in zip(original.getdata(),im.getdata()):
                key='#'+''.join(f'{v:02x}' for v in before)
                expected=tuple(bytes.fromhex(mapping[key][1:])) if key in mapping else before
                assert after==expected,(row['name'],before,after,expected)
        records.append(dict(name=row['name'],receipt=receipt,url=url,crop=crop,size=im.size,
            colors=len(set(c[:3] for c in im.getdata() if c[3])),alpha=sorted(set(im.getchannel('A').getdata())),
            sha256=hashlib.sha256(target.read_bytes()).hexdigest(),source_sha256=hashlib.sha256(raw.read_bytes()).hexdigest()))
(output/'technical.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(files=len(records),all_exact=True,names=[r['name'] for r in records])))
