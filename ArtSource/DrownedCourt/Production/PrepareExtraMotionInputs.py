from pathlib import Path
from PIL import Image
r=Path(__file__).resolve().parent
captain=Image.open(r/'Selected/breakwater_captain.png').convert('RGBA')
wide=Image.new('RGBA',(128,80));wide.paste(captain,(24,0));wide.save(r/'Selected/breakwater_captain_wide.png')
inflated=Image.open(r/'MotionRaw/puffer_sentinel_ability_v2/03.png').convert('RGBA')
inflated.save(r/'Selected/puffer_sentinel_inflated.png')
