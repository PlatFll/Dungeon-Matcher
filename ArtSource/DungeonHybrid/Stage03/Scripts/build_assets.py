from pathlib import Path
import subprocess, shutil, json, hashlib
from PIL import Image
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/Stage03'
L='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
def run(args):
 r=subprocess.run(['rtk','proxy',L,'-b']+args,cwd=P,capture_output=True,text=True);assert r.returncode==0,r.stdout+r.stderr
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
(P/'.utmp/hybrid-stage03').mkdir(parents=True,exist_ok=True)
run(['--script',str(R/'Scripts/CleanupUIStage03.js')])
parts=[]
for f in ['Wave','Control','Resonance']:
 for s in ['TL','TR','BL','BR']:parts.append((f+'_Corner_'+s,9,9))
 for s in ['Top','Bottom']:parts.append((f+'_Rail_'+s,16,5))
 for s in ['Left','Right']:parts.append((f+'_Rail_'+s,5,16))
 parts.extend([(f+'_Clamp',7 if f=='Resonance' else 5,7 if f=='Resonance' else 5),(f+'_Inset',16,16)])
parts+=[('Wave_Skull',15,18)]+[(f+'_Wing_'+s,7,15) for f in ['Wave','Resonance'] for s in ['Left','Right']]
dest=P/'Assets/_Game/Art/DungeonHybrid/Stage03';dest.mkdir(parents=True,exist_ok=True)
records=[]
for n,w,h in parts:
 ase=R/'Native'/f'{n}.aseprite';png=R/'Exports'/f'{n}.png';src=P/'.utmp/hybrid-stage03'/f'{n}.png'
 run(['--crop',f'0,0,{w},{h}',str(src),'--save-as',str(ase)])
 run([str(ase),'--save-as',str(png)])
 im=Image.open(png).convert('RGBA');assert im.size==(w,h);assert set(im.getchannel('A').get_flattened_data())<={0,255}
 original=Image.open(src).convert('RGBA').crop((0,0,w,h));assert im.tobytes()==original.tobytes(),n
 shutil.copyfile(png,dest/png.name)
 records.append(dict(name=n,width=w,height=h,path=(dest/png.name).relative_to(P).as_posix(),sha256=sha(png),source_sha256=sha(ase),colors=len({v[:3] for v in im.get_flattened_data() if v[3]}),bounds=im.getbbox()))
(R/'Modules.json').write_text(json.dumps(dict(parts=records),indent=2)+'\n')
print('PASS:',len(records),'native modules; LibreSprite ASE reopen/export exact; binary alpha; native sizes.')
