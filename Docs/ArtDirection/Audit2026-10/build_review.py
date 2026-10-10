"""Read-only Unity sprite-binding audit; exports inspection copies, never game art.
Run from any directory with Python + Pillow. No network/provider calls.
"""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import re, json, hashlib, math, subprocess
from functools import lru_cache

ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).resolve().parent
MEDIA = OUT / 'media'
BOARDS = OUT / 'boards'
for folder in (MEDIA, BOARDS): folder.mkdir(exist_ok=True)
GUIDS = {}
SOURCES = {}
ISSUES = []
@lru_cache(maxsize=None)
def read(p): return p.read_text(encoding='utf-8-sig')
@lru_cache(maxsize=None)
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
@lru_cache(maxsize=None)
def load_image(p): return Image.open(p).convert('RGBA')
def rel(p): return p.relative_to(ROOT).as_posix()
def record(p):
    SOURCES[rel(p)] = sha(p)
def field(s, name, default=''):
    m = re.search(r'^  '+re.escape(name)+r': *(.*)$', s, re.M)
    return m.group(1).strip().strip("'") if m else default
def ref(s):
    m = re.search(r'fileID: (-?\d+), guid: ([a-f0-9]{32})', s)
    return (m.group(2), int(m.group(1))) if m else None
for p in (ROOT/'Assets').rglob('*.meta'):
    m = re.search(r'^guid: (\w+)', read(p), re.M)
    if m: GUIDS[m.group(1)] = p.with_suffix('')

SPRITES = {}
def sprite(r):
    if r in SPRITES: return SPRITES[r]
    if not r or r[0] not in GUIDS: raise ValueError(f'Unresolved sprite {r}')
    p = GUIDS[r[0]]; meta = p.with_name(p.name+'.meta'); s = read(meta)
    record(p); record(meta)
    im = load_image(p)
    crop = (0,0,im.width,im.height); name = p.stem; pivot = None
    if int(field(s, 'spriteMode', '1')) == 2:
        found = False
        for block in re.split(r'    - serializedVersion: \d+\s*\n', s.split('    sprites:',1)[1]):
            m = re.search(r'      internalID: (-?\d+)',block)
            if not m or int(m.group(1)) != r[1]: continue
            vals = {k:int(float(v)) for k,v in re.findall(r'        (x|y|width|height): ([\d.-]+)',block)}
            x,y,w,h = [vals[k] for k in ('x','y','width','height')]
            crop = (x, im.height-y-h, x+w, im.height-y)
            name = re.search(r'      name: (.+)', block).group(1)
            pm = re.search(r'      pivot: (.+)',block); pivot = pm.group(1) if pm else None
            found = True; break
        if not found: raise ValueError(f'Sprite fileID {r[1]} missing from {rel(meta)}')
    im = im.crop(crop)
    data = dict(path=rel(p), guid=r[0], fileID=r[1], name=name, crop_top_left=list(crop), size=list(im.size), pivot=pivot, sha256=sha(p))
    SPRITES[r] = (im,data)
    return im,data

def controller(p):
    record(p); record(p.with_name(p.name+'.meta'))
    s=read(p); states=[]
    for block in re.split(r'--- !u!',s):
        if not block.startswith('1102 '): continue
        r=ref(field(block,'m_Motion'))
        if not r: continue
        cp=GUIDS[r[0]]
        if cp.suffix!='.anim': raise ValueError(f'Unhandled motion type: {cp}')
        record(cp);record(cp.with_name(cp.name+'.meta'))
        cs=read(cp); keys=[]
        curves=cs.split('  m_PPtrCurves:',1)[1].split('  m_SampleRate:',1)[0]
        for t,f,g in re.findall(r'- time: ([\d.eE+-]+)\s+value: \{fileID: (-?\d+), guid: ([a-f0-9]{32}), type: \d+\}',curves):
            im,sd=sprite((g,int(f)))
            keys.append((float(t),im,sd))
        if not keys: raise ValueError(f'No sprite keys: {rel(cp)}')
        stop=float(re.search(r'    m_StopTime: ([\d.eE+-]+)',cs).group(1))
        speed=float(field(block,'m_Speed','1'))
        # Retain the original key count; consolidate consecutive identical sprites into their holds.
        frames=[]
        for t,im,sd in keys:
            if frames and sd==frames[-1][2]: continue
            frames.append((t,im,sd))
        durations=[((frames[i+1][0] if i+1<len(frames) else stop)-v[0])/speed for i,v in enumerate(frames)]
        if min(durations)<=0: raise ValueError(f'Invalid clip exposures: {rel(cp)}')
        w=max(x[1].width for x in frames); h=max(x[1].height for x in frames)
        atlas=Image.new('RGBA',(w*len(frames),h))
        for i,(_,im,sd) in enumerate(frames): atlas.paste(im,(i*w,0))
        outname=cp.stem+'-'+sha(cp)[:8]+'.png'; atlas.save(MEDIA/outname)
        states.append(dict(name=field(block,'m_Name'), clip=rel(cp), clip_sha256=sha(cp), controller_speed=speed,
            duration=stop/speed, loop='m_LoopTime: 1' in cs, media='media/'+outname, size=[w,h],
            frames=[dict(time=t/speed,duration=durations[i],sprite=sd) for i,(t,im,sd) in enumerate(frames)],
            sprite_key_count=len(keys), events=re.findall(r'functionName: (.*)',cs)))
    return states

def make_actor(p, prefix='', controller_field='animationControllerOverride', sprite_field='fallbackVisualSprite', name=None, zone=None):
    s=read(p);record(p);record(p.with_name(p.name+'.meta'))
    aid=prefix or field(s,'enemyId') or field(s,'playerId')
    z=zone or ('Magical Forest' if '/Forest/' in rel(p) else 'Drowned Court' if '/DrownedCourt/' in rel(p) else 'Ironvein Excavation' if '/Ironvein/' in rel(p) else 'Dungeon')
    cr=ref(field(s,controller_field));sr=ref(field(s,sprite_field))
    if not cr: raise ValueError(f'No controller on {p}')
    cp=GUIDS[cr[0]]; states=controller(cp)
    still,sd=sprite(sr);stillname=aid+'-still.png';still.save(MEDIA/stillname)
    return dict(id=aid,name=name or field(s,'displayName'),zone=z,definition=rel(p),controller=rel(cp),
        still='media/'+stillname,still_binding=sd,visual_size=field(s,'visualSize'),states=states,
        category=field(s,'category'),description=field(s,'description'),eligible_zones=re.findall(r'^  - ((?:dungeon|magical-forest|drowned-court|ironvein-excavation))$',s,re.M))

actors=[]
for p in sorted((ROOT/'Assets/_Game/Data/Enemies').rglob('*.asset')):
    if field(read(p),'enemyId'): actors.append(make_actor(p))
for p in sorted((ROOT/'Assets/_Game/Resources/Players').glob('*.asset')):
    if field(read(p),'playerId'): actors.append(make_actor(p,controller_field='battleAnimatorController',sprite_field='battleCharacterSprite',zone='Players'))
boss=ROOT/'Assets/_Game/Data/Enemies/Ironvein/grand_delver.asset'
for suffix, label in [('Pilot','Grand Delver — pilot'),('Reserve','Grand Delver — reserve suit')]:
    a=make_actor(boss,prefix='grand_delver_'+suffix.lower(),controller_field='mine'+suffix+'Controller',sprite_field='mine'+suffix+'Sprite',name=label,zone='Ironvein alternates (disabled)')
    a['availability']='Serialized optional remount experiment; mineEnableRemount = 0. Not active in the default production encounter.'
    actors.append(a)

font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',17)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',12)
def draw_contact(a):
    rows=[]; width=1200
    for st in a['states']:
        w,h=st['size']; cols=min(12,(width-180)//w); count=len(st['frames'])
        for start in range(0,count,cols): rows.append((st,start,min(start+cols,count),h+24))
    header=max(120,68+a['still_binding']['size'][1])
    height=header+sum(row[3] for row in rows)
    board=Image.new('RGB',(width,height),'#222636'); d=ImageDraw.Draw(board)
    d.text((12,10),a['name']+' | '+a['zone'],font=font,fill='white')
    d.text((12,36),'ALL BOUND DRAWINGS at 1x; full source rectangles, top-left aligned; duplicate endpoint holds consolidated.',font=small,fill='#cbd0dd')
    im=Image.open(OUT/a['still']); board.paste(im,(12,56),im)
    d.text((180,60),'Fallback still above/left; each animation row uses its actual imported canvas. No silhouette scaling.',font=small,fill='#cbd0dd')
    y=header
    for st,start,end,rh in rows:
        atlas=Image.open(OUT/st['media']);w,h=st['size']
        d.text((8,y+3),st['name'],font=small,fill='white')
        d.text((8,y+20),f'{st["duration"]:.3f}s / '+('loop' if st['loop'] else 'once'),font=small,fill='#bec3cf')
        for j in range(start,end):
            im=atlas.crop((j*w,0,(j+1)*w,h));x=180+(j-start)*w
            bg='#ddd9d0' if j%2 else '#111421'; d.rectangle((x,y,x+w-1,y+h-1),fill=bg)
            board.paste(im,(x,y),im)
            d.text((x+2,y+h+2),f'{j+1}:{round(st["frames"][j]["duration"]*1000)}ms',font=small,fill='#cbd0dd')
        y+=rh
    path=BOARDS/(a['id']+'-frames.png');board.save(path);a['contact_board']='boards/'+path.name
for a in actors: draw_contact(a)

def still_board(group, filename):
    cols=5;cellw=280;cellh=310
    im=Image.new('RGB',(cols*cellw,60+math.ceil(len(group)/cols)*cellh),'#222636');d=ImageDraw.Draw(im)
    d.text((12,10),filename+' | ready / fallback at 2x, unchanged source canvas; top-left aligned',font=font,fill='white')
    for n,a in enumerate(group):
        x=(n%cols)*cellw;y=60+(n//cols)*cellh
        d.text((x+6,y),a['name'],font=font,fill='white')
        src=Image.open(OUT/a['still']);sw,sh=src.size
        d.text((x+6,y+22),f'{sw} x {sh} native',font=small,fill='#cbd0dd')
        src=src.resize((sw*2,sh*2),Image.Resampling.NEAREST);im.paste(src,(x+6,y+44),src)
    im.save(BOARDS/(filename+'.png'))
for zone in dict.fromkeys(a['zone'] for a in actors):
    still_board([a for a in actors if a['zone']==zone],re.sub('[^a-z0-9]+','-',zone.lower()).strip('-'))
data=dict(date='2026-10-11',revision=subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip(),
    guide_version='1.12',actors=actors,source_hashes=SOURCES,issues=ISSUES,
    method='EnemyDefinition/PlayerDefinition -> assigned controller -> AnimatorState motion -> AnimationClip sprite fileID/GUID -> imported rectangle. Exact exposure tables; original full rectangles; no production pixels edited.')
(OUT/'bindings.json').write_text(json.dumps(data,indent=2,ensure_ascii=False),encoding='utf-8')
print(json.dumps(dict(actors=len(actors),states=sum(len(a['states']) for a in actors),drawings=sum(len(s['frames']) for a in actors for s in a['states']),sources=len(SOURCES),zones={z:sum(a['zone']==z for a in actors) for z in dict.fromkeys(a['zone'] for a in actors)}),indent=2))
