// Stage07: native LibreSprite cleanup of the new PixelLab torch and local stone-light overlays; never resample artwork.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage07/Torch_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage07/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage07/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE07_SAVED "+name);}
var contextFiles={"Wall_D_DeepStone": "Exports/Stage01/Wall_D_DeepStone.png", "Wall_A_Quiet": "Exports/Stage01/Wall_A_Quiet.png", "Wall_Edge_Left": "Exports/Stage01/Wall_Edge_Left.png", "Wall_B_VioletStone": "Exports/Stage01/Wall_B_VioletStone.png", "Support_Core": "Exports/Stage06/Support_Core.png"};
var contextPlaces=[{"name": "Wall_D_DeepStone", "x": 96, "y": 48, "layer": 1}, {"name": "Wall_A_Quiet", "x": 144, "y": 48, "layer": 1}, {"name": "Wall_A_Quiet", "x": 288, "y": 48, "layer": 1}, {"name": "Wall_Edge_Left", "x": 336, "y": 48, "layer": 1}, {"name": "Wall_A_Quiet", "x": 96, "y": 96, "layer": 1}, {"name": "Wall_B_VioletStone", "x": 144, "y": 96, "layer": 1}, {"name": "Wall_A_Quiet", "x": 288, "y": 96, "layer": 1}, {"name": "Wall_Edge_Left", "x": 336, "y": 96, "layer": 1}, {"name": "Support_Core", "x": 96, "y": 44, "layer": 12}, {"name": "Support_Core", "x": 96, "y": 72, "layer": 12}, {"name": "Support_Core", "x": 96, "y": 100, "layer": 12}, {"name": "Support_Core", "x": 96, "y": 128, "layer": 12}, {"name": "Support_Core", "x": 324, "y": 44, "layer": 12}, {"name": "Support_Core", "x": 324, "y": 72, "layer": 12}, {"name": "Support_Core", "x": 324, "y": 100, "layer": 12}, {"name": "Support_Core", "x": 324, "y": 128, "layer": 12}];
var raw=read(R+"Sources/Stage07/Torch_Raw_PixelLab.png");if(raw.w!==64||raw.h!==64)throw new Error("Wrong source grid");
var fixture=blank(32,56),flame=blank(32,56);
// Transparent trim/padding translation only; source drawing retains native pitch.
for(var y=4;y<60;y++)for(var x=16;x<48;x++)if(raw.p[(y*64+x)*4+3]){
 var c=hex(raw,x,y),a=y<30?flame:fixture;
 // The bottom plate's accidental orange specks become cool mounting iron.
 if(y>=45&&(c==="bc5426"||c==="e97e27"))c="3d3b50";
 pixel(a,x-16,y-4,c);
}
// Reduce iron micro-shading to three broad planes while preserving the generated profile.
for(var y=33;y<=40;y++)for(var x=23;x<=40;x++){
 var dx=x-16,dy=y-4;if(!fixture.p[(dy*32+dx)*4+3])continue;
 if(hex(fixture,dx,dy)==="0a0d11")continue;
 pixel(fixture,dx,dy,x>=27&&x<=34&&y===33||x>=28&&x<=34&&y===36?"3d3b50":"20202c");
}
// Mounting plate receives one short top-left iron edge, not a noisy double rim.
for(var y=46;y<=52;y++)for(var x=29;x<=35;x++){
 var dx=x-16,dy=y-4;if(!fixture.p[(dy*32+dx)*4+3]||hex(fixture,dx,dy)==="0a0d11")continue;
 pixel(fixture,dx,dy,x===29?"3d3b50":"20202c");
}
// Keep exactly one isolated two-pixel ember; connect/simplify the little side tongue.
for(var y=17;y<=18;y++)flame.p[((y-4)*32+(38-16))*4+3]=0;
for(var y=20;y<=21;y++)pixel(flame,38-16,y-4,"e97e27");
pixel(flame,21,8,"e97e27");pixel(flame,21,9,"e97e27");
save(fixture,"Torch_Fixture");save(flame,"Torch_Flame");
// Context is built from approved native modules, not a screenshot or painted replacement.
// __CONTEXT__ injects only intersecting wall/support placement data from Stage06.
var cache={};function native(name){if(!cache[name])cache[name]=read(R+contextFiles[name]);return cache[name];}
function context(ox,oy,w,h){var a=blank(w,h);for(var n=0;n<contextPlaces.length;n++){
 var p=contextPlaces[n],src=native(p.name);
 for(var y=0;y<src.h;y++)for(var x=0;x<src.w;x++){
  var dx=p.x+x-ox,dy=p.y+y-oy;if(dx<0||dy<0||dx>=w||dy>=h)continue;
  if(src.p[(y*src.w+x)*4+3])pixel(a,dx,dy,hex(src,x,y));
 }
 }return a;}
var warm={
 "28283f":["36303f","65403e","92543b"],"35324f":["4e3b49","89523f","b77543"],
 "302940":["40313c","54343c","744332"],"413451":["583d47","89523f","b77543"],
 "382d55":["483441","65403e","744332"],"4e3d6c":["60434d","89523f","a9633e"],
 "67537d":["7a555b","a36a4a","c9864c"],"241f3a":["322533","40303a","4e3134"]
};
function light(ox,w,name){var under=context(ox,68,w,72),a=blank(w,72),cx=128-ox;if(ox>200)cx=336-ox;
 for(var y=8;y<64;y++)for(var x=2;x<w-2;x++){
  // Stepped, sparse local bands; all colors opaque. Mortar and contours remain transparent.
  var dx=Math.abs(x-cx),level=-1;
  if(y<18&&dx<11)level=0;
  else if(y>=18&&y<30&&dx<20)level=dx<10?1:0;
  else if(y>=30&&y<44&&dx<25)level=dx<10?2:dx<18?1:0;
  else if(y>=44&&y<54&&dx<19)level=dx<11?1:0;
  else if(y>=54&&y<64&&dx<10)level=0;
  if(level<0)continue;var cols=warm[hex(under,x,y)];if(cols)pixel(a,x,y,cols[level]);
 }
 save(a,name);
}
light(96,64,"Torch_Light_Left");light(304,56,"Torch_Light_Right");
console.log("STAGE07_COMPLETE 4 native torch and local-light layers");
