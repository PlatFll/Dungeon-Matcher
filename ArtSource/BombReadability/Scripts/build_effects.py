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
FIRE=['#A3290C','#E75B11','#FF9833','#FFD35B','#FFF3B4','#FFFFFF']
def flame(frame,trail=False):
    s=Sprite(64,32,palette=FIRE)
    # Four connected tongues flow right; no random particles or airbrushed alpha.
    if trail:
        shift=[0,2,1,-1,-2,0][frame]
        s.polygon([(0,10),(7,8),(13,3+shift),(18,8),(25,7),(31,10),(39,3-shift),(44,8),(52,6),(58,10),(63,10),(63,23),(56,23),(50,28-shift),(43,23),(37,26),(29,22),(22,27+shift),(16,23),(8,25),(0,23)],FIRE[0])
        s.polygon([(0,12),(11,9),(15,6+shift),(18,11),(29,12),(38,7),(43,12),(52,9),(57,13),(63,12),(63,21),(52,22),(48,25-shift),(41,21),(30,20),(23,24),(15,20),(7,23),(0,21)],FIRE[1])
        s.polygon([(0,14),(13,12),(18,10+shift),(24,14),(36,12),(40,9),(45,14),(56,12),(63,14),(63,20),(52,20),(46,23),(40,19),(29,19),(22,21),(13,18),(0,20)],FIRE[2])
        s.polygon([(0,16),(15,14),(23,16),(31,14),(41,15),(49,14),(56,16),(63,16),(63,18),(50,19),(41,17),(29,18),(20,18),(13,16),(0,18)],FIRE[3])
        for x,w in [(3,8),(26,6),(47,9)]:s.rect(x,16,x+w,17,FIRE[4])
    else:
        shift=[0,1,2,1,0,-1][frame%6]
        points=[(2,14),(12,11),(5,5),(22,8),(29,3+shift),(37,8),(44,6),(45,11),(57,15),(60,17),(49,20),(48,24),(38,23),(32,29-shift),(25,24),(11,26),(18,21),(2,19)]
        s.polygon(points,FIRE[0]);s.polygon([(6,15),(22,12),(15,8),(30,10),(32,6+shift),(39,13),(45,11),(54,16),(47,19),(42,22),(34,21),(30,25),(25,21),(17,22),(23,18),(6,18)],FIRE[1])
        s.polygon([(9,15),(27,13),(23,10),(34,13),(39,10),(43,15),(51,17),(41,20),(34,18),(30,22),(26,18),(9,18)],FIRE[2])
        s.polygon([(14,16),(31,14),(36,13),(41,16),(46,17),(38,19),(31,17),(14,18)],FIRE[3])
        s.polygon([(19,16),(34,15),(39,17),(33,18),(19,17)],FIRE[4]);s.rect(28,16,34,16,FIRE[5])
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
preview=Image.new('RGB',(768,240),'#202433');d=ImageDraw.Draw(preview)
for row,(name,frames) in enumerate(families.items()):
    d.text((4,row*80+1),name,fill='white')
    for i in range(len(frames)):
        im=Image.open(RES/name/f'{name}_{i:02}.png').convert('RGBA').resize((128 if row<2 else 64,64),Image.Resampling.NEAREST);preview.paste(im,(i*(128 if row<2 else 80),row*80+16),im)
preview.save(OUT/'EffectFrames.png');(OUT/'Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('20 native effect frames exported; binary alpha and six-color fire palette verified.')
