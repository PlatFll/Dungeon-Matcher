from pathlib import Path
from PIL import Image,ImageDraw
import json
r=Path(__file__).resolve().parents[1]
def rgba(h):return tuple(bytes.fromhex(h))+(255,)
def color(c,name,x,y):
 R,G,B,A=c
 if not A:return c
 if max(R,G,B)<65:return rgba('0c1014')
 if name=='Moss_Sprig':
  if max(R,G,B)-min(R,G,B)<18:return rgba('233f2b')
  if G<70:return rgba('233f2b')
  if G<100:return rgba('3c5f33')
  if G<130:return rgba('718b45')
  return rgba('94ac57')
 if G>R*.8 and G>B*1.18 and R<190 and (name!='Mushrooms_Pale' or y>=18):
  return rgba('3c5f33'if G<110 else '718b45')
 if name=='Mushrooms_Red':
  cap=y<=11 or (x>=13 and y<=17)
  if cap:
   if R>230 and G>175:return rgba('e7d9b5')
   if R>210:return rgba('d8765c')
   if R>140:return rgba('ac4c43')
   return rgba('6f3031')
 if name=='Mushrooms_Pale' and y<18 and 60<G<100 and R>70:return rgba('9d805b')
 if R>220 and G>200:return rgba('e7d9b5')
 if R>180 and G>135:return rgba('c9af7f')
 if R>100 and G>70:return rgba('9d805b')
 if R>105:return rgba('765131')
 return rgba('3b2c20')
assets=[]
for name,raw in [('Mushrooms_Red','Small_Red_0.png'),('Mushrooms_Pale','Small_Pale_0.png'),('Moss_Sprig','Small_Moss_0.png')]:
 im=Image.open(r/'Raw'/raw).convert('RGBA');edits=[];out=im.copy()
 for y in range(im.height):
  for x in range(im.width):
   c=im.getpixel((x,y));n=color(c,name,x,y)
   if c!=n:edits.append([x,y,bytes(n).hex()]);out.putpixel((x,y),n)
 assets.append(dict(name=name,raw=raw,edits=edits))
 out.save(r/'Previews'/(name+'-planned.png'))
(r/'Scripts/AccentSelection.json').write_text(json.dumps(assets,indent=2))
(r/'Scripts/SelectionConfig.json').write_text(json.dumps(dict(assets=assets,judgment='Native-size red/pale mushrooms and sparse moss sprig; material palette cleanup, no resampling.'),indent=2))
sheet=Image.new('RGBA',(600,230),'#27372f');d=ImageDraw.Draw(sheet)
for i,q in enumerate(assets):
 im=Image.open(r/'Previews'/(q['name']+'-planned.png'));d.text((i*200+4,8),q['name'],fill='white');sheet.alpha_composite(im.resize((192,192),Image.Resampling.NEAREST),(i*200,30))
sheet.save(r/'Previews/SmallAccents-CleanedPlan.png')
print([(a['name'],len(a['edits']))for a in assets])
