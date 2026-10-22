from pathlib import Path
from PIL import Image
import hashlib,json,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/SideMasonry';I=P/'ArtSource/DungeonHybrid/Integration'
test=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert test.get('passed')=='2' and test.get('failed')=='0'
profiles=[]
for path in (I/'Diagnostics/Baseline').glob('*-geometry.json'):
 before=json.loads(path.read_text());final=json.loads((I/'Diagnostics/Final'/path.name).read_text());candidate=json.loads((R/'Diagnostics/Candidate'/path.name).read_text())
 assert before==final==candidate,path.name
 profiles.append(path.name.replace('-geometry.json',''))
assert len(profiles)==5
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
assets=json.loads((R/'Modules.json').read_text())
for a in assets['parts']:
 png=R/'Exports'/(a['name']+'.png');assert sha(png)==a['sha256']
 assert sha(P/'Assets/_Game/Art/DungeonHybrid/SideMasonry'/png.name)==a['sha256']
out=dict(result='PASS',test_passed=2,test_failed=0,profiles=profiles,unchanged_layout=True,production_masonry_binding_changed=False,
 generation_cost=6,balance_after=925.2,reference_input='Authorized existing Rattlebones upload',
 source_silhouette_preserved=True,alpha='binary 0/255',module_count=4,source_palette_colors=12,clean_palette_colors=7)
(R/'Diagnostics/Verification.json').write_text(json.dumps(out,indent=2)+'\n')
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Dungeon side masonry review</title>
<style>body{background:#11121b;color:#eae3ee;margin:24px;font:16px system-ui}a{color:#d3a7ef}section{margin:20px 0;padding:18px;background:#202131;border-radius:10px}img{image-rendering:pixelated}.row{display:flex;gap:24px;flex-wrap:wrap}.scroll{overflow:auto}select{font:inherit;padding:8px;background:#302940;color:white}small{color:#c5b6cc}</style>
<h1>Dungeon masonry — PixelLab + LibreSprite</h1>
<p>The portrait banner is removed. New masonry is a review candidate, shown in actual Unity combat through an editor-only preview. Its production binding remains unchanged. Nothing is merged.</p>
<p><a href="Report.md">Report</a> · <a href="../Integration/Review.html">Current production preview</a> · <a href="https://github.com/PlatFll/Dungeon-Matcher/pull/205">Draft PR #205</a></p>
<section><h2>Actual Unity review</h2><label>Version <select id="v" onchange="show()"><option value="candidate">New stepped masonry candidate</option><option value="current">Current scene, portrait banner removed</option></select></label> <label>Size <select id="s" onchange="show()"><option>720x1280</option><option>1080x1920</option><option>1080x2400</option><option>safe-1080x2400</option><option>360x640</option></select></label>
<p><a id="png" href="Previews/Candidate/Dungeon-720x1280.png">Original screenshot</a></p><div class="scroll"><img id="shot" src="Previews/Candidate/Dungeon-720x1280.png" width="720" height="1280" alt="Masonry review in actual Dungeon combat"></div><small>Three live enemies, original gems, controls and health bars. First enemy and weaknesses are ordinary live game assignments. Screenshots are unedited Unity captures.</small></section>
<section><h2>Native modules (1×)</h2><div class="row">'''
for a in assets['parts']:
 html+=f'<div><p>{a["name"]}<br>{a["width"]}×{a["height"]}, {a["colors"]} colors</p><a href="Exports/{a["name"]}.png"><img src="Exports/{a["name"]}.png" width="{a["width"]}" height="{a["height"]}"></a></div>'
html+='''</div><h2>Raw / cleaned source (4×)</h2><div class="row"><div>PixelLab original<br><img src="Previews/Masonry_Raw_4x.png" width="512" height="512"></div><div>LibreSprite cleanup<br><img src="Previews/Masonry_Clean_4x.png" width="512" height="512"></div></div><p>One generated stepped wall became a paired surround with inward-lit edges and repeatable upper stone courses. Exact source mask preserved; palette reduced from 12 to 7 dungeon stone colors. Four editable LibreSprite modules; 6 generations used.</p></section>
<script>function show(){let size=document.getElementById('s').value,mode=document.getElementById('v').value,d=size.replace('safe-','').split('x');let url=(mode==='candidate'?'Previews/Candidate/':'../Integration/Previews/Final/')+'Dungeon-'+size+'.png';let i=document.getElementById('shot');i.src=url;i.width=+d[0];i.height=+d[1];document.getElementById('png').href=url;}</script></html>'''
(R/'Review.html').write_text(html,encoding='utf-8')
(R/'Report.md').write_text('''# Dungeon side masonry and portrait banner review

The user requested removing the skeleton banner inside the player portrait and generating side masonry inspired by the supplied mockup. The banner is removed from the current hybrid Dungeon only; the portrait recess, frame, character and live UI remain intact. Existing legacy fallback art is preserved.

## Masonry candidate

PixelLab Pro Flash produced one usable native stepped wall from the requested paired source (job `9d2ced30-1336-404f-aca8-2f0718130fe5`, source `457ee932-b079-5518-8311-d314c23c0d04`). Its current authorized Rattlebones image was passed as the rendering reference. The supplied mockup guided the inward-stepped stone silhouette visually; it was not transmitted. Only one source was generated: **6 generations**, balance 931.20 → 925.20.

LibreSprite maps 12 source colors to seven existing Dungeon stone colors, preserving the complete alpha silhouette. Four modules were exported and reopened exactly: left/right 48×86 bases, left/right 24×16 repeats. The opposite side uses a mirrored silhouette with inward-lit edges and shared top-lit planes. Repeat connection edges intentionally touch; the complete bases have two transparent pixels of clearance. All pixels remain native with binary alpha and Point/uncompressed imports. Raw source, request, lineage, editable ASE documents, reproducible cleanup script and measurements are retained.

The candidate is shown inside the actual Game scene by an editor-only review fixture. Existing side masonry is temporarily hidden there; modular repeats and stepped bases are drawn on the existing background canvas beneath the live board and HUD. No alternate gameplay/layout owner is added. The candidate is **not selected in production resources**; approve this appearance before replacing the currently approved masonry. All prior approved source assets remain unchanged.

## Verification

Two existing/reused live-scene capture tests pass: the banner-free current production scene and the new masonry review. Each captures 720×1280, 1080×1920, 1080×2400, a tall safe-area case, and 360×640, with Rattlebones and three live enemies. They check native image density, portrait masking, full base bounds, board/gem visibility, health/intent placement, Guide/Settings controls and an accepted swap. Board, section and health geometry match the pre-integration baseline in both views at every size. The artwork's pixel/alpha/source checks pass independently.

`Diagnostics/UnityTests.xml` and `Verification.json` hold the current evidence; the compiler receipt is recorded separately after final validation. These are graphics-enabled Unity checks, not physical-device testing. No new Android build was performed.

Open `Review.html` for current/candidate screenshots, native exports and raw/clean comparison. Dedicated branch `feature/dungeon-zone-hybrid-pixellab-rebuild`, draft PR #205. Stop for the user's masonry review; do not merge.
''',encoding='utf-8')
print('PASS: both live views retain identical geometry at all five sizes; review gallery and report written.')
