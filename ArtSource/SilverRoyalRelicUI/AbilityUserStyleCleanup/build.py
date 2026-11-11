"""Native LibreSprite cleanup derived from the user's hand-cleaned silver UI."""
from pathlib import Path
from PIL import Image, ImageDraw
from collections import Counter
import hashlib, json, struct, subprocess, shutil, zlib

ROOT=Path(__file__).resolve().parents[3]
ART=ROOT/'ArtSource/SilverRoyalRelicUI'
OUT=ART/'AbilityUserStyleCleanup'
EXE=r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
for sub in ('Inputs','Editable','PNG','QA','Preview'): (OUT/sub).mkdir(parents=True,exist_ok=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def backup(src,dst):
    if dst.exists(): assert sha(src)==sha(dst),f'Preserve existing source {dst}'
    else: shutil.copy2(src,dst)
for n in ('SettingsButtonSilver','PowerUpTileSilver','WavePlaqueSilver'):
    backup(Path('C:/Users/USER/Downloads')/(n+'.png'),OUT/'Inputs'/(n+'.png'))
backup(ART/'UserEdits/AbilityButton.png',OUT/'Inputs/AbilityButton-Before.png')
backup(ART/'UserEdits/AbilityButton.ase',OUT/'Inputs/AbilityButton-Before.ase')
backup(ART/'Stage03/PNG/AbilityFrame.png',OUT/'Inputs/AbilityFrame-Before.png')
backup(ART/'Stage03/PNG/RattlebonesEmblem.png',OUT/'Inputs/RattlebonesEmblem.png')

wave=Image.open(OUT/'Inputs/WavePlaqueSilver.png').convert('RGBA')
before=Image.open(OUT/'Inputs/AbilityButton-Before.png').convert('RGBA')
icon=Image.open(OUT/'Inputs/RattlebonesEmblem.png').convert('RGBA')
assert wave.size==before.size==(176,66)
transparent=(0,0,0,0); board=(28,36,56,255); recess=(17,16,27,255)
# Preserve the user's bevel corners and ornaments at their original pixel size.
# Extend only two straight, ornament-free side segments, seven pixels each.
# Restore the straight lower rail behind the wave's separate hanging skull;
# this button retains its existing, separate crowned ability emblem instead.
rows=list(range(6,27))+[26]*7+list(range(27,43))+[42]*7+list(range(43,61))
assert len(rows)==69
pixels={}
for dy,sy in enumerate(rows,1):
    if dy>=66: continue
    for x in range(176):
        c=wave.getpixel((35 if 70<=x<=104 and sy>=43 else x,sy))
        if c==recess: c=board
        pixels[x,dy]=c
for y in range(66):
    for x in range(176):pixels.setdefault((x,y),transparent)

plan=[[x,y,*pixels[x,y]] for y in range(66) for x in range(176)]
src=(OUT/'Inputs/AbilityFrame-Before.png').as_posix()
ase=(OUT/'Editable/AbilityFrameSilver.ase').as_posix()
png=(OUT/'PNG/AbilityFrameSilver.png').as_posix()
script='var src='+json.dumps(src)+'; var pixels='+json.dumps(plan)+';\n'+r'''
try {
 app.open(src);var s=app.activeSprite;
 if(s.filename.replace(/\\/g,'/')!==src||s.width!==176||s.height!==66)throw new Error('Wrong target');
 var im=s.layer(0).cel(0).image,pc=app.pixelColor;
 for(var i=0;i<pixels.length;i++){var c=pixels[i];im.putPixel(c[0],c[1],pc.rgba(c[2],c[3],c[4],c[5]));}
 s.layer(0).name='Silver bevel - user cleanup study';s.commit();
 s.saveAs(ASE,true);s.saveAs(PNG,true);app.activeDocument.close();
 console.log('ABILITY_CLEANUP_COMPLETE');
}catch(e){console.log('ABILITY_CLEANUP_ERROR '+e);}
app.command.Exit();
'''.replace('ASE',json.dumps(ase)).replace('PNG',json.dumps(png))
js=OUT/'QA/NativeCleanup.js';js.write_text(script,encoding='utf-8')
startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
with (OUT/'QA/LibreSprite-cleanup.log').open('wb') as log:
    process=subprocess.Popen([EXE,'--script',str(js)],cwd=ROOT,startupinfo=startup,stdout=log,stderr=subprocess.STDOUT)
    try: code=process.wait(timeout=30)
    except subprocess.TimeoutExpired:
        process.terminate();process.wait(timeout=10);raise
assert code==0 and b'ABILITY_CLEANUP_COMPLETE' in (OUT/'QA/LibreSprite-cleanup.log').read_bytes()
frame=Image.open(png).convert('RGBA')
assert all(frame.getpixel(p)==v for p,v in pixels.items()),'Native pixel plan mismatch'

# Store component layers without flattening. LibreSprite performs the final
# native .ase save and all PNG exports, including a reopen/export comparison.
def chunk(kind,data):return struct.pack('<IH',len(data)+6,kind)+data
def layered(path,layers):
    chunks=[]
    for i,(label,im,x,y) in enumerate(layers):
        name=label.encode('utf-8')
        chunks.append(chunk(0x2004,struct.pack('<6HB3xH',3,0,0,0,0,0,255,len(name))+name))
        chunks.append(chunk(0x2005,struct.pack('<HhhBHh5xHH',i,x,y,255,2,0,im.width,im.height)+zlib.compress(im.tobytes())))
    payload=b''.join(chunks);data=struct.pack('<IHHH2xI',len(payload)+16,0xF1FA,len(chunks),100,0)+payload
    head=bytearray(128);struct.pack_into('<IHHHHHI',head,0,128+len(data),0xA5E0,1,176,66,32,1)
    struct.pack_into('<H',head,20,100);head[34]=head[35]=1;path.write_bytes(head+data)
layered_source=OUT/'QA/AbilityLayerAssembly.ase'
# Center the existing 36px-tall opaque emblem in the cleaned 40px recess.
# This is a two-pixel placement correction; no emblem pixel is redrawn.
icon_origin=(73,16)
layered(layered_source,[('Clean silver frame and board-color inset',frame,0,0),('Original crowned skull',icon,*icon_origin)])
destase=OUT/'Editable/AbilityButtonSilver.ase';destpng=OUT/'PNG/AbilityButtonSilver.png';roundtrip=OUT/'QA/AbilityButtonSilver-Roundtrip.png'
commands=[[EXE,'--batch',str(layered_source),'--save-as',str(destase),'--save-as',str(destpng)],
          [EXE,'--batch',str(destase),'--save-as',str(roundtrip)]]
with (OUT/'QA/LibreSprite-export.log').open('wb') as log:
    for command in commands:
        result=subprocess.run(command,cwd=ROOT,startupinfo=startup,stdout=log,stderr=subprocess.STDOUT,timeout=20)
        assert result.returncode==0
actual=Image.open(destpng).convert('RGBA');reopened=Image.open(roundtrip).convert('RGBA')
expected=frame.copy();expected.alpha_composite(icon,icon_origin)
assert actual.tobytes()==expected.tobytes()==reopened.tobytes()
assert actual.size==(176,66) and set(actual.getchannel('A').get_flattened_data())=={0,255}
assert sha(OUT/'Inputs/RattlebonesEmblem.png')==sha(ART/'Stage03/PNG/RattlebonesEmblem.png')
assert (59,13,101,255) not in actual.get_flattened_data()
# Opaque connectivity: no detached random pixels in the cleaned frame.
remaining={p for p,c in pixels.items() if c[3]};components=[]
while remaining:
    seed=remaining.pop();component={seed};pending=[seed]
    while pending:
        x,y=pending.pop()
        for dx in (-1,0,1):
            for dy in (-1,0,1):
                q=(x+dx,y+dy)
                if q in remaining:remaining.remove(q);component.add(q);pending.append(q)
    components.append(len(component))
assert len(components)==1,components
report={'status':'review candidate; no production import','canvas':[176,66],
        'opaque_bounds':actual.getbbox(),'binary_alpha':True,
        'palette_count':len({c[:3] for c in actual.get_flattened_data() if c[3]}),
        'frame_connected_components':components,'no_resampling':True,
        'emblem_pixels_unchanged':True,'emblem_origin_before':[73,18],'emblem_origin_after':icon_origin,
        'board_inset':'#1C2438','exact_libresprite_roundtrip':True,'generation_spend':0,
        'references':{p.name:sha(p) for p in (OUT/'Inputs').glob('*.png')},
        'outputs':{p.name:sha(p) for p in [destase,destpng,Path(ase),Path(png)]}}
(OUT/'QA/Validation.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')

# Inspection-only enlargement; exported production sources above stay native.
actual.resize((704,264),Image.Resampling.NEAREST).save(OUT/'Preview/AbilityButtonSilver-4x.png')
boardim=Image.new('RGB',(1120,540),(32,33,43));draw=ImageDraw.Draw(boardim)
draw.text((24,16),'BEFORE',fill='white');draw.text((584,16),'CLEANED TO MATCH YOUR UI',fill='white')
for im,x in ((before,24),(actual,584)):
    boardim.paste(im,(x,42),im)
    zoom=im.resize((528,198),Image.Resampling.NEAREST);boardim.paste(zoom,(x,126),zoom)
draw.text((24,347),'YOUR EDITED REFERENCES - unchanged',fill='white')
for name,x in [('SettingsButtonSilver',24),('PowerUpTileSilver',180),('WavePlaqueSilver',380)]:
    im=Image.open(OUT/'Inputs'/f'{name}.png').convert('RGBA');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST)
    boardim.paste(im,(x,380),im)
boardim.save(OUT/'Preview/BeforeAfter.png')
print(json.dumps(report))
