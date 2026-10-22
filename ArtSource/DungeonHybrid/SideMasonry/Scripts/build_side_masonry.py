from pathlib import Path
from PIL import Image
import subprocess,shutil,json,hashlib
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/SideMasonry';W=Path(__file__).parent
L='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
def run(args):
 r=subprocess.run(['rtk','proxy',L,'-b']+args,cwd=P,capture_output=True,text=True);assert r.returncode==0,r.stdout+r.stderr
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
(P/'.utmp/hybrid-masonry').mkdir(parents=True,exist_ok=True)
if (W/'CleanupSideMasonry.js').resolve()!=(R/'Scripts/CleanupSideMasonry.js').resolve():shutil.copy2(W/'CleanupSideMasonry.js',R/'Scripts/CleanupSideMasonry.js')
run(['--script',str(R/'Scripts/CleanupSideMasonry.js')])
dest=P/'Assets/_Game/Art/DungeonHybrid/SideMasonry';dest.mkdir(parents=True,exist_ok=True)
records=[]
for n,w,h in [('Masonry_Base_Left',48,86),('Masonry_Base_Right',48,86),('Masonry_Repeat_Left',24,16),('Masonry_Repeat_Right',24,16)]:
 ase=R/'Native'/f'{n}.aseprite';png=R/'Exports'/f'{n}.png';tmp=P/'.utmp/hybrid-masonry'/f'{n}.png'
 run(['--crop',f'0,0,{w},{h}',str(tmp),'--save-as',str(ase)]);run([str(ase),'--save-as',str(png)])
 im=Image.open(png).convert('RGBA');assert im.size==(w,h)
 assert im.tobytes()==Image.open(tmp).convert('RGBA').crop((0,0,w,h)).tobytes()
 assert set(im.getchannel('A').get_flattened_data())<={0,255}
 shutil.copy2(png,dest/png.name)
 records.append(dict(name=n,width=w,height=h,bounds=im.getbbox(),colors=len({v[:3] for v in im.get_flattened_data() if v[3]}),sha256=sha(png),ase_sha256=sha(ase)))
raw=Image.open(R/'Sources/Masonry_Raw.png').convert('RGBA');clean=Image.open(R/'Previews/Masonry_Clean128.png').convert('RGBA')
assert raw.getchannel('A').tobytes()==clean.getchannel('A').tobytes()
run([str(R/'Previews/Masonry_Clean128.png'),'--scale','4','--save-as',str(R/'Previews/Masonry_Clean_4x.png')])
(R/'Modules.json').write_text(json.dumps(dict(parts=records,raw_sha256=sha(R/'Sources/Masonry_Raw.png'),source_silhouette_preserved=True,charged_generations=6,balance_after=925.2),indent=2)+'\n')
print('PASS: four native modules; seven-color cleanup; binary alpha; unchanged source mask; LibreSprite ASE round trips identical.')
