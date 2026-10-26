from pathlib import Path
from PIL import Image
R=Path(__file__).resolve().parents[2]
for prefix,h in [('FloorTop',16),('Fascia',26)]:
 for suffix,w in [('A',64),('B',64),('C',64),('Fill',8),('Left',16),('Right',16)]:
  Image.new('RGBA',(w,h)).save(R/f'Native/Stage03/Work-{prefix+suffix}.png')
