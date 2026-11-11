from pathlib import Path
from PIL import Image
import hashlib,json,re,shutil,subprocess,uuid
R=Path(__file__).resolve().parents[3]
A=R/'ArtSource/SilverRoyalRelicUI/Production'; D=R/'Assets/_Game/Resources/UI/BattlePresentation/Silver'
EXE=r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for d in [A/'Inputs',A/'Editable',A/'OtherCharacterIcons',A/'QA',A/'Preview',D]: d.mkdir(parents=True,exist_ok=True)
if not (A/'QA/BeforeHashes.json').exists():
 (A/'QA/BeforeHashes.json').write_text(json.dumps({str(p.relative_to(R)):sha(p) for folder in ['Assets','ProjectSettings','Packages'] for p in (R/folder).rglob('*') if p.is_file()},indent=2))
template=(R/'Assets/_Game/Resources/UI/BattlePresentation/Stone/AbilityPlate.png.meta').read_text()
def meta(p):
 m=Path(str(p)+'.meta')
 if not m.exists():m.write_text(re.sub(r'^guid: .*$', 'guid: '+uuid.uuid4().hex,template,flags=re.M).replace('spriteMeshType: 0','spriteMeshType: 1').replace('wrapU: 0','wrapU: 1').replace('wrapV: 0','wrapV: 1'),encoding='utf-8')
 return re.search(r'^guid: (.*)$',m.read_text(),re.M)[1]
names=['WavePlaqueSilver','SettingsButtonSilver','PowerUpTileSilver','AbilityButtonSilver','RattleBonesAbilityIcon']
manifest={}
for n in names:
 src=A/'Inputs'/(n+'.png'); dst=D/src.name
 if (A/'Inputs'/src.name).exists():assert sha(A/'Inputs'/src.name)==sha(src),'Do not overwrite a different user input'
 shutil.copy2(src,dst);meta(dst)
 im=Image.open(src).convert('RGBA');manifest[n]={'sha256':sha(src),'dimensions':im.size,'bounds':im.getbbox(),'alpha':sorted(set(im.getchannel('A').getdata()))}
 subprocess.run([EXE,'--batch',str(dst),'--save-as',str(A/'Editable'/(n+'.ase'))],check=True,timeout=30,creationflags=0x08000000)
# Exact column slices preserve ornaments and native rail pixels at responsive widths.
wave=Image.open(D/'WavePlaqueSilver.png').convert('RGBA')
for i,(l,r) in enumerate([(0,25),(25,66),(66,110),(110,151),(151,176)]):
 p=D/f'WavePart{i}.png';wave.crop((l,0,r,66)).save(p);meta(p)
assert all(wave.getpixel((x,y))==wave.getpixel((25,y)) for x in range(25,66) for y in range(66)), 'Left rail must repeat cleanly'
assert all(wave.getpixel((x,y))==wave.getpixel((110,y)) for x in range(110,151) for y in range(66)), 'Right rail must repeat cleanly'
glow=Image.open(D/'RattleBonesAbilityIcon.png').convert('RGBA')
glow.putdata([(255,255,255,255 if a and max(r,g,b)>90 else 0) for r,g,b,a in glow.getdata()]);glow.save(D/'RattleBonesAbilityReady.png');meta(D/'RattleBonesAbilityReady.png')
for n,label in [('Bull','Minotaur_Bull'),('Revolver','Kobold_Revolver'),('Flute','Bardley_Flute')]:
 src=R/'Assets/_Game/Resources/UI/BattlePresentation/Stone'/(n+'.png');dst=A/'OtherCharacterIcons'/(label+'.png');shutil.copy2(src,dst)
 subprocess.run([EXE,'--batch',str(dst),'--save-as',str(dst.with_suffix('.ase'))],check=True,timeout=30,creationflags=0x08000000)
 check=A/'QA'/(label+'-reopened.png');subprocess.run([EXE,'--batch',str(dst.with_suffix('.ase')),'--save-as',str(check)],check=True,timeout=30,creationflags=0x08000000)
 assert Image.open(check).convert('RGBA').tobytes()==Image.open(src).convert('RGBA').tobytes()
 manifest[label]={'source':str(src.relative_to(R)),'sha256':sha(src),'editable':str(dst.with_suffix('.ase').relative_to(R))}
# Existing definition owns the icon selection; no player-specific UI logic.
p=R/'Assets/_Game/Data/Player Abilities/Ability_RoyalDecree.asset';s=p.read_text()
for field,n in [('stoneEngraving','RattleBonesAbilityIcon'),('readyStoneEngraving','RattleBonesAbilityReady')]:
 s=re.sub(r'  '+field+r': .*', '  '+field+': {fileID: 21300000, guid: '+meta(D/(n+'.png'))+', type: 3}',s)
p.write_text(s,encoding='utf-8')
(A/'Manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('Exact five user PNGs imported. Three existing icons reopened in LibreSprite and verified. No generations.')
