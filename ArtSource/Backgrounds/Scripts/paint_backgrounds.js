// Background-only native LibreSprite authoring. Run from its Scripts menu.
// References are opened for inspection; no UI or character pixels are extracted.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/Backgrounds/';
var SEED='C:/UnityProjects/Dungeon Matcher/ArtSource/Presentation/DungeonBackdropTile.png';
function canvas(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function dot(a,x,y,c){x=Math.round(x);y=Math.round(y);if(x<0||y<0||x>=a.w||y>=a.h)return;var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=c?parseInt(c.substr(k*2,2),16):0;a.p[i+3]=c?255:0;}
function box(a,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)dot(a,xx,yy,c);}
function line(a,x,y,X,Y,c){var n=Math.max(Math.abs(X-x),Math.abs(Y-y));for(var t=0;t<=n;t++)dot(a,x+(X-x)*t/Math.max(n,1),y+(Y-y)*t/Math.max(n,1),c);}
function poly(a,p,c){for(var y=0;y<a.h;y++){var xs=[];for(var i=0,j=p.length-1;i<p.length;j=i++)if((p[i][1]<=y&&p[j][1]>y)||(p[j][1]<=y&&p[i][1]>y))xs.push(p[i][0]+(y-p[i][1])*(p[j][0]-p[i][0])/(p[j][1]-p[i][1]));xs.sort(function(a,b){return a-b;});for(var i=0;i+1<xs.length;i+=2)box(a,Math.ceil(xs[i]),y,Math.floor(xs[i+1])-Math.ceil(xs[i])+1,1,c);}}
function oval(a,x,y,rx,ry,c){for(var yy=-ry;yy<=ry;yy++)for(var xx=-rx;xx<=rx;xx++)if(xx*xx/(rx*rx)+yy*yy/(ry*ry)<=1)dot(a,x+xx,y+yy,c);}
function stamp(a,b,x,y){for(var yy=0;yy<b.h;yy++)for(var xx=0;xx<b.w;xx++){var i=(yy*b.w+xx)*4,j=((yy+y)*a.w+xx+x)*4;if(xx+x<0||xx+x>=a.w||yy+y<0||yy+y>=a.h||!b.p[i+3])continue;for(var k=0;k<4;k++)a.p[j+k]=b.p[i+k];}}
// Save native transactions here; finish_backgrounds.js and the native batch
// exporter perform the transparent-layer conversion/export in separate passes.
function save(a,name){app.open(SEED);var s=app.activeSprite;s.saveAs(ROOT+name+'.aseprite',false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();s.layer(0).cel(0).image.putImageData(a.p);s.layer(0).name=name;s.commit();s.save();app.activeDocument.close();console.log('BACKGROUND '+name);}
var INK='11121B',MORTAR='1B1925',SH='252331',STONE='302D3B',FACE='393544',EDGE='4B4351',WEAR='5C5060';
var WSH='302625',WOOD='493328',WL='654936',WH='806044',GOLD='8E7048';
function stone(a,x,y,w,h,seed,quiet){
 var face=quiet?'201D2B':seed%3===0?STONE:FACE,edge=quiet?'2E283C':EDGE,shadow=quiet?'191723':SH;
 poly(a,[[x+2,y],[x+w-3,y],[x+w-1,y+2],[x+w-1,y+h-3],[x+w-3,y+h-1],[x+1,y+h-1],[x,y+h-3],[x,y+2]],shadow);
 poly(a,[[x+2,y+1],[x+w-4,y+1],[x+w-2,y+3],[x+w-3,y+h-4],[x+w-5,y+h-2],[x+2,y+h-2],[x+1,y+h-4],[x+1,y+3]],face);
 line(a,x+3,y+1,x+w-5,y+1,edge);line(a,x+1,y+3,x+1,y+7,edge);
 if(!quiet && seed%3!==0){
  // Connected worn planes on the upper-left and bottom-right of each block.
  poly(a,[[x+3,y+3],[x+9+seed%4,y+2],[x+7,y+5],[x+3,y+7]],seed%2?STONE:EDGE);
  poly(a,[[x+w-8,y+h-4],[x+w-3,y+h-7],[x+w-2,y+h-3],[x+w-5,y+h-2]],SH);
  if(seed%4===0){line(a,x+13,y+3,x+17,y+3,EDGE);box(a,x+18,y+h-5,5,2,STONE);}
 }
 if(seed%3===1){line(a,x+w-9,y+h-3,x+w-4,y+h-3,shadow);dot(a,x+w-3,y+h-4,shadow);}
 if(seed%5===2){box(a,x+5,y+5,4,2,shadow);line(a,x+6,y+4,x+9,y+4,face);}
 if(seed%7===3){line(a,x+w-9,y+2,x+w-12,y+5,shadow);line(a,x+w-12,y+5,x+w-10,y+8,shadow);}
}
function masonry(w,h,seed,quiet){var a=canvas(w,h);box(a,0,0,w,h,quiet?'15131F':MORTAR);for(var r=0;r<h/16;r++)for(var c=-1;c<w/32+1;c++)stone(a,c*32+(r%2)*16+1,r*16+1,30,14,seed+r*7+c*3,quiet);return a;}
var walls=[];for(var n=0;n<4;n++){var a=masonry(64,64,n*11,false);walls.push(a);save(a,'Modules/Wall'+String.fromCharCode(65+n));}
// 128px repeat distributes chips over sixteen courses without repetitive cracks.
var surround=masonry(128,128,17,true);save(surround,'GeneralMasonry');
var floors=[];
for(var n=0;n<4;n++){
 var a=canvas(64,64);box(a,0,0,64,64,'302C35');
 for(var r=0;r<3;r++)for(var c=-1;c<3;c++){
  var x=c*32+(r%2)*16,y=r*16;
  poly(a,[[x+4,y+1],[x+31,y+1],[x+27,y+14],[x,y+14]],n%2?'40373A':'39333A');
  line(a,x+5,y+1,x+29,y+1,'5A4842');line(a,x+3,y+3,x,y+12,'4C4042');
  line(a,x+2,y+14,x+25,y+14,'252331');
  if((r+c+n)%4===0){line(a,x+19,y+2,x+16,y+7,'292630');line(a,x+16,y+7,x+19,y+10,'292630');}
  if((r+c+n)%3===1)line(a,x+8,y+9,x+14,y+9,'4C4042');
 }
 // The platform front is below the source-row-48 standing baseline.
 for(var c=-1;c<3;c++)stone(a,c*32+1,49,30,14,n+c,false);
 line(a,0,48,63,48,'685448');line(a,0,49,63,49,'49404A');
 // Shared horizontal boundaries make all four variants interchangeable.
 for(var y=0;y<64;y++)for(var x=0;x<2;x++){var col=y<48?(y%16===0?MORTAR:'39333A'):y===48?'685448':y===49?'49404A':y<61?STONE:MORTAR;dot(a,x,y,col);dot(a,63-x,y,col);}
 floors.push(a);save(a,'Modules/Floor'+String.fromCharCode(65+n));
}
var foundation=masonry(64,64,21,true);save(foundation,'Modules/Foundation');
function flame(a,x,y){poly(a,[[x-6,y+8],[x-8,y+1],[x-4,y-7],[x-2,y-14],[x+1,y-8],[x+3,y-12],[x+4,y-3],[x+7,y+2],[x+5,y+9]],'A6522C');poly(a,[[x-4,y+7],[x-5,y],[x-1,y-9],[x,y-3],[x+3,y-6],[x+4,y+4],[x+2,y+8]],'E99646');poly(a,[[x-2,y+7],[x-2,y+1],[x+1,y-4],[x+1,y+1],[x+3,y+5],[x+1,y+8]],'F9D486');dot(a,x,y+4,'FFF0B7');}
var torch=canvas(32,64);box(torch,13,26,8,30,INK);box(torch,14,27,5,26,'39333A');line(torch,14,28,14,49,'685448');box(torch,6,30,21,5,INK);box(torch,7,30,18,2,'806044');box(torch,9,35,15,5,'302625');line(torch,11,40,15,51,INK);line(torch,23,40,19,51,INK);flame(torch,16,22);save(torch,'Modules/Torch');
function skull(a,x,y,small){var r=small?6:9;oval(a,x,y,r,r-2,INK);oval(a,x,y-1,r-1,r-3,'6D5A4A');oval(a,x-2,y-2,r-3,r-4,'8B745C');box(a,x-r+3,y+2,r*2-5,5,INK);box(a,x-r+4,y+2,r*2-7,4,'6D5A4A');box(a,x-r+2,y-1,small?3:4,3,INK);box(a,x+2,y-1,small?3:4,3,INK);dot(a,x,y+2,INK);for(var t=x-r+5;t<x+r-3;t+=3)line(a,t,y+4,t,y+6,INK);}
function banner(crossed){var a=canvas(48,96);box(a,5,4,39,4,INK);line(a,7,4,41,4,'685448');box(a,6,2,3,8,WSH);box(a,39,2,3,8,WSH);poly(a,[[9,8],[39,8],[39,79],[34,86],[30,83],[25,89],[21,85],[16,90],[9,83]],INK);poly(a,[[11,8],[37,8],[37,78],[33,82],[29,80],[25,85],[20,81],[16,85],[11,80]],'30233F');box(a,13,10,3,62,'47324F');box(a,33,9,3,65,'241E31');line(a,12,77,16,82,GOLD);line(a,17,82,20,78,GOLD);line(a,21,78,25,82,GOLD);line(a,26,82,29,77,GOLD);line(a,30,77,33,80,GOLD);line(a,34,80,36,76,GOLD);
 if(crossed){line(a,17,29,32,48,'806044');line(a,32,29,17,48,'806044');line(a,15,43,21,49,GOLD);line(a,28,48,34,42,GOLD);line(a,16,28,19,28,GOLD);line(a,31,28,34,28,GOLD);}else{skull(a,24,37,false);}
 return a;}
var bannerA=banner(true),bannerB=banner(false);save(bannerA,'Modules/BannerSwords');save(bannerB,'Modules/BannerSkull');
var gate=canvas(112,160);
poly(gate,[[9,158],[9,56],[13,39],[26,20],[44,9],[66,8],[87,20],[100,39],[104,56],[104,158]],INK);
poly(gate,[[23,155],[23,57],[27,42],[38,31],[49,26],[65,26],[77,34],[89,48],[91,61],[91,155]],'12131C');
for(var y=58;y<155;y+=16){stone(gate,8,y,16,15,3+y,false);stone(gate,91,y,16,15,4+y,false);}
var wedge=[[[9,56],[11,40],[24,44],[23,56]],[[12,37],[20,24],[31,34],[26,42]],[[22,21],[36,12],[42,26],[33,32]],[[39,10],[54,6],[55,23],[44,25]],[[57,6],[72,10],[67,26],[58,23]],[[75,12],[88,21],[77,34],[70,27]],[[90,24],[100,37],[87,43],[79,35]],[[102,40],[105,57],[92,57],[89,45]]];
for(var i=0;i<wedge.length;i++){poly(gate,wedge[i],i<4?EDGE:STONE);line(gate,wedge[i][0][0]+1,wedge[i][0][1]+1,wedge[i][1][0]-1,wedge[i][1][1]+1,i<4?'5C5060':EDGE);}
for(var x=33;x<91;x+=13){var y=39+Math.abs(x-58)/3;box(gate,x,y,4,153-y,INK);box(gate,x,y,2,149-y,'3D3741');line(gate,x,y,x,143,'55454A');}
for(var y=65;y<147;y+=55){box(gate,23,y,68,5,INK);box(gate,24,y,65,2,'4A3F48');}
box(gate,60,90,9,12,INK);box(gate,61,91,7,9,'493D40');dot(gate,64,94,'171622');line(gate,64,95,64,98,INK);stone(gate,5,153,103,7,2,false);save(gate,'Modules/BarredGate');
var shrine=canvas(64,112);
poly(shrine,[[8,95],[8,44],[14,23],[31,7],[47,23],[55,44],[55,96]],INK);
poly(shrine,[[13,92],[13,44],[19,25],[31,14],[42,25],[49,44],[49,92]],'302D3B');
poly(shrine,[[19,89],[19,43],[24,31],[31,23],[37,31],[43,44],[43,89]],'1B1925');
line(shrine,13,42,19,26,'5C5060');line(shrine,19,26,31,14,'5C5060');
skull(shrine,31,43,false);poly(shrine,[[23,50],[38,50],[44,71],[41,87],[20,87],[18,71]],'3C343B');box(shrine,25,56,12,22,'53434A');line(shrine,31,54,31,78,'27222C');line(shrine,25,60,36,60,'27222C');line(shrine,25,66,36,66,'27222C');
stone(shrine,7,87,50,10,1,false);stone(shrine,3,99,58,11,4,false);line(shrine,8,101,50,101,'685448');save(shrine,'Modules/BoneShrine');
function crate(a,x,y,w,h){box(a,x+2,y+3,w,h,INK);box(a,x,y,w,h,WSH);box(a,x+2,y+2,w-4,h-4,WOOD);for(var xx=x+5;xx<x+w-2;xx+=8){line(a,xx,y+3,xx,y+h-4,WSH);line(a,xx+1,y+3,xx+1,y+h-6,WL);}box(a,x,y,w,3,WL);box(a,x,y,3,h,WL);box(a,x+w-3,y,3,h,WSH);box(a,x,y+h-3,w,3,WSH);line(a,x+4,y+h-5,x+w-5,y+4,WL);line(a,x+5,y+h-5,x+w-4,y+4,WH);for(var xx=x+2;xx<x+w;xx+=w-5){dot(a,xx,y+1,'796B5C');dot(a,xx,y+h-2,INK);}}
var crates=canvas(80,72);crate(crates,4,32,39,37);crate(crates,38,24,35,44);crate(crates,22,1,35,28);save(crates,'Modules/CrateStack');
function candle(a,x,y,h){box(a,x-1,y,7,h,WSH);box(a,x,y,5,h,'A88A61');line(a,x,y,x,y+h-2,'D8B47A');line(a,x+2,y+1,x+2,y+4,'EDD091');line(a,x+4,y+1,x+4,y+7,'D8B47A');line(a,x+2,y-3,x+2,y,INK);poly(a,[[x,y-4],[x,y-7],[x+2,y-13],[x+4,y-7],[x+4,y-4]],'E99646');line(a,x+2,y-8,x+2,y-4,'FFF0B7');}
var candles=canvas(40,56);oval(candles,20,51,17,3,'1B1925');candle(candles,8,23,28);candle(candles,20,33,18);candle(candles,28,37,14);save(candles,'Modules/Candles');
var bones=canvas(56,40);oval(bones,27,35,25,3,'1B1925');skull(bones,18,24,false);skull(bones,35,17,false);line(bones,8,34,31,30,'6D5A4A');line(bones,38,32,48,35,'53434A');save(bones,'Modules/Skulls');
var chain=canvas(16,128);for(var y=0;y<128;y+=12){box(chain,6,y,5,8,INK);line(chain,6,y+1,6,y+6,'645353');line(chain,7,y,9,y,'806044');box(chain,8,y+7,3,7,'1B1925');line(chain,8,y+8,8,y+12,'493D40');}save(chain,'Modules/Chain');
// Assemble the wall from reusable modules; keep the central lower actor band quiet.
var wall=canvas(512,256);for(var y=0;y<256;y+=64)for(var x=0;x<512;x+=64)stamp(wall,walls[((x/64)*3+y/64)%4],x,y);
// Hard-edged palette lighting on the existing stone clusters, never blurred halos.
var cool=[MORTAR,SH,STONE,FACE,EDGE,WEAR],warm=['211C25','2C2630','3A2F37','44353B','5B4648','6C5048'];
for(var y=0;y<256;y++)for(var x=0;x<512;x++){var d=Math.min(Math.pow((x-167)/43,2)+Math.pow((y-189)/59,2),Math.pow((x-331)/43,2)+Math.pow((y-189)/59,2));if(d>1.2)continue;var i=(y*512+x)*4,c='';for(var k=0;k<3;k++)c+=('0'+wall.p[i+k].toString(16)).slice(-2);var index=cool.indexOf(c.toUpperCase());if(index>=0&&(d<.64||index<3))dot(wall,x,y,warm[index]);}
// The current short-phone HUD covers the upper courses. Keep focal accents lower.
stamp(wall,gate,393,96);stamp(wall,shrine,181,144);stamp(wall,bannerA,270,147);stamp(wall,bannerB,40,147);stamp(wall,torch,151,162);stamp(wall,torch,315,162);stamp(wall,crates,345,183);stamp(wall,candles,424,201);stamp(wall,bones,447,220);stamp(wall,chain,376,0);stamp(wall,chain,115,0);
save(wall,'BattlegroundWall');
var floor=canvas(512,64);for(var x=0;x<512;x+=64)stamp(floor,floors[(x/64)%4],x,0);save(floor,'BattlegroundFloor');
var scene=canvas(512,384);stamp(scene,wall,0,0);stamp(scene,floor,0,256);for(var x=0;x<512;x+=64)stamp(scene,foundation,x,320);save(scene,'BattlegroundScene');
var general=canvas(384,512);for(var y=0;y<512;y+=128)for(var x=0;x<384;x+=128)stamp(general,surround,x,y);save(general,'GeneralBackgroundPreview');
app.open(ROOT+'References/TopBattleground.png');app.open(ROOT+'References/GeneralBackground.png');app.open(ROOT+'BattlegroundScene.aseprite');app.command.ScrollCenter();
