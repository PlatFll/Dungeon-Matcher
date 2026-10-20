// Stage09: direct native LibreSprite cleanup of new PixelLab environmental props. No scaling.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage10/Frame_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage10/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage10/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE10_SAVED "+name);}
var raw=read(R+"Sources/Stage10/Frame_Raw_PixelLab.png");
if(raw.w!==64||raw.h!==64)throw new Error("Unexpected raw grid");
var remap={"151423":"0a0d11","f5f0f5":"ccc3d5","e2dcea":"ccc3d5","ccc3d5":"958ba1","958ba1":"846a95","846a95":"615b6b","4e3d6c":"4e3d6c"};
function mapped(x,y){var c=hex(raw,x,y);return remap[c]||c;}
// PixelLab's continuous five-pixel straight rail profile, with a quiet native
// violet backing. Repeat unit lengths are integer native pixels, never resized art.
var top=blank(24,8),bottom=blank(24,11),left=blank(9,20),right=blank(10,20);
for(var x=0;x<24;x++){
 for(var y=0;y<5;y++){pixel(top,x,y,mapped(26,8+y));pixel(bottom,x,y,mapped(26,8+y));}
 pixel(top,x,5,"241f3a");pixel(top,x,6,"0a0d11");
 pixel(bottom,x,5,"4e3d6c");pixel(bottom,x,6,"241f3a");pixel(bottom,x,7,"0a0d11");
}
for(var y=0;y<20;y++){
 for(var x=0;x<5;x++){pixel(left,x+1,y,mapped(8+x,26));pixel(right,x+1,y,mapped(8+x,26));}
 pixel(left,0,y,"241f3a");pixel(left,6,y,"241f3a");pixel(left,7,y,"0a0d11");
 pixel(right,0,y,"0a0d11");pixel(right,6,y,"241f3a");pixel(right,7,y,"0a0d11");
}
save(top,"Portrait_Rail_Top");save(bottom,"Portrait_Rail_Bottom");save(left,"Portrait_Rail_Left");save(right,"Portrait_Rail_Right");
function copyRect(src,dst,sx,sy,w,h,dx,dy){for(var y=0;y<h;y++)for(var x=0;x<w;x++){var i=((y+sy)*src.w+x+sx)*4;if(src.p[i+3])pixel(dst,x+dx,y+dy,hex(src,x+sx,y+sy));}}
// Adapt the raw 13px diamond socket, close accidental holes, and reconnect the
// orthogonal rail joins. Lighting stays upper-left on all four corners.
function corner(name,w,h,isRight,isBottom){
 var a=blank(w,h),cx=isRight?6:7,cy=isBottom?7:7;
 var rail=isBottom?bottom:top,vy=isBottom?0:9,hx=isRight?0:9,hy=isBottom?4:3;
 copyRect(rail,a,0,0,isRight?7:w-9,7,hx,hy);
 var side=isRight?right:left;copyRect(side,a,0,0,Math.min(side.w,w-(isRight?2:1)),isBottom?7:h-9,isRight?2:1,vy);
 for(var y=cy-6;y<=cy+6;y++)for(var x=cx-6;x<=cx+6;x++){
  var dx=x-cx,dy=y-cy,d=Math.abs(dx)+Math.abs(dy);if(d>6)continue;
  var c=d===6?"0a0d11":d===5?(dx+dy<=0?"ccc3d5":"615b6b"):d===4?(dx+dy<0?"958ba1":"846a95"):d===3?"241f3a":dx+dy<0?"f183fb":dx+dy<2?"ab2ad3":"68108e";
  if(dx===-1&&dy===-1)c="fcd4ff";pixel(a,x,y,c);
 }
 save(a,name);
}
corner("Portrait_Corner_TL",14,14,false,false);corner("Portrait_Corner_TR",13,14,true,false);
corner("Portrait_Corner_BL",14,16,false,true);corner("Portrait_Corner_BR",13,16,true,true);
// Raw square clamp becomes a small separate connector; keep its top-left bevel,
// quiet its broad white plane and add one recessed rivet, without resampling.
var clamp=blank(8,8);
for(var y=0;y<7;y++)for(var x=0;x<7;x++)if(raw.p[((y+7)*64+x+44)*4+3])pixel(clamp,x,y,mapped(x+44,y+7));
pixel(clamp,2,2,"e2dcea");pixel(clamp,3,3,"615b6b");pixel(clamp,3,4,"4e3d6c");
save(clamp,"Portrait_Clamp");
// Empty health shell only. Fill values and labels stay runtime UI.
var hp=blank(64,10);
for(var y=1;y<9;y++)for(var x=0;x<64;x++){
 var edge=x===0||x===63||y===1||y===8;
 var c=edge?"0a0d11":y===2?"846a95":y===7?"4e3d6c":x===1||x===62?"615b6b":"151423";
 pixel(hp,x,y,c);
}
save(hp,"Portrait_HP_Shell");
var heart=read(R+"../../Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Hud/PlayerHeart.png");
save(heart,"Portrait_Heart_Existing");
console.log("STAGE10_COMPLETE 10 new modular pieces plus unchanged existing heart");
