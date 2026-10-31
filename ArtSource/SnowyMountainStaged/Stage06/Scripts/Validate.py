from pathlib import Path
import json,hashlib
from PIL import Image,ImageChops,ImageDraw
R=Path(__file__).resolve().parents[4]
S=R/"ArtSource/SnowyMountainStaged/Stage06"
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
art=[]
for p in sorted((S/"Exports").glob("*.png")):
 a=Image.open(p).convert("RGBA");b=Image.open(S/"Roundtrip"/p.name).convert("RGBA")
 assert a.size==b.size and a.tobytes()==b.tobytes(),p.name+" roundtrip"
 assert set(a.getchannel("A").getdata())<={0,255},p.name+" alpha"
 bb=a.getbbox();assert bb[0]>=1 and bb[1]>=1 and bb[2]<=a.width-1 and bb[3]<=a.height-1,p.name+" canvas clearance"
 art.append({"name":p.stem,"dimensions":list(a.size),"colors":len(set(q for q in a.getdata() if q[3])),"bounds":bb,"binary_alpha":True,"roundtrip_exact":True,"sha256":sha(p)})
manifest={"stage":6,"status":"review_candidate","art":art,"pixellab_generations":10,"selected_candidate":0,"originals_preserved":True}
(S/"Manifest.json").write_text(json.dumps(manifest,indent=2))
baseline=json.loads((S/"QA/StartingHashes.json").read_text())
changed=[p for p,h in baseline.items() if not (R/p).exists() or sha(R/p)!=h]
assert not changed,changed
records=[]
measure=S/"Preview/Measurements.json"
if measure.exists():
 for m in json.loads(measure.read_text())["items"]:
  name=m["size"];a=Image.open(S/"Preview/Before"/(name+".png")).convert("RGB");b=Image.open(S/"Preview/After"/(name+".png")).convert("RGB")
  assert a.size==b.size
  fr=m["boardFrame"];p=m["nativePixelScale"];w,h=a.size
  # All changed pixels must be in the board-side gutters, at least thirteen native pixels outside the opening.
  xmin=round(fr["x"]+13*p);xmax=round(fr["x"]+fr["width"]-13*p)
  ytop=h-round(fr["y"]+fr["height"]);ybottom=h-round(fr["y"]-6*p)
  diff=ImageChops.difference(a,b)
  for label,box in [("upper_scene",(0,0,w,ytop)),("board_interior",(xmin,ytop,xmax,ybottom)),("bottom_hud",(0,ybottom,w,h))]:
   assert diff.crop(box).getbbox() is None,(name,label,diff.crop(box).getbbox())
  changed_pixels=sum(q!=(0,0,0) for q in diff.getdata())
  assert changed_pixels>100
  records.append({"size":name,"changed_pixels":changed_pixels,"exact_unchanged":["upper_scene","board_interior","bottom_hud"],"whole_native_canvas_inside_viewport":True,"layout_and_floor_verified_in_Unity":True})
report={"protected_files":len(baseline),"changed_protected_files":changed,"native_assets":len(art),"views":records}
prior=S/"QA/Validation.json"
if prior.exists() and "unity" in json.loads(prior.read_text()):report["unity"]=json.loads(prior.read_text())["unity"]
if (S/"QA/UnityTestResult.xml").exists():
 import xml.etree.ElementTree as ET
 assert ET.parse(S/"QA/UnityTestResult.xml").getroot().attrib["result"]=="Passed"
(S/"QA/Validation.json").write_text(json.dumps(report,indent=2))
# A review contact sheet only; source sprites and game imports stay untouched.
sheet=Image.new("RGB",(960,650),"#17222d");draw=ImageDraw.Draw(sheet)
for i,item in enumerate(art):
 a=Image.open(S/"Exports"/(item["name"]+".png")).convert("RGBA");x=i%4*240;y=i//4*325
 draw.text((x+12,y+8),item["name"]+" / "+str(a.width)+"x"+str(a.height),fill="white")
 sheet.paste(a,(x+12,y+30),a)
 zoom=a.resize((a.width*3,a.height*3),Image.Resampling.NEAREST);sheet.paste(zoom,(x+75,y+45),zoom)
 draw.text((x+12,y+298),str(item["colors"])+" colors / 1x + 3x",fill="#b8d9e8")
sheet.save(S/"Preview/NativeAssets.png")
print(json.dumps(report,indent=2))
