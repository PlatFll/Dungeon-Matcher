// Concatenate cast_spec.js before running this script in LibreSprite.
// Native pixel/cel authoring only. The inspected Rattlebones reference defines
// the acting and 9x130ms cadence and is never modified. The existing Pan idle
// supplies full-size transparent cel storage only; every pixel is replaced.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/RemainingCast/';
var OUT='0A0D11';
function box(x0,y0,x1,y1){return [[x0,y0],[x1+1,y0],[x1+1,y1+1],[x0,y1+1]];}
var ACTING={
 CrossbowGuard:{head:[[14,6],[44,6],[50,25],[43,25],[43,34],[38,37],[19,36],[17,26],[12,26]],eyes:[[22,28,23,30],[31,28,33,30]],props:[[[10,35],[13,38],[12,45],[30,43],[43,43],[43,49],[29,52],[23,59],[15,60],[11,57],[3,57],[2,51],[8,49]]],propDy:'carry',center:31},
 BarricadeGuard:{head:[[22,8],[41,8],[47,21],[47,33],[40,34],[35,37],[24,37],[19,33],[17,22]],props:[[[8,34],[15,34],[18,39],[17,44],[28,44],[28,50],[15,50],[13,54],[6,52],[2,46]]],propDy:'carry',center:32},
 SpearGuard:{head:[[23,7],[38,7],[48,19],[51,25],[44,25],[43,35],[38,38],[23,37],[19,32],[18,26],[13,26]],eyes:[[23,28,24,30],[32,28,34,30]],props:[[[5,2],[9,2],[12,34],[16,40],[20,40],[20,47],[16,48],[19,64],[15,64],[11,43],[7,24]],[[41,37],[49,38],[57,43],[58,54],[49,60],[42,61],[35,54],[34,44],[38,38]]],propDy:'planted',center:32},
 TownMarshal:{head:[[17,8],[47,8],[47,28],[44,29],[44,34],[39,39],[38,42],[24,42],[19,37],[18,33],[17,32]],eyes:[[23,23,25,25],[32,23,35,25]],props:[[[2,25],[15,25],[17,32],[14,38],[16,43],[15,49],[10,50],[9,39],[4,36]],[[57,25],[63,28],[64,35],[60,40],[57,52],[53,61],[50,61],[51,49],[48,49],[48,41],[53,39],[56,35],[54,30]]],propDy:'carry',center:32},
 SiegeSergeant:{head:[[29,1],[37,1],[44,11],[46,20],[48,24],[42,25],[43,31],[40,39],[36,42],[25,41],[20,35],[23,27],[21,25],[20,21]],eyes:[[26,25,28,26],[36,25,38,26]],props:[[[14,32],[20,32],[22,39],[19,42],[18,54],[23,54],[24,62],[21,64],[7,64],[7,57],[12,54],[14,42],[10,39],[10,34]]],propDy:'planted',center:33},
 SwordKnight:{head:box(18,5,43,35),props:[[[59,20],[63,22],[49,44],[49,49],[45,51],[40,51],[38,48],[43,44],[46,40]]],propDy:'carry',center:31},
 SpearKnight:{head:[[33,1],[45,1],[54,8],[55,20],[46,19],[46,30],[41,34],[39,38],[25,38],[22,34],[23,25],[22,22],[26,14],[30,10]],props:[[[5,3],[9,7],[11,18],[15,19],[18,21],[16,26],[12,28],[16,39],[20,40],[20,47],[17,50],[19,63],[15,64],[11,44],[9,31],[6,27],[3,25],[5,22],[5,17]]],propDy:'planted',center:32,tail:box(44,7,55,20)},
 ShieldKnight:{head:box(22,5,44,35),props:[[[46,28],[52,31],[59,33],[63,35],[61,54],[58,59],[51,64],[45,62],[39,56],[36,46],[37,37],[41,32]]],propDy:'planted',center:31,cape:box(8,49,16,62)},
 KnightCaptain:{head:[[24,8],[38,8],[39,3],[48,3],[54,8],[56,21],[53,26],[55,34],[54,37],[47,35],[45,29],[42,32],[37,36],[26,37],[19,33],[18,22],[20,15]],eyes:[[24,26,26,28],[33,26,35,28]],props:[[[1,17],[5,21],[17,39],[20,42],[23,44],[22,48],[18,51],[14,49],[15,44],[11,42],[3,27]]],propDy:'carry',center:32,cape:box(49,45,61,58),tail:box(47,22,56,36)},
 RoyalSwordsman:{head:box(22,7,47,37),props:[[[1,18],[5,19],[21,43],[25,44],[25,50],[19,53],[15,51],[14,47],[11,48],[9,46],[15,42],[2,26]]],propDy:'carry',center:33,cape:box(51,43,62,61)},
 RoyalLancer:{head:[[36,4],[43,4],[46,8],[49,18],[47,21],[45,25],[46,34],[43,38],[37,40],[29,39],[24,34],[24,25],[27,18],[31,13]],props:[[[4,5],[8,7],[17,25],[19,31],[18,35],[23,40],[27,42],[27,48],[24,50],[28,62],[27,64],[23,64],[18,50],[14,48],[12,42],[12,34],[8,32]]],propDy:'planted',center:35,tail:box(43,9,49,21)},
 RoyalArbalist:{head:[[30,2],[39,2],[44,7],[49,8],[50,14],[39,18],[40,22],[41,30],[36,35],[29,37],[22,35],[19,29],[19,22],[22,18],[27,13]],props:[[[11,34],[14,36],[9,43],[9,45],[22,42],[32,39],[37,40],[40,44],[37,47],[30,48],[25,52],[20,56],[9,56],[0,51],[0,49],[6,46],[6,40]]],propDy:'carry',center:31,cape:box(44,47,49,62),tail:box(38,4,50,17)},
 RoyalStandardBearer:{head:[[33,12],[42,12],[46,18],[47,30],[43,35],[37,39],[29,38],[26,34],[24,27],[26,21],[28,17]],props:[[[8,0],[14,0],[14,28],[22,39],[23,48],[20,64],[12,64],[10,34],[8,28]]],propDy:'planted',center:34,cape:box(49,40,63,57),banner:true},
 RoyalArcanist:{head:[[30,5],[41,6],[46,10],[49,17],[49,24],[45,26],[44,31],[38,35],[29,35],[24,30],[22,25],[23,15],[25,9]],eyes:[[26,20,28,22],[34,20,36,22]],props:[[[12,11],[17,11],[22,15],[22,23],[18,26],[20,64],[16,64],[14,44],[11,43],[11,35],[13,33],[12,26],[7,23],[6,18],[8,14]]],propDy:'planted',center:34,cape:box(48,47,53,62)},
 RoyalMage:{head:[[25,8],[40,8],[46,12],[49,19],[49,32],[45,35],[41,35],[38,38],[29,38],[25,34],[21,34],[21,21],[23,14]],eyes:[[26,24,28,26],[34,24,36,26]],props:[[[11,13],[14,11],[17,18],[20,20],[19,28],[17,32],[19,64],[15,64],[14,45],[11,44],[11,38],[14,35],[13,31],[9,29],[6,23],[8,21],[9,16]]],propDy:'planted',center:34,cape:box(49,46,55,62)},
 King:{head:[[21,4],[45,4],[46,17],[47,20],[48,28],[45,30],[45,34],[39,38],[31,39],[25,36],[22,31],[21,27],[23,21],[22,17]],eyes:[[25,22,28,22],[34,22,37,22]],props:[[[10,17],[14,17],[17,23],[16,31],[20,33],[23,35],[22,39],[18,39],[18,59],[14,63],[10,64],[6,60],[5,40],[3,39],[4,35],[9,33]]],propDy:'planted',center:34,cape:box(52,44,61,63),heavy:true},
 Minotaur:{head:[[5,0],[56,0],[56,26],[49,26],[46,33],[43,35],[25,36],[16,30],[8,24]],eyes:[[25,21,29,23],[40,21,42,23]],eyeFill:['B07A43','7A4D2E'],props:[[[50,26],[55,27],[60,31],[59,35],[55,37],[53,43],[53,50],[52,58],[48,62],[46,60],[47,49],[43,48],[44,42],[48,42],[49,36],[47,34],[48,29]]],propDy:'carry',center:31,cape:[[10,44],[13,46],[14,51],[22,54],[23,59],[8,60],[8,53]],heavy:true}
};
// Include the complete outer contour above each grip. Diagonal selection edges
// must not leave a thin blade/staff outline behind in the moving torso layer.
ACTING.SpearGuard.props.push(box(2,0,12,33));
ACTING.SpearGuard.props.push(box(3,34,20,63));
ACTING.RoyalLancer.props.push(box(2,0,19,34));
ACTING.RoyalLancer.props.push(box(9,34,22,50));
ACTING.RoyalLancer.props.push(box(14,49,27,63));
ACTING.RoyalArcanist.props.push(box(4,9,21,31));
ACTING.RoyalArcanist.props.push(box(9,32,20,63));
ACTING.RoyalMage.props.push(box(5,9,20,35));
ACTING.RoyalMage.props.push(box(10,35,20,63));
ACTING.SpearKnight.props.push(box(2,0,20,63));
ACTING.King.props.push(box(7,15,18,32));
// Complete authored tool silhouettes, including bow stirrups and their grips.
// These masks are reviewed against the still, not inferred from opaque bounds.
ACTING.RoyalArbalist.props=[[[10,34],[16,37],[11,43],[24,41],[31,38],[38,39],[41,42],[41,48],[35,50],[31,52],[31,64],[22,64],[21,59],[13,59],[7,56],[0,56],[0,48],[5,45],[5,40]]];
ACTING.CrossbowGuard.props=[[[10,34],[15,37],[18,41],[23,43],[29,43],[37,40],[41,40],[44,43],[43,49],[40,52],[31,54],[31,60],[25,62],[19,61],[14,58],[8,58],[3,55],[2,51],[6,49],[8,47],[8,39]]];
ACTING.BarricadeGuard.props=[[[7,33],[15,33],[18,38],[18,45],[27,44],[28,49],[24,52],[17,51],[14,54],[5,54],[1,48],[2,41]]];
ACTING.Minotaur.props=[[[51,23],[56,24],[61,29],[61,35],[56,38],[54,43],[54,50],[53,55],[50,61],[45,62],[44,56],[46,49],[42,49],[42,41],[46,39],[47,35],[48,27]]];
ACTING.SiegeSergeant.props=[[[12,31],[21,31],[23,34],[23,39],[21,43],[18,43],[18,53],[24,53],[26,57],[26,64],[6,64],[6,53],[12,52],[12,43],[9,40],[9,33]]];
// The spear and shield belong to different hands; a planted shaft must not pin
// the shield to the floor. Its complete rigid disk follows the supporting arm.
ACTING.SpearGuard.shield=ACTING.SpearGuard.props[1];
ACTING.SpearGuard.props.splice(1,1);
ACTING.TownMarshal.props=[box(1,24,17,51),box(48,22,63,62)];
ACTING.RoyalArbalist.props.push(box(18,55,30,63));
ACTING.SpearKnight.props=[box(2,0,18,63),box(2,31,20,63)];
ACTING.SpearKnight.head=[[30,0],[47,0],[57,8],[58,21],[47,21],[47,31],[40,37],[27,38],[21,35],[19,25],[18,21],[23,15],[29,10]];
ACTING.RoyalMage.props=[box(4,9,19,34),box(10,34,20,63)];
ACTING.RoyalMage.head=[[20,7],[49,7],[51,15],[51,34],[42,37],[27,38],[19,34],[19,20]];
ACTING.ShieldKnight.props=[[[46,23],[54,26],[64,32],[64,52],[59,59],[52,64],[45,64],[38,56],[34,44],[36,35],[41,29]]];
ACTING.SwordKnight.props=[[[58,19],[64,20],[64,26],[51,45],[51,51],[44,54],[38,50],[39,45],[44,39]]];
ACTING.RoyalSwordsman.props=[[[0,16],[6,17],[21,38],[23,41],[27,43],[28,51],[23,55],[17,54],[13,50],[8,49],[8,43],[11,41],[0,27]]];
ACTING.KnightCaptain.props=[[[0,15],[7,17],[20,36],[24,41],[25,47],[23,51],[17,54],[12,51],[12,45],[8,39],[0,28]]];
// Include connected one-pixel helmet/hood edges as well as isolated remnants.
ACTING.ShieldKnight.headTop=box(20,4,46,30);
ACTING.RoyalMage.headTop=box(19,7,52,32);
ACTING.SiegeSergeant.headTop=box(19,0,48,30);
ACTING.King.eyes=[[25,22,27,22],[33,22,36,22]];

// Low poses use a small neck pivot, not a separate nod layered over a body bob.
// More frontal heavy heads retain a restrained pivot and deeper chest breath.
for(var actor in ACTING){ACTING[actor].name=actor;ACTING[actor].pitch=1;}
ACTING.Minotaur.pitch=0;ACTING.King.pitch=0;
function ix(x,y){return (y*64+x)*4;}
function cp(a,i,b,j){for(var c=0;c<4;c++)b[j+c]=a[i+c];}
function px(a,x,y,c){if(x<0||x>63||y<0||y>63)throw Error('Out of canvas');var i=ix(x,y);for(var k=0;k<3;k++)a[i+k]=c?parseInt(c.slice(k*2,k*2+2),16):0;a[i+3]=c?255:0;}
function within(x,y,poly){if(!poly)return false;var hit=false;x+=.5;y+=.5;for(var i=0,j=poly.length-1;i<poly.length;j=i++){var a=poly[i],b=poly[j];if(((a[1]>y)!==(b[1]>y))&&(x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0]))hit=!hit;}return hit;}
function inParts(x,y,parts){for(var i=0;parts&&i<parts.length;i++)if(within(x,y,parts[i]))return true;return false;}
function readReady(name){app.open(ROOT+'Recolored/'+name+'.aseprite');var s=app.activeSprite,c=s.layer(0).cel(0),im=c.image,src=im.getImageData(),out=new Uint8Array(64*64*4);for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++)cp(src,(y*im.width+x)*4,out,ix(x+c.x,y+c.y));app.activeDocument.close();return out;}
function stamp(a,out,dx,dy){for(var y=0;y<64;y++)for(var x=0;x<64;x++){var i=ix(x,y);if(!a[i+3])continue;var xx=x+dx,yy=y+dy;if(xx<0||xx>63||yy<0||yy>63)throw Error('Clipped rigid part '+x+','+y+' offset '+dx+','+dy);cp(a,i,out,ix(xx,yy));}}
function adoptContourRemnants(parts,ac){
 // Preserve the thin edge fragments left just outside an authored selection.
 // These are existing source pixels, never filled or recolored. An isolated
 // contour belongs to the adjacent head/tool, not to the deforming torso.
 var seen=new Uint8Array(4096),body=parts.body,keys=['head','prop','shield','flag'];
 for(var seed=0;seed<4096;seed++)if(!seen[seed]&&body[seed*4+3]){
  var q=[seed],points=[],bottom=false;seen[seed]=1;
  while(q.length){var p=q.pop(),x=p%64,y=Math.floor(p/64);points.push(p);if(y>=62)bottom=true;
   for(var oy=-1;oy<=1;oy++)for(var ox=-1;ox<=1;ox++){var xx=x+ox,yy=y+oy;if(xx<0||xx>63||yy<0||yy>63)continue;var j=yy*64+xx;if(!seen[j]&&body[j*4+3]){seen[j]=1;q.push(j);}}
  }
  if(bottom||points.length>65)continue;
  var votes=[0,0,0,0];
  for(var v=0;v<points.length;v++){var x=points[v]%64,y=Math.floor(points[v]/64);
   for(var oy=-2;oy<=2;oy++)for(var ox=-2;ox<=2;ox++){var xx=x+ox,yy=y+oy;if(xx<0||xx>63||yy<0||yy>63)continue;
    for(var k=0;k<keys.length;k++)if(parts[keys[k]][ix(xx,yy)+3])votes[k]+=3-Math.max(Math.abs(ox),Math.abs(oy));
   }
  }
  var best=0;for(var k=1;k<4;k++)if(votes[k]>votes[best])best=k;
  if(!votes[best])continue;
  for(var v=0;v<points.length;v++){var i=points[v]*4;cp(body,i,parts[keys[best]],i);for(var c=0;c<4;c++)body[i+c]=0;}
 }
}
function extract(src,ac){
 var parts={body:new Uint8Array(src.length),head:new Uint8Array(src.length),prop:new Uint8Array(src.length),shield:new Uint8Array(src.length),flag:new Uint8Array(src.length)};
 for(var y=0;y<64;y++)for(var x=0;x<64;x++){
  var i=ix(x,y),head=ac.grownHead?ac.grownHead[i+3]>0:within(x,y,ac.head)||within(x,y,ac.tail)||within(x,y,ac.headTop);
  var k=inParts(x,y,ac.props)?'prop':within(x,y,ac.shield)?'shield':head?'head':ac.banner&&x>=12&&y<29?'flag':'body';
  cp(src,i,parts[k],i);
 }
 // The source bow overlays trousers and tabards. Those visible cloth pixels
 // remain body material, while every wood/string/iron bow pixel stays rigid.
 for(var y=49;y<64;y++)for(var x=20;x<36;x++){
  var i=ix(x,y),r=src[i],g=src[i+1],b=src[i+2],cloth=ac.name==='CrossbowGuard'&&b>r*1.12&&b>g;
  cloth=cloth||(ac.name==='RoyalArbalist'&&y>=51&&r>g*1.5&&g<80);
  if(cloth&&parts.prop[i+3]){cp(parts.prop,i,parts.body,i);for(var c=0;c<4;c++)parts.prop[i+c]=0;}
 }
 adoptContourRemnants(parts,ac);
 // Hidden shoulder/neck overlap. These narrow seams sit beneath the head in
 // ready, then maintain contact when the head rises or shoulders compress.
 var joins={CrossbowGuard:[27,34,38,38],BarricadeGuard:[26,33,38,38],SpearGuard:[27,34,39,39],TownMarshal:[27,39,37,44],SiegeSergeant:[28,38,38,43],SwordKnight:[26,32,37,38],SpearKnight:[28,34,40,39],ShieldKnight:[27,32,39,38],KnightCaptain:[26,33,38,38],RoyalSwordsman:[28,34,39,39],RoyalLancer:[30,35,41,40],RoyalArbalist:[25,34,36,40],RoyalStandardBearer:[30,35,42,41],RoyalArcanist:[30,32,40,38],RoyalMage:[29,34,40,40],King:[30,35,40,40],Minotaur:[26,32,42,38]};
 var j=joins[ac.name];
 for(var y=j[1];y<=j[3];y++)for(var x=j[0];x<=j[2];x++){
  if(parts.body[ix(x,y)+3])continue;
  // Extend the collar's own colors into the hidden join, never empty canvas.
  for(var yy=y+1;yy<=Math.min(47,y+7);yy++)if(parts.body[ix(x,yy)+3]){cp(parts.body,ix(x,yy),parts.body,ix(x,y));break;}
 }
 // Two-pixel sleeves overlap the held wrists. The elbow can open on the rise
 // while the hand continues to grip a planted tool; no detached weapon hand.
 var bridges={SpearGuard:[19,39,21,46],SpearKnight:[18,40,21,47],RoyalArcanist:[18,35,21,44],RoyalMage:[18,37,21,45],TownMarshal:[46,40,50,49],RoyalLancer:[22,41,26,48]};
 var b=bridges[ac.name];if(b)for(var y=b[1];y<=b[3];y++)for(var x=b[0];x<=b[2];x++)if(src[ix(x,y)+3]&&!parts.head[ix(x,y)+3])cp(src,ix(x,y),parts.body,ix(x,y));
 if(ac.name==='TownMarshal')for(var y=39;y<=49;y++)for(var x=15;x<=20;x++)if(src[ix(x,y)+3])cp(src,ix(x,y),parts.body,ix(x,y));
 return parts;
}
function blink(src,ac,amount){var a=new Uint8Array(src);if(!ac.eyes||!amount)return a;for(var e=0;e<ac.eyes.length;e++){var b=ac.eyes[e],fill=ac.eyeFill?ac.eyeFill[e]:'E6B08A';for(var y=b[1];y<=b[3];y++)for(var x=b[0];x<=b[2];x++){
 if(amount===1){if(y===b[1]&&b[3]>b[1])px(a,x,y,fill);}
 else px(a,x,y,y===b[3]?OUT:fill);
 }}return a;}
function clamp(v,a,b){return Math.max(a,Math.min(b,v));}
function inverseRow(y,nodes){for(var n=1;n<nodes.length;n++)if(y<=nodes[n][1]){var a=nodes[n-1],b=nodes[n];return Math.round(a[0]+(y-a[1])*(b[0]-a[0])/(b[1]-a[1]));}return y;}
function deformBody(src,out,dy,lean,roll,ac,lag){
 // One continuous torso/arm/cape surface. There is no y=48 cut, no removed
 // rectangular cape patch, and no prop pixel in this compression operation.
 for(var y=0;y<64;y++)for(var x=0;x<64;x++){
  var side=clamp((x-ac.center+6)/18,0,1);
  var nodes=[[0,dy],[32,32+dy+roll*side],[42,42+dy*.75+roll*side*.5],[50,50+dy*.4],[58,58+dy*.18],[61,61],[63,63]];
  var sy=inverseRow(y,nodes),weight=clamp((61-sy)/20,0,1);
  var fold=0;
  if(ac.cape&&sy>=44&&sy<61){var sign=x<ac.center?-1:1;fold=-sign*lag*clamp((Math.abs(x-ac.center)-9)/14,0,1)*clamp((sy-42)/15,0,1);}
  var sx=x-Math.round(lean*weight+fold);
  if(sx<0||sx>63||sy<0||sy>63)continue;
  if(src[ix(sx,sy)+3])cp(src,ix(sx,sy),out,ix(x,y));
 }
}
function solidBounds(a){var b=[64,64,-1,-1];for(var y=0;y<64;y++)for(var x=0;x<64;x++)if(a[ix(x,y)+3]){b[0]=Math.min(b[0],x);b[1]=Math.min(b[1],y);b[2]=Math.max(b[2],x);b[3]=Math.max(b[3],y);}return b;}
function pose(ready,ac,f){
 if(f===0||f===8)return new Uint8Array(ready);
 var hd=(ac.heavy?[0,-1,0,2,3,4,2,1,0]:[0,-2,-1,1,4,4,3,1,0])[f];
 var bd=(ac.heavy?[0,-1,0,1,2,3,2,1,0]:[0,-1,-1,1,3,3,2,1,0])[f];
 var lean=[0,1,1,1,0,-1,-1,0,0][f],roll=[0,-1,0,1,1,0,-1,0,0][f];
 var p=extract(blink(ready,ac,f===5?2:(f===4||f===6)?1:0),ac),out=new Uint8Array(ready.length);
 var lag=[0,0,-1,0,1,2,2,1,0][f];
 if(ac.banner){for(var y=0;y<29;y++)for(var x=12;x<64;x++){if(p.flag[ix(x,y)+3]){var fy=y+Math.round(lag*Math.max(0,(x-17)/46));cp(p.flag,ix(x,y),out,ix(x,fy));}}}
 deformBody(p.body,out,bd,lean,roll,ac,lag);
 // Complete the unseen banner backing exposed by a dipping helmet.
 if(ac.banner){for(var y=13;y<25;y++)for(var x=30;x<46;x++)if(!out[ix(x,y)+3])px(out,x,y,y<16?'7C1A30':'4B1127');}
 var pb=solidBounds(p.prop),pdx=ac.propDy==='planted'?0:lean;
 if(pb[0]+pdx<0||pb[2]+pdx>63)pdx=0;
 var pdy=ac.propDy==='planted'?0:Math.min(bd,63-pb[3]);
 // Planted spears/staves stay rigid; the flexing elbow carries the torso dip.
 stamp(p.prop,out,pdx,pdy);
 var sb=solidBounds(p.shield);if(sb[2]>=0)stamp(p.shield,out,0,Math.min(bd,63-sb[3]));
 var hb=solidBounds(p.head);if(hb[1]+hd<0)hd=-hb[1];
 // A one-pixel neck pivot changes the head angle through the dip. The eyes
 // remain in the same head drawing and never descend ahead of the face.
 var tilt=ac.pitch*(f>=3&&f<=6?1:0),hdx=lean;
 for(var y=hb[1];y<=hb[3];y++){
  var pivot=Math.round(tilt*clamp((hb[3]-y)/Math.max(1,hb[3]-hb[1]),0,1));
  var plume=0;
  for(var x=hb[0];x<=hb[2];x++)if(p.head[ix(x,y)+3]){
   var xx=x+hdx+pivot+plume,yy=y+hd;
   if(xx<0||xx>63||yy<0||yy>63)throw Error(ac.name+' head clipping');
   cp(p.head,ix(x,y),out,ix(xx,yy));
  }
 }
 return out;
}
function palette(a){var cs={},out=[];for(var i=0;i<a.length;i+=4)if(a[i+3]){var c=('0'+a[i].toString(16)).slice(-2)+('0'+a[i+1].toString(16)).slice(-2)+('0'+a[i+2].toString(16)).slice(-2);cs[c.toUpperCase()]=true;}for(var c in cs)out.push(c);return out;}
function enlargeRoyalHead(name,ac){
 // Idempotent: work from the preserved pre-correction still, not a previously
 // enlarged result. Only the user-approved helmet/plume region changes.
 app.open(ROOT+'BeforeCorrection/Recolored/'+name+'.aseprite');var s=app.activeSprite;
 var cel=s.layer(0).cel(0),ready=new Uint8Array(cel.image.getImageData());
 if(cel.x||cel.y||cel.image.width!==64||cel.image.height!==64)throw Error('Expected full ready cel');
 var head=new Uint8Array(ready.length),plume=new Uint8Array(ready.length),base=new Uint8Array(ready),grown=new Uint8Array(ready.length);
 var plumeShape=name==='RoyalLancer'?[[34,1],[46,1],[51,9],[52,19],[46,23],[41,21],[40,16],[34,14],[31,12]]:[[29,0],[40,0],[46,6],[52,8],[53,15],[45,18],[36,17],[29,15],[26,14],[26,8]];
 var helmetShape=name==='RoyalLancer'?[[30,12],[39,12],[43,17],[47,23],[48,31],[43,34],[38,37],[30,37],[24,33],[23,24],[26,18]]:[[27,12],[37,12],[41,18],[42,29],[39,32],[35,35],[27,36],[23,34],[19,29],[18,23],[21,18]];
 for(var y=0;y<64;y++)for(var x=0;x<64;x++)if(within(x,y,helmetShape)||within(x,y,plumeShape)){
  var i=ix(x,y);cp(ready,i,within(x,y,plumeShape)?plume:head,i);px(base,x,y,null);
 }
 var cx=name==='RoyalLancer'?35:30;
 for(var y=0;y<42;y++)for(var x=13;x<54;x++){
  var sx=Math.round(cx+(x-cx)/1.22),sy=Math.round(37+(y-37)/1.09);
  if(sx>=0&&sx<64&&sy>=0&&sy<64&&head[ix(sx,sy)+3])cp(head,ix(sx,sy),grown,ix(x,y));
 }
 stamp(plume,grown,1,-1);
 stamp(grown,base,0,0);ac.grownHead=grown;
 cel.image.putImageData(base);s.commit();s.saveAs(ROOT+'Recolored/'+name+'.aseprite',true);app.activeDocument.close();
 return base;
}
function write(name,frames,colors,path){
 app.open('C:/UnityProjects/Dungeon Matcher/ArtSource/CombatIdles/PanVillager_Idle.aseprite');var s=app.activeSprite;
 s.saveAs(path||ROOT+'Idles/'+name+'_Idle.aseprite',false);
 if(s.layer(0).celCount!==9)throw Error('Reference exposures changed');
 for(var f=0;f<9;f++){var c=s.layer(0).cel(f);if(c.x||c.y||c.image.width!==64||c.image.height!==64)throw Error('Full canvas required');c.image.putImageData(frames[f]);}
 s.layer(0).name=name+' idle';var pal=s.palette;pal.length=colors.length+1;pal.set(0,app.pixelColor.rgba(0,0,0,0));for(var i=0;i<colors.length;i++){var c=colors[i];pal.set(i+1,app.pixelColor.rgba(parseInt(c.slice(0,2),16),parseInt(c.slice(2,4),16),parseInt(c.slice(4,6),16),255));}
 s.commit();s.save();app.activeDocument.close();
}
for(var n=0;n<CAST.length;n++){
 var name=CAST[n].name,ac=ACTING[name],ready=(name==='RoyalLancer'||name==='RoyalArbalist')?enlargeRoyalHead(name,ac):readReady(name),frames=[];
 for(var f=0;f<9;f++)frames.push(pose(ready,ACTING[name],f));
 var colors=palette(ready),parts=extract(ready,ac),empty=new Uint8Array(ready.length);
 write(name,[ready,parts.body,parts.head,parts.prop,parts.shield,parts.flag,empty,empty,empty],colors,ROOT+'Review/Correction/RigParts/'+name+'.aseprite');
 write(name,frames,colors);console.log('AUTHORED '+name+' 9x130ms');
}
