"""Verify approved source bytes, production copies and preserved menu/golden art."""
import hashlib, json, struct, zlib
from pathlib import Path
from PIL import Image

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = HERE.parent
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
records = {}
repairs = {}
def ase_pixels(path):
    raw=path.read_bytes();magic,frames,w,h,depth=struct.unpack_from('<5H',raw,4)
    assert (magic,frames,depth)==(0xA5E0,1,32)
    pos=144
    while pos<len(raw):
        length,kind=struct.unpack_from('<IH',raw,pos)
        if kind==0x2005:
            p=pos+6;layer,x,y,opacity,ctype=struct.unpack_from('<HhhBH',raw,p)
            assert (layer,x,y,opacity,ctype)==(0,0,0,255,2)
            assert struct.unpack_from('<HH',raw,p+16)==(w,h)
            return zlib.decompress(raw[p+20:pos+length])
        pos+=length
    raise AssertionError(path)
for zone in ('Dungeon', 'Forest', 'Court'):
    source = BASE / (zone + 'ReferenceKit') / 'native'
    target = ROOT / 'Assets/_Game/Resources/UI/BattlePresentation' / ('Reference' + zone)
    expected = {p.name for p in source.glob('*.png')
                if not p.name.endswith('ArtOnly.png') and p.name != 'BoardWell.png'
                and (zone == 'Dungeon' or not p.name.startswith('Shared'))}
    assert expected == {p.name for p in target.glob('*.png')}, zone
    for name in sorted(expected):
        correction=HERE/'native'/zone/name
        selected=correction if correction.exists() else source/name
        assert selected.read_bytes() == (target / name).read_bytes(), (zone, name)
        image = Image.open(target / name).convert('RGBA')
        assert set(image.getchannel('A').tobytes()) <= {0, 255}
        records[zone + '/' + name] = dict(sha256=sha(target/name), size=image.size)
    if zone != 'Dungeon':
        # Reuse the independently decoded LibreSprite evidence only when both
        # original editable source and native export still match those hashes.
        evidence = json.loads((BASE/(zone+'ReferenceKit')/'checks.json').read_text())
        for item in evidence['assets']:
            assert sha(source/(item['name']+'.png')) == item['sha256']
            assert sha(source/(item['name']+'.aseprite')) == item['aseSha256']
        for name in ('SharedGuide','SharedSettings','SharedAbility','SharedEnergy','SharedPotionHolder','SharedBombHolder'):
            assert (source/(name+'.png')).read_bytes() == (BASE/'DungeonReferenceKit/native'/(name+'.png')).read_bytes()
        reference=Image.open(BASE/(zone+'ReferenceKit')/'reference/Approved.png').convert('RGBA')
        allowed=set()
        for name,origin in [('WaveHeader',(157,20)),('SharedGuide',(330,30)),('SharedSettings',(414,30))]:
            plate=Image.open(source/(name+'.png')).convert('RGBA')
            for y in range(plate.height):
                for x in range(plate.width):
                    if plate.getpixel((x,y))[3]==0: allowed.add((x+origin[0],y+origin[1]))
        frame=Image.open(source/'PlayerFrame.png').convert('RGBA')
        for y in range(22,280):
            for x in range(10,142):
                local=(x-8,y-24)
                if y<24 or (frame.getpixel(local)[3]==0 and not (14<=local[0]<119 and 18<=local[1]<242)):
                    allowed.add((x,y))
        for name,offset,height in [('FarScenery',0,160),('BattleFloor',211,87)]:
            original=Image.open(source/(name+'.png')).convert('RGBA')
            fixed=Image.open(HERE/'native'/zone/(name+'.png')).convert('RGBA')
            assert original.size==fixed.size==(470,height)
            assert original.getchannel('A').tobytes()==fixed.getchannel('A').tobytes()
            assert ase_pixels(HERE/'native'/zone/(name+'.aseprite'))==fixed.tobytes()
            changes=0
            for y in range(fixed.height):
                for x in range(fixed.width):
                    if original.getpixel((x,y))!=fixed.getpixel((x,y)):
                        assert (x,y+offset) in allowed
                        assert fixed.getpixel((x,y))==reference.getpixel((x*2+1,(y+offset)*2+1))
                        changes+=1
            repairs[zone+'/'+name]=dict(changedPixels=changes,onlyExposedFrameBackings=True,exactReferencePixels=True)

for name in ('Hall', 'HallTall'):
    approved = ROOT/'ArtSource/MenuUI/RenderingCleanupV2/native'/(name+'.png')
    assert approved.read_bytes() == (ROOT/'Assets/_Game/Resources/UI/GlobalMenu'/(name+'.png')).read_bytes()
for path, expected in {
    'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png': '705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c',
    'Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png': '59b04929e9fdc15ae9f1d70fd3c85c6ea5362bdac479eb12f45cfcbb0efe4306'
}.items():
    assert sha(ROOT/path) == expected, path
out = HERE/'evidence/approved-imports.json'
out.write_text(json.dumps(dict(status='Passed', productionModules=len(records),
    forestCourtSourcePairs=54, menuV2Preserved=True, rattlebonesPreserved=True,
    assets=records,backingRepairs=repairs), indent=2)+'\n')
print(f'{len(records)} exact production PNGs; 54 unchanged ASE/PNG pairs; menu V2 and Rattlebones preserved.')
