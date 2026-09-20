// Run in LibreSprite. Read preserved cels; edit only newly saved documents.
function off(x,y,w){return (y*w+x)*4;}
function cp(a,i,b,j){for(var k=0;k<4;k++)b[j+k]=a[i+k];}
function hex(a,i){return ('0'+a[i].toString(16)).slice(-2).toUpperCase()+('0'+a[i+1].toString(16)).slice(-2).toUpperCase()+('0'+a[i+2].toString(16)).slice(-2).toUpperCase();}
function ink(a,x,y,w,h,c){if(x<0||y<0||x>=w||y>=h)return;var i=off(x,y,w);if(!c){for(var k=0;k<4;k++)a[i+k]=0;return;}a[i]=parseInt(c.slice(0,2),16);a[i+1]=parseInt(c.slice(2,4),16);a[i+2]=parseInt(c.slice(4,6),16);a[i+3]=255;}
function read(path){app.open(path);var s=app.activeSprite,out=[];for(var f=0;f<s.layer(0).celCount;f++){var a=new Uint8Array(s.width*s.height*4),c=s.layer(0).cel(f),b=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)cp(b,off(x,y,c.image.width),a,off(x+c.x,y+c.y,s.width));out.push(a);}app.activeDocument.close();return out;}
function recolor(a,n){a=new Uint8Array(a);for(var i=0;i<a.length;i+=4){if(a[i+3]<128){for(var k=0;k<4;k++)a[i+k]=0;continue;}var c=MAPS[n][hex(a,i)];if(!c)throw Error(n+' unmapped '+hex(a,i));ink(a,(i/4)%64,Math.floor(i/256),64,64,c);}return a;}
function place(a,w,dx,dy){var b=new Uint8Array(w*64*4);for(var y=0;y<64;y++)for(var x=0;x<64;x++)if(a[off(x,y,64)+3]&&x+dx>=0&&x+dx<w&&y+dy>=0&&y+dy<64)cp(a,off(x,y,64),b,off(x+dx,y+dy,w));return b;}
function swatches(s,n){var p=s.palette,cs=PALETTES[n];p.length=cs.length+1;p.set(0,app.pixelColor.rgba(0,0,0,0));for(var j=0;j<cs.length;j++){var c=cs[j];p.set(j+1,app.pixelColor.rgba(parseInt(c.slice(0,2),16),parseInt(c.slice(2,4),16),parseInt(c.slice(4,6),16),255));}}
function write(name,character,w,frames){
 app.open(ROOT+'Originals/Farmer_Idle.aseprite');var s=app.activeSprite;s.saveAs(ROOT+'Review/'+name+'.aseprite',false);
 // Resize the temporary full-canvas template; every pixel is then overwritten
 // from the unscaled source drawings. No delivered artwork is stretched.
 if(w!==64)s.resize(w,64);
 app.command.BackgroundFromLayer();
 while(s.layer(0).celCount>frames.length){app.command.GotoLastFrame();app.command.RemoveFrame();}
 while(s.layer(0).celCount<frames.length){app.command.GotoLastFrame();app.command.NewFrame();}
 for(var f=0;f<frames.length;f++){var c=s.layer(0).cel(f);if(c.image.width!==w||c.image.height!==64||c.x!==0||c.y!==0)throw Error('Unexpected template cel '+name);c.image.putImageData(frames[f]);}
 app.command.LayerFromBackground();s.layer(0).name=name;swatches(s,character);s.commit();s.save();app.command.GotoFirstFrame();
}
var farmer=read(ROOT+'Originals/Farmer_Idle.aseprite');
var panIdle=read(IDLES+'PanVillager_Idle.aseprite');
var boneIdle=read(IDLES+'Rattlebones_Idle.aseprite');
var bardIdle=read(ROOT+'Originals/Bardley_Idle_BeforeGrounding.aseprite');
var pan=read(ROOT+'Originals/panattack3.ase').map(function(a){return recolor(a,'PanVillager');});
var bone=read(ROOT+'Originals/Royal_Decree_Anim_1.ase').map(function(a){return recolor(a,'Rattlebones');});
var bard=read(ROOT+'Originals/SlimeBard_Ability.ase').map(function(a){return recolor(a,'Bardley');});
// Carry the approved spatial shading treatment through each material's pose.
// Face/eye pixels remain protected; only material ramp application changes.
function material(c,x,y){
 if('176747 4A9B3F 86C83C D5FFAD'.indexOf(c)>=0)return y>=47?'base':x<19?'hand':'slime';
 if('450635 7F1650 BD2169'.indexOf(c)>=0)return y<28?'hat':'cape';
 if('8A5622 C58826 FBEA90'.indexOf(c)>=0)return y<26&&x>18?'hatgold':x<28&&y<44?'pipe':'capegold';
 if('2E568F A9E0FF'.indexOf(c)>=0)return y<28?'hatjewel':'capejewel';
 if('B7A393 FDF5E5'.indexOf(c)>=0)return y<24?'feather':'eye';return '';
}
function groups(a){var g={};for(var y=0;y<64;y++)for(var x=0;x<64;x++){var i=off(x,y,64);if(!a[i+3])continue;var c=hex(a,i),m=material(c,x,y);if(!m)continue;if(!g[m])g[m]=[];g[m].push({x:x,y:y,c:c});}return g;}
function bounds(p){var b=[64,64,0,0];for(var i=0;i<p.length;i++){b[0]=Math.min(b[0],p[i].x);b[1]=Math.min(b[1],p[i].y);b[2]=Math.max(b[2],p[i].x);b[3]=Math.max(b[3],p[i].y);}return b;}
var referenceGroups=groups(bardIdle[0]);
for(var f=0;f<bard.length;f++){
 var a=bard[f];
 for(var y=0;y<23;y++)for(var x=0;x<26;x++){var i=off(x,y,64);if(a[i+3]&&'8A5622 C58826 FBEA90'.indexOf(hex(a,i))>=0)ink(a,x,y,64,64,'B7A393');}
 var g=groups(a);
 for(var m in g){if(m==='eye'||!referenceGroups[m])continue;var p=g[m],r=referenceGroups[m],b=bounds(p),rb=bounds(r);
  for(var j=0;j<p.length;j++){var q=p[j];if(m==='slime'&&q.x>=24&&q.x<=42&&q.y>=27&&q.y<=37&&q.c==='176747')continue;
   var rx=rb[0]+(q.x-b[0])*Math.max(1,rb[2]-rb[0])/Math.max(1,b[2]-b[0]),ry=rb[1]+(q.y-b[1])*Math.max(1,rb[3]-rb[1])/Math.max(1,b[3]-b[1]),best=1e9,c=q.c;
   for(var k=0;k<r.length;k++){var d=(r[k].x-rx)*(r[k].x-rx)+(r[k].y-ry)*(r[k].y-ry);if(d<best){best=d;c=r[k].c;}}ink(a,q.x,q.y,64,64,c);
  }
 }
}

// Complete only the clipped left edge of the impact pan from its intact ready
// drawing. Keep the existing arm, handle and visible disk pixels intact.
var panImpact=place(pan[8],96,16,0);
for(var y=37;y<=62;y++)for(var x=1;x<=26;x++){
 var xx=x-6,yy=y-8,i=off(x,y,64),dx=(x-14)/13,dy=(y-50)/12;
 if(xx<0&&dx*dx+dy*dy<=1.15&&pan[0][i+3])cp(pan[0],i,panImpact,off(xx+16,yy,96));
}
write('PanVillager_AutoAttack','PanVillager',96,[place(panIdle[0],96,16,0),place(pan[3],96,16,0),place(pan[5],96,16,0),place(pan[7],96,16,0),panImpact,place(pan[9],96,16,0),place(pan[0],96,16,0),place(panIdle[0],96,16,0)]);

// Farmer thrust: the fork and both original hand drawings form one translated
// assembly. The shoulders guide it while the hips settle over planted boots.
var ready=farmer[0],prop={},body=new Uint8Array(ready);
for(var y=23;y<61;y++)for(var x=0;x<64;x++){
 var i=off(x,y,64),shaft=Math.abs(y-(35+(x-17)*23/37))<1.65&&x>=17&&x<=55;
 var take=(x<=19&&y<=38)||(x>=21&&x<=27&&y>=38&&y<=44)||(x>=40&&x<=46&&y>=48&&y<=54)||(x>=44&&y>=54)||shaft;
 if(take&&ready[i+3]){prop[y*64+x]=true;ink(body,x,y,64,64,null);}
}
// Expose clean clothing behind the moved forearms/shaft.
for(var y=38;y<=54;y++)for(var x=18;x<=47;x++){
 if((x<25&&y>=42&&y<=48)||(x>39&&y>=41)){ink(body,x,y,64,64,null);continue;}
 if(prop[y*64+x]&&x>=25&&x<=39&&y<=52)ink(body,x,y,64,64,y>=49?'3E312A':x<30?'9A856F':'6C5A4A');
}
function line(a,x0,y0,x1,y1,r,c){var count=Math.max(Math.abs(x1-x0),Math.abs(y1-y0));for(var j=0;j<=count;j++){var x=Math.round(x0+(x1-x0)*j/Math.max(1,count)),y=Math.round(y0+(y1-y0)*j/Math.max(1,count));for(var v=-r;v<=r;v++)for(var u=-r;u<=r;u++)if(u*u+v*v<=r*r+1)ink(a,x+u,y+v,96,64,c);}}
function sleeve(a,x0,y0,x1,y1){line(a,x0,y0,x1,y1,3,'0A0D11');line(a,x0,y0,x1,y1,2,'6C5A4A');line(a,x0,y0-1,x1,y1-1,1,'9A856F');}
function thrust(bx,by,px,py){var a=new Uint8Array(96*64*4);
 for(var y=0;y<64;y++){var t=y<51?1:Math.max(0,(62-y)/11),dx=Math.round(bx*t),dy=Math.round(by*t);for(var x=0;x<64;x++)if(body[off(x,y,64)+3])cp(body,off(x,y,64),a,off(x+16+dx,y+dy,96));}
 sleeve(a,29+16+bx,39+by,24+16+px,41+py);sleeve(a,38+16+bx,40+by,43+16+px,50+py);
 for(var key in prop){var v=+key,x=v%64,y=Math.floor(v/64);cp(ready,v*4,a,off(x+16+px,y+py,96));}return a;
}
write('Farmer_AutoAttack','Farmer',96,[place(ready,96,16,0),thrust(1,1,2,1),thrust(2,1,4,2),thrust(0,0,-2,-1),thrust(-3,0,-9,-4),thrust(-2,1,-6,-2),thrust(0,1,-1,0),place(ready,96,16,0)]);

write('Rattlebones_Ability','Rattlebones',64,[boneIdle[0],bone[2],bone[5],bone[7],bone[9],bone[12],bone[14],bone[17],bone[5],boneIdle[0]]);
var groundedIdle=bardIdle.map(function(a){return place(a,64,0,12);});
write('Bardley_Idle_Grounded','Bardley',64,groundedIdle);
write('Bardley_Ability','Bardley',64,[groundedIdle[0],place(bard[1],64,0,12),place(bard[2],64,0,12),place(bard[4],64,0,12),place(bard[5],64,0,12),place(bard[8],64,0,12),place(bard[10],64,0,12),place(bard[11],64,0,12),place(bard[12],64,0,12),groundedIdle[0]]);
app.command.ScrollCenter();app.command.PlayAnimation();
