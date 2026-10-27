from pathlib import Path
from PIL import Image
import shutil,json
F=Path('C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage03'
shutil.copyfile(R/'Raw/FrameKit-1.png',R/'Prepared/SelectedFrame.png')
sizes={**{f'BoardCorner{s}':(27,27) for s in ['TL','TR','BL','BR']},'BoardRailTop':(25,9),'BoardRailBottom':(25,9),'BoardRailLeft':(9,25),'BoardRailRight':(9,25),'BoardJoint':(9,9),'AmberInset':(9,9)}
for name,size in sizes.items(): Image.new('RGBA',size).save(R/'Prepared'/f'{name}.png')
for suffix in ['TL','TR','BL','BR']: shutil.copyfile(F/f'ArtSource/IronveinStaged/Exports/BoardCorner{suffix}.png',R/f'Prepared/ProtectedCorner{suffix}.png')
js=r'''
var root=__ROOT__,pc=app.pixelColor,src=[];
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('000000'+((pc.rgbaR(v)<<16)|(pc.rgbaG(v)<<8)|pc.rgbaB(v)).toString(16)).slice(-6);}
try{
app.open(root+'Prepared/SelectedFrame.png');var cel=app.activeSprite.layer(0).cel(0),raw=cel.image;
for(var y=0;y<96;y++){src[y]=[];for(var x=0;x<96;x++)src[y][x]=(x>=cel.x&&y>=cel.y&&x<cel.x+raw.width&&y<cel.y+raw.height)?raw.getPixel(x-cel.x,y-cel.y):0;}app.activeDocument.close();
__NAMES__.forEach(function(name){
 var protectedPixels=[];
 if(name.indexOf('BoardCorner')===0){
   app.open(root+'Prepared/ProtectedCorner'+name.slice(-2)+'.png');var old=app.activeSprite.layer(0).cel(0),oi=old.image;
   for(var yy=0;yy<27;yy++){protectedPixels[yy]=[];for(var xx=0;xx<27;xx++)protectedPixels[yy][xx]=(xx>=old.x&&yy>=old.y&&xx<old.x+oi.width&&yy<old.y+oi.height)?oi.getPixel(xx-old.x,yy-old.y):0;}app.activeDocument.close();
 }
 app.open(root+'Prepared/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
 function px(x,y,h){if(x>=0&&y>=0&&x<s.width&&y<s.height)im.putPixel(x,y,typeof h==='string'?rgb(h):h);}
 function box(a,b,c,d,h){for(var yy=b;yy<=d;yy++)for(var xx=a;xx<=c;xx++)px(xx,yy,h);}
 function bolt(x,y){box(x-1,y-1,x+1,y+1,'0a0d11');px(x,y-1,'93a1b0');px(x,y,'c58826');px(x+1,y,'8a5622');}
 if(name.indexOf('BoardCorner')===0){
   // Rebuild the generated chamfered socket at the existing 27px footprint.
   // Keep material lighting upright on all four corners, never mirror the highlights.
   for(var y=2;y<=24;y++)for(var x=2;x<=24;x++){
     var cut=Math.max(0,6-x)+Math.max(0,6-y)+Math.max(0,x-20)+Math.max(0,y-20);
     if(cut>5)continue;
     var edge=x===2||y===2||x===24||y===24||cut===5;
     px(x,y,edge?'0a0d11':(y===3||x===3||cut===4&&x+y<26?'786477':y>=22||x>=23?'2b313a':'42374f'));
   }
   // Short integrated timber tenons continue into the two relevant rail directions.
   var right=name==='BoardCornerTL'||name==='BoardCornerBL',down=name==='BoardCornerTL'||name==='BoardCornerTR';
   for(var k=0;k<3;k++)for(var t=9;t<=17;t++){
     px(right?24+k:2-k,t,t===9||t===17?'0a0d11':t===10?'b07a43':t>=15?'4a2c1c':'7a4d2e');
     px(t,down?24+k:2-k,t===9||t===17?'0a0d11':t===10?'b07a43':t>=15?'4a2c1c':'7a4d2e');
   }
   // Clean closed iron bezel; retain the generated crystal's native facets.
   for(var y=4;y<=22;y++)for(var x=4;x<=22;x++){
     var d=Math.abs(x-13)+Math.abs(y-13);if(d>9)continue;
     if(d===9){px(x,y,'0a0d11');continue;}
     if(d===8){px(x,y,x+y<=26?'93a1b0':'2b313a');continue;}
     var v=src[y+11][x+11],h=hex(v);
     if(['4a2c1c','7a4d2e','30202a'].indexOf(h)>=0)h='c94d08';
     if(d===7)h='0a0d11';
     px(x,y,h);
   }
   bolt(6,6);bolt(20,6);bolt(6,20);bolt(20,20);
   // Keep the original native inner overlap beneath the unchanged rounded board
   // cells. Their transparent edge pixels must not expose a new frame color.
   for(var y=0;y<27;y++)for(var x=0;x<27;x++){
     var innerX=right?x>=18:x<=9,innerY=down?y>=18:y<=9;
     if(innerX&&innerY)px(x,y,protectedPixels[y][x]);
   }
 }
 if(name.indexOf('BoardRail')===0){
   var horizontal=s.width>9;
   // Quiet 9px rails use the selected source's wood planes, with native redrawn seams.
   var ramp=['0a0d11','b07a43','7a4d2e','7a4d2e','7a4d2e','4a2c1c','4a2c1c','30202a','0a0d11'];
   for(var y=0;y<s.height;y++)for(var x=0;x<s.width;x++)px(x,y,ramp[horizontal?y:x]);
   if(horizontal){for(var x=5;x<12;x++)px(x,5,'30202a');for(var x=15;x<21;x++)px(x,3,'b07a43');}
   else{for(var y=5;y<12;y++)px(5,y,'30202a');for(var y=15;y<21;y++)px(3,y,'b07a43');}
 }
 if(name==='BoardJoint'||name==='AmberInset'){
   box(0,0,8,8,'0a0d11');box(1,1,7,7,'42374f');box(1,1,7,1,'786477');box(1,2,1,6,'55606e');box(2,7,7,7,'2b313a');
   if(name==='BoardJoint'){bolt(4,4);px(7,2,'55606e');}
   else for(var y=2;y<=6;y++)for(var x=2;x<=6;x++){
     var d=Math.abs(x-4)+Math.abs(y-4);if(d<=3)px(x,y,d===3?'0a0d11':x+y<8?'ffdb8f':x+y===8?'f5923d':'c94d08');
   }
 }
 s.layer(0).name='PixelLab candidate 1; native modular cleanup';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
 app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('STAGE03_CLEAN '+name);
});console.log('STAGE03_CLEANUP_PASS');}catch(e){console.log('STAGE03_CLEANUP_FAIL '+e);}app.command.Exit();
'''.replace('__ROOT__',json.dumps(str(R).replace('\\','/')+'/')).replace('__NAMES__',json.dumps(list(sizes)))
(R/'Scripts/Cleanup.js').write_text(js)
runner=(F/'ArtSource/IronveinBelowTopHUD/Stage02/Scripts/RunLibreSprite.ps1').read_text().replace('-Tail 7','-Tail 12')
(R/'Scripts/RunLibreSprite.ps1').write_text(runner)
shutil.copyfile(__file__,R/'Scripts/PrepareNative.py')
print('Prepared',len(sizes),'native documents; pixel edits execute in LibreSprite')
