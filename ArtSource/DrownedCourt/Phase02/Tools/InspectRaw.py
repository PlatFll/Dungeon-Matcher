from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
from collections import Counter
import json, hashlib
ROOT=Path(__file__).resolve().parents[1]
REPO=ROOT.parents[2]
def inspect(path):
    data=path.read_bytes(); im=Image.open(path).convert("RGBA")
    colors=Counter(p for p in im.getdata() if p[3])
    alpha=Counter(p[3] for p in im.getdata())
    return dict(file=str(path.relative_to(ROOT)),width=im.width,height=im.height,bounds=im.getbbox(),visible_colors=len(colors),alpha=dict(sorted(alpha.items())),sha256=hashlib.sha256(data).hexdigest(),palette=[dict(hex="#%02X%02X%02X"%c[:3],alpha=c[3],pixels=n) for c,n in colors.most_common()])
rows=[inspect(p) for p in sorted((ROOT/"Raw").glob("*.png"))]
(ROOT/"Inspection").mkdir(exist_ok=True)
(ROOT/"Inspection"/"RawMeasurements.json").write_text(json.dumps(rows,indent=2),encoding="utf8")
font=ImageFont.truetype(str(REPO/"Assets/_Game/Fonts/Thaleah/ThaleahFat.ttf"),16)
sheet=Image.new("RGB",(1168,368),(108,108,108)); draw=ImageDraw.Draw(sheet)
draw.text((20,12),"RAW PIXELLAB RETURNS / 4X NEAREST / BEFORE NATIVE PALETTE CHECK",(255,255,255),font=font)
for n,row in enumerate(r for r in rows if Path(r["file"]).name.startswith("DC_A")):
    im=Image.open(ROOT/row["file"]).convert("RGBA").resize((256,256),Image.Resampling.NEAREST)
    x=16+288*n; sheet.paste(im,(x,50),im)
    draw.text((x,316),Path(row["file"]).stem[7:].replace("_"," "),(255,255,255),font=font)
    draw.text((x,338),str(row["visible_colors"])+" COLORS; "+str(row["width"])+"X"+str(row["height"]),(235,235,235),font=font)
sheet.save(ROOT/"Inspection"/"Raw_4x.png")
print(json.dumps([{k:r[k] for k in ("file","width","height","bounds","visible_colors","alpha")} for r in rows]))
for r in rows:
    if Path(r["file"]).stem.startswith("DC_A"): print(Path(r["file"]).stem,[(x["hex"],x["pixels"]) for x in r["palette"][:8]])

