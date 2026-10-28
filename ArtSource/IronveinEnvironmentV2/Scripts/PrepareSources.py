from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil
F=Path(__file__).resolve().parents[3];R=F/'ArtSource/IronveinEnvironmentV2';W=Path(__file__).parent
for d in ['Prepared','QA/Roundtrip']:(R/d).mkdir(parents=True,exist_ok=True)
jobs=[]
rock={'a5a1ad':'746f83','746f83':'626076','51536b':'49495f'}
wall={'12131f':'11121b','2b2c42':'181a26','3c3d55':'202232','51536b':'26283b','746f83':'2b2e42'}
def raw(n):return Image.open(R/'Raw'/f'{n}.png').convert('RGBA')
def add(n,im,source,m=None,polish='',seam=False):
 im.save(R/'Prepared'/f'{n}.png')
 jobs.append(dict(name=n,source=source,width=im.width,height=im.height,map=m or {},polish=polish,seam=seam))
def crop(n,b):return raw(n).crop(b)
def pad(im,w,h,x=1,y=1):
 a=Image.new('RGBA',(w,h));a.paste(im,(x,y));return a
add('CaveWall',raw('CavernWall'),'CavernWall',wall,'quiet backdrop / shared opposite edges',True)
add('CaveRecess',raw('CavernWall'),'CavernWall',{'12131f':'0a0d11','2b2c42':'10121b','3c3d55':'171a25','51536b':'1b1e2c','746f83':'202333'},'portrait and depth recess',True)
add('Portal',pad(crop('GreatPortal',(15,15,177,177)),164,164),'GreatPortal',rock,'isolated rock highlight cleanup')
add('DistantPortal',pad(crop('MinePortal',(21,24,105,105)),86,83),'MinePortal',{'a5a1ad':'3c3d55','746f83':'2b2c42','51536b':'24273b','3c3d55':'1d1e30','2b2c42':'171824','916344':'493039','694734':'2d2228'},'distant contrast grouping')
add('TimberPost',pad(crop('TimberPost',(5,8,28,119)),25,113),'TimberPost',rock,'metal highlight discipline')
add('Lantern',pad(crop('MineLantern',(4,4,20,40)),18,38),'MineLantern',{'dbaa68':'c18c56'},'small restrained material ramp')
add('Ore',pad(crop('AmberOre',(5,4,26,29)),23,27),'AmberOre',rock,'quiet rock base')
add('Platform',crop('RailPlatform',(3,6,93,28)),'RailPlatform',rock,'remove generation matte; flat repeat',True)
for n in ['LowerCartLeft','LowerToolsRight']:
 im=raw(n);im=im.crop(im.getbbox());add(n,pad(im,im.width+4,im.height+4,2,2),n,rock,'rock cluster cleanup; full silhouette padded')
add('Banner',raw('MiningBanner'),'MiningBanner',{'dbaa68':'c18c56','746f83':'626076'},'quiet cloth/metal accents')
side=raw('SideSupport')
add('SupportCap',side.crop((0,0,32,20)),'SideSupport',rock)
add('SupportBase',side.crop((0,108,32,128)),'SideSupport',rock)
# Straight shaft section: same source column under the decorative bands; selected height repeats by native crops.
shaft=Image.new('RGBA',(32,32))
for y in range(0,32,8):shaft.paste(side.crop((0,40,32,48)),(0,y))
add('SupportShaft',shaft,'SideSupport',rock,'unbroken native timber; no repeated capitals or metal bands')
add('SupportBrace',side.crop((0,30,32,40)),'SideSupport',rock,'separate sparse brace')
add('SupportOreJoint',side.crop((0,50,32,78)),'SideSupport',rock,'one attached ore motif per side')
board=raw('BoardFrameSource');corner=board.crop((6,5,31,30))
for suffix in ['TL','TR','BL','BR']:add('BoardCorner'+suffix,corner,'BoardFrameSource',rock,'upright identical plate; top-left light')
# Choose useful original rows, retaining 1:1 source pixels; no scaling or interpolation.
top=Image.new('RGBA',(25,9));rows=[9,10,11,14,15,16,17,20,21]
for y,sy in enumerate(rows):top.paste(board.crop((32,sy,57,sy+1)),(0,y))
add('BoardRailTop',top,'BoardFrameSource',{'a54120':'694734',**rock})
bottom=top.copy();add('BoardRailBottom',bottom,'BoardFrameSource',{'a54120':'694734',**rock})
left=Image.new('RGBA',(9,25));cols=[9,10,11,14,15,16,17,20,21]
for x,sx in enumerate(cols):left.paste(board.crop((sx,32,sx+1,57)),(x,0))
add('BoardRailLeft',left,'BoardFrameSource',{'a54120':'694734',**rock});add('BoardRailRight',left,'BoardFrameSource',{'a54120':'694734',**rock})
add('BoardJoint',board.crop((60,11,69,22)),'BoardFrameSource',rock,'centered joint')
portrait=raw('PortraitFrameSource');pc=pad(portrait.crop((0,0,9,9)),13,13,2,2)
for suffix in ['TL','TR','BL','BR']:add('PortraitCorner'+suffix,pc,'PortraitFrameSource',rock)
add('PortraitRailTop',portrait.crop((12,0,28,5)),'PortraitFrameSource',rock)
add('PortraitRailBottom',portrait.crop((12,121,28,126)),'PortraitFrameSource',rock)
add('PortraitRailLeft',portrait.crop((0,16,5,32)),'PortraitFrameSource',rock)
add('PortraitRailRight',portrait.crop((91,16,96,32)),'PortraitFrameSource',rock)
add('PortraitJoint',portrait.crop((0,0,9,9)),'PortraitFrameSource',rock)
# Native structural join/rail hints derived from the PixelLab timber and lamp source.
add('TimberBeam',crop('TimberPost',(9,38,18,70)).transpose(Image.Transpose.ROTATE_90),'TimberPost',{'dbaa68':'c18c56'},'native right-angle turn; relight top edge in editor')
add('Chain',crop('MineLantern',(9,4,15,15)),'MineLantern',rock,'repeat attached chain links')
add('InnerShadow',Image.new('RGBA',(2,2),(10,13,17,255)),'native connector',{},'gap-safe underlay')
add('Tracks',raw('DistantTracks'),'DistantTracks',{'916344':'694734','3c3d55':'2b2c42'},'distant rail hint behind actors')
for name,ox in [('WarmWallLeft',110),('WarmWallRight',312)]:
 im=Image.new('RGBA',(48,64));wallsource=raw('CavernWall')
 for y in range(64):
  for x in range(48):
   # Coarse native cluster boundary; underlying rock texture stays at the same repeat phase.
   qx=x//4*4+2;qy=y//4*4+2
   if ((qx-24)/24)**2+((qy-32)/32)**2<1:im.putpixel((x,y),wallsource.getpixel(((x+ox)%64,(y+80)%64)))
 add(name,im,'CavernWall',{'12131f':'17141e','2b2c42':'29202a','3c3d55':'40303a','51536b':'59403e','746f83':'75513e'},'localized native warm rock clusters')
(R/'Scripts/PreparationManifest.json').write_text(json.dumps(jobs,indent=2))
js='var root='+json.dumps(str(R).replace('\\','/')+'/')+',pc=app.pixelColor,jobs='+json.dumps(jobs)+';\n'
js+='''function color(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
try{jobs.forEach(function(j){app.open(root+'Prepared/'+j.name+'.png');var s=app.activeSprite,im=app.activeImage,map={};
for(var h in j.map)map[color(h)]=color(j.map[h]);
for(var y=0;y<s.height;y++)for(var x=0;x<s.width;x++){var c=im.getPixel(x,y);if(!pc.rgbaA(c)){im.putPixel(x,y,0);continue;}if(pc.rgbaA(c)!==255)throw Error('Nonbinary alpha '+j.name);if(map[c]!==undefined)im.putPixel(x,y,map[c]);}
// Delete only isolated interior rock highlight noise; the full silhouette is preserved.
if(j.name==='Portal'||j.name.indexOf('Lower')===0){var edits=[];for(var y=1;y<s.height-1;y++)for(var x=1;x<s.width-1;x++){var c=im.getPixel(x,y);if(c!==color('626076')&&c!==color('746f83')&&c!==color('49495f'))continue;var counts={},best=c,n=0,opaque=true;for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){if(!dx&&!dy)continue;var k=im.getPixel(x+dx,y+dy);if(!pc.rgbaA(k))opaque=false;counts[k]=(counts[k]||0)+1;if(counts[k]>n){best=k;n=counts[k];}}if(opaque&&n>=6&&best!==c)edits.push([x,y,best]);}edits.forEach(function(p){im.putPixel(p[0],p[1],p[2]);});}
if(j.seam){for(var y=0;y<s.height;y++)im.putPixel(s.width-1,y,im.getPixel(0,y));if(j.name!=='Platform')for(var x=0;x<s.width;x++)im.putPixel(x,s.height-1,im.getPixel(x,0));}
if(j.name==='TimberBeam')for(var x=0;x<s.width;x++){im.putPixel(x,0,color('0a0d11'));im.putPixel(x,1,color('916344'));im.putPixel(x,s.height-1,color('2d2228'));}
if(j.name==='Portal'){var warm={};warm[color('2b2c42')]=color('493039');warm[color('3c3d55')]=color('59403e');warm[color('49495f')]=color('75513e');warm[color('626076')]=color('916344');warm[color('746f83')]=color('a47b58');for(var y=58;y<108;y++)for(var x=0;x<s.width;x++){var depth=Math.min(x,s.width-1-x);if(depth<12-Math.floor(Math.abs(y-83)/3)){var c=im.getPixel(x,y);if(warm[c]!==undefined)im.putPixel(x,y,warm[c]);}}}
s.layer(0).name='Native material cleanup - '+j.polish;s.commit();s.saveAs(root+'Native/'+j.name+'.ase',true);s.saveAs(root+'Exports/'+j.name+'.png',true);app.activeDocument.close();console.log('IRONVEIN_CLEANED '+j.name);
});console.log('IRONVEIN_CLEANUP_COMPLETE');}catch(e){console.log('IRONVEIN_CLEANUP_FAILED '+e);}app.command.Exit();
'''
(R/'Scripts/NativeCleanup.js').write_text(js)
# Job receipts are preserved under Receipts/GenerationJobs.json.
print('Prepared',len(jobs),'native pieces for actual LibreSprite recolor/pixel cleanup')
