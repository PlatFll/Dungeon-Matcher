"""Build synchronized before/after review from exact provider output PNGs."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
from html import escape
import json, shutil, hashlib

ROOT=Path(__file__).resolve().parents[1];BASE=ROOT/'ArtSource/Frostwake';OUT=BASE/'face-revision'
specs=json.loads((BASE/'production/technical-checks.json').read_text())
verified=json.loads((OUT/'verification.json').read_text())
sources=json.loads((OUT/'sources.json').read_text())
changed={r['actor']:r for r in verified['actors']}
order=['jarl_haldra','rime_skald','iceborn_champion','iceborn_raider','frostblade_reaver','hearthkeeper','frostfang_wolf']
approved=set(order)
notes={'jarl_haldra':'Revision 2: solid black eyes (#101517), with the existing eyelids and contour.', 'rime_skald':'Revision 2: warm light beige skin (#DFA881, matching Haldra), gray hair and beard, solid dark eyes. Cream cap, horn and burgundy clothing retained.', 'iceborn_champion':'Revision 2: removed the white and gray eye pixels beneath the helmet, including attack and reaction poses. Closed eyelids remain closed.', 'iceborn_raider':'Approved: removed the pale eye highlights; both eyes read as one dark color.', 'frostblade_reaver':'Approved: removed the pale eye slivers beneath the helmet.', 'hearthkeeper':'Approved: removed the cream eye highlights, including the lit casting poses.', 'frostfang_wolf':'Approved: solid ice-blue eye with a dark edge; no white pupil or glint.'}
faces={'jarl_haldra':[59,33,81,50],'rime_skald':[51,45,82,66],'iceborn_champion':[52,36,78,54],'iceborn_raider':[54,54,75,71],'frostblade_reaver':[61,51,81,67],'hearthkeeper':[51,41,73,58],'frostfang_wolf':[22,49,35,62]}
(OUT/'before').mkdir(exist_ok=True)
cards=[];data={};rows=[]
overview=Image.new('RGB',(1536,2000),'#303238')
for index,name in enumerate(order):
    item=next(a['items'][0] for a in sources if a['actor']==name)
    original=OUT/'before'/(name+'.png')
    assert original.exists(), 'Preserve the original review still before game integration'
    # This is a labeled inspection composition, never an imported sprite.
    comparison=Image.open(OUT/(name+'-comparison.png'));overview.paste(comparison,(index%2*768,index//2*500))
    palette_counts=[]
    for kind,path in [('before',OUT/'before'/(name+'.png')),('after',OUT/'candidates'/name/'Still.png')]:
        im=Image.open(path).convert('RGBA');palette_counts.append(len({c for c in im.get_flattened_data() if c[3]}))
        crop=im.crop(faces[name]);crop.resize((crop.width*8,crop.height*8),Image.Resampling.NEAREST).save(OUT/f'{name}-face-{kind}.png')
    clips=[s for s in specs if s['name']==name];data[name]=clips
    cards.append(f'''<section id="{name}"><h2>{name.replace('_',' ').title()}</h2><p>{escape(notes[name])}</p>
<div class="labels"><b>Before · current main</b><b>After · {'approved' if name in approved else 'revised candidate'}</b></div>
<div class="pair stage native"><img src="face-revision/before/{name}.png" width="128" height="104"><img src="face-revision/candidates/{name}/Still.png" width="128" height="104"></div>
<p class="small">Native 1× above · same 128×104 canvas and occupied silhouette · {palette_counts[0]} → {palette_counts[1]} opaque colors.</p>
<div class="pair stage"><img src="face-revision/before/{name}.png" width="384" height="312"><img src="face-revision/candidates/{name}/Still.png" width="384" height="312"></div>
<div class="pair stage faces"><img src="face-revision/{name}-face-before.png"><img src="face-revision/{name}-face-after.png"></div>
<p class="small">Full sprite: 3× nearest-neighbor · face detail: 8× nearest-neighbor.</p>
<div class="controls"><label>Motion <select data-actor="{name}">{''.join('<option>'+s['state']+'</option>' for s in clips)}</select></label><button class="play" data-actor="{name}">Pause</button><button class="step" data-actor="{name}">Next frame</button><span id="counter-{name}"></span></div>
<div class="pair stage"><canvas id="before-{name}" width="128" height="104"></canvas><canvas id="after-{name}" width="128" height="104"></canvas></div>
<p class="small">Synchronized source timing; no retiming or pose changes. <a href="face-revision/{name}-comparison.png">Save comparison PNG</a></p></section>''')
    rows.append({'name':name,'stillColors':palette_counts,'canvas':[128,104],'stillBefore':hashlib.sha256(original.read_bytes()).hexdigest(),'stillAfter':hashlib.sha256((OUT/'candidates'/name/'Still.png').read_bytes()).hexdigest()})
overview.save(OUT/'BeforeAfter.png')
# Focused comparison against the user's previous review, not a new sprite source.
focus=Image.new('RGB',(1536,630),'#303238');fd=ImageDraw.Draw(focus)
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',18)
small=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',14)
for column,name in enumerate(['rime_skald','iceborn_champion','jarl_haldra']):
    x=column*512
    fd.text((x+12,8),name.replace('_',' ').title(),font=font,fill='white')
    fd.text((x+12,36),'Previous review',font=small,fill='#c4d7df')
    fd.text((x+268,36),'Corrected',font=small,fill='#c4d7df')
    previous=OUT/'review-01'/(name+'-Still.png') if name!='iceborn_champion' else OUT/'before'/(name+'.png')
    for side,path in enumerate([previous,OUT/'candidates'/name/'Still.png']):
        im=Image.open(path).convert('RGBA');xx=x+side*256
        focus.paste(im,(xx+64,66),im)
        zoom=im.resize((256,208),Image.Resampling.NEAREST);focus.paste(zoom,(xx,185),zoom)
        face=im.crop(faces[name]);face=face.resize((face.width*8,face.height*8),Image.Resampling.NEAREST)
        focus.paste(face,(xx+(256-face.width)//2,420),face)
    fd.text((x+12,606),'Native 1x above / body 2x / face 8x',font=small,fill='#c4d7df')
focus.save(OUT/'Revision02.png')
refdir=OUT/'references';refdir.mkdir(exist_ok=True)
refs=[('Rattlebones','Docs/ArtDirection/Audit2026-10/media/skeleton-still.png'),('Farmer','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png'),('Pan Villager','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png')]
reference_html=[]
for name,p in refs:
    dst=refdir/(name.replace(' ','')+'.png');shutil.copyfile(ROOT/p,dst)
    reference_html.append(f'<figure><img src="face-revision/references/{dst.name}"><figcaption>{name} · unchanged</figcaption></figure>')
unchanged=[a['actor'] for a in sources if a['actor'] not in changed]
unchanged_html=[]
for name in unchanged:
    source=ROOT/f'Assets/_Game/Art/Frostwake/{name}.png';shutil.copyfile(source,OUT/'before'/(name+'.png'))
    unchanged_html.append(f'<figure><img src="face-revision/before/{name}.png" width="256" height="208"><figcaption>{name.replace("_"," ").title()} · unchanged</figcaption></figure>')
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Frostwake · eye and Skald color review</title>
<style>:root{color-scheme:dark;--stage:#303238}*{box-sizing:border-box}body{margin:0;background:#191c21;color:#edf0f4;font:16px/1.5 system-ui}main{max-width:1080px;margin:auto;padding:28px}h1{font-size:32px;margin-bottom:10px}h2{font-size:23px;color:#bddce7}a{color:#9bd3ee}header p{max-width:900px}nav{display:flex;gap:14px;flex-wrap:wrap}.bar{position:sticky;top:0;background:#191c21ee;padding:12px 0;z-index:2}button,select{background:#303640;border:1px solid #586371;border-radius:6px;color:#fff;padding:8px;cursor:pointer}section{border:1px solid #454b55;border-radius:12px;margin-top:30px;padding:18px;overflow:auto}.labels,.pair{display:grid;grid-template-columns:384px 384px;gap:10px}.labels{margin:8px 0;color:#c4d7df}.stage{background:var(--stage);padding:12px 0}.pair img,.pair canvas{justify-self:center}.native{align-items:start}.faces{margin-top:10px;min-height:185px;align-items:center}img,canvas{image-rendering:pixelated;image-rendering:crisp-edges;max-width:none}canvas{width:384px;height:312px}.small{font-size:13px;color:#aeb9c6}.controls,.refs{display:flex;align-items:center;gap:12px;flex-wrap:wrap}.refs{align-items:start;background:var(--stage);padding:14px}.refs figure{margin:4px 14px}.refs figcaption{font-size:12px}footer{padding:30px 0}.badge{display:inline-block;background:#39331c;color:#eed38d;border-radius:5px;padding:5px 10px}</style>
<main><header><span class="badge">Review candidates · not merged</span><h1>Frostwake face-color revision</h1><p>Six revised enemies, shown against the exact merged artwork. All eleven were checked. Eye corrections use the existing shapes; Skald keeps his costume and equipment. Choose which characters to approve after reviewing stills and synchronized motion.</p>
<p><a href="face-revision/BeforeAfter.png">Open the full before/after comparison</a> · <a href="face-revision/verification.json">Pixel verification</a></p></header>
<div class="bar">Preview background <button data-bg="#303238">Neutral</button> <button data-bg="#ebe7dc">Light</button> <button data-bg="#101217">Dark</button></div>
<nav>'''+''.join(f'<a href="#{n}">{n.replace("_"," ").title()}</a>' for n in order)+'''</nav>
<h2>Current rendering references · native 1×</h2><div class="refs">'''+''.join(reference_html)+'''</div><p class="small">Original full canvases at the same native pixel scale. No resampling or size normalization. Frostwake candidates use their current 128×104 production canvases.</p>'''+''.join(cards)+'''<h2>Checked and retained</h2><p>These five already have dark, unhighlighted eye designs. Their pixels remain unchanged.</p><div class="refs">'''+''.join(unchanged_html)+'''</div><footer><p>Exact PixelLab edits: 0 paid generations. Native size, alpha, source registration and clip timing preserved. Gameplay and Unity assets remain unchanged while you review.</p></footer></main>
<script>const DATA='''+json.dumps(data)+''';const cache=new Map(), players={};
function img(url){if(!cache.has(url)){const i=new Image();i.src=url;cache.set(url,i)}return cache.get(url)}
for(const name of Object.keys(DATA))players[name]={state:'Idle',time:0,paused:false,frame:0};
document.querySelectorAll('[data-bg]').forEach(b=>b.onclick=()=>document.documentElement.style.setProperty('--stage',b.dataset.bg));
document.querySelectorAll('select').forEach(s=>s.onchange=()=>{const p=players[s.dataset.actor];p.state=s.value;p.time=0;p.frame=0});
document.querySelectorAll('.play').forEach(b=>b.onclick=()=>{const p=players[b.dataset.actor];p.paused=!p.paused;b.textContent=p.paused?'Play':'Pause'});
document.querySelectorAll('.step').forEach(b=>b.onclick=()=>{const n=b.dataset.actor,p=players[n],s=DATA[n].find(s=>s.state===p.state);p.paused=true;p.frame=(p.frame+1)%s.frameCount;p.time=s.durationsMs.slice(0,p.frame).reduce((a,b)=>a+b,0);document.querySelector('.play[data-actor="'+n+'"]').textContent='Play'});
let last=performance.now();function draw(now){const dt=now-last;last=now;for(const [name,p] of Object.entries(players)){const s=DATA[name].find(s=>s.state===p.state),duration=s.durationsMs.reduce((a,b)=>a+b,0);if(!p.paused)p.time=(p.time+dt)%duration;let sum=0,f=0;for(;f<s.frameCount-1;f++){sum+=s.durationsMs[f];if(p.time<sum)break}p.frame=f;for(const mode of ['before','after']){const c=document.getElementById(mode+'-'+name),ctx=c.getContext('2d'),url=mode==='before'?'production/'+name+'/'+s.state+'.png':'face-revision/candidates/'+name+'/'+s.state+'.png',i=img(url);ctx.imageSmoothingEnabled=false;ctx.clearRect(0,0,128,104);if(i.complete&&i.naturalWidth)ctx.drawImage(i,f*128,0,128,104,0,0,128,104)}document.getElementById('counter-'+name).textContent=(f+1)+' / '+s.frameCount+' · '+duration+' ms'}requestAnimationFrame(draw)}requestAnimationFrame(draw);
</script></html>'''
html=html.replace('Six revised enemies, shown against the exact merged artwork. All eleven were checked.', 'Three new revisions: Skald, Champion and Haldra. Raider, Reaver, Hearthkeeper and Wolf retain your approval. Champion\'s pale eye pixels missed in the first pass are now corrected through his motion poses.')
html=html.replace('These five already have dark, unhighlighted eye designs.', 'These four retain your approval and their existing dark eye designs.')
html=html.replace('<h2>Current rendering references', '<h2>Latest changes against the previous review</h2><a href="face-revision/Revision02.png"><img alt="Skald beige skin, Champion dark eyes, Haldra black eyes before and after" src="face-revision/Revision02.png" style="width:100%;height:auto"></a><h2>Current rendering references')
html=html.replace('Review candidates · not merged','Approved · game integration')
html=html.replace('Choose which characters to approve after reviewing stills and synchronized motion.','All final corrections were explicitly approved for integration and merge on 11 October 2026.')
html=html.replace('Before · current main','Before · original artwork')
html=html.replace("?'production/'+name+'/'+s.state+'.png'", "?'face-revision/before-motion/'+name+'/'+s.state+'.png'")
html=html.replace('Gameplay and Unity assets remain unchanged while you review.','Approved sprites and animations are imported. Gameplay, clip timings, controllers and GUIDs remain unchanged.')
(BASE/'FaceReview.html').write_text(html,encoding='utf-8')
(OUT/'comparison-metadata.json').write_text(json.dumps(rows,indent=2))
print('Review updated: all seven corrections approved and integrated; four other characters retained.')
