"""Native PixelLab flame cleanup, locked-stone animation, and full-cell net export."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib,json,re,subprocess,uuid,sys

HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
RUNTIME=ROOT/'Assets/_Game/Resources/Combat/ApprovedRevisions'
LIBRE='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
INK=(10,13,17,255)
base=Image.open(HERE.parent/'PixelLab/HearthEmber.png').convert('RGBA')
assert hashlib.sha256((RUNTIME/'HearthEmber.png').read_bytes()).hexdigest()=='c7e9a2096ec252dd4e0ab4966a54259615f60df899e1283b3ed84c982e7ecaca'
palette=sorted({c for c in base.getdata() if c[3]})
stone={c for c in palette if c[2]>c[0] and c!=INK}
warm={c for c in palette if c[0]>c[2]}
editable=set()
for y in range(21):
    row=[x for x in range(32) if base.getpixel((x,y)) in warm]
    for x in range(32):
        if y<15 or (row and min(row)-1<=x<=max(row)+1 and base.getpixel((x,y)) not in stone):editable.add((x,y))
frames=[]
for i in range(8):
    raw=Image.open(HERE/'Raw/HearthFlame'/f'{i:02d}.png').convert('RGBA')
    im=base.copy()
    for x,y in editable:
        c=raw.getpixel((x,y))
        if c[3]==0:im.putpixel((x,y),(0,0,0,0))
        else:
            assert c[3]==255
            im.putpixel((x,y),min(palette,key=lambda p:sum((a-b)**2 for a,b in zip(c[:3],p[:3]))))
    # PixelLab frame 3 has a detached near-black speck. Keep the connected
    # hearth/flame, removing only isolated components of up to three pixels.
    todo={(x,y) for y in range(32) for x in range(32) if im.getpixel((x,y))[3]}
    while todo:
        start=todo.pop();component={start};stack=[start]
        while stack:
            x,y=stack.pop()
            for dx,dy in [(1,0),(-1,0),(0,1),(0,-1),(1,1),(-1,1),(1,-1),(-1,-1)]:
                p=(x+dx,y+dy)
                if p in todo:todo.remove(p);component.add(p);stack.append(p)
        if len(component)<=3:
            assert component<=editable
            for p in component:im.putpixel(p,(0,0,0,0))
    if i==0:assert im.tobytes()==base.tobytes()
    assert all(im.getpixel((x,y))==base.getpixel((x,y)) for y in range(32) for x in range(32) if (x,y) not in editable)
    assert all(im.getpixel((x,y))==base.getpixel((x,y)) for y in range(32) for x in range(32) if base.getpixel((x,y)) in stone)
    assert all(im.getpixel((x,y))[3]==0 for x,y in [(x,y) for y in [0,31] for x in range(32)]+[(x,y) for x in [0,31] for y in range(32)])
    frames.append(im)

# GIF input carries the exact shared palette and 120ms exposures into LibreSprite.
colors=[(0,0,0,0)]+palette
gifframes=[]
for im in frames:
    p=Image.new('P',(32,32));p.putpalette([v for c in colors for v in c[:3]]+[0]*(768-len(colors)*3))
    p.putdata([colors.index(c) for c in im.getdata()]);p.info['transparency']=0;gifframes.append(p)
gifframes[0].save(HERE/'HearthFlame.gif',save_all=True,append_images=gifframes[1:],duration=120,loop=0,transparency=0,disposal=2,optimize=False)
subprocess.run([LIBRE,'-b',str(HERE/'HearthFlame.gif'),'--save-as',str(HERE/'HearthFlame.ase')],check=True)
subprocess.run([LIBRE,'-b',str(HERE/'HearthFlame.ase'),'--sheet-type','horizontal','--sheet',str(HERE/'HearthFlame-strip.png')],check=True)
sheet=Image.open(HERE/'HearthFlame-strip.png').convert('RGBA');assert sheet.size==(256,32)
template=(RUNTIME/'HearthEmber.png.meta').read_text()
def guid(name):return uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/FlameAndFullTileNet/'+name).hex
def meta(path,folder=False):
    if path.with_name(path.name+'.meta').exists():return
    text='fileFormatVersion: 2\nguid: '+guid(path.relative_to(ROOT).as_posix())+'\n'
    if folder:text+='folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
    elif path.suffix=='.png':
        text=re.sub(r'(?m)^guid: \w+', 'guid: '+guid(path.name),template)
        text=re.sub(r'spriteID: \w+','spriteID: '+guid(path.name),text)
    path.with_name(path.name+'.meta').write_text(text)
for i,im in enumerate(frames):
    cel=sheet.crop((i*32,0,(i+1)*32,32))
    assert cel.tobytes()==im.tobytes(),i
    cel.save(HERE/'Frames'/f'HearthFlame_{i:02d}.png')
    target=RUNTIME/'HearthFlame'/f'HearthFlame_{i:02d}.png';cel.save(target);meta(target)
meta(RUNTIME/'HearthFlame',True)
for rel in ['Assets/_Game/Scripts/Board/VFX/HearthEmberFlameView.cs','Assets/_Game/Scripts/Editor/EnemyEffectArtRevisionTests.cs']:meta(ROOT/rel)

net=Image.open(HERE/'Raw/MarineNet/04.png').convert('RGBA')
assert net.size==(64,64) and set(net.getchannel('A').getdata())=={0,255}
net.putdata([c if c[3] else (0,0,0,0) for c in net.getdata()]);net.save(HERE/'MarineSnare.png')
subprocess.run([LIBRE,'-b',str(HERE/'MarineSnare.png'),'--save-as',str(HERE/'MarineSnare.ase')],check=True)
subprocess.run([LIBRE,'-b',str(HERE/'MarineSnare.ase'),'--save-as',str(RUNTIME/'MarineSnare.png')],check=True)
assert Image.open(RUNTIME/'MarineSnare.png').convert('RGBA').tobytes()==net.tobytes()

manifest={'guide_version':'1.17.2','guide_sha256':hashlib.sha256((ROOT/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt').read_bytes()).hexdigest(),
          'references':json.loads((HERE.parent/'PixelLab/manifest.json').read_text())['references'],
          'generations':11,'flame':{'approved_still_sha256':hashlib.sha256((RUNTIME/'HearthEmber.png').read_bytes()).hexdigest(),
          'frames':8,'duration_ms':[120]*8,'canvas':[32,32],'opaque_palette':[list(c) for c in palette],
          'all_stone_pixels_identical':True,'outside_flame_mask_identical':True,'frame_zero_exact_approved_still':True,
          'source_indices':list(range(8)),'libresprite_rgba_equal':True},
          'net':{'source_index':4,'canvas':[64,64],'bounds':net.getbbox(),'opaque_colors':len({c for c in net.getdata() if c[3]}),
          'transparent_fraction':sum(c[3]==0 for c in net.getdata())/4096,'native_geometry_preserved':True,'libresprite_rgba_equal':True},
          'files':{p.relative_to(HERE).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(HERE.rglob('*.png')) if 'Preview' not in p.parts}}
(HERE/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('8 exact 32px flame cels, locked stones, 960ms loop; 64px full-tile net, no resampling. LibreSprite verified.')
if (HERE/'NetRopeRecolor/run.py').exists():
    subprocess.run([sys.executable,str(HERE/'NetRopeRecolor/run.py')],cwd=ROOT,check=True)
