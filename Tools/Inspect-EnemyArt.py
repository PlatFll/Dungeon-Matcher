"""Read-only baseline art inspection using the immutable pre-cohesion sources."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import json, hashlib, collections

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'.utmp/EnemyArtCohesion'
OUT.mkdir(parents=True,exist_ok=True)
data=json.loads((ROOT/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text(encoding='utf-8'))
jobs=json.loads((ROOT/'ArtSource/EnemyArtCohesion/jobs.json').read_text(encoding='utf-8'))
originals={j['path']:j['original'] for j in jobs}
originals.update({j['path']+'.meta':j['original']+'.meta' for j in jobs})
for path,h in data['source_hashes'].items():
    assert hashlib.sha256((ROOT/originals.get(path,path)).read_bytes()).hexdigest()==h,path
cache={}
def pixels(sd):
    p=sd['path']
    if p not in cache:cache[p]=Image.open(ROOT/originals.get(p,p)).convert('RGBA')
    return cache[p].crop(sd['crop_top_left'])
records=[]; suspects=[]
for a in data['actors']:
    if a['zone']=='Players':continue
    seen={}; states=[('Ready',dict(sprite=a['still_binding']))]
    states += [(s['name']+':'+str(i+1),f) for s in a['states'] for i,f in enumerate(s['frames'])]
    for state,f in states:
        sd=f['sprite'];key=(sd['path'],sd['fileID'])
        if key in seen:seen[key]['states'].append(state);continue
        im=pixels(sd);alpha=im.getchannel('A');bounds=alpha.getbbox();w,h=im.size
        edge={side:sum(v>0 for v in alpha.crop(rect).getdata()) for side,rect in
            [('left',(0,0,1,h)),('right',(w-1,0,w,h)),('top',(0,0,w,1)),('bottom',(0,h-1,w,h))]}
        r=dict(actor=a['id'],name=a['name'],states=[state],source=sd,bounds=bounds,edges=edge,
            alpha_values=sorted(set(alpha.getdata())),opaque_colors=len({p[:3] for p in im.getdata() if p[3]}))
        seen[key]=r;records.append(r)
    suspects += [r for r in seen.values() if r['edges']['left'] or r['edges']['right'] or r['edges']['top']]
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',13)
for start in range(0,len(suspects),24):
    group=suspects[start:start+24];bw,bh=330,370
    board=Image.new('RGB',(bw*4,bh*((len(group)+3)//4)),'#222636');draw=ImageDraw.Draw(board)
    for i,r in enumerate(group):
        x=(i%4)*bw;y=(i//4)*bh;im=pixels(r['source']);z=min(3,300//max(im.size))
        draw.text((x+6,y+6),r['name'],font=font,fill='white')
        draw.text((x+6,y+24),', '.join(r['states'])[:44],font=font,fill='#ddddbb')
        draw.text((x+6,y+42),str(r['edges']),font=font,fill='#cccccc')
        up=im.resize((im.width*z,im.height*z),Image.Resampling.NEAREST)
        draw.rectangle((x+6,y+65,x+6+up.width,y+65+up.height),outline='#ff9999')
        board.paste(up,(x+6,y+65),up)
    board.save(OUT/f'edge-review-{start//24:02}.png')
(OUT/'inspection.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
print(json.dumps(dict(actors=len({r['actor'] for r in records}),unique_bound_sprites=len(records),
    side_or_top_edge_contacts=len(suspects),edge_actors=dict(collections.Counter(r['actor'] for r in suspects)),
    inspection=str(OUT/'inspection.json'),boards=(len(suspects)+23)//24),indent=2))
