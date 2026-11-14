from pathlib import Path
from PIL import Image
import subprocess,struct
# Real multi-frame editable LibreSprite sources, with exact native colors and 45ms exposures.
root=Path(__file__).resolve().parents[3];out=root/'ArtSource/BombReadability/Effects';libre=r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
for name in ['FireHead','FireTrail','StarGlint']:
    ims=[Image.open(p).convert('RGBA') for p in sorted((root/'Assets/_Game/Resources/VFX/BombRefinement'/name).glob('*.png'))]
    colors=sorted({v[:3] for im in ims for v in im.getdata() if v[3]});palette=[0,0,0]+[v for c in colors for v in c];palette+= [0]*(768-len(palette));pal=[]
    for im in ims:
        q=Image.new('P',im.size);q.putpalette(palette);q.putdata([colors.index(v[:3])+1 if v[3] else 0 for v in im.getdata()]);pal.append(q)
    gif=out/'Editable'/f'{name}-native.gif';pal[0].save(gif,save_all=True,append_images=pal[1:],duration=40,loop=0,transparency=0,disposal=2,optimize=False)
    ase=out/'Editable'/f'{name}.aseprite';subprocess.run([libre,'-b',str(gif),'--save-as',str(ase)],check=True,capture_output=True)
    data=bytearray(ase.read_bytes());offset=128
    for i in range(struct.unpack_from('<H',data,6)[0]):
        assert struct.unpack_from('<H',data,offset+4)[0]==0xF1FA
        struct.pack_into('<H',data,offset+8,45);offset+=struct.unpack_from('<I',data,offset)[0]
    ase.write_bytes(data)
    sheet=out/'Frames'/f'{name}-verified.png';subprocess.run([libre,'-b',str(ase),'--sheet',str(sheet),'--sheet-type','horizontal'],check=True,capture_output=True)
    result=Image.open(sheet).convert('RGBA')
    for i,im in enumerate(ims):assert result.crop((i*im.width,0,(i+1)*im.width,im.height)).tobytes()==im.tobytes()
print('Refined flame tongues and verified real multi-frame LibreSprite sources.')
