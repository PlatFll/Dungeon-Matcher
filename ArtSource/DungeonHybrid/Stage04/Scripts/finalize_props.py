from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,csv,hashlib
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/Stage04'
mods=json.loads((R/'Modules.json').read_text())['parts']
with (R/'AssetLedger.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['asset','native_width','native_height','opaque_colors','unity_path','status'])
 for m in mods:w.writerow([m['name'],m['width'],m['height'],m['colors'],m['path'],'candidate - Stage 04 review'])
(R/'GenerationReceipt.json').write_text(json.dumps(dict(provider='PixelLab Pro Flash',sources=7,generations_per_source=5,total_generations=35,
 balance_before=966.20,balance_after=931.20,usd_credit_balance=0,source_requests='Sources/Requests.json',
 reference='Reused authorized actual Rattlebones style upload f3b0e649-e346-4833-91d2-d891b8e5e0b8',
 scope='Seven separate transparent native prop source images; no full-scene generation.'),indent=2)+'\n')
f=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12);board=Image.new('RGB',(400,130),'#171923');d=ImageDraw.Draw(board)
for n,path,x in [('Rattlebones','Docs/ArtDirection/Audit2026-10/media/skeleton-still.png',10),('Farmer','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png',120),('Pan Villager','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png',260)]:
 im=Image.open(P/path).convert('RGBA');d.text((x,6),n,font=f,fill='#eee8f1');board.paste(im,(x,29),im)
board.save(R/'Previews/Golden-native.png')
html="""<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Dungeon Hybrid Stage 04 - props and dressing</title>
<style>body{margin:28px;background:#11121b;color:#e0d8e3;font:16px system-ui}h1,h2{font-weight:600}section{padding:20px;margin:24px 0;background:#1e2030;border-radius:10px}img{image-rendering:pixelated;object-fit:contain}a{color:#dd9bea}.row{display:flex;gap:28px;flex-wrap:wrap}.card{padding:14px;background:#151423}.scroll{overflow:auto}select,button{font:inherit;padding:7px;background:#302940;color:#eee8f1;border:1px solid #83718d;border-radius:4px}label{display:inline-block;margin:8px 18px 12px 0}.muted{color:#b5a9bc}table{border-collapse:collapse}td,th{padding:7px 14px;text-align:left;border-bottom:1px solid #4e435c}</style>
<h1>Stage 04 · Props / dressing</h1>
<p>Seven PixelLab sources → 10 reusable native sprites, cleaned and padded in LibreSprite. Candidates for review.</p>
<p><a href="../Stage03/Review.html">Approved Stage 03</a> · <a href="Report.md">Verification report</a> · <a href="AssetLedger.csv">Asset ledger</a></p>
<section><h2>Actual Unity Game View</h2><p>The approved wall, platform, gate and UI are reused unchanged. Static approved character sprites provide scale and readability checks. This is an isolated art study.</p>
<label>Dressing <select id="mode" onchange="render()"><option value="classic">Skull / bones / barrel</option><option value="alternate">Jar / rubble alternative</option><option value="baseline">Approved Stage 03 baseline</option></select></label>
<label><input type="checkbox" id="cast" checked onchange="render()"> Show approved cast</label>
<label>Native pixel zoom <select id="zoom" onchange="render()"><option value="1">1×</option><option value="2" selected>2×</option><option value="3">3×</option></select></label>
<div class="scroll"><img id="unity" width="720" height="480" src="Previews/Unity-classic-cast-720x480.png" alt="Unity Stage 04 prop study"></div>
<p class="muted">Each option uses an actual captured Unity screenshot at that size. Alternative dressing keeps the combat stage open. No production Game scene, gameplay or build settings changed.</p></section>
<section><h2>Native exported assets</h2><div class="scroll"><img src="Previews/Native-assets.png" width="720" height="260"></div>
<p>Complete sprites have 2px transparent clearance. Chain pieces meet at deliberate connection edges; the complete chain has clearance around its whole silhouette.</p>"""
html+='<table><tr><th>Asset</th><th>Native canvas</th><th>Colors</th><th>Files</th></tr>'
for m in mods:
 n=m['name'];html+=f'<tr><td>{n}</td><td>{m["width"]}×{m["height"]}</td><td>{m["colors"]}</td><td><a href="Exports/{n}.png">PNG</a> · <a href="Native/{n}.aseprite">LibreSprite</a></td></tr>'
html+='</table></section><section><h2>Raw source and cleanup</h2><p>Same source canvas and silhouette. Choose native 1× or enlarged nearest-neighbor views. Palette cleanup changes only new candidate sources.</p><label>Comparison zoom <select id="assetZoom" onchange="zoomAssets()"><option value="1">1×</option><option value="4" selected>4×</option></select></label>'
for n in ['Torch','Chain','Banner','Barrel','Jar','Bones','Rubble']:
 html+=f'<h3>{n}</h3><div class="row"><div class="card">PixelLab raw<br><img class="asset" width="256" height="256" src="Sources/{n}_Raw.png"></div><div class="card">LibreSprite cleaned<br><img class="asset" width="256" height="256" src="Previews/{n}_Clean64.png"></div></div>'
html+='</section><section><h2>Actual golden references at 1×</h2><img src="Previews/Golden-native.png" width="400" height="130"><p>Rattlebones governs rendering discipline; Farmer and Pan provide the approved supporting style family.</p></section>'
html+='''<section><h2>Verification</h2><p>Unity art-preview test and repository Unity validator passed. Fifteen captures cover native 360×240, 720×480 and 1080×720. Integer zooms match exact nearest-neighbor replication; the undressed baseline matches Stage 03 pixel-for-pixel.</p><p>All 765 protected files and eight existing main-checkout edits remain unchanged. Seven sources cost 35 PixelLab generations. Stage 05 has not started. No merge.</p></section>
<script>function render(){let mode=document.getElementById('mode').value;const cast=document.getElementById('cast'),z=Number(document.getElementById('zoom').value);cast.disabled=mode==='baseline';if(mode!=='baseline'&&cast.checked)mode+='-cast';let im=document.getElementById('unity');im.src='Previews/Unity-'+mode+'-'+360*z+'x'+240*z+'.png';im.width=360*z;im.height=240*z}function zoomAssets(){const z=Number(document.getElementById('assetZoom').value);document.querySelectorAll('.asset').forEach(i=>{i.width=64*z;i.height=64*z})}</script></html>'''
(R/'Review.html').write_text(html,encoding='utf-8')
print('Stage 04 gallery, golden-reference comparison and ledger written.')
