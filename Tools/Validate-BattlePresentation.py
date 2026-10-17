"""Read-only ASE/PNG/import/provenance checks. No raster pixels are written."""
import hashlib,json,struct,zlib
from pathlib import Path
from PIL import Image
root=Path(__file__).resolve().parent.parent
source=root/'ArtSource/BattlePresentation'
target=root/'Assets/_Game/Resources/UI/BattlePresentation'
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def ase_pixels(path):
    data=path.read_bytes()
    magic,frames,w,h,depth=struct.unpack_from('<5H',data,4)
    assert (magic,frames,depth)==(0xA5E0,1,32),path
    canvas=bytearray(w*h*4); offset=144;cels=0
    while offset<len(data):
        length,kind=struct.unpack_from('<IH',data,offset)
        if kind==0x2004: assert struct.unpack_from('<H',data,offset+6)[0]&12==0,path
        if kind==0x2005:
            p=offset+6; layer,x,y,opacity,kind=struct.unpack_from('<HhhBH',data,p)
            assert (layer,opacity,kind)==(0,255,2),path
            cw,ch=struct.unpack_from('<HH',data,p+16)
            pixels=zlib.decompress(data[p+20:offset+length])
            for row in range(ch):canvas[((row+y)*w+x)*4:((row+y)*w+x+cw)*4]=pixels[row*cw*4:(row+1)*cw*4]
            cels+=1
        offset+=length
    assert cels==1,path
    return (w,h),bytes(canvas)
report=[]
for path in sorted((source/'native').rglob('*.png')):
    relative=path.relative_to(source/'native'); im=Image.open(path).convert('RGBA')
    size,pixels=ase_pixels(path.with_suffix('.aseprite'))
    assert im.size==size and im.tobytes()==pixels,relative
    assert path.read_bytes()==(target/relative).read_bytes(),relative
    alpha=sorted(set(im.getchannel('A').tobytes()));assert set(alpha)<={0,255},relative
    if relative.stem in ('Corner','PlayerTop','PlayerMiddle','PlayerBottom','Banner','Energy','Rune','Lantern','Bubble','Foliage','Kelp'):
        assert alpha==[0,255],(relative,'requires cutout transparency')
    meta=(target/relative.with_suffix('.png.meta')).read_text()
    for setting in ('enableMipMap: 0','filterMode: 0','spritePixelsToUnits: 64','textureCompression: 0','nPOTScale: 0'):
        assert setting in meta,(relative,setting)
    assert 'overridden: 1' not in meta,relative
    report.append(dict(asset=relative.as_posix(),size=im.size,alpha=alpha,colors=sum(1 for _,p in im.getcolors(im.width*im.height) if p[3]),sha256=sha(path),asePixelsMatch=True,unityBytesMatch=True))
expected={
'Assets/_Game/Resources/BattleEnvironments/DrownedCourt_Throne.prefab':'f2a3e6059c4df8ebce3279e7ac29ca5acd9a52e529425abe70c51ecab097bd10',
'Assets/_Game/Resources/UI/Consumables/SlotDisabled.png.meta':'0f7e12e81377e9d521a4d132a17a3b3bfcf575daac8434f77bf20849575f5518',
'Assets/_Game/Resources/UI/Consumables/SlotHighlighted.png.meta':'3a32303ab05166168f39ca8f6f06367234df30cb46393acb6fa5fde7331762f7',
'Assets/_Game/Resources/UI/Consumables/SlotPressed.png.meta':'0443273d14468ccb1584abcf6ab204674debcae15cda563f6aba543e6dc7e88f',
'ArtSource/MenuUI/button-frame-revision.json':'f692fed6c1de726d13874d339c5203709b8bcfadb0a9b7cef98f563a46eb5b58'}
for path,value in expected.items():assert sha(root/path)==value,path
assert len(report)==81,len(report)
(source/'native-checks.json').write_text(json.dumps({'assets':report,'preservedLocalSha256':expected},indent=2)+'\n')
print(f'PASS: {len(report)} exact native ASE/PNG/Unity exports; binary/cutout alpha; Point/64PPU/uncompressed imports; five local files unchanged.')
