from pathlib import Path
from PIL import Image,ImageChops,ImageDraw,ImageFont
import hashlib,json,shutil,sys
P=Path(sys.argv[1]);O=P/'ArtSource/ForestReworkV2/Checkpoint01';S=P/'.utmp/forest-v2-direction'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
checks={};counts={}
for name in ['DirectionStudy','LeafSweep_A','LeafSweep_B','TimberJoinStudy']:
    im=Image.open(O/'Previews'/(name+'.png')).convert('RGBA')
    expected=Image.open(S/(name+'-expected.png')).convert('RGBA')
    reopened=Image.open(S/(name+'-reopened.png')).convert('RGBA')
    assert im.tobytes()==expected.tobytes()==reopened.tobytes(),name
    alpha=sorted(set(im.getchannel('A').getdata()));assert set(alpha)<={0,255}
    counts[name]={'size':im.size,'opaque_colors':len({c[:3] for c in im.getdata() if c[3]}),'alpha':alpha,'sha256':sha(O/'Previews'/(name+'.png'))}
checks['libresprite_plan_and_reopen_exact']=True
new=Image.open(O/'Previews/DirectionStudy.png').convert('RGBA')
old=Image.open(P/'ArtSource/ForestHybrid/Stage05/Previews/Unity-Full-720x1360.png').convert('RGBA')
structure=Image.open(P/'ArtSource/ForestHybrid/Stage05/Previews/Unity-Structure-720x1360.png').convert('RGBA')
for name,box in [('board',(104,528,616,1040)),('entire_lower_hud',(0,1088,720,1360)),('portrait',(16,16,220,456)),('wave',(242,20,462,120)),('guide',(484,28,624,108)),('settings',(636,28,708,108))]:
    assert new.crop(box).tobytes()==old.crop(box).tobytes(),name
    checks[name+'_pixel_exact']=True
changed_actor_pixels=0
for y in range(480):
    for x in range(720):
        if old.getpixel((x,y))!=structure.getpixel((x,y)):
            assert new.getpixel((x,y))==old.getpixel((x,y))
            changed_actor_pixels+=1
checks['retained_actor_hp_countdown_pixels']=changed_actor_pixels
leafmask=Image.new('RGBA',(720,1360))
for name,at in [('LeafSweep_A',(26,480)),('LeafSweep_B',(566,960))]:
    im=Image.open(O/'Previews'/(name+'.png')).convert('RGBA').resize((128,128),Image.Resampling.NEAREST)
    leafmask.alpha_composite(im,at)
assert leafmask.crop((104,528,616,1040)).getchannel('A').getbbox() is None
checks['new_foliage_inside_board_pixels']=0
protected=json.loads((O/'Diagnostics/ProtectedHashes.json').read_text())
changed=[p for p,h in protected.items() if not (P/p).is_file() or sha(P/p)!=h]
assert not changed,changed
checks['protected_files_unchanged']=len(protected)
checks['unity_run']='Not run: checkpoint-one source/art-direction study; no Assets, scene, C#, or import changes.'
checks['device_testing']='Not performed.'
checks['new_pixellab_generations']=0
checks['status']='Direction candidate, awaiting checkpoint-one review. Not production-approved.'
(O/'Diagnostics/Validation.json').write_text(json.dumps({'checks':checks,'native_studies':counts},indent=2))
new.crop((0,480,720,1088)).save(O/'Previews/BoardDetail.png')
new.crop((0,0,720,480)).save(O/'Previews/UpperDetail.png')
old.crop((0,480,720,1088)).save(O/'Previews/BoardBefore.png')
new.crop((0,1088,720,1360)).save(O/'Previews/LowerHudUnchanged.png')
for name,path in [('Rattlebones','Docs/ArtDirection/Audit2026-10/media/skeleton-still.png'),('Farmer','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png'),('PanVillager','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png')]:shutil.copy2(P/path,O/'Previews'/(name+'.png'))

plan='''# Magical Forest v2 — checkpoint one

Status: **direction study for review**, not a final asset set or a new Unity capture.
Branch: `feature/magical-forest-zone-rework-v2`, based on Forest PR #206 commit
`2087e49905a4db1db1dddbdcd995419115715ebe`. The old branch and all prior source art remain intact.

## Why the assembled Forest felt noisy

The board repeated several unrelated motifs: spiral medallions, connector studs,
side braces, lanterns, dangling rune tags, and mushrooms. Their separate silhouettes
competed with the gems. The board structure also appeared heavier than the portrait.
The large central background was pale and visually uniform, despite its distant layers.

## Proposed direction: an old woodland sanctuary

Let the fighters and gems carry the brightest contrasts. Keep large tree silhouettes,
a quiet flat standing strip, and a cool distant clearing. Two small magical lights sit
above the fighters, with warm lanterns confined to the upper tree supports. The board
uses solid timber beams, plain mitred joints, and two connected leaf/vine sweeps.
No mushrooms, lanterns, rune tags, spiral medals or extra braces around the board.

The direction study demonstrates that hierarchy with existing native art and native
pixel motif sketches. It does not establish final wood carving or leaf shapes.
The lower-right motif currently rotates the upper-left study for composition only;
the final asset pass must author its top-left light independently.

## Asset plan and reuse

| Area | Keep | Planned focused work | Review gate |
|---|---|---|---|
| Far forest | Existing ruin/waterfall silhouette and layered trees | Cooler value groups, localized magical light; avoid random particles | 2: top blockout |
| Ground | Existing flat Ground_TopA/B and Ground_FaceA/B, current baseline | Check tile phase and joins only; no added debris in standing area | 2 |
| Large trees | Tree_Left/Right and broad canopy arrangement | Inspect connected canopy masses; preserve open middle | 2 |
| Portrait | Inset mask, character/gem/HP placement, existing slim frame proportions | Simplify tiny clamps only if needed; use the same restrained wood/leaf motif | 3 |
| Board structure | Board rectangle, 64px cells, all gem art, existing modular fitter | Replace spiral corners with quiet joints; reuse rail dimensions and simplify material clusters; remove extra posts/feet/braces | 4 |
| Board foliage | Separate overlay architecture | Two broad connected sweeps, complete padded canvases, no opaque pixels inside grid; independent leaves may later move | 4 |
| Upper lights | Existing hanging lantern silhouette/material family | Retain two attached to branches; no board lamps; small far light accent only | 2 |
| Old optional props | Preserve source files | Exclude mushrooms and loose rune plaques from this composition | 5 |
| Shared UI | Wave/Guide/Settings, HP, progress, potion/bomb, resonance and caption | **No redesign or recolor** | all |
| Characters/gems | Approved Rattlebones and Forest cast, current 64px gems | **No changes** | all |

## Tool use

PixelLab: **zero new calls / zero generations** at this checkpoint. The retained Forest
modules already have PixelLab lineage in `ArtSource/ForestHybrid`. Selective generation
is deferred until the direction is accepted and a specific asset needs it; no full-scene generation.

LibreSprite: executed the exact native-pixel direction plan, authored broader connected
leaf studies and plain timber-joint study, exported PNG and editable ASEPRITE files,
then reopened all four documents. Existing imports were not edited. Python prepares
deterministic pixel plans, composes the review study, and performs independent byte checks.

## Evidence and limits

`Previews/DirectionStudy.png` is a **720×1360 composition study**, based on the previous
Unity capture. It is not a fresh Unity or gameplay screenshot and must never be used
as a runtime background. Environment sketches display at exact 2×; all board/gem pixels
stay at the previous capture's exact size. `Native/` retains editable study documents.

Verified: exact LibreSprite plan/export/reopen agreement; binary alpha; zero foliage
pixels in the 512×512 board; unchanged portrait, shared controls, actor/HP/countdown
pixels, and the full lower HUD region. All 8,125 protected files match their baseline.
No Unity code/import/serialization changes, Unity test run, or device test occurred.
See `Diagnostics/Validation.json` for measured results.

Artistic verdict: the removal of competing props and consolidation into two plant
silhouettes is visibly calmer. Final material polish, independent leaf lighting,
modular joints, responsive Unity behavior and full-screen safe-area coverage remain
future gates. No numerical style-match percentage is asserted.

## Stop and next checkpoint

Stopped at the attachment's first review gate: **art-direction board and asset plan**.
After the user approves this direction, proceed only to **checkpoint 2: top background
blockout**. Portrait, board, integrated Unity scene, and final validation each have
separate subsequent review stops. Do not integrate into Game or merge yet.
'''
(O/'README.md').write_text(plan,encoding='utf-8')
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Magical Forest v2 — direction review</title>
<style>*{box-sizing:border-box}body{margin:0;background:#111a20;color:#e1e9df;font:16px/1.5 system-ui,sans-serif}main{max-width:1540px;margin:auto;padding:28px}h1{font-size:32px;margin:0 0 6px}h2{font-size:22px;margin:28px 0 10px}p{max-width:1040px;color:#acbeb8}a{color:#aac5dc}button{background:#2b433b;color:#eff5ed;border:1px solid #668373;padding:10px 16px;border-radius:5px;cursor:pointer}button[aria-pressed=true]{background:#486a45}.bar{display:flex;gap:12px;flex-wrap:wrap;margin:16px 0}.pair{display:grid;grid-template-columns:1fr 1fr;gap:24px}.panel{background:#18252a;border:1px solid #344848;padding:12px;border-radius:7px;overflow:auto}.label{color:#ceded2;font-weight:600;margin-bottom:10px}.pic{display:block;width:100%;height:auto;image-rendering:pixelated}.native .pic{width:720px;max-width:none}.notes{display:grid;grid-template-columns:repeat(3,1fr);gap:16px}.note{padding:18px;border-top:3px solid #66845a;background:#192a2b}.badge{color:#bdd696;font:600 12px/1.2 system-ui;letter-spacing:1.5px;text-transform:uppercase}.motifs{display:flex;align-items:end;gap:30px;flex-wrap:wrap}.motifs img{image-rendering:pixelated;display:block}.sub{font-size:13px;color:#9bb1ab}table{border-collapse:collapse;width:100%}td,th{text-align:left;padding:10px;border-bottom:1px solid #344848}th{color:#bdd696}code{color:#d2dbad}@media(max-width:760px){.pair,.notes{grid-template-columns:1fr}main{padding:16px}}</style>
<main><span class="badge">Checkpoint 1 · direction candidate</span><h1>A quieter woodland sanctuary</h1><p>Strong timber, two connected foliage sweeps, and magic kept in the distance. This is a LibreSprite composition study using the previous Unity capture—not a new Unity or gameplay screenshot.</p>
<div class="bar"><button id="fit" aria-pressed="true">Fit comparison</button><button id="native" aria-pressed="false">Exact 1× capture pixels</button><a href="Previews/DirectionStudy.png">Open full-size PNG</a><a href="README.md">Asset plan and validation limits</a></div>
<div class="pair" id="compare"><section class="panel"><div class="label">Before · Stage 5 Unity assembly</div><img class="pic" src="../../ForestHybrid/Stage05/Previews/Unity-Full-720x1360.png" alt="Previous Forest with many separate board decorations"></section><section class="panel"><div class="label">Proposed direction · composition study</div><img class="pic" src="Previews/DirectionStudy.png" alt="Clean timber frame with two connected leaf sweeps and a cooler distant forest"></section></div>
<div class="notes"><div class="note"><b>Concentrate the plant shapes</b><p>Two connected leaf/vine accents replace scattered props. The board interior and most of the rails stay clear.</p></div><div class="note"><b>Let distance carry the magic</b><p>Cool layered forest, two small far lights, and upper lanterns. The flat enemy floor stays clean.</p></div><div class="note"><b>Protect the game’s hierarchy</b><p>Characters, gems, HP and the complete lower HUD remain pixel-exact. No UI layout changes.</p></div></div>
<h2>Board framing at review size</h2><div class="pair"><div class="panel"><div class="label">Before</div><img class="pic" src="Previews/BoardBefore.png"></div><div class="panel"><div class="label">Direction study</div><img class="pic" src="Previews/BoardDetail.png"></div></div>
<h2>Native motif studies</h2><p>These sketches establish larger shape grouping. Final tileable corners, independently lit lower foliage, and responsive fitting are later checkpoints.</p><div class="motifs panel"><figure><img src="Previews/LeafSweep_A.png" width="64" height="64"><figcaption class="sub">Leaf A · native 64×64</figcaption></figure><figure><img src="Previews/LeafSweep_A.png" width="256" height="256"><figcaption class="sub">Leaf A · nearest-neighbor 4×</figcaption></figure><figure><img src="Previews/LeafSweep_B.png" width="128" height="128"><figcaption class="sub">Leaf B · composition orientation 2×</figcaption></figure><figure><img src="Previews/TimberJoinStudy.png" width="126" height="126"><figcaption class="sub">Quiet timber joint · 6×</figcaption></figure></div>
<h2>Rendering authorities, unchanged</h2><div class="motifs panel"><figure><img src="Previews/Rattlebones.png" width="128" height="128"><figcaption>Rattlebones · primary · 2×</figcaption></figure><figure><img src="Previews/Farmer.png" width="224" height="176"><figcaption>Farmer · supporting · 2×</figcaption></figure><figure><img src="Previews/PanVillager.png" width="224" height="176"><figcaption>Pan Villager · supporting · 2×</figcaption></figure></div>
<h2>Asset decisions</h2><table><tr><th>Retain</th><th>Refine after direction approval</th><th>Exclude from new assembly</th></tr><tr><td>Tree silhouettes, distant ruin, flat ground</td><td>Far values and restrained magical accents</td><td>Loose hanging rune tag</td></tr><tr><td>Board geometry, cells, gems, modular architecture</td><td>Quiet timber joints and two leaf/vine overlays</td><td>Spiral medallions, board lamps, mushrooms, extra braces</td></tr><tr><td>Portrait placement and mask; shared controls and lower HUD</td><td>Portrait motif only at its own checkpoint</td><td>Any lower-HUD redesign</td></tr></table>
<p class="sub">New PixelLab spend: 0. LibreSprite exports and reopened sources verified. 8,125 protected files unchanged. No Unity rebuild or merge at this checkpoint. Next approval gate: top-background blockout.</p></main><script>const c=document.querySelector('#compare');for(const id of ['fit','native'])document.getElementById(id).onclick=()=>{c.classList.toggle('native',id==='native');document.getElementById('fit').setAttribute('aria-pressed',id==='fit');document.getElementById('native').setAttribute('aria-pressed',id==='native');};</script></html>'''
(O/'Review.html').write_text(html,encoding='utf-8')
shutil.copy2(__file__,O/'Scripts/ValidateAndPublish.py')
print(json.dumps(checks,indent=2))
