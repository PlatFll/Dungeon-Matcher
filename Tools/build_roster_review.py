"""Build an offline review from actual Unity captures and untouched native sources."""
from pathlib import Path
from PIL import Image, ImageDraw
import html

root=Path(__file__).resolve().parents[1]
out=root/'.utmp/RosterEndless'
def esc(value): return html.escape(str(value),quote=True)
sections=[]
def cards(title,paths,description=''):
    items=[]
    for label,path in paths:
        items.append(f'<article><h3>{esc(label)}</h3><a href="{esc(path)}"><img loading="lazy" src="{esc(path)}"></a></article>')
    sections.append(f'<h2>{esc(title)}</h2><p>{esc(description)}</p><section>'+''.join(items)+'</section>')
for folder,prefix in [('Court','tribute-fortified'),('Forest','roots-warded')]:
    paths=[(size,f'{folder}/{prefix}-{size}.png') for size in ['720x1280','1080x1920','1080x2400','safe-inset']]
    cards(folder+' — actual Unity views',paths,'Click to inspect original resolution. Editor audio muted; safe area is simulated.')
    sheet=Image.new('RGB',(1080,645),'#191d26');draw=ImageDraw.Draw(sheet)
    for index,(label,path) in enumerate(paths):
        im=Image.open(out/path);im.thumbnail((270,610),Image.Resampling.NEAREST)
        draw.text((index*270+8,8),label,fill='white');sheet.paste(im,(index*270,30))
    sheet.save(out/(folder+'-Review.png'))
frames=sorted((out/'Court/Motion').glob('*.png'))
if frames:
    images=[]
    for path in frames:
        im=Image.open(path).convert('RGB');im.thumbnail((540,960),Image.Resampling.NEAREST);images.append(im)
    images[0].save(out/'Court/royal-wake.gif',save_all=True,append_images=images[1:],duration=40,loop=0)
cards('Royal pearl: armor, slide and exposed state',[(label,'Court/'+file) for label,file in
    [('Armor','royal-armored.png'),('Captured engine motion','royal-wake.gif'),('Exposed','royal-exposed.png')]],
    'GIF is a sampled Editor capture for review; gameplay keeps the authored 0.24-second slide. The live sequence settles only after the rotation.')
cards('Native pixel candidates',[
    ('Pearl family: 1× and 3×','../../ArtSource/DrownedCourt/RosterRevision/PearlReview.png'),
    ('Root variants','../../ArtSource/Forest/RosterRevision/RootVariantReview.png')],
    'Direct pixel authoring, zero PixelLab generations. Individual dimensions, palettes, alpha and SHA-256 hashes are recorded in source JSON; composites are visual comparisons only.')
cards('King Judgment source motion',[(p.stem,'../../'+p.relative_to(root).as_posix()) for p in (root/'ArtSource/EnemyAttacks').rglob('*Judgment*.gif')],
    'New strikes and heavy finisher retain the approved King identity. New art is awaiting user visual review.')
cards('King finisher — actual Unity contact',[(p.stem,p.relative_to(out).as_posix()) for p in (out/'Judgment').glob('*.png')])
for zone in ['dungeon','magical-forest','drowned-court']:
    cards(zone+' — wave 150 and Continue',[(s,zone+'-'+s+'.png') for s in ['720x1280','1080x1920','1080x2400','safe-inset']],
          'Scaling slice capture, taken before the final Forest panel/status-lane refinements shown above.')
style='body{background:#191d26;color:#f5efdf;font:16px system-ui;margin:24px auto;max-width:1440px;padding:0 20px}section{display:flex;gap:16px;align-items:start;flex-wrap:wrap}article{flex:1;min-width:240px;max-width:680px;background:#252b37;padding:12px;border-radius:8px}img{width:100%;height:auto;image-rendering:pixelated}h2{margin-top:40px}h3{font-size:16px}a{color:#9be6ff}p{max-width:1000px;line-height:1.5}'
(out/'Review.html').write_text('<!doctype html><meta charset="utf-8"><title>Roster and endless revision review</title><style>'+style+'</style><h1>Roster and endless revision</h1><p>Implemented on the feature branch for review. No merge. These are actual Unity renders plus native source previews; they do not establish human pacing or physical-device approval.</p>'+''.join(sections),encoding='utf-8')
print(out/'Review.html')
