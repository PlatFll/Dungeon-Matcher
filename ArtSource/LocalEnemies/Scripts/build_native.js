// Execute in LibreSprite. All final pixel edits and native saves happen here.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/LocalEnemies/';
var PROJECT='C:/UnityProjects/Dungeon Matcher/';
var OUT='0A0D11',SKIN='E6B08A';
function idx(x,y,w){return (y*w+x)*4;}
function hex(a,i){return ('0'+a[i].toString(16)).slice(-2).toUpperCase()+('0'+a[i+1].toString(16)).slice(-2).toUpperCase()+('0'+a[i+2].toString(16)).slice(-2).toUpperCase();}
function put(a,w,h,x,y,c){x=Math.round(x);y=Math.round(y);if(x<0||y<0||x>=w||y>=h)return;var i=idx(x,y,w);for(var k=0;k<3;k++)a[i+k]=c?parseInt(c.slice(k*2,k*2+2),16):0;a[i+3]=c?255:0;}
function copy(a,i,b,j){for(var k=0;k<4;k++)b[j+k]=a[i+k];}
function read(path){app.open(path);var s=app.activeSprite,out=[];for(var f=0;f<s.layer(0).celCount;f++){var a=new Uint8Array(s.width*s.height*4),c=s.layer(0).cel(f),b=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)copy(b,idx(x,y,c.image.width),a,idx(x+c.x,y+c.y,s.width));out.push(a);}app.activeDocument.close();return out;}
function map(a,n,w){var b=new Uint8Array(a);for(var i=0;i<b.length;i+=4){if(b[i+3]<128){for(var k=0;k<4;k++)b[i+k]=0;continue;}var c=MAPS[n][hex(b,i)];if(!c)throw Error(n+' unmapped '+hex(b,i));var x=(i/4)%w,y=Math.floor(i/4/w);
 if(n==='Miner'&&hex(b,i)==='9B9A96'&&y<30&&x>=22&&x<=60)c='9A856F';
 if(n==='Miner'&&hex(b,i)==='E1AD81'&&y<19)c='FBEA90';
 if(n==='BasketVillager'&&hex(b,i)==='D4AF7B'&&y>=25)c=SKIN;
 put(b,w,a.length/4/w,x,y,c);}return b;}
function stamp(a,aw,ah,b,bw,bh,dx,dy){for(var y=0;y<ah;y++)for(var x=0;x<aw;x++)if(a[idx(x,y,aw)+3]&&x+dx>=0&&x+dx<bw&&y+dy>=0&&y+dy<bh)copy(a,idx(x,y,aw),b,idx(x+dx,y+dy,bw));}
function place(a,sw,sh,w,h,dx,dy){var b=new Uint8Array(w*h*4);stamp(a,sw,sh,b,w,h,dx,dy);return b;}
function blink(a,w,boxes){a=new Uint8Array(a);for(var n=0;n<boxes.length;n++){var b=boxes[n];for(var y=b[1];y<=b[3];y++)for(var x=b[0];x<=b[2];x++)put(a,w,a.length/4/w,x,y,SKIN);for(var x=b[0];x<=b[2];x++)put(a,w,a.length/4/w,x,b[3]-1,OUT);}return a;}
// The head/face and shoulders share one displacement. Compression is confined
// to the waist/knees; feet keep the original floor and no eye leads the body.
function bop(a,w,h,dx,dy,waist,floor){var b=new Uint8Array(a.length);for(var y=0;y<h;y++){var t=y<waist?1:Math.max(0,(floor-y)/(floor-waist));for(var x=0;x<w;x++)if(a[idx(x,y,w)+3]){var xx=x+Math.round(dx*t),yy=y+Math.round(dy*t);if(xx>=0&&xx<w&&yy>=0&&yy<h)copy(a,idx(x,y,w),b,idx(xx,yy,w));}}return b;}
function line(a,w,h,x0,y0,x1,y1,r,c){var n=Math.max(Math.abs(x1-x0),Math.abs(y1-y0));for(var i=0;i<=n;i++){var x=Math.round(x0+(x1-x0)*i/Math.max(n,1)),y=Math.round(y0+(y1-y0)*i/Math.max(n,1));for(var v=-r;v<=r;v++)for(var u=-r;u<=r;u++)if(u*u+v*v<=r*r+1)put(a,w,h,x+u,y+v,c);}}
function arm(a,w,h,sx,sy,ex,ey,wx,wy){line(a,w,h,sx,sy,ex,ey,2,OUT);line(a,w,h,sx,sy,ex,ey,1,'6C5A4A');line(a,w,h,sx-1,sy-1,ex,ey-1,0,'9A856F');line(a,w,h,ex,ey,wx,wy,1,OUT);line(a,w,h,ex,ey,wx,wy,0,'B9825D');}
function hand(a,w,h,x,y){for(var v=-2;v<=2;v++)for(var u=-2;u<=2;u++){if(Math.abs(v)===2&&Math.abs(u)===2)continue;put(a,w,h,x+u,y+v,Math.abs(u)===2||Math.abs(v)===2?OUT:v<1?SKIN:'B9825D');}}
function rotated(prop,sw,sh,dest,w,h,px,py,x,y,degrees){var r=degrees*Math.PI/180,c=Math.cos(r),s=Math.sin(r);for(var yy=0;yy<h;yy++)for(var xx=0;xx<w;xx++){var sx=Math.round((xx-x)*c+(yy-y)*s+px),sy=Math.round(-(xx-x)*s+(yy-y)*c+py);if(sx>=0&&sy>=0&&sx<sw&&sy<sh&&prop[idx(sx,sy,sw)+3])copy(prop,idx(sx,sy,sw),dest,idx(xx,yy,w));}}
function write(name,n,w,h,frames){var type=name.split('_')[1],template=type==='Idle'?'ArtSource/CombatIdles/PanVillager_Idle.aseprite':type==='AutoAttack'?'ArtSource/CombatActions/Farmer_AutoAttack.aseprite':'ArtSource/CombatActions/Rattlebones_Ability.aseprite';app.open(PROJECT+template);var s=app.activeSprite;s.saveAs(ROOT+'Review/'+name+'.aseprite',false);if(s.width!==w||s.height!==h)s.resize(w,h);app.command.BackgroundFromLayer();if(s.layer(0).celCount!==frames.length)throw Error('Timing template count '+name);for(var f=0;f<frames.length;f++){var cel=s.layer(0).cel(f);if(cel.x||cel.y||cel.image.width!==w||cel.image.height!==h)throw Error('Canvas template '+name);cel.image.putImageData(frames[f]);}s.layer(0).name=name;var p=s.palette,cs=PALETTES[n];p.length=cs.length+1;p.set(0,app.pixelColor.rgba(0,0,0,0));for(var j=0;j<cs.length;j++){var c=cs[j];p.set(j+1,app.pixelColor.rgba(parseInt(c.slice(0,2),16),parseInt(c.slice(2,4),16),parseInt(c.slice(4,6),16),255));}s.commit();s.save();app.command.GotoFirstFrame();}

var mi=read(ROOT+'Originals/Miner_idle.ase').map(function(a){return map(a,'Miner',80);});
var ma=read(ROOT+'Originals/Miner_attack_anim_1.ase').map(function(a){return map(a,'Miner',80);});
var mm=read(ROOT+'Originals/Miner_mine_anim_1.ase').map(function(a){return map(a,'Miner',80);});
var berry=map(read(ROOT+'Originals/Berries Farmer.png')[0],'BasketVillager',64);
var builder=map(read(ROOT+'Originals/BarricadeVillager.png')[0],'BarricadeVillager',64);
var dips=[0,0,1,2,3,3,2,1,0];
var minerIdle=[],berryIdle=[],builderIdle=[];
for(var f=0;f<9;f++){
 var m=f===5?blink(mi[0],80,[[28,34,32,36],[39,34,43,36]]):mi[0];
 minerIdle.push(place(bop(m,80,80,0,dips[f],61,71),80,80,96,80,8,8));
 var b=f===5?blink(berry,64,[[22,27,24,30],[33,27,35,30]]):berry;
 berryIdle.push(bop(b,64,64,0,dips[f],48,63));
 var c=f===5?blink(builder,64,[[23,26,25,29],[33,26,35,29]]):builder;
 builderIdle.push(bop(c,64,64,0,dips[f],48,63));
}
write('Miner_Idle','Miner',96,80,minerIdle);
write('BasketVillager_Idle','BasketVillager',64,64,berryIdle);
write('BarricadeVillager_Idle','BarricadeVillager',64,64,builderIdle);

// Side swing uses the supplied two-handed torsion and the horizontal follow-
// through, rather than the mining descent. Broad ready/impact poses read first.
var minerAttack=[minerIdle[0],place(ma[2],80,80,96,80,8,9),place(ma[7],80,80,96,80,8,8),place(ma[8],80,80,96,80,8,8),place(ma[10],80,80,96,80,3,8),place(ma[12],80,80,96,80,5,8),place(ma[16],80,80,96,80,8,8),minerIdle[0]];
write('Miner_AutoAttack','Miner',96,80,minerAttack);
var minerAbility=[minerIdle[0],minerIdle[3],place(mm[5],80,80,96,80,8,8),place(mm[12],80,80,96,80,8,4),place(mm[15],80,80,96,80,8,8),place(mm[16],80,80,96,80,8,8),place(mm[17],80,80,96,80,8,8),place(mm[3],80,80,96,80,8,8),minerIdle[2],minerIdle[0]];
write('Miner_Ability','Miner',96,80,minerAbility);

// Berry farmer's free hand gathers from the basket and releases leftwards.
// The red cluster stays attached to the hand; no flying berry is authored.
var berryBody=new Uint8Array(berry);
for(var y=40;y<=55;y++)for(var x=40;x<=49;x++)put(berryBody,64,64,x,y,null);
function berryPose(dx,dy,ex,ey,wx,wy,red){var a=place(bop(berryBody,64,64,dx,dy,49,63),64,64,96,64,16,0);arm(a,96,64,53+dx,41+dy,ex+16,ey,wx+16,wy);hand(a,96,64,wx+16,wy);if(red){line(a,96,64,wx+15,wy-3,wx+17,wy-3,1,'7C1A30');put(a,96,64,wx+16,wy-4,'C74A5A');put(a,96,64,wx+15,wy-3,'C74A5A');}return a;}
var berryAttack=[place(berry,64,64,96,64,16,0),berryPose(0,2,32,49,21,53,false),berryPose(1,1,40,43,39,37,true),berryPose(-1,0,29,36,21,35,true),berryPose(-3,1,25,39,14,39,false),berryPose(-2,2,28,43,21,46,false),berryPose(0,1,40,46,43,51,false),place(berry,64,64,96,64,16,0)];
write('BasketVillager_AutoAttack','BasketVillager',96,64,berryAttack);

// Isolate the original axe, preserving its blade, handle and exact palette.
var axe=new Uint8Array(64*64*4),builderBody=new Uint8Array(builder);
for(var y=45;y<=60;y++)for(var x=0;x<30;x++)if(x<23||y<=52){if(builder[idx(x,y,64)+3])copy(builder,idx(x,y,64),axe,idx(x,y,64));put(builderBody,64,64,x,y,null);}
function axePose(dx,dy,wx,wy,angle,kneel,plank){var source=new Uint8Array(builderBody),a=new Uint8Array(96*64*4);
 if(kneel){for(var y=56;y<64;y++)for(var x=23;x<49;x++)put(source,64,64,x,y,null);line(a,96,64,45,55,39,60,4,OUT);line(a,96,64,45,55,39,59,3,'3E312A');line(a,96,64,52,55,55,61,4,OUT);line(a,96,64,52,55,55,60,3,'6C5A4A');line(a,96,64,36,62,42,62,1,'4A2C1C');line(a,96,64,52,62,58,62,1,'4A2C1C');}
 var body=bop(source,64,64,dx,dy,49,63);stamp(body,64,64,a,96,64,16,0);
 if(plank){for(var y=58;y<=63;y++)for(var x=20;x<=44;x++)put(a,96,64,x,y,(x===20||x===44||y===58||y===63)?OUT:y===59?'B07A43':y===62?'4A2C1C':'7A4D2E');}
 arm(a,96,64,42+dx,41+dy,wx+19,wy-4,wx+16,wy);rotated(axe,64,64,a,96,64,25,50,wx+16,wy,angle);hand(a,96,64,wx+16,wy);return a;}
var builderAttack=[place(builder,64,64,96,64,16,0),axePose(1,1,29,43,45,false,false),axePose(1,2,38,29,90,false,false),axePose(-1,0,27,27,35,false,false),axePose(-3,2,18,41,-5,false,false),axePose(-2,3,21,47,-12,false,false),axePose(0,1,24,49,0,false,false),place(builder,64,64,96,64,16,0)];
write('BarricadeVillager_AutoAttack','BarricadeVillager',96,64,builderAttack);
var builderAbility=[place(builder,64,64,96,64,16,0),axePose(-1,3,24,50,0,false,false),axePose(-3,7,30,51,0,true,true),axePose(-3,7,30,38,55,true,true),axePose(-3,7,30,54,0,true,true),axePose(-3,7,30,48,20,true,true),axePose(-2,5,26,51,0,true,false),axePose(-1,3,24,51,0,false,false),axePose(0,1,24,50,0,false,false),place(builder,64,64,96,64,16,0)];
write('BarricadeVillager_Ability','BarricadeVillager',96,64,builderAbility);
app.command.ScrollCenter();
