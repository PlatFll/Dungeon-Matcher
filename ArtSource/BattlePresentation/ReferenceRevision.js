// Executed inside LibreSprite, after Build.js's document/pixel helpers.
// All coordinates below refer to the supplied 941 x 1672 reference PNGs.
// Preserve source colors: the first pass's global nearest-palette conversion
// flattened the distinct materials and is intentionally bypassed here.
function extract(a,x,y,w,h,W,H){
 var b=blank(W,H);
 for(var Y=0;Y<H;Y++)for(var X=0;X<W;X++){
  var i=(Math.min(a.h-1,Math.floor(y+(Y+.5)*h/H))*a.w+Math.min(a.w-1,Math.floor(x+(X+.5)*w/W)))*4;
  for(var k=0;k<4;k++)b.p[(Y*W+X)*4+k]=a.p[i+k];
 }
 return b;
}
function patch(a,ref,sx,sy,w,h,dx,dy,W,H){copy(a,extract(ref,sx,sy,w,h,W||w,H||h),dx,dy);}
function repeat(a,ref,sx,sy,w,h,dx,dy,W,H){tile(a,crop(ref,sx,sy,w,h),dx,dy,W,H);}
function clearRect(a,x,y,w,h){rect(a,x,y,w,h,null);}
function waterRepair(a,ref,x,y,w,h){
 // Continue the water's light shafts across an occluded HUD area. Sample the
 // unobstructed rows above/below at each column, using stepped native bands.
 var fallback=(142*ref.w+460)*4;
 for(var X=0;X<w;X++){
  var top=((y-4)*ref.w+x+X)*4,bottom=((y+h+4)*ref.w+x+X)*4;
  if(ref.p[top]>100||ref.p[top+2]<ref.p[top+1])top=fallback;
  if(ref.p[bottom]>100||ref.p[bottom+2]<ref.p[bottom+1])bottom=fallback;
  for(var Y=0;Y<h;Y++){
   var t=Math.min(1,Math.floor(Y/6)*6/(h-1)),i=((y+Y)*a.w+x+X)*4;
   for(var k=0;k<3;k++)a.p[i+k]=Math.round(ref.p[top+k]*(1-t)+ref.p[bottom+k]*t);
   a.p[i+3]=255;
  }
 }
}
function copyPolygon(a,ref,sourceRect,destination,points){
 var p=extract(ref,sourceRect[0],sourceRect[1],sourceRect[2],sourceRect[3],destination[2],destination[3]);
 mask(p,points);copy(a,p,destination[0],destination[1]);
}
var zone=ONLY.split('/')[0]==='Scene'?ONLY.split('/')[1]:ONLY.split('/')[0];
if(zone==='Motion')zone=ONLY==='Motion/Torch'?'Dungeon':'Forest';
var file=zone==='Dungeon'?'DUNGEON_ZONE_TARGET':zone==='Forest'?'FOREST_ZONE_TARGET':'DROWNED_COURT_TARGET';
var ref=read(ROOT+'brief/references/'+file+'.png');

// Thin player frame: independent from the much heavier board construction.
// The clean source edge is extended, keeping caps/bolts at their native size.
function playerFrame(){
 var top=extract(ref,21,47,257,44,146,25),bottom=extract(ref,21,520,257,44,146,25);
 var mid=blank(146,32);
 if(zone==='Forest'){
  mask(top,[[10,0],[25,3],[127,3],[140,0],[146,12],[141,25],[130,25],[130,19],[15,19],[15,25],[3,25],[0,12]]);
  clearRect(top,20,20,108,5);
  mask(bottom,[[3,0],[15,0],[15,6],[128,6],[128,0],[141,0],[146,13],[138,25],[10,25],[0,13]]);
  clearRect(bottom,18,0,110,6);
  patch(mid,ref,26,280,18,58,2,0,10,32);patch(mid,ref,254,280,17,58,133,0,10,32);
 }else{
  clearRect(top,15,0,117,6);clearRect(top,15,18,117,7);
  mask(top,[[10,6],[20,10],[127,10],[136,6],[144,15],[139,25],[130,25],[130,18],[16,18],[16,25],[3,25],[0,16]]);
  clearRect(bottom,15,0,117,6);clearRect(bottom,15,18,117,7);
  mask(bottom,[[3,0],[16,0],[16,6],[130,6],[130,0],[139,0],[146,10],[137,22],[128,18],[18,18],[9,22],[0,13]]);
  patch(mid,ref,27,205,18,58,3,0,10,32);patch(mid,ref,253,205,17,58,132,0,10,32);
 }
 save(top,zone+'/PlayerTop');save(mid,zone+'/PlayerMiddle');save(bottom,zone+'/PlayerBottom');
}

function frame(){
 var forest=zone==='Forest';
 var edge=extract(ref,forest?174:177,forest?584:595,forest?151:138,forest?40:28,64,16);
 // End columns meet exactly; retain the original textured beam inside the join.
 for(var y=0;y<16;y++)for(var k=0;k<4;k++)edge.p[(y*64+63)*4+k]=edge.p[y*64*4+k];
 var corner=blank(80,80);tile(corner,edge,0,0,80,16);
 for(var y=16;y<80;y++)for(var x=0;x<16;x++)for(var k=0;k<4;k++)corner.p[(y*80+x)*4+k]=edge.p[(x*64+y%64)*4+k];
 var cap=extract(ref,forest?50:74,forest?565:588,forest?86:53,forest?88:58,forest?38:29,forest?39:32);
 mask(cap,forest?[[9,0],[25,1],[29,6],[38,9],[38,22],[26,26],[19,39],[8,33],[7,23],[0,18],[0,8]]:[[1,1],[27,1],[29,13],[19,32],[4,25],[0,13]]);
 copy(corner,cap,0,0);save(edge,zone+'/Edge');save(corner,zone+'/Corner');
}

function plaque(){
 var W=zone==='Court'?168:144,H=50;
 var p=extract(ref,zone==='Court'?316:336,39,zone==='Court'?316:271,95,W,H);
 // Erase printed lettering using the original unlettered recessed material.
 rect(p,Math.round(W*.094),11,Math.round(W*.812),21,'1b1c22');line(p,Math.round(W*.1),11,Math.round(W*.89),11,'292a30');
 // Keep just the frame, its corners and hanging skull; background is transparent.
 var outline=[[7,3],[119,3],[126,9],[125,31],[119,36],[75,36],[74,43],[64,45],[57,42],[54,36],[8,36],[2,30],[2,9]];
 mask(p,outline.map(function(v){return [v[0]*W/128,v[1]*H/45];}));
 save(p,zone+'/Plaque');
}

function materials(){
 var panel=blank(128,128);
 // Opposite clean strips from behind the player contain no actor, affinity,
 // banner or HP pixels. Stagger the join to continue the masonry courses.
 var start=zone==='Forest'?139:112,depth=zone==='Forest'?156:181;
 var left=extract(ref,47,start,43,depth,32,128),right=extract(ref,210,start,39,depth,32,128);
 copy(panel,left,0,0);copy(panel,right,32,0);copy(panel,left,64,0);copy(panel,right,96,0);
 save(panel,zone+'/Panel');
 var general=zone==='Forest'?extract(ref,37,1400,85,272,42,128):
  zone==='Court'?panel:extract(ref,173,1402,150,61,75,31);
 var surface=blank(128,128);tile(surface,general,0,0,128,128);save(surface,zone+'/General');
 var side=extract(ref,0,600,74,768,36,384);save(side,zone+'/SideDressing');
 // Bottom zone scenery is a separate reusable panorama; controls and labels
 // are removed in source space and remain live UI in Unity.
 var base=crop(ref,0,1394,941,278);
 if(zone==='Forest'){
  repeat(base,ref,284,1640,295,27,132,0,677,278);
  patch(base,ref,180,1400,150,61,132,0,150,61);
  patch(base,ref,650,1395,167,65,648,0,167,65);
 }else if(zone==='Court'){
  repeat(base,ref,287,1643,148,28,134,0,674,278);
 }else{
  repeat(base,ref,175,1404,151,55,134,0,674,278);
 }
 save(extract(base,0,0,941,278,512,152),zone+'/BottomPanel');
 var banner=extract(ref,102,84,96,130,44,64);
 mask(banner,[[0,0],[44,0],[43,55],[34,50],[23,64],[12,51],[1,57]]);save(banner,zone+'/Banner');
}

function cell(){
 // Build the unoccupied 64px cell from the source's exposed stone perimeter.
 // The inner field is sampled from an unoccluded triangle beside a gem; the
 // entire gem mask is discarded. No target gem artwork enters the game.
 var n=ONLY.charAt(ONLY.length-1),sx=n==='B'?384:n==='C'?661:108,sy=625;
 var a=extract(ref,sx,sy,91,91,64,64);
 var field=extract(ref,sx+6,sy+8,7,25,7,25);
 tile(a,field,6,6,52,52);
 // A continuous quiet material plane prevents repeated highlight bars. Keep
 // the outer chipped contour and top-left lighting from the actual source.
 var r=zone==='Court'?'172539':'1b2230';rect(a,9,9,46,46,r);
 poly(a,[[7,9],[23,7],[35,7],[29,10],[16,11],[11,24],[7,35]],zone==='Court'?'23354a':'2a3342');
 line(a,10,56,53,56,zone==='Court'?'111c30':'161c29');
 // Discard every remaining interior pixel outside the protected perimeter,
 // including bright gem tips adjacent to a source cell's edge.
 for(var y=0;y<64;y++)for(var x=0;x<64;x++){
  var i=(y*64+x)*4,R=a.p[i],G=a.p[i+1],B=a.p[i+2];
  if(Math.max(R,G,B)>100 || R>G+9 || B>G+28)px(a,x,y,x<5||x>58||y<5||y>58?'131c2b':r);
 }
 save(a,zone+'/Cell'+n);
}

function motion(){
 var scene=read(ROOT+'native/Scene/'+zone+'.png');
 if(ONLY==='Motion/Rune'){
  var rune=crop(scene,239,78,32,58);
  for(var i=0;i<rune.p.length;i+=4)if(!(rune.p[i+1]>125&&rune.p[i+2]>150&&rune.p[i+2]>rune.p[i]*1.1))rune.p[i+3]=0;
  save(rune,ONLY);
 }else if(ONLY==='Motion/Torch'){
  var flame=crop(scene,145,104,24,40),sheet=blank(144,40);
  for(var i=0;i<flame.p.length;i+=4)if(!(flame.p[i]>150&&flame.p[i+1]>70&&flame.p[i]>flame.p[i+2]*1.2))flame.p[i+3]=0;
  for(var n=0;n<6;n++)copy(sheet,flame,n*24,0);
  save(sheet,ONLY);
 }else{
  var water=crop(scene,376,129,64,96),sheet=blank(384,96);
  for(var n=0;n<6;n++)for(var y=0;y<96;y++)for(var x=0;x<64;x++){
   var i=(y*64+x)*4,j=(((y+n*2)%96)*64+x)*4;
   if(!(water.p[i+1]>130&&water.p[i+2]>140&&water.p[i+2]>water.p[i]*1.1))continue;
   if(!(water.p[j+1]>130&&water.p[j+2]>140))continue;
   for(var k=0;k<4;k++)sheet.p[(y*384+n*64+x)*4+k]=water.p[j+k];
  }
  save(sheet,ONLY);
 }
}

function scenery(){
 var a=blank(941,706);copy(a,crop(ref,0,0,941,594),0,0);
 // Remove the reference player panel in full. This region is covered by the
 // live player frame, while its outer margin remains actor-free background.
 if(zone==='Dungeon'){
  repeat(a,ref,48,86,54,126,0,0,278,706);
  repeat(a,ref,105,1,102,48,331,36,308,94);
  repeat(a,ref,768,146,52,151,655,45,275,101);
  // Clear HUD counters and continue the arch/door's native stone and timber.
  repeat(a,ref,369,181,27,45,366,225,32,117);
  repeat(a,ref,452,218,91,71,398,228,78,118);
  repeat(a,ref,282,277,70,44,352,232,14,79);
  repeat(a,ref,632,137,49,84,597,229,82,80);
  // Restore the hidden lower doorway and left pier behind the mage.
  repeat(a,ref,452,218,91,71,397,311,156,44);
  repeat(a,ref,369,181,27,45,365,310,32,47);
  repeat(a,ref,302,277,28,44,329,321,68,38);
  // Source floor courses are actor-free below the HP strips. Extend them at
  // their original scale, offsetting alternate courses for natural masonry.
  for(var y=357;y<706;y+=42){
   var floor=crop(ref,278,550,657,42);copy(a,floor,278,y);
  }
  // Preserve complete side props outside the actor occlusion region.
  patch(a,ref,808,330,133,188,808,330);
  patch(a,ref,276,321,54,132,276,321);
 }else if(zone==='Forest'){
  repeat(a,ref,46,88,53,125,0,0,278,706);
  repeat(a,ref,281,0,109,43,327,36,305,99);
  repeat(a,ref,635,0,173,45,651,52,275,92);
  // Counters obscure tree bark and the far pool: replace with local texture.
  repeat(a,ref,417,150,28,66,366,229,78,77);
  patch(a,ref,655,159,59,71,605,231,65,73);
  // Restore a continuous distant woodland layer behind the actor masks.
  for(var x=306,n=0;x<814;x+=80,n++){
   var undergrowth=extract(ref,278,210,88,115,88,132);
   mask(undergrowth,[[0,17],[10,12],[13,4],[29,0],[48,8],[64,0],[88,9],[88,132],[0,132]]);
   copy(a,undergrowth,x,294,n%2===1);
  }
  // Continue the original trunk without duplicating the distant deer.
  patch(a,ref,444,246,66,74,444,310,66,112);
  patch(a,ref,475,309,35,102,475,309);
  // The original narrow waterfall strip contains no deer, actors or UI.
  patch(a,ref,698,183,41,120,706,310,41,120);
  patch(a,ref,750,316,65,106,747,316);
  // Clean trail courses beneath the figures and health strips.
  repeat(a,ref,510,455,26,48,275,420,552,286);
  for(var n=0;n<5;n++){
   var grass=extract(ref,347+n*37,550,86,28,86,28);
   mask(grass,[[0,12],[15,4],[32,0],[55,4],[76,0],[86,14],[74,28],[17,28]]);
   copy(a,grass,303+(n*113)%420,435+n*46,n%2===1);
  }
  // Preserve the source's distinct sign, glowing flowers and mushroom bank.
  patch(a,ref,816,279,125,305,816,279);
  patch(a,ref,276,308,52,204,276,308);
 }else{
  repeat(a,ref,46,88,54,125,0,0,278,706);
  waterRepair(a,ref,315,36,323,99);
  waterRepair(a,ref,655,51,273,94);
  // Preserve the actual crowned trident statue, neighboring columns and rays.
  patch(a,ref,381,160,48,68,375,231,70,76);
  rect(a,619,231,72,78,'087e9e');
  copy(a,extract(ref,539,220,25,69,30,78),619,231,true);
  // Reconstruct the submerged arch behind the actor masks from its exposed
  // crown and clean pillar stone; no live actor is baked into the scene.
  rect(a,298,310,520,171,'075171');
  // Weathered native masonry restores the architecture concealed by actors.
  // Rows are staggered and chipped, using the marine source's local colors.
  for(var y=315,row=0;y<481;y+=31,row++)for(var x=298-(row%2)*25;x<818;x+=51){
   bevel(a,x+2,y+2,48,28,(row+Math.floor(x/51))%3===0?'106682':'0c5c79');
   line(a,x+6,y+3,x+42,y+3,'1a7892');line(a,x+4,y+5,x+3,y+20,'126b85');
   line(a,x+8,y+26,x+44,y+26,'084962');line(a,x+28,y+10,x+33,y+10,'116781');
  }
  // Trace the exposed ruined doorway, retaining its actual chipped crown.
  rect(a,517,326,108,155,'07384f');
  patch(a,ref,498,288,72,40,498,288);
  copy(a,extract(ref,498,288,72,40,72,40),570,288,true);
  repeat(a,ref,700,200,29,74,491,336,26,145);
  repeat(a,ref,700,200,29,74,625,336,26,145);
  patch(a,ref,698,173,35,133,350,334,35,147);
  repeat(a,ref,268,1637,173,33,275,481,543,225);
  for(var n=0;n<4;n++){
   var coral=extract(ref,511,430,57,43,57,43);
   mask(coral,[[13,17],[17,9],[28,12],[31,0],[41,0],[46,14],[57,12],[57,43],[7,43],[0,23]]);
   copy(a,coral,304+n*125,439+(n%2)*9,n%2===1);
  }
  patch(a,ref,818,290,123,300,818,290);
  patch(a,ref,277,377,49,113,277,377);
 }
 // Keep every exposed part of the actual woodland / ruin vista. Only the
 // explicitly traced character, equipment and HP masks receive reconstruction.
 if(zone!=='Dungeon'){
  var actors=zone==='Forest'?[
   [[307,329],[367,329],[424,335],[471,357],[477,380],[499,398],[503,488],[480,511],[351,513],[332,484],[324,440],[302,403]],
   [[511,322],[541,318],[545,303],[584,303],[617,311],[649,302],[677,312],[681,338],[722,356],[749,400],[750,470],[722,504],[691,517],[552,515],[540,482],[535,441],[509,399]]
  ]:[
   [[299,309],[350,307],[377,326],[400,319],[420,326],[425,341],[457,343],[466,373],[500,382],[513,414],[515,487],[498,511],[440,520],[351,516],[331,460],[330,408],[302,375]],
   [[541,313],[579,324],[603,327],[626,309],[678,313],[724,339],[747,389],[763,435],[798,446],[823,443],[821,487],[794,515],[640,525],[571,516],[574,443],[554,399],[533,361]]
  ];
  for(var y=310;y<550;y++)for(var x=276;x<818;x++){
   var hp=y>=504&&y<548&&((x>=321&&x<514)||(x>=534&&x<754));
   if(hp||inside(x,y,actors[0])||inside(x,y,actors[1]))continue;
   var i=(y*a.w+x)*4,j=(y*ref.w+x)*4;for(var k=0;k<4;k++)a.p[i+k]=ref.p[j+k];
  }
  if(zone==='Court'){
   patch(a,ref,275,550,666,44,275,550);
   for(var y=594;y<706;y+=44)repeat(a,ref,321,550,488,44,275,y,666,44);
  }
 }
 // A 512 x 384 native scene keeps the established tilemap/floor registration.
 // Nearest pixel selection is performed once here; Unity renders at integer scale.
 save(extract(a,0,0,941,706,512,384),'Scene/'+zone);
}

if(ONLY.indexOf('Motion/')===0)motion();
else if(ONLY.indexOf('Scene/')===0)scenery();
else if(/\/Cell[A-C]$/.test(ONLY))cell();
else if(ONLY.indexOf('/Player')>0)playerFrame();
else if(ONLY.indexOf('/Edge')>0||ONLY.indexOf('/Corner')>0)frame();
else if(ONLY.indexOf('/Plaque')>0)plaque();
else materials();
