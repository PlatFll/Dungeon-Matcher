"""Native component planning; actual editing, ASE saves and PNG exports in LibreSprite."""
from pathlib import Path
from PIL import Image
from collections import Counter
import json,hashlib,subprocess,shutil
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
EXE='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def visible(im):return bytes(v for c in im.get_flattened_data() for v in (c if c[3] else (0,0,0,0)))
def color(h):return tuple(bytes.fromhex(h.lstrip('#')))+(255,)
transparent=(0,0,0,0);recess=color('11101B');outline=color('171622')
raw={n:Image.open(HERE/'Raw'/f'{n}.png').convert('RGBA') for n in ('Ability','Supply','Energy')}
ability=raw['Ability'].copy()
# Lift the top crest, restore the uninterrupted rail under both the crest and
# the unwanted bottom-center split. All corner geometry is kept at native size.
crest=Image.new('RGBA',(22,20))
for y in range(7,25):
 for x in range(85,107):
  c=ability.getpixel((x,y));back=ability.getpixel((65,y))
  if c[3] and c!=back:crest.putpixel((x-85,y-6),c)
  ability.putpixel((x,y),back if back[3] else transparent)
for y in range(50,60):
 for x in range(88,103):ability.putpixel((x,y),ability.getpixel((65,y)))
# Only straight center rails / quiet recess rows are repeated. No scaling.
frame=Image.new('RGBA',(176,66))
ay=list(range(16,26))+[25]*4+list(range(26,50))+[49]*5+list(range(50,60))
for y in range(53):
 sy=ay[y]
 for x in range(172):
  sx=17+x if x<70 else 86 if x<84 else 17+x-14
  c=ability.getpixel((sx,sy))
  if c==color('241F3A'):c=recess
  frame.putpixel((x+2,y+11),c)
# Remove the generator's small gray scratch-like interior swirls. Rebuild them
# as a pair of sparse purple curls, detached from metal and outside the icon.
for y in range(22,53):
 for x in list(range(12,31))+list(range(145,164)):
  if frame.getpixel((x,y))[3] and 20<=y<=52:frame.putpixel((x,y),recess)
swirl=Image.new('RGBA',frame.size)
path=[(28,29),(25,28),(22,29),(20,31),(19,34),(19,37),(21,40),(24,41),(27,40),(28,38),(27,36),(25,36)]
# Connected 1px paths, with one broad quiet shadow cluster underneath.
def line(im,a,b,c):
 x,y=a;xx,yy=b;dx=abs(xx-x);sx=1 if x<xx else -1;dy=-abs(yy-y);sy=1 if y<yy else -1;e=dx+dy
 while True:
  im.putpixel((x,y),c)
  if (x,y)==(xx,yy):break
  e2=2*e
  if e2>=dy:e+=dy;x+=sx
  if e2<=dx:e+=dx;y+=sy
for a,b in zip(path,path[1:]):
 line(swirl,a,b,color('3B0D65'));line(swirl,(175-a[0],a[1]),(175-b[0],b[1]),color('3B0D65'))
frame.alpha_composite(crest,(77,0))
# User revision: only the ability inset matches the shared board tile fill.
for y in range(frame.height):
 for x in range(frame.width):
  if frame.getpixel((x,y))==recess:frame.putpixel((x,y),color('1C2438'))
skull=Image.new('RGBA',(30,40));skull.paste(raw['Ability'].crop((82,70,108,106)),(2,2))
# The icon keeps the generated crown / face. Darken hollow features to the same
# recess rather than treating their gray source fringe as shiny metal.
assets={'AbilityFrame':frame,'AbilityFlourish':swirl,'RattlebonesEmblem':skull}
# Fit the shared square by removing only runs of plain side/rail pixels. Keep
# the generated corner bevels and cardinal jewelry at one source pixel per pixel.
xs=list(range(55,76))+list(range(87,104))+list(range(115,137))
ys=list(range(20,39))+list(range(48,71))+list(range(81,99))
assert len(xs)==len(ys)==60
slot=Image.new('RGBA',(64,64))
for y,sy in enumerate(ys):
 for x,sx in enumerate(xs):
  c=raw['Supply'].getpixel((sx,sy))
  if c==color('241F3A'):c=recess
  slot.putpixel((x+2,y+2),c)
assets['ItemFrame']=slot
# Empty bar casing remains separate from the fill. Shorten the straight trough
# by 23 pixels, keeping both native endcaps and their tiny gold stones intact.
energy=Image.new('RGBA',(147,24))
for y in range(20):
 for x in range(143):
  sx=x+13 if x<71 else x+36
  c=raw['Energy'].getpixel((sx,y+29))
  if c==color('151423'):c=recess
  energy.putpixel((x+2,y+2),c)
assets['EnergyCasing']=energy
fill=Image.new('RGBA',(107,8))
bands=['BC52FA','9020CE','9020CE','9020CE','68168A','68168A','3B0D65','3B0D65']
for y,h in enumerate(bands):
 inset=2 if y in (0,7) else 1 if y in (1,6) else 0
 for x in range(inset,107-inset):fill.putpixel((x,y),color(h))
assets['EnergyFill']=fill
plans=[]
for name,im in assets.items():
 blank=HERE/'QA'/f'Blank-{name}.png';Image.new('RGBA',im.size).save(blank)
 plans.append({'name':name,'blank':blank.as_posix(),'size':list(im.size),'pixels':[[x,y,*im.getpixel((x,y))] for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))[3]]})
script='var plans='+json.dumps(plans)+';\nvar dest='+json.dumps(HERE.as_posix()+'/')+';\n'+r'''
var pc=app.pixelColor;
for(var i=0;i<plans.length;i++){
 var p=plans[i];app.open(p.blank);var s=app.activeSprite;
 if(s.filename.replace(/\\/g,'/')!==p.blank||s.width!==p.size[0]||s.height!==p.size[1])throw new Error('Wrong native canvas');
 var im=s.layer(0).cel(0).image;
 for(var k=0;k<p.pixels.length;k++){var c=p.pixels[k];im.putPixel(c[0],c[1],pc.rgba(c[2],c[3],c[4],c[5]));}
 s.layer(0).name='Royal bevel - PixelLab native cleanup';s.commit();
 s.saveAs(dest+'Editable/'+p.name+'.ase',true);s.saveAs(dest+'PNG/'+p.name+'.png',true);app.activeDocument.close();
 app.open(dest+'Editable/'+p.name+'.ase');app.activeSprite.saveAs(dest+'Roundtrip/'+p.name+'.png',true);app.activeDocument.close();
}
console.log('SILVER_BOTTOM_NATIVE_COMPLETE');app.command.Exit();
'''
(HERE/'QA/NativeProduction.js').write_text(script)
startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
result=subprocess.run(['rtk',EXE,'--script',str(HERE/'QA/NativeProduction.js')],cwd=ROOT,startupinfo=startup,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=90)
(HERE/'QA/LibreSprite.log').write_bytes(result.stdout)
assert result.returncode==0 and b'SILVER_BOTTOM_NATIVE_COMPLETE' in result.stdout,result.stdout.decode(errors='replace')
report={}
for name,expected in assets.items():
 p=HERE/'PNG'/f'{name}.png';im=Image.open(p).convert('RGBA');rt=Image.open(HERE/'Roundtrip'/f'{name}.png').convert('RGBA')
 assert im.size==expected.size==rt.size and visible(im)==visible(expected)==visible(rt),name
 assert set(im.getchannel('A').get_flattened_data())=={0,255},name
 colors=Counter(c[:3] for c in im.get_flattened_data() if c[3]);assert len(colors)<=16,name
 b=im.getbbox();margin=min(b[0],b[1],im.width-b[2],im.height-b[3])
 assert margin>=(0 if name=='EnergyFill' else 1),name
 report[name]={'dimensions':list(im.size),'bounds':b,'opaque_colors':len(colors),'sha256':sha(p),'binary_alpha':True,'exact_libresprite_roundtrip':True,'minimum_margin':margin,'palette':{'#%02X%02X%02X'%c:n for c,n in colors.most_common()}}
 im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST).save(HERE/'Preview'/f'{name}-4x.png')
for n in ('Potion','Bomb'):
 shutil.copy2(ROOT/'Assets/_Game/Resources/UI/Consumables'/f'{n}.png',HERE/'Inputs'/f'{n}.png')
manifest={'stage':3,'selected':'A - Royal bevel','status':'bottom HUD assets pending user review; Unity integration is Stage 4','new_pixellab_generations':18,'prior_pixellab_generations':29,'assets':report,'energy_fill_top_left':[20,7],'energy_fill_size':[107,8],'ability_icon_center':[88,38],'ability_icon_is_character_specific':True,'same_item_template_for_both_supplies':True,'production_runtime_changed':False,'resampling_used_on_production_assets':False}
(HERE/'Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(json.dumps({n:{k:v for k,v in r.items() if k in ('dimensions','opaque_colors','bounds')} for n,r in report.items()}))
