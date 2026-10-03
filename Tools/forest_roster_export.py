"""Export native, palette-locked sheets and a timing preview from reviewed PixelLab poses."""
from pathlib import Path
from PIL import Image, ImageDraw
import base64, hashlib, json
from forest_prepare_channels import palette_repair
ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'ArtSource/Forest/RosterProduction'
OUT=ART/'Selected';OUT.mkdir(exist_ok=True)
NAMES=['Elven_Thornkeeper','Orc_Berserker','Orc_Bloomcaller','Snapvine','Orc_Drummer','Briar_Archer','Ancient_Treant']
IMPACTS={'Elven_Thornkeeper':6,'Orc_Berserker':6,'Orc_Bloomcaller':5,'Snapvine':6,'Orc_Drummer':3,'Briar_Archer':5,'Ancient_Treant':6}
manifest=[];checks=[];cards=[]
def data(path):return 'data:image/png;base64,'+base64.b64encode(path.read_bytes()).decode()
def export(name,state,frames,timing,impact=-1,loop=False,special=False):
    w,h=frames[0].size;folder=OUT/name;folder.mkdir(exist_ok=True)
    sheet=Image.new('RGBA',(w*len(frames),h))
    for i,frame in enumerate(frames):sheet.paste(frame,(i*w,0))
    path=folder/(state+'.png');sheet.save(path)
    item=dict(name=name,state=state,width=w,height=h,frameCount=len(frames),durationsMs=timing,impactFrame=impact,loop=loop,special=special)
    manifest.append(item)
    all_colors=set();alpha=set()
    for f in frames:
        px=list(f.getdata());all_colors.update(p for p in px if p[3]);alpha.update(p[3] for p in px)
    checks.append(dict(name=name,state=state,nativeCanvas=[w,h],frames=len(frames),alpha=sorted(alpha),
                       opaqueColors=len(all_colors),bounds=[f.getbbox() for f in frames],sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    preview=ART/'Review'/(name+'_'+state+'_selected.png')
    cols=min(5,len(frames));rows=(len(frames)+cols-1)//cols
    plate=Image.new('RGB',(w*cols*3,(h+8)*rows*3),(110,110,110));draw=ImageDraw.Draw(plate)
    for i,f in enumerate(frames):
        x=(i%cols)*w*3;y=(i//cols)*(h+8)*3
        plate.paste(f.resize((w*3,h*3),Image.Resampling.NEAREST),(x,y+24),f.resize((w*3,h*3),Image.Resampling.NEAREST))
        draw.text((x+2,y+3),str(i)+(' · impact' if i==impact else ''),fill='white')
    plate.save(preview)
    cards.append(dict(**item,src=data(path),reference=data(ART/'Inputs'/(name+'.png'))))
for name in NAMES:
    ready=Image.open(ART/'Inputs'/(name+'.png')).convert('RGBA')
    # The approved stump source includes empty rows below its roots. Keep its
    # pixels unchanged and align opaque feet to the shared bottom-center anchor.
    shift=ready.height-ready.getbbox()[3]
    if shift:
        grounded=Image.new('RGBA',ready.size);grounded.paste(ready,(0,shift));ready=grounded
    target_bottom=ready.height
    (OUT/name).mkdir(exist_ok=True);ready.save(OUT/name/'Ready.png')
    def poses(raw_state):
        paths=sorted((ART/'Raw'/name/raw_state).glob('*.png'))
        if not paths:return []
        result=[]
        for path in paths:
            f=palette_repair(Image.open(path),ready)
            assert f.size==ready.size,(name,raw_state,f.size)
            if not raw_state.startswith('Death') or name=='Ancient_Treant':
                delta=target_bottom-f.getbbox()[3]
                if delta:
                    aligned=Image.new('RGBA',f.size);aligned.paste(f,(0,delta));f=aligned
            result.append(f)
        result[0]=ready.copy()
        if not raw_state.startswith('Death'):result[-1]=ready.copy()
        return result
    for state in ['Idle','AutoAttack','Hit','Death','Ability']:
        raw='AutoAttackCorrected' if name=='Orc_Bloomcaller' and state=='AutoAttack' else state
        if name=='Ancient_Treant' and state=='Death':raw='DeathCorrected'
        if name=='Orc_Drummer' and state=='Ability':raw='AbilityRetry'
        if name=='Orc_Bloomcaller' and state=='Ability':raw='AbilityCorrected'
        frames=poses(raw)
        if not frames:continue
        # The generator elongates the top twig in 1–4. Keep the grounded
        # collapse poses, which preserve the approved short attached twig.
        if name=='Ancient_Treant' and state=='Death':frames=[frames[i] for i in [0,5,6,7,8]]
        # Omit the generator's isolated cyan orb and its clipped staff pose.
        if name=='Orc_Bloomcaller' and state=='Ability':frames=[frames[i] for i in [0,1,2,5,6,7,8]]
        n=len(frames)
        if state=='Idle':timing=[140]*n
        elif state=='Hit':timing=[45,65,85,65,80][:n]
        elif state=='Death':timing=[90]*(n-1)+[240]
        else:
            timing=[80]*n;timing[0]=100;timing[-1]=140
            if name=='Ancient_Treant':timing=[100]*n;timing[-1]=160
            if state=='AutoAttack':timing[IMPACTS[name]]=45
        assert len(timing)==n
        contact=IMPACTS[name] if state=='AutoAttack' else (7 if name=='Orc_Drummer' else 3 if name=='Orc_Bloomcaller' else 4) if state=='Ability' else -1
        export(name,state,frames,timing,contact,state=='Idle',state=='Ability')
    frames=poses('ChannelSequence')
    if frames:
        # Cut only at the reviewed held anticipation pose; one generated sequence
        # supplies start/hold/release so there is no independent-pose seam.
        cut=6
        export(name,'ChannelStart',frames[:cut+1],[85]*cut+[140],cut,False,True)
        export(name,'ChannelHold',[frames[cut]],[500],loop=True)
        release=frames[cut:]
        export(name,'Release',release,[55]*(len(release)-1)+[150],5 if name=='Ancient_Treant' else 3,False,True)
(OUT/'animation-manifest.json').write_text(json.dumps({'clips':manifest},indent=2),encoding='utf-8')
(ART/'animation-technical-checks.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
page="""<!doctype html><meta charset="utf-8"><title>Forest roster motion review</title>
<style>body{margin:24px;background:#20212a;color:#ece6d7;font:16px system-ui}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(560px,1fr));gap:14px}article{background:#30323b;padding:16px;border-radius:12px}.poses{display:flex;gap:16px;align-items:end}canvas,img{image-rendering:pixelated;background:#777}button,select{padding:8px;background:#ddd0ad;color:#17181c;border:0;border-radius:4px}small{display:block;margin:12px 0}h3{margin-top:0}a{color:#e7c776}</style>
<h1>Forest roster motion</h1><p>Approved ready pose beside generated motion. Fixed native pixels, 3× preview. Use Pause and Next to inspect impact and recovery. In-game timings are driven by this same export.</p><div class="grid" id="grid"></div>
<script>const clips=DATA;for(const c of clips){const box=document.createElement('article');box.innerHTML='<h3>'+c.name.replaceAll('_',' ')+' · '+c.state+'</h3><div class="poses"><img width="'+c.width+'" height="'+c.height+'" src="'+c.reference+'"><canvas width="'+c.width+'" height="'+c.height+'" style="width:'+c.width*3+'px;height:'+c.height*3+'px"></canvas></div><small></small><button>Pause</button> <button>Next frame</button>';grid.append(box);const canvas=box.querySelector('canvas'),ctx=canvas.getContext('2d'),label=box.querySelector('small'),buttons=box.querySelectorAll('button'),im=new Image();im.src=c.src;let frame=0,run=true,last=0;function draw(){ctx.clearRect(0,0,c.width,c.height);ctx.drawImage(im,frame*c.width,0,c.width,c.height,0,0,c.width,c.height);label.textContent=(frame+1)+' / '+c.frameCount+' · '+c.durationsMs.reduce((a,b)=>a+b,0)+' ms'+(frame===c.impactFrame?' · IMPACT':'')}im.onload=draw;buttons[0].onclick=()=>{run=!run;buttons[0].textContent=run?'Pause':'Play';last=performance.now()};buttons[1].onclick=()=>{run=false;buttons[0].textContent='Play';frame=(frame+1)%c.frameCount;draw()};function tick(now){if(run&&now-last>c.durationsMs[frame]+(frame===c.frameCount-1&&!c.loop?650:0)){frame=(frame+1)%c.frameCount;last=now;draw()}requestAnimationFrame(tick)}requestAnimationFrame(tick)}</script>"""
(ART/'MotionReview.html').write_text(page.replace('DATA',json.dumps(cards)),encoding='utf-8')
print(json.dumps(dict(exported=len(manifest),expected=39,names=sorted({c['name'] for c in manifest}))))
