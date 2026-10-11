"""Inspect source colors and pack unchanged inputs for free PixelLab exact edits."""
from pathlib import Path
from PIL import Image
import base64,json
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake';sheet=Image.new('RGBA',(512,128));ops=[]
for name,x,ramp in [('BackdropQuiet',0,['10151fff','171f2cff','222d3dff','303c4fff']),('PanelTimber',128,['171411ff','292019ff','3b2c20ff','52402fff'])]:
    im=Image.open(root/'environment'/(name+'.png')).convert('RGBA');sheet.paste(im,(x,0));colors=sorted({p for p in im.get_flattened_data() if p[3]})
    levels=[sum(c[:3])/3 for c in colors];lo,hi=min(levels),max(levels)
    mapping={'#'+bytes(c).hex():'#'+ramp[min(3,int(4*(sum(c[:3])/3-lo)/(hi-lo+1)))] for c in colors}
    ops.append(dict(operation='map_colors',layer='image',frames=[1],region=[x,0,x+127,127],colors=mapping,reason='Quiet the inspected busy backdrop behind gem and text focal areas, preserving source shapes in four restrained material shades.'))
middle=Image.open(root/'theme/PlayerMiddle.png').convert('RGBA');sheet.paste(middle,(256,0))
patches=[[256+x,y,'#'+bytes(middle.getpixel((x,y))).hex(),'#00000000'] for y in range(4) for x in range(12,134) if middle.getpixel((x,y))[3]]
ops.append(dict(operation='patch_pixels',layer='image',frames=[1],pixels=patches,reason='The repeated frame center incorrectly covers the player. Retain both side rails and open the middle to true transparency.'))
sheet.save(root/'theme/material-repair-input.png')
(root/'theme/material-repair-request.json').write_text(json.dumps(dict(image='data:image/png;base64,'+base64.b64encode((root/'theme/material-repair-input.png').read_bytes()).decode(),changes=ops),indent=2))
print('Inspected palette maps and 488 guarded alpha repairs prepared.')
