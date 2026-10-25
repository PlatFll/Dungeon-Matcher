from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';E=R/'Exports/Stage08';P=R/'Preview/Stage08'
def placements(h):
 return [dict(name='SpongePocket',x=329,y=348+(0 if h==640 else 63),width=28,height=33)]
assets=[];colors=set()
for p in sorted(E.glob('*.png')):
 im=Image.open(p).convert('RGBA');cs=set(c[:3] for c in im.get_flattened_data() if c[3]);colors|=cs
 assets.append(dict(name=p.stem,status='REJECTED_NOT_PLACED' if p.stem=='TridentShard' else 'CANDIDATE',width=im.width,height=im.height,bounds=im.getbbox(),opaque_colors=len(cs),binary_alpha=set(im.getchannel('A').get_flattened_data())<={0,255},sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
for h in (640,800):
 layer=Image.new('RGBA',(360,h))
 for p in placements(h):layer.alpha_composite(Image.open(E/(p['name']+'.png')).convert('RGBA'),(p['x'],p['y']))
 layer.save(P/f'DetailLayers-Native-{h}.png')
study=Image.new('RGB',(310,115),(16,29,34));d=ImageDraw.Draw(study)
for i,a in enumerate(assets):
 im=Image.open(E/(a['name']+'.png')).convert('RGBA');study.paste(im,(i*104+4,25),im);d.text((i*104+4,6),a['name'],fill='white')
study.save(P/'Native-Contact.png');study.resize((930,345),Image.Resampling.NEAREST).save(P/'Native-Contact-3x.png')
data=dict(stage='08',assets=assets,placements=placements(640),placementsTall=placements(800),combined_opaque_colors=len(colors),scope='One selected pocket; source study retained. Trident trial rejected from assembly after native review. Preserved source art and all prior layout.')
(R/'Native/Stage08/Manifest.json').write_text(json.dumps(data,indent=2));print('Two selected assets, one rejected trial; one placed detail; palette',len(colors))
