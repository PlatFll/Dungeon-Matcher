from pathlib import Path
import json, hashlib, shutil, xml.etree.ElementTree as ET
from PIL import Image,ImageDraw,ImageChops
F=Path(r"C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher")
S=F/"ArtSource/SnowyMountainStaged/Stage02"
items=json.loads((S/"Preview/Measurements.json").read_text())["items"]
checks=[]
for r in items:
 a=Image.open(S/"Preview/Before"/(r["size"]+".png")).convert("RGB")
 b=Image.open(S/"Preview/After"/(r["size"]+".png")).convert("RGB")
 delta=ImageChops.difference(a,b)
 rr,gg,bb=delta.split();diff=ImageChops.lighter(ImageChops.lighter(rr,gg),bb).point(lambda x:255 if x else 0)
 w,h=diff.size
 def count(im):return im.histogram()[255]
 v=r["viewport"]
 x0=max(0,int(v["x"])-2);x1=min(w,int(v["x"]+v["width"])+3)
 y0=max(0,h-int(v["y"]+v["height"])-3);y1=min(h,h-int(v["y"])+3)
 outside=diff.copy();ImageDraw.Draw(outside).rectangle((x0,y0,x1-1,y1-1),fill=0)
 g=r["geometry"]
 locked={}
 for name in ["portrait","board","bottom"]:
  q=g[name];xx0=max(0,int(q["x"])+2);xx1=min(w,int(q["x"]+q["width"])-2)
  yy0=max(0,h-int(q["y"]+q["height"])+2);yy1=min(h,h-int(q["y"])-2)
  locked[name]=count(diff.crop((xx0,yy0,xx1,yy1)))
 q=g['portrait'];inset=round(10*r['nativePixelScale'])
 locked['portraitInterior']=count(diff.crop((round(q['x'])+inset,h-round(q['y']+q['height'])+inset,round(q['x']+q['width'])-inset,h-round(q['y'])-inset)))
 assert locked['board']==0 and locked['bottom']==0 and locked['portraitInterior']==0
 assert count(outside)==0
 checks.append(dict(size=r["size"],changedPixels=count(diff),outsideBackgroundViewport=count(outside),lockedRegionChangedPixels=locked))
print(json.dumps(checks,indent=2))
(S/"QA/ScreenDiff.json").write_text(json.dumps(checks,indent=2))
# Review board only; the screenshots themselves remain untouched.
a=Image.open(S/"Preview/Before/720x1280.png");b=Image.open(S/"Preview/After/720x1280.png")
board=Image.new("RGB",(1464,532),"#0d1723");d=ImageDraw.Draw(board)
d.text((12,10),"BEFORE - existing Frostwake backdrop",fill="#e0eef7")
d.text((744,10),"AFTER - Stage 2 selected alpine pass",fill="#e0eef7")
board.paste(a.crop((0,0,720,490)),(12,32));board.paste(b.crop((0,0,720,490)),(744,32))
board.save(S/"Preview/Comparison.png")
xml=F/".utmp/ForestValidation/d480919b-ed07-4c22-87fc-76820ca9f839.xml"
root=ET.parse(xml).getroot()
v={"unityCompile":{"command":"powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1","result":"Passed","log":r"C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-a22bc20a-b130-456e-b940-6f74f95d59a3.log"},
"unityPreviewTest":{"command":"powershell -ExecutionPolicy Bypass -File Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainStage02BackdropReview -Graphics","result":root.attrib.get("result"),"total":root.attrib.get("total"),"passed":root.attrib.get("passed"),"failed":root.attrib.get("failed"),"screens": [r["size"] for r in items],"testXML":str(xml.relative_to(F))},
"deviceTesting":"Not performed. Screenshots are actual Unity Editor Game renders, not physical-phone captures.",
"scope":"Only an editor-test scenery override; production prefabs, themes and scenes remain unchanged.",
"knownExistingIssues":["Legacy Frostwake outer enclosure and old portrait/board/HUD materials remain pending their later stages.","At 360x640 the existing Pixel Perfect Camera warning and crowded upper labels are also present in the baseline; this stage does not alter shared UI."]}
assert root.attrib["result"]=="Passed"
(S/"QA/UnityChecks.json").write_text(json.dumps(v,indent=2))
shutil.copy2(xml,S/"QA/UnityTests.xml")
for n in ["build_review.py","prepare.py","finalize.py"]:
 p=Path(r"C:/Users/USER/.codex/visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a/SnowyMountainStage02")/n
 if p.exists(): shutil.copy2(p,S/"Scripts"/n)
