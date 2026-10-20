from pathlib import Path
from PIL import Image,ImageDraw,ImageFont,ImageChops
import json,hashlib,struct,zlib,itertools,shutil
P=Path(__file__).resolve().parents[3];R=P/"ArtSource/DungeonPixelLab"
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return Image.open(p).convert("RGBA")
files=sorted((R/"Exports/Stage01").glob("*.png"));names=["Wall_A_Quiet","Wall_B_VioletStone","Wall_C_SoftWear","Wall_D_DeepStone"]
ims={p.stem:load(p) for p in files}
def decode_ase(p):
 data=p.read_bytes();w,h,depth=struct.unpack_from("<HHH",data,8);assert depth==32
 pos=128;size=struct.unpack_from("<I",data,pos)[0];pos+=16;out=Image.new("RGBA",(w,h))
 while pos<128+size:
  n,kind=struct.unpack_from("<IH",data,pos)
  if kind==0x2005:
   layer,x,y,opacity,ct=struct.unpack_from("<HhhBH",data,pos+6)
   assert layer==0 and opacity==255 and ct in (0,2)
   cw,ch=struct.unpack_from("<HH",data,pos+22)
   buf=data[pos+26:pos+n];buf=zlib.decompress(buf) if ct==2 else buf
   cel=Image.frombytes("RGBA",(cw,ch),buf);out.paste(cel,(x,y))
  pos+=n
 return out
rows=[]
for f in files:
 im=ims[f.stem];ase=R/("Native/Stage01/"+f.stem+".aseprite")
 assert decode_ase(ase).tobytes()==im.tobytes(),f.stem
 colors=sorted(set(im.get_flattened_data()))
 assert all(c[3]==255 for c in colors)
 rows.append({"name":f.stem,"path":f.relative_to(P).as_posix(),"native_source":ase.relative_to(P).as_posix(),"dimensions":list(im.size),"palette":["#%02X%02X%02X"%c[:3] for c in colors],"opaque_colors":len(colors),"alpha":[255],"bounds":list(im.getbbox()),"sha256":sha(f),"ase_sha256":sha(ase),"native_cel_equals_png":True,"status":"CANDIDATE_PENDING_USER"})
seams=[]
for a,b in itertools.product(names,repeat=2):
 A,B=ims[a],ims[b]
 assert A.crop((47,0,48,48)).tobytes()==B.crop((0,0,1,48)).tobytes(),(a,b,"H")
 assert A.crop((0,47,48,48)).tobytes()==B.crop((0,0,48,1)).tobytes(),(a,b,"V")
 seams.append({"a":a,"b":b,"left_right_match":True,"top_bottom_match":True})
for a,b in itertools.combinations(names,2):
 assert ims[a].tobytes()!=ims[b].tobytes()
 assert not any(ims[a].tobytes()==ImageChops.offset(ims[b],dx,dy).tobytes() for dx in range(48) for dy in range(48)),(a,b,"cyclic-shift duplicate")
# Exact half-module edge reconstruction.
base=ims[names[0]]
for suffix,box in {"Edge_Left":(0,0,24,48),"Edge_Right":(24,0,48,48),"Edge_Top":(0,0,48,24),"Edge_Bottom":(0,24,48,48),"Corner_TL":(0,0,24,24),"Corner_TR":(24,0,48,24),"Corner_BL":(0,24,24,48),"Corner_BR":(24,24,48,48)}.items():
 assert ims["Wall_"+suffix].tobytes()==base.crop(box).tobytes()
assert ims["Wall_Edge_Bottom16"].tobytes()==base.crop((0,0,48,16)).tobytes()
assert ims["Wall_Corner_RightBottom16"].tobytes()==base.crop((0,0,24,16)).tobytes()
field=Image.new("RGBA",(288,192))
pattern=[[0,1,0,2,3,0],[2,0,3,0,1,0],[0,3,0,1,0,2],[1,0,2,0,0,3]]
for y,row in enumerate(pattern):
 for x,v in enumerate(row):field.paste(ims[names[v]],(x*48,y*48))
field.save(R/"Previews/Stage01/Alternating_Field_1x.png")
field.resize((1152,768),Image.Resampling.NEAREST).save(R/"Previews/Stage01/Alternating_Field_4x.png")
raw=load(R/"Sources/Stage01/Wall_Raw_PixelLab.png")
contact=Image.new("RGBA",(384,112),(10,13,17,255));d=ImageDraw.Draw(contact)
d.text((4,3),"Raw 64x64",(218,216,229));contact.paste(raw,(4,25))
for i,n in enumerate(names):
 x=88+i*72;d.text((x,3),"Clean "+chr(65+i),(218,216,229));contact.paste(ims[n],(x,25))
contact.save(R/"Previews/Stage01/Contact_1x.png")
contact.resize((1536,448),Image.Resampling.NEAREST).save(R/"Previews/Stage01/Contact_4x.png")
guide=json.loads((R/"Progress/Stage01_Reference_Audit.json").read_text())
checks={"source_raw_dimensions":list(raw.size),"source_raw_sha256":sha(R/"Sources/Stage01/Wall_Raw_PixelLab.png"),"source_raw_opaque_colors":len(set(raw.get_flattened_data())),"final_family_colors":sorted(set(c for r in rows for c in r["palette"])),"assets":rows,"pairwise_seams":seams,"half_module_reconstruction":True,"test_field_dimensions":[288,192],"layout":pattern,"resampling":"none; copied native PixelLab brick clusters into a new 48x48 repeat, repaired mortar and bevels in LibreSprite","unity":"PENDING"}
placement=json.loads((R/"Progress/Stage01WallPlacement.json").read_text())
expected=Image.new("RGBA",(360,640));coverage=Image.new("L",(360,640));draw=ImageDraw.Draw(coverage)
for p in placement["placements"]:
 im=ims[p["name"]];x,y=p["x"],p["y"]
 assert x>=0 and y>=0 and x+im.width<=360 and y+im.height<=640
 assert not coverage.crop((x,y,x+im.width,y+im.height)).getbbox(),"Overlapping modules"
 expected.paste(im,(x,y));draw.rectangle((x,y,x+im.width-1,y+im.height-1),fill=255)
assert coverage.getextrema()==(255,255),"Wall gap"
screens=[]
for scale in (1,3):
 capture=load(R/f"Previews/Stage01/UnityWall-{360*scale}x{640*scale}.png")
 predicted=expected.resize(capture.size,Image.Resampling.NEAREST)
 assert capture.size==(360*scale,640*scale)
 diff=sum(a!=b for a,b in zip(capture.get_flattened_data(),predicted.get_flattened_data()))
 assert diff==0,(scale,diff)
 screens.append(dict(scale=scale,dimensions=list(capture.size),compared_pixels=capture.width*capture.height,different_rgba_pixels=diff))
for f in files:assert sha(f)==sha(P/"Assets/_Game/Art/DungeonPixelLab/Wall"/f.name)
checks["unity"]={"captures":screens,"pieces":112,"overlaps":0,"gaps":0,"all_sprite_bounds_inside":True,"imports_byte_exact":True}
checks["no_cyclic_shift_duplicates"]=True
(R/"Diagnostics/Stage01/PixelChecks.json").write_text(json.dumps(checks,indent=2)+"\n")
print(json.dumps({"assets":len(rows),"palette_union":checks["final_family_colors"],"native_roundtrips":len(rows),"seam_pairings":len(seams),"alpha":"255 only","field":"6x4"}))
