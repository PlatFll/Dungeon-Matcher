"""Read-only pixel validation and review evidence; all production pixels are authored in LibreSprite."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib,json,struct,zlib,html
K=Path(__file__).resolve().parent;P=K.parents[2]
def decode(path):
    b=path.read_bytes();w,h=struct.unpack_from('<HH',b,8);i=144
    while i<len(b):
        size,kind=struct.unpack_from('<IH',b,i);s=i+6
        if kind==0x2005:
            x,y=struct.unpack_from('<hh',b,s+2);ctype=struct.unpack_from('<H',b,s+7)[0]
            cw,ch=struct.unpack_from('<HH',b,s+16);raw=b[s+20:i+size]
            if ctype==2:raw=zlib.decompress(raw)
            out=Image.new('RGBA',(w,h));out.paste(Image.frombytes('RGBA',(cw,ch),raw),(x,y));return out
        i+=size
    raise ValueError('Missing cel '+str(path))
def pixels(im):return list(im.get_flattened_data())
manifest=json.loads((K/'AssetManifest.json').read_text())
records=[];groups={};pages=[]
for source in manifest['modules']:
    group,name=source['group'],source['name'];p=K/f'Exports/{group}/{name}.png'
    im=Image.open(p).convert('RGBA');ase=decode(K/f'Editable/{group}/{name}.aseprite')
    assert im.size==(source['width'],source['height']),name
    assert im.tobytes()==ase.tobytes(),('Native source/export mismatch',name)
    alpha=set(im.getchannel('A').get_flattened_data());assert alpha<={0,255},name
    sourcefile=P/source['source'];before=None
    if sourcefile.is_file():
        before=Image.open(sourcefile).convert('RGBA')
        if source.get('crop'):
            x,y,w,h=source['crop'];before=before.crop((x,y,x+w,y+h))
    exact=before is not None and before.tobytes()==im.tobytes()
    mask_equal=before is not None and before.getchannel('A').tobytes()==im.getchannel('A').tobytes()
    colors=len({p[:3] for p in pixels(im) if p[3]})
    retained=True if before is None else all(a==b for a,b in zip(pixels(im),pixels(before)) if a[3])
    if source['changes'].startswith('Lossless native reuse') or source['changes']=='Lossless native extraction.':assert exact,name
    if 'isolation' in source['changes'] and not source.get('repairRects'):assert retained,name
    if source.get('repairRects'):
        for y in range(im.height):
            for x in range(im.width):
                if any(rx<=x<rx+rw and ry<=y<ry+rh for rx,ry,rw,rh in source['repairRects']):continue
                if im.getpixel((x,y))[3]:assert im.getpixel((x,y))==before.getpixel((x,y)),(name,x,y)
    rec=dict(source,opaque_colors=colors,alpha=sorted(alpha),bounds=im.getbbox(),ase_png_exact=True,
        source_pixels_exact=exact,source_mask_exact=mask_equal,retained_pixels_exact=retained,
        sha256=hashlib.sha256(p.read_bytes()).hexdigest(),visual_status='Pending contact-sheet and Unity inspection')
    records.append(rec);groups.setdefault(group,[]).append((name,before,im))
    dest=K/f'Review/Assets/{group}';dest.mkdir(parents=True,exist_ok=True)
    if before is not None:before.save(dest/f'{name}-before.png')
    sw,sh=im.size;cell=max(330,sw*3+20);height=sh*4+100
    board=Image.new('RGB',(cell*2,height),'#242430');d=ImageDraw.Draw(board)
    for index,(label,img) in enumerate([('Source crop',before),('Native LibreSprite module',im)]):
        x=index*cell+10;d.text((x,8),name+' - '+label,fill='white')
        if img is None:continue
        board.paste(img,(x,35),img);board.paste(img.resize((sw*3,sh*3),Image.Resampling.NEAREST),(x,sh+65),img.resize((sw*3,sh*3),Image.Resampling.NEAREST))
    board.save(dest/f'{name}-comparison.png')
    pages.append(f'<article><h3>{html.escape(group+" / "+name)}</h3><p>{sw}×{sh}, {colors} opaque colors; binary alpha; source/export exact. Source pixels exact: {exact}; source mask exact: {mask_equal}.</p><p>{html.escape(source["changes"])}</p><img src="Assets/{group}/{name}-comparison.png"></article>')
for group,items in groups.items():
    width=1200;row=0;x=12;y=32;positions=[]
    for name,before,im in items:
        w=max(230,im.width*3+20);h=im.height*3+40
        if x+w>width and x>12:x=12;y+=row;row=0
        positions.append((name,im,x,y));x+=w;row=max(row,h)
    board=Image.new('RGB',(max(width,max(x+im.width*3+12 for _,im,x,y in positions)),y+row+12),'#242430');d=ImageDraw.Draw(board)
    for name,im,x,y in positions:
        d.text((x,y),name,fill='white');large=im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST);board.paste(large,(x,y+20),large)
    board.save(K/f'Review/{group}-contact.png')
seams={}
for material in ['PortraitWall','BattleWall','OuterWall']:
    tiles=[Image.open(K/f'Exports/WallTiles/{material}{v}.png').convert('RGBA') for v in 'ABCD']
    for a in tiles:
        assert a.size==(64,64)
        for b in tiles:
            assert a.crop((0,0,1,64)).tobytes()==b.crop((63,0,64,64)).tobytes()
            assert a.crop((0,0,64,1)).tobytes()==b.crop((0,63,64,64)).tobytes()
    grid=Image.new('RGBA',(256,256))
    for y in range(4):
        for x in range(4):grid.paste(tiles[(x+y*3)%4],(x*64,y*64))
    grid.save(K/f'Review/{material}-repeat.png');seams[material]='16 horizontal + 16 vertical edge pairings pass'
for r in records:
    if 'repeat-end repair' not in r['changes']:continue
    a=Image.open(K/f'Exports/{r["group"]}/{r["name"]}.png')
    w,h=a.size
    horizontal='Top' in r['name'] or 'Bottom' in r['name']
    assert (a.crop((0,0,1,h)).tobytes()==a.crop((w-1,0,w,h)).tobytes()) if horizontal else (a.crop((0,0,w,1)).tobytes()==a.crop((0,h-1,w,h)).tobytes()),r['name']
(K/'Evidence/AssetChecks.json').write_text(json.dumps({'assets':records,'wall_seams':seams,'rail_repeat_edges':'pass','artistic_approval':'Pending visual review; technical checks do not establish style approval'},indent=2)+'\n')
(K/'Review/Assets.html').write_text('<!doctype html><meta charset="utf-8"><title>Dungeon modular per-asset review</title><style>body{background:#161821;color:#eee;font:15px sans-serif;margin:28px}article{margin:30px 0;padding:20px;background:#242430}img{image-rendering:pixelated;max-width:100%}p{max-width:1000px}</style><h1>Dungeon modular assets — native before/after and 3×</h1><p>Sources are immutable approved native art. Boundary repairs and isolation changes are identified individually. This is a review candidate, not user-approved production art.</p>'+''.join(pages),encoding='utf-8')
print(json.dumps({'modules':len(records),'all_native_exports_exact':True,'binary_alpha':True,'wall_pairs_per_material':32,'rail_seams':'pass'}))
