// Stage 01: native pixel/cel edits executed by LibreSprite, no image resampling.
// __ROOT__ is replaced with this checkout's absolute ArtSource/DungeonPixelLab path.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function copy(a){var b=blank(a.w,a.h);b.p.set(a.p);return b;}
function crop(a,x,y,w,h){var b=blank(w,h);for(var Y=0;Y<h;Y++)for(var X=0;X<w;X++)pixel(b,X,Y,hex(a,x+X,y+Y));return b;}
function save(a,name){app.open(R+"Sources/Stage01/Wall_Raw_PixelLab.png");var s=app.activeSprite;
 // Allocate the destination, then replace EVERY byte with authored native data.
 s.saveAs(R+"Native/Stage01/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();
 app.command.BackgroundFromLayer();s.commit();
 var cel=s.layer(0).cel(0);if(cel.image.width!==a.w||cel.image.height!==a.h)throw new Error("Cel allocation mismatch");
 cel.image.putImageData(a.p);s.layer(0).name=name;s.commit();
 var check=s.layer(0).cel(0).image.getImageData();if(check.length!==a.p.length)throw new Error("Native byte count mismatch");
 for(var q=0;q<check.length;q++)if(check[q]!==a.p[q])throw new Error("Native pixel mismatch at "+q);
 s.saveAs(R+"Native/Stage01/"+name+".aseprite",true);app.activeDocument.close();
 console.log("STAGE01_SAVED "+name+" "+a.w+"x"+a.h);
}
var raw=read(R+"Sources/Stage01/Wall_Raw_PixelLab.png");
if(raw.w!==64||raw.h!==64)throw new Error("Unexpected PixelLab source dimensions");
var M="151423",S="201d33",B="28283f",H="35324f",A="302940",AH="413451";
var ramp={"151423":M,"201d33":S,"35324f":B,"4e3d6c":H};
var tile=blank(48,48);
for(var y=0;y<48;y++)for(var x=0;x<48;x++){
 var row=Math.floor(y/12),v=y%12,u=(x+(row%2)*12)%24;
 var color=M;
 if(v>0&&v<11&&u>0&&u<23){
  // PixelLab's complete brick at x14..37,y23..32 supplies all material planes.
  // Copy native source pixels; trim redundant two-pixel right-edge bevel.
  color=ramp[hex(raw,14+u-1,23+v-1)];
  if(!color)throw new Error("Raw source palette changed");
  // Slim the generated three-pixel bevel; make the right overlap read as a recess.
  if(u>=3&&v>=2&&color===H)color=B;
  if(u===22&&v>=5)color=S;
  if(v===10&&u>=16)color=S;
 }
 pixel(tile,x,y,color);
}
// Shared profiles are locked across variants; split bricks continue identically.
for(var y=0;y<48;y++){
 var v=y%12,row=Math.floor(y/12);
 if(row%2===1){var c=(v===0||v===11)?M:v===1?H:v===10?S:B;pixel(tile,0,y,c);pixel(tile,47,y,c);}
}
var names=["Wall_A_Quiet","Wall_B_VioletStone","Wall_C_SoftWear","Wall_D_DeepStone"],variants=[];
for(var n=0;n<4;n++){
 var a=copy(tile);
 // Changes stay away from every tile edge; all 16 directional pairings remain valid.
 for(var y=14;y<=21;y++)for(var x=14;x<=32;x++){
  var c=hex(a,x,y);
  if(n===1){if(c===B)pixel(a,x,y,A);if(c===H)pixel(a,x,y,AH);}
  if(n===3&&c===B)pixel(a,x,y,S);
 }
 if(n===1){for(var x=15;x<=31;x++)if(hex(a,x,13)===H)pixel(a,x,13,AH);}
 if(n===2){
  // Two coherent worn bevel segments, never isolated random noise.
  for(var x=29;x<=34;x++)pixel(a,x,25,B);
  for(var x=3;x<=7;x++)pixel(a,x,37,B);
  for(var x=29;x<=34;x++)pixel(a,x,26,H);
 }
 if(n===3){for(var x=16;x<=30;x++)if(hex(a,x,13)===H)pixel(a,x,13,B);}
 variants.push(a);save(a,names[n]);
}
// Phase-locked half modules close rectangular material fields without new borders,
// pillars, floor lips or artificial shaded bands. They are exact native subsets.
save(crop(tile,0,0,24,48),"Wall_Edge_Left");
save(crop(tile,24,0,24,48),"Wall_Edge_Right");
save(crop(tile,0,0,48,24),"Wall_Edge_Top");
save(crop(tile,0,24,48,24),"Wall_Edge_Bottom");
save(crop(tile,0,0,24,24),"Wall_Corner_TL");
save(crop(tile,24,0,24,24),"Wall_Corner_TR");
save(crop(tile,0,24,24,24),"Wall_Corner_BL");
save(crop(tile,24,24,24,24),"Wall_Corner_BR");
// Native closure for a 360x640 preview: 13 full courses plus 16 pixels.
save(crop(tile,0,0,48,16),"Wall_Edge_Bottom16");
save(crop(tile,0,0,24,16),"Wall_Corner_RightBottom16");
console.log("STAGE01_COMPLETE 14 native sources");
