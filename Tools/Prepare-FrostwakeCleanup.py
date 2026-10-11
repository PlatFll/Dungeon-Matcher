"""Read-only native assembly and exact inspected shell-window edit proposals."""
from pathlib import Path
from PIL import Image
import json,base64
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake';out=root/'cleanup'
sheet=Image.new('RGBA',(192,64));pixels=[]
for i,name in enumerate(['Frost','HeavyIce','Bifrost']):
    im=Image.open(root/'environment'/(name+'.png')).convert('RGBA');sheet.paste(im,(i*64,0))
    for y in range(64):
        for x in range(64):
            # Windows chosen after inspection of each actual generated shell.
            inside=((x-31.5)/17.5)**2+((y-32)/17)**2<=1 if i==0 else \
                (((x-31.5)/16.7)**2+((y-31.5)/16.7)**2<=1 if i==1 else 26<=x<=43 and 27<=y<=37)
            c=im.getpixel((x,y))
            if inside and c[3]:pixels.append([x+i*64,y,'#'+bytes(c).hex(),'#00000000'])
sheet.save(out/'shell-source.png');(out/'shell-patches.json').write_text(json.dumps(pixels))
print(json.dumps({'image':base64.b64encode((out/'shell-source.png').read_bytes()).decode(),'pixels':pixels}))
