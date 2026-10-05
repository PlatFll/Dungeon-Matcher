from pathlib import Path
from PIL import Image
r=Path(__file__).resolve().parent
im=Image.open(r/'Selected/reef_spearman.png').convert('RGBA')
out=Image.new('RGBA',(128,80));out.paste(im,(32,0));out.save(r/'Selected/reef_spearman_wide.png')
