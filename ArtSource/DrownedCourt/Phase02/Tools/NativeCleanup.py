from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
from collections import Counter
import json,hashlib
ROOT=Path(__file__).resolve().parents[1]; REPO=ROOT.parents[2]
def rgb(h):return tuple(bytes.fromhex(h.lstrip("#")))
def hx(p):return "#%02X%02X%02X"%p[:3]
GROUPS={
"DC_A01_Reef_Spearman":{
"0A0D11":"101214 15171A 0C0D0F 211616",
"344845":"263333 344845","496962":"415D58 496962 50756C","6F9B8B":"5E877A 669284 6F9B8B 7CA593","A6C4AA":"9FB79D 96A993 C3CAAF",
"A96326":"935321 A96326 BE722D","D68B3D":"D68B3D E6A75C","F0BC7C":"EEB977 F4C49C",
"4B2432":"42201D 5B2925","783C4A":"71342E 853C35","A96365":"964741",
"A94C3D":"A3462F BE5337","DC7A56":"D66B4A E57E5B","F2B087":"F19A76 CB9372",
"4A2C1C":"30241F 42271A 53321F","7A4D2E":"684027 7D4A28","E0D2B8":"E5D7BA D4C09E","9F907A":"C1A582 A68868 9C7758","5B5145":"7E6146 6A5136 514D42 5A5E52","B07A43":"B18E58","839096":"717C6C 7F8D88"
},
"DC_A02_Hammerhead_Bruiser":{
"0A0D11":"0E1012 121416 0B0D0E 090A0B 1F1F1F",
"2B3945":"202A32 293642","45596A":"334352 3B4E5F 415566 45596A 4A5D6E","6B7D90":"506375 596C7D 65778A 6B7D90",
"FDF5E5":"F6F6F6","E5D6BD":"E5D6BD D0C2A9","BFB398":"BFB398 C0BAA9","9F907A":"9D9580",
"55606E":"494543 56534F 696867","93A1B0":"88857D 9C9B98","6C5A4A":"79766C 6C6352",
"4A2C1C":"442F21 523623 633F26","7A4D2E":"754B2B 86552E 6D523A","B07A43":"9F6732","3E312A":"302925 3F3834"
},
"DC_A03_Pearl_Thief":{
"0A0D11":"050B14 090A0B 02060D 060608",
"57406F":"57406F","725282":"725282 8A6592","A683B4":"98729F A683B4","D5B5E6":"BE9BD2 D9BDEF",
"242D3D":"201E35 373251","47536A":"43405E",
"8A5622":"66461B 8E5C1A","C58826":"C18B2D F1B841","FBEA90":"F5D496",
"9F907A":"8B7A68 AB9B82","B7A393":"CDB898","FDF5E5":"F3E6CB F8F8F7","4A2C1C":"3D2B2C 533130"
},
"DC_A04_Pearl_Cantor":{
"0A0D11":"0B0E11 0E1317 080A0B","142B35":"132128 1A3341","25454D":"213D4C 2A4754","4A6E6C":"35535A 3E5C61",
"A8703C":"936337 B27B46","D69D59":"CC924E E3AB5F","F3CB8C":"F3C886",
"B7A393":"C4A179","E0D2B8":"F1D7B5","FDF5E5":"F9EBD4",
"4A2C1C":"251E16 3F2C1E","7A4D2E":"724828","9F907A":"70675F 908C89"
}}
(ROOT/"Candidates").mkdir(exist_ok=True)
reports=[]
for name,groups in GROUPS.items():
    im=Image.open(ROOT/"Raw"/(name+".png")).convert("RGBA")
    mapping={rgb(src):rgb(dst) for dst,sources in groups.items() for src in sources.split()}
    missing={p[:3] for p in im.get_flattened_data() if p[3] and p[:3] not in mapping}
    assert not missing,(name,missing)
    result=im.copy();result.putdata([mapping[p[:3]]+(p[3],) if p[3] else p for p in im.get_flattened_data()])
    assert result.getchannel("A").tobytes()==im.getchannel("A").tobytes()
    assert result.size==im.size
    result.save(ROOT/"Candidates"/(name+".png"))
    reports.append(dict(id=name,operation="Exhaustive native material-color mapping only; no pixel relocation, mask edits or resampling",alpha_mask_identical=True,raw_colors=len(set(p for p in im.get_flattened_data() if p[3])),candidate_colors=len(set(p for p in result.get_flattened_data() if p[3])),changed_rgb_pixels=sum(a!=b for a,b in zip(im.get_flattened_data(),result.get_flattened_data())),mapping={"#"+dst:["#"+s for s in ss.split()] for dst,ss in groups.items()}))
cell=Image.open(ROOT/"Raw"/"DC_M02_Marine_Cell.png").convert("RGBA")
palette=[rgb(h) for h in ("243739","384B49","465954","61766D","7A8E80","99A897","B3B5A0")]
pixels=[];padding=0
for p in cell.get_flattened_data():
    if p[3]<255 or min(p[:3])>220: pixels.append(palette[0]+(255,));padding+=1
    else:pixels.append(min(palette,key=lambda c:sum((c[i]-p[i])**2 for i in range(3)))+(255,))
cell.putdata(pixels); cell.save(ROOT/"Candidates"/"DC_M02_Marine_Cell.png")
reports.append(dict(id="DC_M02_Marine_Cell",operation="Native RGB material mapping; pale/transparent padding replaced with opaque grout; no resampling",padding_pixels_replaced=padding,palette=[hx(p) for p in palette]))
for name in ("DC_M01_Marine_Frame","DC_E01_Reefgate_Study"):
    im=Image.open(ROOT/"Raw"/(name+".png")); im.save(ROOT/"Candidates"/(name+".png"))
    reports.append(dict(id=name,operation="Unmodified RGBA pixels; copied as candidate"))
(ROOT/"CleanupRecord.json").write_text(json.dumps(reports,indent=2),encoding="utf8")
font=ImageFont.truetype(str(REPO/"Assets/_Game/Fonts/Thaleah/ThaleahFat.ttf"),16)
sheet=Image.new("RGB",(1168,680),(108,108,108)); draw=ImageDraw.Draw(sheet);draw.fontmode="1"
for n,name in enumerate(GROUPS):
    for row,folder in enumerate(("Raw","Candidates")):
        im=Image.open(ROOT/folder/(name+".png")).convert("RGBA").resize((256,256),Image.Resampling.NEAREST)
        x=16+n*288;y=28+row*328
        sheet.paste(im,(x,y),im)
        draw.text((x,y+267),("RAW" if row==0 else "CANDIDATE")+" / "+name[7:].replace("_"," "),(255,255,255),font=font)
sheet.save(ROOT/"Inspection"/"Cleanup_4x.png")
print(json.dumps([{k:v for k,v in r.items() if k!="mapping"} for r in reports],indent=2))

