from pathlib import Path
from PIL import Image
import json,hashlib,shutil,sys,xml.etree.ElementTree as ET
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint02';w=Path(__file__).parent
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
baseline=json.loads((o/'Diagnostics/ProtectedHashes.json').read_text())
changed=[k for k,h in baseline.items() if not(p/k).exists() or sha(p/k)!=h];assert not changed,changed
xml=ET.parse(o/'Diagnostics/UnityTests.xml').getroot();assert xml.attrib['result']=='Passed' and xml.attrib['failed']=='0'
native=json.loads((o/'Diagnostics/NativeValidation.json').read_text());pixels=json.loads((o/'Diagnostics/PixelValidation.json').read_text())
log=Path('C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-fd0dc628-5e9c-4ed5-96fc-07553c89efbc.log')
assert log.exists();shutil.copy2(log,o/'Diagnostics/Compilation.log')
report={'status':'Checkpoint 2 top-background blockout; user visual review pending','protected_files_unchanged':len(baseline),'unity_explicit_tests':{'passed':1,'failed':0,'process_exit':0},'compilation_validator':{'script':'Tools/Validate-Unity.ps1','exit':0,'log':str(log)},'pixel_checks':pixels,'native_sources':native,'pixellab':{'jobs':1,'charged_generations':9,'job_id':'e73e2738-f93c-4ade-91bc-b33efab095b5','asset_id':'013f4596-64e3-513e-b978-7d9d95651a5b','actual_style_reference':'Rattlebones skeleton-still.png, verified SHA-256 in ReferenceHashes.json'},'diagnostics_history':['First attempt incorrectly treated intentional portrait backing overfill as an unmasked extent; check corrected to measure clipped repeat surfaces, with an additional pixel mask-spill probe.','Second attempt passed the test and wrote all captures, but process exited -1073741819 during shutdown. XML and log retained. Unchanged rerun passed and exited 0.','Comparison baseline excludes 872 pixels of former board caps/ivy extending into rows458..479; no character or UI art moved. Transparent UI holes correctly reveal the revised background.'],'not_performed':['production combat','Android build','real-device testing'],'stop':'Checkpoint 2 complete; next review gate is portrait frame. Board-frame assets remain deferred to checkpoint4. No merge.'}
(o/'REPORT.json').write_text(json.dumps(report,indent=2))
table='\n'.join('| '+x['name']+' | '+str(x['dimensions'][0])+'×'+str(x['dimensions'][1])+' | '+str(x['opaque_colors'])+' |' for x in native)
readme='''# Magical Forest v2 — checkpoint 2: top background

**Status: Unity art-only blockout ready for visual review. No production integration or merge.**
The user authorized execution after the checkpoint-one direction study. This is the
next review stop, not permission to skip the portrait and board-frame checkpoints.

## Result

The distant woodland now has a clear old stone bridge, a narrow waterfall opening,
and darker layered trees. Its quiet lower third separates the unchanged fighters.
Two small far wisps provide restrained magical accents. The hanging rune plaque
is excluded from this preview; the two existing upper lanterns stay on their tree
supports. No props or extra detail were added to the standing floor.

The existing large trees, foreground canopy, flat ground, character/gem artwork,
portrait and shared controls remain untouched. The lower HUD and all previous
Forest scenes/source modules are unchanged. Only the upper review area is shown;
the board and lower HUD are not rebuilt at this checkpoint.

## Brief asset list

| New separate module | Native size | Opaque colors |
|---|---:|---:|
'''+table+'''

`Far_EnchantedSanctuary` is a distant support layer, not a flattened game screen.
The three depth repeats are scoped palette variants of existing support tiles,
preserving their dimensions, shapes and masks. `Distant_Wisp` has a padded binary
alpha canvas and four opaque colors; it is placed twice above the fighters.

PixelLab: **one source, 9 charged generations**. The actual verified Rattlebones PNG
was supplied as the rendering reference. Request, reference hash, job, receipt and
raw source remain in `Raw/` and `Diagnostics/`. No other generation was requested.

LibreSprite: all five native modules were authored/exported as editable documents.
The far cleanup consolidates 684 pixels of tiny moss/rock/canopy clusters in explicit
regions, groups two bank planes, and adds restrained waterfall strokes. The raw
11-color palette remains 11 colors; no unnecessary global quantization or blur.
Context tiles use explicit material mappings. Native exports match the prepared
pixel plans and reopened LibreSprite documents exactly.

## Actual Unity preview

Scene: `Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint02.unity`.
Editor menu: **Dungeon Matcher / Art Preview / Forest rework v2 checkpoint 02**.
It reads the prior Stage5 upper assembly and adds/replaces only the background
support modules. It never saves the source scene and is excluded from build settings.
The unchanged Rattlebones and three Forest enemies are static readability samples,
with static HP/countdowns. This is not a live combat or phone-build claim.

- `Previews/Unity-Before-720x480.png`: previous background, captured in this same scene.
- `Previews/Unity-After-720x480.png`: revised background with cast and UI.
- `Previews/Unity-After-1440x960.png`: exact nearest-neighbor 2× capture.
- `Previews/Unity-Background-720x480.png`: cast/stat samples hidden for background review.
- `Previews/Unity-MaskProbe-720x480.png`: backing overfill visualization.

## Executed checks

`Tools/Validate-Unity.ps1` **passed** with Unity6000.3.19f1. The explicit
`ForestReworkV2Checkpoint02Tests.TopBackgroundReview` passed, final process exit0.
Every visible sprite is Point filtered at uniform integer scale and aligned to
whole pixels. Standalone sprites fit their canvas; repeating masked backing is
tested by visible extent and a separate spill probe. Text remains untruncated.

Independent PNG checks passed: exact2× rendering; 35,066 unchanged actor/HP/countdown
pixels; 36,236 unchanged opaque frame/control pixels; exact portrait interior and
standing-ground region; zero mask spill and zero exposed camera pixels. Transparent
frame holes correctly show the new forest. The baseline matches the previous full
capture above row458; its last22rows exclude872 pixels of old board decorations
which intruded into the former top crop. No sprite placement was changed to obtain
that match. All8,125 protected source/production files match their baseline hashes.

The first test's overfill assertion mistake and one subsequent Unity shutdown anomaly
are retained in diagnostics. The unchanged test rerun passed and exited cleanly.
No production gameplay, Android build, real-device, or adaptive full-screen layout
test was performed for this isolated art blockout.

## Review stop

This checkpoint establishes distant mood, focal depth and readable stage separation.
It does not approve final artwork automatically. The cleaner board direction from
checkpoint1 remains the target for checkpoint4; it has not been replaced by the old
decorated board. Stop for user review before checkpoint3, the portrait frame variant.
Do not merge.
'''
for before,after in [('Unity6000','Unity 6000'),('exit0','exit 0'),('exact2×','exact 2×'),('All8,125','All 8,125'),('row458','row 458'),('last22rows','last 22 rows'),('exclude872','exclude 872'),('checkpoint1','checkpoint 1'),('checkpoint4','checkpoint 4'),('checkpoint3','checkpoint 3')]:readme=readme.replace(before,after)
(o/'README.md').write_text(readme,encoding='utf-8')
cards=''.join('<figure><img src="Previews/'+x['name']+'.png" width="'+str(x['dimensions'][0])+'" height="'+str(x['dimensions'][1])+'"><figcaption>'+x['name']+'<br>'+str(x['dimensions'][0])+'×'+str(x['dimensions'][1])+' · '+str(x['opaque_colors'])+' colors</figcaption></figure>' for x in native)
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Magical Forest — Unity top-background review</title><style>*{box-sizing:border-box}body{margin:0;background:#101c22;color:#e2eade;font:16px/1.5 system-ui}main{max-width:1540px;margin:auto;padding:28px}h1{font-size:32px;margin:5px 0 12px}h2{font-size:22px;margin:28px 0 12px}p{color:#aec4bd;max-width:1050px}.badge{color:#b4cf92;font-size:12px;text-transform:uppercase;letter-spacing:1.5px}a{color:#aacee0}.row{display:grid;grid-template-columns:1fr 1fr;gap:22px}.panel{background:#17272c;border:1px solid #314b4d;border-radius:7px;padding:12px;overflow:auto}.pic{display:block;width:100%;height:auto;image-rendering:pixelated}.label{font-weight:600;margin-bottom:10px}.bar{display:flex;gap:10px;flex-wrap:wrap;margin:15px 0}button{background:#315044;border:1px solid #638476;color:#eff5ea;padding:9px 14px;border-radius:4px;cursor:pointer}.native .pic{width:720px;max-width:none}.assets{display:flex;gap:25px;flex-wrap:wrap;align-items:end}.assets img{image-rendering:pixelated;display:block}.assets figcaption{font-size:13px;color:#b7cbc2}figure{margin:10px}.result{border-left:3px solid #7a9e69;padding:8px 20px;margin-top:20px}.small{font-size:13px}table{border-collapse:collapse;width:100%}th,td{text-align:left;border-bottom:1px solid #34504e;padding:9px}@media(max-width:800px){.row{grid-template-columns:1fr}main{padding:15px}}</style><main><span class="badge">Checkpoint 2 · actual Unity art-only preview</span><h1>Depth first, decoration second</h1><p>A distant enchanted bridge and waterfall, broad tree layers and two quiet wisps. The flat fighting area, characters, portrait and shared controls retain their existing pixels and placement.</p><div class="bar"><button onclick="document.querySelector('#comparison').classList.remove('native')">Fit comparison</button><button onclick="document.querySelector('#comparison').classList.add('native')">Exact capture pixels</button><a href="Previews/Unity-After-1440x960.png">Open actual 2× Unity capture</a><a href="README.md">Asset list and verification</a></div><div class="row" id="comparison"><div class="panel"><div class="label">Before · same Unity review scene</div><img class="pic" src="Previews/Unity-Before-720x480.png"></div><div class="panel"><div class="label">Revised · actual Unity capture</div><img class="pic" src="Previews/Unity-After-720x480.png"></div></div><div class="result"><b>Kept unchanged</b><p>Large foreground trees and canopy, two upper lanterns, flat ground, Rattlebones and Forest cast, portrait frame, HP and countdowns, and shared controls. The lower HUD and all existing production files are untouched.</p><b>Rebuilt at this checkpoint</b><p>One far-forest support layer, three matching depth repeats, and one padded wisp used twice. The loose upper rune tag is omitted. Board and portrait frame rebuilds remain their own review checkpoints.</p></div><h2>Background without fighter samples</h2><div class="panel"><img class="pic" style="max-width:720px" src="Previews/Unity-Background-720x480.png"></div><h2>Native asset list · 1×</h2><div class="assets panel">'''+cards+'''</div><h2>PixelLab source and LibreSprite cleanup · 2×</h2><div class="row"><div class="panel"><div class="label">Raw PixelLab source</div><img src="Raw/FarForest_PixelLab.png" width="512" height="320" style="image-rendering:pixelated"></div><div class="panel"><div class="label">Cleaned native far layer</div><img src="Previews/Far_EnchantedSanctuary.png" width="512" height="320" style="image-rendering:pixelated"></div></div><h2>Verification</h2><p>Unity compilation passed. Explicit preview test passed and exited0. Exact2× capture equality, whole-pixel alignment, no clipped standalone sprites, zero portrait-mask spill, and8,125 unchanged protected files. The preview contains static readability samples, not live combat. No Android or real-device testing is claimed.</p><p class="small">PixelLab: one source,9 generations. LibreSprite: five editable native documents, exact export/reopen checks. A first-pass mask-extent test error and a later shutdown anomaly are documented; the final rerun passed cleanly.</p><p><a href="../Checkpoint01/Review.html">Earlier cleaner-board direction study</a> · Stop for review here; next checkpoint is the portrait frame. No merge.</p></main></html>'''
for before,after in [('exited0','exited 0'),('Exact2×','Exact 2×'),('and8,125','and 8,125'),('source,9','source, 9')]:html=html.replace(before,after)
(o/'Review.html').write_text(html,encoding='utf-8')
shutil.copy2(w/'RunForestV2C2.ps1',o/'Scripts/RunUnity.ps1');shutil.copy2(__file__,o/'Scripts/PublishReview.py')
# Strip Unity's incidental trailing whitespace only from files created in this pass.
paths=list((p/'Assets/_Game/Art/ForestReworkV2').rglob('*.meta'))+[p/'Assets/_Game/Art/ForestReworkV2.meta',p/'Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint02.unity',p/'Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint02.unity.meta',p/'Assets/_Game/Scripts/Editor/ForestReworkV2Checkpoint02Tests.cs.meta']
for path in paths:
 if path.exists():path.write_text('\n'.join(line.rstrip() for line in path.read_text().splitlines())+'\n')
print(json.dumps({'protected':len(baseline),'unity_tests':xml.attrib['result'],'new_modules':len(native),'charged_generations':9,'review':str(o/'Review.html')}))
