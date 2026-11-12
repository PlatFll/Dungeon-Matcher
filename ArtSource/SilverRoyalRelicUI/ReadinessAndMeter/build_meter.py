from pathlib import Path
from PIL import Image
import hashlib,json,re,shutil,subprocess,uuid

repo=Path(__file__).resolve().parents[3]
root=repo/'ArtSource/SilverRoyalRelicUI/ReadinessAndMeter'
for name in ('Inputs','Editable','Exports','QA','Preview'):(root/name).mkdir(parents=True,exist_ok=True)
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
locked=list((repo/'Assets/_Game/Resources/UI/BattlePresentation/Silver').glob('*.png'))
if not (root/'QA/BeforeHashes.json').exists():
    (root/'QA/BeforeHashes.json').write_text(json.dumps({str(p.relative_to(repo)):sha(p) for p in locked},indent=2))
src=repo/'ArtSource/SilverRoyalRelicUI/Stage03/PNG/EnergyCasing.png'
shutil.copy2(src,root/'Inputs/EnergyCasing.png')
old=repo/'Assets/_Game/Resources/UI/BattlePresentation/ReferenceDungeon/SharedEnergy.png'
shutil.copy2(old,root/'Inputs/PreviousProductionEnergy.png')
im=Image.open(src).convert('RGBA'); assert im.size==(147,24)
# Keep the existing silver casing's native silhouette and bevels; make only the
# charge window transparent so the actual track/fill can render below the rim.
for y in range(8,16):
    for x in range(15,132):
        assert im.getpixel((x,y))==(23,22,34,255),(x,y,im.getpixel((x,y)))
        im.putpixel((x,y),(0,0,0,0))
export=root/'Exports/EnergyFrameSilver.png'; im.save(export)
exe=r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
ase=root/'Editable/EnergyFrameSilver.ase'
for args in ([str(export),'--save-as',str(ase)],[str(ase),'--save-as',str(root/'QA/LibreSpriteRoundtrip.png')]):
    subprocess.run([exe,'--batch',*args],check=True,timeout=30,creationflags=0x08000000)
assert Image.open(root/'QA/LibreSpriteRoundtrip.png').convert('RGBA').tobytes()==im.tobytes()
dest=repo/'Assets/_Game/Resources/UI/BattlePresentation/Silver/EnergyFrameSilver.png';shutil.copy2(export,dest)
meta=(repo/'Assets/_Game/Resources/UI/BattlePresentation/Silver/AbilityButtonSilver.png.meta').read_text()
if not dest.with_suffix('.png.meta').exists():
    meta=re.sub(r'guid: [0-9a-f]+','guid: '+uuid.uuid4().hex,meta,count=1)
    dest.with_suffix('.png.meta').write_text(meta)
(root/'QA/AssetIntegrity.json').write_text(json.dumps({'source':str(src.relative_to(repo)),'source_sha256':sha(src),'export_sha256':sha(dest),'canvas':[147,24],'fill_window_top_left':[15,8,117,8],'opaque_palette':sorted({'#%02X%02X%02X'%p[:3] for p in im.getdata() if p[3]}),'alpha':[0,255],'only_window_pixels_changed':True,'libresprite_roundtrip_equal':True,'generation_spend':0},indent=2))

r=repo
im=Image.new('RGBA',(8,8),(28,36,56,255))
for x in range(8):im.putpixel((x,0),(23,22,34,255))
png=root/'Exports/EnergyTrackSilver.png';im.save(png)
exe=r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe';ase=root/'Editable/EnergyTrackSilver.ase'
for args in ([str(png),'--save-as',str(ase)],[str(ase),'--save-as',str(root/'QA/EnergyTrackRoundtrip.png')]):subprocess.run([exe,'--batch',*args],check=True,timeout=30,creationflags=0x08000000)
assert Image.open(root/'QA/EnergyTrackRoundtrip.png').convert('RGBA').tobytes()==im.tobytes()
dest=r/'Assets/_Game/Resources/UI/BattlePresentation/Silver/EnergyTrackSilver.png';shutil.copy2(png,dest)
meta=(dest.parent/'AbilityButtonSilver.png.meta').read_text();meta=re.sub(r'guid: [0-9a-f]+','guid: '+uuid.uuid4().hex,meta,count=1);
if not dest.with_suffix('.png.meta').exists():dest.with_suffix('.png.meta').write_text(meta)

report=json.loads((root/'QA/AssetIntegrity.json').read_text())
report['track']={'canvas':[8,8],'colors':['#171622','#1C2438'],'binary_opaque':True,'native_libresprite_roundtrip_equal':True}
(root/'QA/AssetIntegrity.json').write_text(json.dumps(report,indent=2))
