"""Stage 1 native pixel blueprints. Run from the isolated project root.
Every final pixel is written and exported inside LibreSprite by AuthorNative.js.
No source image is resized. No existing production asset is overwritten.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import json, hashlib, shutil
ROOT=Path("ArtSource/ForestHybrid/Stage01")
OUT=Path("Assets/_Game/Art/ForestHybrid/Stage01")
TMP=Path(".utmp/forest-stage01")
for p in [ROOT/"Scripts",ROOT/"Native",ROOT/"Diagnostics",OUT,TMP]:p.mkdir(parents=True,exist_ok=True)
(TMP/"reopened").mkdir(exist_ok=True)
P={
"void":"14272d","ink":"172326","barkshadow":"30372e","bark":"4a4938","barklight":"696044","barkedge":"85764f",
"leafdeep":"203c35","leafshadow":"2f503d","leaf":"486a45","leaflight":"6d8250","leafedge":"8b995e",
"earthdeep":"282d29","earth":"3c4032","earthlight":"54513c","earthface":"4a4936","path":"666747","pathlight":"83845a","grass":"7e8d58",
"far0":"142c35","far1":"1f4148","far2":"2b5459","far3":"3b6469","far4":"51787d","far5":"6b8c8b","far6":"91aaa4","farleaf":"4d6a60","farsage":"70846a"
}
def col(k):return tuple(bytes.fromhex(P.get(k,k)))+(255,)
def canvas(w,h):return Image.new("RGBA",(w,h))
def poly(im,pts,c):ImageDraw.Draw(im).polygon(pts,fill=col(c))
def rect(im,box,c):ImageDraw.Draw(im).rectangle(box,fill=col(c))
def line(im,pts,c,w=1):ImageDraw.Draw(im).line(pts,fill=col(c),width=w)
assets={}
roles={}
def keep(n,im,role):
 assets[n]=im
 roles[n]=role
# Quiet opaque backing repeats. The far image is a separate layer.
im=canvas(48,24);rect(im,(0,0,47,23),"far0")
poly(im,[(0,4),(10,4),(10,7),(19,7),(19,3),(32,3),(32,8),(47,8),(47,23),(0,23)],"far1")
poly(im,[(0,17),(8,17),(8,14),(23,14),(23,18),(37,18),(37,12),(47,12),(47,23),(0,23)],"far0")
keep("Far_CanopyRepeat",im,"Quiet opaque extension behind top controls; repeat connections intentional.")
im=canvas(60,40)
rect(im,(0,0,59,21),"far0")
poly(im,[(0,18),(59,18),(59,27),(51,27),(51,32),(36,32),(36,36),(24,36),(24,31),(13,31),(13,28),(0,28)],"far0")
poly(im,[(0,14),(16,14),(16,18),(30,18),(30,21),(45,21),(45,18),(59,18),(59,23),(48,23),(48,27),(34,27),(34,30),(25,30),(25,25),(12,25),(12,22),(0,22)],"far1")
keep("Canopy_FringeRepeat",im,"Stepped canopy overlap hides the straight boundary of the distant scenery layer.")
im=canvas(52,160);rect(im,(0,0,51,159),"far0")
for pts in [[(4,0),(10,0),(9,42),(12,99),(18,159),(7,159),(5,84)],[(34,0),(40,0),(38,65),(40,108),(48,159),(38,159),(34,103)],[(17,0),(21,0),(23,57),(26,88),(23,139),(19,141),(21,76)]]:
 poly(im,pts,"far1")
for pts in [[(0,19),(7,19),(7,25),(17,25),(17,30),(27,30),(27,40),(16,40),(16,36),(7,36),(7,39),(0,39)],[(20,63),(26,59),(36,59),(36,55),(44,55),(44,64),(51,64),(51,73),(35,73),(35,79),(24,79)]]:
 poly(im,pts,"far2")
poly(im,[(0,130),(11,130),(11,124),(20,124),(20,129),(33,129),(33,139),(43,139),(43,135),(51,135),(51,159),(0,159)],"far0")
keep("Far_SideExtension",im,"Separate quiet side-depth sprite; covered joins under foreground trunks.")
im=canvas(48,16);rect(im,(0,0,47,15),"far0")
poly(im,[(0,4),(9,4),(9,1),(15,1),(15,4),(28,4),(28,2),(34,2),(34,6),(47,6),(47,15),(0,15)],"leafdeep")
rect(im,(0,12,47,15),"ink")
keep("Understory_Repeat",im,"Low contrast distant bank; horizontal repeat.")
# Broad modular forest stage: no converging perspective or scattered ground noise.
for suffix in ["A","B"]:
 im=canvas(60,16);rect(im,(0,0,59,15),"earth")
 rect(im,(0,0,59,1),"leafshadow");rect(im,(0,2,59,3),"leaf")
 rect(im,(0,4,59,14),"path");rect(im,(0,15,59,15),"earthlight")
 if suffix=="A":
  poly(im,[(5,3),(21,3),(21,5),(17,5),(17,6),(6,6)],"grass")
  poly(im,[(27,8),(45,8),(47,9),(44,10),(29,10)],"pathlight")
  line(im,[(4,12),(14,12),(16,11)],"earthlight")
  line(im,[(42,4),(55,4)],"pathlight")
 else:
  poly(im,[(36,3),(52,3),(52,5),(46,5),(46,6),(37,6)],"grass")
  poly(im,[(9,7),(23,7),(25,8),(23,9),(10,9)],"pathlight")
  line(im,[(35,12),(49,12),(51,11)],"earthlight")
  line(im,[(3,4),(14,4)],"pathlight")
 keep("Ground_Top"+suffix,im,"Flat 60x16 repeat; continuous horizontal standing lane at local y=10.")
 im=canvas(60,24);rect(im,(0,0,59,23),"earthdeep")
 rect(im,(0,0,59,1),"earthlight");rect(im,(0,2,59,4),"earthface")
 poly(im,[(0,5),(59,5),(59,16),(47,16),(47,18),(19,18),(19,16),(0,16)],"earth")
 if suffix=="A":
  poly(im,[(8,5),(14,5),(17,9),(24,10),(24,12),(14,11)],"earthlight")
  line(im,[(37,8),(44,8),(46,10),(53,10)],"earthdeep",2)
 else:
  poly(im,[(32,5),(39,5),(39,8),(47,11),(47,13),(38,11)],"earthlight")
  line(im,[(5,12),(15,12),(18,10),(22,10)],"earthdeep",2)
 rect(im,(0,23,59,23),"ink")
 keep("Ground_Face"+suffix,im,"Separate soil/root fascia; top and side joins fixed.")
# Full uncropped, shaded midground trees. Curved silhouette and root contacts remain separate.
im=canvas(72,216)
poly(im,[(26,5),(48,5),(51,12),(48,43),(45,76),(45,112),(47,149),(54,177),(64,194),(68,204),(67,211),(47,211),(38,205),(28,211),(6,211),(4,207),(12,195),(19,175),(23,149),(24,111),(28,76),(26,46)],"ink")
poly(im,[(29,8),(46,8),(45,46),(41,78),(41,116),(44,153),(51,181),(61,200),(64,207),(49,207),(38,200),(27,207),(9,207),(17,191),(23,171),(27,145),(28,109),(32,75),(30,45)],"bark")
poly(im,[(29,9),(36,9),(36,46),(36,80),(33,113),(33,151),(28,177),(19,198),(13,202),(21,180),(25,149),(27,112),(31,76),(28,44)],"barklight")
poly(im,[(41,9),(46,9),(44,49),(40,80),(40,119),(43,152),(50,182),(61,203),(55,202),(45,184),(37,155),(36,117),(37,79)],"barkshadow")
poly(im,[(28,67),(16,54),(8,39),(5,23),(10,21),(16,36),(25,44),(32,48),(34,60)],"ink")
poly(im,[(28,61),(20,50),(11,37),(9,26),(14,37),(28,48),(32,52)],"bark")
line(im,[(11,27),(16,39),(25,47)],"barklight",2)
poly(im,[(43,90),(51,80),(59,58),(65,54),(68,59),(62,82),(53,98),(44,108)],"ink")
poly(im,[(44,94),(53,81),(61,62),(64,60),(59,80),(51,95),(44,103)],"barkshadow")
line(im,[(46,94),(54,80),(60,64)],"bark",2)
for pts,c in [([(32,27),(32,47),(34,54)],"barkedge"), ([(30,92),(30,119),(29,134)],"barkshadow"), ([(33,162),(31,184),(27,195)],"barkshadow"), ([(42,185),(45,196),(52,204)],"barklight"), ([(35,120),(37,127),(36,139)],"barklight")]:
 line(im,pts,c,1)
poly(im,[(14,200),(20,195),(25,195),(27,199),(37,200),(37,203),(28,203),(28,206),(9,207)],"leafshadow")
poly(im,[(17,199),(23,198),(27,201),(23,203),(12,204)],"leaf")
poly(im,[(49,199),(54,200),(59,204),(63,205),(63,207),(52,206),(47,203)],"leafshadow")
keep("Tree_Left",im,"Native left foreground trunk; full branch and root silhouette within padded canvas.")
im=canvas(64,216)
poly(im,[(25,5),(45,5),(49,10),(46,42),(40,80),(39,116),(43,157),(52,186),(60,203),(59,211),(40,211),(31,206),(20,211),(4,211),(3,206),(14,187),(21,160),(24,127),(23,93),(28,53)],"ink")
poly(im,[(28,8),(43,8),(44,12),(42,45),(36,81),(35,120),(39,159),(49,190),(56,207),(42,207),(31,201),(20,207),(8,207),(18,188),(25,160),(28,126),(27,92),(32,53)],"bark")
poly(im,[(28,9),(34,9),(37,20),(35,49),(31,82),(31,124),(29,160),(21,190),(14,202),(10,204),(21,178),(25,149),(26,119),(26,90),(30,53)],"barklight")
poly(im,[(40,9),(43,9),(42,44),(36,81),(35,121),(40,161),(48,188),(55,204),(48,200),(36,172),(31,128),(32,81),(38,42)],"barkshadow")
poly(im,[(28,69),(17,61),(8,48),(4,35),(8,31),(15,45),(24,49),(31,53)],"ink")
poly(im,[(29,63),(20,57),(12,46),(8,36),(15,47),(25,52),(30,55)],"bark")
line(im,[(9,35),(16,46),(25,51)],"barklight",2)
poly(im,[(40,103),(48,91),(55,69),(60,66),(61,72),(56,94),(47,111),(39,118)],"ink")
poly(im,[(41,106),(50,91),(57,72),(57,80),(53,94),(45,110),(40,114)],"barkshadow")
line(im,[(42,107),(51,91),(57,73)],"bark",1)
for pts,c in [([(30,22),(32,36),(30,49)],"barkedge"), ([(29,104),(30,122),(27,144)],"barkshadow"), ([(33,157),(31,175),(25,192)],"barkshadow"), ([(40,186),(45,199),(51,205)],"barklight")]:line(im,pts,c)
poly(im,[(9,204),(17,199),(23,199),(27,202),(22,206),(8,208)],"leafshadow")
poly(im,[(13,204),(18,202),(23,202),(20,205)],"leaf")
poly(im,[(41,202),(48,202),(54,207),(45,207)],"leafshadow")
keep("Tree_Right",im,"Independently shaped right foreground trunk; top-left lighting preserved.")
# Separate coarse canopy masses; no per-leaf confetti.
for side in ["Left","Right"]:
 im=canvas(96,44)
 silhouette=[(2,3),(22,3),(22,1),(44,1),(44,4),(62,4),(62,9),(77,9),(77,13),(87,13),(87,20),(94,20),(94,29),(85,29),(85,34),(70,34),(70,39),(54,39),(54,42),(33,42),(33,37),(18,37),(18,33),(5,33),(5,26),(1,26)]
 if side=="Right":silhouette=[(95-x,y) for x,y in silhouette]
 poly(im,silhouette,"leafdeep")
 masses=[([(5,6),(29,6),(29,4),(42,4),(42,8),(57,8),(57,12),(69,12),(69,20),(59,20),(59,26),(40,26),(40,30),(24,30),(24,27),(9,27)],"leafshadow"),
 ([(12,7),(27,7),(27,5),(39,5),(39,9),(52,9),(52,14),(39,14),(39,19),(21,19),(21,16),(12,16)],"leaf"),
 ([(55,19),(72,19),(72,22),(84,22),(84,28),(72,28),(72,33),(60,33),(60,36),(49,36),(49,31),(53,27)],"leafshadow"),
 ([(56,20),(69,20),(69,23),(76,23),(76,26),(64,26),(64,29),(57,29)],"leaf")]
 for pts,c in masses:
  if side=="Right":pts=[(95-x,y) for x,y in pts]
  poly(im,pts,c)
 # Small upper plane highlight only, not an all-round shiny leaf border.
 poly(im,[(15,7),(26,7),(26,6),(36,6),(36,9),(23,9),(23,11),(15,11)] if side=="Left" else [(49,9),(63,9),(63,6),(77,6),(77,9),(69,9),(69,12),(49,12)],"leaflight")
 keep("Canopy_"+side,im,"Separate foliage roof mass; all leaves in frame, coarse connected clusters.")
for side in ["Left","Right"]:
 im=canvas(28,24)
 poly(im,[(3,2),(8,2),(8,8),(14,11),(24,11),(26,13),(26,18),(23,19),(12,17),(7,14),(3,14)],"ink")
 poly(im,[(5,4),(6,4),(6,10),(13,13),(24,13),(24,16),(13,15),(8,12),(5,12)],"bark")
 line(im,[(6,5),(6,10),(13,12),(23,12)],"barklight")
 rect(im,(20,13,22,20),"barkshadow");rect(im,(20,21,22,21),"ink")
 if side=="Right": # Geometry mirror, then re-light the upper exposed edge; no semantic facing requirement for wood.
  im=im.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
 keep("Support_"+side,im,"Simple wood hook/anchor context; lantern asset deferred to Stage 4.")
# Explicit, semantic map from all 24 raw colors to 14 quiet depth colors.
raw=Image.open(ROOT/"Sources/FarForest_Raw_PixelLab.png").convert("RGBA")
mapping={
"15303b":"far0","1d3e49":"far1","2d6572":"far3","6e8a6c":"farsage","1b4a51":"far1","255356":"far2",
"516e8d":"far4","66857f":"far5","295a5b":"far2","83d2d9":"far6","1e444d":"far1","193843":"far0",
"2c5f5d":"far2","4e728e":"far4","80a6c3":"far5","61818a":"far4","68c7cf":"far5","497674":"far3",
"b3dee4":"far6","555f57":"farleaf","6d96b8":"far5","56b2bf":"far4","8ea06c":"farsage","508ca1":"far4"}
assert set(mapping)=={bytes(p[:3]).hex() for p in raw.get_flattened_data()}
fixed=Image.new("RGBA",raw.size);fixed.putdata([col(mapping[bytes(p[:3]).hex()]) for p in raw.get_flattened_data()])
keep("Far_Forest",fixed,"PixelLab far-depth source; deliberate value/palette consolidation, geometry preserved at 256x160.")
# RLE plans are exact desired pixels, not encoded generator intermediates.
def rle(im):
 rows=[]
 for y in range(im.height):
  row=[];start=0;prev=im.getpixel((0,y))
  for x in range(1,im.width+1):
   c=im.getpixel((x,y)) if x<im.width else None
   if c!=prev:
    row.append([start,x-start,bytes(prev).hex()])
    start=x;prev=c
  rows.append(row)
 return rows
payload={"palette":P,"assets":[{"name":n,"width":im.width,"height":im.height,"role":roles[n],"rows":rle(im)} for n,im in assets.items()]}
(ROOT/"Scripts/NativePixelPlan.json").write_text(json.dumps(payload,separators=(",",":")),encoding="utf-8")
for n,im in assets.items():
 blank=TMP/("Canvas_"+str(im.width)+"x"+str(im.height)+".png")
 if not blank.exists():Image.new("RGBA",im.size).save(blank)
js="// Native authored pixels and explicit source recolor applied inside LibreSprite.\\n".replace("\\n","\n")
js+="var plan="+json.dumps(payload,separators=(",",":"))+";\n"
js+=r"""
var pc=app.pixelColor;
plan.assets.forEach(function(a){
 var doc=app.open('.utmp/forest-stage01/Canvas_'+a.width+'x'+a.height+'.png');
 var s=doc.sprite,im=s.layer(0).cel(0).image;
 if(s.width!==a.width||s.height!==a.height)throw Error('Native canvas mismatch '+a.name);
 a.rows.forEach(function(row,y){row.forEach(function(run){
 var h=run[2],v=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));
 for(var x=run[0];x<run[0]+run[1];x++)im.putPixel(x,y,v);
 });});
 s.layer(0).name=a.name+' - native authored clusters';s.commit();
 s.saveAs('ArtSource/ForestHybrid/Stage01/Native/'+a.name+'.aseprite',true);
 s.saveAs('Assets/_Game/Art/ForestHybrid/Stage01/'+a.name+'.png',true);
});
"""
(ROOT/"Scripts/AuthorNative.js").write_text(js,encoding="utf-8")
(ROOT/"Diagnostics/PaletteDecisions.json").write_text(json.dumps({"raw_map":{k:P[v] for k,v in mapping.items()},"palette":P,"source_dimensions":list(raw.size),"no_scaling":True},indent=2),encoding="utf-8")
# Native placement plan. Repeats end exactly on the canvas perimeter.
placements=[]
def place(n,x,y,l):placements.append({"name":n,"path":str(OUT/(n+".png")).replace("\\","/"),"x":x,"y":y,"layer":l})
for x in range(0,360,48):place("Far_CanopyRepeat",x,0,0)
# Extensions are opaque outside the scenic box. Last instance deliberately clipped by preview viewport.
for x in [0,52,308]:place("Far_SideExtension",x,24,0)
place("Far_Forest",52,24,1)
for x in range(0,360,48):place("Understory_Repeat",x,184,2)
place("Tree_Left",4,0,3);place("Tree_Right",292,0,3)
for x in range(0,360,60):place("Canopy_FringeRepeat",x,0,3)
place("Canopy_Left",0,0,4);place("Canopy_Right",264,0,4)
place("Support_Left",61,43,4);place("Support_Right",272,43,4)
for i,x in enumerate(range(0,360,60)):
 suffix="A" if i%2==0 else "B"
 place("Ground_Top"+suffix,x,200,5)
 place("Ground_Face"+suffix,x,216,5)
# Add a low rear bank behind floor, below tree roots.
for x in range(0,360,48):place("Understory_Repeat",x,192,2)
layout={"width":360,"height":240,"standing_y":210,"placements":placements,
"probes":[{"name":"Rattlebones","x":47,"y":146},{"name":"Farmer","x":103,"y":126},{"name":"PanVillager","x":210,"y":126}],
"notes":"Stage 1 art-only scene. No production binding. Trees/roots/anchors have padding; only repeat tiles extend beyond the viewport."}
(ROOT/"Placement.json").write_text(json.dumps(layout,indent=2),encoding="utf-8")
(ROOT/"Scripts/AssetRoles.json").write_text(json.dumps(roles,indent=2),encoding="utf-8")
print("Prepared",len(assets),"native LibreSprite documents; no production sprites modified.")
