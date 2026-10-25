from pathlib import Path
import json,hashlib,shutil,re
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/WaterfallCorrection';W=Path(__file__).parent
qa=json.loads((R/'Diagnostics/Validation.json').read_text());assert qa['result']=='Passed'
log=Path(r'C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-35d13ef3-0939-4cdd-9e5a-7d83c63e73bd.log');shutil.copy2(log,R/'Diagnostics/UnityCompile.log')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
test=F/'Assets/_Game/Scripts/Editor/ForestWaterfallRevisionTests.cs'
(R/'Diagnostics/Compilation.json').write_text(json.dumps(dict(command='powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1',result='Passed',exit_code=0,unity='6000.3.19f1',log_sha256=sha(log),test_source_sha256=sha(test)),indent=2))
for file in ['BuildWaterfallCorrection.py','VerifyWaterfallCorrection.py','InstallWaterfallCorrection.py','FinishWaterfallCorrection.py']:
 shutil.copy2(W/file,R/'Scripts'/file)
for p in list((F/'Assets/_Game/Art/ForestSurroundRevision/WaterfallCorrection').rglob('*.meta'))+[F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_Waterfall.unity',F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_Waterfall.unity.meta']:
 p.write_text('\n'.join(l.rstrip()for l in p.read_text().splitlines())+'\n')
for p in (R/'Diagnostics').glob('*.err'):
 if p.stat().st_size==0:p.unlink()
page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Forest — waterfall correction</title>
<style>body{margin:24px;background:#14231d;color:#e6eddf;font:16px system-ui;line-height:1.5}h1{font-size:28px}p{max-width:1000px}img,video{image-rendering:pixelated;max-width:100%}.row{display:flex;gap:20px;flex-wrap:wrap;align-items:flex-start}article{padding:14px;background:#243a2d}a{color:#bae7ab}button{padding:10px 18px;background:#365a42;color:inherit;border:1px solid #87ac88;cursor:pointer}.note{padding:14px;background:#243a2d}.still{width:360px}</style>
<h1>Waterfall correction · long wisps removed</h1><p>Both long-tailed comet placements are gone. The waterfall now visibly flows downward in a crisp 24-frame, two-second cycle. The bridge and banks stay fixed.</p>
<p class="note">Actual Unity review copy. Original assets and earlier scenes are preserved. Other wisps, foliage, characters, board and UI match the previous version at the same animation times. No merge.</p>
<button onclick="for(let v of document.querySelectorAll('video')){v.currentTime=0;v.play()}">Play / replay Unity preview</button>
<h2>Upper environment in Unity</h2><div class="row"><video id="top" width="720" height="480" controls autoplay muted playsinline preload="auto" poster="Previews/Unity-Top.png"><source src="Previews/Unity-Top-Motion.webm" type="video/webm"></video><article><p>Water detail · exact loop</p><img src="Previews/Unity-Waterfall.gif" width="88" height="432" alt="Looping water animation cropped from Unity"></article></div>
<h2>Full scene</h2><div class="row"><video width="720" height="1360" controls muted playsinline preload="metadata" poster="Previews/Unity-Final-720x1360.png"><source src="Previews/Unity-Motion.webm" type="video/webm"></video><article><h3>Before</h3><img class="still" src="Previews/Unity-Before.png"><p>Both long comet wisps removed in the revised scene.</p></article></div>
<p>The full recordings are eight-second samples. The water detail loops continuously; the remaining lights retain their independent timings.</p>
<h2>Native source and checks</h2><p>24 poses at 40 × 144, three water tones, binary source alpha. Native pixel flow advances downward by one pixel per pose at 12 fps. No filtering or screen distortion.</p>
<p>Unity compilation and the focused render test pass. 96 matching before/after frame pairs differ only inside the existing water and the two removed comet regions. All 6,982 prior files are preserved. Exact pixel doubling passes at 720 × 1360 and 1440 × 2720.</p>
<p><a href="Native/Waterfall_Flow.aseprite">LibreSprite source</a> · <a href="Previews/Waterfall-Strip.png">Native strip</a> · <a href="Previews/Unity-Final-720x1360.png">Final PNG</a> · <a href="README.md">Report</a> · <a href="Diagnostics/Validation.json">Validation</a></p><p>Zero PixelLab generations used for this correction. Review only; nothing merged.</p></html>'''
(R/'Review.html').write_text(page,encoding='utf-8')
for link in re.findall(r'(?:src|href)="([^"]+)"',page):assert (R/link).is_file() or link=='README.md',link
report='''# Forest waterfall correction

The previous waterfall animation had only five low-contrast single-pixel streaks and did not read clearly in the assembled scene. The new LibreSprite source has 24 native 40×144 poses with broader connected light/shadow ribbons. These move downward by one pixel per pose at 12 fps and close exactly after two seconds. Painting is restricted to the original bright water plane above and below the bridge; banks, trees and bridge pixels remain fixed.

Both long comet-wisp instances are inactive in a new copied scene. Their source artwork and the previous review scene are retained. No replacement wisps were added. The other light/foliage tracks are unchanged.

Scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_Waterfall.unity`.
Previous scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_Living.unity`, unchanged.
Source: `Native/Waterfall_Flow.aseprite`; all native exports are in `Exports/`.

## Verified

- `Tools/Validate-Unity.ps1`: passed with Unity 6000.3.19f1.
- `ForestWaterfallRevisionTests.WaterfallReview`: passed in Unity with 96 captures of each version at matching times.
- Pixel comparison: no changes outside the water and removed comet regions. All remaining motion, characters, board and HUD pixels match at each sampled time.
- All 24 water poses are visible, loop exactly in actual Unity water pixels, and reopen/export from LibreSprite with exact authored pixels and binary alpha.
- Uniform native scale, viewport clearance, Point filtering, no mipmaps and Reduced Motion checks pass. 1440×2720 is exact pixel doubling of 720×1360.
- 6,982 prior files remain hash-identical. Scene audit preserves every old object and changes only the two comet active flags, the water sprite and its track/frame references.
- No production gameplay integration, Android build or physical-device testing is claimed. No PixelLab calls or credits used.

`Review.html` contains actual Unity recordings and a looping water-only detail. The eight-second scene recording is not advertised as a seamless loop for the independently timed lights. Full raw frame pairs remain under `.utmp/forest-waterfall/` locally.

Draft PR #209 remains unmerged. Stop for user review.
'''
(R/'README.md').write_text(report,encoding='utf-8')
p=F/'Docs/PROJECT_STATE.md';s=p.read_text(encoding='utf-8');old='Draft PR #209 remains unmerged; stop for user visual approval.'
new='''The current correction removes both long comet placements on a further copy,
`ArtPreview_ForestSurround_Waterfall`, and replaces the barely visible shimmer
with a 24-pose downward water-flow cycle. Its LibreSprite source, 96 matching
before/after Unity frame pairs and exact scope/hash checks pass. All 6,982 prior
files are unchanged; zero new generations used. [Current waterfall review](../ArtSource/ForestSurroundRevision/WaterfallCorrection/README.md).
Draft PR #209 remains unmerged; stop for user visual approval.'''
assert s.count(old)==1;s=s.replace(old,new);p.write_text(s,encoding='utf-8')
print('Waterfall review gallery, report and source-hash evidence saved.')
