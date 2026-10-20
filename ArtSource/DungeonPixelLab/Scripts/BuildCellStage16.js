// Stage 16: user-requested 64px tile expansion in LibreSprite.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Exports/Stage11/Board_Cell.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage16/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage16/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE16_SAVED "+name);}
// User-authorized native 64x64 variant. Repeat the flat interior; move the
// existing right/bottom bevel to the expanded edge. No palette change or resampling.
var src=read(R+'Exports/Stage11/Board_Cell.png');
if(src.w!==35||src.h!==35)throw new Error('Unexpected approved tile dimensions');
var dst=blank(64,64);
for(var y=0;y<64;y++)for(var x=0;x<64;x++){
 var sx=x<3?x:x>=61?x-29:Math.min(x,30);
 var sy=y<3?y:y>=57?y-29:Math.min(y,27);
 for(var k=0;k<4;k++)dst.p[(y*64+x)*4+k]=src.p[(sy*35+sx)*4+k];
}
save(dst,'Board_Cell64');
console.log('STAGE16_COMPLETE 64x64 native tile');
