from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
from collections import Counter
import hashlib,json,colorsys
F=Path(__file__).resolve().parents[4]
S=F/'ArtSource/SnowyMountainStaged/Stage05'
items=[]
for p in sorted((S/'Exports').glob('*.png')):
 im=Image.open(p).convert('RGBA'); rt=Image.open(S/'Roundtrip'/p.name).convert('RGBA')
 assert im.size==rt.size and im.tobytes()==rt.tobytes(),p
 colors=set(im.get_flattened_data());assert {c[3] for c in colors}<={0,255}
 items.append(dict(name=p.stem,dimensions=im.size,colors=len({c[:3] for c in colors if c[3]}),binary_alpha=True,roundtrip_exact=True,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
old=Image.open(S/'Raw/OriginalCobbleWall.png').convert('RGBA');new=Image.open(S/'Exports/CobbleWall_Muted.png').convert('RGBA')
assert old.size==new.size and old.getchannel('A').tobytes()==new.getchannel('A').tobytes()
mapping={}
for a,b in zip(old.get_flattened_data(),new.get_flattened_data()):
 mapping.setdefault(a,b);assert mapping[a]==b,'Every source color must map to a single new color; geometry must not change'
def stats(im):
 cs=list(im.convert('RGB').get_flattened_data());return dict(mean_value=sum(max(c) for c in cs)/len(cs),mean_saturation=sum(colorsys.rgb_to_hsv(*(v/255 for v in c))[1] for c in cs)/len(cs))
wall=dict(before=stats(old),after=stats(new),geometry_exact=True)
assert wall['after']['mean_value']<wall['before']['mean_value']*.8
assert wall['after']['mean_saturation']<wall['before']['mean_saturation']
hashes=json.loads((S/'QA/StartingHashes.json').read_text());changed=[p for p,h in hashes.items() if hashlib.sha256((F/p).read_bytes()).hexdigest()!=h]
assert not changed,changed
report=dict(assets=items,wall=wall,protected_files_unchanged=len(hashes))
(S/'QA/NativeAssets.json').write_text(json.dumps(report,indent=2))
sheet=Image.new('RGB',(960,760),'#20232c');d=ImageDraw.Draw(sheet)
for i,item in enumerate(items):
 x=(i%4)*240+10;y=(i//4)*140+12;im=Image.open(S/'Exports'/(item['name']+'.png')).convert('RGBA')
 d.text((x,y),item['name'],fill='white');sheet.paste(im,(x,y+25),im)
 if im.width<=32:
  zoom=im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST);sheet.paste(zoom,(x+65,y+25),zoom)
 d.text((x,y+117),str(item['dimensions'])+' / '+str(item['colors'])+' colors',fill='#aab9cb')
for i,im in enumerate([old,new]):
 x=10+i*470;d.text((x,585),['Previous wall','Muted wall'][i],fill='white');z=im.resize((462,156),Image.Resampling.NEAREST);sheet.paste(z,(x,605),z)
sheet.save(S/'QA/NativeAssets.png')
print(json.dumps({'modules':len(items),'wall':wall,'protected':len(hashes)}))
