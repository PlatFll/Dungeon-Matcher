"""Independent native/export/Unity evidence checks; run from the Forest worktree root."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,xml.etree.ElementTree as ET
ROOT=Path("ArtSource/ForestHybrid/Stage01")
ART=Path("Assets/_Game/Art/ForestHybrid/Stage01")
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def rgba(p):return Image.open(p).convert("RGBA")
plan=json.loads((ROOT/"Scripts/NativePixelPlan.json").read_text(encoding="utf-8"))
manifest=[]
for a in plan["assets"]:
 p=ART/(a["name"]+".png");im=rgba(p);pixels=list(im.get_flattened_data())
 assert im.size==(a["width"],a["height"]),p
 assert set(v[3] for v in pixels)<={0,255},p
 assert all(v==(0,0,0,0) for v in pixels if v[3]==0),"Transparent RGB residue"
 bound=im.getbbox()
 if a["name"].startswith(("Tree_","Support_")) or a["name"] in ["Canopy_Left","Canopy_Right"]:
  assert bound[0]>0 and bound[1]>0 and bound[2]<im.width and bound[3]<im.height,("cut silhouette",p,bound)
 if a["name"].startswith("Ground_"):
  assert all(im.getpixel((0,y))==im.getpixel((im.width-1,y)) for y in range(im.height)),("tile edge",p)
 # Native editable-document reopen exports must be pixel-identical.
 reopened=rgba(Path(".utmp/forest-stage01/reopened")/p.name)
 assert im.tobytes()==reopened.tobytes(),("LibreSprite roundtrip",p)
 manifest.append({"name":a["name"],"path":str(p).replace("\\","/"),"width":im.width,"height":im.height,
 "opaque_colors":len({v[:3] for v in pixels if v[3]}),"alpha":sorted({v[3] for v in pixels}),"bounds":bound,
 "sha256":sha(p),"editable":str(ROOT/"Native"/(a["name"]+".aseprite")).replace("\\","/"),"role":a["role"]})
raw=rgba(ROOT/"Sources/FarForest_Raw_PixelLab.png")
fixed=rgba(ART/"Far_Forest.png")
mapping=json.loads((ROOT/"Diagnostics/PaletteDecisions.json").read_text())["raw_map"]
expected=[tuple(bytes.fromhex(mapping[bytes(p[:3]).hex()]))+(255,) for p in raw.get_flattened_data()]
assert list(fixed.get_flattened_data())==expected,"Far source was scaled or changed beyond the explicit color map"
# Check two ground variants connect in either order.
for a in ["A","B"]:
 for b in ["A","B"]:
  for kind in ["Top","Face"]:
   left,right=rgba(ART/("Ground_"+kind+a+".png")),rgba(ART/("Ground_"+kind+b+".png"))
   assert all(left.getpixel((59,y))==right.getpixel((0,y)) for y in range(left.height))
layout=json.loads((ROOT/"Placement.json").read_text())
assembly=Image.new("RGBA",(layout["width"],layout["height"]))
for p in sorted(layout["placements"],key=lambda a:a["layer"]):assembly.alpha_composite(rgba(p["path"]),(p["x"],p["y"]))
assert assembly.getchannel("A").getextrema()==(255,255),"Unfilled background gap"
assembly.save(ROOT/"Previews/Assembly-native.png")
paths={"Rattlebones":"Docs/ArtDirection/Audit2026-10/media/skeleton-still.png","Farmer":"Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png","PanVillager":"Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png"}
cast=assembly.copy()
for p in layout["probes"]:
 im=rgba(paths[p["name"]])
 assert p["y"]+im.getbbox()[3]==layout["standing_y"],"Foot baseline mismatch"
 cast.alpha_composite(im,(p["x"],p["y"]))
cast.save(ROOT/"Previews/Assembly-cast-native.png")
cast.resize((1080,720),Image.Resampling.NEAREST).save(ROOT/"Previews/Assembly-cast-3x.png")
# Actual captured Unity pixels must equal their own integer nearest-neighbor scale.
checks=[]
for stem in ["Unity-structure-","Unity-cast-"]:
 first=rgba(ROOT/"Previews"/(stem+"360x240.png"))
 for scale in [1,2,3]:
  p=ROOT/"Previews"/(stem+str(360*scale)+"x"+str(240*scale)+".png")
  captured=rgba(p)
  assert first.resize(captured.size,Image.Resampling.NEAREST).tobytes()==captured.tobytes(),("Unity scale changed pixels",p)
  checks.append({"path":str(p).replace("\\","/"),"size":captured.size,"integer_pixel_match":True,"sha256":sha(p)})
# Existing imported assets/scene hashes must remain exact.
audit=json.loads((ROOT/"Diagnostics/StartingState.json").read_text())
changed=[p for p,h in audit["protected"].items() if sha(Path(p))!=h]
assert not changed,("Existing art/UI/scene changed",changed)
tests=ET.parse(ROOT/"Diagnostics/UnityTests.xml").getroot()
assert tests.attrib["failed"]=="0" and tests.attrib["passed"]!="0",tests.attrib
# Full native asset contact sheet. No resampling of individual sprites.
board=Image.new("RGBA",(900,630),(19,25,29,255));d=ImageDraw.Draw(board)
positions=[(16,28),(90,28),(170,28),(244,28),(312,28),(390,28),(466,28),(544,28),(625,28),(710,28),(16,300),(124,300),(232,300),(302,300),(376,300)]
for a,(x,y) in zip(manifest,positions):
 im=rgba(a["path"]);board.alpha_composite(im,(x,y))
 d.text((x,y+im.height+5),a["name"].replace("_","\n"),fill=(223,230,221,255))
 d.text((x,y+im.height+31),str(im.width)+"x"+str(im.height)+" / "+str(a["opaque_colors"])+"c",fill=(150,173,166,255))
d.text((16,10),"FOREST STAGE 1 - ALL ASSETS AT 1 NATIVE PIXEL PER IMAGE PIXEL",fill="white")
board.save(ROOT/"Previews/NativeAssets.png")
report={"status":"PASS","assets":len(manifest),"binary_alpha":True,"native_roundtrip":True,"ground_variant_seams":True,"full_background_coverage":True,
"cast_foot_baseline":layout["standing_y"],"protected_files_unchanged":len(audit["protected"]),
"pixel_lab_calls":1,"pixel_lab_generations":9,"raw_far_colors":len(set(raw.get_flattened_data())),"clean_far_colors":len(set(fixed.get_flattened_data())),
"unity_test":tests.attrib,"captures":checks,"manifest":manifest}
(ROOT/"Diagnostics/Validation.json").write_text(json.dumps(report,indent=2),encoding="utf-8")
print(json.dumps({k:v for k,v in report.items() if k not in ["manifest","captures","unity_test"]},indent=2))
