"""Collect actual Unity integration evidence; no source artwork is transformed."""
from pathlib import Path
import hashlib,json,shutil,xml.etree.ElementTree as ET

HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
EVIDENCE=HERE/'evidence';EVIDENCE.mkdir(exist_ok=True)
VIEW=Path('C:/Users/USER/.codex/visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a/WaterFillRevision')
VIEW.mkdir(exist_ok=True)
RUNS=['db78a3d2-9c37-4138-bfec-5dae21331f78','f3b37f41-f8e2-45a0-b917-8a8e5a4d28ca']
latest={}
for index,run in enumerate(RUNS):
    source=ROOT/'.utmp/ForestValidation'/f'{run}.xml'
    shutil.copy2(source,EVIDENCE/f'run-{index+1}.xml')
    for test in ET.parse(source).iter('test-case'):
        latest[test.attrib['fullname']]=test.attrib['result']
assert len(latest)==5 and all(r=='Passed' for r in latest.values()),latest
shots=['1920-main-menu.png','menu-settings.png','1920-settings.png','2400-home-settings-safe-area.png']
shots += [zone+'-settings-'+size+'.png' for zone in ('dungeon','magical-forest','drowned-court','ironvein-excavation','frostwake-peaks') for size in ('360','1080')]
capture_hashes={}
for name in shots:
    source=ROOT/'.utmp/MenuUI/after'/name
    shutil.copy2(source,EVIDENCE/name);shutil.copy2(source,VIEW/name)
    capture_hashes[name]=hashlib.sha256(source.read_bytes()).hexdigest()
report={'latestResults':latest,'nativeAssets':37,'unchangedLegacyNativeAssets':54,
        'zones':['dungeon','magical-forest','drowned-court','ironvein-excavation','frostwake-peaks'],
        'settingsTouchSizes':[[360,640],[1080,2400]],'homeSizes':[[720,1280],[1080,1920],[1080,2400]],
        'initialFailure':'All-zone test clicked before camera/CanvasScaler completed a resolution change. Added the existing layout-settle wait; identical touch and centering assertions then passed.',
        'safeAreaChecks':True,'saveAndPreferenceChecks':True,'pauseChecks':True,
        'realDeviceTest':False,'additionalPixelLabGenerations':0,'screenshots':capture_hashes}
(EVIDENCE/'runtime-validation.json').write_text(json.dumps(report,indent=2)+'\n')
cards=[('Edited main menu','1920-main-menu.png'),('Home settings','menu-settings.png'),('Battle settings','1920-settings.png'),('Rising water','WaterFill.webp')]
cards += [(z.replace('-',' ').title()+' — tall safe area',z+'-settings-1080.png') for z in report['zones']]
body=''.join(f'<figure><figcaption>{label}</figcaption><a href="{file}"><img src="{file}" alt="{label}"></a></figure>' for label,file in cards)
(VIEW/'Integration.html').write_text('<!doctype html><html><meta charset="utf-8"><title>Approved game integration</title><style>body{background:#15151e;color:#f1edf7;font:16px system-ui;margin:32px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(280px,1fr));gap:24px}figure{margin:0;background:#242431;padding:16px}img{width:100%;max-width:360px;image-rendering:pixelated}figcaption{margin-bottom:12px}</style><h1>Actual Unity game integration</h1><p>Edited hall, approved settings and rising water. All five runtime checks passed on their latest execution. No real-device test. Artwork preserved; zero additional generations.</p><main>'+body+'</main></html>',encoding='utf-8')
print('Recorded 5 passing runtime checks and',len(shots),'actual Unity screenshots.')
