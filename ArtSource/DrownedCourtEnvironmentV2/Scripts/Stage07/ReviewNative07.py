from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';E=R/'Exports/Stage07';P=R/'Preview/Stage07'
def placements(height):
 out=[]
 def put(n,x,y):
  im=Image.open(E/(n+'.png'));out.append(dict(name=n,x=x,y=y,width=im.width,height=im.height))
 for x in range(0,352,32):put('Seafloor',x,height-6)
 put('SeafloorShort',352,height-6)
 y0=545 if height==640 else 608
 for x in (0,329):
  for y in range(y0,height-66,32):put('RuinConnector',x,y)
 put('LeftReef',0,height-98);put('RightReef',280,height-95)
 put('RubbleA',72,height-8);put('RubbleB',91,height-6)
 put('RubbleB',264,height-6);put('RubbleA',249,height-8)
 return out
assets=[];allcolors=set()
for p in sorted(E.glob('*.png')):
 im=Image.open(p).convert('RGBA');colors=set(c[:3] for c in im.get_flattened_data() if c[3]);allcolors|=colors
 assets.append(dict(name=p.stem,width=im.width,height=im.height,bounds=im.getbbox(),opaque_colors=len(colors),binary_alpha=set(im.getchannel('A').get_flattened_data())<={0,255},sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
for h in (640,800):
 canvas=Image.new('RGBA',(360,h))
 for v in placements(h):canvas.alpha_composite(Image.open(E/(v['name']+'.png')).convert('RGBA'),(v['x'],v['y']))
 canvas.save(P/f'ReefLayers-Native-{h}.png')
 before=Image.open(R/f'Preview/Stage06/Unity-Actors-360x{h}.png').convert('RGBA')
 before.alpha_composite(canvas)
 before.save(P/f'LayoutStudy-360x{h}.png')
board=Image.new('RGB',(520,245),(16,29,34));d=ImageDraw.Draw(board)
for i,a in enumerate(assets):
 x=10+(i%4)*130;y=26+(i//4)*130;im=Image.open(E/(a['name']+'.png')).convert('RGBA');board.paste(im,(x,y),im);d.text((x,y-15),a['name'],fill='#ded4af')
board.save(P/'Native-Contact.png');board.resize((1040,490),Image.Resampling.NEAREST).save(P/'Native-Contact-2x.png')
manifest=dict(stage='07',assets=assets,placements=placements(640),placementsTall=placements(800),combined_opaque_colors=len(allcolors),scope='Candidate lower reef corners only; two one-piece main formations; unchanged accepted upper scene and UI; native placement, no resizing.')
(R/'Native/Stage07/Manifest.json').write_text(json.dumps(manifest,indent=2));print(json.dumps({'assets':len(assets),'placements':len(manifest['placements']),'placements_tall':len(manifest['placementsTall']),'palette_count':len(allcolors)}))
