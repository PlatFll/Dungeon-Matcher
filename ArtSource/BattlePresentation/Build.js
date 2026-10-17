// Native reconstruction in LibreSprite. Reference fragments are reusable material,
// architectural and ornament studies; no complete screenshot enters the game.
var ROOT='__ROOT__', PROJECT='__PROJECT__', ONLY='__ONLY__';
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function px(a,x,y,c){x=Math.round(x);y=Math.round(y);if(x<0||y<0||x>=a.w||y>=a.h)return;var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=c?parseInt(c.substr(k*2,2),16):0;a.p[i+3]=c?255:0;}
function rect(a,x,y,w,h,c){for(var v=y;v<y+h;v++)for(var u=x;u<x+w;u++)px(a,u,v,c);}
function line(a,x,y,X,Y,c){var n=Math.max(Math.abs(X-x),Math.abs(Y-y));for(var t=0;t<=n;t++)px(a,x+(X-x)*t/Math.max(1,n),y+(Y-y)*t/Math.max(1,n),c);}
function inside(x,y,p){var b=false;for(var i=0,j=p.length-1;i<p.length;j=i++){var A=p[i],B=p[j];if((A[1]>y)!=(B[1]>y)&&x<(B[0]-A[0])*(y-A[1])/(B[1]-A[1])+A[0])b=!b;}return b;}
function poly(a,p,c){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(inside(x,y,p))px(a,x,y,c);}
function mask(a,p){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(!inside(x,y,p))px(a,x,y,null);return a;}
function bevel(a,x,y,w,h,c){poly(a,[[x+3,y],[x+w-3,y],[x+w,y+3],[x+w,y+h-3],[x+w-3,y+h],[x+3,y+h],[x,y+h-3],[x,y+3]],c);}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function copy(a,b,x,y,flip){for(var Y=0;Y<b.h;Y++)for(var X=0;X<b.w;X++){var i=(Y*b.w+(flip?b.w-1-X:X))*4,j=((y+Y)*a.w+x+X)*4;if(!b.p[i+3]||x+X<0||x+X>=a.w||y+Y<0||y+Y>=a.h)continue;for(var k=0;k<4;k++)a.p[j+k]=b.p[i+k];}}
function crop(a,x,y,w,h){var b=blank(w,h);for(var Y=0;Y<h;Y++)for(var X=0;X<w;X++)for(var k=0;k<4;k++)b.p[(Y*w+X)*4+k]=a.p[((y+Y)*a.w+x+X)*4+k];return b;}
var neutral=['090d16','111827','1a2336','273449','3b4659','555b6b','73798b','999dad','b8bdca','d4d9de','f1eee6'];
var violet=['111021','1b1730','27203d','372a51','4c3868','654d81','806196','a486b3','c6afd2'];
var wood=['11191b','22221d','332b23','49382a','634732','80593a','a47747','c69d63','e1bf85'];
var green=['0e2224','183830','24503a','396a42','568548','7f9e50','adc965'];
var water=['09232e','103947','164d5d','1c6474','2c7e8a','45969b','6eb5b6','9bd7d4','cef0e5'];
var fire=['562b32','90452f','d17532','fca849','ffdb7a','fff3b1'];
var magic=['073d64','0c678b','0d9ebc','42d5e5','a4f5f0'];
var palette=neutral.concat(violet,wood,green,water,fire,magic,['561742','862058','b04580','d67fb3','eab8cb','46284c','592965','843591','b338d8','e07df6']);
var rgb=palette.map(function(c){return [parseInt(c.substr(0,2),16),parseInt(c.substr(2,2),16),parseInt(c.substr(4,2),16)];});
function nearest(r,g,b){var n=0,best=1e20;for(var k=0;k<rgb.length;k++){var c=rgb[k],d=(r-c[0])*(r-c[0])+1.3*(g-c[1])*(g-c[1])+(b-c[2])*(b-c[2]);if(d<best){best=d;n=k;}}return palette[n];}
function extract(a,x,y,w,h,W,H){var b=blank(W,H);for(var Y=0;Y<H;Y++)for(var X=0;X<W;X++){var i=(Math.floor(y+(Y+.5)*h/H)*a.w+Math.floor(x+(X+.5)*w/W))*4;px(b,X,Y,nearest(a.p[i],a.p[i+1],a.p[i+2]));}return b;}
function tile(a,b,x,y,w,h){for(var Y=0;Y<h;Y++)for(var X=0;X<w;X++){var i=((Y%b.h)*b.w+X%b.w)*4,j=((y+Y)*a.w+x+X)*4;if(x+X>=a.w||y+Y>=a.h)continue;for(var k=0;k<4;k++)a.p[j+k]=b.p[i+k];}}
function save(a,name){if(name!==ONLY)return;var path=ROOT+'native/'+name+'.aseprite';app.open(PROJECT+'Assets/_Game/Resources/UI/Consumables/Potion.png');var s=app.activeSprite;s.saveAs(path,false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();s.layer(0).cel(0).image.putImageData(a.p);s.layer(0).name='Editing';s.commit();s.layer(0).name=name;s.commit();s.saveAs(path,true);app.activeDocument.close();var check=read(path);if(check.w!==a.w||check.h!==a.h)throw new Error('Size '+name);for(var i=0;i<a.p.length;i++)if(check.p[i]!==a.p[i])throw new Error('Pixel '+name+' '+i);console.log('BATTLE_ASSET_OK '+name);}
function states(a,name){save(a,name);for(var z=0;z<3;z++){var b=blank(a.w,a.h);b.p.set(a.p);for(var i=0;i<b.p.length;i+=4){if(!b.p[i+3])continue;var gray=Math.round((a.p[i]+a.p[i+1]*2+a.p[i+2])/4*.72);for(var k=0;k<3;k++)b.p[i+k]=z===0?Math.min(255,a.p[i+k]+12):z===1?Math.round(a.p[i+k]*.72):gray;}save(b,name+['Hover','Pressed','Disabled'][z]);}}
function rim(w,h){var a=blank(w,h);bevel(a,0,0,w,h,'090d16');bevel(a,1,1,w-2,h-2,'555b6b');bevel(a,2,2,w-4,h-5,'d4d9de');bevel(a,4,4,w-8,h-9,'999dad');bevel(a,7,7,w-14,h-15,'090d16');bevel(a,8,8,w-16,h-17,'1a2336');for(var x=10;x<w-10;x+=26){line(a,x,3,x+6,3,'f1eee6');line(a,x+7,4,x+9,4,'73798b');}for(var n=0;n<4;n++){var x=n%2?w-6:5,y=n>1?h-6:5;bevel(a,x-3,y-3,7,7,'273449');bevel(a,x-2,y-2,5,5,'b8bdca');px(a,x-1,y-2,'f1eee6');px(a,x+1,y+1,'555b6b');}return a;}
var d=read(ROOT+'brief/references/DUNGEON_ZONE_TARGET.png'),f=read(ROOT+'brief/references/FOREST_ZONE_TARGET.png'),c=read(ROOT+'brief/references/DROWNED_COURT_TARGET.png');
if(ONLY.indexOf('Shared/')===0){
 var guide=extract(d,660,61,158,77,88,40);mask(guide,[[7,0],[81,0],[88,6],[88,34],[81,40],[7,40],[0,34],[0,6]]);rect(guide,10,10,68,19,'1a2336');line(guide,12,11,76,11,'273449');states(guide,'Shared/Guide');
 var supply=blank(64,64);copy(supply,extract(d,141,1469,133,117,64,56),0,4);mask(supply,[[5,4],[59,4],[64,10],[64,54],[59,60],[5,60],[0,54],[0,10]]);rect(supply,10,13,45,38,'999dad');poly(supply,[[10,13],[47,13],[42,25],[20,37],[10,37]],'b8bdca');rect(supply,42,38,20,18,'999dad');line(supply,11,52,52,52,'73798b');states(supply,'Shared/Supply');
 var ability=extract(d,299,1458,351,130,176,64);mask(ability,[[9,0],[165,0],[176,9],[176,56],[168,64],[8,64],[0,56],[0,8]]);rect(ability,12,10,152,44,'16091f');line(ability,12,10,162,10,'843591');line(ability,12,53,162,53,'843591');
 for(var side=0;side<2;side++){var x=side?110:15;var pts=[[x,33],[x+10,28],[x+15,34],[x+23,32],[x+26,39],[x+39,36],[x+45,40]];for(var i=1;i<pts.length;i++)line(ability,pts[i-1][0],pts[i-1][1],pts[i][0],pts[i][1],'b338d8');px(ability,x+15,34,'e07df6');}
 states(ability,'Shared/Ability');
 var en=rim(144,32);rect(en,18,12,107,8,null);save(en,'Shared/Energy');
 var fill=blank(8,8);rect(fill,0,0,8,8,'843591');rect(fill,0,0,8,2,'e07df6');rect(fill,0,2,8,3,'b338d8');save(fill,'Shared/EnergyFill');
 var shared=blank(80,80),edge=blank(64,16);rect(edge,0,0,64,16,neutral[0]);
 for(var n=0;n<2;n++){var x=n*32;rect(edge,x+1,1,30,14,neutral[5]);rect(edge,x+2,2,28,10,neutral[7]);line(edge,x+3,2,x+28,2,neutral[9]);line(edge,x+2,3,x+2,11,neutral[8]);line(edge,x+3,13,x+28,13,neutral[3]);line(edge,x+23,9,x+26,9,neutral[6]);px(edge,x+24,8,neutral[5]);line(edge,x+8,4,x+12,4,neutral[8]);}
 tile(shared,edge,0,0,80,16);for(var y=16;y<80;y++)for(var x=0;x<16;x++)for(var k=0;k<4;k++)shared.p[(y*80+x)*4+k]=edge.p[(x*64+y%64)*4+k];
 bevel(shared,0,0,16,16,neutral[0]);bevel(shared,2,2,12,12,neutral[6]);bevel(shared,4,3,8,8,neutral[9]);rect(shared,6,6,4,4,neutral[3]);px(shared,6,5,neutral[10]);save(shared,'Shared/Corner');save(edge,'Shared/Edge');
}
function leaf(a,x,y,n){var col=green[3+n%3];poly(a,[[x,y],[x+7,y-2],[x+10,y+2],[x+4,y+8],[x,y+6]],green[1]);poly(a,[[x+1,y],[x+7,y-1],[x+8,y+2],[x+3,y+6]],col);line(a,x+2,y+3,x+6,y+1,green[4]);}
function terrain(w,h,zone){var a=blank(w,h),r=zone==='Forest'?wood:zone==='Court'?water:violet;rect(a,0,0,w,h,r[0]);for(var y=0;y<h;y+=24)for(var x=-32;x<w;x+=64){var X=x+(y%48?32:0);rect(a,X+1,y+1,61,21,r[1+(Math.floor(x/64)+Math.floor(y/24)+15)%2]);line(a,X+3,y+2,X+58,y+2,r[3]);line(a,X+60,y+3,X+60,y+20,r[0]);line(a,X+7,y+7,X+11,y+7,r[3]);}return a;}
function floor(a,y,zone){var r=zone==='Court'?water:zone==='Forest'?wood:violet;rect(a,0,y,a.w,a.h-y,r[0]);for(var Y=y;Y<a.h;Y+=24)for(var X=-48;X<a.w;X+=64){var x=X+(Y%48?32:0),n=(Math.floor(X/64)+Math.floor(Y/24)+30)%3;poly(a,[[x+9,Y+2],[x+64,Y+2],[x+53,Y+21],[x-1,Y+21]],r[2+n]);line(a,x+11,Y+3,x+60,Y+3,r[5]);line(a,x-1,Y+22,x+52,Y+22,r[1]);line(a,x+53,Y+20,x+62,Y+4,r[0]);line(a,x+20,Y+8,x+29,Y+8,r[3]);line(a,x+42,Y+15,x+48,Y+15,r[1]);}}
function crown(a,x,y,w,h,n){var r=green;poly(a,[[x,y+h*.6],[x+w*.05,y+h*.32],[x+w*.2,y+h*.32],[x+w*.2,y+h*.12],[x+w*.38,y+h*.12],[x+w*.45,y],[x+w*.63,y+h*.08],[x+w*.67,y+h*.2],[x+w*.86,y+h*.2],[x+w*.9,y+h*.42],[x+w,y+h*.5],[x+w*.95,y+h*.82],[x+w*.8,y+h],[x+w*.25,y+h],[x+w*.15,y+h*.83],[x,y+h*.84]],r[1+n%2]);
 poly(a,[[x+w*.11,y+h*.4],[x+w*.22,y+h*.4],[x+w*.22,y+h*.2],[x+w*.42,y+h*.2],[x+w*.47,y+h*.1],[x+w*.63,y+h*.18],[x+w*.64,y+h*.33],[x+w*.85,y+h*.33],[x+w*.88,y+h*.48],[x+w*.74,y+h*.6],[x+w*.48,y+h*.52],[x+w*.37,y+h*.68],[x+w*.17,y+h*.64]],r[3+n%2]);
 for(var j=0;j<5;j++){var X=x+Math.floor(w*(.2+j*.13)),Y=y+Math.floor(h*(.3+(j%2)*.15));line(a,X,Y,X+5,Y,r[4+n%2]);}}
function trunk(a,x,y,w,h){poly(a,[[x+w*.35,y],[x+w*.8,y],[x+w*.65,y+h*.52],[x+w*.75,y+h*.83],[x+w,y+h],[x+w*.2,y+h],[x,y+h*.96],[x+w*.34,y+h*.62]],wood[1]);poly(a,[[x+w*.42,y],[x+w*.65,y],[x+w*.48,y+h*.51],[x+w*.58,y+h*.91],[x+w*.72,y+h],[x+w*.25,y+h],[x+w*.25,y+h*.85]],wood[3]);line(a,x+w*.48,y+4,x+w*.4,y+h*.58,wood[5]);for(var j=1;j<8;j++){var Y=y+j*h/8;line(a,x+w*.43,Y,x+w*.52,Y+10,wood[2]);}poly(a,[[x+w*.49,y+h*.28],[x+w*.04,y+h*.16],[x-18,y+h*.08],[x-11,y+h*.05],[x+w*.2,y+h*.15],[x+w*.63,y+h*.22]],wood[2]);}
function textureWhere(a,texture,colors,minY,maxY){var allowed={};for(var n=0;n<colors.length;n++)allowed[colors[n]]=true;for(var y=minY;y<maxY;y++)for(var x=0;x<a.w;x++){var i=(y*a.w+x)*4,c='';for(var k=0;k<3;k++)c+=('0'+a.p[i+k].toString(16)).slice(-2);if(!allowed[c])continue;var j=(((y+Math.floor(x/texture.w)*7)%texture.h)*texture.w+x%texture.w)*4;for(var k=0;k<3;k++)a.p[i+k]=texture.p[j+k];}}
function zoneArt(z,ref){
 var r=z==='Forest'?wood:z==='Court'?water:violet;
 var edge=z==='Forest'?extract(ref,194,585,130,31,64,16):extract(ref,192,596,130,27,64,16);
 // Match opposite seams to two quiet pixels of continuous beam material.
 for(var y=0;y<16;y++){for(var x=0;x<2;x++)for(var k=0;k<4;k++)edge.p[(y*64+63-x)*4+k]=edge.p[(y*64+x)*4+k];}save(edge,z+'/Edge');
 var corner=blank(80,80);tile(corner,edge,0,0,80,16);for(var y=16;y<80;y++)for(var x=0;x<16;x++)for(var k=0;k<4;k++)corner.p[(y*80+x)*4+k]=edge.p[(x*64+y%64)*4+k];
 var cap=extract(ref,z==='Forest'?53:74,z==='Forest'?567:589,z==='Forest'?82:54,z==='Forest'?75:53,z==='Forest'?38:28,z==='Forest'?36:28);
 mask(cap,[[0,8],[8,0],[cap.w-8,0],[cap.w,8],[cap.w,cap.h-8],[cap.w-8,cap.h],[8,cap.h],[0,cap.h-8]]);copy(corner,cap,0,0);save(corner,z+'/Corner');
 var plaque=rim(128,32);for(var i=0;i<plaque.p.length;i+=4){if(plaque.p[i+3]&&plaque.p[i]>45&&Math.abs(plaque.p[i]-plaque.p[i+1])<40){var v=Math.floor((plaque.p[i]/255)*(r.length-1));for(var k=0;k<3;k++)plaque.p[i+k]=parseInt(r[v].substr(k*2,2),16);}}
 rect(plaque,9,7,110,18,'111827');line(plaque,10,8,117,8,r[3]);if(z==='Forest'){leaf(plaque,2,2,1);leaf(plaque,116,19,2);}save(plaque,z+'/Plaque');
 var panel=terrain(128,128,z);save(panel,z+'/Panel');
 var general=terrain(128,128,z);if(z==='Forest'){for(var x=0;x<128;x+=54){poly(general,[[x+3,0],[x+15,0],[x+9,128],[x-8,128]],wood[3]);for(var y=3;y<128;y+=25)leaf(general,x+5,y,y);}}if(z==='Court'){for(var y=16;y<128;y+=48){line(general,17,y,19,y-15,green[3]);line(general,21,y,22,y-19,green[4]);}}save(general,z+'/General');
 // Actor-free outer scenery, rebuilt as a narrow native decorative strip.
 // It never enters the board rectangle or supplies gameplay/UI content.
 var side=extract(ref,0,600,72,768,36,384);save(side,z+'/SideDressing');
 var top=blank(146,25),bottom=blank(146,25),middle=blank(146,32);
 tile(top,edge,0,0,146,16);tile(bottom,edge,0,9,146,16);
 for(var y=0;y<32;y++)for(var x=0;x<10;x++)for(var k=0;k<4;k++){var v=edge.p[(x*64+y)*4+k];middle.p[(y*146+x)*4+k]=v;middle.p[(y*146+145-x)*4+k]=v;}
 copy(top,cap,0,0);copy(top,cap,146-cap.w,0,true);copy(bottom,cap,0,0);copy(bottom,cap,146-cap.w,0,true);
 save(top,z+'/PlayerTop');save(bottom,z+'/PlayerBottom');save(middle,z+'/PlayerMiddle');
 // Individual border ornaments use hand-traced transparent silhouettes.
 var ornament;
 if(z==='Forest'){ornament=blank(56,80);for(var y=8;y<78;y+=11){line(ornament,28,y-8,25,y+3,wood[3]);leaf(ornament,20+(y%3)*3,y,Math.floor(y/8));leaf(ornament,32,y+2,1);}save(ornament,z+'/Foliage');}
 if(z==='Court'){ornament=blank(48,88);for(var n=0;n<6;n++){var x=8+n*6;poly(ornament,[[x,86],[x-2,56],[x+4,35+n%3*8],[x+1,12+n%2*15],[x+5,3+n%2*15],[x+8,30],[x+5,57],[x+5,86]],green[1+n%3]);line(ornament,x+2,79,x+3,48,green[4]);}save(ornament,z+'/Kelp');}
 var banner=extract(ref,z==='Forest'?106:107,z==='Forest'?90:85,87,127,44,64);
 mask(banner,[[0,0],[44,0],[43,53],[35,49],[24,64],[12,50],[1,57]]);save(banner,z+'/Banner');
 if(z==='Court'){
  var statue=blank(128,160);
  poly(statue,[[46,52],[78,48],[97,77],[99,117],[110,137],[27,137],[39,111],[36,83]],water[1]);
  poly(statue,[[51,54],[78,53],[89,84],[86,114],[101,133],[39,133],[48,110]],water[3]);
  poly(statue,[[61,72],[72,71],[68,111],[75,132],[52,132],[57,109]],water[4]);
  poly(statue,[[78,73],[86,82],[78,115],[90,131],[79,129],[73,112]],water[2]);
  var bust=extract(c,491,133,167,176,84,88);
  mask(bust,[[1,48],[11,42],[14,17],[19,6],[18,26],[23,30],[25,16],[29,2],[32,3],[29,30],[35,27],[38,6],[40,1],[42,5],[37,36],[25,38],[24,51],[32,48],[34,36],[41,30],[42,20],[45,16],[45,9],[51,15],[57,7],[57,14],[63,10],[64,16],[69,16],[65,23],[68,31],[65,41],[71,43],[82,52],[84,75],[78,88],[22,88],[10,70],[0,67]]);
  // Restore the shoulder hidden by the reference's sword/counter UI.
  poly(bust,[[68,51],[79,54],[83,64],[83,86],[63,87],[67,72]],water[2]);
  poly(bust,[[70,51],[78,57],[78,69],[74,80],[71,76],[72,65]],water[3]);
  line(bust,73,55,77,59,water[4]);copy(statue,bust,24,4);
  line(statue,52,97,47,122,water[2]);line(statue,87,106,92,123,water[1]);
  poly(statue,[[26,135],[102,135],[109,139],[20,139]],water[5]);rect(statue,20,140,89,8,water[2]);
  poly(statue,[[20,145],[109,145],[117,151],[12,151]],water[4]);rect(statue,12,152,105,6,water[1]);
  line(statue,30,143,48,143,water[3]);line(statue,81,153,112,153,water[2]);
  // The statue is all marine stone; remove occluding pale HUD flecks in the study.
  for(var i=0;i<statue.p.length;i+=4)if(statue.p[i+3] && (statue.p[i]>100 || statue.p[i]>statue.p[i+1]))
   for(var k=0;k<3;k++)statue.p[i+k]=parseInt(water[3].substr(k*2,2),16);
  save(statue,'Court/Statue');
 }
}
var z=ONLY.split('/')[0];if(z==='Dungeon'||z==='Forest'||z==='Court')zoneArt(z,z==='Dungeon'?d:z==='Forest'?f:c);
// Scenery is assembled from independent architectural pieces over repeated native
// material patches. Targets' actors, labels, gems and UI are never sampled here.
if(ONLY.indexOf('Scene/')===0){
 var dz=ONLY.split('/')[1],scene=terrain(512,384,dz),ref=dz==='Dungeon'?d:dz==='Forest'?f:c;
 if(dz==='Dungeon'){
  var wall=extract(d,33,2,176,48,88,24);tile(scene,wall,0,0,512,256);
  var arch=extract(d,365,130,261,105,131,53),pier=extract(d,369,182,30,46,15,23);
  rect(scene,256,90,100,166,'111021');
  for(var x=260;x<355;x+=12){rect(scene,x,91,3,165,'27203d');line(scene,x,93,x,252,'4c3868');}
  for(var y=139;y<246;y+=46){rect(scene,257,y,100,5,'090d16');line(scene,258,y+1,355,y+1,'555b6b');for(var x=262;x<355;x+=20)px(scene,x,y+2,'999dad');}
  copy(scene,arch,241,54);tile(scene,pier,241,104,15,152);tile(scene,pier,356,104,15,152);
  tile(scene,extract(d,480,550,330,43,165,22),0,256,512,128);
  var banner=extract(d,320,140,47,119,24,60);copy(scene,banner,207,108);copy(scene,banner,389,108);
  var pillar=extract(d,704,301,51,49,26,25);tile(scene,pillar,478,0,26,256);tile(scene,pillar,158,0,26,256);
  var bones=extract(d,738,375,61,52,31,26);mask(bones,[[5,1],[27,0],[31,11],[29,26],[0,24],[2,12]]);copy(scene,bones,446,261);
 }else if(dz==='Forest'){
  var woodland=read(PROJECT+'ArtSource/Forest/Production/Selected/Environment/Tall_Woodland.png');
  tile(scene,woodland,0,0,512,256);
  // Existing approved native woodland remains a clean depth layer. Recompose
  // the clearing and add the target's river and broad foreground tree forms.
  copy(scene,woodland,200,0);
  // A connected rocky cascade; avoid rectangular landscape patches in the vista.
  poly(scene,[[357,103],[395,98],[405,127],[397,151],[414,173],[400,190],[392,219],[380,238],[371,256],[321,256],[335,231],[342,214],[344,183],[355,169],[346,143]],'273449');
  poly(scene,[[357,108],[387,103],[400,128],[383,147],[399,170],[387,191],[374,206],[373,228],[348,249],[334,250],[348,225],[352,201],[358,180],[369,162],[358,141]],'3b4659');
  poly(scene,[[368,109],[385,109],[381,141],[388,167],[374,187],[372,214],[362,233],[343,256],[330,256],[350,228],[354,207],[358,181],[369,158],[365,140]],water[3]);
  poly(scene,[[374,111],[381,110],[375,141],[382,166],[368,186],[367,213],[357,232],[339,253],[334,253],[352,227],[360,210],[363,182],[375,163],[369,140]],water[6]);
  var fall=extract(f,700,185,28,44,14,22);mask(fall,[[3,0],[13,0],[11,22],[1,22],[0,15]]);copy(scene,fall,369,113);
  for(var n=0;n<4;n++){var X=[366,375,359,347][n],Y=139+n*28;line(scene,X-2,Y,X+12,Y,water[8]);line(scene,X+1,Y+2,X+16,Y+2,water[5]);px(scene,X-4,Y-1,water[6]);leaf(scene,X+18,Y-2,n);}
  var ancient=read(PROJECT+'ArtSource/Forest/Production/Selected/Environment/Ancient_Tree.png');
  copy(scene,ancient,169,128);copy(scene,ancient,411,128,true);
  var canopy=read(PROJECT+'ArtSource/Forest/Production/Selected/Environment/Distant_Canopy.png');tile(scene,crop(canopy,0,0,256,28),0,0,512,28);
  tile(scene,read(PROJECT+'ArtSource/Forest/Production/Selected/Environment/Woodland_Ground.png'),0,256,512,128);
  var fern=read(PROJECT+'ArtSource/Forest/Production/Selected/Environment/Forest_Fern.png');
  copy(scene,fern,177,248);copy(scene,fern,448,260,true);
 }else{
  var court=read(PROJECT+'Assets/_Game/Art/DrownedCourt/court_background.png');
  rect(scene,0,0,512,256,water[2]);
  poly(scene,[[0,0],[512,0],[512,49],[475,67],[429,53],[382,93],[347,78],[295,120],[260,84],[219,95],[182,66],[137,88],[96,53],[45,71],[0,45]],water[3]);
  poly(scene,[[183,0],[422,0],[402,26],[380,19],[343,42],[307,31],[276,55],[246,38],[216,47]],water[4]);
  poly(scene,[[314,0],[345,0],[237,224],[223,224]],water[3]);poly(scene,[[417,0],[439,0],[352,189],[336,189]],water[3]);
  for(var x=178;x<510;x+=97){poly(scene,[[x,62],[x+17,55],[x+40,64],[x+37,73],[x+29,73],[x+24,219],[x+9,219],[x+15,74],[x,73]],water[1]);line(scene,x+21,78,x+18,218,water[2]);}
  copy(scene,court,217,128);
  // The complete freestanding reference-derived statue replaces the central
  // gate silhouette while the original court masonry supplies recessed depth.
  copy(scene,read(ROOT+'native/Court/Statue.png'),284,94);
  var cap=extract(c,677,139,85,45,43,23);mask(cap,[[5,0],[35,0],[43,6],[43,16],[36,23],[7,23],[0,17],[0,7]]);
  var column=extract(c,696,190,52,45,26,23);
  for(var n=0;n<2;n++){var x=n?439:182;tile(scene,column,x+7,78+n*5,26,179-n*5);copy(scene,cap,x,55+n*5);}
  tile(scene,read(PROJECT+'Assets/_Game/Art/DrownedCourt/court_floor.png'),0,256,512,128);
  var coral=extract(c,869,456,69,79,35,40);mask(coral,[[18,0],[35,4],[35,40],[0,40],[0,27],[8,21],[9,7]]);copy(scene,coral,454,246);copy(scene,coral,151,280,true);
 }
 save(scene,'Scene/'+dz);
}
if(ONLY.indexOf('Motion/')===0){
 var flame=blank(24*6,40);for(var n=0;n<6;n++){var a=blank(24,40),s=[0,1,0,-1,-1,0][n];poly(a,[[6,31],[3,24],[7,18],[8,10],[13+s,2],[13+s,13],[18,19],[19,28],[16,33]],fire[2]);poly(a,[[8,30],[6,24],[11+s,16],[12,10],[14,23],[17,26],[14,33]],fire[4]);poly(a,[[10,31],[9,27],[13,22],[14,29],[13,33]],fire[5]);rect(a,3,33,18,3,neutral[3]);rect(a,6,36,12,3,neutral[1]);px(a,17+n%3,5+n*2,fire[4]);copy(flame,a,n*24,0);}save(flame,'Motion/Torch');
 var lantern=extract(f,826,170,60,91,30,46);mask(lantern,[[13,0],[17,0],[17,8],[24,13],[27,18],[24,21],[24,37],[27,40],[20,43],[17,46],[12,46],[10,43],[3,40],[7,37],[7,21],[2,18],[5,13],[13,8]]);save(lantern,'Motion/Lantern');
 var rune=blank(32,40);var pts=[[16,2],[4,19],[16,36],[27,23],[18,14],[12,20],[18,25]];for(var n=1;n<pts.length;n++){line(rune,pts[n-1][0],pts[n-1][1],pts[n][0],pts[n][1],magic[2]);line(rune,pts[n-1][0]+1,pts[n-1][1],pts[n][0]+1,pts[n][1],magic[4]);}save(rune,'Motion/Rune');
 var bubble=blank(12*6,12);for(var n=0;n<6;n++){var a=blank(12,12);if(n<4){var r=3+n%2;for(var t=0;t<360;t+=15){var rad=t*Math.PI/180;px(a,6+Math.cos(rad)*r,6+Math.sin(rad)*r,water[t<160?4:7]);}px(a,4,3,water[8]);}else{var r=n===4?4:5;for(var t=0;t<4;t++)px(a,6+(t%2?r:-r),6+(t<2?r:-r),water[n===4?7:5]);}copy(bubble,a,n*12,0);}save(bubble,'Motion/Bubble');
 var flow=blank(64*6,96);for(var n=0;n<6;n++){var a=blank(64,96);for(var y=-24;y<96;y+=24){var Y=y+n*4,X=Math.round(46-Y*.28);line(a,X,Y,X-2,Y+7,water[7]);line(a,X+3,Y+3,X+1,Y+11,water[6]);line(a,X-3,Y+10,X+1,Y+10,water[8]);}copy(flow,a,n*64,0);}save(flow,'Motion/Waterfall');
}
