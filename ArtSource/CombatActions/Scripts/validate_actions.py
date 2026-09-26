"""Read-only validation of native frames, exports, palette and fixed grounding."""
from pathlib import Path
import sys, json, hashlib
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase, color_counts
palettes=json.loads((ROOT/'Scripts/palettes.json').read_text())
report={}
for name in ['Farmer_AutoAttack','PanVillager_AutoAttack','Rattlebones_Ability','Bardley_Ability']:
    character=name.split('_')[0];attack=name.endswith('AutoAttack')
    frames,ds=read_ase(ROOT/(name+'.aseprite'));w=96 if attack else 64
    expected=[80,80,120,40,120,80,80,80] if attack else [80,80,120,80,120,80,80,80,80,80]
    assert ds==expected,(name,ds)
    png=Image.open(ROOT/(name+'.png')).convert('RGBA')
    assert png.size==(w*len(frames),64)
    gif=Image.open(ROOT/(name+'.gif'))
    assert gif.n_frames==len(frames)
    metadata=json.loads((ROOT/(name+'.json')).read_text())
    assert [f['duration'] for f in metadata['frames']]==ds
    for i,im in enumerate(frames):
        assert im.size==(w,64)
        assert set(color_counts([im])) <= {'#'+c for c in palettes[character]},(name,i,'palette')
        assert {p[3] for p in im.getdata()} <= {0,255},(name,i,'alpha')
        assert im.getbbox()[3]==64,(name,i,'floor')
        assert im.crop((0,0,1,64)).getbbox() is None and im.crop((w-1,0,w,64)).getbbox() is None,(name,i,'clipped side')
        assert im.tobytes()==png.crop((i*w,0,(i+1)*w,64)).tobytes(),(name,i,'native export')
        gif.seek(i);assert gif.info['duration']==ds[i]
        decoded=gif.convert('RGBA')
        assert all((a==b if a[3] else b[3]==0) for a,b in zip(im.getdata(),decoded.getdata())),(name,i,'GIF pixels')
    idle,_=read_ase(ROOT.parent/'CombatIdles'/(character+'_Idle.aseprite'))
    for im in [frames[0],frames[-1]]:
        crop=im.crop((16,0,80,64)) if attack else im
        assert crop.tobytes()==idle[0].tobytes(),(name,'ready pose continuity')
    report[name]={'frames':len(frames),'canvas':[w,64],'durations_ms':ds,'total_ms':sum(ds),
        'impact_frame':5 if attack else None,'impact_ms':320 if attack else None,
        'colors':len(color_counts(frames)),'all_frames_grounded':True,'native_export_exact':True}
farmer,fd=read_ase(ROOT.parent/'CombatIdles/Farmer_Idle.aseprite')
original,od=read_ase(ROOT/'Originals/Farmer_Idle.aseprite')
assert fd==od and all(a.tobytes()==b.tobytes() for a,b in zip(farmer,original)), 'User Farmer idle changed'
bard,bd=read_ase(ROOT.parent/'CombatIdles/Bardley_Idle.aseprite')
original,od=read_ase(ROOT/'Originals/Bardley_Idle_BeforeGrounding.aseprite')
assert bd==od
for a,b in zip(bard,original):
    assert b.crop((0,52,64,64)).getbbox() is None
    assert a.crop((0,0,64,12)).getbbox() is None
    assert a.crop((0,12,64,64)).tobytes()==b.crop((0,0,64,52)).tobytes(), 'Bard idle changed beyond translation'
report['idle_preservation']={'Farmer':'exact user pixels and timing','Bardley':'exact 12px downward translation, no crop loss'}
(ROOT/'Validation.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS: all 36 action frames, approved palettes, binary alpha, complete margins, floor contact, native sheets/GIF timing, ready-pose continuity, exact Farmer idle and Bardley translation.')
