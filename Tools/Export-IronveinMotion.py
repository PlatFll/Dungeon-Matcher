"""Assemble reviewed native frames and timings; never resample or repaint production pixels."""
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
BASE=ROOT/'ArtSource/Ironvein'
selections=json.loads((BASE/'Production/motion-selections.json').read_text(encoding='utf-8'))
output=BASE/'Motion';output.mkdir(exist_ok=True)
clips=[];records=[]
for selection in selections:
    name,state=selection['name'],selection['state']
    paths=[BASE/f for f in selection['frames']]
    frames=[Image.open(f).convert('RGBA') for f in paths]
    w,h=frames[0].size
    times=selection['durationsMs']
    assert len(times)==len(frames) and all(t>=10 and t%10==0 for t in times)
    assert all(f.size==(w,h) for f in frames)
    assert all(set(f.getchannel('A').get_flattened_data())<={0,255} for f in frames)
    impact=selection.get('impactFrame',0)
    assert 0<=impact<len(frames)
    sheet=Image.new('RGBA',(w*len(frames),h))
    for i,frame in enumerate(frames): sheet.paste(frame,(i*w,0))
    directory=output/name;directory.mkdir(exist_ok=True)
    target=directory/(state+'.png');sheet.save(target)
    # Verify export did not change any visible RGBA pixel or native canvas.
    exported=Image.open(target).convert('RGBA')
    for i,frame in enumerate(frames): assert exported.crop((i*w,0,(i+1)*w,h)).tobytes()==frame.tobytes()
    clip=dict(name=name,state=state,width=w,height=h,frameCount=len(frames),
              durationsMs=times,impactFrame=impact,loop=selection.get('loop',False),
              special=selection.get('special',False),attack=selection.get('attack',False))
    clips.append(clip)
    records.append(dict(**clip,source_frames=selection['frames'],source_sha256=[hashlib.sha256(p.read_bytes()).hexdigest() for p in paths],
        occupied_bounds=[f.getbbox() for f in frames],palette_counts=[len(set(c[:3] for c in f.get_flattened_data() if c[3])) for f in frames],
        alpha=[0,255],impact_ms=sum(times[:impact]),total_ms=sum(times),scale=1,
        sheet_sha256=hashlib.sha256(target.read_bytes()).hexdigest(),review=selection['review']))
(output/'animation-manifest.json').write_text(json.dumps(dict(clips=clips),indent=2)+'\n',encoding='utf-8')
(BASE/'Production/motion-technical.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
review=ROOT/'.utmp/Ironvein/MotionReview';review.mkdir(parents=True,exist_ok=True)
html='''<!doctype html><meta charset="utf-8"><title>Ironvein native motion review</title>
<style>body{background:#222831;color:#eee;font:16px system-ui;margin:24px}header{position:sticky;top:0;background:#222831;padding:12px;z-index:2}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(360px,1fr));gap:12px}article{border:1px solid #59606b;padding:16px;border-radius:8px}canvas{image-rendering:pixelated;background:#343941;vertical-align:bottom;margin:8px}small{display:block;color:#ecbf72}button,select{font:inherit;padding:6px}</style>
<header><h1>Ironvein · native motion review</h1><p>Internal production selections — pending user visual review. Actual PNG pixels at 1× and 3×. Native dimensions and palette measurements are in motion-technical.json.</p><button onclick="paused=!paused">Pause / play</button> <select onchange="speed=+this.value"><option value="1">Actual timing</option><option value="0.5">Half speed</option><option value="0.25">Quarter speed</option></select></header><main></main><script>
const clips=DATA;let paused=false,speed=1,clock=0,previous=performance.now();const views=[];
for(const c of clips){const a=document.createElement('article');a.innerHTML='<b>'+c.name+' · '+c.state+'</b><small>'+c.width+'×'+c.height+' native · '+c.frameCount+' frames · '+c.total_ms+' ms'+(c.attack||c.special?' · contact '+c.impact_ms+' ms':'')+'</small>';const pair=[];for(const scale of [1,3]){const v=document.createElement('canvas');v.width=c.width;v.height=c.height;v.style.width=c.width*scale+'px';v.style.height=c.height*scale+'px';a.append(v);pair.push(v.getContext('2d'));}const label=document.createElement('small');a.append(label);document.querySelector('main').append(a);const img=new Image();img.src='../../../ArtSource/Ironvein/Motion/'+c.name+'/'+c.state+'.png';views.push({c,pair,img,label});}
function draw(now){if(!paused)clock+=(now-previous)*speed;previous=now;for(const v of views){const c=v.c;let t=clock%(c.total_ms+(c.loop?0:450)),i=0;while(i<c.frameCount-1&&t>=c.durationsMs[i]){t-=c.durationsMs[i++];}for(const x of v.pair){x.clearRect(0,0,c.width,c.height);if(v.img.complete)x.drawImage(v.img,i*c.width,0,c.width,c.height,0,0,c.width,c.height);}v.label.textContent='Frame '+i+(i===c.impactFrame&&(c.attack||c.special)?' · CONTACT':'');}requestAnimationFrame(draw);}requestAnimationFrame(draw);
</script>'''.replace('DATA',json.dumps(records))
(review/'Review.html').write_text(html,encoding='utf-8')
print(json.dumps(dict(clips=len(clips),names=sorted({c['name'] for c in clips}),review=str(review/'Review.html'))))
