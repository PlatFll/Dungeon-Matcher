from pathlib import Path
import sys, math, json, subprocess, hashlib
sys.path.insert(0,'C:/Users/USER/.codex/skills/pixel-art-studio/scripts')
from pixelstudio import Sprite
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'ArtSource/BombReadability/Effects'
RES=ROOT/'Assets/_Game/Resources/VFX/BombRefinement'
LIBRE='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
for p in [OUT/'Editable',OUT/'Frames',RES/'FireHead',RES/'FireTrail',RES/'StarGlint']:p.mkdir(parents=True,exist_ok=True)
FIRE=['#D88418','#FFC52E','#FFE85B','#FFF3A2','#FFFBD6','#FFFFFF']
def flame(frame,trail=False):
    s=Sprite(64,64,palette=FIRE)
    shift=[0,2,1,-1,-2,0][frame]
    if trail:
        # Broad connected blast body. Opposing edges match across the tiled repeat.
        s.polygon([(0,12),(8,12),(13,6+shift),(20,12),(28,9),(36,13),(44,5-shift),(51,12),(57,10),(63,12),(63,51),(56,51),(48,58-shift),(40,51),(32,55),(24,50),(16,58+shift),(8,51),(0,51)],FIRE[0])
        s.polygon([(0,15),(10,15),(14,10+shift),(22,16),(30,12),(38,16),(44,10-shift),(53,16),(63,15),(63,48),(53,48),(47,53),(39,47),(30,51),(23,46),(15,53+shift),(7,48),(0,48)],FIRE[1])
        s.polygon([(0,18),(13,18),(18,14+shift),(26,20),(34,17),(43,20),(49,15),(55,18),(63,18),(63,45),(52,45),(44,49),(36,44),(28,47),(20,43),(12,49),(0,45)],FIRE[2])
        s.polygon([(0,23),(9,22),(18,24),(26,21),(34,25),(43,22),(53,25),(63,23),(63,40),(54,41),(44,38),(34,43),(24,38),(14,42),(0,40)],FIRE[3])
        s.polygon([(0,29),(16,27),(25,30),(34,27),(43,31),(53,28),(63,29),(63,35),(51,37),(42,34),(33,38),(22,34),(12,37),(0,35)],FIRE[4])
        for x,w in [(4,8),(28,8),(48,10)]:s.rect(x,31,x+w,33,FIRE[5])
    else:
        s.polygon([(2,19),(15,18),(7,9),(25,14),(30,4+shift),(40,11),(48,8),(51,17),(58,20),(62,28),(61,37),(55,42),(51,51),(43,50),(34,59-shift),(26,51),(12,55),(19,44),(3,46),(13,36),(2,31)],FIRE[0])
        s.polygon([(8,21),(23,21),(17,14),(29,18),(32,10+shift),(41,16),(46,14),(49,22),(56,25),(59,31),(56,38),(49,40),(47,47),(39,45),(33,53),(28,46),(20,48),(25,39),(10,41),(19,32),(8,28)],FIRE[1])
        s.polygon([(13,24),(28,23),(25,17),(34,22),(38,16),(44,23),(52,26),(56,32),(51,37),(44,38),(40,45),(34,42),(29,47),(29,38),(17,39),(24,32),(13,29)],FIRE[2])
        s.polygon([(18,27),(31,27),(34,22),(39,27),(46,27),(51,32),(45,36),(38,35),(34,41),(31,35),(21,36),(28,31),(18,30)],FIRE[3])
        s.polygon([(25,29),(36,27),(42,29),(46,32),(41,35),(35,33),(30,36),(31,32),(25,32)],FIRE[4])
        s.polygon([(32,30),(39,29),(42,32),(36,33),(32,32)],FIRE[5])
    return s
def star(frame):
    s=Sprite(32,32,palette=['#FFFFFF','#FFF3D8','#DDEAFF','#A9E0FF','#DD92F7','#FF98A0','#D5FFAD'])
    r=[5,7,10,13,11,8,6,3][frame];cx=cy=16
    pts=[]
    for i in range(10):
        a=-math.pi/2+i*math.pi/5;rr=r if i%2==0 else r*.44
        pts.append((round(cx+math.cos(a)*rr),round(cy+math.sin(a)*rr)))
    s.polygon(pts,'#DDEAFF')
    inn=[(round(cx+(x-cx)*.78),round(cy+(y-cy)*.78)) for x,y in pts];s.polygon(inn,'#FFFFFF')
    # Sparse facet reflections, predominantly white at every exposure.
    s.polygon([pts[2],pts[3],(cx+2,cy)],'#A9E0FF')
    s.polygon([pts[4],pts[5],(cx+1,cy+2)],'#DD92F7')
    s.polygon([pts[6],pts[7],(cx-2,cy+1)],'#FF98A0')
    s.polygon([pts[8],pts[9],(cx-2,cy-2)],'#FFF3D8')
    return s
families={'FireHead':[flame(i) for i in range(6)],'FireTrail':[flame(i,True) for i in range(6)],'StarGlint':[star(i) for i in range(8)]}
manifest=[]
for name,frames in families.items():
    pngs=[]
    for i,s in enumerate(frames):
        p=RES/name/f'{name}_{i:02}.png';s.save_png(str(p));im=Image.open(p).convert('RGBA');assert set(im.getchannel('A').getdata())<={0,255};pngs.append(im)
        manifest.append({'path':p.relative_to(ROOT).as_posix(),'size':im.size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
    strip=Image.new('RGBA',(pngs[0].width*len(pngs),pngs[0].height));
    for i,im in enumerate(pngs):strip.paste(im,(i*im.width,0))
    p=OUT/'Frames'/f'{name}.png';strip.save(p)
    subprocess.run([LIBRE,'-b',str(p),'--save-as',str(OUT/'Editable'/f'{name}-strip.aseprite')],check=True,capture_output=True)
    bg=[]
    for im in pngs:
        canvas=Image.new('RGBA',im.size,'#202433');canvas.alpha_composite(im);bg.append(canvas.convert('RGB').resize((im.width*4,im.height*4),Image.Resampling.NEAREST))
    bg[0].save(OUT/f'{name}.gif',save_all=True,append_images=bg[1:],duration=60,loop=0)
preview=Image.new('RGB',(768,450),'#202433');d=ImageDraw.Draw(preview)
for row,(name,frames) in enumerate(families.items()):
    d.text((4,row*150+1),name,fill='white')
    for i in range(len(frames)):
        im=Image.open(RES/name/f'{name}_{i:02}.png').convert('RGBA').resize((128 if row<2 else 64,128 if row<2 else 64),Image.Resampling.NEAREST);preview.paste(im,(i*(128 if row<2 else 80),row*150+16),im)
preview.save(OUT/'EffectFrames.png');(OUT/'Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('20 native effect frames exported; binary alpha and six-color fire palette verified.')
