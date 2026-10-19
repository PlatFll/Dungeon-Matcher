// Portrait correction: center the original green-reference pipe profile. No resampling.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage10/Frame_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage10/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage10/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE10_SAVED "+name);}

// Use the actual five-pixel pipe profile from the user's green-highlighted
// ORIGINAL left rail. Center that visible profile, not the asymmetric backing.
var green=read(R+'Diagnostics/Stage10Correction/BeforeExports/Portrait_Rail_Left.png');
var vertical=blank(7,20),horizontal=blank(24,7);
for(var i=0;i<5;i++){
 var c=hex(green,i+1,5);
 for(var y=0;y<20;y++)pixel(vertical,i+1,y,c);
 for(var x=0;x<24;x++)pixel(horizontal,x,i+1,c);
}
save(vertical,'Portrait_Rail_Left');save(vertical,'Portrait_Rail_Right');
save(horizontal,'Portrait_Rail_Top');save(horizontal,'Portrait_Rail_Bottom');
console.log('STAGE10_COMPLETE Four visible pipes centered exactly within existing 7px clamps');
