// Stage12: direct native LibreSprite cleanup of new PixelLab environmental props. No scaling.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage12/Plaque_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage12/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage12/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE12_SAVED "+name);}


var raw=read(R+'Sources/Stage12/Plaque_Raw_PixelLab.png');
if(raw.w!==64||raw.h!==64)throw new Error('Unexpected source canvas');
// Select native rows/columns, removing redundant shade runs. No resampling.
// The original rivet at (9,20) becomes the exact center (4,4).
var xs=[4,5,6,8,9,10,11,12,13],ys=[16,17,18,19,20,21,22,23,25],corner=blank(9,9);
for(var y=0;y<9;y++)for(var x=0;x<9;x++){
 if((x===0||x===8)&&(y===0||y===8))continue;
 var c=hex(raw,xs[x],ys[y]);
 if(x===0||y===0||x===8||y===8)c='0a0d11';
 else if(x===7||y===7)c='615b6b';
 // Calm the bright rim; keep one small upper-left specular accent.
 if(c==='e2dcea')c='ccc3d5';
 if(x===2&&y===2)c='e2dcea';
 if(x===4&&y===4)c='0a0d11';
 pixel(corner,x,y,c);
}
['TL','TR','BL','BR'].forEach(function(n){save(corner,'Plaque_Corner_'+n);});
// Source lower rail has clean outline/highlight/base/purple-shadow planes.
// Replace only the highlight/shadow material colors to match cool metal.
var rail=blank(16,5),side=blank(5,16),profile=[];
for(var i=0;i<5;i++){var c=hex(raw,18,40+i);if(c==='e2dcea')c='ccc3d5';if(c==='846a95')c='615b6b';profile.push(c);}
for(var i=0;i<5;i++)for(var k=0;k<16;k++){pixel(rail,k,i,profile[i]);pixel(side,i,k,profile[i]);}
save(rail,'Plaque_Rail_Top');save(rail,'Plaque_Rail_Bottom');save(side,'Plaque_Rail_Left');save(side,'Plaque_Rail_Right');
// Small source-derived violet socket, centered on the rail axis.
var clamp=blank(7,7);
for(var y=0;y<7;y++)for(var x=0;x<7;x++){
 if((x===0||x===6)&&(y===0||y===6))continue;
 var c=x===0||x===6||y===0||y===6?'0a0d11':x===1||y===1?'ccc3d5':x===5||y===5?'615b6b':'958ba1';
 if(x>=2&&x<=4&&y>=2&&y<=4)c='68168a';
 if((x===2&&y<=3)||(y===2&&x<=3))c='c22bea';
 pixel(clamp,x,y,c);
}
save(clamp,'Plaque_Clamp');
var fill=blank(16,16);for(var y=0;y<16;y++)for(var x=0;x<16;x++)pixel(fill,x,y,'151423');save(fill,'Plaque_Inset');
// Separately authored native gear; same source metal ramp, no baked text.
var mask=['......###......','......###......','..###.###.###..','..###########..','..###########..','...#########...','###############','###############','###############','...#########...','..###########..','..###########..','..###.###.###..','......###......','......###......'];
function solid(x,y){return x>=0&&x<15&&y>=0&&y<15&&mask[y][x]==='#'&&!(x>=6&&x<=8&&y>=6&&y<=8);}
var gear=blank(15,15);
for(var y=0;y<15;y++)for(var x=0;x<15;x++)if(solid(x,y)){
 var edge=!solid(x-1,y)||!solid(x+1,y)||!solid(x,y-1)||!solid(x,y+1);
 var c=edge?'0a0d11':x+y<13?'ccc3d5':x+y<20?'958ba1':'615b6b';
 if((x===7&&y===1)||(x===3&&y===3))c='e2dcea';pixel(gear,x,y,c);
}
save(gear,'Plaque_Gear');
console.log('STAGE12_COMPLETE 11 native modular plaque components');
