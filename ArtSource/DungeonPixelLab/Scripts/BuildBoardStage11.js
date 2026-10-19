// Stage11: direct native LibreSprite cleanup of new PixelLab environmental props. No scaling.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage11/Board_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage11/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage11/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE11_SAVED "+name);}

var raw=read(R+'Sources/Stage11/Board_Raw_PixelLab.png');
if(raw.w!==64||raw.h!==64)throw new Error('Unexpected source dimensions');
// Retain the generated quiet stone profile and seam vocabulary. Redraw the
// duplicate flat shade rows into nine native pixels, never resample the image.
var rows=[10,11,13,14,15,16,18,19,20],rail=blank(32,9),side=blank(9,32);
for(var x=0;x<32;x++)for(var y=0;y<9;y++){
 var c=hex(raw,28,rows[y]);
 if(x===16)c='0a0d11';else if(x===17&&y>1&&y<7)c='846a95';
 pixel(rail,x,y,c);pixel(side,y,x,c);
}
save(rail,'Board_Rail_Top');save(rail,'Board_Rail_Bottom');
save(side,'Board_Rail_Left');save(side,'Board_Rail_Right');
// Regular native diamond socket around the source's jewel color clusters.
// Chunky stone bevel is restrained; the tiny light sits on its upper-left facet.
var corner=blank(17,17);
for(var y=0;y<17;y++)for(var x=0;x<17;x++){
 var dx=x-8,dy=y-8,d=Math.abs(dx)+Math.abs(dy);if(d>8)continue;
 var c=d===8?'0a0d11':d===7?(dx+dy<0?'aba0b8':'382d55'):d===6?(dx+dy<0?'846a95':'4e3d6c'):d===5?'241f3a':d===4?'421a5a':dx+dy<0?'f078ff':dx+dy<2?'c22bea':'68168a';
 if(dx===-1&&dy===-2)c='ddd5e3';pixel(corner,x,y,c);
}
['TL','TR','BL','BR'].forEach(function(n){save(corner,'Board_Corner_'+n);});
var clamp=blank(9,9);
for(var y=0;y<9;y++)for(var x=0;x<9;x++){
 var c=x===0||y===0||x===8||y===8?'0a0d11':y===1||x===1?'846a95':y===7||x===7?'382d55':'4e3d6c';
 if(x===2&&y===2)c='aba0b8';pixel(clamp,x,y,c);
}
save(clamp,'Board_Clamp');
// The separately authored quiet cell uses the established 35px mechanical pitch.
// No cracks, texture noise or baked gameplay pieces.
var cell=blank(35,35);
for(var y=0;y<35;y++)for(var x=0;x<35;x++){
 var c=x===0||y===0||x===34||y===34?'0a0d11':x===1||y===1?'344057':x===33||y===33?'111525':y>=28?'192033':'1c2438';
 if((x===1&&y>=32)||(y===1&&x>=32))c='253149';pixel(cell,x,y,c);
}
save(cell,'Board_Cell');
console.log('STAGE11_COMPLETE 10 modular board components with 35px native cell');
