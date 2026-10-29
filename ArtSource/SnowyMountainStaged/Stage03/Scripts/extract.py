from pathlib import Path
from PIL import Image
import json
S=Path(r"C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/SnowyMountainStaged/Stage03")
a=Image.open(S/"Raw/PortraitSource.png").convert("RGBA")
parts={}
def crop(name,box):
 im=a.crop(box);im.save(S/"Selected"/(name+".png"));parts[name]={"sourceRect":box,"size":im.size}
crop("Portrait_Rail_Top",(48,14,80,22))
crop("Portrait_Rail_Bottom",(48,244,80,252))
crop("Portrait_Rail_Left",(3,49,11,81))
crop("Portrait_Rail_Right",(117,49,125,81))
for side in ["TL","TR","BL","BR"]:crop("Portrait_Corner_"+side,(0,119,16,135))
crop("Portrait_Clamp",(1,120,13,132))
crop("Backing",(16,53,80,85))
snow={(254,255,255),(205,232,247),(132,175,209),(176,214,238),(163,202,226),(201,236,245)}
for name,box in [("SnowCap_Left",(0,0,32,36)),("SnowCap_Right",(96,0,128,36)),("SnowCap_Middle",(48,0,80,16)),("SnowFoot_Left",(16,232,40,246)),("SnowFoot_Right",(88,232,112,246))]:
 im=a.crop(box)
 im.putdata([p if p[:3] in snow and p[3] else (0,0,0,0) for p in im.getdata()])
 im.save(S/"Selected"/(name+".png"));parts[name]={"sourceRect":box,"size":im.size,"selection":"Snow/ice palette only; transparent remaining native pixels"}
(S/"Extraction.json").write_text(json.dumps(parts,indent=2))
print("Prepared",len(parts),"native module selections.")
