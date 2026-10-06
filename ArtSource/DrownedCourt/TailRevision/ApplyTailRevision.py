"""Authorized direct-pixel additions. Never resample or overwrite a body pixel.
Immutable Before PNGs are the input, so rerunning does not accumulate tails.
Run after older still/motion preparation scripts, before importing reviewed motion.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import json, hashlib, math, shutil

here=Path(__file__).resolve().parent; repo=here.parents[2]
selected=here.parent/'Production/Selected';before=here/'Before';before.mkdir(exist_ok=True)
art=repo/'Assets/_Game/Art/DrownedCourt';review=here/'Review';review.mkdir(exist_ok=True)
names=['hammerhead_bruiser','breakwater_captain']
clips=json.loads((selected/'Motion/animation-manifest.json').read_text())['clips']
outline=(10,13,17,255);shade=(43,57,69,255);mid=(69,89,106,255);light=(107,125,144,255)

def anchor(name,state,i):
    if name=='hammerhead_bruiser':
        if state=='Idle': return [(52,59,0),(52,59,0),(51,58,-3),(52,58,-3),(52,59,0)][i]
        if state=='Hit': return [(52,59,0),(46,58,-4),(46,58,-4),(52,59,0)][i]
        if state=='AutoAttack': return [(52,59,0),(52,59,0),(52,59,-3),(52,59,-4),(49,59,-6),(49,59,-6),(49,59,-4),(54,60,3),(52,59,0),(52,59,0)][i]
        if state=='Death': return death_bruiser[i]
    if state=='Idle': return [(70,58,0),(70,58,0),(70,57,-2),(70,58,0),(70,58,0)][i]
    if state=='AutoAttack': return [(70,58,0),(70,57,-2),(70,57,-3),(70,57,-3),(70,57,-3),(70,57,-3),(70,57,-3),(70,57,-3),(70,61,5),(70,61,5),(70,61,5),(70,61,3),(70,60,1),(70,59,0),(70,58,0)][i]
    if state=='Hit': return [(70,58,0),(66,58,-4),(66,58,-4),(70,58,0)][i]
    if state=='Death': return death_captain[i]
    # All four channel roles reuse these exact source poses.
    sequence={'Ability':list(range(17))+[0], 'ChannelStart':list(range(8)),
              'ChannelHold':[7,8,7], 'Release':list(range(7,17))+[0],
              'Interrupt':list(range(7,-1,-1))+[0]}[state]
    pose=sequence[i];return (70,58 if pose in (0,1,15,16) else 57,-2 if 3<=pose<=12 else 0)

death_bruiser=[(52,59,0),(46,58,-4),(53,58,-20),(56,68,-45),(54,73,-65),(54,74,-65)]+[(54,74,-65)]*7
death_captain=[(70,58,0),(66,58,-4),(66,58,-4),(68,60,-8),(69,64,-18),(71,67,-40),(70,72,-95),(65,73,-125)]+[(65,73,-145)]*5

def add_tail(original,name,pose):
    x,y,angle=pose;wide=name=='breakwater_captain';r=math.radians(angle)
    def pts(values):
        # Captain's tail emerges beyond the original shield; native body is unchanged.
        return [(round(x+(a+(9 if a>=8 and wide else 0))*math.cos(r)-b*math.sin(r)),
                 round(y+(a+(9 if a>=8 and wide else 0))*math.sin(r)+b*math.cos(r))) for a,b in values]
    layer=Image.new('RGBA',original.size);d=ImageDraw.Draw(layer)
    polygon=pts([(0,-4),(8,-3),(14,-5),(17,-13),(19,-15),(19,-7),(17,-1),(21,5),(20,7),(15,3),(9,2),(0,3)])
    assert all(0<px<original.width-1 and 0<py<original.height-1 for px,py in polygon),(name,pose,polygon)
    d.polygon(polygon,fill=mid,outline=outline)
    d.polygon(pts([(1,1),(9,1),(15,-1),(18,4),(19,5),(15,2),(9,1),(1,2)]),fill=shade)
    d.line(pts([(2,-3),(8,-2),(14,-4),(17,-11)]),fill=light,width=1)
    layer.alpha_composite(original)
    old=list(original.get_flattened_data());new=list(layer.get_flattened_data())
    assert all(a==b for a,b in zip(old,new) if a[3]),'original body pixel changed'
    assert set(new)-{(0,0,0,0)} <= set(old)-{(0,0,0,0)},'palette expanded'
    assert layer.getbbox()[3]==original.getbbox()[3],'floor shifted'
    added={(i%original.width,i//original.width) for i,(a,b) in enumerate(zip(old,new)) if not a[3] and b[3]}
    # Every new connected piece must touch the original body, never a detached fin.
    pending=set(added)
    while pending:
        todo=[pending.pop()];touches=False
        while todo:
            px,py=todo.pop()
            for dx,dy in [(1,0),(-1,0),(0,1),(0,-1)]:
                near=(px+dx,py+dy)
                if near in pending:pending.remove(near);todo.append(near)
                elif 0<=near[0]<original.width and 0<=near[1]<original.height and original.getpixel(near)[3]:touches=True
        assert touches,(name,pose,'detached tail pixels')
    return layer

def baseline(relative):
    source=selected/relative;backup=before/relative;backup.parent.mkdir(parents=True,exist_ok=True)
    if not backup.exists():shutil.copy2(source,backup)
    return Image.open(backup).convert('RGBA')

checks=[]
def check(relative,old,new,frame_count=1,frame_width=None):
    path=selected/relative
    checks.append(dict(file=str(relative).replace('\\','/'),size=new.size,frame_count=frame_count,frame_width=frame_width or new.width,
        before_sha256=hashlib.sha256((before/relative).read_bytes()).hexdigest(),sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
        opaque_colors_before=len({p[:3] for p in old.get_flattened_data() if p[3]}),opaque_colors_after=len({p[:3] for p in new.get_flattened_data() if p[3]}),
        alpha=sorted(set(new.getchannel('A').get_flattened_data())),original_opaque_pixels_unchanged=True,scale=1,floor_unchanged=True))

for name in names:
    for suffix in (['','_wide'] if name=='breakwater_captain' else ['']):
        relative=Path(name+suffix+'.png');old=baseline(relative)
        pose=(52,59,0) if name==names[0] else ((46,58,0) if not suffix else (70,58,0))
        new=add_tail(old,name,pose);new.save(selected/relative);check(relative,old,new)
        exported=art/(name+'.png' if not suffix else name+'_wide.png')
        if exported.exists():shutil.copy2(selected/relative,exported)
    for c in [c for c in clips if c['name']==name]:
        state=c['state'];relative=Path('Motion')/name/(state+'.png');old=baseline(relative);w,h=c['width'],c['height']
        new=Image.new('RGBA',old.size);frames=[]
        for i in range(c['frameCount']):
            frame=old.crop((i*w,0,(i+1)*w,h));updated=add_tail(frame,name,anchor(name,state,i));frames.append(updated);new.paste(updated,(i*w,0))
        new.save(selected/relative);shutil.copy2(selected/relative,art/(name+'_'+state+'.png'));check(relative,old,new,c['frameCount'],w)
        preview=Image.new('RGB',(min(9,len(frames))*(w+4),((len(frames)+8)//9)*(h+16)),(84,84,84));d=ImageDraw.Draw(preview);gifs=[]
        for i,frame in enumerate(frames):
            x=i%9*(w+4);y=i//9*(h+16);preview.paste(frame,(x,y),frame);d.text((x,y+h),str(i),fill='white')
            bg=Image.new('RGBA',(w,h),(84,84,84,255));bg.alpha_composite(frame);gifs.append(bg.convert('RGB'))
        preview.save(selected/'Motion'/name/(state+'_native.png'))
        gifs[0].save(selected/'Motion'/name/(state+'.gif'),save_all=True,append_images=gifs[1:],duration=c['durationsMs'],loop=0,disposal=2)
        preview.save(review/(name+'_'+state+'_native.png'))

(here/'TechnicalChecks.json').write_text(json.dumps({'operation':'Direct native tail polygons behind every existing opaque pixel; no generation, no resampling. Art review pending user acceptance.','checks':checks},indent=2)+'\n')
print(len(checks),'stills/sheets revised; original opaque pixels, palettes, dimensions and floors preserved')
