from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
S=Path(r"C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/SnowyMountainStaged/Stage03")
parts={p.stem:Image.open(p).convert("RGBA") for p in sorted((S/"Exports").glob("*.png"))}
records=[];colors=set()
for name,im in parts.items():
 rt=Image.open(S/"Roundtrip"/(name+".png")).convert("RGBA")
 assert rt.tobytes()==im.tobytes(),name
 cs=set(im.getdata());opaque={c[:3] for c in cs if c[3]}
 assert {c[3] for c in cs}<={0,255}
 colors|=opaque
 records.append(dict(name=name,size=im.size,colors=len(opaque),binaryAlpha=True,roundtripPixelsEqual=True,sha256=hashlib.sha256((S/"Exports"/(name+".png")).read_bytes()).hexdigest()))
# Structural rails contain no transparent holes.
for name in ["Portrait_Rail_Top","Portrait_Rail_Bottom","Portrait_Rail_Left","Portrait_Rail_Right","Backing"]:
 assert parts[name].getchannel("A").getextrema()==(255,255),name
atlas=Image.new("RGBA",(640,400),(13,23,35,255));d=ImageDraw.Draw(atlas)
for i,(name,im) in enumerate(parts.items()):
 x=(i%4)*160;y=(i//4)*100
 d.text((x+6,y+4),name.replace("Portrait_",""),fill="#e0eef7")
 atlas.alpha_composite(im,(x+8,y+25))
 # 2x enlarged alongside native.
 atlas.alpha_composite(im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST),(x+65,y+25))
atlas.save(S/"Preview/Modules.png")
def panel(w,h):
 out=Image.new("RGBA",(w+16,h+16),(0,0,0,0));ox=oy=8
 backing=Image.new("RGBA",(w-18,h-18),(0,0,0,0))
 for y in range(0,h-18,32):
  for x in range(0,w-18,64):backing.alpha_composite(parts["Backing"],(x,y))
 out.alpha_composite(backing,(ox+9,oy+9))
 def put(n,x,y):out.alpha_composite(parts[n],(ox+int(x),oy+int(y)))
 def tile(n,x,y,wid,hei):
  img=parts[n];fill=Image.new("RGBA",(int(wid),int(hei)),(0,0,0,0))
  for py in range(0,int(hei),img.height):
   for px in range(0,int(wid),img.width):fill.alpha_composite(img,(px,py))
  out.alpha_composite(fill,(ox+int(x),oy+int(y)))
 tile("Portrait_Rail_Top",8,4,w-16,8);tile("Portrait_Rail_Bottom",8,h-12,w-16,8)
 tile("Portrait_Rail_Left",4,8,8,h-16);tile("Portrait_Rail_Right",w-12,8,8,h-16)
 for n,x,y in [("TL",0,0),("TR",w-16,0),("BL",0,h-16),("BR",w-16,h-16)]:put("Portrait_Corner_"+n,x,y)
 for length,horizontal in [(w,True),(h,False)]:
  count=int((length-32)//50)
  if count%2==0 and count:count-=1
  for i in range(1,count+1):
   along=round(7.5+(length-16)*i/(count+1))-5.5
   if horizontal:
    put("Portrait_Clamp",along,2);put("Portrait_Clamp",along,h-14)
   else:
    put("Portrait_Clamp",2,along);put("Portrait_Clamp",w-14,along)
 put("SnowCap_Left",0,-4);put("SnowCap_Right",w-32,-4)
 tile("SnowCap_Middle",32,-4,w-64,16)
 put("SnowFoot_Left",9,h-18);put("SnowFoot_Right",w-33,h-18)
 return out
p=panel(98,203);p.save(S/"Preview/Portrait-native.png")
p.resize((p.width*4,p.height*4),Image.Resampling.NEAREST).save(S/"Preview/Portrait-4x.png")
qa=dict(modules=records,combinedOpaquePalette=len(colors),resampling=False,structuralRailsAndBackingOpaque=True,allRoundtripsExact=True)
(S/"QA/Native.json").write_text(json.dumps(qa,indent=2))
print(len(parts),"modules",len(colors),"opaque colors; every roundtrip and alpha check passed")
