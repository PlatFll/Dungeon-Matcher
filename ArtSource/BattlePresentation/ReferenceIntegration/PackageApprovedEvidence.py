"""Preserve the combined, executed Unity run without replacing historical evidence."""
import argparse, hashlib, json, re, shutil
import xml.etree.ElementTree as ET
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
parser = argparse.ArgumentParser()
parser.add_argument('--tests', type=Path, required=True)
parser.add_argument('--validator-log', type=Path, required=True)
parser.add_argument('--art-tests', type=Path, required=True)
args = parser.parse_args()
run = ET.parse(args.tests).getroot()
assert run.attrib['result'] == 'Passed'
assert run.attrib['total'] == run.attrib['passed'] == '44'
assert run.attrib['failed'] == run.attrib['skipped'] == '0'
art_run=ET.parse(args.art_tests).getroot()
assert art_run.attrib['result']=='Passed' and art_run.attrib['total']==art_run.attrib['passed']=='4'
assert art_run.attrib['failed']==art_run.attrib['skipped']=='0'
log = args.validator_log.read_text(encoding='utf-8', errors='replace')
assert 'Application will terminate with return code 0' in log
assert not re.search(r'error CS\d{4}|Scripts have compiler errors|Compilation failed|Aborting batchmode due to failure', log)
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
evidence = HERE/'evidence'
shutil.copyfile(args.tests, evidence/'approved-tests.xml')
shutil.copyfile(args.tests.with_suffix('.log'), evidence/'approved-tests.log')
shutil.copyfile(args.art_tests, evidence/'approved-art-tests.xml')
shutil.copyfile(args.art_tests.with_suffix('.log'), evidence/'approved-art-tests.log')
shutil.copyfile(args.validator_log, evidence/'approved-unity-validation.log')
captures = ROOT/'.utmp/ThreeZonePresentation/after'
names = dict(dungeon='Dungeon', **{'magical-forest':'Forest', 'drowned-court':'Court'})
copied = []
for zone, title in names.items():
    for suffix, target in [('1080','1080'),('720x1280','720'),('1080x2400','Tall')]:
        dest = HERE/(title+'ApprovedUnity'+target+'.png')
        shutil.copyfile(captures/(zone+'-'+suffix+'.png'),dest)
        copied.append(dest)
    for path in captures.glob(zone+'*-layout.txt'):
        (evidence/('approved-'+path.name)).write_text(path.read_text().rstrip()+'\n')
source_paths = [ROOT/'Assets/_Game/Scripts'/p for p in (
    'UI/BattlePresentationSkin.cs','UI/DungeonPresentationArt.cs','UI/ReferenceKitSurface.cs',
    'UI/PlayerAreaThreeSliceFrameController.cs','UI/GameplayPixelLayoutValidator.cs',
    'UI/WaveTrackerUI.cs','UI/RunControlsUI.cs','Board/BoardVisuals.cs','Editor/ThreeZonePresentationTests.cs',
    'Editor/BattlePresentationImporter.cs')]
record = dict(status='Passed', userApprovalDate='2026-10-17', generationCalls=0,
    unityVersion='6000.3.19f1', audioMuted=True, tests=run.attrib, finalArtRetest=art_run.attrib,
    validator=dict(exitCode=0,originalLog=str(args.validator_log),sha256=sha(args.validator_log)),
    sources={p.relative_to(ROOT).as_posix():sha(p) for p in source_paths},
    captures={p.name:sha(p) for p in copied},
    limits=['No physical-device validation','Hidden source surfaces reconstructed',
            'Point-filtered responsive fitting can produce uneven physical pixel widths'])
(evidence/'approved-merge.json').write_text(json.dumps(record,indent=2)+'\n')
gallery=''.join(f'<section><h2>{title}</h2><img src="{title}ApprovedUnity1080.png"><p><a href="{title}ApprovedUnity720.png">720×1280</a> · <a href="{title}ApprovedUnityTall.png">1080×2400 safe-area capture</a></p></section>' for title in names.values())
(HERE/'ApprovedReview.html').write_text('''<!doctype html><meta charset="utf-8"><title>Approved zone integration</title>
<style>body{background:#17131f;color:#e7deef;font:16px system-ui;margin:24px}a{color:#c4aaff}main{display:flex;gap:24px;flex-wrap:wrap}img{width:360px;max-width:100%;image-rendering:pixelated}</style>
<h1>Dungeon, Forest and Drowned Court — actual Unity captures</h1>
<p>Approved native modules integrated. Shared controls, live characters and gems. Validation ran with audio muted.</p><main>'''+gallery+'</main><a href="evidence/approved-merge.json">Executed validation</a>',encoding='utf-8')
print('Packaged 44 passing tests and nine actual Unity captures.')
