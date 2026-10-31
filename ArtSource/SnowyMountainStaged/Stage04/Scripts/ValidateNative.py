from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json
S=Path(__file__).resolve().parents[1]
parts={p.stem:Image.open(p).convert('RGBA') for p in sorted((S/'Exports').glob('*.png'))}; records=[];colors=set()
for name,im in parts.items():
 rt=Image.open(S/'Roundtrip'/(name+'.png')).convert('RGBA');assert rt.tobytes()==im.tobytes(),name
 cs=set(im.get_flattened_data());assert {c[3] for c in cs}<={0,255},name
 opaque={c[:3] for c in cs if c[3]};colors|=opaque
 if name.startswith('Separator') or name=='Foundation':assert im.getchannel('A').getextrema()==(255,255),name
 if name.startswith('SnowFloor'):
  assert all(im.getpixel((x,20))[3]==255 for x in range(64)),name+' standing row'
  assert all(im.getpixel((0,y))==im.getpixel((63,y)) for y in range(32)),name+' repeating seam'
 if name.startswith('SnowOverhang'):
  b=im.getbbox();assert b[0]>0 and b[1]>0 and b[2]<im.width and b[3]<im.height,name+' complete canvas clearance'
 records.append(dict(name=name,size=im.size,opaqueColors=len(opaque),binaryAlpha=True,roundtripExact=True,occupiedBounds=im.getbbox(),sha256=hashlib.sha256((S/'Exports'/(name+'.png')).read_bytes()).hexdigest()))
atlas=Image.new('RGBA',(840,480),(13,23,35,255));d=ImageDraw.Draw(atlas)
for i,(name,im) in enumerate(parts.items()):
 x=(i%3)*280;y=(i//3)*160;d.text((x+8,y+8),name,fill='#cfe7ef');d.text((x+8,y+28),f'{im.width}x{im.height} native / 2x',fill='#90b3c8')
 atlas.alpha_composite(im,(x+8,y+48));atlas.alpha_composite(im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST),(x+8,y+94))
atlas.save(S/'Preview/Modules.png')
strip=Image.new('RGBA',(384,92),(0,0,0,0))
for i in range(6):
 suffix='ABC'[i%3];strip.alpha_composite(parts['SnowFloor_'+suffix],(i*64,0));strip.alpha_composite(parts['Separator_'+suffix],(i*64,32));strip.alpha_composite(parts['Foundation'],(i*64,56))
strip.alpha_composite(parts['SnowOverhang_A'],(2,27));strip.alpha_composite(parts['SnowOverhang_B'],(309,27))
strip.save(S/'Preview/Strip-native.png');strip.resize((1152,276),Image.Resampling.NEAREST).save(S/'Preview/Strip-3x.png')
report=dict(modules=records,combinedOpaqueColors=len(colors),standingRowNativeY=20,walkingSurfaceNativeHeight=32,separatorNativeHeight=24,allRoundtripsExact=True)
(S/'QA/Native.json').write_text(json.dumps(report,indent=2));print('Native QA passed:',len(parts),'modules;',len(colors),'opaque colors; binary alpha; seams and connected mound clearance')
