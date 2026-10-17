// Native LibreSprite reconstruction for the three-enemy composition.
// All coordinates are on the ONE 470 x 836 sampled reference grid.
// Counter / actor repairs are explicit masks, not generative reinterpretation.
var wall=crop(N,0,0,470,180);
var waveShape=[[175,22],[295,22],[303,29],[303,54],[296,59],[249,59],[249,66],[231,66],[228,59],[176,59],[169,52],[169,30]];
rect(wall,11,28,129,152,'141a2c');
poly(wall,waveShape,'171421');rect(wall,330,30,79,39,'171421');rect(wall,414,30,45,40,'171421');

function stoneCourse(a,x,y,w,h,base,light,shadow){
 rect(a,x,y,w,h,'101020');
 poly(a,[[x+2,y+1],[x+w-3,y+1],[x+w-1,y+3],[x+w-2,y+h-2],[x+2,y+h-1],[x+1,y+h-3]],base);
 line(a,x+3,y+1,x+w-4,y+1,light);
 line(a,x+1,y+3,x+1,y+h-4,light);
 line(a,x+3,y+h-2,x+w-4,y+h-2,shadow);
}
// Patch only the three small statistic clusters. The door itself remains intact.
var patches=[[167,133,37,40],[281,133,36,40],[387,133,39,40]];
for(var n=0;n<patches.length;n++){
 var q=patches[n],patch=blank(q[2],q[3]);
 for(var row=0;row<3;row++){
  var cuts=row%2?[0,15,q[2]]:[0,24,q[2]];
  for(var col=0;col<2;col++)stoneCourse(patch,cuts[col],row*14,cuts[col+1]-cuts[col],14,
   n===2?'22203c':'29223f',n===2?'34304f':'3c2c52','181629');
 }
 copy(wall,patch,q[0],q[1]);
}
// Continue the arch's two legs across the hidden counters, using connected
// violet blocks and a dark inner edge. Door planks / hinges are original pixels.
for(var row=0;row<3;row++){
 stoneCourse(wall,195,132+row*15,20,15,row%2?'372344':'402a51','61436e','241b35');
 stoneCourse(wall,286,132+row*15,19,15,row%2?'2b203d':'322440','483252','1a172c');
}
// The mage's orb extends just above the floor: continue the unobscured masonry.
stoneCourse(wall,145,168,33,12,'292441','3b2d50','211b30');
repairs.push({name:'three enemy stat clusters and staff orb',rects:patches.concat([[145,168,28,12]]),reason:'Remove baked swords, numbers and purple orb; redraw concealed masonry and arch courses.'});
var archShape=[[226,59],[251,59],[275,68],[292,84],[304,107],[308,128],[308,180],[193,180],[193,122],[199,99],[211,78]];
var architecture=cut(crop(wall,0,0,470,180),archShape,true);
cut(wall,archShape,false);

var floor=crop(N,137,177,333,121),restored=blank(333,121);
var courses=[177,182,188,195,203,212,222,234,247,260,275];
var bases=['25223f','292441','2d2646','32294c','382d52','3e3159','453660','4a3965','4f3c68','533e6a'];
for(var row=0;row<courses.length-1;row++){
 var y0=courses[row]-177,h=courses[row+1]-courses[row];
 var pitch=23+row*5,offset=row%2?-pitch/2:0;
 rect(restored,0,y0,333,h,'29233e');
 var c=0;
 for(var x=Math.floor(offset)-pitch;x<333;x+=pitch){
  var drift=Math.round((x-115)*.018),b=bases[Math.max(0,row-(c++%3===1?1:0))];
  poly(restored,[[x+1,y0+1],[x+pitch-2,y0+1],[x+pitch+drift-1,y0+h-1],[x+drift+1,y0+h-1]],b);
  line(restored,x+2,y0+1,x+pitch-4,y0+1,row<4?'463359':row<8?'5a4169':'674c75');
  if(row>3){line(restored,x+6,y0+3,x+12,y0+3,'4d385e');line(restored,x+pitch-12,y0+h-3,x+pitch-7,y0+h-3,'382b4d');}
 }
}
// Explicit actor and HP masks, including disconnected weapons and badges.
var actors=[
 [[145,174],[153,168],[166,171],[174,180],[173,191],[178,190],[179,184],[204,180],[219,187],[222,206],[226,222],[229,246],[226,255],[170,255],[167,241],[157,241],[155,203],[149,195]],
 [[243,190],[248,190],[268,215],[276,206],[275,190],[287,183],[301,185],[311,197],[313,208],[324,207],[335,214],[338,236],[333,251],[331,256],[275,256],[272,241],[261,238],[258,222]],
 [[349,185],[359,182],[365,193],[376,187],[389,181],[407,183],[417,195],[420,214],[427,235],[432,255],[374,256],[367,249],[360,234],[352,222],[349,203]]
];
function occluded(x,y){
 if(y>=255&&y<275&&x>=143&&x<443)return true;
 for(var n=0;n<actors.length;n++)if(inside(x+.5,y+.5,actors[n]))return true;
 return false;
}
// A continuous floor avoids disconnected props and conspicuous patch edges.
for(var y=177;y<275;y++)for(var x=137;x<470;x++){
 var i=((y-177)*333+x-137)*4;
 for(var k=0;k<4;k++)floor.p[i+k]=restored.p[i+k];
}
repairs.push({name:'three actors, weapons and health bars',polygons:actors,rects:[[137,177,333,98]],reason:'Continuous quiet flagstone floor redrawn to remove disconnected weapon pixels and repair seams. Source front foundation below row 275 retained.'});

// Cut props into their own native layers. Their former locations are filled
// with restrained material planes, then the same exact silhouettes are placed
// over them. This leaves each foreground object independently editable.
var foreground=blank(470,298),propArt=[];
function prop(name,x,y,w,h,shape,fill){
 var a=cut(crop(N,x,y,w,h),shape,true);
 // The bow crosses the rear bone pile in the source. It contributes warm
 // gold/wood fragments that are not part of the lavender bones.
 if(name==='RearBones'){
  for(var i=0;i<a.p.length;i+=4)
   if(a.p[i]>a.p[i+2]*1.25 && a.p[i+1]>a.p[i+2]*1.1)a.p[i+3]=0;
  // The arrow grip/white hand crosses the lower pile. Reconstruct two fallen
  // bones below the retained skull, instead of leaving a limb-like fragment.
  rect(a,0,20,w,h-20,null);
  poly(a,[[1,25],[3,21],[6,22],[15,17],[18,18],[19,21],[15,23],[7,27],[4,29],[1,28]],'312747');
  line(a,4,25,16,19,'958ba1');line(a,5,26,16,20,'7a7287');
  rect(a,3,23,3,3,'b2a8bd');rect(a,15,18,3,3,'b2a8bd');
  poly(a,[[8,25],[10,22],[14,23],[23,26],[26,25],[28,28],[26,31],[23,30],[12,27],[10,28]],'312747');
  line(a,12,24,25,28,'958ba1');line(a,12,25,24,29,'7a7287');
  rect(a,10,23,3,3,'b2a8bd');rect(a,24,27,3,3,'b2a8bd');
 }
 for(var Y=0;Y<h;Y++)for(var X=0;X<w;X++)if(a.p[(Y*w+X)*4+3]){
  if(y+Y<180)px(wall,x+X,y+Y,fill);
  if(y+Y>=177&&x+X>=137)px(floor,x+X-137,y+Y-177,fill);
 }
 copy(foreground,a,x,y);propArt.push({name:name,a:a,x:x,y:y});
}
prop('TopLeftTorch',135,95,25,65,[[11,0],[18,13],[20,24],[24,31],[24,43],[19,48],[18,62],[13,65],[9,59],[9,48],[2,43],[0,30],[5,23],[7,11]],'30223a');
prop('TopRightTorch',359,93,26,67,[[12,0],[17,13],[22,24],[25,32],[25,44],[19,49],[17,63],[13,67],[10,63],[9,48],[3,44],[1,33],[5,22],[7,10]],'39243d');
prop('RearBarrels',319,146,35,46,[[5,0],[30,1],[33,6],[33,20],[35,25],[34,42],[27,46],[5,44],[1,39],[0,21],[3,17]],'211b30');
prop('NearRightBarrel',421,189,38,46,[[5,0],[30,0],[35,4],[36,12],[35,40],[29,45],[7,46],[1,40],[0,10]],'362a49');
prop('NearLeftBones',137,199,24,40,[[0,1],[10,0],[18,7],[17,18],[23,23],[24,31],[16,36],[6,40],[0,35],[4,27],[0,20]],'423356');
prop('NearRightBones',436,210,34,38,[[18,0],[28,5],[34,10],[34,29],[28,36],[12,38],[0,33],[3,24],[12,21],[10,12]],'423356');
prop('RearBones',341,183,28,34,[[10,0],[21,1],[27,8],[25,19],[21,23],[27,28],[19,34],[8,29],[0,29],[0,24],[8,22],[8,17],[6,8]],'362a49');
// Reassemble editable background strata in the same fixed reference coordinates.
emit('FarWall',wall,0,0,'Rear masonry, chains and banners with counters removed','background');
emit('ArchDoor',crop(architecture,193,59,115,121),193,59,'Centered arched door and reconstructed leg stones','background');
emit('BattleFloor',floor,137,177,'Quiet repaired floor, native perspective and actor-free surface','background');
emit('ForegroundProps',foreground,0,0,'Separate warm torch pair, barrels and near bones','foreground');
for(var n=0;n<propArt.length;n++){var p=propArt[n];emit(p.name,p.a,p.x,p.y,'Independently editable foreground prop','prop-module',false);}
emit('PlayerFootMasonry',crop(N,0,280,137,18),0,280,'Source masonry below player panel','background');
var surround=crop(N,0,28,140,252);rect(surround,11,0,129,252,'141a2c');
emit('PlayerSurround',surround,0,28,'Left outer masonry and under-frame backing','background');
