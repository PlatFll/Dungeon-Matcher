from pathlib import Path
import hashlib,json,shutil,sys,xml.etree.ElementTree as ET
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint03';sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
baseline=json.loads((o/'Diagnostics/ProtectedHashes.json').read_text());changed=[f for f,h in baseline.items() if not(p/f).is_file() or sha(p/f)!=h]
assert not changed,changed
native=json.loads((o/'Diagnostics/NativeValidation.json').read_text());pixel=json.loads((o/'Diagnostics/PixelValidation.json').read_text())
xml=ET.parse(o/'Diagnostics/UnityTests.xml').getroot();assert xml.attrib['result']=='Passed' and xml.attrib['failed']=='0'
log=Path('C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-b3be2ed7-e5c4-42bd-a233-674f07ae11d0.log');shutil.copy2(log,o/'Diagnostics/Compilation.log')
report={'status':'Checkpoint 3 portrait candidate, awaiting user visual approval','protected_existing_files_unchanged':len(baseline),'assets':native,'unity':{'validator_exit':0,'validator_log':str(log),'test_result':xml.attrib['result'],'passed':int(xml.attrib['passed']),'failed':int(xml.attrib['failed']),'process_exit':int(sys.argv[2]),'captures':[[720,480],[1440,960]],'frame_sizes':[[30,30],[74,180],[98,220],[129,257],[163,287]],'undersized_29x29_shell_hidden':True},'pixel_checks':pixel,'pixellab':{'new_jobs':0,'new_generations':0,'reused_lineage':'ArtSource/ForestHybrid/Stage02/Sources/PixelLabJob.json','reason':'Existing native wood/ivy source and accepted direction study already define the motif. Direct pixel work gives exact reusable joints without generation.'},'unchanged':['approved checkpoint 2 forest/background and floor','portrait inset, pennant, Rattlebones, affinity gem and HP','shared top controls and complete lower HUD','all previous Forest modules and scenes','production Game scene and mechanics'],'limits':['Unity art-only preview with static character/stat samples; not live combat','responsive shell geometry tested; production safe areas/content reflow deferred','no Android or real-device test'],'next':'Stop for checkpoint 3 review. Board frame is checkpoint 4. No merge.'}
(o/'REPORT.json').write_text(json.dumps(report,indent=2))
table='\n'.join('| '+r['name']+' | '+str(r['size'][0])+' x '+str(r['size'][1])+' | '+str(r['opaque_colors'])+' |' for r in native['assets'])
(o/'README.md').write_text('''# Magical Forest v2 - checkpoint 3: portrait frame

Candidate ready for user visual review. No production integration or merge.
The user's Continue approved checkpoint 2 and authorized only the next review stop.

## Result

The existing thin portrait proportions remain. Quiet square mitred timber joints
replace the four small diamond finials. Native 32-pixel rail repeats use one broad
grain groove and top-left lighting; the right and bottom are shaded independently.
Two small joined leaf/vine silhouettes replace the detached leaf clusters. Foliage
is outside the mask, on its own padded canvases. One centered wooden tenon per
vertical side replaces three repeating studs at this portrait size.

The existing ResponsiveModularFrameFitter still owns corners, tiling and centered
connectors. No runtime frame system was added. The 98x220 native target, 12-pixel
mask inset and 32-pixel backing overfill are unchanged. The new art retains the
existing eight-color combined wood/foliage material subset.

## Assets

| Separate PNG module | Native pixels | Opaque colors |
|---|---:|---:|
'''+table+'''

Eleven matching editable LibreSprite documents are in Native/. Scripts prepare
explicit pixel plans; LibreSprite authors, saves and exports those plans, then
reopens the documents. Every reopened pixel matches its exported PNG. All eight
corner/rail connection ports and each rail's repeat endpoints match exactly.
Standalone foliage has padding and binary alpha; no filtered/resampled art.
NativeFrameStudy is a display-only assembly study, never a runtime sprite.

PixelLab: zero new calls or spending. The prior Stage02 source has verified
PixelLab/Rattlebones lineage; its existing palette and the checkpoint-one direction
study supply the material basis. New generations add no value to precise frame
joints. LibreSprite handles all eleven final native pieces.

## Preserved and validated

All '''+str(len(baseline))+''' existing protected files match their starting hashes, including the
approved checkpoint-2 background, characters, gems, all prior scene/source files,
shared controls and lower HUD. Only a new portrait preview scene/editor fixture
and the new review assets/evidence are added.

Tools/Validate-Unity.ps1 passed using Unity 6000.3.19f1. The explicit
ForestReworkV2Checkpoint03Tests.PortraitReview test passed. Actual Unity captures
at 720x480 and 1440x960 have exact nearest-neighbor 2x equality. Whole-pixel origins,
uniform scale, full standalone sprite extents and untruncated labels pass.

Frame shell cases: 30x30, 74x180, 98x220, 129x257 and 163x287 retain native corners,
centered rails/connectors and the mask/overfill relationship. The undersized 29x29
shell hides safely. This is a shell geometry check, not content reflow validation
at an unusably small size.

Before capture matches the approved checkpoint-2 capture exactly. Only frame and
foliage source masks contain changed pixels. Portrait contents and the whole enemy
side are pixel-identical. Removing the old upper ivy reveals 48 screen pixels of
the exact existing timber backing inside the opening; every other interior pixel
is unchanged. The initial whole-interior comparison flagged this intended removal;
the final check verifies its exact footprint and the exposed source tile pixels.
A magenta overfill probe changes zero pixels outside the
portrait mask. No camera gaps are exposed.

This is an art-only Unity preview with static Rattlebones and three Forest enemies,
not live combat, production safe-area mapping, Android or real-device validation.
Scene: Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint03.unity.
Menu: Dungeon Matcher / Art Preview / Forest rework v2 checkpoint 03.
The scene is excluded from build settings. Existing source scenes are never saved.

## Review and next step

Review.html contains actual Unity before/after captures, a portrait closeup and each
module at 1x/4x. Artistic verdict: calmer square joinery reads as a built timber
frame; two connected foliage accents keep the forest identity without extra props.
Character/gem contrast remains dominant. User approval is still required.

STOP here. The board-frame variant is the next checkpoint; no board or lower HUD
work belongs in this pass. No merge.
''')
cards=''.join('<figure><figcaption>'+r['name']+' - '+str(r['size'][0])+'x'+str(r['size'][1])+'</figcaption><img src="Previews/'+r['name']+'.png" width="'+str(r['size'][0])+'" height="'+str(r['size'][1])+'"><img src="Previews/'+r['name']+'.png" width="'+str(r['size'][0]*4)+'" height="'+str(r['size'][1]*4)+'"></figure>' for r in native['assets'])
(o/'Review.html').write_text('''<!doctype html><meta charset="utf-8"><title>Forest v2 - portrait checkpoint 3</title>
<style>body{background:#111b22;color:#dce6dc;font:16px system-ui;margin:28px}h1{font-size:24px}p{max-width:1000px;line-height:1.6}a{color:#a8d6be}img{image-rendering:pixelated;display:inline-block;vertical-align:top}section{display:flex;gap:24px;flex-wrap:wrap}figure{margin:0 0 24px;padding:14px;background:#1b2b32;border:1px solid #344a49}figcaption{margin-bottom:12px}button{font:inherit;background:#344a49;color:white;border:1px solid #619494;padding:8px 16px;cursor:pointer}.assets figure{min-width:180px}.assets img{margin:12px}</style>
<h1>Magical Forest - portrait frame review</h1><p>Checkpoint 3 only. Actual Unity art-only captures with static Rattlebones and three Forest enemies. The approved background, portrait contents, shared controls and lower-HUD files are unchanged. No production integration or merge.</p>
<p><button onclick="document.getElementById('scene').src='Previews/Unity-Before-720x480.png';document.getElementById('label').textContent='Before - previous portrait';">Before</button> <button onclick="document.getElementById('scene').src='Previews/Unity-After-720x480.png';document.getElementById('label').textContent='After - revised portrait';">After</button></p>
<figure><figcaption id="label">After - revised portrait</figcaption><img id="scene" src="Previews/Unity-After-720x480.png" width="720" height="480"></figure>
<section><figure><figcaption>Before - portrait closeup (nearest-neighbor)</figcaption><img src="Previews/Portrait-Before-Display.png" width="452" height="936"></figure><figure><figcaption>After - square timber joints, two vine groups</figcaption><img src="Previews/Portrait-After-Display.png" width="452" height="936"></figure></section>
<h2>Native asset modules: 1x and 4x</h2><p>Four 15x15 corners, four 32-pixel repeats with 9-pixel thickness, one 7x7 join, and two padded vines. Eight material colors total, binary alpha. PixelLab: zero new generations; prior source retained. LibreSprite: eleven exported/reopened native documents.</p><section class="assets">'''+cards+'''</section><p>Compilation and explicit Unity test passed. Exact 2x rendering, complete sprite bounds, five responsive shell sizes, eight exact corner ports and zero portrait-mask spill verified. <a href="README.md">Notes</a> · <a href="REPORT.json">Verification report</a> · <a href="../Checkpoint02/Review.html">Approved background checkpoint</a></p>''',encoding='utf-8')
if Path(__file__).resolve()!=(o/'Scripts/PublishReview.py').resolve():shutil.copy2(__file__,o/'Scripts/PublishReview.py')
print('Published checkpoint 3 review; protected files unchanged:',len(baseline))
