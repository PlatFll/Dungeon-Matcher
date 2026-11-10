"""Reproduce selected PixelLab still cleanup and exact LibreSprite exports."""
from pathlib import Path
import hashlib, json, subprocess
from PIL import Image, ImageDraw

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
LIBRE = Path('C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe')
RUNTIME = ROOT/'Assets/_Game/Resources/Combat/ApprovedRevisions'
INK = (10,13,17,255)
manifest = {'guide_version':'1.17.2','generation_cost':20,'effects':{}}
manifest['guide_sha256'] = hashlib.sha256((ROOT/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt').read_bytes()).hexdigest()
manifest['references'] = json.loads((HERE.parent/'manifest.json').read_text())['references']
manifest['raw_candidates'] = {p.relative_to(HERE).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted((HERE/'Raw').glob('*/*.png'))}
for ref in manifest['references'].values():
    assert hashlib.sha256((ROOT/ref['path']).read_bytes()).hexdigest()==ref['sha256']
for name,index in [('MarineSnare',51),('HearthEmber',3)]:
    raw = HERE/'Raw'/name/f'{index:02d}.png'
    im = Image.open(raw).convert('RGBA')
    original = im.copy()
    changes = []
    if name == 'MarineSnare':
        # Preserve every colored rope/barb pixel and the perimeter. Thin only
        # heavy interior ink; retain the upper/left separation on each cord.
        interior = Image.new('1',(32,32))
        ImageDraw.Draw(interior).polygon([(7,10),(11,6),(20,7),(25,12),(25,21),(21,26),(11,26),(6,21),(6,13)],fill=1)
        for y in range(32):
            for x in range(32):
                if not interior.getpixel((x,y)) or original.getpixel((x,y)) != INK: continue
                neighbors = [original.getpixel((x+dx,y+dy)) for dx,dy in [(1,0),(0,1)]]
                if not any(c[3] and c != INK for c in neighbors):
                    im.putpixel((x,y),(0,0,0,0)); changes.append([x,y])
    if name == 'HearthEmber':
        # The raw stone base touches the canvas bottom. Move the intact drawing
        # up two native pixels, retaining all pixels and two clear rows below.
        im = Image.new('RGBA',(32,32))
        im.alpha_composite(original,(0,-2))
    # Clear invisible RGB only; no alpha thresholding or resampling.
    for y in range(32):
        for x in range(32):
            if not im.getpixel((x,y))[3]: im.putpixel((x,y),(0,0,0,0))
    png = HERE/(name+'.png'); ase = HERE/(name+'.ase')
    im.save(png)
    subprocess.run([str(LIBRE),'-b',str(png),'--save-as',str(ase)],check=True)
    subprocess.run([str(LIBRE),'-b',str(ase),'--save-as',str(png)],check=True)
    exported = Image.open(png).convert('RGBA')
    assert exported.tobytes()==im.tobytes(), 'LibreSprite changed native pixels'
    assert im.size==(32,32) and set(im.getchannel('A').getdata())=={0,255}
    assert im.getbbox()[0]>=2 and im.getbbox()[1]>=2 and im.getbbox()[2]<=31 and im.getbbox()[3]<=31
    (RUNTIME/(name+'.png')).write_bytes(png.read_bytes())
    guid={'MarineSnare':'189bafbb194a47bf9d1be6cd04c88421','HearthEmber':'695d03ce24364970b15ee3b9b6e16b92'}[name]
    meta=(RUNTIME/(name+'.png.meta')).read_text()
    assert 'guid: '+guid in meta and 'filterMode: 0' in meta and 'spritePixelsToUnits: 64' in meta
    manifest['effects'][name] = {
        'selected_index':index,'raw_path':raw.relative_to(ROOT).as_posix(),
        'raw_sha256':hashlib.sha256(raw.read_bytes()).hexdigest(),
        'final_sha256':hashlib.sha256(png.read_bytes()).hexdigest(),
        'dimensions':list(im.size),'bounds':list(im.getbbox()),
        'opaque_colors':len({c for c in im.getdata() if c[3]}),
        'transparent_fraction':sum(c[3]==0 for c in im.getdata())/1024,
        'alpha':[0,255],'removed_interior_ink_pixels':changes,
        'native_translation':[0,-2] if name=='HearthEmber' else [0,0],
        'opaque_colors_preserved':{c for c in im.getdata() if c[3]}=={c for c in original.getdata() if c[3]},
        'libresprite_roundtrip_rgba_equal':True,'runtime_guid':guid,'runtime_guid_preserved':True,
        'cleanup':'Marine net: thin redundant interior ink only; Hearth: translate intact native drawing upward two pixels for clear bottom padding. Invisible RGB normalized.'}
(HERE/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
review = Image.new('RGBA',(800,360),'#202633'); d=ImageDraw.Draw(review)
for col,name in enumerate(['MarineSnare','HearthEmber']):
    x=col*400+12;d.text((x,10),name+' / native 32px + 4x',fill='white')
    for row,(label,path) in enumerate([('Earlier direct-pixel draft',HERE.parent/(name+'.png')),('PixelLab + LibreSprite',HERE/(name+'.png'))]):
        y=42+row*154;im=Image.open(path).convert('RGBA')
        d.text((x,y),label,fill='#dce4ee');review.alpha_composite(im,(x,y+28))
        review.alpha_composite(im.resize((128,128),Image.Resampling.NEAREST),(x+65,y+21))
        # A pale field makes transparent gaps and silhouette clearance visible.
        swatch=Image.new('RGBA',(32,32),'#bfb5a2');swatch.alpha_composite(im)
        review.alpha_composite(swatch.resize((128,128),Image.Resampling.NEAREST),(x+210,y+21))
review.save(HERE/'Review.png')
print(json.dumps({n:{k:v for k,v in m.items() if k!='removed_interior_ink_pixels'} for n,m in manifest['effects'].items()},indent=2))
