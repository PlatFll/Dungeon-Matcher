from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil,sys
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint04';s=p/'.utmp/forest-v2-c4'
raw=Image.open(o/'Raw/Foliage_PixelLab.png').convert('RGBA')
O=(23,35,38,255); S=(48,87,62,255); B=(72,106,69,255); H=(129,144,91,255)
# Five original PixelLab ivy leaves, reviewed individually at 8x. No new decoration.
boxes=[(15,2,26,13),(31,7,41,16),(43,2,54,13),(2,13,17,28),(2,33,13,46)]
# Preserve the generated leaf silhouettes; simplify its broken vein highlights into
# broad top-left planes. Stems keep their original curves and dark edge pixels.
def planes(im,bs):
    out=im.copy()
    for y in range(im.height):
        for x in range(im.width):
            c=im.getpixel((x,y))
            out.putpixel((x,y),(0,0,0,0) if not c[3] else O if c==O else S)
    for x0,y0,x1,y1 in bs:
        w=x1-x0;h=y1-y0
        masks=[]
        # Connected light and shadow polygons; no per-pixel noise or gradients.
        for pts in [[(x0,y0),(x1,y0),(x1,y0+h//3),(x0+w//3,y1),(x0,y1)],
                    [(x0,y0),(x0+w*3//4,y0),(x0+w//2,y0+h//3),(x0,y0+h//2)]]:
            m=Image.new('1',im.size);ImageDraw.Draw(m).polygon(pts,fill=1);masks.append(m)
        for y in range(y0,y1):
            for x in range(x0,x1):
                c=im.getpixel((x,y))
                if c[3] and c!=O:
                    out.putpixel((x,y),H if masks[1].getpixel((x,y)) else B if masks[0].getpixel((x,y)) else S)
    return out
ul=planes(raw,boxes)
# Drop one unnecessary outline spike above the first leaf, then pad native pixels.
ul.putpixel((24,1),(0,0,0,0))
u=Image.new('RGBA',(64,64));u.alpha_composite(ul,(2,3))
# Reverse only identity geometry. Re-author the new leaf planes from upper-left;
# a rotated lighting map would incorrectly illuminate this companion from below.
geometry=raw.transpose(Image.Transpose.ROTATE_180)
rb=[(64-x1,64-y1,64-x0,64-y0) for x0,y0,x1,y1 in boxes]
lr=planes(geometry,rb);lr.putpixel((39,62),(0,0,0,0))
l=Image.new('RGBA',(64,64));l.alpha_composite(lr,(-2,-3))
for name,im in [('Board_Foliage_UL',u),('Board_Foliage_LR',l)]:im.save(s/(name+'-cleaned.png'))
notes={'source_sha256':hashlib.sha256((o/'Raw/Foliage_PixelLab.png').read_bytes()).hexdigest(),'leaf_count_per_accent':5,'method':'Preserve five generated ivy silhouettes and stem curve; consolidate scattered vein glints into connected matte planes. Remove one top outline spike. Pad upper-left by 2x3 native pixels. Lower-right uses reversed geometry with newly authored top-left planes, not rotated shading. No resampling.','colors':4,'alpha':'binary, zero transparent RGB','canvas':[64,64],'bounds':{'upper_left':u.getbbox(),'lower_right':l.getbbox()},'approval':'candidate awaiting user review'}
(o/'Diagnostics/Cleanup.json').write_text(json.dumps(notes,indent=2),encoding='utf-8')
if Path(__file__).resolve()!=(o/'Scripts/CleanFoliage.py').resolve():shutil.copy2(__file__,o/'Scripts/CleanFoliage.py')
canvas=Image.new('RGBA',(192,64),'#292936')
for i,im in enumerate([raw,u,l]):canvas.alpha_composite(im,(64*i,0))
canvas.resize((1152,384),Image.Resampling.NEAREST).save(Path(__file__).parent/'C4FoliageCleanup-6x.png')
print(json.dumps(notes))
