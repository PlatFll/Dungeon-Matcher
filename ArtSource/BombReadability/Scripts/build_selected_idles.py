from pathlib import Path
from PIL import Image, ImageDraw
import math, json, hashlib, subprocess, uuid, struct, shutil
ROOT=Path(__file__).resolve().parents[3] if 'ArtSource' in str(__file__) else Path(r'C:/Users/USER/.codex/worktrees/menu-ali-edited-background/Dungeon Matcher')
SRC=ROOT/'ArtSource/BombReadability/Candidates/Cleaned'
OUT=ROOT/'ArtSource/BombReadability/SelectedIdles'
RES=ROOT/'Assets/_Game/Resources/VFX/BombRefinement/Idles'
LIBRE=r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
for d in [OUT/'Editable',OUT/'Sheets',OUT/'Preview',RES]:d.mkdir(parents=True,exist_ok=True)
def canonical(im):
    im=im.convert('RGBA'); im.putdata([p if p[3] else (0,0,0,0) for p in im.getdata()]); return im
base={n:canonical(Image.open(SRC/(n+'.png'))) for n in ['PoisonA','HealingC','DirectionalD','DirectionalD-Column','StarA']}
green=[(70,101,68,255),(98,156,84,255),(140,206,99,255),(186,237,164,255)]
def poison(i):
    im=base['PoisonA'].copy(); d=ImageDraw.Draw(im)
    # Clear only the two detached generated bubbles. Preserve the rim and shell.
    d.rectangle((20,0,39,9),fill=(0,0,0,0))
    # Liquid highlights flow one source pixel; the metal vessel never moves.
    for y in range(10,20):
        for x in range(14,52):
            p=base['PoisonA'].getpixel((x,y))
            if p in green:
                dx=[0,1,0,-1][i//6]
                neighbor=base['PoisonA'].getpixel((x+dx,y))
                if neighbor in green:im.putpixel((x,y),neighbor)
    # Two staggered surface bubbles: swell, lift, open ring, four-pixel pop, rest.
    for cx,phase in [(25,i%24),(36,(i+12)%24)]:
        if phase<3: continue
        cy=13-max(0,(phase-5)//2)
        if phase<8:
            r=1 if phase<5 else 2
            d.ellipse((cx-r,cy-r,cx+r,cy+r),fill=green[1]);d.point((cx-1,cy-1),fill=green[3])
        elif phase<15:
            d.ellipse((cx-2,cy-2,cx+2,cy+2),fill=green[0]);d.ellipse((cx-1,cy-1,cx+1,cy+1),fill=green[2]);d.point((cx-1,cy-1),fill=green[3])
        elif phase<18:
            d.ellipse((cx-2,cy-2,cx+2,cy+2),outline=green[2]);d.point((cx-1,cy-2),fill=green[3])
        elif phase<20:
            for dx,dy in [(-3,0),(3,0),(0,-3),(0,2)]:d.point((cx+dx,cy+dy),fill=green[2])
    return im
def healing(i):
    im=base['HealingC'].copy();d=ImageDraw.Draw(im)
    for x,start,y0 in [(17,4,24),(46,14,26),(23,21,22)]:
        age=i-start
        if 0<=age<9:
            y=y0-age*2; x+=(-age//4 if x<32 else age//4); r=1 if age<2 or age>6 else 2
            color='#FFD35B' if age<6 else '#C58826'
            d.line((x-r,y,x+r,y),fill=color);d.line((x,y-r,x,y+r),fill=color)
            if 2<=age<6:d.point((x,y),fill='#FFF7C0')
    return im
# Use the complete reflective footprint, including the farthest star tip.
star_facets=[(x,y) for y in range(64) for x in range(64)
             if base['StarA'].getpixel((x,y))[3] and min(base['StarA'].getpixel((x,y))[:3])>75]
star_sweep_start=min(x+y for x,y in star_facets)-4
star_sweep_end=max(x+y for x,y in star_facets)+4
def star(i):
    im=base['StarA'].copy()
    # Enter before the first tip and leave beyond the opposite tip within the same exposures.
    if 7<=i<=22:
        beam=round(star_sweep_start+(star_sweep_end-star_sweep_start)*(i-7)/15)
        for y in range(64):
            for x in range(64):
                p=im.getpixel((x,y));dist=abs(x+y-beam)
                if p[3] and min(p[:3])>75 and dist<4:
                    im.putpixel((x,y),(255,255,255,255) if dist<2 else (243,250,255,255))
    # Keep every contour pixel anchored; only the facet colors glint.
    return im
families={'Poison':([poison(i) for i in range(24)],100,[32,38]),'Healing':([healing(i) for i in range(32)],100,[32,36]),'Star':([star(i) for i in range(32)],80,None)}
manifest=[]
template=(ROOT/'Assets/_Game/Resources/VFX/BombRefinement/FireHead/FireHead_00.png.meta').read_text()
def meta(p):
    m=Path(str(p)+'.meta')
    if not m.exists():
        import re
        t=re.sub(r'guid: \w+', 'guid: '+uuid.uuid4().hex,template,count=1)
        m.write_text(t)
def editable(name,ims,ms):
    colors=sorted({p[:3] for im in ims for p in im.getdata() if p[3]});assert len(colors)<255
    palette=[0,0,0]+[v for c in colors for v in c];palette+=[0]*(768-len(palette));lookup={c:i+1 for i,c in enumerate(colors)};pal=[]
    for im in ims:
        q=Image.new('P',im.size);q.putpalette(palette);q.putdata([lookup[p[:3]] if p[3] else 0 for p in im.getdata()]);pal.append(q)
    gif=OUT/'Editable'/f'{name}-native.gif';pal[0].save(gif,save_all=True,append_images=pal[1:],duration=ms,loop=0,transparency=0,disposal=2,optimize=False)
    ase=OUT/'Editable'/f'{name}.aseprite';subprocess.run([LIBRE,'-b',str(gif),'--save-as',str(ase)],check=True,capture_output=True)
    # GIF coalesces identical holds. Re-expand exact exposures in the native cel file.
    data=ase.read_bytes();head=bytearray(data[:128]);off=128;expanded=[]
    for frame_index in range(struct.unpack_from('<H',data,6)[0]):
        length=struct.unpack_from('<I',data,off)[0];frame=bytearray(data[off:off+length]);duration=struct.unpack_from('<H',frame,8)[0]
        count=round(duration/ms);assert count>=1 and duration==count*ms
        struct.pack_into('<H',frame,8,ms)
        # Duplicates need only the cel chunk; keep first frame's palette/layer metadata once.
        pos=16;cel=[]
        for _ in range(struct.unpack_from('<H',frame,6)[0]):
            size,typ=struct.unpack_from('<IH',frame,pos)
            if typ==0x2005:cel.append(bytes(frame[pos:pos+size]))
            pos+=size
        expanded.append(bytes(frame))
        for repeat in range(count-1):
            payload=b''.join(cel);fh=bytearray(16);struct.pack_into('<IHHH',fh,0,len(payload)+16,0xF1FA,len(cel),ms);expanded.append(bytes(fh)+payload)
        off+=length
    assert len(expanded)==len(ims),(name,len(expanded),len(ims))
    struct.pack_into('<I',head,0,128+sum(map(len,expanded)));struct.pack_into('<H',head,6,len(expanded));ase.write_bytes(bytes(head)+b''.join(expanded))
    sheet=OUT/'Sheets'/f'{name}.png';subprocess.run([LIBRE,'-b',str(ase),'--sheet',str(sheet),'--sheet-type','horizontal'],check=True,capture_output=True)
    exported=canonical(Image.open(sheet));assert exported.width==len(ims)*64,(name,exported.size,len(ims))
    for i,im in enumerate(ims):assert exported.crop((i*64,0,(i+1)*64,64)).tobytes()==canonical(im.copy()).tobytes(),(name,i)
def preview(name,ims,ms,socket=None):
    gem=Image.open(ROOT/'Assets/_Game/Art/Gems/Small gems/Gesm16/Emerald16.png').convert('RGBA');views=[]
    for i,im in enumerate(ims):
        c=Image.new('RGBA',(64,64),'#202837');c.alpha_composite(im)
        if socket:c.alpha_composite(gem,(socket[0]-8,socket[1]-8))
        views.append(c.convert('RGB').resize((256,256),Image.Resampling.NEAREST))
    views[0].save(OUT/'Preview'/f'{name}.gif',save_all=True,append_images=views[1:],duration=ms,loop=0)
for name,(ims,ms,socket) in families.items():
    folder=RES/name;folder.mkdir(exist_ok=True)
    for i,im in enumerate(ims):
        p=folder/f'{name}_{i:02}.png';im.save(p);meta(p)
        assert set(im.getchannel('A').getdata())<={0,255}
        b=im.getbbox();assert b[0]>0 and b[1]>0 and b[2]<64 and b[3]<64,(name,i,b)
    editable(name,ims,ms);preview(name,ims,ms,socket)
    manifest.append({'family':name,'frames':len(ims),'milliseconds':ms,'canvas':[64,64],'socket':socket,'binaryAlpha':True})
# Rock the complete rigid bomb about its socket: no separate moving sides,
# no translation, and no filtered pixels. Bake the whole-unit turn into native cels.
angles=[0,3,0,-3,0,3,0,-3]
for name,src,socket in [('Row','DirectionalD',[32,32]),('Column','DirectionalD-Column',[32,31])]:
    ims=[];folder=RES/(name+'Idle');folder.mkdir(exist_ok=True)
    for i,angle in enumerate(angles):
        im=base[src].rotate(angle,resample=Image.Resampling.NEAREST,center=tuple(socket))
        # At this tiny angle, every native 16px gem remains the exact same pixel
        # arrangement. Verify composing the fixed gem equals turning the whole unit.
        for gp in (ROOT/'Assets/_Game/Art/Gems/Small gems/Gesm16').glob('*.png'):
            gem=canonical(Image.open(gp));combined=base[src].copy()
            combined.alpha_composite(gem,(socket[0]-8,socket[1]-8))
            combined=combined.rotate(angle,resample=Image.Resampling.NEAREST,center=tuple(socket))
            separate=im.copy();separate.alpha_composite(gem,(socket[0]-8,socket[1]-8))
            assert combined.tobytes()==separate.tobytes(),(name,i,gp.name,'rigid gem alignment')
        bounds=im.getbbox();assert min(bounds[0],bounds[1],64-bounds[2],64-bounds[3])>0
        p=folder/f'{name}_{i:02}.png';im.save(p);meta(p);ims.append(im)
    editable(name,ims,80);preview(name,ims,80,socket)
    manifest.append({'family':name,'resourceFolder':name+'Idle','frames':8,'milliseconds':80,'canvas':[64,64],'socket':socket,'binaryAlpha':True,'fixedSocket':True,'anglesDegrees':angles,'runtime':'whole rigid bomb rocks around fixed socket; baked nearest-pixel cels, no root translation'})
    p=RES/f'{name}.png';base[src].save(p);meta(p)
# Preserve existing Unity GUIDs for approved replacements; static fallbacks use exact chosen sources.
for src,dst in [('PoisonA','Assets/_Game/Art/Board/Bombs/PoisonBomb.png'),('HealingC','Assets/_Game/Art/Board/Bombs/HealingBomb.png'),('StarA','Assets/_Game/Art/Gems/Special Overlays/StarColorBomb.png')]:shutil.copyfile(SRC/(src+'.png'),ROOT/dst)
for name,ims,ms in [('Poison',families['Poison'][0],100),('Healing',families['Healing'][0],100),('Star',families['Star'][0],80)]:
    board=Image.new('RGBA',(64*8,64*4),'#202837')
    for i,im in enumerate(ims):board.alpha_composite(im,(i%8*64,i//8*64))
    board.resize((1024,512),Image.Resampling.NEAREST).save(OUT/'Preview'/f'{name}-frames.png')
(OUT/'Manifest.json').write_text(json.dumps({'selected':['Poison A','Healing C','Directional D','Star A'],'sequences':manifest,'generationSpending':0,'method':'Direct native cel editing; LibreSprite import/export with exact RGBA round-trip check'},indent=2)+'\n')
print('Selected idles exported and exact LibreSprite round trips verified.')
