"""Export native sprite inputs with transparent action margins, never resampling."""
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
base=ROOT/'ArtSource/Ironvein'
records=[]
for path in sorted((base/'Selected').glob('*.png')):
    im=Image.open(path).convert('RGBA')
    # Preserve each source pixel; anchor soles to one shared canvas baseline.
    bounds=im.getchannel('A').getbbox()
    width=80 if im.width==64 else 112
    height=80 if im.height==64 else 112
    at=((width-im.width)//2,height-bounds[3])
    result=Image.new('RGBA',(width,height))
    result.paste(im,at)
    target=base/'Inputs'/path.name;target.parent.mkdir(exist_ok=True)
    result.save(target)
    assert list(im.getchannel('A').getdata()).count(255)==list(result.getchannel('A').getdata()).count(255)
    records.append(dict(name=path.stem,source=str(path.relative_to(ROOT)),
                        output=str(target.relative_to(ROOT)),source_size=im.size,
                        canvas=result.size,integer_translation=at,scale=1,
                        source_sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
(base/'Production/input-layout.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
print(json.dumps(records,indent=2))

# Separate phase source: generated at native 64, placed on the mech's canvas so
# changing controllers cannot alter the common floor anchor or source-pixel size.
pilot=base/'Variants/grand_delver_pilot-v3/00.png'
if pilot.exists():
    im=Image.open(pilot).convert('RGBA');bounds=im.getbbox()
    target=Image.new('RGBA',(112,112));at=((112-im.width)//2,112-bounds[3]);target.paste(im,at)
    target.save(base/'Inputs/grand_delver_pilot.png')
    (base/'Production/pilot-layout.json').write_text(json.dumps(dict(source=str(pilot.relative_to(ROOT)),
        source_size=im.size,occupied_bounds=bounds,canvas=[112,112],integer_translation=at,scale=1,
        source_sha256=hashlib.sha256(pilot.read_bytes()).hexdigest()),indent=2)+'\n',encoding='utf-8')

smith=base/'Corrections/smith-edge/00.png'
if smith.exists():
    original=Image.open(base/'Inputs/seismic_smith.png').convert('RGBA');repaired=Image.open(smith).convert('RGBA')
    assert original.size==repaired.size==(80,80)
    for y in range(80):
        for x in range(80):
            if not (0<=x<12 and 30<=y<58): assert original.getpixel((x,y))==repaired.getpixel((x,y))
    repaired.save(base/'Inputs/seismic_smith-repaired.png')
    print('Smith: all unmasked RGBA pixels verified unchanged.')
