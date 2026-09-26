// Original pixel art authored with LibreSprite's native cel API.
// No filtering, external image generator, or antialiasing is used.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/Presentation/';
var PROJECT='C:/UnityProjects/Dungeon Matcher/';
function canvas(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function dot(a,x,y,c){x=Math.round(x);y=Math.round(y);if(x<0||y<0||x>=a.w||y>=a.h)return;var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=c?parseInt(c.slice(2*k,2*k+2),16):0;a.p[i+3]=c?255:0;}
function rect(a,x,y,w,h,c){for(var v=y;v<y+h;v++)for(var u=x;u<x+w;u++)dot(a,u,v,c);}
function line(a,x,y,xx,yy,c,r){var n=Math.max(Math.abs(xx-x),Math.abs(yy-y));for(var t=0;t<=n;t++)rect(a,Math.round(x+(xx-x)*t/Math.max(1,n))-(r||0),Math.round(y+(yy-y)*t/Math.max(1,n))-(r||0),1+2*(r||0),1+2*(r||0),c);}
function oval(a,x,y,rx,ry,c){for(var v=-ry;v<=ry;v++)for(var u=-rx;u<=rx;u++)if(u*u/(rx*rx)+v*v/(ry*ry)<=1)dot(a,x+u,y+v,c);}
function poly(a,pts,c){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var inside=false;for(var i=0,j=pts.length-1;i<pts.length;j=i++){var p=pts[i],q=pts[j];if(((p[1]>y)!=(q[1]>y))&&(x<(q[0]-p[0])*(y-p[1])/(q[1]-p[1])+p[0]))inside=!inside;}if(inside)dot(a,x,y,c);}}
function stamp(a,b,x,y){for(var v=0;v<b.h;v++)for(var u=0;u<b.w;u++){var i=(v*b.w+u)*4;if(!b.p[i+3])continue;var c='';for(var k=0;k<3;k++)c+=('0'+b.p[i+k].toString(16)).slice(-2);dot(a,x+u,y+v,c);}}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=canvas(s.width,s.height),d=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=d[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function save(a,name){app.open(PROJECT+'Assets/_Game/Resources/UI/Consumables/Potion.png');var s=app.activeSprite;s.saveAs(ROOT+name+'.aseprite',false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw Error(name+' canvas');c.image.putImageData(a.p);s.layer(0).name=name;s.commit();app.command.LayerFromBackground();s.commit();s.save();app.command.ScrollCenter();app.activeDocument.close();}
var OUT='0A0D11',INK='151423',DEEP='241F3A',MID='382D55',LIT='4E3D6C',EDGE='846A95';

// Quiet, seamless purple masonry for gutters and the HUD. The border joints
// land on whole pixels and the four courses alternate their vertical joints.
var wall=canvas(64,64);
rect(wall,0,0,64,64,'11101B');
for(var row=0;row<4;row++)for(var col=-1;col<3;col++){
 var x=col*32+(row%2)*16,y=row*16;
 rect(wall,x+1,y+1,30,14,DEEP);rect(wall,x+2,y+2,28,11,'211D31');
 line(wall,x+3,y+2,x+28,y+2,'30283F');line(wall,x+2,y+3,x+2,y+10,'30283F');
 line(wall,x+5,y+13,x+28,y+13,'181523');
 if((row+col)%3===0){line(wall,x+22,y+5,x+19,y+8,'191724');dot(wall,x+19,y+9,'191724');}
}save(wall,'DungeonBackdropTile');

function torch(flicker){var a=canvas(64,64);
 oval(a,32,30,18,23,'252329');oval(a,32,28,13,18,'322D31');
 rect(a,28,33,8,23,OUT);rect(a,29,34,6,20,'34313C');line(a,30,35,30,51,'6D6863');
 rect(a,22,35,20,5,OUT);rect(a,23,35,18,2,'6D6863');rect(a,25,39,14,4,'34313C');
 poly(a,[[23,32],[26,24],[28,20],[29,10+flicker],[34,16],[36,13],[37,23],[41,29],[39,35],[27,35]],'8A3B24');
 poly(a,[[26,32],[28,25],[31,17+flicker],[34,22],[36,20],[39,30],[36,34],[29,34]],'E56B12');
 poly(a,[[29,32],[30,27],[33,22],[33,27],[36,30],[34,34],[30,34]],'FFB870');
 line(a,31,30,32,32,'FFECB4',1);dot(a,28,16-flicker,'F5923D');return a;}
var torchA=torch(0),torchB=torch(2);save(torchA,'Torch');save(torchB,'TorchAlt');

var banner=canvas(64,64);
line(banner,13,8,51,8,OUT,2);line(banner,14,7,50,7,'877560');
oval(banner,13,8,2,2,'998772');oval(banner,51,8,2,2,'998772');
poly(banner,[[17,9],[48,9],[48,52],[40,58],[32,54],[25,58],[17,53]],OUT);
poly(banner,[[19,10],[46,10],[46,51],[40,55],[32,51],[25,55],[19,51]],MID);
rect(banner,21,11,3,37,LIT);rect(banner,42,10,3,41,DEEP);rect(banner,24,11,18,3,LIT);
line(banner,20,50,25,53,'877560');line(banner,25,53,32,49,'877560');line(banner,32,49,40,53,'877560');line(banner,40,53,44,50,'877560');
// A small crown-and-bone heraldic device, subordinate to characters.
poly(banner,[[25,23],[25,18],[29,21],[32,16],[35,21],[39,18],[39,23]],'998772');
oval(banner,32,31,8,8,'877560');rect(banner,28,35,9,6,'877560');rect(banner,26,29,4,4,DEEP);rect(banner,34,29,4,4,DEEP);rect(banner,31,34,2,2,DEEP);line(banner,30,38,30,40,DEEP);line(banner,34,38,34,40,DEEP);save(banner,'RoyalBanner');

var barrel=canvas(64,64);
oval(barrel,32,58,24,4,'17181D');oval(barrel,32,38,21,22,OUT);rect(barrel,12,25,40,27,OUT);
oval(barrel,31,38,18,21,'4A2C1C');rect(barrel,14,24,36,28,'7A4D2E');
for(var x=16;x<49;x+=7){line(barrel,x,26,x-1,51,'4A2C1C');line(barrel,x+1,27,x,49,'B07A43');}
oval(barrel,32,21,20,7,OUT);oval(barrel,32,21,18,5,'B07A43');oval(barrel,32,22,15,3,'7A4D2E');line(barrel,24,19,23,24,'4A2C1C');line(barrel,34,18,34,24,'4A2C1C');line(barrel,41,20,42,23,'4A2C1C');
for(var y=29;y<=48;y+=19){rect(barrel,12,y,40,5,OUT);rect(barrel,13,y,38,3,'34313C');line(barrel,15,y,46,y,'6D6863');dot(barrel,17,y+1,'998772');dot(barrel,46,y+1,'6D6863');}save(barrel,'Barrel');

var skull=canvas(64,64);oval(skull,32,59,26,4,'17181D');
function boneHead(a,x,y,s){oval(a,x,y,9*s,7*s,OUT);oval(a,x,y-1,8*s,6*s,'756656');rect(a,x-5*s,y+4*s,10*s,5*s,OUT);rect(a,x-4*s,y+3*s,8*s,5*s,'998772');oval(a,x-2*s,y-2*s,5*s,4*s,'998772');rect(a,x-6*s,y-1*s,4*s,3*s,OUT);rect(a,x+2*s,y-1*s,4*s,3*s,OUT);rect(a,x,y+3*s,1*s,2*s,OUT);line(a,x-2*s,y+6*s,x-2*s,y+8*s,OUT);line(a,x+2*s,y+6*s,x+2*s,y+8*s,OUT);}
boneHead(skull,24,46,1);boneHead(skull,41,49,1);line(skull,9,58,26,53,'877560',1);line(skull,35,58,54,55,'756656',1);save(skull,'SkullPile');

// Bespoke title lettering: warm bevels, deep violet extrusion, central story gem.
var glyph={D:['11110','11011','11001','11001','11001','11011','11110'],U:['11011','11011','11011','11011','11011','11011','01110'],N:['11001','11101','11101','11011','11011','11011','11001'],G:['01110','11001','11000','11011','11001','11001','01110'],E:['11111','11000','11000','11110','11000','11000','11111'],O:['01110','11011','11011','11011','11011','11011','01110'],M:['11011','11111','11111','11011','11011','11011','11011'],A:['01110','11011','11011','11111','11011','11011','11011'],T:['11111','01110','00100','00100','00100','00100','00100'],C:['01111','11000','11000','11000','11000','11000','01111'],H:['11011','11011','11011','11111','11011','11011','11011'],R:['11110','11011','11011','11110','11100','11010','11011']};
var logo=canvas(210,88);
function text(a,str,y){var scale=3,x=Math.floor((a.w-(str.length*6-1)*scale)/2);
 for(var pass=0;pass<3;pass++)for(var n=0;n<str.length;n++)for(var yy=0;yy<7;yy++)for(var xx=0;xx<5;xx++)if(glyph[str[n]][yy][xx]==='1'){
  var dx=x+n*18+xx*3,dy=y+yy*3;
  if(pass===0)rect(a,dx-1,dy-1,5,6,OUT);
  else if(pass===1)rect(a,dx+1,dy+3,3,3,MID);
  else rect(a,dx,dy,3,3,yy<3?'FDF5E5':yy<5?'E7C979':'B88A46');
 }}
text(logo,'DUNGEON',5);text(logo,'MATCHER',59);
line(logo,24,43,82,43,DEEP,2);line(logo,128,43,186,43,DEEP,2);line(logo,30,42,78,42,'846A95');line(logo,132,42,180,42,'846A95');
poly(logo,[[105,28],[123,39],[116,53],[105,59],[94,53],[87,39]],OUT);
poly(logo,[[105,30],[120,39],[113,51],[105,57],[97,51],[90,39]],'65038D');
poly(logo,[[105,30],[120,39],[112,42],[99,42],[90,39]],'F078FF');
poly(logo,[[99,42],[112,42],[105,57]],'C22BEA');poly(logo,[[112,42],[120,39],[113,51],[105,57]],'68168A');
poly(logo,[[105,31],[115,38],[98,38]],'F9F5EB');line(logo,99,40,111,40,'F078FF');
dot(logo,84,35,'F078FF');line(logo,127,47,131,47,'F078FF');line(logo,129,45,129,49,'F078FF');save(logo,'DungeonMatcherLogo');

// More readable versions of the existing consumables, still compact 24px art.
var potion=canvas(24,24);
rect(potion,9,2,7,5,OUT);rect(potion,10,2,5,3,'7A4D2E');rect(potion,10,2,4,1,'E7C979');
rect(potion,9,6,7,6,OUT);rect(potion,10,6,5,7,'2E568F');rect(potion,10,6,2,6,'A9E0FF');
oval(potion,12,16,8,7,OUT);oval(potion,12,16,7,6,'2E568F');oval(potion,12,17,6,5,'7C1A30');oval(potion,11,16,5,4,'C74A5A');
rect(potion,7,13,10,2,'FF98A0');rect(potion,6,13,2,5,'A9E0FF');rect(potion,7,12,2,2,'FDF5E5');line(potion,17,16,17,19,'4E96DB');rect(potion,9,8,7,2,'8A5622');rect(potion,10,8,5,1,'FBEA90');dot(potion,10,16,'FF98A0');save(potion,'Potion');
var bomb=canvas(24,24);
oval(bomb,11,15,9,8,OUT);oval(bomb,11,14,8,7,DEEP);oval(bomb,10,13,7,6,'34313C');oval(bomb,8,12,4,4,'5A535B');rect(bomb,6,10,3,2,'B7A393');dot(bomb,6,9,'FDF5E5');
rect(bomb,10,5,6,5,OUT);rect(bomb,11,6,4,2,'8A5622');rect(bomb,11,6,4,1,'FBEA90');
line(bomb,13,5,16,2,'B07A43');line(bomb,16,2,19,3,'E7C979');dot(bomb,20,3,'FFECB4');line(bomb,20,1,20,2,'E56B12');dot(bomb,22,4,'F5923D');dot(bomb,19,5,'F5923D');save(bomb,'Bomb');

// Authored menu scenery from the same native masonry and dressing.
var menu=canvas(320,480);
for(var y=0;y<480;y+=64)for(var x=0;x<320;x+=64)stamp(menu,wall,x,y);
// A recessed central doorway keeps the interactive menu quiet.
rect(menu,62,79,196,322,'11101B');poly(menu,[[62,80],[78,58],[106,43],[214,43],[242,58],[258,80]],'11101B');
for(var y=90;y<405;y+=32){rect(menu,45,y,17,30,DEEP);rect(menu,46,y,2,29,LIT);rect(menu,258,y,17,30,DEEP);rect(menu,259,y,2,29,LIT);}
poly(menu,[[44,85],[64,52],[103,32],[217,32],[257,52],[277,85],[258,85],[244,64],[212,49],[108,49],[76,64],[62,85]],DEEP);
line(menu,47,80,67,52,LIT);line(menu,67,52,103,34,LIT);line(menu,103,34,216,34,LIT);line(menu,217,35,253,53,LIT);
for(var y=416;y<480;y+=16)for(var x=-32;x<320;x+=64){rect(menu,x+(y/16%2)*32+1,y+1,62,14,'211D31');line(menu,x+(y/16%2)*32+2,y+1,x+(y/16%2)*32+62,y+1,'30283F');}
// Keep the important dressing within the central 216px visible on tall phones.
stamp(menu,torchA,36,147);stamp(menu,torchB,220,147);stamp(menu,banner,36,58);stamp(menu,banner,220,58);stamp(menu,barrel,36,361);stamp(menu,skull,218,382);save(menu,'MenuDungeon');
app.open(ROOT+'DungeonMatcherLogo.aseprite');app.command.ScrollCenter();
