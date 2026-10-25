from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib
R=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2');E=R/'Exports/Stage02';P=R/'Preview/Stage02';Q=R/'QA/Stage02'
assets=[]
for p in E.glob('*.png'):
 im=Image.open(p).convert('RGBA');px=list(im.getdata());a=im.getchannel('A');raw=R/f'RawPixelLab/Stage02/{p.stem}-raw.png'
 same=not raw.exists() or a.tobytes()==Image.open(raw).convert('RGBA').getchannel('A').tobytes();assert same,p.stem+' alpha changed'
 assets.append({'name':p.stem,'width':im.width,'height':im.height,'colors':len(set(v[:3] for v in px if v[3])),'alpha':sorted(set(v[3] for v in px)),'bbox':im.getbbox(),'raw_alpha_preserved':same,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
def row(n,y):return list(Image.open(E/(n+'.png')).convert('RGBA').crop((0,y,64,y+1)).getdata())
for n in ['PillarShaft','PillarShaftWorn']:
 assert row(n,0)==row(n,23)
 assert row(n,0)==row('PillarCapital',19)==row('PillarBase',0)
# Actual vertical test stack and horizontal ledge neighbors, native pixels only.
test=Image.new('RGBA',(192,188))
for col in range(3):
 y=0;seq=['PillarCapital','PillarShaft','PillarShaftWorn','PillarShaft' if col%2 else 'PillarShaftWorn','PillarBase']
 for n in seq:
  im=Image.open(E/(n+'.png')).convert('RGBA');test.alpha_composite(im,(col*64,y));y+=im.height
ledge=Image.open(E/'RuinedLedge.png').convert('RGBA');assert ledge.crop((0,0,1,16)).tobytes()==ledge.crop((63,0,64,16)).tobytes()
for x in [0,64,128]:test.alpha_composite(ledge,(x,128))
test.save(P/'JoinStudy-Native.png');test.resize((576,564),Image.Resampling.NEAREST).save(P/'JoinStudy-3x.png')
placements=[{'name':'MonarchStatue','x':192,'y':46},{'name':'RuinedLedge','x':192,'y':188},{'name':'RuinedLedge','x':256,'y':188},{'name':'CourtArch','x':168,'y':99}]
placements.append({'name':'PillarCapital','x':110,'y':44})
for i in range(5):placements.append({'name':'PillarShaftWorn' if i in [1,4] else 'PillarShaft','x':110,'y':64+24*i})
placements.extend([{'name':'PillarBase','x':110,'y':184},{'name':'BrokenPillar','x':264,'y':85},{'name':'RubbleJoint','x':299,'y':184}])
manifest={'stage':'02','status':'CANDIDATE_PENDING_USER_REVIEW','ppu':64,'native_canvas':[360,240],'assets':assets,'placements':placements,'joins':'All capital/shaft/worn/base connector rows exact RGBA. Ledge edge columns exact. Repeated native join study supplied.'}
(Q/'NativeChecks.json').write_text(json.dumps(manifest,indent=2)+'\n');(R/'Native/Stage02/Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
comp=Image.open(R/'Preview/Stage01/LayerStudy-Native.png').convert('RGBA')
for v in placements:comp.alpha_composite(Image.open(E/(v['name']+'.png')).convert('RGBA'),(v['x'],v['y']))
comp.save(P/'LayerStudy-Native.png');comp.resize((1080,720),Image.Resampling.NEAREST).save(P/'LayerStudy-3x.png')
print(json.dumps([{k:a[k] for k in ['name','width','height','colors','bbox']} for a in assets]));print('All module connectors and repeat checks passed.')
