"""Read-only image checks. Writes measurements, never paints/resamples source art."""
from pathlib import Path
from collections import Counter
import hashlib
import json
import sys

from PIL import Image

PROJECT = Path(__file__).resolve().parents[3]
ROOT = PROJECT / 'ArtSource/Backgrounds'
sys.path.insert(0, str(PROJECT / 'ArtSource/CombatIdles/Scripts'))
from inspect_idles import read_ase

records = {}
for native in sorted(ROOT.rglob('*.aseprite')):
    png = native.with_suffix('.png')
    frames, durations = read_ase(native)
    image = Image.open(png).convert('RGBA')
    assert len(frames) == 1 and frames[0].size == image.size
    assert frames[0].tobytes() == image.tobytes(), native
    alpha = sorted(set(image.getchannel('A').getdata()))
    assert set(alpha) <= {0, 255}, (native, alpha)
    colors = Counter('#%02X%02X%02X' % pixel[:3] for pixel in image.getdata() if pixel[3])
    records[str(native.relative_to(ROOT)).replace('\\', '/')] = {
        'size': image.size, 'alpha': alpha, 'palette': dict(sorted(colors.items())),
        'native_sha256': hashlib.sha256(native.read_bytes()).hexdigest(),
        'png_sha256': hashlib.sha256(png.read_bytes()).hexdigest(),
        'native_png_exact': True,
    }

assert len(records) == 23, len(records)
scene = Image.open(ROOT / 'BattlegroundScene.png').convert('RGBA')
assert scene.size == (512, 384)
for name, rect in [('BattlegroundWall', (0, 0, 512, 256)),
                   ('BattlegroundFloor', (0, 256, 512, 320))]:
    assert scene.crop(rect).tobytes() == Image.open(ROOT / (name + '.png')).convert('RGBA').tobytes()
foundation = Image.open(ROOT / 'Modules/Foundation.png').convert('RGBA')
for x in range(0, 512, 64):
    assert scene.crop((x, 320, x + 64, 384)).tobytes() == foundation.tobytes()

floor = Image.open(ROOT / 'BattlegroundFloor.png').convert('RGBA')
for x in range(64, 512, 64):
    assert floor.crop((x-1, 0, x, 64)).tobytes() == floor.crop((x, 0, x+1, 64)).tobytes()

general = Image.open(ROOT / 'GeneralMasonry.png').convert('RGBA')
preview = Image.open(ROOT / 'GeneralBackgroundPreview.png').convert('RGBA')
for y in range(0, 512, 128):
    for x in range(0, 384, 128):
        assert preview.crop((x, y, x+128, y+128)).tobytes() == general.tobytes()

imports = {
    'BattlegroundWall.png': 'Assets/_Game/Art/Backgrounds/BattleArea/BackgroundRefinement/BattlegroundWall.png',
    'BattlegroundFloor.png': 'Assets/_Game/Art/Backgrounds/BattleArea/BackgroundRefinement/BattlegroundFloor.png',
    'Modules/Foundation.png': 'Assets/_Game/Art/Backgrounds/BattleArea/BackgroundRefinement/Foundation.png',
    'GeneralMasonry.png': 'Assets/_Game/Resources/UI/DungeonPresentation/DungeonBackdropTile.png',
}
for source, imported in imports.items():
    assert (ROOT / source).read_bytes() == (PROJECT / imported).read_bytes(), imported

output = ROOT / 'Validation/AssetManifest.json'
output.write_text(json.dumps(records, indent=2) + '\n', encoding='utf-8')
print('PASS: 23 native/PNG pairs, binary alpha, exact assembled scene and tiled surround, floor joins, four exact Unity copies.')
