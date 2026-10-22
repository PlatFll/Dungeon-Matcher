"""Stage 3 explicit native plans, executed in LibreSprite; no approved asset writes."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
import json,shutil,hashlib
R=Path('ArtSource/ForestHybrid/Stage03');A=Path('Assets/_Game/Art/ForestHybrid/Stage03');T=Path('.utmp/forest-stage03')
for p in [A,R/'Native',R/'Scripts',R/'Previews',T,T/'reopened']:p.mkdir(parents=True,exist_ok=True)
P=json.loads(Path('ArtSource/ForestHybrid/Stage02/Diagnostics/PaletteDecisions.json').read_text())['palette']
def C(n):return tuple(bytes.fromhex(P.get(n,n)))+(255,)
def blank(w,h):return Image.new('RGBA',(w,h))
def poly(im,points,c):ImageDraw.Draw(im).polygon(points,fill=C(c))
def line(im,points,c,w=1):ImageDraw.Draw(im).line(points,fill=C(c),width=w)
def rect(im,b,c):ImageDraw.Draw(im).rectangle(b,fill=C(c))
raw=Image.open(R/'Sources/BoardFrame_Raw_PixelLab.png').convert('RGBA');clean=blank(*raw.size)
# Source already used the approved wood palette. Normalize transparent RGB and consolidate the brightest wood planes.
source_map={'b3915f':'8a6845','8a6845':'69523a','69523a':'69523a','414035':'414035','172326':'172326','486a45':'486a45','203c35':'203c35'}
for y in range(raw.height):
 for x in range(raw.width):
  p=raw.getpixel((x,y))
  if p[3]:clean.putpixel((x,y),C(source_map[bytes(p[:3]).hex()]))
assets={};roles={}
def save(n,im,role):assets[n]=im;roles[n]=role
# Wider structural plate around the generated knot. Keep the carving's native 11x11 pixels.
plate=blank(23,23)
poly(plate,[(4,1),(18,1),(21,4),(21,18),(18,21),(4,21),(1,18),(1,4)],'ink')
poly(plate,[(4,2),(18,2),(20,4),(20,18),(18,20),(4,20),(2,18),(2,4)],'wood')
line(plate,[(3,17),(3,5),(5,3),(17,3)],'light')
line(plate,[(4,18),(6,19),(17,19),(19,17),(19,5)],'shadow')
knot=clean.crop((95,19,106,30));plate.alpha_composite(knot,(6,6))
for n in ['TL','TR','BL','BR']:save('Board_Corner_'+n,plate.copy(),'23x23 chamfered structural block with the native PixelLab spiral carving; centered at (11,11), upright light.')
for side in ['Top','Bottom','Left','Right']:
 h=side in ['Top','Bottom'];im=blank(32,11) if h else blank(11,32)
 ramp=['ink','light','wood','wood','wood','wood','wood','shadow','shadow','deep','ink']
 if side in ['Bottom','Right']:ramp=['ink','wood','wood','wood','wood','wood','shadow','shadow','shadow','deep','ink']
 for y in range(im.height):
  for x in range(im.width):im.putpixel((x,y),C(ramp[y if h else x]))
 for start,end,v,c in [(7,17,3,'light' if side in ['Top','Left'] else 'wood'),(19,25,7,'deep')]:
  line(im,[(start,v),(end,v)] if h else [(v,start),(v,end)],c)
 save('Board_Rail_'+side,im,'11px native rail, centered at 5; exact repeated endpoints and broad quiet timber planes.')
peg=blank(11,11)
poly(peg,[(3,1),(7,1),(9,3),(9,7),(7,9),(3,9),(1,7),(1,3)],'ink')
poly(peg,[(3,2),(7,2),(8,3),(8,7),(7,8),(3,8),(2,7),(2,3)],'wood')
line(peg,[(3,6),(3,3),(6,3)],'light');rect(peg,(5,5,6,6),'deep')
save('Board_Connector',peg,'11x11 centered wooden peg; shared material family with the approved portrait.')
post=blank(23,64)
for x in range(1,22):
 c='ink' if x in [1,21] else 'light' if x==3 else 'shadow' if x>=18 else 'wood'
 line(post,[(x,0),(x,63)],c)
line(post,[(8,10),(8,29),(7,33)],'shadow');line(post,[(14,35),(14,53)],'light')
save('Side_PostRepeat',post,'23x64 sturdy vertical support; intended open repeat ends, exact y-seam equality.')
cap=blank(27,24)
poly(cap,[(5,1),(21,1),(25,5),(25,18),(21,22),(5,22),(1,18),(1,5)],'ink')
poly(cap,[(5,3),(21,3),(23,5),(23,18),(21,20),(5,20),(3,18),(3,5)],'wood')
line(cap,[(4,16),(4,6),(6,4),(20,4)],'light');line(cap,[(6,19),(20,19),(22,17)],'shadow')
line(cap,[(9,9),(17,9),(18,10),(18,14),(10,14),(10,11),(14,11)],'shadow')
save('Side_PostCap',cap,'Padded 27x24 carved cap; overlaps repeat by four pixels to avoid cut end seams.')
foot=blank(31,36)
poly(foot,[(7,1),(23,1),(23,21),(29,29),(29,33),(1,33),(1,29),(7,21)],'ink')
poly(foot,[(9,2),(21,2),(21,22),(27,29),(27,31),(3,31),(3,29),(9,22)],'wood')
line(foot,[(10,4),(10,23),(5,29),(19,29)],'light');line(foot,[(20,4),(20,23),(25,29)],'shadow')
save('Side_PostFoot',foot,'31x36 complete spreading base; kept outside all cells, no clipping or flattened crop.')
# Source-derived leaf accents; never scale the generated clusters.
greens={C('leaf'),C('leafdark')}
for name,box in [('Upper',(12,11,31,40)),('Lower',(96,83,115,113))]:
 src=clean.crop(box);mask=Image.new('L',src.size)
 for y in range(src.height):
  for x in range(src.width):
   if src.getpixel((x,y)) in greens:mask.putpixel((x,y),255)
 b=mask.getbbox();assert b;src=src.crop(b);mask=mask.crop(b);im=blank(src.width+6,src.height+6)
 alpha=Image.new('L',im.size);alpha.paste(mask,(3,3));edge=alpha.filter(ImageFilter.MaxFilter(3))
 for y in range(im.height):
  for x in range(im.width):
   if edge.getpixel((x,y)):im.putpixel((x,y),C('ink'))
 for y in range(src.height):
  for x in range(src.width):
   if mask.getpixel((x,y)):im.putpixel((x+3,y+3),src.getpixel((x,y)))
 save('Board_Ivy_'+name,im,'Padded source ivy cluster; used only outside cell opening, no bright gem-like colors.')
tie=blank(23,13);poly(tie,[(3,1),(19,1),(21,3),(21,9),(19,11),(3,11),(1,9),(1,3)],'ink')
rect(tie,(3,3,19,9),'shadow');line(tie,[(4,3),(18,3)],'light');rect(tie,(6,5,7,6),'deep');rect(tie,(15,5,16,6),'deep')
save('Side_Brace',tie,'23x13 horizontal support brace joining each outer post to the rim; does not enter cells.')
def runs(im):
 out=[]
 for y in range(im.height):
  row=[];start=0;last=im.getpixel((0,y))
  for x in range(1,im.width+1):
   v=im.getpixel((x,y)) if x<im.width else None
   if v!=last:row.append([start,x-start,bytes(last).hex()]);start=x;last=v
  out.append(row)
 return out
plan={'palette':P,'assets':[{'name':n,'w':im.width,'h':im.height,'rows':runs(im),'role':roles[n]} for n,im in assets.items()]}
(R/'Scripts/NativePixelPlan.json').write_text(json.dumps(plan,separators=(',',':')))
for im in list(assets.values())+[clean]:Image.new('RGBA',im.size).save(T/('Canvas_'+str(im.width)+'x'+str(im.height)+'.png'))
full={'name':'BoardFrame_SourceCleanup','w':128,'h':128,'rows':runs(clean)}
js='// Exact native pixel plans executed in LibreSprite.\nvar items='+json.dumps(plan['assets']+[full],separators=(',',':'))+';\n'
js+='''var pc=app.pixelColor;
items.forEach(function(a){var doc=app.open('.utmp/forest-stage03/Canvas_'+a.w+'x'+a.h+'.png'),s=doc.sprite,im=s.layer(0).cel(0).image;
if(s.width!==a.w||s.height!==a.h)throw Error('Canvas mismatch');
a.rows.forEach(function(row,y){row.forEach(function(run){var h=run[2],v=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=run[0];x<run[0]+run[1];x++)im.putPixel(x,y,v);});});
s.layer(0).name=a.name+' native';s.commit();s.saveAs('ArtSource/ForestHybrid/Stage03/Native/'+a.name+'.aseprite',true);
var dest=a.name==='BoardFrame_SourceCleanup'?'ArtSource/ForestHybrid/Stage03/Previews/':'Assets/_Game/Art/ForestHybrid/Stage03/';s.saveAs(dest+a.name+'.png',true);});
'''
(R/'Scripts/AuthorNative.js').write_text(js)
names=list(assets)+[full['name']]
(R/'Scripts/VerifyNativeReopen.js').write_text('var names='+json.dumps(names)+";\nnames.forEach(function(n){var d=app.open('ArtSource/ForestHybrid/Stage03/Native/'+n+'.aseprite');d.sprite.saveAs('.utmp/forest-stage03/reopened/'+n+'.png',true);});\n")
(R/'Diagnostics/PaletteDecisions.json').write_text(json.dumps({'source_map':source_map,'palette_from':'ArtSource/ForestHybrid/Stage02/Diagnostics/PaletteDecisions.json','no_resampling':True,'source_carving_rect':[95,19,106,30],'source_carving_destination':[6,6]},indent=2))
print('Prepared',len(assets),'native modules plus cleaned PixelLab source.')
