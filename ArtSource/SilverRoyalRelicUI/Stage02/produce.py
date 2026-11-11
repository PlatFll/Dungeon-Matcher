"""Convert approved PixelLab A into native modular top controls using LibreSprite."""
from pathlib import Path
from PIL import Image
from collections import Counter
import hashlib,json,subprocess,shutil,uuid
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2];OLD=HERE.parent/'Stage01'
EXE='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
DEST=ROOT/'Assets/_Game/Art/UI/SilverRoyalRelic/Stage02'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def visible(im):return bytes(v for c in im.get_flattened_data() for v in (c if c[3] else (0,0,0,0)))
baseline=HERE/'QA/StartingProduction.json'
if not baseline.exists():
    baseline.write_text(json.dumps({p.relative_to(ROOT).as_posix():sha(p) for folder in ('Assets','ProjectSettings','Packages') for p in (ROOT/folder).rglob('*') if p.is_file()},indent=2)+'\n')
for name in ('A_Wave','A_Settings'):
    source=OLD/'Cleaned'/f'{name}.png';target=HERE/'Inputs'/f'{name}.png'
    if target.exists():assert sha(source)==sha(target)
    else:shutil.copy2(source,target)
wave=Image.open(HERE/'Inputs/A_Wave.png').convert('RGBA');settings=Image.open(HERE/'Inputs/A_Settings.png').convert('RGBA')
assert sha(HERE/'Inputs/A_Wave.png')==json.loads((OLD/'Manifest.json').read_text())['assets']['A_Wave']['sha256']
blank=(0,0,0,0);recess=(17,16,27,255)
base=wave.copy();ornaments={}
# Only the approved native ornament pixels differing from the quiet rail are
# lifted. The same source rail column reconstructs the hidden backing exactly.
for name,region in [('WaveDiamond',(68,0,89,19)),('WavePendant',(64,40,91,62))]:
    overlay=Image.new('RGBA',wave.size)
    for y in range(region[1],region[3]):
        for x in range(region[0],region[2]):
            c=wave.getpixel((x,y));back=wave.getpixel((55,y))
            if c[3] and c!=back:overlay.putpixel((x,y),c)
            base.putpixel((x,y),back if back[3] else blank)
    b=overlay.getbbox();crop=overlay.crop(b)
    w,h=crop.size;canvas=Image.new('RGBA',(w+4+(w%2),h+4+(h%2)))
    canvas.paste(crop,((canvas.width-w)//2,2));ornaments[name]=canvas
# Remove only incidental shade flips along the straight rails, keeping all
# chamfer/side ornament pixels. A repeated quiet column needs no resampling.
for y in range(10,54):
    for x in range(24,133):base.putpixel((x,y),base.getpixel((55,y)))
body=Image.new('RGBA',(176,48))
for y in range(44):
    for x in range(176):
        sx=x if x<60 else 55 if x<79 else x-19
        body.putpixel((x,y+2),base.getpixel((sx,y+10)))
frame=settings.copy();gear=Image.new('RGBA',(32,32))
# The detached gear is a distinct component within the dark recess. Preserve its
# drawing exactly, then correct its one-pixel optical registration in the square.
for y in range(16,41):
    for x in range(14,40):
        c=settings.getpixel((x,y))
        if c[:3]!=recess[:3] and c[3]:gear.putpixel((x-14+3,y-16+3),c)
        frame.putpixel((x,y),recess)
combined=frame.copy();combined.alpha_composite(gear,(12,12))
assets={'WaveBody':body,**ornaments,'SettingsFrame':frame,'SettingsGear':gear,'Settings':combined}
plans=[]
for name,im in assets.items():
    blankfile=HERE/'QA'/f'Blank-{name}.png';Image.new('RGBA',im.size).save(blankfile)
    plans.append({'name':name,'blank':blankfile.as_posix(),'size':list(im.size),
                  'pixels':[[x,y,*im.getpixel((x,y))] for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))[3]]})
script='var plans='+json.dumps(plans)+';\nvar dest='+json.dumps(HERE.as_posix()+'/')+';\n'+r'''
var pc=app.pixelColor;
for(var i=0;i<plans.length;i++){
 var p=plans[i];app.open(p.blank);var s=app.activeSprite;
 if(s.filename.replace(/\\/g,'/')!==p.blank||s.width!==p.size[0]||s.height!==p.size[1])throw new Error('Wrong native canvas');
 var im=s.layer(0).cel(0).image;
 for(var k=0;k<p.pixels.length;k++){var c=p.pixels[k];im.putPixel(c[0],c[1],pc.rgba(c[2],c[3],c[4],c[5]));}
 s.layer(0).name='Approved A - native modular pixels';s.commit();
 s.saveAs(dest+'Editable/'+p.name+'.ase',true);s.saveAs(dest+'PNG/'+p.name+'.png',true);app.activeDocument.close();
 app.open(dest+'Editable/'+p.name+'.ase');app.activeSprite.saveAs(dest+'Roundtrip/'+p.name+'.png',true);app.activeDocument.close();
}
console.log('SILVER_TOP_NATIVE_COMPLETE');app.command.Exit();
'''
(HERE/'QA/NativeProduction.js').write_text(script)
startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
result=subprocess.run(['rtk',EXE,'--script',str(HERE/'QA/NativeProduction.js')],cwd=ROOT,startupinfo=startup,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=90)
(HERE/'QA/LibreSprite.log').write_bytes(result.stdout)
assert result.returncode==0 and b'SILVER_TOP_NATIVE_COMPLETE' in result.stdout,result.stdout.decode(errors='replace')
report={}
meta=(ROOT/'Assets/_Game/Resources/UI/BattlePresentation/Stone/WaveBody.png.meta').read_text()
import re
for name,expected in assets.items():
    p=HERE/'PNG'/f'{name}.png';im=Image.open(p).convert('RGBA');rt=Image.open(HERE/'Roundtrip'/f'{name}.png').convert('RGBA')
    assert im.size==expected.size==rt.size and visible(im)==visible(expected)==visible(rt),name
    assert set(im.getchannel('A').get_flattened_data())=={0,255},name
    colors=Counter(c[:3] for c in im.get_flattened_data() if c[3]);assert len(colors)<=12,name
    b=im.getbbox();assert min(b[0],b[1],im.width-b[2],im.height-b[3])>=2,name
    report[name]={'dimensions':list(im.size),'bounds':b,'opaque_colors':len(colors),'sha256':sha(p),'binary_alpha':True,'exact_libresprite_roundtrip':True}
    shutil.copy2(p,DEST/f'{name}.png')
    mp=DEST/f'{name}.png.meta'
    if not mp.exists():
        text=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid4().hex,meta,count=1)
        text=re.sub(r'spriteBorder: .*','spriteBorder: '+('{x: 20, y: 10, z: 20, w: 10}' if name=='WaveBody' else '{x: 0, y: 0, z: 0, w: 0}'),text)
        text=re.sub(r'spriteID: [a-f0-9]+','spriteID: '+uuid.uuid4().hex,text)
        mp.write_text(text)
    im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST).save(HERE/'Preview'/f'{name}-4x.png')
for folder in [DEST.parent,DEST]:
    mp=Path(str(folder)+'.meta')
    if not mp.exists():mp.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
manifest={'stage':2,'selected':'A - Royal bevel','selection_date':'2026-11-11','status':'native top controls pending Unity review',
 'source':'../Stage01/Cleaned/A_Wave.png and A_Settings.png','new_pixellab_generations':0,'prior_pixellab_generations':29,
 'assets':report,'preview_only':True,'wave_body_border_lbrt':[20,10,20,10],
 'diamond_top_offset':-8,'pendant_top_offset':32,'production_runtime_changed':False}
(HERE/'Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(json.dumps({n:r['dimensions'] for n,r in report.items()}))
