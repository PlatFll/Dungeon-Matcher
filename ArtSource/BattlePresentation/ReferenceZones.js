// Both references use the same sampling grid. Every visible landmark keeps its
// original registration. Only text, actors, bars and gems are removed.
function clone(a,sx,sy,w,h,dx,dy){copy(a,crop(N,sx,sy,w,h),dx,dy);}
var forest=ZONE==='Forest';
var masks=forest?[
 [[184,116],[220,116],[220,156],[183,156]],[[302,116],[338,116],[338,155],[302,155]],
 [[151,181],[165,169],[186,168],[201,173],[221,178],[238,172],[237,182],[226,190],[239,209],[248,234],[246,254],[172,254],[169,231],[164,219],[154,214]],
 [[257,172],[260,165],[272,165],[278,174],[288,164],[302,157],[314,153],[322,163],[337,172],[347,177],[354,192],[370,201],[369,237],[355,251],[346,255],[271,255],[268,240],[267,223],[263,222],[263,205]],
 [[159,253],[361,253],[361,275],[159,275]]
]:[
 [[184,116],[222,116],[222,156],[183,156]],[[310,116],[348,116],[348,156],[310,156]],
 [[151,169],[157,164],[168,168],[175,157],[184,162],[194,167],[202,158],[212,172],[224,169],[229,185],[241,196],[243,214],[252,229],[249,249],[240,256],[183,256],[178,248],[174,224],[166,215],[158,208]],
 [[270,167],[277,164],[286,173],[291,180],[300,168],[308,175],[317,161],[324,158],[333,167],[340,161],[345,176],[353,184],[364,189],[375,210],[380,235],[393,233],[406,219],[410,228],[398,249],[381,258],[300,258],[295,245],[291,225],[284,214],[278,205]],
 [[159,253],[377,253],[377,276],[159,276]]
];
// Constrained patch restoration from unoccluded pixels of this SAME reference.
// Donors are restricted to the same vertical material band and never contain
// controls or characters. No blur, interpolation or generative reconstruction.
function restore(a,regions){
 var w=a.w,h=a.h,src=crop(a,0,0,w,h),known=new Uint8Array(w*h),left=0;
 function hidden(x,y){for(var m=0;m<regions.length;m++)if(inside(x+.5,y+.5,regions[m]))return true;return false;}
 for(var y=0;y<h;y++)for(var x=0;x<w;x++){var ok=!hidden(x,y);known[y*w+x]=ok?1:0;if(!ok)left++;}
 var donors=[];
 for(var y=110;y<287;y+=2)for(var x=143;x<435;x+=2){
  var ok=true;
  // No signpost, side lantern, player panel, or frame donors.
  if(forest&&x>395&&y>119&&y<212)continue;
  if(!forest&&x>395&&y<245)continue;
  for(var yy=-5;yy<=5&&ok;yy++)for(var xx=-5;xx<=5;xx++)if(!known[(y+yy)*w+x+xx]){ok=false;break;}
  if(ok)donors.push([x,y]);
 }
 var steps=0;
 while(left&&steps++<3500){
  var bestCount=-1,tx=0,ty=0;
  for(var y=115;y<277;y++)for(var x=150;x<412;x++)if(!known[y*w+x]){
   var count=0;for(var yy=-4;yy<=4;yy+=2)for(var xx=-4;xx<=4;xx+=2)if(known[(y+yy)*w+x+xx])count++;
   if(count>bestCount){bestCount=count;tx=x;ty=y;}
  }
  if(bestCount<0)throw Error('Mask outside bounded repair area');
  var best=1e30,bx=0,by=0;
  for(var c=0;c<donors.length;c++){
   var cx=donors[c][0],cy=donors[c][1];if(Math.abs(cy-ty)>18)continue;
   // Keep dry path/stone floor separate from woodland/underwater architecture.
   if((ty>=211)!==(cy>=211))continue;
   if(forest && ty<211 && tx<255 && cx>263)continue;
   if(forest && ty>=211){var ci=(cy*w+cx)*4;if(src.p[ci]<src.p[ci+1]*.95||src.p[ci+2]>src.p[ci+1]*.85)continue;}
   var error=Math.abs(cy-ty)*200+Math.abs(cx-tx)*4;
   for(var yy=-4;yy<=4;yy+=2)for(var xx=-4;xx<=4;xx+=2)if(known[(ty+yy)*w+tx+xx]){
    var di=((ty+yy)*w+tx+xx)*4,si=((cy+yy)*w+cx+xx)*4;
    for(var k=0;k<3;k++){var d=a.p[di+k]-src.p[si+k];error+=d*d;}
   }
   if(error<best){best=error;bx=cx;by=cy;}
  }
  if(best===1e30)throw Error('No suitable source patch '+ty);
  for(var yy=-5;yy<=5;yy++)for(var xx=-5;xx<=5;xx++){
   var x=tx+xx,y=ty+yy,i=y*w+x;if(known[i])continue;
   var si=((by+yy)*w+bx+xx)*4;for(var k=0;k<4;k++)a.p[i*4+k]=src.p[si+k];known[i]=1;left--;
  }
 }
 if(left)throw Error('Unfinished restoration '+left);
}
// The patch study was rejected: it repeated small features and broke masonry.
// Trace connected hidden material planes instead. Visible source stays exact.
masks[2]=forest?[[151,175],[164,165],[185,165],[208,170],[240,169],[240,189],[248,213],[251,244],[246,255],[171,255],[164,235],[160,221],[150,215]]:
 [[149,158],[169,155],[183,158],[202,157],[216,165],[233,166],[234,188],[250,207],[255,230],[254,255],[180,257],[170,237],[167,216],[153,209]];
masks[3]=forest?[[255,160],[279,158],[292,156],[312,150],[327,157],[331,169],[352,173],[367,184],[375,200],[375,237],[360,255],[269,257],[264,238],[260,223],[256,211]]:
 [[270,158],[279,163],[296,174],[310,161],[324,154],[339,160],[352,162],[365,179],[374,198],[381,223],[395,227],[411,216],[414,229],[404,247],[383,259],[296,258],[291,236],[283,215],[275,207]];
var scene=crop(N,0,0,470,298),paint=crop(scene,0,0,470,298);
var textured=crop(scene,0,0,470,298);restore(textured,masks);
if(forest){
 rect(paint,145,110,270,105,'244d43');
 // Far foliage shapes carry depth; no duplicated deer, lantern or rune.
 var leafColors=['1c3c36','2c5743','355d42','436c3d'];
 for(var row=0;row<6;row++)for(var col=0;col<12;col++){
  var x=145+col*23+(row%2)*9,y=111+row*17;
  poly(paint,[[x,y+5],[x+5,y+5],[x+5,y],[x+13,y],[x+13,y+4],[x+19,y+4],[x+19,y+10],[x+24,y+10],[x+24,y+17],[x+4,y+17]],leafColors[(row*3+col)%4]);
 }
 // Extend the tree's turning root and the boulder group through hidden areas.
 poly(paint,[[201,108],[216,113],[219,144],[229,174],[242,195],[234,208],[217,198],[210,174]],'35402f');
 poly(paint,[[205,115],[211,117],[214,147],[225,180],[235,194],[230,195],[216,180]],'555239');
 poly(paint,[[213,183],[224,174],[237,179],[247,195],[252,210],[217,214],[207,204]],'3c5354');
 poly(paint,[[216,185],[225,178],[233,183],[236,198],[224,202],[216,210]],'506368');
 poly(paint,[[237,183],[243,190],[247,206],[238,210],[233,199]],'2c4148');
 // Bridge continuation is quiet stone under moss, at the original height.
 poly(paint,[[291,160],[318,156],[345,156],[364,163],[360,175],[339,170],[319,170],[303,180],[291,177]],'31534d');
 line(paint,295,160,348,157,'486a47');line(paint,304,163,353,162,'3c624b');
 // A shallow earthen path, with larger connected patches and sparse stones.
 rect(paint,145,211,270,69,'776b3f');
 var patches=[[[145,215],[199,211],[210,218],[187,223],[148,222]],[[225,216],[281,211],[298,217],[272,224],[233,223]],[[317,214],[385,210],[406,220],[368,224],[326,220]],[[160,231],[223,227],[248,233],[212,241],[175,238]],[[270,233],[316,229],[340,235],[319,243],[281,242]],[[166,253],[218,249],[240,255],[214,260],[175,259]],[[244,263],[300,258],[329,263],[300,270],[255,269]],[[329,253],[384,249],[409,255],[386,265],[339,261]]];
 for(var p=0;p<patches.length;p++)poly(paint,patches[p],p%2?'97814a':'8c7c45');
 var stones=[[195,219,14,3],[251,224,16,3],[313,216,14,3],[222,244,23,4],[283,252,22,4],[353,239,18,4],[185,269,26,4],[335,273,28,4]];
 for(var p=0;p<stones.length;p++){var q=stones[p];rect(paint,q[0],q[1],q[2],q[3],'65644b');line(paint,q[0]+2,q[1],q[0]+q[2]-2,q[1],'8f8b64');}
 // Irregular low grass clumps, kept clear of the central fighter contact plane.
 for(var p=0;p<22;p++){var x=151+(p*37)%252,y=213+(p*17)%62;line(paint,x,y,x+9,y,'536734');rect(paint,x+2,y-2,3,2,'6a7b37');}
}else{
 // Distant water and column fragments behind the two tridents.
 rect(paint,149,154,268,58,'0b4c68');
 for(var p=0;p<5;p++){
  var x=157+p*49;rect(paint,x,159,20,59,'105974');rect(paint,x+3,159,3,55,'16647b');rect(paint,x+16,159,3,58,'0c405c');
  for(var y=172;y<215;y+=17)line(paint,x,y,x+19,y,'0b415a');
 }
 // Existing mid-distance portal remains open, rather than a new focal object.
 rect(paint,254,161,53,49,'0a4059');
 // Coral along the base of the ruins, below the statue and behind the actors.
 for(var p=0;p<17;p++){var x=155+p*15,y=208-(p%3)*4;line(paint,x,y,x+2,y-13,'2e6374');line(paint,x+2,y-8,x+8,y-12,'487986');}
 // Perspective courses continue the original stone deck. No repeated strip.
 var ys=[210,216,224,234,247,259,274,283];
 var ramps=['266777','307786','3d8390','397c88','377b86','3e8190','397788'];
 for(var row=0;row<ys.length-1;row++){
  var y=ys[row],Y=ys[row+1],pitch=34+row*7,offset=row%2?18:0;
  rect(paint,147,y,270,Y-y,'164e62');
  for(var x=135+offset;x<418;x+=pitch){
   poly(paint,[[x+2,y+1],[x+pitch-3,y+1],[x+pitch-1,Y-3],[x+pitch-4,Y-1],[x+1,Y-1]],ramps[row]);
   line(paint,x+3,y+1,x+pitch-5,y+1,'5b9c9e');line(paint,x+4,Y-2,x+pitch-7,Y-2,'236173');
   if((Math.floor(x/pitch)+row)%2===0)line(paint,x+9,y+3,x+13,y+3,'488b94');
  }
 }
}
for(var y=154;y<283;y++)for(var x=147;x<418;x++){
 var hidden=false;for(var m=2;m<masks.length;m++)if(inside(x+.5,y+.5,masks[m]))hidden=true;
 // Join the repaired woodland vegetation into a continuous bank, instead of
 // leaving two vegetation shapes that echo the removed actor silhouettes.
 if(forest&&x>=153&&x<375&&y>=166&&y<275)hidden=true;
 if(hidden){
  var i=(y*470+x)*4;
  // Woodland surfaces are irregular, so keep source texture instead of flat
  // polygon fills. The Court uses traced joints with restrained source grain.
  var useTexture=false;
  if(forest){
   // Sample small existing material clusters into the traced planes. Keep
   // their scale and source colors, without copying architecture or stepping
   // through a grid of near-identical best-match patches.
   var sx,sy;
   if(y>=211){sx=249+((x+Math.floor(y/7)*3)%13);sy=Math.max(212,Math.min(273,y+((Math.floor(x/17)%3)-1)*2));}
   else {sx=145+((x+Math.floor(y/11)*5)%20);sy=126+((y+Math.floor(x/19)*3)%33);}
   var si=(sy*470+sx)*4,R=N.p[si],G=N.p[si+1],B=N.p[si+2];
   var material=y>=211 ? R>G*.88&&B<G*.82 : G>R*1.04&&G>B*.92;
   var base=color(paint,x,y);
   var stone=['3c5354','506368','2c4148'].indexOf(base)>=0;
   if(material&&!stone)for(var k=0;k<4;k++)paint.p[i+k]=N.p[si+k];
  }
  if(!forest&&y<210)useTexture=true;
  if(!forest&&y>=210){
   var maxDelta=0;for(var k=0;k<3;k++)maxDelta=Math.max(maxDelta,Math.abs(textured.p[i+k]-paint.p[i+k]));
   useTexture=maxDelta<24;
  }
  for(var k=0;k<4;k++)scene.p[i+k]=useTexture?textured.p[i+k]:paint.p[i+k];
 }
}
if(forest){
 // Whole source foliage/wood clusters, with irregular overlap contours. These
 // preserve the actual reference's leaf scale instead of inventing noisy marks.
 var patches=[[145,125,38,43,153,166],[145,128,38,43,187,167],[225,124,39,43,223,166],[266,111,33,45,260,166],[266,112,33,42,290,167]];
 for(var p=0;p<patches.length;p++){
  var q=patches[p],a=crop(N,q[0],q[1],q[2],q[3]);
  for(var yy=0;yy<a.h;yy++)for(var xx=0;xx<a.w;xx++){
   if(xx<2+(yy%13<6?1:0)||yy<2+(xx%17<7?1:0))continue;
   var si=(yy*a.w+xx)*4,di=((q[5]+yy)*470+q[4]+xx)*4;
   for(var k=0;k<4;k++)scene.p[di+k]=a.p[si+k];
  }
 }
 // Water remains behind the original bridge, extending naturally through the
 // area previously hidden by the orc; source bridge/deer at right stay intact.
 clone(scene,377,181,31,30,313,181);clone(scene,378,182,31,29,344,182);
 clone(scene,375,157,31,24,344,158);
 // Join the path edge with source grass clusters, all at the same native scale.
 for(var p=0;p<7;p++)clone(scene,241,205,18,8,155+p*27,207+(p%2));
 // Restore every unoccluded source pixel around the repair, especially the
 // center tree/root and bridge. Patch supports must never introduce a visible
 // rectangular seam across an original landmark.
 for(var y=154;y<283;y++)for(var x=147;x<418;x++){
  var hidden=false;for(var m=2;m<masks.length;m++)if(inside(x+.5,y+.5,masks[m]))hidden=true;
  if(!hidden){var i=(y*470+x)*4;for(var k=0;k<4;k++)scene.p[i+k]=N.p[i+k];}
 }
}
// Whole, unoccluded same-depth patches for the two small counter rectangles.
if(forest){clone(scene,145,115,38,41,183,115);clone(scene,263,109,39,42,301,115);clone(scene,220,75,28,45,220,75);}
else {clone(scene,221,114,41,44,183,114);clone(scene,227,110,41,44,309,114);}
repairs.push({name:'live enemy content',polygons:masks,connectedForestRepair:forest?[153,166,222,109]:null,method:'Source clusters and connected hidden material planes; reference landmarks retained. Hidden surfaces are reconstructed, not recovered original pixels.'});
// Controls and panel are separate layers. Their backing is fully covered.
rect(scene,10,22,132,258,forest?'24271c':'082b42');
rect(scene,157,21,156,47,forest?'263b30':'164761');rect(scene,330,30,79,39,'172432');rect(scene,414,30,45,40,'172432');
emit('FarScenery',crop(scene,0,0,470,160),0,0,'Exact woodland/ruin vista, local counter repairs','background');
emit('MidScenery',crop(scene,0,160,470,51),0,160,'Source bridge, tree roots or ruin base; locally restored silhouettes','background');
emit('BattleFloor',crop(scene,0,211,470,87),0,211,'Source shallow path or stone paving; no added floor courses','background');
// Restore the panel behind live affinity, character and health displays.
var panel=crop(N,22,42,105,224),clean=blank(105,224);
var colors=forest?['261e19','3a2d24','50402c','1c1b17']:['082333','0b3045','15445a','061e2c'];
rect(clean,0,0,105,224,colors[0]);
for(var row=0;row<11;row++){
 var y=row*23-6,xs=row%2?[-19,21,64,108]:[-2,43,83,124];
 for(var c=0;c<3;c++){
  var x=xs[c],W=xs[c+1]-x;
  rect(clean,x+1,y+1,W-2,21,colors[1]);line(clean,x+2,y+2,x+W-4,y+2,colors[2]);
  line(clean,x+W-2,y+4,x+W-2,y+19,colors[3]);
  if((row+c)%2===0)line(clean,x+9,y+11,x+12,y+11,colors[0]);
 }
}
for(var y=0;y<224;y++)for(var x=0;x<105;x++){
 var gx=x+22,gy=y+42;
 if((gx>=48&&gx<101&&gy<108)||(gx>=50&&gx<101&&gy>=108&&gy<149)||(gx>=25&&gx<125&&gy>=148&&gy<258)){
  var i=(y*105+x)*4;for(var k=0;k<4;k++)panel.p[i+k]=clean.p[i+k];
 }
}
emit('PlayerMasonry',panel,22,42,'Native panel masonry restored only under hidden content','panel');
var frame=crop(N,8,24,135,258);rect(frame,14,18,105,224,null);
cut(frame,[[15,0],[119,0],[135,14],[134,242],[120,258],[12,258],[0,244],[0,15]],true);
emit('PlayerFrame',frame,8,24,'Carved woodland or barnacled blue stone frame','frame');
var banner=crop(N,51,42,49,66);cut(banner,[[0,0],[49,0],[48,60],[38,55],[25,66],[10,56],[0,61]],true);
emit('PlayerBanner',banner,51,42,'Original stag/skull banner','prop');
var wave=crop(N,157,20,157,48);rect(wave,forest?25:15,forest?12:10,forest?111:127,forest?19:22,forest?'25241c':'103149');
cut(wave,[[8,0],[149,0],[157,9],[156,39],[148,45],[90,45],[90,48],[65,48],[65,45],[9,45],[0,37],[0,9]],true);
emit('WaveHeader',wave,157,20,'Original blank header with separate runtime label','frame');
// The approved Dungeon controls are the ONE shared family in all three kits.
var shared=[['SharedGuide',330,30],['SharedSettings',414,30],['SharedEnergy',163,707],['SharedAbility',149,729],['SharedPotionHolder',69,733],['SharedBombHolder',333,733]];
for(var s=0;s<2;s++){var p=shared[s];emit(p[0],read(ROOT+'../DungeonReferenceKit/native/'+p[0]+'.png'),p[1],p[2],'Approved shared silver control','shared');}
var bf=crop(N,30,288,410,414);rect(bf,23,25,364,369,null);
cut(bf,[[13,0],[393,0],[410,14],[410,399],[397,414],[12,414],[0,400],[0,15]],true);
emit('BoardFrame',bf,30,288,'Four distinct corners and continuous original beams','frame');
var cells=read(ROOT+'../DungeonReferenceKit/native/BoardWell.png');
emit('BoardWell',cells,53,313,'Shared quiet empty stone cells','board');
var corners=[['BoardTL',0,0,30,30],['BoardTR',380,0,30,30],['BoardBL',0,384,30,30],['BoardBR',380,384,30,30],['BoardTop',30,0,350,24],['BoardBottom',30,393,350,21],['BoardLeft',0,30,23,354],['BoardRight',388,30,22,354]];
for(var c=0;c<corners.length;c++){var p=corners[c];emit(p[0],crop(bf,p[1],p[2],p[3],p[4]),30+p[1],288+p[2],'Exact registered frame module','frame-module',false);}
emit('LeftBorderDressing',crop(N,0,298,30,403),0,298,'Original left lantern/foliage or coral/column','prop');
emit('RightBorderDressing',crop(N,440,298,30,403),440,298,'Distinct right decorations, never mirrored','prop');
var bottom=crop(N,0,701,470,135);
// Shared controls cover these backing areas. Reconstruct quiet central backing
// and keep the actual irregular corner roots/coral/foreground untouched.
var bottomMask=[[163,704,147,28],[65,728,75,66],[148,727,179,68],[332,729,73,65],[120,793,286,23]];
for(var q=0;q<bottomMask.length;q++){
 var p=bottomMask[q];for(var y=p[1];y<p[1]+p[3];y++)for(var x=p[0];x<p[0]+p[2];x++){
  var sx=150+(x%160),sy=818+(y%12),si=(sy*470+sx)*4,di=((y-701)*470+x)*4;
  for(var k=0;k<4;k++)bottom.p[di+k]=N.p[si+k];
 }
}
emit('BottomMasonry',bottom,0,701,'Original roots/stone/coral surround; no enclosing bottom frame','background');
for(var s=2;s<shared.length;s++){var p=shared[s];emit(p[0],read(ROOT+'../DungeonReferenceKit/native/'+p[0]+'.png'),p[1],p[2],'Approved shared silver control','shared');}
makeDocument(composite,OUTPUT+ZONE+'ArtOnly.aseprite',ZONE+'ArtOnly');
console.log('KIT_MANIFEST '+JSON.stringify({size:[470,836],modules:modules,repairs:repairs}));
console.log('KIT_COMPLETE');
