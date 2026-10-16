"""Read-only baseline, protected-asset hashes, and material inspection."""
from pathlib import Path
import hashlib,json,shutil
from collections import Counter
from PIL import Image,ImageDraw,ImageFont
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
if (HERE/'baseline.json').exists():
    raise SystemExit('Baseline already exists; refusing to overwrite the original artwork/evidence.')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for name in ['before','native','review','evidence','logs']:(HERE/name).mkdir(exist_ok=True)
live=ROOT/'Assets/_Game/Resources/UI/GlobalMenu'
records={}
for p in live.iterdir():
    if p.is_file():records[p.relative_to(ROOT).as_posix()]=sha(p)
for folder in ['Assets/_Game/Scripts/UI','Assets/_Game/Scenes']:
    for p in (ROOT/folder).rglob('*'):
        if p.is_file():records[p.relative_to(ROOT).as_posix()]=sha(p)
for p in [ROOT/'Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png',ROOT/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt',ROOT/'Docs/ArtDirection/Golden_Reference_Register.md',ROOT/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png',ROOT/'ArtSource/MenuUI/references/MainMenuTarget.png']:
    records[p.relative_to(ROOT).as_posix()]=sha(p)
for p in ['Assets/_Game/Resources/BattleEnvironments/DrownedCourt_Throne.prefab','Assets/_Game/Resources/UI/Consumables/SlotDisabled.png.meta','Assets/_Game/Resources/UI/Consumables/SlotHighlighted.png.meta','Assets/_Game/Resources/UI/Consumables/SlotPressed.png.meta','ArtSource/MenuUI/button-frame-revision.json']:
    records[p]=sha(ROOT/p)
for name in ['Hall','HallTall']:
    original=live/(name+'.png');source=ROOT/'ArtSource/MenuUI/native'/(name+'.png')
    assert original.read_bytes()==source.read_bytes(),name
    for path in [original,source.with_suffix('.aseprite')]:shutil.copyfile(path,HERE/'before'/path.name)
assert records['Docs/ArtDirection/Audit2026-10/media/skeleton-still.png']=='705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c'
(HERE/'baseline.json').write_text(json.dumps(records,indent=2)+'\n')
im=Image.open(live/'HallTall.png').convert('RGBA')
assert im.size==(360,800)
assert im.crop((0,0,360,640)).tobytes()==Image.open(live/'Hall.png').convert('RGBA').tobytes()
print('Current source',im.size,'opaque colors',len(Counter(im.getdata())),'alpha',sorted(set(im.getchannel('A').getdata())))
print('Most used colors',[(tuple(c),n) for c,n in Counter(im.getdata()).most_common(35)])
grid=im.resize((720,1600),Image.Resampling.NEAREST);draw=ImageDraw.Draw(grid)
for y in range(0,800,20):draw.line((0,y*2,720,y*2),fill='#848484',width=1);draw.text((0,y*2),str(y),fill='white')
for x in range(0,360,20):draw.line((x*2,0,x*2,1600),fill='#848484',width=1);draw.text((x*2,0),str(x),fill='white')
grid.save(HERE/'review/CoordinateStudy.png')
print('Protected',len(records),'existing files; source copies preserved.')
