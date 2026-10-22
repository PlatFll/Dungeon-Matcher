from pathlib import Path
from PIL import Image
import hashlib,json,subprocess,shutil
P=Path(__file__).resolve().parents[5]
W=Path(__file__).parent;R=P/'ArtSource/DungeonHybrid/SideMasonry/MatchedCorners'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for folder in ['Native','Exports','Scripts','Diagnostics','Previews','Sources']: (R/folder).mkdir(parents=True,exist_ok=True)
audit=json.loads((P/'ArtSource/DungeonHybrid/Integration/Diagnostics/ReferenceAudit.json').read_text())
for ref in audit['references']:
 assert sha(P/ref['path'])==ref['sha256'],ref['path']
assert sha(P/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt')==audit['guide_sha256']
assert sha(P/'Docs/ArtDirection/Golden_Reference_Register.md')==audit['register_sha256']
if (W/'MatchedCorners.js').resolve()!=(R/'Scripts/MatchedCorners.js').resolve(): shutil.copy2(W/'MatchedCorners.js',R/'Scripts/MatchedCorners.js')
# The immutable user correction reference is already archived in Sources/.
L='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
def run(args):
 r=subprocess.run(['rtk','proxy',L,'-b']+args,cwd=P,capture_output=True,text=True)
 assert r.returncode==0,r.stdout+r.stderr
run(['--script',str(R/'Scripts/MatchedCorners.js')])
dest=P/'Assets/_Game/Art/DungeonHybrid/SideMasonry/MatchedCorners';dest.mkdir(parents=True,exist_ok=True)
records=[]
for side in ['Left','Right']:
 name='Masonry_Corner_'+side
 source=P/'Assets/_Game/Art/DungeonPixelLab/Support'/('Support_Corner_'+side+'.png')
 before=Image.open(source).convert('RGBA')
 ase=R/'Native'/(name+'.aseprite');png=R/'Exports'/(name+'.png')
 run(['--crop','0,0,58,88',str(P/'.utmp/hybrid-masonry'/('Matched_'+side+'.png')),'--save-as',str(ase)])
 run([str(ase),'--save-as',str(png)])
 im=Image.open(png).convert('RGBA');assert im.size==(58,88)
 assert set(im.getchannel('A').get_flattened_data())=={0,255}
 palette={v[:3] for v in im.get_flattened_data() if v[3]}
 assert palette=={v[:3] for v in before.get_flattened_data() if v[3]}
 # First two courses are copied in shape and shading from the current sides.
 top=im.crop((0 if side=='Left' else 30,2,28 if side=='Left' else 58,30))
 assert top.tobytes()==before.tobytes()
 assert im.tobytes()==Image.open(P/'.utmp/hybrid-masonry'/('Matched_'+side+'.png')).convert('RGBA').crop((0,0,58,88)).tobytes()
 shutil.copy2(png,dest/png.name)
 run([str(png),'--scale','4','--save-as',str(R/'Previews'/(name+'_4x.png'))])
 records.append(dict(name=name,width=58,height=88,colors=len(palette),bounds=im.getbbox(),source=str(source.relative_to(P)),source_sha256=sha(source),sha256=sha(png),ase_sha256=sha(ase)))
(R/'Modules.json').write_text(json.dumps(dict(parts=records,source_palette_exact=True,top_courses_exact=True,additional_generations=0,previous_candidate='Rejected by user: retain the existing side stones, add only matching masonry.'),indent=2)+'\n')
print('PASS: two native LibreSprite corner assets; same five colors and first two courses as the approved side stones.')
