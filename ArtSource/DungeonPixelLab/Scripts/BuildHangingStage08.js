// Stage08: native LibreSprite cleanup of PixelLab banner and substantial chain-seed repair; no resampling.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage08/Hanging_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage08/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage08/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE08_SAVED "+name);}

var raw=read(R+"Sources/Stage08/Hanging_Raw_PixelLab.png"),banner=blank(36,64);
if(raw.w!==96||raw.h!==64)throw new Error("Wrong banner source dimensions");
// Keep every native contour/crest pixel. Replace misassigned gray cloth with a plum ramp.
for(var y=0;y<64;y++)for(var x=30;x<66;x++)if(raw.p[(y*raw.w+x)*4+3]){
 var c=hex(raw,x,y);
 if(y>=9){if(c==="3d3b50")c="3b2445";else if(c==="3b2445")c="241f3a";else if(c==="754460")c="553151";else if(c==="6b4039"||c==="9a6244")c="553151";}
 else {if(c==="be9a79"||c==="9a6244")c="3d3b50";else if(c==="9d7766")c="20202c";}
 pixel(banner,x-30,y,c);
}
save(banner,"Hanging_Banner");
// PixelLab chain arrived as a clipped two-pixel strand. Treat it as a seed, not a finished chain.
// Native repair expands the face-on links, opens holes, normalizes pitch, adds complete mount/end.
var seed=read(R+"Sources/Stage08/Chain_Raw_PixelLab.png");
if(seed.w!==16||seed.h!==64)throw new Error("Wrong chain seed");
var link=blank(12,12),colors={o:"0a0d11",m:"20202c",h:"3d3b50",r:"6b4039"};
// Start from the generated strand at native pitch; all rows are subsequently repaired below.
for(var y=0;y<12;y++)for(var x=7;x<=8;x++){
 var c=hex(seed,x,y+12),v=parseInt(c.substr(0,2),16);
 pixel(link,x-2,y,v<20?colors.o:v<40?colors.m:colors.h);
}
// Explicit native repair replaces clipped/no-hole rows with one readable linked cycle.
var rows=[".....oh.....","....ohmo....","...oh..mo...","...oh..mo...","...om..ro...","...om..mo...","....ommo....",".....om.....",".....oh.....",".....om.....",".....om.....",".....oh....."];
for(var y=0;y<12;y++)for(var x=0;x<12;x++){var c=rows[y].charAt(x);if(c==="."){for(var k=0;k<4;k++)link.p[(y*12+x)*4+k]=0;}else pixel(link,x,y,colors[c]);}
save(link,"Hanging_Chain_Repeat");
var end=blank(12,12);
for(var y=0;y<8;y++)for(var x=0;x<12;x++)if(link.p[(y*12+x)*4+3])pixel(end,x,y,hex(link,x,y));
save(end,"Hanging_Chain_End");
var anchor=blank(12,12);
for(var y=2;y<=5;y++)for(var x=2;x<=9;x++)pixel(anchor,x,y,y===2||y===5||x===2||x===9?colors.o:y===3?colors.h:colors.m);
pixel(anchor,3,4,colors.r);
for(var y=6;y<12;y++){pixel(anchor,5,y,colors.o);pixel(anchor,6,y,y===6||y===11?colors.h:colors.m);}
save(anchor,"Hanging_Chain_Anchor");
console.log("STAGE08_COMPLETE 4 native hanging modules; banner contour retained, chain seed repaired");
