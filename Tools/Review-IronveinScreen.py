"""Assemble links to real Unity captures and original native material PNGs."""
import html,json,shutil
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
out=ROOT/'.utmp/Ironvein/ScreenReview';out.mkdir(parents=True,exist_ok=True)
def copy(path,name):
    shutil.copy2(path,out/name);return html.escape(name)
items=[]
for p in sorted((ROOT/'.utmp/Ironvein/Captures').glob('phase09-*.png')):
    name=copy(p,p.name);items.append(f'<figure><a href="{name}"><img class="screen" src="{name}"></a><figcaption>{html.escape(p.stem)} · actual Unity capture</figcaption></figure>')
mock=ROOT/'.utmp/IronveinPack/Dungeon_Matcher_Ironvein_Excavation_Astra_Pack/references/Ironvein_Full_Screen_Mockup_CONCEPT.png'
name=copy(mock,'CompositionTarget.png')
items.append(f'<figure><a href="{name}"><img class="screen" src="{name}"></a><figcaption>Supplied composition target; not native source art</figcaption></figure>')
materials=[]
for family in ('Scene','Mechanics'):
    for p in sorted((ROOT/'ArtSource/Ironvein'/family).glob('*/00.png')):
        if p.parent.name in {'cave_floor','tunnel'}:continue
        name=copy(p,f'{family}-{p.parent.name}.png')
        info=json.loads((p.parent/'technical.json').read_text())[0]
        materials.append(f'<figure><img src="{name}"><figcaption>{html.escape(p.parent.name)} · {info["size"]} · {info["colors"]} visible colors</figcaption></figure>')
for p in sorted((ROOT/'ArtSource/Ironvein/UI/Prepared').glob('*.png')):
    if '-raw' in p.name or p.stem=='BackdropDim':continue
    name=copy(p,'UI-'+p.name);materials.append(f'<figure><img src="{name}"><figcaption>{p.stem} · native PNG</figcaption></figure>')
doc='''<!doctype html><meta charset="utf-8"><title>Ironvein screen review</title>
<style>body{background:#181c22;color:#f5e9d2;font:16px system-ui;margin:32px}h1,h2{color:#efb363}.grid{display:flex;flex-wrap:wrap;gap:16px;align-items:start}figure{margin:0;padding:16px;background:#343941;border-radius:8px;max-width:440px}img{image-rendering:pixelated}.screen{width:270px;height:auto}figcaption{margin-top:12px;max-width:270px}p{max-width:1000px}a{color:#a9d6ff}</style>
<h1>Ironvein Excavation — screen and materials</h1>
<p>Production candidates for visual review. Actual Unity captures at the four required sizes; click for full resolution. Compare composition with the supplied mockup, not pixel identity. Player, skills and six gems remain the existing assets. Music is an original temporary cue awaiting listening review.</p>
<div class="grid">'''+''.join(items)+'''</div><h2>Native materials at 1×</h2><p>Individual PNG dimensions, alpha, hashes and palette counts are recorded beside each source. This gallery alone does not certify them. Original rejected variants remain source provenance.</p><div class="grid">'''+''.join(materials)+'</div>'
(out/'Review.html').write_text(doc,encoding='utf-8')
print(json.dumps({'gallery':str(out/'Review.html'),'screens':len(items),'materials':len(materials)}))
