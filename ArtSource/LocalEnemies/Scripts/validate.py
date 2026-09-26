"""Read-only source/export, timing, palette and anchor checks."""
from pathlib import Path
import json, sys, struct
from PIL import Image
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / 'CombatIdles/Scripts'))
from inspect_idles import read_ase, color_counts
base = ROOT / 'Review' if '--review' in sys.argv else ROOT
palettes = json.loads((ROOT / 'Scripts/palettes.json').read_text())
report = {}
for name in ['Miner_Idle','Miner_AutoAttack','Miner_Ability','BasketVillager_Idle',
             'BasketVillager_AutoAttack','BarricadeVillager_Idle',
             'BarricadeVillager_AutoAttack','BarricadeVillager_Ability']:
    character, action = name.split('_')
    expected = [130]*9 if action == 'Idle' else [80,80,120,40,120,80,80,80] if action == 'AutoAttack' else [80,80,120,80,120,80,80,80,80,80]
    frames, durations = read_ase(base / (name+'.aseprite'))
    assert durations == expected, (name, durations)
    w, h = frames[0].size
    png = Image.open(base / (name+'.png')).convert('RGBA')
    gif = Image.open(base / (name+'.gif'))
    assert png.size == (w*len(frames),h) and gif.n_frames == len(frames)
    metadata = json.loads((base/(name+'.json')).read_text())
    assert [f['duration'] for f in metadata['frames']] == durations
    for f, im in enumerate(frames):
        assert set(color_counts([im])) <= {'#'+c for c in palettes[character]}, (name,f,'palette')
        assert {p[3] for p in im.get_flattened_data()} <= {0,255}, (name,f,'alpha')
        assert im.size == (w,h)
        assert im.getbbox()[0] > 0 and im.getbbox()[2] < w, (name,f,'side clipping')
        if not (name == 'Miner_Ability' and f == 3):
            assert im.getbbox()[3] == h, (name,f,'ground contact')
        assert im.tobytes() == png.crop((f*w,0,(f+1)*w,h)).tobytes(), (name,f,'native export')
        gif.seek(f)
        assert gif.info['duration'] == durations[f]
        assert all((a==b if a[3] else b[3]==0) for a,b in zip(im.get_flattened_data(),gif.convert('RGBA').get_flattened_data())), (name,f,'GIF alpha/pixels')
    idle,_ = read_ase(base/(character+'_Idle.aseprite'))
    def ready(im):
        return im.crop((16,0,80,64)) if character != 'Miner' and action != 'Idle' else im
    assert ready(frames[0]).tobytes() == idle[0].tobytes()
    assert ready(frames[-1]).tobytes() == idle[0].tobytes()
    report[name] = dict(frames=len(frames),canvas=[w,h],durations_ms=durations,
        impact_frame=None if action=='Idle' else 5,
        impact_ms=None if action=='Idle' else 320 if action=='AutoAttack' else 360,
        colors=len(color_counts(frames)),bounds=[im.getbbox() for im in frames],
        exact_native_export=True,binary_alpha=True)
(base/'Validation.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS: 71 frames; native/PNG/GIF pixels and timing, approved palettes, clean alpha, fixed canvases and ready-pose continuity. Miner ability frame 4 intentionally airborne.')
