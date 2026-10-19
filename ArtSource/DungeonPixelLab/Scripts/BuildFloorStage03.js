// Stage03: native LibreSprite edits of the generated sidescroller capstones.
// Only the shallow surface cluster is selected; generated supports remain source-only.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function clear(a,x,y){var i=(y*a.w+x)*4;for(var k=0;k<4;k++)a.p[i+k]=0;}
function copy(a,w){var b=blank(w||a.w,a.h);for(var y=0;y<b.h;y++)for(var x=0;x<b.w;x++)for(var k=0;k<4;k++)b.p[(y*b.w+x)*4+k]=a.p[(y*a.w+x)*4+k];return b;}
function save(a,name){app.open(R+"Sources/Stage03/Floor_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage03/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage03/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE03_SAVED "+name);}
var raw=read(R+"Sources/Stage03/Floor_Raw_PixelLab.png");
if(raw.w!==128||raw.h!==128)throw new Error("Unexpected raw grid");
var M="241f3a",S="382d55",B="4e3d6c",T="67537d",H="846a95";
var map={"665592":T,"41305d":B,"8881bb":H,"332648":B,"3c2c55":B,"2b1934":S,"34224b":B,"24112a":S,"36284d":B,"312245":B,"382245":B,"210f22":M,"1e0d1e":M};
var base=blank(32,12);
for(var y=0;y<12;y++)for(var x=0;x<32;x++){
 var c=y===0?M:y===1?S:y===10?B:y===11?S:T;
 if(y>=2&&y<=8){
  var sy=14+y,si=(sy*raw.w+96+x)*4;
  c=raw.p[si+3]===0?T:map[hex(raw,96+x,sy)];
  if(!c)throw new Error("Raw source color changed");
  // Join two generated 16px capstones into one wide plane, at native scale.
  // Fill their former joints and rounded shoulders; no scaling or interpolation.
  if(x<=1||x>=30||(x>=14&&x<=17))c=T;
  // Keep the broad plane and its short connected top-left light; remove noisy toe shades.
  if(y===8)c=T;
 }
 if(y===9)c=B;
 pixel(base,x,y,c);
}
// Common open-join profiles: joints are drawn within variants, not deep cracks at module borders.
for(var y=0;y<12;y++){var c=y===0?M:y===1?S:y>=9?(y===11?S:B):T;pixel(base,0,y,c);pixel(base,31,y,c);}
var names=["Floor_Mid_A","Floor_Mid_B","Floor_Mid_C"],variants=[];
for(var n=0;n<3;n++){
 var a=copy(base),joint=[16,21,11][n];
 // Consolidate the generated light bars into one restrained connected patch per plane.
 for(var x=2;x<30;x++)pixel(a,x,5,T);
 for(var x=3;x<joint-2;x++)pixel(a,x,4,H);
 for(var x=joint+3;x<29;x++)pixel(a,x,4,n===1?T:H);
 for(var y=2;y<=10;y++)pixel(a,joint,y,B);
 pixel(a,joint,1,M);
 if(n===1){for(var x=7;x<=10;x++)pixel(a,x,9,T);}
 if(n===2){pixel(a,25,8,B);pixel(a,26,8,B);}
 variants.push(a);save(a,names[n]);
}
var left=copy(variants[0]);clear(left,0,0);clear(left,0,1);clear(left,1,0);
for(var y=2;y<11;y++)pixel(left,0,y,S);pixel(left,1,2,B);pixel(left,0,11,M);
save(left,"Floor_Left");
// A fully authored 30px end cap closes x106..360; it is not a clipped 32px sprite.
var right=copy(variants[1],30);for(var y=0;y<12;y++)pixel(right,0,y,hex(base,0,y));
clear(right,29,0);clear(right,29,1);clear(right,28,0);
for(var y=2;y<11;y++)pixel(right,29,y,S);pixel(right,28,2,B);pixel(right,29,11,M);
save(right,"Floor_Right");
console.log("STAGE03_COMPLETE 5 native surface documents");
