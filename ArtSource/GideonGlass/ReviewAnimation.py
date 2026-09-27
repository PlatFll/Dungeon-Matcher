"""Native cel/export integrity and read-only frame/playback inspection layouts."""
from pathlib import Path
import sys, json, hashlib
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parent
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase

name=sys.argv[1] if len(sys.argv)>1 else 'Gideon_Idle'
frames,durations=read_ase(ROOT/(name+'.aseprite'))
sheet=Image.open(ROOT/(name+'.png')).convert('RGBA')
gif=Image.open(ROOT/(name+'.gif'))
ready=Image.open(ROOT/'Gideon_Ready.png').convert('RGBA')
report={'name':name,'frame_count':len(frames),'durations_ms':durations,'loop_ms':sum(durations),'frames':[]}
def components(im):
    remaining={(x,y) for y in range(64) for x in range(64) if im.getpixel((x,y))[3]}
    counts=[]
    while remaining:
        group={remaining.pop()}; pending=list(group)
        while pending:
            x,y=pending.pop()
            for xx in range(x-1,x+2):
                for yy in range(y-1,y+2):
                    if (xx,yy) in remaining:
                        remaining.remove((xx,yy));group.add((xx,yy));pending.append((xx,yy))
        counts.append(len(group))
    return sorted(counts,reverse=True)
for i,f in enumerate(frames):
    gif.seek(i)
    exported=sheet.crop((i*64,0,i*64+64,64))
    assert exported.tobytes()==f.tobytes(),f'Native sheet mismatch {i}'
    # GIF transparent RGB may differ; compare visible pixels and alpha instead.
    decoded=gif.convert('RGBA')
    assert all(a==b or (a[3]==0 and b[3]==0) for a,b in zip(f.get_flattened_data(),decoded.get_flattened_data()))
    alpha=sorted({p[3] for p in f.get_flattened_data()})
    assert alpha==[0,255]
    if name=='Gideon_Idle':
        assert len(components(f))==1, f'Disconnected idle geometry in frame {i}'
    report['frames'].append({'index':i,'bounds':f.getbbox(),'components':components(f),'colors':len({p[:3] for p in f.get_flattened_data() if p[3]}),
        'ground_matches_ready':f.crop((0,62,64,64)).tobytes()==ready.crop((0,62,64,64)).tobytes(),
        'cane_matches_ready':f.crop((44,44,49,64)).tobytes()==ready.crop((44,44,49,64)).tobytes()})
report['first_equals_ready']=frames[0].tobytes()==ready.tobytes()
report['seam_changed_pixels']=sum(a!=b for a,b in zip(frames[-1].get_flattened_data(),frames[0].get_flattened_data()))
out=Image.new('RGB',(840,600),'#25212d');d=ImageDraw.Draw(out)
for i,f in enumerate(frames):
    x=(i%5)*168;y=(i//5)*300
    d.text((x+4,y+5),f'{i+1}: {durations[i]}ms',fill='white')
    for j,bg in enumerate(['#171420','#ded8ce']):
        tile=Image.new('RGBA',(64,64),bg);tile.alpha_composite(f)
        out.paste(tile.resize((128,128),Image.Resampling.NEAREST),(x+4,y+26+j*134))
out.save(ROOT/(name+'_frames.png'))
ref=Image.open(ROOT.parent/'CombatIdles/Rattlebones_Idle.png').convert('RGBA')
comparison=[]
for i,f in enumerate(frames):
    row=Image.new('RGBA',(128,64),'#ded8ce');row.alpha_composite(ref.crop((i%9*64,0,(i%9+1)*64,64)));row.alpha_composite(f,(64,0));comparison.append(row.convert('RGB'))
comparison[0].save(ROOT/(name+'_comparison.gif'),save_all=True,append_images=comparison[1:],duration=durations,loop=0,disposal=2)
(ROOT/(name+'_audit.json')).write_text(json.dumps(report,indent=2))
print(json.dumps(report))
