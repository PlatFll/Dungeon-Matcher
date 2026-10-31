from pathlib import Path
import json,hashlib
from PIL import Image,ImageChops,ImageDraw
R=Path(__file__).resolve().parents[4]
S=R/"ArtSource/SnowyMountainStaged/Stage07"
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
source={"LowerRock_Left":("SelectedLeftRock",(3,5,63,58)),"LowerRock_Right":("SelectedRightRock",(2,5,62,60)),"SupplyCrate":("Crate",(3,3,37,36))}
art=[]
for p in sorted((S/"Exports").glob("*.png")):
 a=Image.open(p).convert("RGBA");b=Image.open(S/"Roundtrip"/p.name).convert("RGBA")
 assert a.size==b.size and a.tobytes()==b.tobytes(),p.name+" roundtrip"
 assert set(a.getchannel("A").getdata())<={0,255},p.name+" alpha"
 bb=a.getbbox();assert bb[0]>=1 and bb[1]>=1 and bb[2]<=a.width-1 and bb[3]<=a.height-1,p.name+" edge clearance"
 if p.stem in source:
  name,box=source[p.stem];original=Image.open(S/"Raw"/(name+".png")).convert("RGBA").crop(box)
  assert a.crop((1,1,a.width-1,a.height-1)).getchannel("A").tobytes()==original.getchannel("A").tobytes(),p.name+" original silhouette"
 else:assert a.tobytes()==Image.open(R/"ArtSource/SnowyMountainStaged/Stage06/Exports/IceCluster.png").convert("RGBA").tobytes(),"Approved ice reused exactly"
 art.append({"name":p.stem,"dimensions":list(a.size),"colors":len(set(q for q in a.getdata() if q[3])),"bounds":bb,"binary_alpha":True,"roundtrip_exact":True,"source_mask_exact":True,"sha256":sha(p)})
(S/"Manifest.json").write_text(json.dumps({"stage":7,"status":"review_candidate","art":art,"pixellab_generations":21,"rock_selection":[4,13],"ice_reused_exact":True},indent=2))
baseline=json.loads((S/"QA/StartingHashes.json").read_text());changed=[p for p,h in baseline.items() if not (R/p).exists() or sha(R/p)!=h];assert not changed,changed
records=[];measure=S/"Preview/Measurements.json"
if measure.exists():
 for m in json.loads(measure.read_text())["items"]:
  name=m["size"];a=Image.open(S/"Preview/Before"/(name+".png")).convert("RGB");b=Image.open(S/"Preview/After"/(name+".png")).convert("RGB")
  assert a.size==b.size;fr=m["boardFrame"];p=m["nativePixelScale"];w,h=a.size
  cutoff=h-round(fr["y"]-8*p)
  diff=ImageChops.difference(a,b)
  assert diff.crop((0,0,w,cutoff)).getbbox() is None,(name,"Approved top, board and Stage6 dressing changed")
  assert diff.crop((round(w*.25),cutoff,round(w*.75),h)).getbbox() is None,(name,"Central resonance/text lane changed")
  controls=[]
  for c in m["geometry"]["controls"]:
   if c["name"] not in ["Bomb","HealthPotion","AbilityButton"]:continue
   r=c["rect"];x,y,cw,ch=round(r["x"]),round(r["y"]),round(r["width"]),round(r["height"]);box=(x,h-y-ch,x+cw,h-y)
   d=diff.crop(box)
   if c["name"]=="AbilityButton":assert d.getbbox() is None,(name,c["name"])
   else:
    mask=Image.open(R/c["sprite"]).convert("RGBA").getchannel("A").resize((cw,ch),Image.Resampling.NEAREST).point(lambda v:255 if v==255 else 0)
    assert ImageChops.multiply(d.convert("RGB"),Image.merge("RGB",(mask,mask,mask))).getbbox() is None,(name,c["name"],"opaque control pixels changed")
   controls.append(c["name"])
  changed_pixels=sum(q!=(0,0,0) for q in diff.getdata());assert changed_pixels>50
  records.append({"size":name,"changed_pixels":changed_pixels,"exact_unchanged":["upper_scene","board","approved_board_side_dressing","central_bottom_lane"],"opaque_control_pixels_unchanged":controls})
report={"protected_files":len(baseline),"changed_protected_files":changed,"native_assets":len(art),"views":records}
prior=S/"QA/Validation.json"
if prior.exists() and "unity" in json.loads(prior.read_text()):report["unity"]=json.loads(prior.read_text())["unity"]
(S/"QA/Validation.json").write_text(json.dumps(report,indent=2))
sheet=Image.new("RGB",(960,350),"#17222d");draw=ImageDraw.Draw(sheet)
for i,item in enumerate(art):
 a=Image.open(S/"Exports"/(item["name"]+".png")).convert("RGBA");x=i*240
 draw.text((x+8,8),item["name"]+" / "+str(a.width)+"x"+str(a.height),fill="white");sheet.paste(a,(x+8,32),a)
 z=a.resize((a.width*3,a.height*3),Image.Resampling.NEAREST);sheet.paste(z,(x+32,110),z);draw.text((x+8,320),str(item["colors"])+" colors / 1x + 3x",fill="#b8d9e8")
sheet.save(S/"Preview/NativeAssets.png")
print(json.dumps(report,indent=2))
