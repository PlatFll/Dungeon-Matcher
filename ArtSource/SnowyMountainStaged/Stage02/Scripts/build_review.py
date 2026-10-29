from pathlib import Path
from PIL import Image, ImageDraw
import json,hashlib,shutil
F=Path(r"C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher")
S=F/"ArtSource/SnowyMountainStaged/Stage02"
native=json.loads((S/"QA/Native.json").read_text())
for n in ["A","B"]:
 im=Image.open(S/"Raw"/(n+".png")).convert("RGBA")
 im.resize((768,640),Image.Resampling.NEAREST).save(S/"Preview"/("Raw"+n+"-2x.png"))
im=Image.open(S/"Exports/AlpinePass.png").convert("RGBA")
im.resize((768,640),Image.Resampling.NEAREST).save(S/"Preview/AlpinePass-2x.png")
target=Image.open(F/"ArtSource/SnowyMountainStaged/Pack/references/00_TARGET_SNOWY_MOUNTAIN_SCENE.png")
target.crop((0,0,941,730)).save(S/"Preview/Target-top.png")
palette=sorted(set(im.getdata()),key=lambda c:sum(c[:3]))
board=Image.new("RGB",(len(palette)*18,32),"#0a0d11");d=ImageDraw.Draw(board)
for i,c in enumerate(palette):d.rectangle((18*i,0,18*i+17,31),fill=c[:3])
board.save(S/"Preview/Palette.png")
ledger=json.loads((S/"Generation.json").read_text())
ledger.update(endingBalance=4950,totalSpent=50,completedJobs=2,selected="B",status="Candidate for Stage 2 user review")
for n,job in zip(["A","B"],ledger["jobs"]):
 job["outputSHA256"]=hashlib.sha256((S/"Raw"/(n+".png")).read_bytes()).hexdigest()
(S/"Generation.json").write_text(json.dumps(ledger,indent=2),encoding="utf-8")
html="""<!doctype html><meta charset="utf-8"><title>Snowy Mountain • Stage 2</title>
<style>
body{background:#0d1723;color:#e0eef7;font:16px system-ui;margin:24px;max-width:1600px}
h1{font-size:26px}p{max-width:1050px;line-height:1.6}
nav,button,select{padding:10px;background:#203344;color:#e0eef7;border:1px solid #486278;border-radius:5px}
a{color:#a7e7ff} .row{display:flex;gap:24px;flex-wrap:wrap;align-items:flex-start}
figure{margin:0 0 24px;background:#152332;padding:16px;border-radius:9px;max-width:100%;box-sizing:border-box}
figcaption{margin-bottom:12px}img{image-rendering:pixelated;max-width:100%;object-fit:contain}
.shot{width:360px}.large .shot{width:540px}.asset{width:384px}.zoom .asset{width:768px}
.note{color:#b3c7d6}.badge{color:#a5e4c2}
</style>
<h1>Snowy Mountain — Stage 2 battle backdrop</h1>
<p class="badge">Review candidate • isolated branch • no merge • production Frostwake binding preserved</p>
<p>The selected sheltered pass adds snow shelves, icicles, pines, a distant stone bridge and ruined towers.
PixelLab generated two text-only 384×320 candidates. Candidate B received a LibreSprite cleanup from 40 to 28 colors, with no resampling.
The before/after images below are actual Unity Game-scene captures with three live enemies and Rattlebones.
The current floor, portrait, board, buttons and lower HUD remain unchanged; their later stages have not begun.</p>
<nav>Phone capture: <select id="size">
<option>720x1280</option><option>1080x1920</option><option>1080x2400</option><option>1080x2400-safe</option><option>360x640</option>
</select> <button onclick="document.getElementById('screens').classList.toggle('large')">Larger display</button>
 · <a href="Preview/Measurements.json">Unity measurements</a> · <a href="QA/Native.json">Native QA</a> · <a href="Generation.json">Generation ledger</a></nav>
<p class="note">Screenshots are reduced to fit this page. Open the original image for full-resolution inspection. Source assets below include true 1× views.</p>
<div class="row" id="screens">
<figure><figcaption>Before — existing Frostwake scenery</figcaption><a id="beforeLink"><img id="before" class="shot"></a></figure>
<figure><figcaption>Revised — selected pass in Unity</figcaption><a id="afterLink"><img id="after" class="shot"></a></figure>
</div>
<h2>Selected native artwork</h2>
<button onclick="document.getElementById('clean').classList.toggle('zoom')">Toggle 1× / 2× nearest-neighbor</button>
<div id="clean" class="row"><figure><figcaption>Cleaned B • 384×320 • 28 opaque colors • LibreSprite</figcaption>
<a href="Exports/AlpinePass.png"><img class="asset" src="Exports/AlpinePass.png"></a><p><a href="Native/AlpinePass.ase">Editable LibreSprite file</a></p></figure>
<figure><figcaption>Actual golden rendering references, unchanged at native size</figcaption>
<img src="../Stage01/GoldenReferences/Rattlebones.png"><img src="../Stage01/GoldenReferences/Farmer.png"><img src="../Stage01/GoldenReferences/PanVillager.png">
<p class="note">Rendering discipline only; environment hues remain alpine.</p><img src="Preview/Palette.png"></figure></div>
<h2>Raw PixelLab exploration</h2>
<div class="row">
<figure><figcaption>A — sweeping bridge approach, retained alternative</figcaption><a href="Raw/A.png"><img src="Raw/A.png" width="384"></a></figure>
<figure><figcaption>B — sheltered pass, selected before cleanup</figcaption><a href="Raw/B.png"><img src="Raw/B.png" width="384"></a></figure>
</div>
<h2>Visual target</h2><figure><figcaption>Supplied mockup, upper-region reference crop only</figcaption><img src="Preview/Target-top.png" width="600"></figure>
<p>PixelLab usage: 2 Pro calls × 25 = <strong>50 subscription generations</strong>. Balance: 4,950 remaining. No reference-image uploads.</p>
<script>
const size=document.getElementById('size');
function change(){for(const name of ['before','after']){let p='Preview/'+(name==='before'?'Before':'After')+'/'+size.value+'.png';document.getElementById(name).src=p;document.getElementById(name+'Link').href=p;}}
size.onchange=change;change();
</script>"""
(S/"Review.html").write_text(html,encoding="utf-8")
print("Gallery and native preview assets prepared.")
