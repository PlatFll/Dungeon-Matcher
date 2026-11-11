"""User-requested exact inset recolor in LibreSprite; preserve all other pixels."""
from pathlib import Path
from PIL import Image
from collections import Counter
import json,hashlib,shutil,subprocess
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
tile=ROOT/'Assets/_Game/Art/DungeonPixelLab/Stage16/Board_Cell64.png'
tile_im=Image.open(tile).convert('RGBA');target=tile_im.getpixel((32,32));assert target==(28,36,56,255)
old=(17,16,27,255)
png=HERE/'PNG/AbilityFrame.png';ase=HERE/'Editable/AbilityFrame.ase'
before=HERE/'Inputs/AbilityFrame-BeforeBoardColor.png';beforease=HERE/'Inputs/AbilityFrame-BeforeBoardColor.ase'
if not before.exists():shutil.copy2(png,before);shutil.copy2(ase,beforease)
im=Image.open(before).convert('RGBA');coords=[[x,y] for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))==old]
others={p:sha(p) for folder in ('PNG','Editable','Roundtrip') for p in (HERE/folder).glob('*') if p.stem!='AbilityFrame'}
script='var path='+json.dumps(ase.as_posix())+';var points='+json.dumps(coords)+';\n'+r'''
app.open(path);var s=app.activeSprite;
if(s.filename.replace(/\\/g,'/')!==path||s.width!==176||s.height!==66)throw new Error('Wrong edit target');
var im=s.layer(0).cel(0).image;
for(var i=0;i<points.length;i++){im.putPixel(points[i][0],points[i][1],app.pixelColor.rgba(28,36,56,255));}
s.commit();s.saveAs(path,true);s.saveAs(path.replace('/Editable/','/PNG/').replace('.ase','.png'),true);app.activeDocument.close();
app.open(path);app.activeSprite.saveAs(path.replace('/Editable/','/Roundtrip/').replace('.ase','.png'),true);app.activeDocument.close();
console.log('ABILITY_BOARD_COLOR_COMPLETE');app.command.Exit();
'''
sp=HERE/'QA/RecolorAbility.js';sp.write_text(script)
startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
run=subprocess.run(['rtk','C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe','--script',str(sp)],startupinfo=startup,cwd=ROOT,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=60)
assert run.returncode==0 and b'ABILITY_BOARD_COLOR_COMPLETE' in run.stdout,run.stdout.decode(errors='replace')
(HERE/'QA/RecolorAbilityExecution.txt').write_bytes(run.stdout)
after=Image.open(png).convert('RGBA');rt=Image.open(HERE/'Roundtrip/AbilityFrame.png').convert('RGBA')
changed=0
for y in range(im.height):
 for x in range(im.width):
  a,b=im.getpixel((x,y)),after.getpixel((x,y));assert a[3]==b[3]
  assert b==(target if a==old else a),(x,y,a,b)
  assert b==rt.getpixel((x,y));changed+=a!=b
assert changed==len(coords)>0 and all(sha(p)==h for p,h in others.items())
after.resize((704,264),Image.Resampling.NEAREST).save(HERE/'Preview/AbilityFrame-4x.png')
manifest=json.loads((HERE/'Manifest.json').read_text());colors=Counter(c[:3] for c in after.get_flattened_data() if c[3]);r=manifest['assets']['AbilityFrame'];r['sha256']=sha(png);r['opaque_colors']=len(colors);r['palette']={'#%02X%02X%02X'%c:n for c,n in colors.most_common()};manifest['ability_background_revision']={'user_request':'Match board tile background color; preserve all other art','color':'#1C2438','tile_source':tile.relative_to(ROOT).as_posix(),'tile_sha256':sha(tile),'before_sha256':sha(before),'changed_pixels':changed,'other_asset_exports_unchanged':True,'new_generations':0}
(HERE/'Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
(HERE/'QA/AbilityBackgroundRevision.json').write_text(json.dumps({'status':'PASS',**manifest['ability_background_revision'],'unchanged_alpha':True,'only_requested_fill_replaced':True,'exact_reopen_roundtrip':True},indent=2)+'\n')
shutil.copy2(tile,HERE/'Inputs/Board_Cell64.png')
print(json.dumps(manifest['ability_background_revision']))
