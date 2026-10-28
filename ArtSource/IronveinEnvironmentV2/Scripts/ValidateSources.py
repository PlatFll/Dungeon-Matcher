from pathlib import Path
from PIL import Image,ImageDraw
from collections import Counter
import json,hashlib
F=Path(__file__).resolve().parents[3];R=F/'ArtSource/IronveinEnvironmentV2'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
data=[];js=[];occupied=[]
for p in sorted((R/'Exports').glob('*.png')):
 im=Image.open(p).convert('RGBA');px=list(im.get_flattened_data());alpha=set(v[3] for v in px);assert alpha<={0,255},p
 row=dict(name=p.stem,width=im.width,height=im.height,colors=len({v[:3] for v in px if v[3]}),alpha=sorted(alpha),bounds=im.getbbox(),sha256=sha(p),source=str(p.relative_to(F)).replace('\\','/'))
 if p.stem in ['CaveWall','CaveRecess']:assert [im.getpixel((0,y)) for y in range(im.height)]==[im.getpixel((im.width-1,y)) for y in range(im.height)]
 if p.stem=='Platform':assert [im.getpixel((0,y)) for y in range(im.height)]==[im.getpixel((im.width-1,y)) for y in range(im.height)]
 data.append(row)
 s=str(R).replace('\\','/')
 js.append('app.open('+json.dumps(s+'/Native/'+p.stem+'.ase')+');app.activeSprite.saveAs('+json.dumps(s+'/QA/Roundtrip/'+p.name)+',true);app.activeDocument.close();')
(R/'Scripts/Roundtrip.js').write_text('\n'.join(js)+'\nconsole.log("IRONVEIN_ROUNDTRIP_COMPLETE");app.command.Exit();')
(R/'QA/NativeAssets.json').write_text(json.dumps(data,indent=2))
baseline=json.loads((R/'QA/ProtectedBaseline.json').read_text());changed=[p for p,h in baseline.items() if not(F/p).exists() or sha(F/p)!=h]
(R/'QA/ProtectedCheck.json').write_text(json.dumps(dict(files=len(baseline),changed=changed),indent=2))
# Every asset is shown at native 1x; enlarged items keep integer nearest-neighbor sampling.
cellw,cellh=220,230;out=Image.new('RGB',(cellw*6,cellh*((len(data)+5)//6)),(24,26,36));d=ImageDraw.Draw(out)
for i,a in enumerate(data):
 im=Image.open(R/'Exports'/f'{a["name"]}.png').convert('RGBA');x=(i%6)*cellw+8;y=(i//6)*cellh+26
 d.text((x,y-20),a['name'],fill='white');out.paste(im,(x,y),im);d.text((x,y+180),f'{im.width}x{im.height}; {a["colors"]} colors',fill='#bfc5d3')
out.save(R/'Preview/NativeAssets.png')
print(json.dumps(dict(assets=len(data),colors=[min(x['colors'] for x in data),max(x['colors'] for x in data)],binaryAlpha=True,changedProtected=changed)))
