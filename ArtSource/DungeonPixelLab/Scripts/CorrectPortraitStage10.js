// Stage09: direct native LibreSprite cleanup of new PixelLab environmental props. No scaling.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage10/Frame_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage10/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage10/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE10_SAVED "+name);}

function copy(src,sx,sy,w,h){var a=blank(w,h);for(var y=0;y<h;y++)for(var x=0;x<w;x++)if(src.p[((sy+y)*src.w+sx+x)*4+3])pixel(a,x,y,hex(src,sx+x,sy+y));return a;}
var old=R+'Diagnostics/Stage10Correction/BeforeExports/';
var corner=read(old+'Portrait_Corner_TL.png'),diamond=blank(13,13);
for(var y=0;y<13;y++)for(var x=0;x<13;x++)if(Math.abs(x-6)+Math.abs(y-6)<=6)pixel(diamond,x,y,hex(corner,x+1,y+1));
['TL','TR','BL','BR'].forEach(function(s){save(diamond,'Portrait_Corner_'+s);});
var top=copy(read(old+'Portrait_Rail_Top.png'),0,0,24,7);
save(top,'Portrait_Rail_Top');save(top,'Portrait_Rail_Bottom');
var side=copy(read(old+'Portrait_Rail_Left.png'),1,0,7,20);
save(side,'Portrait_Rail_Left');save(side,'Portrait_Rail_Right');
save(copy(read(old+'Portrait_Clamp.png'),0,0,7,7),'Portrait_Clamp');
console.log('STAGE10_COMPLETE Corrected symmetric native pieces; HP and heart untouched');
