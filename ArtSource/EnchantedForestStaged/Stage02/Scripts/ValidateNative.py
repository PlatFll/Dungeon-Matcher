from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil
p=Path(__file__).resolve().parents[4]
r=p/'ArtSource/EnchantedForestStaged/Stage02'; a=p/'Assets/_Game/Art/EnchantedForestStaged/Stage02'; t=p/'.utmp/enchanted-stage02'
items={};report=[];palette=set()
for f in sorted(a.glob('*.png')):
 im=Image.open(f).convert('RGBA');expected=Image.open(t/(f.stem+'-expected.png')).convert('RGBA');reopen=Image.open(t/'reopened'/f.name).convert('RGBA')
 assert im.tobytes()==expected.tobytes()==reopen.tobytes(),f.name
 colors={c[:3] for c in im.getdata() if c[3]};alpha={c[3] for c in im.getdata()};palette|=colors
 assert alpha <= {0,255}
 bbox=im.getbbox()
 if 'Ivy' in f.stem: assert bbox[0]>0 and bbox[1]>0 and bbox[2]<im.width and bbox[3]<im.height,(f.name,bbox)
 meta=f.with_suffix('.png.meta').read_text()
 for s in ['filterMode: 0','enableMipMap: 0','textureCompression: 0','spritePixelsToUnits: 64']: assert s in meta,(f,s)
 items[f.stem]=im;report.append({'name':f.stem,'size':im.size,'colors':len(colors),'alpha':sorted(alpha),'bounds':bbox,'png_sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'aseprite_sha256':hashlib.sha256((r/'Native'/(f.stem+'.aseprite')).read_bytes()).hexdigest()})
assert len(items)==14
# Native repeating ports must match at both ends and all corner connections.
get=lambda n:items['Portrait_'+n]
for side in ['Top','Bottom']:
 im=get('Rail_'+side); assert im.crop((0,0,1,9)).tobytes()==im.crop((31,0,32,9)).tobytes()
for side in ['Left','Right']:
 im=get('Rail_'+side);assert im.crop((0,0,9,1)).tobytes()==im.crop((0,31,9,32)).tobytes()
for c,hor,ver,hx,vy in [('TL','Top','Left',14,14),('TR','Top','Right',0,14),('BL','Bottom','Left',14,0),('BR','Bottom','Right',0,0)]:
 im=get('Corner_'+c)
 assert im.crop((hx,3,hx+1,12)).tobytes()==get('Rail_'+hor).crop((0,0,1,9)).tobytes(),c+' horizontal'
 assert im.crop((3,vy,12,vy+1)).tobytes()==get('Rail_'+ver).crop((0,0,9,1)).tobytes(),c+' vertical'
sheet=Image.new('RGBA',(460,((len(items)+3)//4)*144),(22,26,28,255));d=ImageDraw.Draw(sheet)
for i,(name,im) in enumerate(items.items()):
 x=(i%4)*115;y=(i//4)*144;d.text((x+4,y+2),name.replace('Portrait_',''),fill='white');d.text((x+4,y+17),str(im.width)+'x'+str(im.height),fill='#a0adb0');sheet.alpha_composite(im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST),(x+4,y+33))
sheet.save(r/'Previews/Modules-2x.png')
data={'passed':True,'module_count':len(items),'combined_opaque_colors':len(palette),'palette':['#%02x%02x%02x'%c for c in sorted(palette)],'binary_alpha':True,'libresprite_reopened_identical':True,'all_eight_corner_ports_match':True,'four_rail_repeat_seams_match':True,'modules':report}
(r/'Diagnostics/NativeValidation.json').write_text(json.dumps(data,indent=2))
print(json.dumps({k:v for k,v in data.items() if k not in ['modules','palette']}))
