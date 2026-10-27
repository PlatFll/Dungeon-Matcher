from pathlib import Path
from PIL import Image
import json,shutil
F=Path('C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage04'
shutil.copyfile(R/'Raw/ProDrill-17.png',R/'Prepared/SelectedDrill.png')
sizes={'MountLeft':(38,36),'MountRight':(38,36),'MotorLeft':(18,27),'MotorRight':(18,27),'NoseRight':(16,17),'NoseLeft':(16,17),'PowerBlock':(9,9)}
for n,size in sizes.items():Image.new('RGBA',size).save(R/'Prepared'/(n+'.png'))
js=r'''
var root=__ROOT__,pc=app.pixelColor,src=[];
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('000000'+((pc.rgbaR(v)<<16)|(pc.rgbaG(v)<<8)|pc.rgbaB(v)).toString(16)).slice(-6);}
try{
app.open(root+'Prepared/SelectedDrill.png');var rawCel=app.activeSprite.layer(0).cel(0),raw=rawCel.image;
for(var y=0;y<32;y++){src[y]=[];for(var x=0;x<32;x++)src[y][x]=(x>=rawCel.x&&y>=rawCel.y&&x<rawCel.x+raw.width&&y<rawCel.y+raw.height)?raw.getPixel(x-rawCel.x,y-rawCel.y):0;}app.activeDocument.close();
__NAMES__.forEach(function(name){
 app.open(root+'Prepared/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
 function px(x,y,h){if(x>=0&&y>=0&&x<s.width&&y<s.height)im.putPixel(x,y,typeof h==='string'?rgb(h):h);}
 function box(a,b,c,d,h){for(var yy=b;yy<=d;yy++)for(var xx=a;xx<=c;xx++)px(xx,yy,h);}
 function bolt(x,y){box(x-1,y-1,x+1,y+1,'0a0d11');px(x,y-1,'93a1b0');px(x,y,'c58826');px(x+1,y,'8a5622');}
 if(name.indexOf('Mount')===0){
   var mirrored=name==='MountRight';
   function shape(x,y){if(mirrored)x=37-x;return (x>=0&&x<=25&&y>=1&&y<=34&&!(x<2&&(y<3||y>32)))||(x>=22&&x<=37&&((y>=2&&y<=6)||(y>=29&&y<=33)))||(x>=29&&x<=37&&y>=2&&y<=33);}
   for(var y=0;y<36;y++)for(var x=0;x<38;x++)if(shape(x,y)){
     var edge=!shape(x-1,y)||!shape(x+1,y)||!shape(x,y-1)||!shape(x,y+1);
     var upper=!shape(x,y-2),left=!shape(x-2,y),lower=!shape(x,y+2),right=!shape(x+2,y);
     px(x,y,edge?'0a0d11':upper?'786477':left?'55606e':lower||right?'2b313a':'42374f');
   }
   [4,33].forEach(function(x){bolt(x,4);bolt(x,31);});
   // Two restrained inset seams imply a forged backplate, without surface noise.
   var edge=mirrored?33:4;for(var y=10;y<=25;y++)px(edge,y,'2b313a');
 }
 if(name.indexOf('Motor')===0){
   var mirrored=name==='MotorRight';
   for(var y=0;y<27;y++)for(var x=0;x<18;x++){
     var v=src[y+3][x+5];if(!pc.rgbaA(v))continue;
     var h=hex(v);if(h==='4a2c1c'||h==='7a4d2e')h='2b313a';
     // The old tiny center glow is replaced by its separate, clean power block.
     if(x>=7&&x<=12&&y>=10&&y<=17)h='2b313a';
     px(mirrored?17-x:x,y,h);
   }
   // Finish the mating contour and put the lit rim on the world upper-left.
   var collar=mirrored?0:17;for(var y=7;y<=19;y++)px(collar,y,'0a0d11');
   bolt(5,8);bolt(14,8);bolt(5,20);bolt(14,20);
 }
 if(name.indexOf('Nose')===0){
   var mirrored=name==='NoseLeft';var values=[];
   for(var y=0;y<17;y++){values[y]=[];for(var x=0;x<16;x++)values[y][x]=0;}
   for(var y=2;y<=14;y++)for(var x=0;x<=10;x++){
     var v=src[y+8][x+21];if(pc.rgbaA(v))values[y][x]=v;
   }
   // The raw sheet cuts the last few tip pixels at its edge. Reconstruct a
   // closed tapered contour at native resolution on the larger isolated canvas.
   for(var x=11;x<=13;x++){
     var half=13-x;for(var y=8-half-1;y<=8+half+1;y++)values[y][x]=rgb(y===8-half-1||y===8+half+1?'0a0d11':y<=8?'93a1b0':'55606e');
   }
   values[8][14]=rgb('0a0d11');
   for(var y=0;y<17;y++)for(var x=0;x<16;x++)if(values[y][x])px(mirrored?15-x:x,y,values[y][x]);
 }
 if(name==='PowerBlock'){
   box(0,0,8,8,'0a0d11');box(1,1,7,7,'42374f');box(1,1,7,1,'786477');box(1,2,1,6,'55606e');box(2,7,7,7,'2b313a');
   for(var y=2;y<=6;y++)for(var x=2;x<=6;x++){var d=Math.abs(x-4)+Math.abs(y-4);if(d<=2)px(x,y,x+y<8?'ffdb8f':x+y===8?'f5923d':'c94d08');}
 }
 s.layer(0).name='PixelLab Pro 17; separated native drill module';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
 app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('STAGE04_CLEAN '+name);
});console.log('STAGE04_CLEANUP_PASS');}catch(e){console.log('STAGE04_CLEANUP_FAIL '+e);}app.command.Exit();
'''.replace('__ROOT__',json.dumps(str(R).replace('\\','/')+'/')).replace('__NAMES__',json.dumps(list(sizes)))
(R/'Scripts/Cleanup.js').write_text(js);shutil.copyfile(F/'ArtSource/IronveinBelowTopHUD/Stage03/Scripts/RunLibreSprite.ps1',R/'Scripts/RunLibreSprite.ps1');shutil.copyfile(__file__,R/'Scripts/PrepareNative.py')
receipts=json.loads(Path(__file__).with_name('stage04-receipts.json').read_text());receipts['requests'][-1].update({'status':'selected candidate 17; other candidates retained as alternatives','selection':17});receipts['cleanup']='Split the generated motor and spiral bit into native pieces; discard wraparound specks, complete truncated tip with native pixels, simplify small material noise, add separate mechanical mounts and amber inset, retain top-left light on both assemblies.'
(R/'Generation.json').write_text(json.dumps(receipts,indent=2));print('Prepared 7 native modules')
