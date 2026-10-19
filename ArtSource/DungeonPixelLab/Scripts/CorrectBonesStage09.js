// Stage09: direct native LibreSprite cleanup of new PixelLab environmental props. No scaling.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage09/Barrel_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage09/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage09/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE09_SAVED "+name);}
var bones=read(R+"Sources/Stage09/Bones_Raw_PixelLab.png");
if(bones.w!==64||bones.h!==64)throw new Error("Unexpected bone source grid");
var skull=blank(24,22),loose=blank(26,12);
for(var y=38;y<50;y++)for(var x=30;x<55;x++)if(bones.p[(y*64+x)*4+3]){
 var c=hex(bones,x,y);
 if(c!=="0a0d11")c=y<=44?"ccc3b2":y<=46?"a39796":"766879";
 pixel(loose,x-29,y-38,c);
}
save(loose,"Clutter_Bones");
console.log("STAGE09_COMPLETE raw bone alpha restored");
