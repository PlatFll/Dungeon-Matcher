"""Independent Stage 2 export, mask, native-scale, preservation and source checks."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,xml.etree.ElementTree as ET
R=Path("ArtSource/ForestHybrid/Stage02");A=Path("Assets/_Game/Art/ForestHybrid/Stage02")
def rgba(p):return Image.open(p).convert("RGBA")
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
plan=json.loads((R/"Scripts/NativePixelPlan.json").read_text(encoding="utf-8"));manifest=[]
for a in plan["assets"]:
 p=A/(a["name"]+".png");im=rgba(p);px=list(im.get_flattened_data())
 assert im.size==(a["w"],a["h"]),p
 assert {v[3] for v in px}<={0,255},p
 assert all(v==(0,0,0,0) for v in px if not v[3]),p
 opened=rgba(Path(".utmp/forest-stage02/reopened")/p.name);assert im.tobytes()==opened.tobytes(),("LibreSprite roundtrip",p)
 b=im.getbbox()
 if any(s in a["name"] for s in ["Corner_","Clamp","Leaves_","Pennant"]):
  assert b[0]>0 and b[1]>0 and b[2]<im.width and b[3]<im.height,("cut contour",p,b)
 if "Rail_" in a["name"]:
  if im.width==16:assert all(im.getpixel((0,y))==im.getpixel((15,y)) for y in range(9)),p
  else:assert all(im.getpixel((x,0))==im.getpixel((x,15)) for x in range(9)),p
 manifest.append({"name":a["name"],"path":str(p).replace("\\","/"),"size":im.size,"palette_count":len({v[:3] for v in px if v[3]}),"alpha":sorted({v[3] for v in px}),"bounds":b,"sha256":sha(p),"role":a["role"]})
corners=[rgba(A/("Portrait_Corner_"+n+".png")) for n in ["TL","TR","BL","BR"]]
assert all(im.tobytes()==corners[0].tobytes() for im in corners)
native=rgba(R/"Previews/Unity-portrait-470x288.png")
scaled=[]
for scale in [1,2,3]:
 p=R/"Previews"/("Unity-portrait-"+str(470*scale)+"x"+str(288*scale)+".png")
 im=rgba(p);expected=native.resize(im.size,Image.Resampling.NEAREST)
 diff=sum(x!=y for x,y in zip(expected.get_flattened_data(),im.get_flattened_data()))
 assert diff==0,("Rendered integer scale mismatch",p,diff)
 scaled.append({"size":im.size,"sha256":sha(p),"exact_integer_pixels":True})
# Deliberately tint only the overfilled backing. No pixel outside the clipping rect may change.
mask=(20,36,131,270);probe=rgba(R/"Previews/Unity-mask-probe.png")
outside=inside=0
for y in range(native.height):
 for x in range(native.width):
  if native.getpixel((x,y))!=probe.getpixel((x,y)):
   if mask[0]<=x<mask[2] and mask[1]<=y<mask[3]:inside+=1
   else:outside+=1
assert outside==0,("Portrait mask leaked",outside)
assert inside>15000,("Probe did not exercise backing",inside)
audit=json.loads((R/"Diagnostics/StartingState.json").read_text(encoding="utf-8"))
changed=[p for p,h in audit["protected"].items() if sha(Path(p))!=h]
assert not changed,("Protected files changed",changed)
tests=ET.parse(R/"Diagnostics/UnityTests.xml").getroot()
assert tests.attrib["failed"]=="0" and tests.attrib["passed"]=="1",tests.attrib
raw=rgba(R/"Sources/PortraitFrame_Raw_PixelLab.png")
clean=rgba(R/"Previews/PortraitFrame_SourceCleanup.png")
assert clean.size==raw.size==(96,128)
decisions=json.loads((R/"Diagnostics/PaletteDecisions.json").read_text())
for src,dst in zip(raw.get_flattened_data(),clean.get_flattened_data()):
 h=bytes(src[:3]).hex()
 if src[3]==0 or h in decisions["removed_white_matte"]:assert dst==(0,0,0,0)
 else:assert dst==tuple(bytes.fromhex(decisions["source_map"][h]))+(255,)
assert clean.tobytes()==rgba(Path(".utmp/forest-stage02/reopened/PortraitFrame_SourceCleanup.png")).tobytes()
# Portrait crop includes the complete outward ivy accents, not only its interior.
crop=native.crop((6,18,152,284));crop.save(R/"Previews/Portrait-native.png")
crop.resize((584,1064),Image.Resampling.NEAREST).save(R/"Previews/Portrait-4x.png")
# Native source modules and native Unity crop on one sheet, never resized.
sheet=Image.new("RGBA",(680,390),(20,30,32,255));d=ImageDraw.Draw(sheet)
d.text((12,10),"FOREST STAGE 2 - NATIVE ASSETS + ACTUAL UNITY PORTRAIT CROP",fill="white")
for i,a in enumerate(manifest):
 x=12+(i%5)*100;y=48+(i//5)*94
 im=rgba(a["path"]);sheet.alpha_composite(im,(x,y))
 name=a["name"].replace("Portrait_","").replace("_","\n")
 d.text((x,y+im.height+5),name,fill=(220,227,208,255))
 d.text((x,y+im.height+31),str(im.width)+"x"+str(im.height)+" / "+str(a["palette_count"])+"c",fill=(147,172,157,255))
sheet.alpha_composite(crop,(524,48));d.text((524,321),"Native Unity crop",fill="white")
sheet.save(R/"Previews/NativeAssets.png")
results={"status":"PASS","new_assets":len(manifest),"editable_documents":len(manifest)+1,
"binary_alpha":True,"libresprite_roundtrip":True,"rail_repeat_seams":True,"four_corners_identical_upright":True,
"frame_geometry":"15px corners / 9px rails / 7px connectors; aligned centers, native tiled rails",
"resize_cases":[[30,30],[128,224],[135,258],[163,287],[129,257]],"undersize_hidden":[29,29],
"portrait_rect":[8,24,135,258],"backing_mask":mask,"mask_probe_inside_changed_pixels":inside,"mask_probe_outside_changed_pixels":outside,
"protected_files_unchanged":len(audit["protected"]),"pixel_lab_calls":1,"pixel_lab_generations_stage2":6,"pixel_lab_generations_cumulative":15,
"raw_opaque_colors":len({p[:3] for p in raw.get_flattened_data() if p[3]}),"clean_opaque_colors":len({p[:3] for p in clean.get_flattened_data() if p[3]}),
"captures":scaled,"manifest":manifest}
(R/"Diagnostics/Validation.json").write_text(json.dumps(results,indent=2),encoding="utf-8")
print(json.dumps({k:v for k,v in results.items() if k not in ["manifest","captures"]},indent=2))
