"""Native pose comparison of the selected palette-cleaned production clips."""
from pathlib import Path
from PIL import Image,ImageDraw
import json
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake';out=root/'production';specs=json.loads((out/'animation-manifest.json').read_text())['clips'];actors=sorted({s['name'] for s in specs})
review=Image.new('RGB',(768,132*len(actors)),'#303238');draw=ImageDraw.Draw(review)
for row,name in enumerate(actors):
    draw.text((4,row*132+3),name,fill='white')
    for column,state in enumerate(['Idle','AutoAttack','Ability','ChannelHold','Release','Death']):
        spec=next((s for s in specs if s['name']==name and s['state']==state),None)
        if spec is None:continue
        n=spec['frameCount']-1 if state=='Death' else spec['impactFrame'];sheet=Image.open(out/name/(state+'.png')).convert('RGBA');im=sheet.crop((n*128,0,(n+1)*128,104))
        review.paste(im,(column*128,row*132+20),im)
review.save(out/'native-key-poses.png');print('Native key-pose sheet saved.')
from html import escape
cards=[]
for name in actors:
    clips=[s for s in specs if s['name']==name]
    cards.append('<section><h2>'+escape(name.replace('_',' ').title())+'</h2><div class="clips">'+''.join(
        '<figure><figcaption>'+escape(s['state'])+'</figcaption><img width="384" height="312" src="production/'+name+'/'+s['state']+'.gif"><small>'+str(s['frameCount'])+' exposures · '+str(sum(s['durationsMs']))+' ms</small></figure>' for s in clips)+'</div></section>')
(root/'MotionReview.html').write_text('''<!doctype html><meta charset="utf-8"><title>Frostwake production motion</title><style>
body{font:17px system-ui;background:#202329;color:#eee;margin:32px}h1{color:#b8e1ef}a{color:#e8bc72}.clips{display:flex;flex-wrap:wrap;gap:16px}figure{margin:0;background:#303238;padding:12px}img{image-rendering:pixelated}small{display:block}section{border-top:1px solid #55606e;margin-top:24px}.native{max-width:none}p{max-width:950px;line-height:1.5}</style>
<h1>Frostwake Peaks · production motion</h1><p>Eleven user-approved identities. 70 selected clips. GIFs below show 3× nearest-neighbor playback of native 128×104 cels, with a fixed floor four pixels above the bottom. Each character retains its approved 15-color palette and binary alpha. The native PNGs and technical records establish dimensions; this gallery is a visual review.</p>
<p><a href="Review.html">Original still approval comparison</a> · <a href="production/animation-manifest.json">Clip timings</a> · <a href="motion-palette-checks.json">Native checks</a></p>
<h2>Native key poses · 1× source scale</h2><img class="native" src="production/native-key-poses.png">'''+''.join(cards),encoding='utf-8')
print('MotionReview.html saved.')
