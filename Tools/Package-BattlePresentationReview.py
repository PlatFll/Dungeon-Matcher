"""Package original Unity captures and native art links; never edit raster pixels."""
import hashlib, html, json, shutil, subprocess
from pathlib import Path
from PIL import Image

root=Path(__file__).resolve().parent.parent
art=root/'ArtSource/BattlePresentation'
review=art/'review'
evidence=root/'.utmp/ThreeZonePresentation'
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def keep(source,destination):
    destination.parent.mkdir(parents=True,exist_ok=True)
    shutil.copyfile(source,destination)
zones=[('dungeon','Dungeon','DUNGEON_ZONE_TARGET'),('magical-forest','Magical Forest','FOREST_ZONE_TARGET'),('drowned-court','Drowned Court','DROWNED_COURT_TARGET')]
sections=[]
for key,label,target in zones:
    for stage in ('before','after'):
        keep(evidence/stage/f'{key}-1080.png',review/stage/f'{key}.png')
    for size in ('720x1280','1080x2400'):
        keep(evidence/'after'/f'{key}-{size}.png',review/'sizes'/f'{key}-{size}.png')
    for path in (evidence/'after').glob(key+'-*.txt'):
        keep(path,review/'validation'/path.name)
    for path in (evidence/'after'/f'{key}-motion').glob('*.png'):
        keep(path,review/'motion'/key/path.name)
    figures=''.join(f'<figure><figcaption>{title}</figcaption><a href="{url}"><img src="{url}" alt="{label} {title}"></a></figure>' for title,url in [
        ('Before · Unity',f'review/before/{key}.png'),('After · Unity',f'review/after/{key}.png'),('User target',f'brief/references/{target}.png')])
    sections.append(f'<section><h2>{label}</h2><div class="compare">{figures}</div><p>Additional Unity views: <a href="review/sizes/{key}-720x1280.png">720 × 1280</a> · <a href="review/sizes/{key}-1080x2400.png">1080 × 2400 with safe-area inset</a></p><details><summary>Ambient preview · actual Unity capture sequence</summary><p>24 screenshots, sampled every 150 ms. Other actor idle animation is also present.</p><button data-zone="{key}">Play / pause</button><br><img class="motion" data-zone="{key}" src="review/motion/{key}/00.png" alt="{label} ambient capture"></details></section>')

characters=[]
for path in sorted((evidence/'characters').glob('*.png')):
    keep(path,review/'characters'/path.name)
    characters.append(f'<a href="review/characters/{path.name}">{html.escape(path.stem)}</a>')
checks=json.loads((art/'native-checks.json').read_text())
assets=[]
for asset in checks['assets']:
    name=asset['asset'];w,h=asset['size']
    assets.append(f'<figure><figcaption>{name} · {w} × {h}</figcaption><a href="native/{name}"><img src="native/{name}" width="{w}" height="{h}" alt="{name}"></a></figure>')
page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Three-zone battle presentation review</title><style>
body{margin:0 auto;max-width:1400px;padding:24px;background:#121621;color:#ecedf4;font:16px/1.55 system-ui}a{color:#ace2ff}h1,h2{line-height:1.2}section{margin:36px 0;border-top:1px solid #46536b;padding-top:20px}.compare{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:14px}figure{margin:0}figcaption{font-weight:600;margin-bottom:8px}.compare img{width:100%;height:auto;image-rendering:pixelated}.motion{width:360px;max-width:100%;image-rendering:pixelated}.native{display:flex;gap:20px;flex-wrap:wrap}.native figure{max-width:100%;overflow:auto;background:#333844;padding:8px}.native img{image-rendering:pixelated;max-width:none}button{padding:10px 20px;background:#38465d;color:white;border:1px solid #8995ab;cursor:pointer}summary{cursor:pointer}li{margin:6px 0}.notice{background:#273247;padding:15px 20px;border-left:4px solid #70bdd6}@media(max-width:700px){body{padding:12px}.compare{gap:5px}figcaption{font-size:12px}}
</style><h1>Three-zone battle presentation</h1><p>Implemented review candidate · 14 October 2026 · Unity 6000.3.19f1 · audio muted during validation.</p><div class="notice">The comparisons show actual Unity captures. Characters, gems, cell art and gameplay are preserved. The mockups are visual targets, not runtime screenshots or proof of exact native asset size.</div>
'''+''.join(sections)+'''
<section><h2>Remaining visual differences</h2><ul>
<li>The live game keeps its existing eight-by-eight board, battle-area proportions, health bars, counters, combat hints and Thaleah labels. Court retains the functional wave counter.</li>
<li>Dungeon masonry and floor repeat more visibly than the target. The Forest uses the approved woodland/tree modules and a narrower, simpler waterfall. Court uses a reconstructed statue and reused court masonry; its water depth, large ruins and stone palette are simpler than the target.</li>
<li>All scenes are assembled native scenery, rather than exact replicas of every background object. Target costumes, characters and gems were deliberately not copied.</li>
<li>The shared bottom HUD retains the existing layout and accessibility feedback. Its outer silver beam is simpler than the target’s ornamental construction. Side dressing hides when a viewport has insufficient board gutter.</li>
<li>Automated checks and desktop Unity captures do not establish physical-device performance or user art approval.</li>
</ul><h2>Playable ability symbols</h2><p>All five existing symbols were checked inside the shared frame: '''+' · '.join(characters)+'''</p>
<h2>Native asset exports</h2><p>57 editable LibreSprite sources. These views use the files' real native dimensions; click to inspect the original PNG. <a href="native-checks.json">Exact measurements and import checks</a> · <a href="README.md">Implementation / validation notes</a>.</p><div class="native">'''+''.join(assets)+'''</div></section>
<script>const playing={};document.querySelectorAll('button[data-zone]').forEach(b=>b.onclick=()=>{const z=b.dataset.zone;if(playing[z]){clearInterval(playing[z]);delete playing[z];return;}let n=0;const img=document.querySelector('img.motion[data-zone="'+z+'"]');playing[z]=setInterval(()=>{n=(n+1)%24;img.src='review/motion/'+z+'/'+String(n).padStart(2,'0')+'.png'},150)});</script></html>'''
(art/'Review.html').write_text(page,encoding='utf-8')

paths=[*sorted((art/'brief/references').glob('*.png')),
       root/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt',root/'Docs/ArtDirection/Golden_Reference_Register.md',
       root/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png',
       root/'Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png',root/'Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png',
       root/'Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png',root/'Assets/_Game/Art/CombatIdles/Farmer_Idle.png',root/'Assets/_Game/Art/CombatIdles/PanVillager_Idle.png',
       *[root/'ArtSource/Forest/Production/Selected/Environment'/name for name in ('Tall_Woodland.png','Ancient_Tree.png','Distant_Canopy.png','Woodland_Ground.png','Forest_Fern.png')],
       root/'Assets/_Game/Art/DrownedCourt/court_background.png',root/'Assets/_Game/Art/DrownedCourt/court_floor.png']
references=[dict(path=p.relative_to(root).as_posix(),sha256=sha(p)) for p in paths]
goldens={'skeleton-still.png':'705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c','farmer-still.png':'6189bc2cfe01bab84bafada24a7b129a3fd8c2225f629eb8580386f461109e2d','pan_villager-still.png':'e1be89f8231483df5db6739dd2cae7bef83e11aaa9b028e4cb1b60ea55aba0ca'}
for p in paths:
    if p.name in goldens: assert sha(p)==goldens[p.name],p
manifest=dict(status='implemented review candidate; unmerged; no new golden memberships',
    baseCommit='a0aafe53e22f2efe7cbf0319b9ce4a0de1e8fe3b',nativeAssets=57,
    references=references,pixelLab=dict(generations=1,selectedOutputs=0,rejectedOutputs=1,creditsPurchased=0,remainingSubscription=0,remainingDailyBonus=4,receipt='generation/statue-job.json'),
    method='Native LibreSprite reconstruction, extracted material/ornament studies, hand-authored ambient frames, recomposed existing environment modules. No full mockup runtime texture.',
    nativeFinalization='Tools/Build-BattlePresentation.ps1; exact reopened cel comparison; layer flag normalization; LibreSprite PNG export',
    captures=[dict(path=p.relative_to(art).as_posix(),sha256=sha(p)) for p in sorted(review.rglob('*.png'))])
(art/'provenance.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(f'Packaged {len(manifest["captures"])} original screenshots and 57 native asset links. Golden image hashes verified.')
