"""Read-only pixel audits of native exports. Pillow is not used to author game art."""
from pathlib import Path
import hashlib, json, struct, zlib
from PIL import Image

ROOT = Path(__file__).resolve().parent
OUT = ROOT / 'Validation'
OUT.mkdir(exist_ok=True)
rows = []
for folder in ['Environment', 'UI']:
    for path in sorted((ROOT / folder).rglob('*.png')):
        if 'Review' in path.stem: continue
        im = Image.open(path).convert('RGBA')
        alpha = sorted(set(im.getchannel('A').get_flattened_data()))
        assert set(alpha) <= {0,255}, (path, alpha)
        transparent = folder == 'UI' and not any(k in path.stem for k in ['Middle', 'Fill', 'Track']) or 'Props' in path.parts
        if transparent: assert 0 in alpha, ('required transparency', path)
        source = path.with_suffix('.aseprite')
        assert source.is_file(), source
        data=source.read_bytes()
        assert struct.unpack_from('<H',data,4)[0] == 0xA5E0
        assert struct.unpack_from('<HH',data,8) == im.size
        offset=144
        native=None
        for _ in range(struct.unpack_from('<H',data,134)[0]):
            length,kind=struct.unpack_from('<IH',data,offset)
            if kind==0x2004: assert struct.unpack_from('<H',data,offset+6)[0] & 8 == 0, ('background layer flag',source)
            if kind==0x2005:
                assert struct.unpack_from('<H',data,offset+13)[0] == 2
                native=zlib.decompress(data[offset+26:offset+length])
            offset+=length
        assert native == im.tobytes(), ('PNG/source RGBA mismatch',path)
        rows.append(dict(file=path.relative_to(ROOT).as_posix(), source=source.relative_to(ROOT).as_posix(),
            size=im.size, bounds=im.getbbox(), alpha=alpha,
            palette=sorted('#%02x%02x%02x%02x' % c for c in set(im.get_flattened_data())),
            sha256=hashlib.sha256(path.read_bytes()).hexdigest(), source_sha256=hashlib.sha256(source.read_bytes()).hexdigest(),
            ppu=64, pivot=[0.5,0.5], border=([16]*4 if path.stem.startswith(('ButtonLarge','Panel')) else [8]*4 if path.stem.startswith('ButtonSmall') else [16,8,16,8] if path.stem=='WavePlaque' else [0]*4), status='review_candidate'))

for family in ['ButtonLarge','ButtonSmall','Settings']:
    base=Image.open(ROOT/f'UI/{family}Normal.png').convert('RGBA')
    for state in ['Highlighted','Pressed','Disabled']:
        variant=Image.open(ROOT/f'UI/{family}{state}.png').convert('RGBA')
        assert variant.size==base.size and variant.getchannel('A').tobytes()==base.getchannel('A').tobytes(), (family,state)

master=Image.open(ROOT/'Environment/DungeonMaster.png').convert('RGBA')
assert master.size == (512,384)
assert set(master.getchannel('A').get_flattened_data()) == {255}
for row in range(6):
    for col in range(8):
        tile=Image.open(ROOT/f'Environment/Baked/Cell{chr(65+row)}{chr(65+col)}.png').convert('RGBA')
        assert tile.size == (64,64)
        assert tile.tobytes() == master.crop((col*64,row*64,col*64+64,row*64+64)).tobytes(), (row,col)
edges=[]
for names,vertical in [(['WallA','WallB','WallC','WallD','WallWarmA','WallWarmB'],True),
                       (['FloorA','FloorB','FloorC','FloorD'],False),
                       (['FoundationA','FoundationB','FoundationC'],True)]:
    tiles={name:Image.open(ROOT/f'Environment/Tiles/{name}.png').convert('RGBA') for name in names}
    for a,ia in tiles.items():
        assert ia.size==(64,64)
        for b,ib in tiles.items():
            assert ia.crop((63,0,64,64)).tobytes()==ib.crop((0,0,1,64)).tobytes(), ('horizontal',a,b)
            if vertical: assert ia.crop((0,63,64,64)).tobytes()==ib.crop((0,0,64,1)).tobytes(), ('vertical',a,b)
            edges.append([a,b,'horizontal+vertical' if vertical else 'horizontal'])
(OUT/'AssetAudit.json').write_text(json.dumps(rows,indent=2)+'\n')
(OUT/'EnvironmentAssembly.json').write_text(json.dumps(dict(master_size=[512,384],baked_cells_verified=48,
    all_neighbor_pairs=edges,bands=dict(wall=[0,255],floor=[256,319],foundation=[320,383])),indent=2)+'\n')
print(f'PASS: {len(rows)} native exports, binary alpha, native sources; 48 baked cells match master; {len(edges)} tile neighbor pairs match.')
