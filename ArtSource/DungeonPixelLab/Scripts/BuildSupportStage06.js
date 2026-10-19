// Stage06: native LibreSprite edits of the new PixelLab pillar; never resample artwork.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage06/Pillar_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage06/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage06/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE06_SAVED "+name);}
var raw=read(R+"Sources/Stage06/Pillar_Raw_PixelLab.png");if(raw.w!==64||raw.h!==96)throw new Error("Wrong source grid");
var ramp={"151423":"151423","241f3a":"302940","382d55":"382d55","67537d":"4e3d6c"};
function source(a,sx,sy,dx,dy){if(raw.p[(sy*raw.w+sx)*4+3]){var col=ramp[hex(raw,sx,sy)];if(!col)throw new Error("Unexpected source color");pixel(a,dx,dy,col);}}
// Preserve the generated native capital and foot profiles; simplify scattered side shades.
var capital=blank(28,16),core=blank(28,28),foot=blank(28,24);
for(var y=0;y<14;y++)for(var x=0;x<28;x++)source(capital,x+19,y+11,x,y+2);
for(var y=0;y<24;y++)for(var x=0;x<28;x++)source(foot,x+19,y<8?69:y+61,x,y);
// Source shaft is 16px wide at x24..39, with 14px stone courses. Keep its native pitch.
for(var y=0;y<28;y++)for(var x=5;x<21;x++)source(core,x+19,36+y%14,x,y);
function cleanPlanes(a){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(a.p[(y*a.w+x)*4+3]){
 var c=hex(a,x,y);if(c==="302940")pixel(a,x,y,"151423");
}}
cleanPlanes(capital);cleanPlanes(core);cleanPlanes(foot);
// Continuous top-left edge and broad matte faces, never a highlight around all four sides.
for(var y=0;y<28;y++){var r=y%14;for(var x=5;x<21;x++)pixel(core,x,y,r===0?"151423":x===5||x===20?"151423":r===13?"382d55":x>=17?"382d55":r===1?"67537d":"4e3d6c");}
for(var y=2;y<13;y++)pixel(core,6,y,"67537d");
for(var y=16;y<27;y++)pixel(core,6,y,"67537d");
for(var x=5;x<20;x++)if(capital.p[(3*28+x)*4+3])pixel(capital,x,3,"67537d");
for(var x=5;x<19;x++)if(capital.p[(10*28+x)*4+3])pixel(capital,x,10,"67537d");
// Eight authored neck rows preserve the source's complete foot while reaching the established y180 floor.
for(var y=0;y<8;y++)for(var x=5;x<21;x++)pixel(foot,x,y,x===5||x===20?"151423":x===6?"67537d":x>=17?"382d55":"4e3d6c");
for(var x=4;x<20;x++)pixel(foot,x,12,"67537d");
for(var x=2;x<23;x++)pixel(foot,x,18,"67537d");
// Small under-ledge corbel: source cap profile plus two quieter stepped support stones.
var corbel=blank(28,20);
for(var y=0;y<7;y++)for(var x=0;x<28;x++)source(corbel,x+19,y+11,x,y);
for(var y=7;y<20;y++){
 var l=y<12?4:y<17?6:8,r=28-l;
 for(var x=l;x<r;x++)pixel(corbel,x,y,y===7||y===12||y===17||y===19||x===l||x===r-1?"151423":x>=r-4?"241f3a":y===8||y===13||y===18?"4e3d6c":"382d55");
}
// Corner returns repeat all the way down the side gutters. Same source 14px course pitch,
// authored wider faces and staggered inner edges; no mirroring of top-left illumination.
function gutter(right){var a=blank(28,28);for(var y=0;y<28;y++){
 var row=y%14,reach=y<14?24:27,l=right?28-reach:0,r=right?28:reach;
 for(var x=l;x<r;x++){
  var rel=x-l,c=row===0?"151423":row===13?"241f3a":rel===0||x===r-1?"241f3a":x>=r-4?"302940":row===1?"4e3d6c":"382d55";
  pixel(a,x,y,c);
 }
 // Sparse left-plane highlights, not texture speckle.
 if(row>=2&&row<7)pixel(a,l+1,y,"4e3d6c");
 }return a;}
var end=blank(28,8);for(var y=0;y<8;y++)for(var x=0;x<28;x++)pixel(end,x,y,y===0||y===7?"151423":y===1?"4e3d6c":x>=24?"241f3a":"382d55");
save(capital,"Support_Capital");save(core,"Support_Core");save(foot,"Support_Foot");save(corbel,"Support_Corbel");save(gutter(false),"Support_Corner_Left");save(gutter(true),"Support_Corner_Right");save(end,"Support_Edge_Foot");
console.log("STAGE06_COMPLETE 7 native structural modules");
