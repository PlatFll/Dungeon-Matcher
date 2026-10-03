"""Export selected vine motion without scaling or changing approved stills."""
from pathlib import Path
from PIL import Image
import json, hashlib, base64
from forest_prepare_channels import palette_repair
ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'ArtSource/Forest/RosterProduction'
reference=Image.open(ART/'Raw/Vines_B/Concept/00.png').convert('RGBA')
hit=[palette_repair(Image.open(p),reference) for p in sorted((ART/'Raw/Vines_B/Hit').glob('*.png'))]
assert len(hit)==9
hit[0]=reference.copy()
# The independent spread request barely moved. Reverse the approved recoil
# poses to grow from edge tendrils to the exact dense resting mat instead.
clips={'Spread':list(reversed(hit)),'Hit':hit}
checks=[]
for state,frames in clips.items():
    folder=ART/'Selected/Vines_B'/state;folder.mkdir(parents=True,exist_ok=True)
    for i,frame in enumerate(frames):
        assert frame.size==(64,64)
        path=folder/f'{i:02}.png';frame.save(path)
        checks.append(dict(state=state,frame=i,size=list(frame.size),bounds=frame.getbbox(),
            alpha=sorted({p[3] for p in frame.getdata()}),sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
(ART/'vine-motion-checks.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
selection={"approvedDate":"2026-10-03","choices":{"Wood":"A","Stone":"B","Chain":"A","Thorn":"A","Root1":"B","Root2":"B","Vines":"B"},
"motion":{"Spread":"Reverse of Vines_B/Hit, matching growth silhouette to recoil and selected still. Standalone Spread output rejected for negligible movement.","Hit":"Vines_B/Hit; 9 frames; first frame exact selected still."}}
(ART/'APPROVED_BLOCKERS.json').write_text(json.dumps(selection,indent=2),encoding='utf-8')
def data(frame):
    import io
    stream=io.BytesIO();frame.save(stream,format='PNG');return 'data:image/png;base64,'+base64.b64encode(stream.getvalue()).decode()
page='''<!doctype html><meta charset="utf-8"><title>Selected dense vine motion</title><style>body{background:#252632;color:#eee;font:18px system-ui;margin:32px}.row{display:flex;gap:32px}canvas{image-rendering:pixelated;width:256px;height:256px;background:#777}button{padding:10px}</style><h1>Vines B · selected motion</h1><p>Native 64×64 · 4× nearest-neighbor preview · growth 405 ms · recoil 315 ms.</p><div class="row" id="row"></div><script>const clips=DATA;for(const [name,urls] of Object.entries(clips)){let a=document.createElement('article');a.innerHTML='<h2>'+name+'</h2><canvas width="64" height="64"></canvas><p></p><button>Pause</button> <button>Next frame</button>';row.append(a);const c=a.querySelector('canvas').getContext('2d'),p=a.querySelector('p'),b=a.querySelectorAll('button'),ims=urls.map(u=>{let i=new Image();i.src=u;return i});let f=0,run=true,last=0;function draw(){c.clearRect(0,0,64,64);if(ims[f].complete)c.drawImage(ims[f],0,0);p.textContent=(f+1)+' / '+ims.length}b[0].onclick=()=>{run=!run;b[0].textContent=run?'Pause':'Play'};b[1].onclick=()=>{run=false;b[0].textContent='Play';f=(f+1)%ims.length;draw()};function tick(t){if(run&&t-last>((f===8?700:0)+(name==='Spread'?45:35))){f=(f+1)%ims.length;last=t}draw();requestAnimationFrame(tick)}requestAnimationFrame(tick)}</script>'''
(ART/'VineMotionReview.html').write_text(page.replace('DATA',json.dumps({s:[data(f) for f in fs] for s,fs in clips.items()})),encoding='utf-8')
print(json.dumps({'frames':len(checks),'selected':selection['choices']}))
