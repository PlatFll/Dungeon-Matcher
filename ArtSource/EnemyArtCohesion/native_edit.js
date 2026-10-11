// Pixel mutations and PNG/ASE saves run exclusively inside LibreSprite.
// Source crops are resolved from actual Unity GUID/fileID rectangles.
function copy(a,i,b,j){for(var k=0;k<4;k++)b[j+k]=a[i+k];}
function read(path){
 app.open(ROOT+path);var s=app.activeSprite,c=s.layer(0).cel(0);
 var out={w:s.width,h:s.height,p:new Uint8Array(s.width*s.height*4)},raw=new Uint8Array(c.image.getImageData());
 for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)copy(raw,(y*c.image.width+x)*4,out.p,((y+c.y)*out.w+x+c.x)*4);
 app.activeDocument.close();return out;
}
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function mirror(a){var b=blank(a.w,a.h);for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)copy(a.p,(y*a.w+x)*4,b.p,(y*a.w+a.w-1-x)*4);return b;}
function rgb(a,i){return ('0'+a.p[i].toString(16)).slice(-2)+('0'+a.p[i+1].toString(16)).slice(-2)+('0'+a.p[i+2].toString(16)).slice(-2);}
function ink(a,x,y,c){x=Math.round(x);y=Math.round(y);if(x<0||y<0||x>=a.w||y>=a.h)return;var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=c?parseInt(c.substr(k*2,2),16):0;a.p[i+3]=c?255:0;}
function line(a,x,y,X,Y,c){var n=Math.max(Math.abs(X-x),Math.abs(Y-y));for(var t=0;t<=n;t++)ink(a,x+(X-x)*t/Math.max(n,1),y+(Y-y)*t/Math.max(n,1),c);}
function rectangle(a,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)ink(a,xx,yy,c);}
function polygon(a,points,c){for(var y=0;y<a.h;y++){var xs=[];for(var i=0,j=points.length-1;i<points.length;j=i++)if((points[i][1]<=y&&points[j][1]>y)||(points[j][1]<=y&&points[i][1]>y))xs.push(points[i][0]+(y-points[i][1])*(points[j][0]-points[i][0])/(points[j][1]-points[i][1]));xs.sort(function(a,b){return a-b;});for(var i=0;i+1<xs.length;i+=2)rectangle(a,Math.ceil(xs[i]),y,Math.floor(xs[i+1])-Math.ceil(xs[i])+1,1,c);}}
function hasState(row,name){for(var i=0;i<row.states.length;i++)if(row.states[i][0]===name)return row.states[i][1];return -1;}
function components(a,color,region){var seen={},out=[];for(var y=region[1];y<region[3];y++)for(var x=region[0];x<region[2];x++){var start=y*a.w+x;if(seen[start]||!a.p[start*4+3]||rgb(a,start*4)!==color)continue;var q=[start],part=[];seen[start]=true;while(q.length){var k=q.pop(),xx=k%a.w,yy=Math.floor(k/a.w);part.push([xx,yy]);var adj=[[xx-1,yy],[xx+1,yy],[xx,yy-1],[xx,yy+1]];for(var z=0;z<4;z++){var X=adj[z][0],Y=adj[z][1],n=Y*a.w+X;if(X<region[0]||Y<region[1]||X>=region[2]||Y>=region[3]||seen[n]||!a.p[n*4+3]||rgb(a,n*4)!==color)continue;seen[n]=true;q.push(n);}}out.push(part);}return out;}
function quietSmallClusters(a,color,target,max,region){components(a,color,region||[0,0,a.w,a.h]).forEach(function(c){if(c.length<=max)c.forEach(function(p){ink(a,p[0],p[1],target);});});}
function seismicCleanup(a,row){
 // Separate the face from broad charcoal beard; keep skin, clasp and contour.
 var skin=components(a,'dca273',[44,20,86,67]),face=null;
 skin.forEach(function(c){if(c.length>4&&(!face||Math.min.apply(null,c.map(function(p){return p[1];}))<Math.min.apply(null,face.map(function(p){return p[1];}))))face=c;});
 if(face&&hasState(row,'Death')<4){
  var xs=face.map(function(p){return p[0];}),ys=face.map(function(p){return p[1];});
  var cx=Math.round((Math.min.apply(null,xs)+Math.max.apply(null,xs))/2),fy=Math.max.apply(null,ys);
  for(var y=fy+1;y<fy+17;y++)for(var x=cx-8;x<=cx+9;x++){
   var i=(y*a.w+x)*4,c=rgb(a,i),span=y<fy+11?9:7;
   if(Math.abs(x-cx)>span||!a.p[i+3])continue;
   if(['171719','22201f','333437','404246','525960','858687'].indexOf(c)>=0)ink(a,x,y,x<cx-3&&y<fy+10?'333437':'22201f');
  }
 }
 // Apron planes and hammer bevels use distinct, connected three-tone ramps.
 for(var i=0;i<a.p.length;i+=4)if(a.p[i+3]){var c=rgb(a,i),to={'171719':'22201f','404246':'333437','543c2b':'402f24'}[c];if(to)ink(a,(i/4)%a.w,Math.floor(i/4/a.w),to);}
 quietSmallClusters(a,'858687','525960',3);
}
function lanternCleanup(a){
 // Sparse shell/coffer glints remain; the pale mouth and lure stay focal.
 quietSmallClusters(a,'9f907a','5b5145',2);
 quietSmallClusters(a,'796798','4a3d68',2);
 quietSmallClusters(a,'68878a','aec0b4',2);
}
function removeDetachedHitSpecks(a){
 // The two marine hit sources contain unrelated isolated pixels above and
 // beside the actor. Keep all connected anatomy, equipment and eye details.
 var seen={},parts=[];
 for(var n=0;n<a.w*a.h;n++){
  if(seen[n]||!a.p[n*4+3])continue;
  var q=[n],part=[];seen[n]=true;
  while(q.length){var k=q.pop(),x=k%a.w,y=Math.floor(k/a.w);part.push(k);
   for(var yy=y-1;yy<=y+1;yy++)for(var xx=x-1;xx<=x+1;xx++){
    var m=yy*a.w+xx;if(xx<0||yy<0||xx>=a.w||yy>=a.h||seen[m]||!a.p[m*4+3])continue;seen[m]=true;q.push(m);
   }
  }parts.push(part);
 }
 parts.forEach(function(p){if(p.length<=4)p.forEach(function(k){ink(a,k%a.w,Math.floor(k/a.w),null);});});
}
function frameFromOriginal(job,index){
 var s=read(job.original),r=job.sprites[index].source.crop_top_left,b=blank(job.size[0],job.size[1]);
 for(var y=r[1];y<r[3];y++)for(var x=r[0];x<r[2];x++)copy(s.p,(y*s.w+x)*4,b.p,((y-r[1]+job.padding[1])*b.w+x-r[0]+job.padding[0])*4);
 return b;
}
function finishEffectTips(a,colors,depth){
 // Named clipped spell/travel cels only. Continue each existing colored
 // edge cluster to a short pointed end; add no separate particles.
 var runs=[],run=[];
 for(var y=0;y<a.h;y++){
  var i=(y*a.w+24)*4;
  if(a.p[i+3]&&colors.indexOf(rgb(a,i))>=0)run.push(y);
  else if(run.length){runs.push(run);run=[];}
 }
 if(run.length)runs.push(run);
 runs.forEach(function(ys){var mid=(ys[0]+ys[ys.length-1])/2,half=ys.length/2;
  ys.forEach(function(y){var d=Math.max(1,Math.round(depth*(1-Math.abs(y-mid)/(half+.5)))),c=rgb(a,(y*a.w+24)*4);
   for(var x=24-d;x<24;x++)ink(a,x,y,c);
  });
 });
}
function nativeEdgeDetails(a,job,row,index){
 var ability=hasState(row,'Ability'),attack=hasState(row,'AutoAttack');
 if(job.actor==='grand_delver_pilot'){
  var eject=hasState(row,'Eject'),head=eject===5?[38,18,58,40]:eject===6?[42,31,60,50]:null;
  if(head){
   // During ejection the pilot briefly turns east while the empty machine
   // stays west. Turn the complete helmet/face/beard patch within its neck
   // registration; do not reverse the machine, arms or landing trajectory.
   var old=new Uint8Array(a.p);
   for(var y=head[1];y<head[3];y++)for(var x=head[0];x<head[2];x++)copy(old,((y+20)*a.w+x+24)*4,a.p,((y+20)*a.w+head[0]+head[2]-1-x+24)*4);
  }
 }
 if(job.actor==='grand_delver_reserve'&&hasState(row,'Death')===3){
  // The repaired glove must use dull steel, not a new white sparkle.
  for(var y=91;y<130;y++)for(var x=133;x<160;x++){var i=(y*a.w+x)*4;if(a.p[i+3]&&a.p[i]>220&&a.p[i+1]>220&&a.p[i+2]>220)ink(a,x,y,'858687');}
 }
 if(job.actor==='royal_standard_bearer'&&ability>=1&&ability<=4){
  var donor=frameFromOriginal(job,0),dx=[0,6,11,13,13][ability],dy=[0,-4,-7,-9,-11][ability];
  // The banner itself is intact. Only the small steel finial crosses y=0.
  for(var y=0;y<11;y++)for(var x=8;x<14;x++)if(y+dy<2)copy(donor.p,((y+20)*donor.w+x+24)*4,a.p,((y+dy+20)*a.w+x+dx+24)*4);
 }
 if(job.actor==='siege_sergeant'&&ability===5){
  polygon(a,[[36,20],[37,17],[42,16],[48,18],[49,20]],'0a0d11');
  polygon(a,[[37,20],[38,18],[42,17],[47,19],[48,20]],'7a4d2e');
  rectangle(a,38,18,4,3,'55606e');line(a,38,18,41,17,'93a1b0');
 }
 if(job.actor==='orc_trailguard'&&attack===5){
  polygon(a,[[24,50],[22,51],[21,54],[22,58],[24,59]],'0a0d11');
  polygon(a,[[24,51],[23,52],[22,54],[23,57],[24,58]],'b07a43');
 }
 if(job.actor==='court_mage'&&ability>=10&&ability<=12)finishEffectTips(a,['23daff','0084c4','a9e0ff','fdf5e5','fefbef'],6);
 if(job.actor==='royal_swordsman'&&(attack===4||attack===5))finishEffectTips(a,['fdf5e5','fefbef','93a1b0'],8);
 if(job.actor==='barricade_guard'&&(ability===10||ability===11))finishEffectTips(a,['fbea90','c58826'],4);
 if(job.actor==='obsidian_sentinel'&&hasState(row,'SlamRelease')>=3){
  var dust=['f7ca61','a27b4a','eec265','f3c65e','ac8456'];finishEffectTips(a,dust,3);
  var reverse=mirror(a);finishEffectTips(reverse,dust,3);a.p=mirror(reverse).p;
 }
 if(job.actor==='royal_arcanist'&&(ability>=6||hasState(row,'Benediction')>=6))finishEffectTips(a,['fbea90','c58826','fdf5e5'],3);
 if(job.actor==='rivet_gunner'&&attack>=2&&attack<=5)finishEffectTips(a,['fd8a06','e77f1c','fa9112','fcfefc','d96813','faf4df'],5);
 if(job.actor==='stonewright'&&hasState(row,'OreChargedAutoAttack')>=3)finishEffectTips(a,['fad184','d79961','9d683a'],3);
 if(job.actor==='rivet_turret'&&hasState(row,'OreChargedAutoAttack')>=3&&hasState(row,'OreChargedAutoAttack')<=4){
  // Complete the cut gun muzzle rim without inventing a new flame.
  var ys=[];for(var y=20;y<a.h-4;y++){var i=(y*a.w+24)*4;if(a.p[i+3]&&['0a0d11','717c86','545f69','2d3339','363d44'].indexOf(rgb(a,i))>=0)ys.push(y);}
  if(ys.length){var top=Math.min.apply(null,ys),bottom=Math.max.apply(null,ys);
   for(var y=top;y<=bottom;y++){var i=(y*a.w+24)*4;if(a.p[i+3]){copy(a.p,i,a.p,(y*a.w+23)*4);ink(a,22,y,'0a0d11');}}
  }
 }
 if(job.actor==='town_marshal'&&ability===6){
  var edge=a.w-25,ys=[];
  for(var y=20;y<62;y++){var i=(y*a.w+edge)*4;if(a.p[i+3]&&['93a1b0','55606e','2b313a'].indexOf(rgb(a,i))>=0)ys.push(y);}
  if(ys.length){var lo=Math.min.apply(null,ys),hi=Math.max.apply(null,ys);
   for(var y=lo;y<=hi;y++){ink(a,edge+1,y,y===lo||y===hi?'0a0d11':'55606e');if(y>lo&&y<hi)ink(a,edge+2,y,'0a0d11');}
  }
 }
 if(job.actor==='puffer_sentinel'&&hasState(row,'InflatedAttack')===9){
  // An unattached vertical black seam came with this source drawing.
  var parts=components(a,'0a0d11',[24,65,40,a.h-4]);
  parts.forEach(function(p){var xs=p.map(function(q){return q[0];}),ys=p.map(function(q){return q[1];});
   if(Math.max.apply(null,xs)-Math.min.apply(null,xs)<=1&&Math.max.apply(null,ys)-Math.min.apply(null,ys)>=5)p.forEach(function(q){ink(a,q[0],q[1],null);});
  });
 }
}
function restoreRemount(a,job,row,index){
 if(job.actor!=='grand_delver_reserve'||hasState(row,'Remount')<0)return a;
 var f=hasState(row,'Remount'),ox=job.padding[0],oy=job.padding[1];
 var donor=frameFromOriginal(job,6),base=blank(a.w,a.h);
 function dolly(b,start,footBoxes){
  // One complete rigid dolly, two restrained wood planes and steel rails.
  // Preserve the original machine feet in front of it, never stretch rails.
  var body=new Uint8Array(b.p);rectangle(b,0,94+oy,b.w,b.h-94-oy,null);
  var x=start+ox,y=98+oy;
  polygon(b,[[x,y],[x+80,y],[x+89,y+5],[x+89,y+10],[x+8,y+10],[x,y+5]],'0a0d11');
  polygon(b,[[x+3,y+1],[x+79,y+1],[x+85,y+4],[x+9,y+4]],'a66a2f');
  rectangle(b,x+9,y+5,77,3,'654026');line(b,x+9,y+5,x+84,y+5,'c89349');
  line(b,x+9,y+9,x+86,y+9,'858687');line(b,x+3,y+5,x+9,y+9,'525960');
  [12,76].forEach(function(dx){polygon(b,[[x+dx-3,y+6],[x+dx+2,y+6],[x+dx+4,y+9],[x+dx+2,y+12],[x+dx-3,y+12],[x+dx-4,y+9]],'0a0d11');rectangle(b,x+dx-2,y+7,4,4,'525960');ink(b,x+dx-1,y+8,'858687');});
  footBoxes.forEach(function(box){for(var yy=94;yy<(f<=7?102:112);yy++)for(var xx=box[0];xx<box[1];xx++){var i=((yy+oy)*b.w+xx+ox)*4;if(body[i+3])copy(body,i,b.p,i);}});
 }
 if(f<=5){
  dolly(donor,31,[[31,47],[77,93]]);
  var shift=[74,62,48,34,20,6][f];
  for(var y=0;y<donor.h;y++)for(var x=0;x<donor.w-shift;x++)if(donor.p[(y*donor.w+x)*4+3])copy(donor.p,(y*donor.w+x)*4,base.p,(y*base.w+x+shift)*4);
  // Retain each pilot pose in front of the arriving empty suit. Reflect the
  // pilot locally so his face stays west during the backward boarding step.
  if(f<=3){
   var pilot=f===3?frameFromOriginal(job,2):a,cut=[98,97,79,79][f],pts=[];
   for(var y=72;y<112;y++)for(var x=0;x<cut;x++)if(pilot.p[((y+oy)*pilot.w+x+ox)*4+3])pts.push([x,y]);
   var lo=Math.min.apply(null,pts.map(function(p){return p[0];})),hi=Math.max.apply(null,pts.map(function(p){return p[0];}));
   pts.forEach(function(p){copy(pilot.p,((p[1]+oy)*pilot.w+p[0]+ox)*4,base.p,((p[1]+oy)*base.w+lo+hi-p[0]+ox)*4);});
  }
  return base;
 }
 if(f<=12)dolly(a,[0,0,0,0,0,0,31,31,43,64,84,98,104][f],f<=7?[[22,46],[76,94]]:f===8?[[13,39],[64,88]]:f===9?[[20,48],[58,82]]:f===10?[[18,45],[63,86]]:f===11?[[18,46],[64,86]]:[[18,46],[56,83]]);
 return a;
}
function mageFace(a,job,row,index){
 var f=hasState(row,'Ability');if(f<5||f>10)return a;
 if(f===9){
  // The original release turns its streaks east for one drawing. Advance
  // the adjacent westward contact drawing into this beat; keep its exact
  // registration, all repairs, face and established clip exposure.
  var s=read(job.original),rr=job.sprites[index+1],r=rr.source.crop_top_left,b=blank(a.w,a.h);
  for(var y=r[1];y<r[3];y++)for(var x=r[0];x<r[2];x++)copy(s.p,(y*s.w+x)*4,b.p,((y-r[1]+20)*b.w+x-r[0]+24)*4);
  return editFrame(b,job,rr,index+1);
 }
 var r=FOCAL[f],d=read('ArtSource/EnemyArtCohesion/Sheets/RoyalMage_Idle-789d3d3080.png'),original=new Uint8Array(a.p);
 function cyan(x,y){if(x<0||y<0||x>=a.w||y>=a.h)return false;var i=(y*a.w+x)*4;return original[i+3]&&original[i]<140&&original[i+1]>150&&original[i+2]>160;}
 for(var y=r.face[1];y<r.face[3];y++)for(var x=r.face[0];x<r.face[2];x++){
  var i=(y*a.w+x)*4,c=rgb(a,i),light=c==='fdf5e5'||c==='fefbef'||c==='a9e0ff';
  if(cyan(x,y)||(light&&(cyan(x-1,y)||cyan(x+1,y)||cyan(x,y-1)||cyan(x,y+1))))
   copy(d.p,((y+r.best.dy)*d.w+x+r.best.dx)*4,a.p,i);
 }
 return a;
}
function quietShieldEmblem(a,job,row){
 // Flatten the narrow gold/bronze engraving to one gold plane inside the
 // shield, leaving its dark silhouette, steel field and outer bevel intact.
 var ox=(job.old_size[0]-64)/2+24,oy=job.old_size[1]-64+20;
 var f=hasState(row,'Ability'),attack=hasState(row,'AutoAttack'),box=[35,29,58,54];
 if(f>=3&&f<=8)box=f===3?[51,4,64,25]:[49,-1,67,23];
 if(f===9)box=[45,21,65,49];
 if(attack>=2&&attack<=9)box=[20,29,42,56];
 for(var y=box[1]+oy;y<box[3]+oy;y++)for(var x=box[0]+ox;x<box[2]+ox;x++){
  var i=(y*a.w+x)*4,c=rgb(a,i);
  if(a.p[i+3]&&['b07a43','7a4d2e','8a5622','fbea90'].indexOf(c)>=0)ink(a,x,y,'c58826');
 }
}
function kingArc(a,job,row,index){
 var strike1=hasState(row,'JudgmentStrike1'),strike2=hasState(row,'JudgmentStrike2'),attack=hasState(row,'AutoAttack');
 if(!(strike1===4||strike2===3||attack===4))return a;
 // The following drawing exposes the same follow-through body and sword.
 // Use it beneath a narrow outer sweep, so the flash no longer replaces the
 // entire lower silhouette with a solid cream wedge.
 var src=read(job.original),r=job.sprites[index+1].source.crop_top_left,b=blank(a.w,a.h);
 for(var y=r[1];y<r[3];y++)for(var x=r[0];x<r[2];x++)copy(src.p,(y*src.w+x)*4,b.p,((y-r[1]+20)*b.w+x-r[0]+24)*4);
 for(var y=20;y<a.h-4;y++)for(var x=4;x<a.w-4;x++){
  var i=(y*a.w+x)*4,c=rgb(a,i);if(c!=='fdf5e5'&&c!=='93a1b0')continue;
  var edge=false;
  for(var k=1;k<=3;k++)if(x-k>=0&&!a.p[(y*a.w+x-k)*4+3]||y+k<a.h&&!a.p[((y+k)*a.w+x)*4+3])edge=true;
  if(edge&&(x<58||y>a.h-20))copy(a.p,i,b.p,i);
 }
 return b;
}
function pufferFace(a,row){
 if(hasState(row,'Death')>=0||hasState(row,'AutoAttack')>=0||hasState(row,'Hit')>=0)return;
 var creams=components(a,'e0d2b8',[24,20,a.w-24,a.h-4]),belly=null;
 creams.forEach(function(c){if(!belly||c.length>belly.length)belly=c;});
 if(!belly||belly.length<750)return;
 var eyes=components(a,'0a0d11',[44,26,77,56]),eye=null;
 eyes.forEach(function(c){
  var xs=c.map(function(p){return p[0];}),ys=c.map(function(p){return p[1];});
  var w=Math.max.apply(null,xs)-Math.min.apply(null,xs)+1,h=Math.max.apply(null,ys)-Math.min.apply(null,ys)+1;
  // The nearby nose/contour becomes a larger connected cluster on later
  // frames. Select the compact eye by its shape, never by area alone.
  if(c.length>=6&&c.length<=16&&w>=2&&w<=5&&h<=5&&c.length/(w*h)>.55&&(!eye||c.length>eye.length))eye=c;
 });
 if(!eye)return;
 var ex=Math.min.apply(null,eye.map(function(p){return p[0];})),ey=Math.min.apply(null,eye.map(function(p){return p[1];}));
 // Lower the same slanted eye onto the inflated cheek instead of leaving it
 // stranded at the top rim; a small left-facing mouth restores expression.
 eye.forEach(function(p){ink(a,p[0],p[1],'f0bc7c');});
 eye.forEach(function(p){ink(a,p[0]-1,p[1]+5,'0a0d11');});
 line(a,ex-7,ey+13,ex-5,ey+15,'7a4d2e');
 line(a,ex-5,ey+15,ex+1,ey+15,'7a4d2e');
 line(a,ex+1,ey+15,ex+4,ey+12,'7a4d2e');
}
function savePixels(a,path){
 app.open(ROOT+'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png');var s=app.activeSprite;
 s.saveAs(ROOT+path.replace('.png','.aseprite'),false);
 s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();
 var c=s.layer(0).cel(0);if(c.x||c.y||c.image.width!==a.w||c.image.height!==a.h)throw Error('Canvas template '+path);
 c.image.putImageData(a.p);s.commit();
 s.save();app.activeDocument.close();
}
function editFrame(a,job,row,index){
 a=restoreRemount(a,job,row,index);
 var rule=RULES[job.actor];
 if(rule){
  if(rule.map)for(var i=0;i<a.p.length;i+=4)if(a.p[i+3]){var to=rule.map[rgb(a,i)];if(to)ink(a,(i/4)%a.w,Math.floor(i/4/a.w),to);}
  (rule.clusters||[]).forEach(function(c){quietSmallClusters(a,c[0],c[1],c[2]);});
 }
 if(job.actor==='seismic_smith')seismicCleanup(a,row);
 if(job.actor==='lantern_warden')lanternCleanup(a);
 if(job.actor==='puffer_sentinel')pufferFace(a,row);
 // These three source keys reverse the entire character toward the right.
 // Reflect the complete pose about its fixed center to restore the leftward
 // staff action; do not reverse the timing or mirror individual hands.
 if(job.actor==='puffer_sentinel'&&hasState(row,'AutoAttack')>=3&&hasState(row,'AutoAttack')<=5)a=mirror(a);
 if(job.actor==='snapvine'&&hasState(row,'Death')>0){
  var f=hasState(row,'Death'),replacement=read('ArtSource/EnemyArtCohesion/Candidates/snapvine-death-'+f+'-pose.png');
  a=blank(a.w,a.h);var top=replacement.h;
  // The generated wilt has useful leftward plant acting but an unwanted
  // falling twig above it. The plant's first green plane establishes its
  // contour; remove the detached overhead object, including its dark edge.
  for(var y=0;y<replacement.h;y++)for(var x=0;x<replacement.w;x++){
   var i=(y*replacement.w+x)*4,p=replacement.p;
   if(p[i+3]&&p[i+1]>=p[i]&&p[i]>p[i+2]+8&&p[i]>40)top=Math.min(top,y);
  }
  for(var y=Math.max(0,top-1);y<replacement.h;y++)for(var x=0;x<replacement.w;x++)copy(replacement.p,(y*replacement.w+x)*4,a.p,((y+20)*a.w+x+24)*4);
 }
 var override=OVERRIDES[job.stem+'-'+index];
 if(override){
  if(override.patches)override.patches.forEach(function(patch){
   var replacement=read(patch.path),m=patch.mask;
   if(replacement.w!==a.w||replacement.h!==a.h)throw Error('Override dimensions '+job.stem);
   for(var y=m[1];y<m[1]+m[3];y++)for(var x=m[0];x<m[0]+m[2];x++)copy(replacement.p,(y*a.w+x)*4,a.p,(y*a.w+x)*4);
  });
  else {var replacement=read(override.path);if(replacement.w!==a.w||replacement.h!==a.h)throw Error('Override dimensions '+job.stem);a=replacement;}
 }
 if(job.actor==='lantern_warden'&&hasState(row,'AutoAttack')>=6&&hasState(row,'AutoAttack')<=7)a=mirror(a);
 // These death drawings turn the nose and visible eye east. Mirror the whole
 // connected pose, including the carried weapon, through the fixed anchor.
 if((job.actor==='elven_scout'||job.actor==='moray_siphoner')&&hasState(row,'Death')>=3)a=mirror(a);
 if((job.actor==='puffer_sentinel'||job.actor==='hammerhead_bruiser')&&(hasState(row,'Hit')>=0||hasState(row,'Death')===1))removeDetachedHitSpecks(a);
 if(job.actor==='court_mage')a=mageFace(a,job,row,index);
 if(job.actor==='shield_knight')quietShieldEmblem(a,job,row);
 if(job.actor==='king')a=kingArc(a,job,row,index);
 nativeEdgeDetails(a,job,row,index);
 return a;
}
try {
 JOBS.forEach(function(job){
  var src=read(job.original),w=job.size[0],h=job.size[1],frames=[],atlas=blank(w*job.sprites.length,h);
  job.sprites.forEach(function(row,f){
   var r=row.source.crop_top_left,a=blank(w,h);
   for(var y=r[1];y<r[3];y++)for(var x=r[0];x<r[2];x++)
    copy(src.p,(y*src.w+x)*4,a.p,((y-r[1]+job.padding[1])*w+x-r[0]+job.padding[0])*4);
   a=editFrame(a,job,row,f);frames.push(a);
   for(var y=0;y<h;y++)for(var x=0;x<w;x++)copy(a.p,(y*w+x)*4,atlas.p,(y*atlas.w+f*w+x)*4);
  });
  app.open(ROOT+'ArtSource/CombatIdles/PanVillager_Idle.aseprite');var s=app.activeSprite;
  s.saveAs(ROOT+'ArtSource/EnemyArtCohesion/Native/'+job.stem+'.aseprite',false);
  s.resize(w,h);s.commit();app.command.BackgroundFromLayer();s.commit();
  while(s.layer(0).celCount>frames.length){app.command.GotoLastFrame();app.command.RemoveFrame();}
  while(s.layer(0).celCount<frames.length){app.command.GotoLastFrame();app.command.NewFrame();}
  for(var f=0;f<frames.length;f++){
   var c=s.layer(0).cel(f);if(c.x||c.y||c.image.width!==w||c.image.height!==h)throw Error('Animation template '+job.stem);
   c.image.putImageData(frames[f].p);
  }
  s.layer(0).name='West-facing production';s.commit();
  s.save();app.activeDocument.close();
  console.log('EXPORTED '+job.stem);
 });
 console.log('NATIVE_SUCCESS '+JOBS.length);
}catch(e){console.log('NATIVE_ERROR '+e.toString());}
app.command.Exit();
