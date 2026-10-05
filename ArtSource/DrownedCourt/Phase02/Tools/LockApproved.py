"""Apply the user's single-color eye correction to exact native pixels."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, json

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT.parent / "Approved"
OUT.mkdir(exist_ok=True)
dark = (10, 13, 17, 255)
edits = {
    "DC_A01_Reef_Spearman": [],  # Already a single solid dark eye.
    "DC_A02_Hammerhead_Bruiser": [(15,14),(16,13),(44,13)],
    "DC_A03_Pearl_Thief": [(20,33),(20,34),(20,35),(30,34),(31,34),(30,35),(31,35),(30,36)],
    "DC_A04_Pearl_Cantor": [(25,15),(26,15),(24,16),(25,16),(26,16)],
}
records = []
sheet = Image.new("RGB", (1152,330), (96,96,96))
draw = ImageDraw.Draw(sheet)
for index, source in enumerate(sorted((ROOT / "Candidates").glob("*.png"))):
    before = Image.open(source).convert("RGBA")
    after = before.copy()
    changes = []
    for x,y in edits.get(source.stem, []):
        old = after.getpixel((x,y))
        assert old[3] == 255
        after.putpixel((x,y), dark)
        changes.append({"x": x,"y": y,"before": list(old),"after": list(dark)})
    target = OUT / source.name
    if changes:
        after.save(target)
    else:
        target.write_bytes(source.read_bytes())
    assert after.getchannel("A").tobytes() == before.getchannel("A").tobytes()
    changed = [(x,y) for y in range(after.height) for x in range(after.width) if before.getpixel((x,y)) != after.getpixel((x,y))]
    assert set(changed) == set(edits.get(source.stem, []))
    records.append({"id": source.stem,"approval": "APPROVED_WITH_USER_REQUESTED_SOLID_DARK_EYES", "approved_on": "2026-10-05", "source": str(source.relative_to(ROOT.parent)).replace("\\","/"), "source_sha256": hashlib.sha256(source.read_bytes()).hexdigest(),"file": target.name, "sha256":hashlib.sha256(target.read_bytes()).hexdigest(),"size":list(after.size),"eye_edits":changes,"alpha_unchanged":True,"pixels_outside_eye_edits_unchanged":True})
    if index < 4:
        sheet.paste(after.resize((256,256),Image.Resampling.NEAREST),(index*288+16,24),after.resize((256,256),Image.Resampling.NEAREST))
        draw.text((index*288+16,292),source.stem[7:].replace("_"," "),fill="white")
(OUT / "APPROVED_DESIGNS.json").write_text(json.dumps({"user_direction":"Approve all Phase 2 designs; change eyes to fully one color; continue remaining implementation.","eye_color":"#0A0D11","art_generation_units":0,"assets":records},indent=2),encoding="utf8")
sheet.save(OUT / "Approved_Four_Solid_Eyes_4x.png")
print(json.dumps({"locked":len(records),"changed_pixels":sum(len(r["eye_edits"]) for r in records),"outside_eye_pixels_changed":0,"paid_generations":0}))
