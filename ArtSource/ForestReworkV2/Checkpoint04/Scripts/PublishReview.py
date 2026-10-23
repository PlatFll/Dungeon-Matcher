from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil,sys,xml.etree.ElementTree as ET
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint04';sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
baseline=json.loads((o/'Diagnostics/ProtectedHashes.json').read_text());changed=[f for f,h in baseline.items() if not(p/f).is_file() or sha(p/f)!=h];assert not changed,changed
native=json.loads((o/'Diagnostics/NativeValidation.json').read_text());pixels=json.loads((o/'Diagnostics/PixelValidation.json').read_text())
xml=ET.parse(o/'Diagnostics/UnityTests.xml').getroot();assert xml.attrib['result']=='Passed' and xml.attrib['failed']=='0';assert len(native['assets'])==10
log=Path('C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-b405ce19-9a06-4ef6-bebb-67e0eabdf09a.log');shutil.copy2(log,o/'Diagnostics/Compilation.log')
report={'status':'Checkpoint4 board-frame candidate awaiting visual approval','protected_existing_files_unchanged':len(baseline),'native':native,'pixels':pixels,'unity':{'validator_exit':0,'validator_log':str(log),'test_passed':int(xml.attrib['passed']),'test_failed':int(xml.attrib['failed']),'process_exit':int(sys.argv[2]),'sizes':[[46,46],[304,304],[305,303],[360,280],[259,411]],'undersized_45x45_shell_hides':True},'pixellab':{'jobs':1,'charged_generations':5,'job_id':'6dcaea77-1ca7-4d83-bd19-493bfec99f9e','source_image_id':'02ad0226-980a-51e7-96d3-f687a7955e4e','role':'Sparse leaf/vine motif only; actual verified Rattlebones image used for rendering traits, separate approved foliage palette'},'libresprite':'Ten native sources authored/exported and reopened exactly; leaf cleanup and independent lower-right light described in Cleanup.json','unchanged':['approved background and portrait checkpoint sources/scenes','all gem and cell drawings, native 64x64 and existing 512x512 board','shared controls and lower HUD','production scene, gameplay, zone bindings','all prior source art'], 'not_performed':['production combat','safe-area integration','Android build','real-device test'],'stop':'Checkpoint4 complete. Next is integrated Unity review, checkpoint5. No merge.'}
(o/'REPORT.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
table='\n'.join('| '+x['name']+' | '+str(x['size'][0])+' x '+str(x['size'][1])+' | '+str(x['opaque_colors'])+' |' for x in native['assets'])
(o/'README.md').write_text('''# Magical Forest v2 - checkpoint 4: board frame

Status: review candidate. The user authorized Continue after the portrait review.
Stop at this board-frame checkpoint. No production integration or merge.

## Result and asset list

The board uses sturdy square mitred timber with sparse connected grain. Spiral
corner plates, repeated connector studs, extra side posts/feet/braces, board lamps,
rune tags and mushrooms are absent from the revised assembly. Two broad connected
leaf/vine accents provide the forest identity. They remain separate sprites with
padded canvases and do not overlap a cell. They can later support gentle ambient
motion; no motion is authored or enabled at this checkpoint.

| Module | Native pixels | Opaque colors |
|---|---:|---:|
'''+table+'''

The 304x304 native frame target at 2x retains its prior 608x608 screen footprint,
surrounding the unchanged 512x512 board at a 24-native-pixel inset. Gems and cells
remain their original 64x64 drawings. No resampling, resizing or redraw of board
content occurs. The existing ResponsiveModularFrameFitter owns native corners and
tiled rails. No new runtime frame or layout system is introduced.

PixelLab contributes one 64x64 foliage source, charged 5 generations. Its actual
style image is the verified Rattlebones PNG, for rendering discipline only. The
existing muted woodland palette is supplied independently. Raw prompt, seed,
reference hashes, job ID, receipts and source remain in Raw/ and Diagnostics/.
No full screen or new frame structure is generated. No further calls are made.

LibreSprite writes all ten native modules from explicit pixel plans and reopens
the editable sources for exact export comparison. The rails and joints are direct
native drawings in the portrait's material family. Foliage cleanup is local and
its lower-right shading is authored independently; see Diagnostics/Cleanup.json.
All eight corner/rail ports and repeated rail endpoints match exactly. Standalone
leaves retain transparent padding, binary alpha and zero transparent RGB.

## Unity review and validation

The isolated board scene clones only the previous board contents/frame/props for
comparison. The 64 cells and gems retain their relative positions and exact scale.
The before/after view has a shared unchanged timber backing and a larger review
canvas so both foliage contours fit. Approved background/portrait and lower-HUD
scenes are not rebuilt here. Full-screen integration remains checkpoint 5.

Actual Unity captures: 720x704 and 1440x1408; exact nearest-neighbor 2x equality.
Compilation: Tools/Validate-Unity.ps1 passed, Unity 6000.3.19f1.
Explicit test: ForestReworkV2Checkpoint04Tests.BoardReview passed, process exit 0.
Five shell cases include odd dimensions and non-square extents; native corners,
rail centering and tile pitch pass. A 45x45 undersized shell hides rather than
distorting its parts. These are shell checks, not production phone safe-area tests.

Every pixel in the 512x512 opening matches the previous Unity capture and an
independent composition of the original cell/gem PNGs. Frame-on versus frame-off
captures and a separate alpha/placement test show zero board intrusion. Empty
cells remain exact. Full independent-sprite extents, integer origins, uniform
scale, Point filtering and zero camera gaps pass.

All '''+str(len(baseline))+''' pre-existing protected files retain their hashes. Previous candidate
assets remain available. Production scene, mechanics, characters, gems, controls,
approved background/portrait and shared lower HUD are unchanged. No live-combat,
Android or real-device validation is claimed.

Scene: Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint04.unity.
Menu: Dungeon Matcher / Art Preview / Forest rework v2 checkpoint 04.
The scene is excluded from build settings. Review.html shows actual Unity
before/after, empty cells and native modules. New artwork remains a candidate for
user visual approval. Checkpoint 5 is next; do not merge.
''',encoding='utf-8')
cards=''.join('<figure><figcaption>'+r['name']+' - '+str(r['size'][0])+'x'+str(r['size'][1])+'</figcaption><img src="Previews/'+r['name']+'.png" width="'+str(r['size'][0])+'" height="'+str(r['size'][1])+'"><img src="Previews/'+r['name']+'.png" width="'+str(r['size'][0]*3)+'" height="'+str(r['size'][1]*3)+'"></figure>' for r in native['assets'])
(o/'Review.html').write_text('''<!doctype html><meta charset="utf-8"><title>Forest v2 board frame - checkpoint 4</title>
<style>body{background:#111b22;color:#dce6dc;font:16px system-ui;margin:28px}h1{font-size:24px}p{max-width:1000px;line-height:1.6}a{color:#a8d6be}img{image-rendering:pixelated;vertical-align:top}figure{margin:0 0 24px;padding:14px;background:#1b2b32;border:1px solid #344a49;display:table}figcaption{margin-bottom:12px}button{font:inherit;background:#344a49;color:white;border:1px solid #619494;padding:8px 16px;cursor:pointer}.assets{display:flex;gap:20px;flex-wrap:wrap}.assets figure{min-width:160px}.assets img{margin:10px}</style>
<h1>Magical Forest - cleaner board frame</h1><p>Checkpoint 4 only. Actual Unity art-only board preview. Same 512x512 board, original 64x64 gems/cells. Approved background, portrait and lower HUD remain unchanged. Full-screen assembly is the next checkpoint.</p>
<p><button onclick="setView('Before','Previous board framing')">Before</button> <button onclick="setView('After','Revised timber and foliage')">After</button> <button onclick="setView('Empty','Empty cells: frame clearance')">Empty board</button></p>
<figure><figcaption id="label">Revised timber and foliage</figcaption><img id="scene" src="Previews/Unity-After-720x704.png" width="720" height="704"></figure>
<script>function setView(which,label){document.getElementById('scene').src='Previews/Unity-'+which+'-720x704.png';document.getElementById('label').textContent=label;}</script>
<p>Four native corners, four repeating rails, two separate foliage sweeps. No additional board props. PixelLab: one foliage source, 5 generations. LibreSprite: ten native documents. <a href="../Checkpoint03/Review.html">Approved portrait/background</a></p>
<h2>Native modules: 1x and 3x</h2><div class="assets">'''+cards+'''</div><h2>Foliage source and cleanup</h2><figure><figcaption>PixelLab raw (left), upper-left cleanup, lower-right companion with independent light</figcaption><img src="Raw/Foliage_PixelLab.png" width="192" height="192"><img src="Previews/Board_Foliage_UL.png" width="192" height="192"><img src="Previews/Board_Foliage_LR.png" width="192" height="192"></figure><p>Compilation and Unity preview checks passed. Exact 2x rendering, five shell sizes, eight matching corner ports and zero cell intrusion. <a href="README.md">Notes</a> | <a href="REPORT.json">Verification report</a>. No merge.</p>''',encoding='utf-8')
board=Image.new('RGB',(720,640),'#17222b');d=ImageDraw.Draw(board)
for n,r in enumerate(native['assets']):
 im=Image.open(o/'Previews'/(r['name']+'.png'));x=n%4*180;y=n//4*210;d.text((x+4,y+4),r['name'].replace('Board_',''),fill='white');big=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);board.paste(big,(x+4,y+26),big)
board.save(o/'Previews/NativeModules-2x.png')
if Path(__file__).resolve()!=(o/'Scripts/PublishReview.py').resolve():shutil.copy2(__file__,o/'Scripts/PublishReview.py')
print('Review published, protected files unchanged:',len(baseline))
