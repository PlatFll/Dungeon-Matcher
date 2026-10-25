from pathlib import Path
import json,hashlib,shutil,re,sys
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
R=F/'ArtSource/ForestSurroundRevision/LowerFireflies';W=Path(__file__).parent
qa=json.loads((R/'Diagnostics/Validation.json').read_text());assert qa['result']=='Passed'
log=Path(sys.argv[1]);assert log.is_file()
shutil.copy2(log,R/'Diagnostics/UnityCompile.log')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
(R/'Diagnostics/Compilation.json').write_text(json.dumps(dict(command='powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1',result='Passed',exit_code=0,unity='6000.3.19f1',log_sha256=sha(log),test_source_sha256=sha(F/'Assets/_Game/Scripts/Editor/ForestFireflyReviewTests.cs')),indent=2))
for name in ['VerifyLowerFireflies.py','FinishLowerFireflies.py']:
 shutil.copy2(W/name,R/'Scripts'/name)
for name in ['ArtPreview_ForestSurround_Fireflies.unity','ArtPreview_ForestSurround_Fireflies.unity.meta']:
 p=F/'Assets/_Game/Scenes'/name
 p.write_text('\n'.join(l.rstrip()for l in p.read_text().splitlines())+'\n')
page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Forest — lower fireflies</title>
<style>body{margin:24px;background:#14231d;color:#e6eddf;font:16px system-ui;line-height:1.5}h1{font-size:28px}p{max-width:980px}img,video{image-rendering:pixelated;max-width:100%}.row{display:flex;gap:20px;flex-wrap:wrap;align-items:flex-start}article{padding:14px;background:#243a2d}a{color:#bae7ab}button{padding:10px 18px;background:#365a42;color:inherit;border:1px solid #87ac88;cursor:pointer}.note{padding:14px;background:#243a2d}.still{width:360px}</style>
<h1>Warm fireflies around the board and lower roots</h1>
<p>Eight yellow fireflies use the existing top-HUD sprite. Each has its own gentle drift, brightness and fade timing, leaving the gems, controls and text clear.</p>
<p class="note">Actual Unity review scene. Previous artwork and scenes remain preserved. The top HUD, waterfall, other spirits and foliage retain their existing motion. Nothing merged.</p>
<button onclick="for(let v of document.querySelectorAll('video')){v.currentTime=0;v.play()}">Play / replay Unity preview</button>
<h2>Board surroundings and lower area</h2>
<video width="720" height="880" controls autoplay muted playsinline preload="auto" poster="Previews/Unity-Lower.png"><source src="Previews/Unity-Lower-Motion.webm" type="video/webm"></video>
<h2>Full scene and comparison</h2><div class="row"><video width="720" height="1360" controls muted playsinline preload="metadata" poster="Previews/Unity-Final-720x1360.png"><source src="Previews/Unity-Motion.webm" type="video/webm"></video><article><h3>Before</h3><img class="still" src="Previews/Unity-Before.png" alt="Previous forest scene"><h3>After</h3><img class="still" src="Previews/Unity-Final-720x1360.png" alt="Forest with extra yellow fireflies"></article></div>
<p>Recordings show eight seconds of Unity animation. The independently timed lights do not all blink together. Motion is snapped to native pixels; only opacity changes continuously.</p>
<h2>Verification</h2><p>Unity compilation and the focused render test passed. All 96 matching before/after frame pairs differ only inside the eight small firefly regions. The top HUD, gems, board, lower controls and text remain pixel-identical. 720 × 1360 and 1440 × 2720 captures have exact pixel doubling.</p>
<p>Original sources preserved: 7,102 files. Zero PixelLab generations or credits used. This is an editor art-review scene; production gameplay and device testing are separate.</p>
<p><a href="Previews/Unity-Final-720x1360.png">Final PNG</a> · <a href="README.md">Verification report</a> · <a href="Diagnostics/Validation.json">Pixel checks</a> · <a href="../../../Assets/_Game/Art/EnchantedForestStaged/Stage03/Firefly_Mote.png">Native firefly source</a></p></html>'''
# Gallery is served from ArtSource; copy the unchanged native input for a usable source link.
shutil.copy2(F/'Assets/_Game/Art/EnchantedForestStaged/Stage03/Firefly_Mote.png',R/'Previews/Firefly-Native.png')
page=page.replace('../../../Assets/_Game/Art/EnchantedForestStaged/Stage03/Firefly_Mote.png','Previews/Firefly-Native.png')
(R/'Review.html').write_text(page,encoding='utf-8')
(R/'README.md').write_text('''# Lower forest fireflies — review only

Adds eight yellow fireflies around the board-side roots and lower corner formations. All reuse the approved 8×8 `EnchantedForestStaged/Stage03/Firefly_Mote.png` with no pixel edits, recoloring or generation. Independent fade periods span 2.6–5.6 seconds, with one- to two-native-pixel drift. The original 19 motion tracks are unchanged.

New copied scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_Fireflies.unity`.
Previous scene: `ArtPreview_ForestSurround_Waterfall.unity`, preserved.
Builder/test: `Assets/_Game/Scripts/Editor/ForestFireflyReviewTests.cs`.

## Verified

- `Tools/Validate-Unity.ps1`: passed with Unity 6000.3.19f1.
- `ForestFireflyReviewTests.FireflyReview`: passed; 96 matching-time captures of each version over eight seconds. Every new light moves, becomes invisible and returns to its intended brightness.
- Pixel comparison: no changes outside the eight bounded firefly regions. The entire top HUD and previous motion, board/gems/frame, buttons, center panel and resonance text match at every sampled time.
- Uniform 2× native scale, integer positioning, Point filtering, no mipmaps, viewport clearance, UI clearance, no raycast interception and Reduced Motion checks pass.
- 1440×2720 is exact nearest-neighbor doubling of the 720×1360 capture.
- All 7,102 tracked baseline files remain hash-identical. Scene audit preserves every old object and track; only the root child list and motion track list grow.
- The first test run caught exact floating-point Vector2 comparison noise at 16×16. The final test checks dimensions with the established 0.02 screen-pixel tolerance and separately verifies exact rendered pixel doubling. Initial failure evidence is retained as Attempt01.
- The first pixel comparison caught one firefly occluded by the board-frame foliage. The final layer draws above the decorative frame and below the shared HUD; the matched-pixel check confirms all eight appear and disappear in the rendered scene.

`Review.html` contains actual Unity recordings. Raw paired frames are retained locally under `.utmp/forest-fireflies/`. Existing artwork, prior sources and scenes are preserved. No production gameplay or physical-device test is claimed. No PixelLab calls or credits used.

Draft PR #209 remains unmerged. Stop for visual review.
''',encoding='utf-8')
for link in re.findall(r'(?:src|href)="([^"]+)"',page):assert (R/link).is_file(),link
p=F/'Docs/PROJECT_STATE.md';text=p.read_text(encoding='utf-8')
old='Draft PR #209 remains unmerged; stop for user visual approval.'
assert text.count(old)==1
new='''The latest copied review, `ArtPreview_ForestSurround_Fireflies`, adds eight warm
fireflies around the board and lower roots using the unchanged top-HUD sprite.
All 19 previous motion tracks and 7,102 baseline files remain intact. Unity
compilation, 96 matched-time render pairs, UI clearance, Reduced Motion and
exact pixel doubling pass. Zero generations used. [Latest firefly review](../ArtSource/ForestSurroundRevision/LowerFireflies/README.md).
Draft PR #209 remains unmerged; stop for user visual approval.'''
p.write_text(text.replace(old,new),encoding='utf-8')
print('Firefly gallery, verification report and checkpoint ready.')
