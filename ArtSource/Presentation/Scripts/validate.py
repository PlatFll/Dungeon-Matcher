"""Read-only native/export fidelity checks for the presentation pass."""
from pathlib import Path
import sys, json
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'CombatIdles/Scripts'))
from inspect_idles import read_ase, color_counts

report = {}
for source in sorted((ROOT / 'Presentation').glob('*.aseprite')):
    frames, durations = read_ase(source)
    exported = Image.open(source.with_suffix('.png')).convert('RGBA')
    assert len(frames) == 1 and frames[0].tobytes() == exported.tobytes(), source
    assert set(exported.getchannel('A').get_flattened_data()) <= {0, 255}, source
    report[source.stem] = {'canvas': exported.size, 'colors': len(color_counts(frames)), 'exact_native_export': True}

original_names = {'Explosion': 'Explosion3', 'PoisonExplosion': 'Poisonexplosion2',
                  'ShieldExplosion': 'ShieldExplosion', 'HealingExplosion': 'HealingExplosion'}
for name, original in original_names.items():
    path = ROOT / 'TileVfx' / name
    frames, durations = read_ase(path.with_suffix('.aseprite'))
    supplied, _ = read_ase(ROOT / 'TileVfx/Originals' / (original + '.ase'))
    assert len(frames) == 13 and durations == [30] * 13, name
    assert set(color_counts(frames)) <= set(color_counts(supplied)), name
    sheet = Image.open(path.with_suffix('.png')).convert('RGBA')
    metadata = json.loads(path.with_suffix('.json').read_text())
    assert sheet.size == (832, 64)
    assert [f['duration'] for f in metadata['frames']] == durations
    for index, frame in enumerate(frames):
        assert frame.size == (64,64)
        assert frame.tobytes() == sheet.crop((index*64,0,(index+1)*64,64)).tobytes(), (name,index)
        assert set(frame.getchannel('A').get_flattened_data()) <= {0,255}
        bounds = frame.getbbox()
        assert bounds and bounds[0] > 0 and bounds[1] > 0 and bounds[2] < 64 and bounds[3] < 64, (name,index,bounds)
    report[name] = {'canvas':[64,64], 'frames':13, 'total_ms':390, 'source_palette_only':True,
                    'exact_native_exports':True, 'clear_canvas_margin':True}

(ROOT / 'Presentation/Review/verification.json').write_text(json.dumps(report,indent=2))
print('PASS:', len(report), 'native artwork/VFX exports; exact pixels, palette, alpha, margins and timings.')
