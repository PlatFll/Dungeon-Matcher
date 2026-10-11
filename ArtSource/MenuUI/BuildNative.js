// Native cel authoring in LibreSprite. References are visual targets only.
var ROOT='__ART_ROOT__', PROJECT='__PROJECT_ROOT__';
var OUT='0a0d11',DARK='141522',FACE='1e2235',MID='414052',SHADOW='626078',STEEL='aaa8ba',LIGHT='d6d5de',WHITE='f8f4ef';
function canvas(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function dot(a,x,y,c){x=Math.round(x);y=Math.round(y);if(x<0||y<0||x>=a.w||y>=a.h)return;var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=c?parseInt(c.substr(2*k,2),16):0;a.p[i+3]=c?255:0;}
function rect(a,x,y,w,h,c){for(var v=y;v<y+h;v++)for(var u=x;u<x+w;u++)dot(a,u,v,c);}
function line(a,x,y,X,Y,c,r){var n=Math.max(Math.abs(X-x),Math.abs(Y-y));for(var t=0;t<=n;t++)rect(a,Math.round(x+(X-x)*t/Math.max(n,1))-(r||0),Math.round(y+(Y-y)*t/Math.max(n,1))-(r||0),1+2*(r||0),1+2*(r||0),c);}
function poly(a,pts,c){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var b=false;for(var i=0,j=pts.length-1;i<pts.length;j=i++){var p=pts[i],q=pts[j];if((p[1]>y)!=(q[1]>y)&&x<(q[0]-p[0])*(y-p[1])/(q[1]-p[1])+p[0])b=!b;}if(b)dot(a,x,y,c);}}
function bevel(a,x,y,w,h,c){poly(a,[[x+3,y],[x+w-3,y],[x+w,y+3],[x+w,y+h-3],[x+w-3,y+h],[x+3,y+h],[x,y+h-3],[x,y+3]],c);}
function ring(a,x,y,r,c){for(var Y=-r;Y<=r;Y++)for(var X=-r;X<=r;X++)if(X*X+Y*Y<=r*r)dot(a,x+X,y+Y,c);}
function jewel(a,x,y,r){var p=[[x,y-r],[x+r,y],[x,y+r],[x-r,y]];poly(a,p,OUT);poly(a,[[x,y-r+2],[x+r-2,y],[x,y+r-2],[x-r+2,y]],STEEL);poly(a,[[x,y-r+4],[x+r-4,y],[x,y+r-4],[x-r+4,y]],'620591');poly(a,[[x,y-r+4],[x,y+1],[x-r+4,y]],'ec9aff');poly(a,[[x,y+1],[x+r-4,y],[x,y+r-4]],'a718e8');line(a,x,y-r+4,x+r-5,y-1,'fcdbff');dot(a,x,y-r+3,WHITE);}
function rivet(a,x,y){bevel(a,x-3,y-3,7,7,OUT);bevel(a,x-2,y-2,5,5,SHADOW);line(a,x-1,y-2,x+1,y-2,WHITE);dot(a,x-2,y-1,LIGHT);dot(a,x,y,STEEL);dot(a,x+1,y+2,MID);}
function wear(a,x,y,w,h){for(var n=0;n<Math.floor(w/22);n++){var X=x+8+n*22,Y=y+2+(n%3);line(a,X,Y,X+2,Y,SHADOW);dot(a,X+3,Y+1,LIGHT);}for(var n=0;n<Math.floor(w/37);n++)line(a,x+13+n*37,y+h-3,x+16+n*37,y+h-3,SHADOW);}
function frame(w,h,fill,gem){var a=canvas(w,h);bevel(a,0,0,w,h,OUT);bevel(a,2,1,w-4,h-3,SHADOW);bevel(a,3,2,w-6,h-6,LIGHT);bevel(a,5,4,w-10,h-9,STEEL);bevel(a,8,7,w-16,h-15,OUT);bevel(a,9,8,w-18,h-17,fill);line(a,12,9,w-13,9,LIGHT);line(a,11,h-10,w-12,h-10,MID);wear(a,8,3,w-16,4);rivet(a,9,6);rivet(a,w-10,6);rivet(a,9,h-7);rivet(a,w-10,h-7);if(gem){jewel(a,6,Math.floor(h/2),6);jewel(a,w-7,Math.floor(h/2),6);}return a;}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=canvas(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function stamp(a,b,x,y){for(var Y=0;Y<b.h;Y++)for(var X=0;X<b.w;X++){var i=(Y*b.w+X)*4;if(!b.p[i+3])continue;for(var k=0;k<4;k++){var j=((Y+y)*a.w+X+x)*4+k;if(X+x>=0&&X+x<a.w&&Y+y>=0&&Y+y<a.h)a.p[j]=b.p[i+k];}}}
function save(a,name){app.open(PROJECT+'Assets/_Game/Resources/UI/Consumables/Potion.png');var s=app.activeSprite;s.saveAs(ROOT+'native/'+name+'.aseprite',false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();s.layer(0).cel(0).image.putImageData(a.p);s.layer(0).name=name;s.commit();s.save();app.activeDocument.close();}
function states(a,name){save(a,name);for(var z=0;z<3;z++){var b=canvas(a.w,a.h);b.p.set(a.p);for(var i=0;i<b.p.length;i+=4){if(!b.p[i+3])continue;for(var k=0;k<3;k++){var c=b.p[i+k];b.p[i+k]=z===0?Math.min(255,c+12):z===1?Math.round(c*.73):Math.round((b.p[i]+b.p[i+1]+b.p[i+2])/3*.62);}}save(b,name+['Hover','Pressed','Disabled'][z]);}}
states(frame(224,52,'bebdc9',true),'Play');states(frame(136,44,'bebdc9',true),'Secondary');states(frame(168,44,'bebdc9',true),'Challenge');
states(frame(184,38,'0cac43',false),'Affirmative');states(frame(184,38,'514f65',false),'Neutral');states(frame(184,38,'c71540',false),'Destructive');
var plate=canvas(32,32);bevel(plate,0,0,32,32,OUT);bevel(plate,1,1,30,30,MID);bevel(plate,2,2,28,28,FACE);line(plate,3,3,28,3,SHADOW);save(plate,'IconPlate');
// Tiled native panel: authored corners remain unchanged at any extent.
var panel=canvas(48,48);rect(panel,0,0,48,48,OUT);rect(panel,2,2,44,44,SHADOW);rect(panel,3,3,42,42,LIGHT);rect(panel,4,4,40,40,STEEL);rect(panel,9,9,30,30,OUT);rect(panel,11,11,26,26,DARK);
for(var n=14;n<40;n+=12){line(panel,n,3,n,7,MID);line(panel,n,40,n,44,MID);line(panel,3,n,7,n,MID);line(panel,40,n,44,n,MID);dot(panel,n+2,4,WHITE);dot(panel,4,n+2,WHITE);}save(panel,'Panel');
var j=canvas(22,22);jewel(j,11,11,11);save(j,'CornerJewel');
var row=frame(208,32,FACE,false);save(row,'SettingsRow');
function toggle(on){var a=canvas(52,24),dark=on?'044620':MID,mid=on?'068b32':SHADOW,lit=on?'52ee5c':STEEL;
 bevel(a,0,0,52,24,OUT);bevel(a,1,1,50,22,lit);bevel(a,3,3,46,18,dark);line(a,5,4,45,4,mid);
 bevel(a,32,1,19,22,OUT);bevel(a,33,2,17,20,mid);bevel(a,35,4,13,16,lit);bevel(a,37,6,9,12,mid);line(a,35,4,37,6,WHITE);line(a,45,4,43,6,WHITE);line(a,35,19,37,17,dark);line(a,45,19,43,17,dark);dot(a,45,4,WHITE);dot(a,44,3,WHITE);dot(a,46,5,WHITE);return a;}
save(toggle(true),'ToggleOn');save(toggle(false),'ToggleOff');
function skull(crown){var a=canvas(28,32);if(crown){poly(a,[[6,7],[5,1],[10,4],[13,0],[17,4],[22,1],[21,9]],OUT);poly(a,[[7,7],[7,3],[11,6],[13,2],[16,6],[20,3],[20,8]],'c58826');line(a,8,7,19,7,'fbea90');}
 bevel(a,3,9,22,17,OUT);bevel(a,5,10,18,15,'b7a393');bevel(a,5,10,17,12,WHITE);rect(a,8,23,12,6,OUT);rect(a,9,22,3,5,WHITE);rect(a,13,22,3,5,WHITE);rect(a,17,22,2,4,'b7a393');bevel(a,7,15,6,7,OUT);bevel(a,16,15,5,7,OUT);rect(a,13,21,3,3,OUT);return a;}
save(skull(true),'CrownedSkull');save(skull(false),'Skull');
var crest=canvas(64,32);
for(var side=0;side<2;side++){var x=side?46:0;poly(crest,[[x+4,3],[x+12,4],[x+17,10],[x+13,17],[x+15,24],[x+3,26],[x,22],[x+3,16],[x+1,10]],OUT);poly(crest,[[x+5,5],[x+11,6],[x+14,10],[x+10,17],[x+12,22],[x+4,23],[x+3,20],[x+5,15],[x+3,10]],STEEL);line(crest,x+5,5,x+10,6,WHITE);line(crest,x+3,10,x+5,14,LIGHT);dot(crest,x+7,11,MID);dot(crest,x+8,19,SHADOW);}
stamp(crest,skull(false),18,1);rect(crest,29,7,3,3,WHITE);rect(crest,34,6,3,4,WHITE);save(crest,'SettingsCrest');
var divider=canvas(184,16);line(divider,2,7,76,7,OUT,1);line(divider,108,7,181,7,OUT,1);line(divider,2,6,76,6,STEEL);line(divider,108,6,181,6,STEEL);
var sk=skull(false); // Draw separator's deliberately smaller skull as native clusters.
bevel(divider,86,1,14,10,OUT);bevel(divider,87,2,12,8,STEEL);rect(divider,89,5,3,3,OUT);rect(divider,95,5,3,3,OUT);rect(divider,90,10,2,3,STEEL);rect(divider,94,10,2,3,STEEL);save(divider,'Divider');
// White utility icons, authored on one 24px grid.
function icon(name,draw){var a=canvas(24,24);draw(a,OUT,2);draw(a,WHITE,0);save(a,name);}
icon('Music',function(a,c,r){line(a,8,7,8,18,c,1+r);line(a,18,4,18,15,c,1+r);line(a,8,7,18,4,c,1+r);ring(a,5,18,2+r,c);ring(a,15,15,2+r,c);});
icon('Sfx',function(a,c,r){poly(a,[[4,9],[8,9],[13,4],[13,21],[8,16],[4,16]],c);line(a,16,8,18,10,c,r);line(a,18,10,18,15,c,r);line(a,18,15,16,17,c,r);line(a,20,5,23,9,c,r);line(a,23,9,23,16,c,r);line(a,23,16,20,20,c,r);});
icon('Motion',function(a,c,r){line(a,12,2,22,12,c,r);line(a,22,12,12,22,c,r);line(a,12,22,2,12,c,r);line(a,2,12,12,2,c,r);ring(a,12,12,2+r,c);});
icon('Vibration',function(a,c,r){line(a,7,3,17,3,c,r);line(a,7,3,7,21,c,r);line(a,17,3,17,21,c,r);line(a,7,21,17,21,c,r);line(a,3,7,2,11,c,r);line(a,2,11,3,16,c,r);line(a,21,7,22,11,c,r);line(a,22,11,21,16,c,r);});
icon('Resume',function(a,c,r){poly(a,[[5-r,3-r],[21+r,12],[5-r,21+r]],c);});
icon('Suspend',function(a,c,r){line(a,5,3,5,22,c,1+r);line(a,5,3,12,3,c,1+r);line(a,12,3,12,22,c,1+r);line(a,6,22,12,22,c,1+r);line(a,14,12,22,12,c,1+r);line(a,18,8,22,12,c,1+r);line(a,18,16,22,12,c,1+r);});
icon('Retry',function(a,c,r){for(var t=40;t<=310;t+=5){var rad=t*Math.PI/180;ring(a,12+Math.cos(rad)*8,12+Math.sin(rad)*8,1+r,c);}poly(a,[[16-r,0],[22+r,6],[15-r,9]],c);});
var gear=canvas(32,32);bevel(gear,0,0,32,32,OUT);bevel(gear,1,1,30,30,SHADOW);bevel(gear,3,2,26,27,LIGHT);bevel(gear,4,4,24,23,FACE);rivet(gear,3,3);rivet(gear,28,3);rivet(gear,3,28);rivet(gear,28,28);
for(var t=0;t<8;t++){var q=t*Math.PI/4;line(gear,16+Math.cos(q)*7,16+Math.sin(q)*7,16+Math.cos(q)*10,16+Math.sin(q)*10,LIGHT,1);}ring(gear,16,16,8,LIGHT);ring(gear,16,16,4,FACE);states(gear,'Gear');
var gem=canvas(28,28);jewel(gem,14,14,13);save(gem,'Mastery');
var swords=canvas(28,28);for(var n=0;n<2;n++){var x=n?24:3,X=n?4:23;line(swords,x,3,X,24,OUT,2);line(swords,x,3,X,23,STEEL,1);line(swords,x,3,X,21,WHITE);line(swords,n?7:16,21,n?14:23,14,'c58826',1);}save(swords,'Practice');
var cup=canvas(28,28);poly(cup,[[6,4],[22,4],[20,15],[16,19],[16,23],[22,24],[22,27],[6,27],[6,24],[12,23],[12,19],[8,15]],OUT);rect(cup,8,5,12,8,'c58826');rect(cup,9,5,4,9,'fbea90');poly(cup,[[8,11],[20,11],[18,15],[14,18],[10,15]],'c58826');rect(cup,13,18,3,6,'c58826');rect(cup,8,24,12,2,'fbea90');for(var n=0;n<2;n++){var x=n?22:4;line(cup,x,5,x,11,'c58826',1);line(cup,x,12,n?18:8,15,'c58826',1);}save(cup,'Challenges');
var knight=skull(false);bevel(knight,4,7,21,8,MID);bevel(knight,7,3,15,10,STEEL);line(knight,13,2,13,11,WHITE);line(knight,5,12,24,12,LIGHT);save(knight,'Characters');
save(read(PROJECT+'Assets/_Game/Resources/UI/Consumables/Potion.png'),'Shop');
// Native brand lettering, a distinct asset. Long UI labels remain TMP text.
var glyph={D:['11110','11011','11001','11001','11001','11011','11110'],U:['11011','11011','11011','11011','11011','11011','01110'],N:['11001','11101','11101','11011','11011','11011','11001'],G:['01111','11000','11000','11011','11001','11001','01111'],E:['11111','11000','11000','11110','11000','11000','11111'],O:['01110','11011','11011','11011','11011','11011','01110'],M:['11011','11111','11111','11011','11011','11011','11011'],A:['01110','11011','11011','11111','11011','11011','11011'],T:['11111','01110','00100','00100','00100','00100','00100'],C:['01111','11000','11000','11000','11000','11000','01111'],H:['11011','11011','11011','11111','11011','11011','11011'],R:['11110','11011','11011','11110','11100','11010','11011']};
var logo=canvas(280,124);line(logo,13,18,267,18,OUT,5);line(logo,13,16,267,16,SHADOW,3);line(logo,18,14,262,14,STEEL);
for(var x=55;x<250;x+=85){poly(logo,[[x-8,15],[x,0],[x+8,15]],OUT);poly(logo,[[x-5,14],[x,4],[x+4,14]],STEEL);line(logo,x,4,x,12,WHITE);}jewel(logo,16,13,9);jewel(logo,263,13,9);
function word(str,y,s,gold){var x=Math.floor((logo.w-(str.length*6-1)*s)/2),mask=canvas(logo.w,logo.h);
 for(var n=0;n<str.length;n++)for(var Y=0;Y<7;Y++)for(var X=0;X<5;X++)if(glyph[str[n]][Y][X]==='1')rect(mask,x+(n*6+X)*s,y+Y*s+Math.round(Math.abs(n-3)*1.3),s,s,WHITE);
 function has(X,Y){return X>=0&&X<mask.w&&Y>=0&&Y<mask.h&&mask.p[(Y*mask.w+X)*4+3];}
 for(var Y=0;Y<mask.h;Y++)for(var X=0;X<mask.w;X++)if(has(X,Y))rect(logo,X-2,Y-2,5,8,OUT);
 for(var Y=0;Y<mask.h;Y++)for(var X=0;X<mask.w;X++)if(has(X,Y))dot(logo,X+1,Y+4,gold?'8a3622':'4e3d6c');
 for(var Y=0;Y<mask.h;Y++)for(var X=0;X<mask.w;X++)if(has(X,Y)){var t=(Y-y)/(7*s),c=gold?(t<.38?'facd42':t<.7?'f39122':'d87621'):(t<.38?LIGHT:t<.7?'b6b4c6':'9c94b1');if(!has(X,Y-1)||!has(X-1,Y))c=gold?'ffe99a':WHITE;else if(!has(X,Y+1)||!has(X+1,Y))c=gold?'a74e1b':'747086';dot(logo,X,Y,c);if(!gold&&has(X,Y-2)&&has(X+2,Y)&&((X*7+Y*13)%293===0)){dot(logo,X,Y,'8e879e');dot(logo,X+1,Y,'a7a0b4');}}
}
word('DUNGEON',23,6,false);word('MATCHER',72,5,true);line(logo,49,117,232,117,OUT,3);line(logo,49,115,232,115,STEEL);jewel(logo,140,114,10);jewel(logo,37,100,8);jewel(logo,244,100,8);
// The reference's skull-shaped O, with connected jaw and dark sockets.
rect(logo,195,23,32,46,null);var skullO=canvas(32,44);bevel(skullO,0,0,31,31,OUT);bevel(skullO,2,1,27,28,STEEL);bevel(skullO,3,1,24,23,LIGHT);rect(skullO,7,27,19,10,OUT);rect(skullO,8,26,4,8,LIGHT);rect(skullO,14,26,4,9,LIGHT);rect(skullO,20,26,4,7,STEEL);bevel(skullO,5,12,9,10,OUT);bevel(skullO,18,12,9,10,OUT);rect(skullO,14,23,4,4,OUT);stamp(logo,skullO,195,26);save(logo,'Logo');
var rule=canvas(24,8);rect(rule,0,0,24,8,OUT);rect(rule,0,1,24,6,SHADOW);rect(rule,1,1,21,4,STEEL);line(rule,2,1,21,1,LIGHT);line(rule,22,2,22,5,MID);dot(rule,4,3,LIGHT);save(rule,'HeaderRule');
// Clean the generated portal's hottest tones so the unchanged hero remains focal.
var hall=read(ROOT+'candidates/Hall.png');
for(var y=170;y<300;y++)for(var x=130;x<232;x++){var k=(y*hall.w+x)*4,r=hall.p[k],g=hall.p[k+1],b=hall.p[k+2];if(b>180&&r>70&&r>b*.45&&g<r*.8){hall.p[k]=Math.round(r*.67);hall.p[k+1]=Math.round(g*.75);hall.p[k+2]=Math.round(b*.74);}}
save(hall,'Hall');
// Tall phones retain the original scene at the top. Continue the foreground
// and carpet at native resolution, taking the boundary palette from the hall.
var tall=canvas(360,800);stamp(tall,hall,0,0);
for(var y=640;y<800;y++)for(var x=0;x<360;x++){
 var edge=(639*360+x)*4,k=(y*360+x)*4,depth=y-640;
 // Draw a stepped, widening carpet between dark foreground silhouettes.
 var sampleX=Math.max(0,Math.min(359,Math.round(180+(x-180)/(1+depth/600))));
 edge=(639*360+sampleX)*4;
 for(var c=0;c<3;c++)tall.p[k+c]=hall.p[edge+c];tall.p[k+3]=255;
 // Foreground highlights resolve into connected dark shapes, never long stripes.
 if(sampleX<79||sampleX>281){
  var fade=8+Math.round(11*Math.sin(sampleX*.09))+Math.round(8*Math.cos(sampleX*.04));
  if(depth>fade+16)dot(tall,x,y,'110624');
  else if(depth>fade+8)dot(tall,x,y,'1f123c');
  else if(depth>fade)dot(tall,x,y,'271235');
 }
}
// Quiet connected wear on the extended carpet, using the scene's dark palette.
for(var y=660;y<790;y+=29){var x=130+(y%37);line(tall,x,y,x+9,y,'220735');line(tall,x+7,y+1,x+14,y+1,'220735');}
save(tall,'HallTall');
app.command.Exit();
