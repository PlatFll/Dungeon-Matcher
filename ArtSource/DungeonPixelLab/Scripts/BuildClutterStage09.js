// Stage09: direct native LibreSprite cleanup of new PixelLab environmental props. No scaling.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage09/Barrel_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage09/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage09/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE09_SAVED "+name);}
var raw=read(R+"Sources/Stage09/Barrel_Raw_PixelLab.png");
if(raw.w!==64||raw.h!==64)throw new Error("Unexpected barrel source grid");
var barrel=blank(36,42);
// Preserve the native silhouette and wood planes; crop and pad without scaling.
for(var y=18;y<60;y++)for(var x=14;x<50;x++)if(raw.p[(y*64+x)*4+3]){
 var c=hex(raw,x,y);
 // Iron was a broad flat bright stripe. Use a cool middle plane, right shadow,
 // and short upper-left highlights, preserving all original hoop boundaries.
 if(c==="625e71"||c==="3d3b50"){
  c=x>=39?"20202c":"3d3b50";
  if(x>=22&&x<=28&&(y===28||y===29||y===49||y===50))c="625e71";
 }
 pixel(barrel,x-14,y-18,c);
}
save(barrel,"Clutter_Barrel");
var bones=read(R+"Sources/Stage09/Bones_Raw_PixelLab.png");
if(bones.w!==64||bones.h!==64)throw new Error("Unexpected bone source grid");
var skull=blank(24,22),loose=blank(26,12);
// Separate original components at transparent gutters; preserve every opaque pixel's position.
for(var y=28;y<50;y++)for(var x=8;x<30;x++)if(bones.p[(y*64+x)*4+3]){
 var c=hex(bones,x,y);
 // Simplify disconnected cheek micro-shades into two connected matte turning planes.
 // Deep eye/nose sockets, contour and teeth separations remain untouched.
 if(y>=44&&y<=46&&c!=="0a0d11")c=x>=20?"a39796":"766879";
 pixel(skull,x-8,y-28,c);
}
for(var y=38;y<50;y++)for(var x=30;x<55;x++)if(bones.p[(y*64+x)*4+3]){
 var c=hex(bones,x,y);
 if(c!=="0a0d11")c=y<=44?"ccc3b2":y<=46?"a39796":"766879";
 pixel(loose,x-29,y-38,c);
}
// The raw fragment silhouettes read like feet at scene size. Repair those small
// components into narrow shafts with clear knuckled ends on the same native grid.
loose=blank(26,12);
function fragment(x,y,rows){var colors={O:"0a0d11",b:"ccc3b2",s:"a39796"};for(var j=0;j<rows.length;j++)for(var i=0;i<rows[j].length;i++){var c=rows[j].charAt(i);if(c!==".")pixel(loose,x+i,y+j,colors[c]);}}
fragment(2,4,["OO......OO","ObO....ObO","ObbOOOObbO",".ObbbbbssO","ObbOOOObbO","OO......OO"]);
fragment(13,4,[".OO......OO","ObbO....ObO","ObbbOOOOssO",".ObbbbbbssO","..OOO.OObbO","........OO."]);
save(skull,"Clutter_Skull");save(loose,"Clutter_Bones");
console.log("STAGE09_COMPLETE 3 native prop modules; barrel/skull silhouettes preserved, bone fragments clarified");
