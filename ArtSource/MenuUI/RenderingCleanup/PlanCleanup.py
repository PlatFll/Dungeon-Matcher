"""Select local microchips. LibreSprite authors the actual exact pixel edits.
Explicit material regions; only existing adjacent colors. No image filtering.
"""
from pathlib import Path
from collections import Counter
import json,hashlib
from PIL import Image,ImageDraw
HERE=Path(__file__).resolve().parent
im=Image.open(HERE/'before/HallTall.png').convert('RGBA');W,H=im.size
p=list(im.get_flattened_data())
regions=[
 ('upper stone left',[(1,5),(31,5),(30,36),(23,47),(22,88),(7,95),(1,85)]),
 ('upper stone right',[(332,3),(359,3),(359,132),(344,132),(337,112),(335,87),(335,44)]),
 ('wall below left banner',[(43,214),(82,214),(83,251),(44,251)]),
 ('wall below right banner',[(279,212),(314,212),(314,247),(283,247)]),
 ('left arch masonry',[(108,144),(128,140),(122,188),(118,217),(114,241),(113,275),(106,284),(103,270),(110,248),(110,218),(105,202)]),
 ('right arch masonry',[(233,141),(247,144),(251,202),(247,218),(250,248),(256,275),(247,284),(241,258),(241,217),(237,187)]),
 ('left stair planes',[(25,331),(137,331),(134,358),(126,382),(120,420),(114,443),(13,443),(13,418),(31,403),(31,373),(19,367)]),
 ('right stair planes',[(226,332),(335,332),(340,369),(326,379),(329,406),(347,420),(346,443),(246,443),(241,422),(239,398),(232,376)]),
 ('left paving',[(13,447),(112,447),(95,525),(84,566),(65,590),(39,590),(40,553),(19,534),(1,522),(1,471)]),
 ('right paving',[(248,447),(346,447),(359,472),(359,521),(339,536),(322,553),(318,590),(291,590),(275,570),(263,525)]),
 ('carpet interior',[(151,363),(211,363),(217,419),(247,553),(250,594),(111,594),(115,552),(141,419)]),
 ('chest wood',[(36,265),(65,265),(65,277),(37,277)]),
 ('chest lower wood',[(37,289),(66,289),(66,304),(37,304)]),
 ('crate wood',[(268,294),(284,294),(284,311),(268,311)]),
 ('barrel wood',[(304,274),(318,274),(318,286),(304,286)]),
]
stone={'1f123c','2b1c4d','3e2462','593479','6d48a2'}
carpet={'25063d','31084e','3f0a67'}
wood={'271235','3b1a35','5f2b43','854d45','772f49'}
hx=lambda c:''.join(f'{v:02x}' for v in c[:3])
groups={}
for name,poly in regions:
 m=Image.new('1',(W,H));ImageDraw.Draw(m).polygon(poly,fill=1)
 group=carpet if 'carpet' in name else wood if 'wood' in name else stone
 for i,v in enumerate(m.get_flattened_data()):
  if v:groups[i]=(name,group)
# Whole-source components prevent mask edges being misread as tiny fragments.
component=[-1]*len(p);components=[]
for i in range(len(p)):
 if component[i]>=0:continue
 k=len(components);component[i]=k;queue=[i];members=[]
 while queue:
  q=queue.pop();members.append(q);x=q%W;y=q//W
  for t in ([q-1] if x else [])+([q+1] if x<W-1 else [])+([q-W] if y else [])+([q+W] if y<H-1 else []):
   if component[t]<0 and p[t]==p[i]:component[t]=k;queue.append(t)
 components.append(members)
edits=[];counts=Counter();islands=Counter()
for members in components:
 n=len(members);first=members[0]
 if n>14 or first not in groups:continue
 name,group=groups[first];old=p[first]
 if hx(old) not in group:continue
 if any(q not in groups or groups[q][0]!=name for q in members):continue
 xs=[q%W for q in members];ys=[q//W for q in members]
 if max(xs)-min(xs)>7 or max(ys)-min(ys)>7:continue
 surround=set();safe=True
 for q in members:
  x=q%W;y=q//W
  for dy in range(-1,2):
   for dx in range(-1,2):
    t=q+dy*W+dx
    if not(0<=x+dx<W and 0<=y+dy<H) or t not in groups or groups[t][0]!=name or hx(p[t]) not in group:
     safe=False;break
    if p[t]!=old:surround.add(t)
   if not safe:break
  if not safe:break
 if not safe or not surround:continue
 choices=Counter(p[t] for t in surround if len(components[component[t]])>=max(24,n*4))
 if not choices:continue
 new,votes=choices.most_common(1)[0]
 if votes/len(surround)<.6:continue
 if max(abs(old[k]-new[k]) for k in range(3))>39:continue
 for q in members:edits.append([q%W,q//W,*old[:3],*new[:3]])
 counts[name]+=n;islands[name]+=1
manifest={'source_sha256':hashlib.sha256((HERE/'before/HallTall.png').read_bytes()).hexdigest(),
 'size':[W,H],'regions':[{'name':n,'polygon':poly} for n,poly in regions],
 'changed_pixels':len(edits),'changed_percent':round(len(edits)*100/(W*H),4),
 'by_region':dict(counts),'components_by_region':dict(islands),'pixels':edits}
header=json.dumps({k:v for k,v in manifest.items() if k!='pixels'},indent=2)
(HERE/'cleanup-plan.json').write_text(header[:-2]+',\n  "pixels": [\n    '+',\n    '.join(json.dumps(e) for e in edits)+'\n  ]\n}\n')
print(json.dumps({k:v for k,v in manifest.items() if k not in ['pixels','regions']},indent=2))
overlay=im.copy();d=ImageDraw.Draw(overlay)
for x,y,*rgb in edits:d.point((x,y),fill='#00ffb0')
overlay.resize((720,1600),Image.Resampling.NEAREST).save(HERE/'review/EditMap.png')
js="""// Exact native cel edit in LibreSprite; no resampling or filters.
var ROOT='__ROOT__',edits=__EDITS__;
function clean(name,height){
 app.open(ROOT+'before/'+name+'.aseprite');
 var s=app.activeSprite;
 if(s.width!==360||s.height!==height)throw 'Unexpected source dimensions';
 var c=s.layer(0).cel(0),p=c.image.getImageData();
 if(c.x!==0||c.y!==0||c.image.width!==360||c.image.height!==height)throw 'Unexpected cel registration';
 for(var n=0;n<edits.length;n++){
  var e=edits[n];if(e[1]>=height)continue;var i=(e[1]*360+e[0])*4;
  for(var k=0;k<3;k++){if(p[i+k]!==e[2+k])throw 'Original pixel mismatch at '+e[0]+','+e[1];}
  for(var k=0;k<3;k++)p[i+k]=e[5+k];
 }
 c.image.putImageData(p);s.commit();s.saveAs(ROOT+'native/'+name+'.aseprite',false);
 app.activeDocument.close();app.open(ROOT+'native/'+name+'.aseprite');
 app.activeSprite.saveAs(ROOT+'native/'+name+'.png',false);app.activeDocument.close();
}
clean('HallTall',800);clean('Hall',640);app.command.Exit();
""".replace('__ROOT__',HERE.as_posix()+'/').replace('__EDITS__',json.dumps(edits,separators=(',',':')))
(HERE/'logs/ApplyCleanup.js').write_text(js)
