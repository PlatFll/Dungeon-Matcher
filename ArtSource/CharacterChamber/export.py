"""Export the two PixelLab originals through LibreSprite; run from the repository root."""
import argparse
import re
import subprocess
import tempfile
import uuid
from pathlib import Path
from PIL import Image, ImageDraw

source = Path(__file__).resolve().parent
project = source.parents[1]
out = project / "Assets/_Game/Resources/UI/CharacterChamber"
template = (out.parent / "MerchantStall/Card.png.meta").read_text()
parser = argparse.ArgumentParser()
parser.add_argument("libresprite", help="Path to the LibreSprite executable")
args = parser.parse_args()
sheet = Image.open(source / "FittingsSource.png").convert("RGBA")
boxes = {
    "Plaque": ((4, 4, 124, 58), (15, 11, 15, 13)),
    "Stats": ((134, 8, 252, 57), (8, 8, 8, 8)),
    "Parchment": ((4, 74, 123, 149), (12, 12, 12, 12)),
    "Roster": ((146, 66, 227, 153), (10, 9, 10, 10)),
    "RosterSelected": ((22, 162, 106, 251), (10, 9, 10, 10)),
    "LevelUp": ((134, 178, 250, 233), (9, 9, 9, 9)),
}
def export(name, im, border=(0, 0, 0, 0)):
    with tempfile.TemporaryDirectory() as scratch:
        raw = Path(scratch) / "clean.png"
        im.save(raw)
        ase = source / (name + ".ase")
        subprocess.run([args.libresprite, "-b", str(raw), "--save-as", str(ase)], check=True)
        png = out / (name + ".png")
        subprocess.run([args.libresprite, "-b", str(ase), "--save-as", str(png)], check=True)
        restored = Image.open(png).convert("RGBA")
        assert restored.size == im.size and restored.tobytes() == im.convert("RGBA").tobytes(), name
    meta = png.with_suffix(".png.meta")
    if not meta.exists():
        guid = uuid.uuid5(uuid.NAMESPACE_URL, "dungeon-matcher/character-chamber/" + name).hex
        text = re.sub(r"(?m)^guid: .+$", "guid: " + guid, template)
        text = re.sub(r"spriteBorder: .*", "spriteBorder: {x: %d, y: %d, z: %d, w: %d}" % border, text)
        meta.write_text(text)

for name, (box,border) in boxes.items():
    im = sheet.crop(box)
    for y in range(im.height):
        for x in range(im.width):
            r,g,b,a = im.getpixel((x,y))
            if a and (r,g,b) in [(52,38,61),(51,37,61),(53,40,62),(51,38,59)]:
                im.putpixel((x,y),(51,37,61,255))
    export(name,im,border)
chamber = Image.open(source / "ChamberSource.png").convert("RGBA")
export("Chamber",chamber)
# Crop a complete brick repeat out of the same chamber for the lower shelves.
export("Masonry",chamber.crop((115,31,195,64)))

# Stat glyphs use the same crisp outline and upper-left lighting as the menu.
icons = {
    "LevelStar": ([(7,0),(9,4),(14,5),(11,8),(12,13),(7,11),(3,14),(4,9),(0,5),(5,4)], "#e7ac40","#ffe99a"),
    "Heart": ([(1,3),(3,1),(6,1),(7,3),(9,1),(12,1),(14,3),(14,7),(7,14),(0,7),(0,4)], "#c95168","#ffa0a8"),
    "Energy": ([(7,0),(13,0),(9,5),(14,5),(3,15),(5,9),(1,9)], "#efbb40","#fff1a6"),
    "Shield": ([(1,2),(7,0),(13,2),(13,8),(11,11),(7,14),(3,11),(1,8)], "#7396b9","#bfdceb"),
}
for name,(points,fill,glint) in icons.items():
    im=Image.new("RGBA",(16,16)); d=ImageDraw.Draw(im)
    d.polygon(points,fill=fill,outline="#251c31")
    if name=="Heart": d.line([(2,5),(3,3),(5,3)],fill=glint)
    elif name=="Energy": d.line([(5,7),(8,2),(10,2)], fill=glint)
    elif name=="Shield": d.line([(3,3),(6,2),(6,10)],fill=glint)
    else: d.line([(3,5),(6,5),(7,2)],fill=glint)
    export(name,im)
