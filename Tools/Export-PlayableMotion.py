"""Pack selected PixelLab cels losslessly. No drawing, resizing or color edits."""
from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json
root=Path(__file__).resolve().parents[1]/'ArtSource/FrostwakePlayables'
out=root/'production';out.mkdir(exist_ok=True)
clips=[];checks=[]
def pack(name,source,indices,times,loop=False,pivot=0):
    revised=root/'eye-revision'/source
    folder=revised if source.startswith('Minotaur_') and revised.exists() else root/'motion'/source/'palette-locked'
    frames=[Image.open(folder/f'{i:02}.png').convert('RGBA') for i in indices]
    w,h=frames[0].size
    assert all(f.size==(w,h) for f in frames)
    sheet=Image.new('RGBA',(w*len(frames),h))
    for i,frame in enumerate(frames):sheet.paste(frame,(w*i,0))
    path=out/f'{name}.png';sheet.save(path)
    char=name.split('_')[0];still=root/'eye-revision'/'Minotaur.png' if char=='Minotaur' and revised.exists() else root/'locked'/f'{char}.png'
    pal={p for p in Image.open(still).convert('RGBA').get_flattened_data() if p[3]}
    for i,f in enumerate(frames):
        assert set(f.getchannel('A').get_flattened_data())<={0,255}
        assert {p for p in f.get_flattened_data() if p[3]}<=pal
        b=f.getbbox();assert b[0]>0 and b[1]>0 and b[2]<w and b[3]<h,(name,i,b)
        checks.append(dict(clip=name,frame=i,source=source,sourceFrame=indices[i],bounds=b,colors=len({p for p in f.get_flattened_data() if p[3]})))
    clips.append(dict(name=name,character=char,state=name.split('_')[1],width=w,height=h,pivotY=pivot,durations=times,loop=loop,sha256=hashlib.sha256(path.read_bytes()).hexdigest(),source=source,indices=indices))
    # Review-only native and integer enlargement, never used in Unity.
    previews=[]
    for f in frames:
        p=Image.new('RGB',(w*3,h*3),'#303238');big=f.resize(p.size,Image.Resampling.NEAREST);p.paste(big,(0,0),big);previews.append(p)
    previews[0].save(out/f'{name}.gif',save_all=True,append_images=previews[1:],duration=times,loop=0,disposal=2)
    contact=Image.new('RGB',(w*3*len(frames),h*3),'#303238')
    for i,p in enumerate(previews):contact.paste(p,(i*w*3,0))
    contact.save(out/f'{name}-review.png')
for char,pivot in [('Minotaur',16),('Kobold',4)]:
    idle=list(range(9)) if char=='Minotaur' else [0,1,2,3,4,3,2,1,0]
    pack(char+'_Idle',char+'_Idle',idle,[130]*9,True,pivot)
    pack(char+'_Hit',char+'_Hit',list(range(5)),[40,60,80,60,80],False,pivot)
    pack(char+'_Death',char+'_Death',list(range(9)),[60,70,80,90,90,90,90,90,140],False,pivot)
pack('Minotaur_Ability','Minotaur_Jump',[0,1,2,3,4],[80]*5,False,16)
pack('Minotaur_Travel','Minotaur_Idle',[0],[130],True,16)
pack('Minotaur_Recovery','Minotaur_Jump',[6,7,8,0],[80]*4,False,16)
pack('Kobold_Ability','Kobold_Spin',list(range(9)),[60]*9,False,4)
pack('Kobold_Fire','Kobold_Fire',list(range(5)),[40,80,40,80,60],False,4)
pack('Kobold_Recovery','Kobold_Fire',[3,4],[80,100],False,4)
(out/'manifest.json').write_text(json.dumps(dict(clips=clips,selectionNotes={'Kobold_Idle':'Generated frames 5 and 6 introduced an unrequested flame. Rejected those frames; use the compact first-five poses forward/backward without editing their pixels.'}),indent=2))
(out/'technical-checks.json').write_text(json.dumps(checks,indent=2))
html=['<!doctype html><meta charset="utf-8"><title>Dungeon Matcher playable motion</title><style>body{background:#25272c;color:#eee;font:16px system-ui}main{display:grid;grid-template-columns:repeat(3,1fr);gap:12px}article{background:#303238;padding:12px;border-radius:8px}img{image-rendering:pixelated;max-width:100%}</style><h1>Locked playable motion</h1><p>Native pixels at 3×. Minotaur 96×96 padded canvas; Kobold 96×80. Game timing; color palettes locked to approved stills.</p><main>']
for c in clips:html.append(f'<article><h2>{c["name"]}</h2><img src="{c["name"]}.gif"><p>{sum(c["durations"])} ms / {len(c["durations"])} drawings</p></article>')
html.append('</main>');(out/'Review.html').write_text('\n'.join(html))
print(f'Packed {len(clips)} clips; {len(checks)} cels: native, unclipped, binary alpha and approved palettes.')
