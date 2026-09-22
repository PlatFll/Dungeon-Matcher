"""Read-only pixel/export checks for the selected enemy idle family."""
from pathlib import Path
import sys,json,hashlib
from PIL import Image
ROOT=Path(__file__).resolve().parents[1];ART=ROOT.parent;PROJECT=ART.parent
sys.path.insert(0,str(ART/'CombatIdles/Scripts'))
from inspect_idles import read_ase,color_counts
sys.path.insert(0,str(ROOT/'Scripts'))
from validate_geometry import check_parts
LOCALS=['Miner','BasketVillager','BarricadeVillager']
GUARDS=['CrossbowGuard','BarricadeGuard','SpearGuard','SiegeSergeant']
REMAINING=['TownMarshal','SwordKnight','SpearKnight','ShieldKnight','KnightCaptain','RoyalSwordsman','RoyalLancer','RoyalArbalist','RoyalStandardBearer','RoyalArcanist','RoyalMage','King']
report={}
for name in LOCALS+GUARDS+REMAINING:
    folder=ART/'LocalEnemies' if name in LOCALS else ART/'GuardIdles' if name in GUARDS else ROOT/'SelectedIdles'
    path=folder/(name+'_Idle.aseprite');frames,ms=read_ase(path)
    ready_path=ART/'LocalEnemies/Originals/BeforeGroundedFamily'/path.name if name in LOCALS else ROOT/'Recolored'/(name+'.aseprite')
    ready=read_ase(ready_path)[0][0]
    size=(96,80) if name=='Miner' else (64,64);w,h=size
    allowed=json.loads((ART/'LocalEnemies/Scripts/palettes.json' if name in LOCALS else ROOT/'Palettes.json').read_text())[name]
    assert len(frames)==9 and ms==[130]*9,(name,'timing')
    assert frames[0].tobytes()==ready.tobytes()==frames[-1].tobytes(),(name,'ready pose')
    assert {c.lstrip('#') for c in color_counts(frames)}<={c.lstrip('#') for c in allowed},(name,'palette')
    png=Image.open(path.with_suffix('.png')).convert('RGBA');gif=Image.open(path.with_suffix('.gif'))
    assert png.size==(w*9,h) and gif.n_frames==9,(name,'export dimensions')
    metadata=json.loads(path.with_suffix('.json').read_text())
    assert [f['duration']for f in metadata['frames']]==ms
    for f,im in enumerate(frames):
        assert im.size==size and set(im.getchannel('A').get_flattened_data())<={0,255}
        assert im.getbbox()[3]==h,(name,f,'ground bound')
        gif.seek(f);assert gif.info['duration']==130
        for other in [png.crop((f*w,0,(f+1)*w,h)),gif.convert('RGBA')]:
            assert all(a==b if a[3]else b[3]==0 for a,b in zip(im.get_flattened_data(),other.get_flattened_data())),(name,f,'export pixels')
    geometry={}
    if name in REMAINING or name=='CrossbowGuard':geometry=check_parts(name,frames)
    if name in LOCALS:
        x0,y0,x1={'Miner':(38,77,62),'BasketVillager':(26,61,43),'BarricadeVillager':(25,61,44)}[name]
        for f,im in enumerate(frames):
            for y in range(y0,h):
                for x in range(x0,x1+1):
                    assert im.getpixel((x,y))==ready.getpixel((x,y)),(name,f,'anchored boots',x,y)
        geometry['boots_fixed']=True
    if name in GUARDS and name!='CrossbowGuard':
        original=ROOT/'BeforeCorrection/Idles'/path.name
        assert original.read_bytes()==path.read_bytes(),(name,'approved guard changed')
    unity=PROJECT/'Assets/_Game/Art/CombatIdles'/path.with_suffix('.png').name
    if '--unity' in sys.argv:assert unity.read_bytes()==path.with_suffix('.png').read_bytes(),(name,'Unity bytes')
    report[name]={'source':str(path.relative_to(PROJECT)).replace('\\','/'),'frames':9,'duration_ms':130,'canvas':size,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'geometry':geometry,'unity_checked':'--unity'in sys.argv}
(ROOT/'FamilyVerification.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS: 19 idles, 171 native/PNG/GIF frames; ready poses, palettes, binary alpha, 130ms exposures, fixed feet and rigid exposed cast props. Unity checked:', '--unity' in sys.argv)
