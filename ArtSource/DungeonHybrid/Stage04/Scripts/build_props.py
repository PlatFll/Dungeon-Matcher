from pathlib import Path
import subprocess,json,hashlib,shutil
from PIL import Image
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/Stage04'
L='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
def run(args):
 r=subprocess.run(['rtk','proxy',L,'-b']+args,cwd=P,capture_output=True,text=True)
 assert r.returncode==0,r.stdout+r.stderr
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
(P/'.utmp/hybrid-stage04').mkdir(parents=True,exist_ok=True)
run(['--script',str(R/'Scripts/CleanupPropsStage04.js')])
parts=[('Torch',24,54),('Chain_Anchor',14,25),('Chain_Repeat',14,14),('Chain_End',14,14),('Banner',37,52),('Barrel',40,45),('Jar',40,37),('Skull',31,30),('Bones',32,28),('Rubble',48,29)]
dest=P/'Assets/_Game/Art/DungeonHybrid/Stage04';dest.mkdir(parents=True,exist_ok=True)
records=[]
for n,w,h in parts:
 ase=R/'Native'/f'{n}.aseprite';png=R/'Exports'/f'{n}.png';src=P/'.utmp/hybrid-stage04'/f'{n}.png'
 run(['--crop',f'0,0,{w},{h}',str(src),'--save-as',str(ase)])
 run([str(ase),'--save-as',str(png)])
 im=Image.open(png).convert('RGBA');assert im.size==(w,h)
 assert im.tobytes()==Image.open(src).convert('RGBA').crop((0,0,w,h)).tobytes(),n
 assert set(im.getchannel('A').get_flattened_data())<={0,255}
 shutil.copyfile(png,dest/png.name)
 records.append(dict(name=n,width=w,height=h,bounds=im.getbbox(),colors=len({v[:3] for v in im.get_flattened_data() if v[3]}),
  path=(dest/png.name).relative_to(P).as_posix(),sha256=sha(png),source_sha256=sha(ase)))
(R/'Modules.json').write_text(json.dumps(dict(parts=records),indent=2)+'\n')
for n in ['Torch','Chain','Banner','Barrel','Jar','Bones','Rubble']:
 raw=Image.open(R/'Sources'/f'{n}_Raw.png').convert('RGBA');clean=Image.open(R/'Previews'/f'{n}_Clean64.png').convert('RGBA')
 assert raw.size==clean.size and raw.getchannel('A').tobytes()==clean.getchannel('A').tobytes(),n
print('PASS: 10 native exports; LibreSprite editable sources reopened exactly; seven source silhouettes preserved.')
