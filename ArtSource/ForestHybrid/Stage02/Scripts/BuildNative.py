"""Forest Stage 2 portrait shell. Builds exact native edit plans for LibreSprite.
Run from the isolated worktree root. Stage 1 and existing production assets are read-only.
"""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
import json
R=Path("ArtSource/ForestHybrid/Stage02");A=Path("Assets/_Game/Art/ForestHybrid/Stage02");T=Path(".utmp/forest-stage02")
for p in [R/"Scripts",R/"Native",R/"Previews",A,T,T/"reopened"]:p.mkdir(parents=True,exist_ok=True)
P={"ink":"172326","deep":"2b2d27","shadow":"414035","wood":"69523a","light":"8a6845","shine":"b3915f","leafdark":"203c35","leaf":"486a45","leaflight":"81905b","panel0":"182321","panel1":"202b25","panel2":"29342b","cloth":"30573e","clothlight":"486a45","emblem":"9baf7a","emblemlight":"bfca9d"}
def C(n):return tuple(bytes.fromhex(P.get(n,n)))+(255,)
raw=Image.open(R/"Sources/PortraitFrame_Raw_PixelLab.png").convert("RGBA")
mapping={"9d5a33":"wood","653721":"shadow","161315":"ink","91512e":"wood","0d0d0d":"ink","754227":"shadow","c17a42":"light","221917":"deep","526c3a":"leaf","502919":"deep","dba76d":"shine","8e9c4c":"leaflight","303f29":"leafdark"}
discard={"fdfdfa","d2d1cd","c2c2c0"}
clean=Image.new("RGBA",raw.size)
for y in range(raw.height):
 for x in range(raw.width):
  v=raw.getpixel((x,y));h=bytes(v[:3]).hex()
  if not v[3] or h in discard:continue
  assert h in mapping,h
  clean.putpixel((x,y),C(mapping[h]))
# The full cleanup is a review source, not a flattened production frame.
clean.save(T/"SourceColorPlan.png")
assets={};roles={}
def save(n,im,role):assets[n]=im;roles[n]=role
def blank(w,h):return Image.new("RGBA",(w,h))
def rect(im,b,c):ImageDraw.Draw(im).rectangle(b,fill=C(c))
def poly(im,pts,c):ImageDraw.Draw(im).polygon(pts,fill=C(c))
def line(im,pts,c,w=1):ImageDraw.Draw(im).line(pts,fill=C(c),width=w)
# The clean lower-left carved ornament supplies one upright, consistently lit corner.
# Duplicate the carving without mirroring light; harden its boundary around the true rail center.
plate=clean.crop((10,97,25,112))
for y in range(15):
 for x in range(15):
  radius=abs(x-7)+abs(y-7)
  if radius>6:plate.putpixel((x,y),(0,0,0,0))
  elif radius==6:plate.putpixel((x,y),C("ink"))
# Remove accidental hole inherited from inner white matte; fill only the plate interior.
for y in range(2,13):
 for x in range(2,13):
  if abs(x-7)+abs(y-7)<6 and plate.getpixel((x,y))[3]==0:plate.putpixel((x,y),C("wood"))
for k in ["TL","TR","BL","BR"]:save("Portrait_Corner_"+k,plate.copy(),"15x15 upright carved peg. Center (7,7), complete padded contour and consistent top-left light.")
# Native nine-pixel rails. The source material ramps are reconstructed for exact repeat boundaries.
for side in ["Top","Bottom","Left","Right"]:
 horiz=side in ["Top","Bottom"];im=blank(16,9) if horiz else blank(9,16)
 ramp=["ink","light","wood","wood","wood","shadow","shadow","deep","ink"]
 if side=="Bottom":ramp=["ink","wood","wood","shadow","shadow","shadow","deep","deep","ink"]
 if side=="Right":ramp=["ink","wood","wood","shadow","shadow","shadow","deep","deep","ink"]
 for y in range(im.height):
  for x in range(im.width):im.putpixel((x,y),C(ramp[y if horiz else x]))
 # Two restrained connected grain clusters; edge columns/rows remain identical.
 if horiz:
  line(im,[(5,3),(9,3)],"light" if side=="Top" else "wood")
  line(im,[(10,6),(12,6)],"deep")
 else:
  line(im,[(3,4),(3,8)],"light" if side=="Left" else "wood")
  line(im,[(6,10),(6,12)],"deep")
 save("Portrait_Rail_"+side,im,"Straight repeatable rail. Native thickness 9, centerline 4; no texture stretching.")
im=blank(7,7)
rect(im,(1,1,5,5),"ink");rect(im,(2,2,4,4),"wood");line(im,[(2,3),(2,2),(4,2)],"light");rect(im,(3,3,3,3),"deep")
save("Portrait_Clamp",im,"Quiet 7x7 square connector; its center (3,3) aligns to every rail centerline.")
im=blank(32,32);rect(im,(0,0,31,31),"panel1")
rect(im,(0,0,1,31),"panel0");rect(im,(2,0,2,31),"panel2")
rect(im,(16,0,17,31),"panel0");rect(im,(18,0,18,31),"panel2")
line(im,[(8,9),(8,15),(9,17)],"panel0");line(im,[(24,20),(24,25)],"panel2")
save("Portrait_Inset",im,"Quiet 32x32 wooden backing, tiled beyond all interior edges then clipped with RectMask2D.")
# Recover only the two green groups from the source. One-pixel outline is reconnected at native resolution.
for side,box in [("Upper",(10,2,33,25)),("Lower",(72,84,94,108))]:
 crop=clean.crop(box);m=Image.new("L",crop.size)
 for y in range(crop.height):
  for x in range(crop.width):
   if crop.getpixel((x,y)) in [C("leafdark"),C("leaf"),C("leaflight")]:m.putpixel((x,y),255)
 b=m.getbbox();assert b
 sub=crop.crop(b);mask=m.crop(b);im=blank(sub.width+6,sub.height+6)
 alpha=Image.new("L",im.size);alpha.paste(mask,(3,3));outline=alpha.filter(ImageFilter.MaxFilter(3))
 for y in range(im.height):
  for x in range(im.width):
   if outline.getpixel((x,y)):im.putpixel((x,y),C("ink"))
 for y in range(sub.height):
  for x in range(sub.width):
   if mask.getpixel((x,y)):im.putpixel((x+3,y+3),sub.getpixel((x,y)))
 save("Portrait_Leaves_"+side,im,"Source-derived ivy, isolated with full padding; limited to two restrained frame accents.")
# Optional forest identity motif. Native redraw of the mockup's antler pennant, not a generated character.
im=blank(28,44)
poly(im,[(4,3),(23,3),(23,37),(14,42),(4,37)],"ink")
poly(im,[(6,5),(21,5),(21,35),(14,39),(6,35)],"cloth")
line(im,[(6,6),(6,34),(14,38)],"clothlight")
line(im,[(3,3),(24,3)],"shadow",2);rect(im,(3,2,4,4),"light");rect(im,(23,2,24,4),"light")
# Small pale antler mark, with an open center rather than a skeleton/skull face.
for pts in [[(13,26),(9,22),(8,17),(6,14),(6,11)],[(9,20),(11,17),(11,13)],[(15,26),(19,22),(20,17),(22,14),(22,11)],[(19,20),(17,17),(17,13)]]:
 line(im,pts,"emblem",2)
poly(im,[(11,24),(17,24),(17,28),(14,32),(11,28)],"emblem")
line(im,[(12,25),(16,25)],"emblemlight")
save("Portrait_AntlerPennant",im,"28x44 native antler motif from the forest mockup; clear of the separate gem and face.")
def runs(im):
 out=[]
 for y in range(im.height):
  row=[];start=0;last=im.getpixel((0,y))
  for x in range(1,im.width+1):
   v=im.getpixel((x,y)) if x<im.width else None
   if v!=last:row.append([start,x-start,bytes(last).hex()]);start=x;last=v
  out.append(row)
 return out
plan={"palette":P,"assets":[{"name":n,"w":im.width,"h":im.height,"rows":runs(im),"role":roles[n]} for n,im in assets.items()]}
(R/"Scripts/NativePixelPlan.json").write_text(json.dumps(plan,separators=(",",":")),encoding="utf-8")
for im in list(assets.values())+[clean]:
 p=T/("Canvas_"+str(im.width)+"x"+str(im.height)+".png")
 if not p.exists():Image.new("RGBA",im.size).save(p)
full={"name":"PortraitFrame_SourceCleanup","w":clean.width,"h":clean.height,"rows":runs(clean)}
js="// Exact native palette cleanup, source extraction and authored modular pixels executed in LibreSprite.\nvar items="+json.dumps(plan["assets"]+[full],separators=(",",":"))+";\n"
js+=r"""
var pc=app.pixelColor;
items.forEach(function(a){
 var doc=app.open('.utmp/forest-stage02/Canvas_'+a.w+'x'+a.h+'.png'),s=doc.sprite,im=s.layer(0).cel(0).image;
 if(s.width!==a.w||s.height!==a.h)throw Error('Native canvas mismatch');
 a.rows.forEach(function(row,y){row.forEach(function(run){
 var h=run[2],v=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));
 for(var x=run[0];x<run[0]+run[1];x++)im.putPixel(x,y,v);
 });});
 s.layer(0).name=a.name+' - native modular cleanup';s.commit();
 s.saveAs('ArtSource/ForestHybrid/Stage02/Native/'+a.name+'.aseprite',true);
 var dest=a.name==='PortraitFrame_SourceCleanup'?'ArtSource/ForestHybrid/Stage02/Previews/':'Assets/_Game/Art/ForestHybrid/Stage02/';
 s.saveAs(dest+a.name+'.png',true);
});
"""
(R/"Scripts/AuthorNative.js").write_text(js,encoding="utf-8")
(R/"Diagnostics/PaletteDecisions.json").write_text(json.dumps({"source_map":{k:P[v] for k,v in mapping.items()},"removed_white_matte":sorted(discard),"native_size":[96,128],"no_resampling":True,"palette":P},indent=2),encoding="utf-8")
(R/"Placement.json").write_text(json.dumps({"width":470,"height":288,"portrait":[8,24,135,258],"inset":12,"corner":15,"rail":9,"connector":7,"standing_y":258,
"portrait_contents":{"pennant":[53,15],"gem":[51,78],"rattlebones":[35,124],"hp":[15,211,104,24]},
"notes":"Matches existing Forest reference portrait rect (8,24,135,258). Reassemble Stage 1 repeats at native scale to the current 470x288 top HUD reference. No original asset or layout file changed."},indent=2),encoding="utf-8")
names=list(assets)+[full["name"]]
(R/"Scripts/VerifyNativeReopen.js").write_text("var names="+json.dumps(names)+";\nnames.forEach(function(n){var d=app.open('ArtSource/ForestHybrid/Stage02/Native/'+n+'.aseprite');d.sprite.saveAs('.utmp/forest-stage02/reopened/'+n+'.png',true);});\n",encoding="utf-8")
print("Prepared",len(assets),"modular sprites and one editable source-cleanup document.")
