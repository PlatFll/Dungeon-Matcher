"""Compose review-only layouts from verified native exports. Never edits game art."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,shutil,hashlib,re
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
qa=json.loads((HERE/'QA/NativeValidation.json').read_text())
fontfile=ROOT/'Assets/_Game/Fonts/Thaleah/ThaleahFat.ttf'
shutil.copy2(fontfile,HERE/'Preview/ThaleahFat.ttf')
shutil.copy2(fontfile.parent/'ATTRIBUTION.md',HERE/'Preview/FontAttribution.md')
ui=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',18)
small=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',14)
title=ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf',24)
pixel=ImageFont.truetype(str(fontfile),32)
styles=[('A','Royal bevel','A strong, broad bevel; the clearest mobile silhouette. Recommended.',30),
        ('B','Moonlit filigree','A slimmer silver rim with a delicate skull-and-scroll pendant.',24),
        ('C','Forged relic','Thicker faceted silver and a more compact corner-studded gear.',30)]
def load(path):return Image.open(HERE/path).convert('RGBA')
def label(im,text,cy):
    out=im.copy();d=ImageDraw.Draw(out);d.fontmode='1';b=d.textbbox((0,0),text,font=pixel)
    d.text(((out.width-(b[2]-b[0]))//2-b[0],cy-(b[3]-b[1])//2-b[1]),text,font=pixel,fill='#FFF9F0')
    return out
def stamp(board,im,x,y,scale=1):
    board.alpha_composite(im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST),(x,y))
overview=Image.new('RGBA',(1050,960),'#11121D');d=ImageDraw.Draw(overview)
d.text((28,20),'SILVER ROYAL RELIC  /  STAGE 1',font=title,fill='#F3F0E8')
d.text((28,56),'PixelLab + LibreSprite · style candidates only · no game changes',font=ui,fill='#B5B7C7')
cards=[]
for i,(key,name,verdict,cy) in enumerate(styles):
    wave=load(f'Cleaned/{key}_Wave.png');settings=load(f'Cleaned/{key}_Settings.png');demo=label(wave,'WAVE 1',cy)
    demo.save(HERE/f'Preview/{key}_Wave-Label.png')
    # Isolated panel corner crop illustrates the generated border treatment.
    corner=wave.crop((0,10,min(40,wave.width),min(48,wave.height)))
    corner.save(HERE/f'Preview/{key}_Border.png')
    y=95+i*280
    d.rounded_rectangle((20,y,1030,y+268),radius=12,fill='#1C1D2B')
    d.text((36,y+12),f'{key}  {name}',font=title,fill='#F3F0E8')
    stamp(overview,demo,36,y+53,3);stamp(overview,settings,620,y+53,3)
    d.text((808,y+18),'Native 1×',font=small,fill='#B5B7C7');stamp(overview,demo,808,y+45);stamp(overview,settings,860,y+113)
    d.text((36,y+244),verdict,font=small,fill='#B5B7C7')
    wq=qa[f'{key}_Wave'];sq=qa[f'{key}_Settings']
    cards.append(f'''<article id="{key}"><header><div><span class="letter">{key}</span><h2>{name}</h2></div><p>{verdict}</p></header>
    <div class="native"><span class="caption">Native 1× — actual exported pixels</span><div class="pair"><div class="wave" data-key="{key}" data-width="{wave.width}" data-height="{wave.height}" data-cy="{cy}" style="width:{wave.width}px;height:{wave.height}px"><img src="Cleaned/{key}_Wave.png" width="{wave.width}" height="{wave.height}" alt="{name} empty wave frame"><span class="wave-label" style="top:{cy-9}px">WAVE 1</span></div><img src="Cleaned/{key}_Settings.png" width="56" height="56" alt="{name} settings button"></div></div>
    <div class="enlarged"><div class="pair"><div class="wave" data-key="{key}" data-width="{wave.width}" data-height="{wave.height}" data-cy="{cy}"><img src="Cleaned/{key}_Wave.png" alt="{name} enlarged wave"><span class="wave-label">WAVE 1</span></div><img class="setting" src="Cleaned/{key}_Settings.png" alt="{name} enlarged settings"></div></div>
    <p class="meta">Wave {wave.width}×{wave.height} · {wq['color_count']} colors &nbsp; / &nbsp; Settings 56×56 · {sq['color_count']} colors · hard transparency</p>
    <details><summary>Border detail, palette and source files</summary><div class="detail"><img src="Preview/{key}_Border.png" width="240" height="228" alt="Enlarged silver border study"><div><p>Top-left silver highlights, shaded lower bevels and small gold focal points.</p><div class="swatches">{''.join('<span title="'+c+'" style="background:'+c+'"></span>' for c in wq['colors'])}</div><p><a href="Cleaned/{key}_Wave.png">Wave PNG</a> · <a href="Editable/{key}_Wave.ase">Editable wave</a><br><a href="Cleaned/{key}_Settings.png">Settings PNG</a> · <a href="Editable/{key}_Settings.ase">Editable settings</a></p></div></div></details></article>''')
overview.convert('RGB').save(HERE/'Preview/Candidates.png')
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Silver Royal Relic · Stage 1</title><style>
@font-face{font-family:Thaleah;src:url('Preview/ThaleahFat.ttf')}*{box-sizing:border-box}body{margin:0;background:#10111a;color:#eeeef3;font:16px/1.5 system-ui,sans-serif}main{max-width:1120px;margin:auto;padding:36px 24px}h1{font-size:36px;line-height:1.15;margin:8px 0 16px}h2{font-size:24px;display:inline;margin:0}.eyebrow{color:#d8bb6a;letter-spacing:.14em;font-size:12px}p{color:#b8b9ca;max-width:850px}a{color:#e2d197}button,input,select{font:inherit;color:inherit;background:#292a3d;border:1px solid #626379;border-radius:6px;padding:6px 10px}button{cursor:pointer}.toolbar{display:flex;flex-wrap:wrap;gap:16px;align-items:center;background:#1c1d2b;padding:14px;border-radius:10px;position:sticky;top:0;z-index:2}.toolbar input{width:95px}.letter{display:inline-block;color:#10111a;background:#e3d6a3;border-radius:5px;padding:0 10px;margin-right:14px}article{margin-top:24px;border:1px solid #3b3d50;border-radius:12px;overflow:hidden;background:#1c1d2b}article header{padding:22px 24px 0}article header p{margin:9px 0 16px}.native,.enlarged{padding:16px 24px;background:var(--surface,#151622);overflow:auto}.native{border-top:1px solid #303244;border-bottom:1px solid #303244}.caption{font-size:12px;color:#9397ae;display:block;margin-bottom:10px}.pair{display:flex;align-items:center;gap:24px;width:max-content;min-height:60px}.wave{position:relative;flex:none}.wave img{display:block;width:100%;height:100%}.wave-label{position:absolute;left:0;width:100%;text-align:center;font:32px/1 Thaleah;pointer-events:none;color:#fff9f0;white-space:nowrap}.native .wave-label{transform:translateY(-7px)}img{image-rendering:pixelated}.meta,details{padding:0 24px 18px;font-size:14px}.meta{margin:10px 0 0}summary{cursor:pointer;color:#dfd2a4}.detail{display:flex;gap:26px;align-items:center;flex-wrap:wrap;margin-top:18px}.swatches{display:flex;flex-wrap:wrap;gap:4px}.swatches span{width:24px;height:24px;border:1px solid #5c5a66}.reference{width:100%;height:auto;image-rendering:auto}.refs{display:flex;flex-wrap:wrap;align-items:center;gap:24px;background:#222535;padding:24px}.refs img{object-fit:contain;image-rendering:pixelated}.note{border-left:3px solid #bda25f;padding-left:16px}.raw img{width:auto;height:256px;max-width:none}.raw{display:flex;gap:18px;overflow:auto;padding:20px;background:#151622}footer{margin:32px 0;color:#999caf;font-size:14px}@media(max-width:600px){main{padding:20px 12px}h1{font-size:29px}.toolbar{position:static}}
</style><main><span class="eyebrow">DUNGEON MATCHER / GLOBAL UI EXPLORATION</span><h1>Silver is the frame.<br>Gold is the accent.</h1><p>Three coherent wave/settings directions generated with PixelLab and cleaned in LibreSprite. These are review candidates; production UI and gameplay are unchanged.</p><p class="note">Recommendation: <strong>A — Royal bevel</strong>. Its broader, quieter silver planes and larger gear read most clearly at native size. B is the more delicate ornamental alternative; C explores heavier forged facets.</p><div class="toolbar"><label>Enlarged view <select id="zoom"><option value="2">2×</option><option value="3" selected>3×</option><option value="4">4×</option></select></label><label>Wave <input id="wave" type="number" min="1" max="99999" value="1"></label><label>Surface <select id="surface"><option value="#151622">Dark indigo</option><option value="#15434d">Teal</option><option value="#243a2e">Forest green</option><option value="#d6e4e9">Pale snow</option></select></label><button id="labels">Hide sample labels</button></div>
'''+''.join(cards)+'''
<section><h2 style="display:block;margin-top:36px">Reference and source trail</h2><p>The original target below guided silhouettes and decoration. No pixels were extracted from it to make these candidates.</p><details><summary>Open supplied design target</summary><img class="reference" src="Inputs/UserTarget.png" alt="Original silver and gold relic UI target"></details><p>Current golden references, unchanged, at native size:</p><div class="refs"><img src="Inputs/Rattlebones.png" alt="Approved Rattlebones"><img src="Inputs/Farmer.png" alt="Approved Farmer"><img src="Inputs/Pan.png" alt="Approved Pan"><img src="Inputs/CurrentEmerald.png" alt="Approved Emerald"><img src="Inputs/CurrentRuby.png" alt="Approved Ruby"></div><details><summary>Current live UI for comparison</summary><div class="refs"><img src="Inputs/CurrentWave.png" alt="Existing wave"><img src="Inputs/CurrentSettings.png" alt="Existing settings"><img src="Inputs/CurrentAbility.png" alt="Existing ability"><img src="Inputs/CurrentSupply.png" alt="Existing item slot"></div></details><details><summary>Raw PixelLab sources before cleanup</summary><p>A and B were generated as pairs. The first C source omitted the wave and overshot the intended Settings size; separate C wave and compact Settings generations completed that direction. Every source is preserved.</p><div class="raw"><figure><img src="Raw/A.png" alt="Raw A"><figcaption>A / raw</figcaption></figure><figure><img src="Raw/B.png" alt="Raw B"><figcaption>B / raw</figcaption></figure><figure><img src="Raw/C.png" alt="Superseded raw C"><figcaption>C / incomplete first source</figcaption></figure><figure><img src="Raw/CWave.png" alt="Raw C wave"><figcaption>C / wave</figcaption></figure><figure><img src="Raw/CSettings.png" alt="Raw C settings"><figcaption>C / compact settings</figcaption></figure></div></details></section>
<footer><p>Stage 1 only · 29 PixelLab generations · no merge</p><p>Hard 0/255 alpha; native artwork never scaled; six LibreSprite ASE sources reopened with exact visible-pixel matches. Settings canvases are square 56×56 with transparent padding. The decorative wave extents vary; Stage 2 must separate ornaments and fit native modular frame pieces to the existing live layout. Sample labels and background swatches are browser review aids, not Unity captures or cross-zone validation.</p><p><a href="README.md">Handoff notes</a> · <a href="QA/NativeValidation.json">Pixel checks</a> · <a href="ImplementationAudit.md">Existing UI audit</a> · <a href="Preview/FontAttribution.md">Font attribution</a></p></footer></main><script>
const zoom=document.getElementById('zoom'),wave=document.getElementById('wave'),surface=document.getElementById('surface');let show=true;
function render(){const z=+zoom.value,n=Math.max(1,Math.min(99999,+wave.value||1));document.documentElement.style.setProperty('--surface',surface.value);document.querySelectorAll('.wave').forEach(el=>{let scale=el.closest('.native')?1:z;el.style.width=(+el.dataset.width*scale)+'px';el.style.height=(+el.dataset.height*scale)+'px';let t=el.querySelector('span');t.textContent='WAVE '+n;t.style.display=show?'':'none';t.style.fontSize=(32*scale)+'px';t.style.lineHeight=(32*scale)+'px';t.style.top=((+el.dataset.cy-16)*scale)+'px';t.style.transform='none';});document.querySelectorAll('.setting').forEach(el=>{el.width=56*z;el.height=56*z;});}zoom.onchange=render;wave.oninput=render;surface.onchange=render;document.getElementById('labels').onclick=()=>{show=!show;document.getElementById('labels').textContent=show?'Hide sample labels':'Show sample labels';render();};render();
</script></html>'''
(HERE/'Review.html').write_text(html,encoding='utf-8')
missing=[]
for path in re.findall(r'(?:src|href)="([^"]+)"',html):
    if not (HERE/path).is_file():missing.append(path)
assert not missing,missing
print('Review built: six native assets, three comparison pairs, all local links verified.')
