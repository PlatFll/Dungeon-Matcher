"""Prepare native separation masks for LibreSprite, then independently check exports."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import json, hashlib, sys, re, shutil, subprocess
P=Path(__file__).resolve().parents[4]
R=P/'ArtSource/DungeonHybrid/Stage02'
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p): return Image.open(p).convert('RGBA')
names=['Gate_Recess','Gate_Iron','Gate_Lock','Arch_Left','Arch_Right','Jamb_Left','Jamb_Right','Skull_Keystone','Relief_Left','Relief_Right']
raw=load(R/'Sources/Gate_Raw_PixelLab.png');clean=load(R/'Exports/Gate_Clean.png')
dark=(16,19,31,255)
if len(sys.argv)==1 or sys.argv[1]=='prepare':
    (P/'.utmp/hybrid-stage02-masks').mkdir(parents=True,exist_ok=True)
    # Opening boundaries follow the original source's quiet dark arch, not an ellipse resample.
    inside={}
    for y in range(26,118):
        if y>=52: inside[y]=(28,92);continue
        runs=[];start=None
        for x in range(27,94):
            yes=x<93 and raw.getpixel((x,y))==dark
            if yes and start is None:start=x
            if not yes and start is not None:
                if x-start>=4:runs.append((start,x))
                start=None
        assert runs,(y,'missing arch opening')
        inside[y]=(min(a for a,b in runs),max(b for a,b in runs))
    owners={n:[] for n in names};under={n:[] for n in names}
    for y in range(128):
        for x in range(128):
            c=clean.getpixel((x,y));i=y*128+x
            if not c[3]:continue
            if 12<=x<27 and 67<=y<86:n='Relief_Left'
            elif 94<=x<109 and 67<=y<86:n='Relief_Right'
            elif 49<=x<72 and y<28:n='Skull_Keystone'
            elif x>=89 and y>=52:n='Jamb_Right'
            elif y in inside and inside[y][0]<=x<inside[y][1]:
                n='Gate_Recess' if c==dark else ('Gate_Lock' if 79<=x<=89 and 70<=y<=83 else 'Gate_Iron')
                if n!='Gate_Recess':under['Gate_Recess'].append(i)
            elif y>=52:n='Jamb_Left' if x<60 else 'Jamb_Right'
            else:n='Arch_Left' if x<60 else 'Arch_Right'
            owners[n].append(i)
            if n.startswith('Relief_'):under['Jamb_'+n.split('_')[1]].append(i)
    # All 10 pieces have two native transparent pixels beyond their occupied bounds.
    parts=[]
    for n in names:
        ids=owners[n]+under[n];xs=[i%128 for i in ids];ys=[i//128 for i in ids]
        x0,y0,x1,y1=min(xs)-2,min(ys)-2,max(xs)+3,max(ys)+3
        assert min(x0,y0)>=0 and max(x1,y1)<=128
        parts.append(dict(name=n,offset=[x0,y0],width=x1-x0,height=y1-y0,
                          path='Assets/_Game/Art/DungeonHybrid/Stage02/'+n+'.png',
                          layer=names.index(n),keep=owners[n],under=under[n],under_color='10131f' if n=='Gate_Recess' else '4e3d6c'))
    script="// Generated coordinate masks. Execute in LibreSprite after CleanupGate.js.\nvar parts="+json.dumps(parts,separators=(',',':'))+";\n"
    script+='''
var c=app.pixelColor;
function rgb(h){return c.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
for(var p=0;p<parts.length;p++){
 var part=parts[p],doc=app.open('ArtSource/DungeonHybrid/Stage02/Exports/Gate_Clean.png');
 var s=doc.sprite,im=s.layer(0).cel(0).image,keep={},under={};
 if(s.width!==128||s.height!==128||im.width!==128||im.height!==128)throw new Error('Unexpected source dimensions');
 for(var k=0;k<part.keep.length;k++)keep[part.keep[k]]=true;
 for(var k=0;k<part.under.length;k++)under[part.under[k]]=true;
 for(var y=0;y<128;y++)for(var x=0;x<128;x++){
   var i=y*128+x;if(under[i])im.putPixel(x,y,rgb(part.under_color));else if(!keep[i])im.putPixel(x,y,0);
 }
 s.commit();
 // This build's batch Sprite.crop does not apply; use its verified CLI --crop.
 s.saveAs('.utmp/hybrid-stage02-masks/'+part.name+'.png',true);
}
'''
    (R/'Scripts/SeparateGate.js').write_text(script)
    compact=[{k:v for k,v in a.items() if k not in ['keep','under','under_color']} for a in parts]
    (R/'Assembly.json').write_text(json.dumps(dict(width=128,height=128,scene_origin=[173,60],parts=compact),indent=2)+'\n')
    print('Prepared 10 native LibreSprite parts')
elif sys.argv[1]=='export':
    exe='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
    m=json.loads((R/'Assembly.json').read_text())
    for a in m['parts']:
        crop=','.join(map(str,a['offset']+[a['width'],a['height']]))
        src=P/'.utmp/hybrid-stage02-masks'/(a['name']+'.png')
        ase=R/'Native'/(a['name']+'.aseprite');png=R/'Exports'/(a['name']+'.png')
        for args in [['--crop',crop,str(src),'--save-as',str(ase)],[str(ase),'--save-as',str(png)]]:
            result=subprocess.run(['rtk','proxy',exe,'-b']+args,capture_output=True,text=True,cwd=P)
            if result.returncode:raise RuntimeError(result.stdout+result.stderr)
        assert load(png).size==(a['width'],a['height'])
    print('LibreSprite cropped, saved and reopened all 10 editable native sources')
else:
    m=json.loads((R/'Assembly.json').read_text());assembled=Image.new('RGBA',(128,128));rows=[]
    dest=P/'Assets/_Game/Art/DungeonHybrid/Stage02';dest.mkdir(parents=True,exist_ok=True)
    for a in m['parts']:
        f=R/'Exports'/(a['name']+'.png');im=load(f);assert im.size==(a['width'],a['height'])
        assert set(im.getchannel('A').getdata())=={0,255};b=im.getbbox()
        assert b==(2,2,im.width-2,im.height-2),(a['name'],b)
        assembled.alpha_composite(im,tuple(a['offset']));shutil.copyfile(f,P/a['path'])
        rows.append(dict(**a,bounds=b,opaque_colors=len({c[:3] for c in im.getdata() if c[3]}),alpha=[0,255],sha256=sha(f),source='Native/'+a['name']+'.aseprite',aseprite_sha256=sha(R/'Native'/(a['name']+'.aseprite'))))
    assert assembled.tobytes()==clean.tobytes(),'Modular reassembly must exactly match the cleaned gate'
    (R/'Diagnostics/AssetAudit.json').write_text(json.dumps(dict(result='PASS',source_canvas=clean.size,source_bounds=clean.getbbox(),opaque_colors=len({c[:3] for c in clean.getdata() if c[3]}),alpha=[0,255],pieces=rows,exact_modular_reassembly=True,raw_sha256=sha(R/'Sources/Gate_Raw_PixelLab.png'),clean_sha256=sha(R/'Exports/Gate_Clean.png')),indent=2)+'\n')
    # Inspection boards only; never imported into Unity.
    font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12)
    board=Image.new('RGBA',(680,330),(25,25,38,255));d=ImageDraw.Draw(board)
    d.text((10,8),'PixelLab source / 2x',font=font);d.text((278,8),'LibreSprite cleaned / 2x',font=font)
    board.alpha_composite(raw.resize((256,256),Image.Resampling.NEAREST),(4,30));board.alpha_composite(clean.resize((256,256),Image.Resampling.NEAREST),(272,30))
    board.alpha_composite(load(P/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png').resize((128,128),Image.Resampling.NEAREST),(540,148))
    d.text((540,285),'Rattlebones 2x',font=font);d.text((10,305),'Same 128x128 canvas. Native pixels; no scaling in source art.',font=font)
    board.save(R/'Previews/Source-clean-reference.png')
    board=Image.new('RGBA',(640,340),(25,25,38,255));d=ImageDraw.Draw(board)
    for k,a in enumerate(rows):
        x=(k%5)*128;y=(k//5)*170;im=load(R/'Exports'/(a['name']+'.png'))
        d.text((x+4,y+4),a['name'],font=font);d.text((x+4,y+20),f'{im.width}x{im.height} / 1x',font=font)
        board.alpha_composite(im,(x+4,y+42))
    board.save(R/'Previews/Modules-native-1x.png')
    print('Verified 10 pieces, binary alpha, 2px padding and exact modular reassembly')
