// Run only inside LibreSprite after preserving the reviewed production files.
// All raster work happens in native cels. The output directory must exist.
// This is a local repair of the 2026-09-21 reviewed poses, not a draft generator.
var ROOT = 'C:/UnityProjects/Dungeon Matcher/ArtSource/LocalEnemies/';
var SOURCE = ROOT + 'Originals/BeforePresentationPolish/';
var DEST = ROOT + 'Review/Polished/';
var OUT = '0A0D11';

function pixel(x,y,w){return (y*w+x)*4;}
function color(a,i){return ('0'+a[i].toString(16)).slice(-2).toUpperCase()+('0'+a[i+1].toString(16)).slice(-2).toUpperCase()+('0'+a[i+2].toString(16)).slice(-2).toUpperCase();}
function set(a,w,h,x,y,c){
 if(x<0||x>=w||y<0||y>=h)throw Error('Pixel outside canvas');
 var i=pixel(x,y,w);
 for(var k=0;k<3;k++)a[i+k]=c?parseInt(c.slice(k*2,k*2+2),16):0;
 a[i+3]=c?255:0;
}
function cp(a,i,b,j){for(var k=0;k<4;k++)b[j+k]=a[i+k];}
function rectangle(a,w,h,x0,y0,x1,y1,c){for(var y=y0;y<=y1;y++)for(var x=x0;x<=x1;x++)set(a,w,h,x,y,c);}
function patch(src,dst,sw,dw,dh,x0,y0,x1,y1,dx,dy,opaqueOnly){
 for(var y=y0;y<=y1;y++)for(var x=x0;x<=x1;x++){
  var xx=x+dx,yy=y+dy;
  if(xx<0||xx>=dw||yy<0||yy>=dh)throw Error('Patch outside canvas');
  if(!opaqueOnly||src[pixel(x,y,sw)+3])cp(src,pixel(x,y,sw),dst,pixel(xx,yy,dw));
 }
}
function stroke(a,w,h,x0,y0,x1,y1,r,c){
 var n=Math.max(Math.abs(x1-x0),Math.abs(y1-y0));
 for(var i=0;i<=n;i++){
  var x=Math.round(x0+(x1-x0)*i/Math.max(n,1)),y=Math.round(y0+(y1-y0)*i/Math.max(n,1));
  for(var v=-r;v<=r;v++)for(var u=-r;u<=r;u++)if(u*u+v*v<=r*r+1)set(a,w,h,x+u,y+v,c);
 }
}
function read(name,w,h,count){
 app.open(SOURCE+name+'.aseprite');var s=app.activeSprite,frames=[];
 if(s.width!==w||s.height!==h||s.layer(0).celCount!==count)throw Error('Unexpected source '+name);
 for(var f=0;f<count;f++){
  var cel=s.layer(0).cel(f);
  if(cel.x||cel.y||cel.image.width!==w||cel.image.height!==h)throw Error('Full transparent cel required '+name);
  frames.push(new Uint8Array(cel.image.getImageData()));
 }
 app.activeDocument.close();return frames;
}
function save(name,w,h,frames){
 app.open(SOURCE+name+'.aseprite');var s=app.activeSprite;
 if(s.width!==w||s.height!==h||s.layer(0).celCount!==frames.length)throw Error('Timing template changed '+name);
 s.saveAs(DEST+name+'.aseprite',false);
 for(var f=0;f<frames.length;f++)s.layer(0).cel(f).image.putImageData(frames[f]);
 s.commit();s.save();app.activeDocument.close();
}

var miner=read('Miner_Idle',96,80,9);
var basket=read('BasketVillager_Idle',64,64,9);
var builder=read('BarricadeVillager_Idle',64,64,9);
var basketAttack=read('BasketVillager_AutoAttack',96,64,8);
var builderAttack=read('BarricadeVillager_AutoAttack',96,64,8);
var builderAbility=read('BarricadeVillager_Ability',96,64,10);

// The previous candle extraction ended one row before the holder base. Both
// recovery poses therefore had a transparent horizontal cut beneath the lamp.
// Restore only the missing dark metal base; the face, flame and cap stay put.
for(var k=0;k<2;k++){
 var f=k===0?2:7;
 rectangle(miner[f],96,80,33,30,41,30,OUT);
}
save('Miner_Idle',96,80,miner);

// The planted-foot patch previously wiped the trouser-to-boot connection.
// Restore native ready-pose trouser pixels locally, without moving the soles.
for(var f=2;f<8;f++){
 patch(builder[0],builder[f],64,64,64,24,58,44,59,0,0,false);
 if(f===2||f===3||f===7)patch(builder[0],builder[f],64,64,64,24,56,44,57,0,0,false);
}
save('BarricadeVillager_Idle',64,64,builder);

// A basket cannot be grounded by copying the full bottom six image rows. Its
// rim rose four pixels while its base stayed frozen. Move the approved basket,
// berries and loaded hand together. Keep the existing head/shoulder poses.
var rigidBasket=new Uint8Array(64*64*4);
patch(basket[0],rigidBasket,64,64,64,9,48,24,63,0,0,true);
var basketDx=[0,0,-1,-1,-2,-3,-2,-1,0];
var basketDy=[0,0,-1,-2,-3,-4,-3,-2,0];
for(var f=2;f<8;f++){
 var a=basket[f],dx=basketDx[f],dy=basketDy[f];
 // Above the old floor patch, x=24 belongs to the coat and is protected.
 rectangle(a,64,64,5,48+dy,23,57,null);
 rectangle(a,64,64,5,58,24,63,null);
 patch(rigidBasket,a,64,64,64,9,48,24,63,dx,dy,true);
 // Close the small sleeve-to-grip seam with the existing skin ramp.
 var hx=20+dx,hy=49+dy;
 set(a,64,64,hx,hy-1,'E6B08A');
 set(a,64,64,hx+1,hy-1,'B9825D');
}
save('BasketVillager_Idle',64,64,basket);

// Rebuild only the free throwing arm on articulated supplied idle poses. The
// ready/return drawings remain exact. Native-sized head, hat, body and loaded
// basket are copied intact, so the facial acting shares the shoulder dip.
function berryHand(a,x,y,withBerry,release){
 rectangle(a,96,64,x-1,y-1,x+2,y+2,OUT);
 rectangle(a,96,64,x-1,y-1,x+1,y+1,'E6B08A');
 rectangle(a,96,64,x,y+1,x+1,y+1,'B9825D');
 if(release){set(a,96,64,x-2,y-1,OUT);set(a,96,64,x-2,y,'E6B08A');}
 if(withBerry){
  rectangle(a,96,64,x-1,y-4,x+1,y-2,'7C1A30');
  set(a,96,64,x-1,y-4,'C74A5A');set(a,96,64,x,y-4,'C74A5A');
 }
}
function throwPose(source,sx,sy,ex,ey,wx,wy,berry,release){
 var a=new Uint8Array(96*64*4);
 patch(source,a,64,96,64,0,0,63,63,16,0,true);
 // Clear the old free arm while preserving face, neck, chest and waistband.
 rectangle(a,96,64,58,37,71,40,null);
 rectangle(a,96,64,56,41,71,54,null);
 // Clean torso edge behind the replacement sleeve.
 stroke(a,96,64,55,44,55,51,0,OUT);
 stroke(a,96,64,sx,sy,ex,ey,2,OUT);
 stroke(a,96,64,sx,sy,ex,ey,1,'6C5A4A');
 stroke(a,96,64,sx-1,sy-1,ex,ey-1,0,'9A856F');
 stroke(a,96,64,ex,ey,wx,wy,1,OUT);
 stroke(a,96,64,ex,ey,wx,wy,0,'B9825D');
 berryHand(a,wx,wy,berry,release);
 return a;
}
basketAttack[1]=throwPose(basket[2],55,42,49,48,35,51,false,false);
basketAttack[2]=throwPose(basket[3],55,42,65,43,64,34,true,false);
basketAttack[3]=throwPose(basket[3],55,42,51,37,41,35,true,false);
basketAttack[4]=throwPose(basket[4],54,43,44,40,33,39,false,true);
basketAttack[5]=throwPose(basket[5],53,44,44,45,36,45,false,false);
basketAttack[6]=throwPose(basket[7],55,42,59,47,59,51,false,false);
save('BasketVillager_AutoAttack',96,64,basketAttack);

// Reconnect the two existing attack boots to their trouser legs. The authored
// axe rotations, grip, face, impact frame and wind-up remain unchanged.
for(var k=0;k<3;k++){
 var f=[1,3,6][k];
 patch(builderAttack[0],builderAttack[f],96,96,64,40,56,60,59,0,0,false);
}
save('BarricadeVillager_AutoAttack',96,64,builderAttack);

// The rear knee had become a long flat slab. Keep the forward foot behind the
// plank and the contact pose above y57; shorten the rear boot and round the
// cloth knee with a two-pixel highlight and a separate dark shin crease.
for(var f=2;f<=6;f++){
 var a=builderAbility[f],dx=f===6?1:0;
 rectangle(a,96,64,49+dx,58,64+dx,63,null);
 rectangle(a,96,64,50+dx,58,54+dx,61,OUT);
 rectangle(a,96,64,51+dx,58,53+dx,60,'6C5A4A');
 set(a,96,64,51+dx,58,'9A856F');set(a,96,64,51+dx,59,'9A856F');
 rectangle(a,96,64,52+dx,61,59+dx,62,OUT);
 rectangle(a,96,64,54+dx,61,57+dx,61,'3E312A');
 rectangle(a,96,64,58+dx,60,61+dx,62,OUT);
 rectangle(a,96,64,59+dx,61,60+dx,62,'4A2C1C');
 rectangle(a,96,64,52+dx,63,61+dx,63,OUT);
}
save('BarricadeVillager_Ability',96,64,builderAbility);
app.command.GotoFirstFrame();
