from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil,html
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
R=F/'ArtSource/ForestSurroundRevision/LivingForest';W=Path(__file__).parent
validation=json.loads((R/'Diagnostics/Validation.json').read_text())
manifest=json.loads((R/'Diagnostics/NativeManifest.json').read_text())
cost=dict(start_generations=677.20,end_generations=672.20,generations_spent=5,usd_spent=0,
 selected=['Seed','Curl','Comet'],rejected={'Mote':'Scattered sparkles; replaced by the compact Seed source.','Ribbon':'Creature-like silhouette; rejected.'})
(R/'Diagnostics/Costs.json').write_text(json.dumps(cost,indent=2))
cards=[]
for item in manifest:
 n=item['name'];w=item['width'];h=item['height'];count=item['frames']
 if n in ['Grove_Motion_Copy','Corner_Right_NoSprig','Upper_Mote_A','Upper_Mote_B','Waterfall']:continue
 cards.append(f'<article><h3>{n.replace("_"," ")}</h3><p>{w} × {h} native · {count} poses · {item["colors"]} colors</p><img src="Exports/{n}_00.png" width="{w}" height="{h}" alt="Native first pose"> <img src="Previews/{n}-Motion.gif" alt="Enlarged native pose preview"><p><a href="Native/{n}.aseprite">LibreSprite source</a> · <a href="Previews/{n}-Strip.png">Pose strip</a></p></article>')
raw=[]
for name in ['Seed','Curl','Comet']:
 im=Image.open(R/'Raw'/f'{name}.png')
 raw.append(f'<article><h3>{name}</h3><p>Raw PixelLab · {im.width} × {im.height}</p><img src="Raw/{name}.png" width="{im.width}" height="{im.height}"></article>')
page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Living Magical Forest — motion review</title>
<style>body{margin:24px;background:#13201b;color:#e7ecdf;font:16px system-ui;line-height:1.5}h1{font-size:28px}p{max-width:1100px}a{color:#b9e1ac}img,video{image-rendering:pixelated;max-width:100%}.row{display:flex;gap:20px;flex-wrap:wrap;align-items:flex-start}article{padding:16px;background:#24382d;max-width:720px}.note{background:#24382d;padding:15px}button{padding:10px 16px;margin:4px;background:#355140;color:inherit;border:1px solid #738f72;cursor:pointer}.full{width:720px}.compare{width:360px}summary{cursor:pointer;padding:12px}video{background:#0b1611}</style>
<h1>Living Magical Forest</h1><p>The stray green sprig is removed on a copy. Brief leaf gusts, different spirit silhouettes and independent fade/drift timings bring the forest to life. Upper wisps, gold fireflies and two formerly baked-in blue motes now move; the waterfall has a faint descending shimmer.</p>
<p class="note">Actual Unity art-review captures, with static character and UI samples. The original scene and all prior artwork remain intact. No production gameplay integration or merge. The board, portrait, controls and text retain their exact pixels and placement.</p>
<h2>Actual Unity motion — 24 seconds</h2><p>Play to see differently timed fades and several quiet gusts. The water motion is deliberately subtle. The video stops at the end; the separate rhythms do not share a 24-second loop.</p>
<button onclick="document.getElementById('motion').play()">Play motion</button><button onclick="document.getElementById('motion').pause()">Pause</button><button onclick="let v=document.getElementById('motion');v.currentTime=0;v.play()">Replay</button>
<div class="row"><video id="motion" class="full" width="720" height="1360" controls playsinline muted preload="metadata" poster="Previews/Unity-Final-720x1360.png"><source src="Previews/Unity-Motion.webm" type="video/webm"></video><article><h3>Upper environment detail</h3><video width="720" height="480" controls playsinline muted preload="metadata" poster="Previews/Unity-Top.png"><source src="Previews/Unity-Top-Motion.webm" type="video/webm"></video><p>Leaves bend on native pixels. Lights fade smoothly while their positions snap to native pixels. The waterfall overlay is restricted to the existing water.</p><h3>Small correction</h3><img src="Previews/Sprig-BeforeAfter-4x.png" alt="Before and after removing the right sprig"></article></div>
<h2>Before / revised still</h2><div class="row"><article><h3>Before</h3><img class="compare" src="Previews/Unity-Before.png"></article><article><h3>Animated copy at 1.25 seconds</h3><img class="compare" src="Previews/Unity-Final-720x1360.png"></article></div>
<p><a href="Previews/Unity-Final-720x1360.png">720 × 1360 PNG</a> · <a href="Previews/Unity-Final-1440x2720.png">1440 × 2720 PNG</a> · <a href="Previews/Unity-ReducedMotion.png">Reduced-motion still</a></p>
<h2>Native sprite poses</h2><p>Native first poses beside enlarged pose previews. These isolated GIFs show the drawings; Unity controls the longer rests, fades and staggered timings.</p><div class="row">'''+''.join(cards)+'''</div>
<h2>Selected PixelLab sources</h2><div class="row">'''+''.join(raw)+'''</div><details><summary>Two rejected studies, preserved for source history</summary><div class="row"><article><p>Mote — too scattered</p><img src="Raw/Mote.png"></article><article><p>Ribbon — creature-like shape</p><img src="Raw/Ribbon.png"></article></div></details>
<h2>Verification</h2><p>43 native PNG poses round-trip through 11 LibreSprite sources exactly. Binary source transparency and canvas clearance pass. Unity captures cover 288 samples, 24 seconds, 720 × 1360 and 1440 × 2720; pixel doubling is exact. Board, portrait, frame and HUD pixels stay unchanged throughout. Paused and reduced-motion checks pass.</p>
<p>6,746 previous art, scene, script, resource and settings files remain unchanged. Five PixelLab generations spent (677.20 → 672.20); no USD credits used.</p><p><a href="README.md">Full report</a> · <a href="Diagnostics/Validation.json">Pixel verification</a> · <a href="Diagnostics/MotionEvidence.json">Motion checks</a> · <a href="Diagnostics/UnityTests.xml">Unity test result</a> · <a href="Diagnostics/Costs.json">Usage</a></p><p>Ready for your review. Nothing merged.</p></html>'''
(R/'Review.html').write_text(page,encoding='utf-8')
final=Image.open(R/'Previews/Unity-Final-720x1360.png');final.crop((0,0,720,480)).save(R/'Previews/Unity-Top.png')
before=Image.open(R/'Previews/Unity-Before.png');comp=Image.new('RGB',(1448,1392),'#13201b');d=ImageDraw.Draw(comp);d.text((8,9),'BEFORE',fill='white');d.text((736,9),'LIVING FOREST COPY',fill='white');comp.paste(before,(0,32));comp.paste(final,(728,32));comp.save(R/'Previews/BeforeAfter-Full.png')
report='''# Living Magical Forest — review copy

The outer green sprig on the right corner is removed. The unchanged corner source remains in `PolishFinal`; the revised source is `Native/Corner_Right_NoSprig.aseprite`.

Three five-pose foliage layers receive occasional 3.1-second gusts, separated by 9.7–12.5-second periods. Their anchors, palettes and native scale are preserved. Light silhouettes include a compact mote, curled crescent, long comet and the existing flame. Eight lower placements and the upper spirit/firefly layers have independent fade and drift rhythms. Fades range from quick 2.1-second fireflies to slower 8.9-second spirits. Their native position changes are integer pixels, never subpixel transforms.

Two tiny blue lights were separated from a copied upper backdrop. The other upper spirit layers and gold firefly already existed separately. A second gold firefly adds a different rhythm. The waterfall uses a 12-pose low-contrast overlay, clipped to existing water pixels; the bridge, foliage and shore shapes are unchanged.

## Sources and scope

- Separate scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_Living.unity`.
- Previous source scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_FinalPolish.unity`, unchanged.
- 11 editable LibreSprite documents; 43 native PNG poses under `Exports/` and the isolated Unity art folder.
- 5 PixelLab generations, 3 selected studies, 2 rejected. Raw outputs, receipts and reference hashes are preserved.
- Source alpha is binary; intentional Unity opacity animates light fades. No sprite resampling or scaling baked into the artwork.
- Board, gems, characters, portrait frame, UI, combat mechanics, saves and all production scene bindings are unchanged.

## Verified

- `Tools/Validate-Unity.ps1`: successful Unity compilation.
- `LivingForestReviewTests.MotionReview`: passed, actual Unity Play Mode render captures from the copied art scene.
- 720×1360 and 1440×2720: exact pixel doubling; Point filtering and no mipmaps; no truncated UI text.
- 288 captures over 24 seconds: no native canvas or viewport clipping; varying light opacity/position; foliage rests for most of the time; all water poses visited.
- Exact pixel comparisons protect the board, frame, portrait, bottom controls and all HUD text throughout the sequence. Changes stay inside explicitly authorized motion/removal regions.
- All 6,746 snapshotted prior art/scene/script/resource/settings files unchanged.
- Pause and Reduced Motion produce stable cosmetic poses.
- No Android build or physical-device test in this art-review pass. This is not a claim of production gameplay integration.

## Review and reuse

Open `Review.html` through the existing loopback gallery. The video contains actual Unity frames at 12 fps. In the scene, opacity updates every Unity frame; source poses remain discrete. Video stops after 24 seconds because independent rhythms do not share that loop length.

Open the copied scene and Play to inspect continuously. The editor-only component intentionally cannot alter a production build. Native GIFs/strips show the pose family, while the scene controls the holds, fades and drift. Existing assets/scenes remain the rollback baseline.

`Diagnostics/NativeVerification.json`, `Validation.json`, `MotionEvidence.json`, `SceneAudit.json`, `ReferenceHashes.json` and `Costs.json` retain evidence and source lineage. Raw motion captures and full local logs are in the worktree scratch/diagnostics directories.

Draft PR #209 remains unmerged. Stop for user visual approval.
'''
(R/'README.md').write_text(report,encoding='utf-8')
for name in ['VerifyLivingNative.py','VerifyLivingScene.py','FinishLivingForest.py','DownloadLivingForest.py','InstallLivingForestReview.py','LivingForestReviewMotion.cs','LivingForestReviewTests.cs']:
 shutil.copy2(W/name,R/'Scripts'/name)
for p in (R/'Diagnostics').glob('*.err'):
 if p.stat().st_size==0:p.unlink()
# New review metadata may have Unity's blank trailing spaces. Preserve existing metadata.
for p in list((F/'Assets/_Game/Art/ForestSurroundRevision/LivingForest').rglob('*.meta'))+[F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_Living.unity']:
 s=p.read_text();p.write_text('\n'.join(line.rstrip() for line in s.splitlines())+'\n')
state=F/'Docs/PROJECT_STATE.md';s=state.read_text(encoding='utf-8');marker='## Forest surround polish'
new='''## Living Forest motion copy (2026-10-25, review only)

The user requested non-destructive motion on the latest forest scene. A separate
`ArtPreview_ForestSurround_Living` scene removes the marked right-corner sprig,
adds occasional native-pixel leaf gusts, varied spirit sizes/fade/drift rhythms,
moving upper lights and a subtle water-only waterfall shimmer. All 6,746 prior
source/art/scene/script/resource/settings files remain unchanged. Five PixelLab
generations produced three selected wisp studies; LibreSprite sources and exact
exports are retained. Unity compilation and the copied-scene motion test pass:
288 captures over 24 seconds, exact pixel doubling at 720×1360 / 1440×2720,
protected board/portrait/HUD pixels unchanged, pause/reduced motion stable.
This is an editor art preview, not production gameplay integration or device
testing. [Motion review and validation](../ArtSource/ForestSurroundRevision/LivingForest/README.md).
Draft PR #209 remains unmerged; stop for user visual approval.

'''
assert marker in s and '## Living Forest motion copy' not in s
state.write_text(s.replace(marker,new+marker,1),encoding='utf-8')
print('Gallery, source handoff, cost ledger and project checkpoint written.')
