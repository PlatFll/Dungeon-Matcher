from pathlib import Path
import json,hashlib,urllib.request,shutil
from PIL import Image
F=Path(r"C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher")
S=F/"ArtSource/SnowyMountainStaged/Stage02"
scratch=Path(r"C:/Users/USER/.codex/visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a/SnowyMountainStage02")
for d in ["Raw","Selected","Native","Exports","Roundtrip","Preview","QA","Scripts"]: (S/d).mkdir(parents=True,exist_ok=True)
shutil.copy2(scratch/"Generation.json",S/"Generation.json")
for name,job in [("A","526c26a4-46f2-463b-be03-0d747aa44ca7"),("B","d0533cb2-4648-486f-9fe7-7efc9d4d856f")]:
 p=S/"Raw"/(name+".png")
 if not p.exists(): urllib.request.urlretrieve("https://api.pixellab.ai/mcp/images/"+job+"/download",p)
 im=Image.open(p).convert("RGBA")
 print(name,im.size,len(set(im.getdata())),sorted(set(im.getchannel("A").getdata())))
 print(im.getcolors(im.width*im.height)[:3])
 shutil.copy2(p,S/"Selected"/("Alpine"+name+".png"))
pairs=[("Docs/ArtDirection/Audit2026-10/media/skeleton-still.png","705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c"),("Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png","6189bc2cfe01bab84bafada24a7b129a3fd8c2225f629eb8580386f461109e2d"),("Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png","e1be89f8231483df5db6739dd2cae7bef83e11aaa9b028e4cb1b60ea55aba0ca")]
for p,h in pairs: assert hashlib.sha256((F/p).read_bytes()).hexdigest()==h,p
print("Current golden images match registered hashes.")
