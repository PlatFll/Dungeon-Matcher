"""Independently decode native ASE cels and validate exact PNG/layer exports."""
import hashlib,json,struct,zlib,re
from pathlib import Path
from PIL import Image
K=Path(__file__).resolve().parent; P=K.parents[2]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def decode(path):
    raw=path.read_bytes();magic,frames,w,h,depth=struct.unpack_from('<5H',raw,4)
    assert (magic,frames,depth)==(0xa5e0,1,32),path
    layers={};names=[];pos=144
    while pos<len(raw):
        size,kind=struct.unpack_from('<IH',raw,pos);s=pos+6
        if kind==0x2004:
            length=struct.unpack_from('<H',raw,s+16)[0]
            names.append(raw[s+18:s+18+length].decode())
        if kind==0x2005:
            layer,x,y,opacity,ctype=struct.unpack_from('<HhhBH',raw,s)
            assert opacity==255 and ctype in (0,2),(path,ctype)
            cw,ch=struct.unpack_from('<HH',raw,s+16)
            data=raw[s+20:pos+size];data=zlib.decompress(data) if ctype==2 else data
            im=Image.frombytes('RGBA',(cw,ch),data)
            canvas=Image.new('RGBA',(w,h));canvas.paste(im,(x,y));layers[layer]=canvas
        pos+=size
    out=Image.new('RGBA',(w,h))
    for n,im in sorted(layers.items()):out=Image.alpha_composite(out,im)
    return out,names
allowed={tuple(bytes.fromhex(v)) for v in re.findall(r"'([0-9a-f]{6})'",(K/'Palette.js').read_text())}
record={'status':'Passed native checks; visual review separate','native_size':[470,836],'palette_budget':len(allowed),'generation_calls':0,'assets':[]}
record['build_sources']={name:sha(K/name) for name in ['Build.js','TopReconstruction.js','Palette.js','LayeredMaster.js']}
for png in sorted((K/'native').glob('*.png')):
    im=Image.open(png).convert('RGBA');ase,names=decode(png.with_suffix('.aseprite'))
    assert im.size==ase.size and im.tobytes()==ase.tobytes(),png
    alpha=set(im.getchannel('A').tobytes());assert alpha<={0,255},png
    colors={c[:3] for c in im.get_flattened_data() if c[3]}
    assert colors<=allowed,(png,len(colors-allowed))
    record['assets'].append({'name':png.stem,'size':im.size,'opaque_colors':len(colors),'alpha':sorted(alpha),'sha256':sha(png),'ase_sha256':sha(png.with_suffix('.aseprite')),'layers':names})
flat=Image.open(K/'native/DungeonArtOnly.png').convert('RGBA')
layered,names=decode(K/'native/DungeonLayered.aseprite')
assert len(names)>15 and len(names)==len(set(names))
assert flat.tobytes()==layered.tobytes(),'Layered master must reconstruct the flat export exactly'
record['layered_master_layers']=len(names)
provenance=json.loads((K/'provenance.json').read_text())
for rel,expected in provenance['protected'].items():assert sha(P/rel)==expected,rel
record['protected_files_unchanged']=len(provenance['protected'])
(K/'checks.json').write_text(json.dumps(record,indent=2)+'\n')
print(f"{len(record['assets'])} exact LibreSprite/PNG pairs; {len(names)} editable master layers; binary alpha; {len(allowed)}-color material palette; {len(provenance['protected'])} protected files unchanged.")
