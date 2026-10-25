from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib,shutil
R=Path(__file__).resolve().parents[2];F=R.parents[1]
E=R/'Exports/Stage03';P=R/'Preview/Stage03';Q=R/'QA/Stage03'
assets=[]
for path in sorted(E.glob('*.png')):
 im=Image.open(path).convert('RGBA');colors=sorted(set(c[:3] for c in im.getdata() if c[3]));a=sorted(set(im.getchannel('A').getdata()))
 assert set(a)<={0,255};assets.append(dict(name=path.stem,width=im.width,height=im.height,colors=len(colors),alpha=a,bbox=im.getbbox(),sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
assert len(assets)==13,len(assets)
placements=[]
for prefix,y in [('FloorTop',198),('Fascia',214)]:
 x=0
 for suffix in ['Left','A','B','C','B','A','Fill','Right']:
  name=prefix+suffix;a=next(a for a in assets if a['name']==name);placements.append(dict(name=name,x=x,y=y));x+=a['width']
 assert x==360
manifest=dict(stage='03',status='CANDIDATE_PENDING_USER_REVIEW',ppu=64,native_canvas=[360,240],assets=assets,placements=placements,floor_baseline=dict(global_y=865,native_y=205.5,walkway_native_y=[198,214]),construction='PixelLab material seed; hand-authored exact joins and native corner variants in LibreSprite. No scaling or resampling. One 360x42 strip assembled from separate modules, behind portrait and actors.')
for path in [R/'Native/Stage03/Manifest.json',Q/'Manifest.json']:path.write_text(json.dumps(manifest,indent=2)+'\n')
# Read-only review composites. These are not assets and are never imported as flattened scenes.
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12)
sheet=Image.new('RGBA',(480,350),(11,25,33,255));d=ImageDraw.Draw(sheet)
for i,a in enumerate(assets):
 x=12+(i%3)*160;y=12+(i//3)*60
 d.text((x,y),a['name'],font=font,fill='white');im=Image.open(E/(a['name']+'.png')).convert('RGBA');sheet.alpha_composite(im,(x,y+17))
# Seed has a taller source canvas; separate full raw/clean study below avoids overlap.
sheet.save(P/'Native-Contact.png');sheet.resize((960,700),Image.Resampling.NEAREST).save(P/'Native-Contact-2x.png')
raw=Image.open(R/'RawPixelLab/Stage03/PlatformSeed.png').convert('RGBA');seed=Image.open(E/'PlatformSeed.png').convert('RGBA');study=Image.new('RGBA',(280,90),(11,25,33,255));d=ImageDraw.Draw(study)
d.text((6,2),'Raw PixelLab',font=font,fill='white');d.text((146,2),'LibreSprite seed',font=font,fill='white');study.alpha_composite(raw,(6,22));study.alpha_composite(seed,(146,22));study.resize((840,270),Image.Resampling.NEAREST).save(P/'Source-Comparison-3x.png')
floor=Image.new('RGBA',(360,42))
for p in placements:floor.alpha_composite(Image.open(E/(p['name']+'.png')).convert('RGBA'),(p['x'],p['y']-198))
floor.save(P/'Module-Join-Native.png');floor.resize((1080,126),Image.Resampling.NEAREST).save(P/'Module-Join-3x.png')
native=Image.open(R/'Exports/Stage01/DeepWater.png').convert('RGBA')
for name,x,y in [('LightShafts',0,0),('FarRuins',104,0),('MidRuins',104,8),('Particles',0,0)]:native.alpha_composite(Image.open(R/f'Exports/Stage01/{name}.png').convert('RGBA'),(x,y))
native.alpha_composite(floor,(0,198))
old=json.loads((R/'Native/Stage02/Manifest.json').read_text())
for p in old['placements']:native.alpha_composite(Image.open(R/f'Exports/Stage02/{p["name"]}.png').convert('RGBA'),(p['x'],p['y']))
native.save(P/'LayerStudy-Native.png');native.resize((720,480),Image.Resampling.NEAREST).save(P/'LayerStudy-2x.png')
