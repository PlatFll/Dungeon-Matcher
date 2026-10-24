from pathlib import Path
from collections import Counter
import hashlib,json,shutil,sys
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint05';w=Path(__file__).parent
sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
baseline=json.loads((o/'Diagnostics/ProtectedHashes.json').read_text())
changes=[f for f,h in baseline.items() if not(p/f).is_file() or sha(p/f)!=h];assert not changes,changes
checks=json.loads((o/'Diagnostics/PixelValidation.json').read_text())
uses=json.loads((o/'Diagnostics/Unity-720x1360.json').read_text())['images']
paths=sorted({u['path'] for u in uses});sources=[{'path':f,'sha256':sha(p/f)} for f in paths]
(o/'Diagnostics/AssemblySources.json').write_text(json.dumps(sources,indent=2))
compilation=Path('C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-eabbafe3-b302-4337-8117-387847150513.log')
shutil.copy2(compilation,o/'Diagnostics/Compilation.log');shutil.copy2(w/'RunForestV2C5.ps1',o/'Scripts/RunUnity.ps1')
report={'status':'Checkpoint 5 assembled art-only scene, awaiting visual review','source_approval':'User approved checkpoint 4 and requested full assembly with approved artwork and lower HUD unchanged','protected_existing_files_unchanged':len(baseline),'reused_image_source_count':len(paths),'new_art_assets':0,'pixel_checks':checks,'unity':{'validator_exit':0,'validator_log':str(compilation),'preview_test':'ForestReworkV2Checkpoint05Tests.FullAssemblyReview','test_process_exit':int(sys.argv[2])},'generation_spend_this_checkpoint':0,'libresprite':'No new drawing or editing. Uses exact approved LibreSprite exports from checkpoints 2-4 and earlier retained assets. All source hashes preserved.','scope':'New review scene and editor fixture only. Shared lower HUD and production bindings unchanged.','not_performed':['live combat','phone safe-area integration','Android build','real-device testing','checkpoint 6 final validation'],'next':'Stop for user review. Do not merge.'}
(o/'REPORT.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
(o/'README.md').write_text('''# Magical Forest v2 - checkpoint 5: assembled Unity preview

The approved top background and portrait, board frame and shared lower HUD now
appear together in a new art-only Unity scene. Rattlebones and three existing
Forest enemies are static readability samples, with existing gems, health bars,
countdowns and shared controls. No production or gameplay binding changes.

## Reused artwork

- Checkpoint 2 enchanted woodland, layered depth and distant lights.
- Checkpoint 3 portrait: mitred timber, connected vines, centered tenons and clipped backing.
- Checkpoint 4 board: four corners, four repeating rails and two ivy overlays.
- Earlier approved ground, near trees, hanging lanterns, current characters and UI.
- Original 64x64 gems/cells and 512x512 board; shared lower HUD and continuous backing.

No new PNGs or editable art documents are created or modified. PixelLab: zero
generations this stage. LibreSprite: exact approved exports reused without edits.
The previous stages' generation and cleanup lineage remains in their own folders.
The full scene retains modular, independent images rather than a flattened asset.
The builder copies only the approved scene hierarchies and remaps the board review
by 432 screen pixels to the established full layout. It never invokes earlier
builders or saves earlier scene paths. Preview text grids are rebound to this scene.

## Executed checks

- Tools/Validate-Unity.ps1 passed, Unity 6000.3.19f1.
- Explicit FullAssemblyReview test passed; process exit 0.
- Actual Unity captures at 720x1360 and 1440x2720; exact nearest-neighbor 2x equality.
- Entire 720x480 top region matches the approved checkpoint 3 render exactly.
- Entire 512x512 board opening matches the approved checkpoint 4 and original renders.
- Every opaque approved board-frame pixel matches checkpoint 4 after placement.
- All shared lower HUD pixels match the original full review. The only difference
  below y=1104 is 476 pixels of the old board ivy beside the energy bar, already
  removed by approved checkpoint 4. Those pixels reveal the exact original backing;
  all other pixels in the lower region match. No HUD drawing or layout changes.
- Portrait mask probe changes only pixels inside its 148x392 screen rectangle.
- Empty cells remain exact. No camera gaps, clipped independent sprites, stretched
  pixels or truncated text. Native Point filtering and integer uniform scale pass.
- All '''+str(len(baseline))+''' pre-existing protected files retain their SHA-256 hashes.

This is an art-only Unity Play Mode capture with static samples, not live combat.
Production phone safe-area integration, gameplay behavior, Android and real-device
testing are outside this checkpoint. Checkpoint 6 final validation has not started.

## Review

Open Review.html for full before/after, structure-only, top detail and lower HUD.
Scene: Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint05.unity.
Menu: Dungeon Matcher / Art Preview / Forest rework v2 checkpoint 05 full assembly.
The review scene is excluded from build settings. Diagnostics/AssemblySources.json
records all '''+str(len(paths))+''' reused image sources and current hashes. REPORT.json and pixel/Unity
evidence distinguish performed checks from untested production behavior.

Stop for visual review. Do not merge.
''',encoding='utf-8')
from PIL import Image
full=Image.open(o/'Previews/Unity-Full-720x1360.png')
full.crop((0,0,720,480)).save(o/'Previews/Unity-Top-720x480.png')
full.crop((0,1104,720,1360)).save(o/'Previews/Unity-LowerHUD-720x256.png')
(o/'Review.html').write_text('''<!doctype html><meta charset="utf-8"><title>Magical Forest - full assembled Unity preview</title>
<style>body{margin:28px;background:#111b22;color:#dce6dc;font:16px system-ui}h1{font-size:25px}p{max-width:1050px;line-height:1.55}a{color:#a8d6be}img{image-rendering:pixelated;vertical-align:top}figure{display:table;margin:18px 0;padding:14px;background:#1b2b32;border:1px solid #344a49}figcaption{margin-bottom:12px}button{font:inherit;background:#344a49;color:white;border:1px solid #619494;padding:8px 14px;cursor:pointer}</style>
<h1>Magical Forest - full assembled Unity preview</h1>
<p>Checkpoint 5. Approved artwork assembled in Unity, with Rattlebones, three existing Forest enemy samples and original 64x64 gems. Art-only review; combat is not running. Lower HUD unchanged. No new artwork, generation spending or production integration.</p>
<p><button onclick="show('Full','Approved forest assembly')">Assembled preview</button> <button onclick="show('Before','Previous full forest review')">Before</button> <button onclick="show('Structure','Environment and empty board')">Structure only</button></p>
<figure><figcaption id="caption">Approved forest assembly - actual Unity capture, 720x1360</figcaption><img id="scene" src="Previews/Unity-Full-720x1360.png" width="720" height="1360"></figure>
<script>function show(which,label){document.getElementById('scene').src='Previews/Unity-'+which+'-720x1360.png';document.getElementById('caption').textContent=label+' - actual Unity capture, 720x1360';}</script>
<p><a href="Previews/Unity-Full-1440x2720.png">Open full 2x Unity capture</a> | <a href="../Checkpoint04/Review.html">Approved board assets</a> | <a href="../Checkpoint03/Review.html">Approved portrait assets</a></p>
<figure><figcaption>Approved top background and portrait - pixel-identical to checkpoint 3</figcaption><img src="Previews/Unity-Top-720x480.png" width="720" height="480"></figure>
<figure><figcaption>Shared lower HUD unchanged; the old board vine beside the bar is removed</figcaption><img src="Previews/Unity-LowerHUD-720x256.png" width="720" height="256"></figure>
<p>Unity compilation and preview test passed. Exact 2x rendering; unchanged top, board and lower HUD; no mask spill, clipped independent sprites or camera gaps. <a href="README.md">Notes</a> | <a href="REPORT.json">Verification report</a>. Checkpoint 6 awaits approval. Nothing merged.</p>
''',encoding='utf-8')
if Path(__file__).resolve()!=(o/'Scripts/PublishReview.py').resolve():shutil.copy2(__file__,o/'Scripts/PublishReview.py')
print('Published full Unity review. Protected files:',len(baseline),'reused image sources:',len(paths),'new art: 0')
