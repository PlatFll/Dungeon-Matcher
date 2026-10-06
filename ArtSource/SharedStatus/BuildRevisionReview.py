"""Package existing Unity captures and test records; never redraw production art."""
from pathlib import Path
import base64
import hashlib
import html
import json
import shutil
import xml.etree.ElementTree as ET
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).parent / 'Review'
OUT.mkdir(exist_ok=True)
EVIDENCE = OUT / 'Evidence'
EVIDENCE.mkdir(exist_ok=True)
RUNS = [
    ('Broad affected regression', 'a47a252f-85c8-472b-a443-b3fbae0a372e'),
    ('Forecast freshness and affected live cases', 'd5528f07-eb52-487f-9b73-54ea13ce727d'),
    ('Final target readability and lifecycle cases', '534ed03f-b278-481c-8277-ae7175e84631'),
]
latest, runs = {}, []
for label, key in RUNS:
    source = ROOT / '.utmp/ForestValidation' / (key + '.xml')
    tree = ET.parse(source).getroot()
    cases = list(tree.iter('test-case'))
    assert cases and all(c.get('result') == 'Passed' for c in cases), key
    for case in cases:
        latest[case.get('fullname')] = {'result': case.get('result'), 'run': key}
    shutil.copyfile(source, EVIDENCE / source.name)
    runs.append({'label': label, 'xml': 'Evidence/' + source.name,
                 'passed': len(cases), 'sha256': hashlib.sha256(source.read_bytes()).hexdigest()})

captures = [
    ('sigils-720x1280.png', '720 × 1280'),
    ('sigils-1080x1920.png', '1080 × 1920'),
    ('sigils-1080x2400.png', '1080 × 2400'),
    ('sigils-1080x1920-safe.png', '1080 × 1920 with safe inset'),
    ('slippery-preview.png', 'Flooded Slippery: three-piece forecast'),
    ('cast-announcement.png', 'Successful Siphon announcement'),
]
records, figures = [], []
for filename, label in captures:
    source = ROOT / '.utmp/StatusRevision/Visual' / filename
    data = source.read_bytes()
    with Image.open(source) as im:
        width, height = im.size
    records.append({'file': filename, 'width': width, 'height': height,
                    'sha256': hashlib.sha256(data).hexdigest()})
    uri = 'data:image/png;base64,' + base64.b64encode(data).decode()
    figures.append(f'<figure><figcaption>{html.escape(label)}</figcaption>'
                   f'<a href="{uri}" target="_blank"><img src="{uri}" alt="{html.escape(label)}"></a></figure>')
    layout = source.with_suffix('.txt')
    if layout.exists():
        shutil.copyfile(layout, EVIDENCE / layout.name)

native = []
for filename in ['NativeIcons.png', 'NativeSigils.png']:
    data = (Path(__file__).parent / filename).read_bytes()
    uri = 'data:image/png;base64,' + base64.b64encode(data).decode()
    native.append(f'<div class="native"><p>{filename}: 1:1 pixels</p><img src="{uri}" alt="{filename}"></div>')

glyphs = []
for folder, size in [('PlayerStatuses', 16), ('CasterSigils', 12)]:
    for source in sorted((ROOT / 'Assets/_Game/Resources/UI' / folder).glob('*.png')):
        with Image.open(source) as im:
            rgba = im.convert('RGBA')
            pixels = list(rgba.get_flattened_data())
            alpha = sorted({p[3] for p in pixels})
            assert im.size == (size, size) and alpha == [0, 255], source
            glyphs.append({'file': source.relative_to(ROOT).as_posix(), 'width': size,
                           'height': size, 'alpha': alpha,
                           'opaquePalette': len({p[:3] for p in pixels if p[3]}),
                           'sha256': hashlib.sha256(source.read_bytes()).hexdigest()})
summary = {'uniquePassingCases': len(latest), 'runs': runs, 'captures': records, 'nativeGlyphs': glyphs,
           'latestResultByCase': dict(sorted(latest.items())),
           'limits': 'Affected coverage only. Historical unrelated failures remain; no physical Android or human pacing claim.'}
(OUT / 'Evidence.json').write_text(json.dumps(summary, indent=2) + '\n', encoding='utf-8')
page = '''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width">
<title>Dungeon Matcher — status and caster revision</title>
<style>body{margin:0;background:#171b22;color:#e8e9e3;font:16px/1.5 system-ui}main{max-width:1400px;margin:auto;padding:28px}
h1{font-size:28px}a{color:#a3d6df}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(290px,1fr));gap:20px}
figure{margin:0;background:#252c34;padding:12px;border-radius:8px}figcaption{margin-bottom:12px}figure img{width:100%;height:auto;image-rendering:pixelated}
.native{background:#545454;padding:14px;display:inline-block;margin:8px 12px 20px 0}.native img{image-rendering:pixelated}
.note{max-width:1000px}code{color:#badbd5}</style><main>
<h1>Status and caster revision</h1><p class="note">Actual Unity captures. All seven statuses and deliberate overlapping warning sources are enabled in the portrait fixture for stress testing. New statuses remain unassigned in production. Editor audio was muted.</p>
<p>WHO: triangle / square / ring. WHAT: cast name and inspection. WHEN: existing countdowns and lane telegraphs.</p>
<p><a href="../../DrownedCourt/TailRevision/Review/Review.html">Shark-tail before/after and every motion</a> · <a href="Evidence.json">Capture hashes and test case evidence</a></p>
<h2>Native glyphs</h2>''' + ''.join(native) + '<h2>Runtime captures</h2><div class="grid">' + ''.join(figures) + '</div>'
page += f'<h2>Validation</h2><p>{len(latest)} unique passing cases across the broad regression and its affected reruns. '
page += 'The required Unity validator passed with 6000.3.19f1. Historical unrelated failures, physical Android testing and human pacing remain separate.</p><ul>'
page += ''.join(f'<li><a href="{r["xml"]}">{html.escape(r["label"])}: {r["passed"]} passed</a></li>' for r in runs)
page += '</ul><p>2026-10-06 · Review only; no merge.</p></main></html>'
(OUT / 'Review.html').write_text(page, encoding='utf-8')
print(json.dumps({'uniquePassingCases': len(latest), 'runs': [r['passed'] for r in runs], 'captures': records}, indent=2))
