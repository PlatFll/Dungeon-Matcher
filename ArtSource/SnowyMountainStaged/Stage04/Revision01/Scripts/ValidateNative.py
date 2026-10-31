from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
S=Path(__file__).resolve().parents[1]
parts={p.stem:Image.open(p).convert('RGBA') for p in sorted((S/'Exports').glob('*.png'))}
records=[]
for n,im in parts.items():
 assert im.tobytes()==Image.open(S/'Roundtrip'/(n+'.png')).convert('RGBA').tobytes()
 assert {p[3] for p in im.get_flattened_data()}<={0,255}
 records.append(dict(name=n,size=im.size,opaqueColors=len({p[:3] for p in im.get_flattened_data() if p[3]}),binaryAlpha=True,roundtripExact=True,sha256=hashlib.sha256((S/'Exports'/(n+'.png')).read_bytes()).hexdigest()))
raw=Image.open(S/'Raw/OriginalPixelLab.png').convert('RGBA');head=Image.new('RGBA',(320,84))
for i in range(5):head.paste(parts[f'Ledge_{i}'],(i*64,0))
diff=[(x,y) for y in range(84) for x in range(320) if head.getpixel((x,y))!=raw.getpixel((x,y))]
assert all(x==319 and y>=40 for x,y in diff),diff[:5]
wall=parts['CobbleWall'];assert wall.getchannel('A').getextrema()==(255,255)
total=320*84
report=dict(modules=records,originalLedgePixelsPreserved=total-len(diff),originalLedgePixelsTotal=total,exactOriginalPercent=100*(total-len(diff))/total,changedPixels=len(diff),changeReason='Fill the empty right export column below y=40 to avoid a transparent seam; all other original pixels and colors unchanged.',newPixelLabGenerations=0)
(S/'QA/Native.json').write_text(json.dumps(report,indent=2))
im=Image.new('RGBA',(320,240))
for y in range(84,240,52):
 for x in range(0,320,154):im.alpha_composite(wall,(x,y))
im.alpha_composite(head);im.save(S/'Preview/LedgeAndWall-native.png');im.resize((960,720),Image.Resampling.NEAREST).save(S/'Preview/LedgeAndWall-3x.png')
print('Native QA:',len(parts),'modules;',report['exactOriginalPercent'],'percent exact ledge pixels;',len(diff),'edge fills; no generation spend')
