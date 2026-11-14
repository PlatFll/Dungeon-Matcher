from pathlib import Path
from PIL import Image,ImageDraw
import json,shutil,hashlib
root=Path(__file__).resolve().parents[3];art=root/'ArtSource/BombReadability';out=art/'SelectedIdles/Preview';out.mkdir(exist_ok=True)
res=root/'Assets/_Game/Resources/VFX/BombRefinement/Idles'
for name in ['Emerald','Ruby','Amber','Topaz','Sapphire','Amethyst']:shutil.copy2(root/f'Assets/_Game/Art/Gems/Small gems/Gesm16/{name}16.png',out/f'{name}16.png')
shutil.copytree(res,out/'Frames',dirs_exist_ok=True,ignore=shutil.ignore_patterns('*.meta'))
for name in ['PoisonA','HealingC','DirectionalD','DirectionalD-Column','StarA']:shutil.copy2(art/f'Candidates/Cleaned/{name}.png',out/f'{name}.png')
for kind in ['SelectedIdles','RowBomb','ColumnBomb','ColorCrystal']:
    frames=sorted((root/'.utmp/BombRefinement'/kind).glob('Frame*.png'))
    if len(frames)==100:
        ims=[Image.open(p).convert('RGB').resize((360,640),Image.Resampling.NEAREST) for p in frames]
        ims[0].save(out/f'Unity-{kind}.gif',save_all=True,append_images=ims[1:],duration=30,loop=0,optimize=False)
        for i in [0,25,35,45]:shutil.copy2(frames[i],out/f'Unity-{kind}-{i:03}.png')
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Selected bomb idles · Dungeon Matcher</title><style>
:root{font:16px system-ui;color:#eaf0fc;background:#111722;color-scheme:dark}body{max-width:1200px;margin:auto;padding:28px}h1{margin-bottom:10px}p{color:#b8c5d9;line-height:1.5}a{color:#a7d9ff}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(205px,1fr));gap:14px}.card{background:#202837;border:1px solid #3b485e;border-radius:10px;padding:16px}.art{display:flex;align-items:end;gap:8px;justify-content:center;padding:10px;background:#17212d;min-height:210px}canvas,img{image-rendering:pixelated}button,select,input{font:inherit}button,select{padding:8px;border:1px solid #71839b;border-radius:4px;background:#28364a;color:white}nav{display:flex;gap:12px;align-items:center;flex-wrap:wrap;position:sticky;top:0;background:#111722ee;padding:12px;z-index:2}small{color:#b3c2da}.game img{width:100%;max-width:360px}.game{display:grid;grid-template-columns:repeat(auto-fit,minmax(285px,1fr));gap:16px}.note{border-left:3px solid #d9ed96;padding:12px;background:#263240}h2{margin-top:32px}</style>
<h1>Your selected bombs — animated</h1><p>Poison A · Healing C · Directional D · Star A. The selected shapes and gem socket placement are preserved. Shield remains unchanged.</p><div class="note">Native 64×64 frames · LibreSprite sources · no additional PixelLab spending · review branch only</div>
<nav><button id="play">Pause</button><button id="step">Next frame</button><label>Gem <select id="gem"><option>Emerald</option><option>Ruby</option><option>Amber</option><option>Topaz</option><option>Sapphire</option><option>Amethyst</option><option>Empty</option></select></label><label>Speed <select id="speed"><option value="1">1×</option><option value="0.5">½×</option></select></label></nav>
<div class="grid" id="cards"></div>
<h2>Actual Unity Game scene</h2><p>Real Game-scene captures on a disposable profile: the first board displays all six gem colors in the selected casings, plus unchanged shields. The other captures activate the bombs through real swaps. Gameplay, clear footprints and refill timing are unchanged.</p>
<div class="game"><div class="card"><h3>Selected idles in-game</h3><img src="Unity-SelectedIdles.gif"><p>Rows from top: poison, healing, row, column, shield, star. Normal gems remain in the outer columns.</p><a href="Unity-SelectedIdles-000.png">Full-resolution screenshot</a></div><div class="card"><h3>Row blast</h3><img src="Unity-RowBomb.gif"><p>Broader yellow-white blast travels left and right.</p><a href="Unity-RowBomb-035.png">Full-resolution impact</a></div><div class="card"><h3>Column blast</h3><img src="Unity-ColumnBomb.gif"><p>Matching native blast travels up and down.</p><a href="Unity-ColumnBomb-035.png">Full-resolution impact</a></div></div>
<p><a href="../Manifest.json">Animation manifest</a> · <a href="../../ReviewNotes.md">Verification notes</a> · <a href="https://github.com/PlatFll/Dungeon-Matcher/pull/232">Draft PR #232</a></p>
<script>const defs=[['Poison','Poison A',24,100,[32,38],'Subtle liquid movement; staggered bubbles rise, open and pop.'],['Healing','Healing C',32,100,[32,36],'Tiny yellow plus signs rise in occasional pulses.'],['Row','Directional D · Row',8,80,[32,32],'The entire bomb rocks as one rigid piece around its fixed center.'],['Column','Directional D · Column',8,80,[32,31],'The entire vertical bomb rocks in place, including its casing and socket.'],['Star','Star A',32,80,null,'Stationary star; the glint crosses the full sprite from corner to corner.']];let entries=[],images={},elapsed=0,running=true,last=0;
function framePath(id,f){return 'Frames/'+((id==='Row'||id==='Column')?id+'Idle':id)+'/'+id+'_'+String(f).padStart(2,'0')+'.png'}
function pic(path){if(!images[path]){images[path]=new Image;images[path].src=path}return images[path]}
for(const [id,label,n,ms,socket,desc] of defs){let el=document.createElement('div');el.className='card';el.innerHTML='<h3>'+label+'</h3><div class="art"></div><p>'+desc+'</p><small>1× native + 2× nearest-neighbor · <span></span></small><p><a href="../Editable/'+id+'.aseprite">LibreSprite source</a></p>';document.querySelector('#cards').append(el);let canvases=[];for(let scale of [1,2]){let c=document.createElement('canvas');c.width=c.height=64;c.style.width=c.style.height=(64*scale)+'px';el.querySelector('.art').append(c);canvases.push(c)}entries.push({id,n,ms,socket,canvases,span:el.querySelector('span')});for(let f=0;f<n;f++)pic(framePath(id,f))}
function render(t){if(last&&running)elapsed+=(t-last)*Number(document.querySelector('#speed').value);last=t;let g=pic(document.querySelector('#gem').value+'16.png');for(const e of entries){let f=Math.floor(elapsed/e.ms)%e.n;let path=framePath(e.id,f);let im=pic(path);for(const c of e.canvases){let ctx=c.getContext('2d');ctx.imageSmoothingEnabled=false;ctx.clearRect(0,0,64,64);if(im.complete&&im.naturalWidth)ctx.drawImage(im,0,0);if(e.socket&&g.complete&&g.naturalWidth)ctx.drawImage(g,e.socket[0]-8,e.socket[1]-8);}e.span.textContent='frame '+(f+1)+' / '+e.n;}requestAnimationFrame(render)}
document.querySelector('#play').onclick=e=>{running=!running;e.target.textContent=running?'Pause':'Play'};document.querySelector('#step').onclick=()=>{running=false;document.querySelector('#play').textContent='Play';elapsed+=100};requestAnimationFrame(render);</script></html>'''
(out/'Review.html').write_text(html,encoding='utf-8')
checks={};checks['shieldUnchanged']=hashlib.sha256((root/'Assets/_Game/Art/Board/Bombs/ShieldBomb.png').read_bytes()).hexdigest()=='3992f944830f2827dc16fd105695fe4112c0c5c1953d5bd6e467523e8ab49d40';checks['rattlebonesUnchanged']=hashlib.sha256((root/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png').read_bytes()).hexdigest()=='705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c'
checks['frameCount']=0;checks['minimumEdgeClearance']=64;checks['binaryAlpha']=True;checks['poisonHealingSocketsUnchanged']=True;checks['starSilhouetteAnchored']=True;checks['directionalWholeUnitRotation']=True;checks['directionalGemCompositesRigid']=True;checks['animatedFamilies']={};star_glinted=set()
for fam,source,cy in [('Poison','PoisonA',38),('Healing','HealingC',36),('Star','StarA',None),('RowIdle','DirectionalD',32),('ColumnIdle','DirectionalD-Column',31)]:
    still=Image.open(art/f'Candidates/Cleaned/{source}.png').convert('RGBA')
    # Generated originals carry (0,1,2) beneath zero alpha. Compare visible RGBA
    # exactly with the same transparent-RGB normalization as LibreSprite export.
    still.putdata([pixel if pixel[3] else (0,0,0,0) for pixel in still.getdata()])
    unique=set()
    for p in (res/fam).glob('*.png'):
        im=Image.open(p).convert('RGBA');b=im.getbbox();checks['frameCount']+=1;unique.add(hashlib.sha256(im.tobytes()).hexdigest())
        if fam=='Star':
            checks['starSilhouetteAnchored'] &= im.getchannel('A').tobytes()==still.getchannel('A').tobytes()
            star_glinted.update((x,y) for y in range(64) for x in range(64)
                                if im.getpixel((x,y))!=still.getpixel((x,y)))
        if fam in ['RowIdle','ColumnIdle']:
            angle=[0,3,0,-3,0,3,0,-3][int(p.stem.rsplit('_',1)[1])]
            expected=still.rotate(angle,resample=Image.Resampling.NEAREST,center=(32,cy))
            checks['directionalWholeUnitRotation'] &= im.tobytes()==expected.tobytes()
            for gp in (root/'Assets/_Game/Art/Gems/Small gems/Gesm16').glob('*.png'):
                gem=Image.open(gp).convert('RGBA');whole=still.copy()
                whole.alpha_composite(gem,(24,cy-8))
                whole=whole.rotate(angle,resample=Image.Resampling.NEAREST,center=(32,cy))
                rendered=im.copy();rendered.alpha_composite(gem,(24,cy-8))
                checks['directionalGemCompositesRigid'] &= rendered.tobytes()==whole.tobytes()
        checks['minimumEdgeClearance']=min(checks['minimumEdgeClearance'],b[0],b[1],64-b[2],64-b[3]);checks['binaryAlpha'] &= set(im.getchannel('A').getdata())<={0,255}
        if cy and fam not in ['RowIdle','ColumnIdle']:
            for y in range(cy-10,cy+11):
                for x in range(22,43):
                    if (x-32)**2+(y-cy)**2<=100:checks['poisonHealingSocketsUnchanged'] &= im.getpixel((x,y))==still.getpixel((x,y))
    checks['animatedFamilies'][fam]=len(unique);assert len(unique)>1,(fam,'idle lost its animation')
star_base=Image.open(art/'Candidates/Cleaned/StarA.png').convert('RGBA')
# Every nonwhite reflective pixel must visibly catch the light at least once,
# including the farthest tip. Dark outline and alpha must never be altered.
reflective={(x,y) for y in range(64) for x in range(64)
            if star_base.getpixel((x,y))[3] and min(star_base.getpixel((x,y))[:3])>75}
needs_light={xy for xy in reflective if star_base.getpixel(xy) not in [(255,255,255,255),(243,250,255,255)]}
checks['starGlintFullCoverage']=needs_light<=star_glinted
checks['starGlintOnlyReflectivePixels']=star_glinted<=reflective
checks['starGlintDiagonalRange']=[min(x+y for x,y in star_glinted),max(x+y for x,y in star_glinted)]
assert checks['starGlintFullCoverage'] and checks['starGlintOnlyReflectivePixels'],checks
assert checks['starSilhouetteAnchored'] and checks['directionalWholeUnitRotation'] and checks['directionalGemCompositesRigid'] and checks['frameCount']==104
assert checks['shieldUnchanged'] and checks['rattlebonesUnchanged'] and checks['binaryAlpha'] and checks['poisonHealingSocketsUnchanged'] and checks['minimumEdgeClearance']>0,checks
(art/'SelectedIdles/NativeValidation.json').write_text(json.dumps(checks,indent=2)+'\n')
# Rebuild the compact review from the exact current runtime cels.
compact=[];gem=Image.open(out/'Emerald16.png').convert('RGBA')
for i in range(32):
    canvas=Image.new('RGB',(384,158),'#202837');draw=ImageDraw.Draw(canvas)
    for col,(family,name,cy,label) in enumerate([('RowIdle','Row',32,'Row shake'),('ColumnIdle','Column',31,'Column shake'),('Star','Star',None,'Full star glint')]):
        tile=Image.open(res/family/f'{name}_{i%8 if cy else i:02}.png').convert('RGBA')
        if cy:tile.alpha_composite(gem,(24,cy-8))
        tile=tile.resize((128,128),Image.Resampling.NEAREST)
        canvas.paste(tile,(col*128,22),tile);draw.text((col*128+8,6),label,fill='#eaf0fc')
    compact.append(canvas)
compact[0].save(out/'AnchoredIdles.gif',save_all=True,append_images=compact[1:],duration=80,loop=0,optimize=False)

serve=Path(r'C:/Users/USER/.codex/visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a/AbilityReadinessReview/BombRefinement')
shutil.copytree(art/'SelectedIdles',serve/'SelectedIdles',dirs_exist_ok=True);shutil.copy2(art/'ReviewNotes.md',serve/'ReviewNotes.md')
print(json.dumps(checks));print('http://127.0.0.1:57126/BombRefinement/SelectedIdles/Preview/Review.html')
