from pathlib import Path
from PIL import Image
import json
repo=Path(__file__).resolve().parents[4]
out=repo/'.utmp/enchanted-stage01';out.mkdir(parents=True,exist_ok=True)
for name,w,h,x,y in json.loads(Path(__file__).with_name('Modules.json').read_text()):
    Image.new('RGBA',(w,h)).save(out/(name+'.png'))
print('Blank native document canvases prepared. Run AuthorNative.js in LibreSprite from the repository root.')
