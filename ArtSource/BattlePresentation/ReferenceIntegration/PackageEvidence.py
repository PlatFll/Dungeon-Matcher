"""Package executed Unity evidence; never synthesizes runtime screenshots."""
import argparse, hashlib, json, re, shutil, subprocess
import xml.etree.ElementTree as ET
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
parser = argparse.ArgumentParser()
parser.add_argument('--tests', type=Path, required=True)
parser.add_argument('--validator-log', type=Path, required=True)
args = parser.parse_args()
run = ET.parse(args.tests).getroot()
assert run.attrib['result'] == 'Passed' and run.attrib['total'] == '40'
assert run.attrib['failed'] == run.attrib['skipped'] == '0'
log = args.validator_log.read_text(encoding='utf-8', errors='replace')
assert 'Application will terminate with return code 0' in log
assert not re.search(r'error CS\d{4}|Scripts have compiler errors|Compilation failed|Aborting batchmode due to failure', log)
history = json.loads((HERE.parent/'DungeonReferenceKit/provenance.json').read_text())
for path, expected in history['protectedLocalFiles'].items():
    assert sha(ROOT/path) == expected, path

captures = ROOT/'.utmp/ThreeZonePresentation/after'
for source, target in [('dungeon-1080.png','DungeonUnity.png'),
                       ('dungeon-720x1280.png','DungeonUnity720.png'),
                       ('dungeon-1080x2400.png','DungeonUnityTall.png')]:
    shutil.copyfile(captures/source, HERE/target)
evidence = HERE/'evidence'
evidence.mkdir(exist_ok=True)
shutil.copyfile(args.tests, evidence/'tests.xml')
shutil.copyfile(args.validator_log, evidence/'unity-validation.log')
shutil.copyfile(args.tests.with_suffix('.log'), evidence/'tests.log')
for path in sorted(captures.glob('*-layout.txt')):
    (evidence/path.name).write_text(path.read_text().rstrip()+'\n')

changed = subprocess.check_output(['rtk','proxy','git','diff','HEAD','--name-only','--','Assets/_Game/Scripts'], cwd=ROOT, text=True).splitlines()
changed += ['Assets/_Game/Scripts/UI/ReferenceKitSurface.cs']
sources = {p: sha(ROOT/p) for p in sorted(set(changed))}
production = ROOT/'Assets/_Game/Resources/UI/BattlePresentation/ReferenceDungeon'
assets = {p.name: sha(p) for p in sorted(production.glob('*.png'))}
assert len(assets) == 36
for name, value in assets.items():
    assert value == sha(HERE.parent/'DungeonReferenceKit/native'/name), name
for zone in ['Forest','Court']:
    checks = json.loads((HERE.parent/(zone+'ReferenceKit')/'checks.json').read_text())
    assert checks['assemblyMatchesModules'] and checks['sharedControlsPixelIdentical']
    assert len(checks['assets']) == 27

record = dict(status='Passed; Forest and Court source candidates await visual review',
    generationCalls=0, unityVersion='6000.3.19f1', audioMuted=True,
    commands=['Tools/Test-ReferencePresentation.ps1', 'Tools/Validate-Unity.ps1', 'ReviewReferenceKits.py'],
    tests=dict(run.attrib), validator=dict(result='Passed', exitCode=0, originalLog=str(args.validator_log), sha256=sha(args.validator_log)),
    testedSources=sources, approvedDungeonImports=assets,
    evidence={str(p.relative_to(HERE)):sha(p) for p in sorted(evidence.iterdir())},
    captures={p.name:sha(p) for p in sorted(HERE.glob('DungeonUnity*.png'))},
    protectedLocalFiles=history['protectedLocalFiles'],
    limits=['No physical-device testing', 'New Forest/Court scenery is source-only',
            'Hidden surfaces are reconstructed', 'Dungeon rim opening fitted to the unchanged square board',
            'Point-filtered registered scenery may use fractional responsive scaling'])
(HERE/'validation.json').write_text(json.dumps(record, indent=2)+'\n')
(HERE/'Review.html').write_text('''<!doctype html><meta charset="utf-8"><title>Dungeon integration and zone kit review</title>
<style>body{background:#17131f;color:#e7deef;font:16px system-ui;margin:24px;max-width:1120px}a{color:#c4aaff}img{max-width:100%;image-rendering:pixelated}.runtime{width:470px}nav{display:flex;gap:24px;flex-wrap:wrap}</style>
<h1>Dungeon integration · Forest and Underwater review</h1>
<p>Dungeon is implemented from the approved modules. Forest and Underwater are new source kits for visual approval.</p>
<nav><a href="../ForestReferenceKit/Review.html">Forest: comparison, overlay and native assets</a><a href="../CourtReferenceKit/Review.html">Underwater: comparison, overlay and native assets</a><a href="validation.json">Validation record</a></nav>
<h2>Actual Dungeon Unity capture</h2><p>Live characters, gems, counters and controls. 40 tests passed; Unity validation passed. Audio muted.</p>
<a href="DungeonUnity.png"><img class="runtime" src="DungeonUnity.png" alt="Actual 1080 by 1920 Unity capture"></a>
<p><a href="DungeonUnity720.png">720 × 1280</a> · <a href="DungeonUnityTall.png">1080 × 2400 with safe-area test</a></p>
<h2>Forest source comparison</h2><a href="../ForestReferenceKit/Review.html"><img src="../ForestReferenceKit/review/Comparison.png" alt="Forest approved target and rebuilt candidate"></a>
<h2>Underwater source comparison</h2><a href="../CourtReferenceKit/Review.html"><img src="../CourtReferenceKit/review/Comparison.png" alt="Underwater approved target and rebuilt candidate"></a>
<p>Hidden surfaces use reconstructed source materials. Court hidden paving is more regular than the exposed reference stones. These are declared differences, not claims of exact recovery. Zero generations.</p>''', encoding='utf-8')
print('Packaged actual Unity captures, 40 passed tests, validator log, source hashes and protected-file checks.')
