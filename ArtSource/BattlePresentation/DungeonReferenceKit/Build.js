// Executed by LibreSprite. All parts share ONE native coordinate grid.
// The supplied target is read-only; only this isolated review kit is written.
var ROOT='__KIT__/', PROJECT='__PROJECT__/', OUTPUT='__OUTPUT__/';
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function crop(a,x,y,w,h){var b=blank(w,h);for(var Y=0;Y<h;Y++)for(var X=0;X<w;X++)for(var k=0;k<4;k++)b.p[(Y*w+X)*4+k]=a.p[((y+Y)*a.w+x+X)*4+k];return b;}
function copy(a,b,x,y){for(var Y=0;Y<b.h;Y++)for(var X=0;X<b.w;X++){var i=(Y*b.w+X)*4,j=((y+Y)*a.w+x+X)*4;if(x+X<0||x+X>=a.w||y+Y<0||y+Y>=a.h||!b.p[i+3])continue;for(var k=0;k<4;k++)a.p[j+k]=b.p[i+k];}}
function px(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)return;var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=c?parseInt(c.substr(2*k,2),16):0;a.p[i+3]=c?255:0;}
function color(a,x,y){var s='',i=(y*a.w+x)*4;for(var k=0;k<3;k++)s+=('0'+a.p[i+k].toString(16)).slice(-2);return s;}
function rect(a,x,y,w,h,c){for(var Y=y;Y<y+h;Y++)for(var X=x;X<x+w;X++)px(a,X,Y,c);}
function inside(x,y,p){var b=false;for(var i=0,j=p.length-1;i<p.length;j=i++){var A=p[i],B=p[j];if((A[1]>y)!=(B[1]>y)&&x<(B[0]-A[0])*(y-A[1])/(B[1]-A[1])+A[0])b=!b;}return b;}
function poly(a,p,c){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(inside(x+.5,y+.5,p))px(a,x,y,c);}
function cut(a,p,keep){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(inside(x+.5,y+.5,p)!==keep)px(a,x,y,null);return a;}
function line(a,x,y,X,Y,c){var n=Math.max(Math.abs(X-x),Math.abs(Y-y));for(var t=0;t<=n;t++)px(a,Math.round(x+(X-x)*t/Math.max(n,1)),Math.round(y+(Y-y)*t/Math.max(n,1)),c);}
var input=read(ROOT+'reference/ApprovedDungeon.png'),N=blank(470,836);
for(var y=0;y<836;y++)for(var x=0;x<470;x++)for(var k=0;k<4;k++)N.p[(y*470+x)*4+k]=input.p[((y*2+1)*941+x*2+1)*4+k];
var modules=[],repairs=[],composite=blank(470,836);
function makeDocument(a,path,name){
 app.open(PROJECT+'Assets/_Game/Resources/UI/Consumables/Potion.png');
 var s=app.activeSprite;s.saveAs(path,false);s.resize(a.w,a.h);s.commit();
 app.command.BackgroundFromLayer();s.commit();
 s.layer(0).cel(0).image.putImageData(a.p);s.layer(0).name=name;s.commit();s.saveAs(path,true);app.activeDocument.close();
 var q=read(path);if(q.w!==a.w||q.h!==a.h)throw Error('Dimensions '+name);
 for(var i=0;i<a.p.length;i++)if(q.p[i]!==a.p[i])throw Error('Reopened pixels '+name+' '+i);
}
function emit(name,a,x,y,role,group,compose){
 makeDocument(a,OUTPUT+name+'.aseprite',name);
 modules.push({name:name,x:x,y:y,width:a.w,height:a.h,role:role,group:group,composite:compose!==false});
 if(compose!==false)copy(composite,a,x,y);
 console.log('KIT_ASSET_OK '+name);
}
function clone(a,sx,sy,w,h,dx,dy){copy(a,crop(N,sx,sy,w,h),dx,dy);}
function repeatedMaterial(a,sx,sy,w,h,x,y,W,H){for(var Y=0;Y<H;Y++)for(var X=0;X<W;X++){var i=(((sy+Y%h)*N.w)+sx+X%w)*4,j=((y+Y)*a.w+x+X)*4;for(var k=0;k<4;k++)a.p[j+k]=N.p[i+k];}}
// Constrained source-patch restoration: copies existing floor pixels into the
// occluded regions, matching their visible neighbors and source depth. No
// generation, interpolation, color filtering or changes outside the mask.
function repairFloor(a,masks){
 var w=a.w,h=a.h,known=new Uint8Array(w*h),original=crop(a,0,0,w,h),missing=0;
 function hidden(x,y){return inside(x+.5,y+.5,masks[0])||inside(x+.5,y+.5,masks[1])||(y>=251&&y<274&&x>=160&&x<378);}
 function floorOnly(x,y){return x>=140&&x<433&&y>=179&&y<290&&!(x<173&&y<234)&&!(x>=340&&x<378&&y<206)&&!(x>=361&&y<226)&&!(x>=398&&y<251)&&!(x>=418&&y<274);}
 for(var y=0;y<h;y++)for(var x=0;x<w;x++){var k=y*w+x;known[k]=hidden(x+137,y+177)?0:1;if(!known[k])missing++;}
 var candidates=[];
 for(var y=8;y<h-8;y+=2)for(var x=8;x<w-8;x+=2){
  var valid=true;
  for(var yy=-7;yy<=7&&valid;yy++)for(var xx=-7;xx<=7;xx++)if(!known[(y+yy)*w+x+xx]||!floorOnly(x+xx+137,y+yy+177)){valid=false;break;}
  if(valid)candidates.push([x,y]);
 }
 if(!candidates.length)throw Error('No source floor patches');
 var steps=0;
 while(missing>0&&steps++<2500){
  var tx=-1,ty=-1,most=-1;
  // Prefer patches with many existing border pixels, so joints continue inward.
  for(var y=0;y<h;y++)for(var x=0;x<w;x++)if(!known[y*w+x]){
   var score=0;
   for(var yy=-6;yy<=6;yy+=3)for(var xx=-6;xx<=6;xx+=3)if(y+yy>=0&&y+yy<h&&x+xx>=0&&x+xx<w&&known[(y+yy)*w+x+xx])score++;
   if(score>most){most=score;tx=x;ty=y;}
  }
  var samples=[];
  for(var yy=-6;yy<=6;yy+=3)for(var xx=-6;xx<=6;xx+=3)if(ty+yy>=0&&ty+yy<h&&tx+xx>=0&&tx+xx<w&&known[(ty+yy)*w+tx+xx])samples.push([xx,yy,((ty+yy)*w+tx+xx)*4]);
  var best=1e30,bx=0,by=0;
  for(var c=0;c<candidates.length;c++){
   var cx=candidates[c][0],cy=candidates[c][1],dy=Math.abs(cy-ty);
   if(dy>12)continue;
   var error=dy*dy*30*samples.length;
   for(var t=0;t<samples.length;t++){
    var s=samples[t],si=((cy+s[1])*w+cx+s[0])*4;
    for(var k=0;k<3;k++){var dif=a.p[s[2]+k]-original.p[si+k];error+=dif*dif;}
    if(error>=best)break;
   }
   if(error<best){best=error;bx=cx;by=cy;}
  }
  if(best===1e30)throw Error('No floor donor at row '+ty);
  for(var yy=-7;yy<=7;yy++)for(var xx=-7;xx<=7;xx++){
   var X=tx+xx,Y=ty+yy;if(X<0||Y<0||X>=w||Y>=h||known[Y*w+X])continue;
   var si=((by+yy)*w+bx+xx)*4,di=(Y*w+X)*4;
   for(var k=0;k<4;k++)a.p[di+k]=original.p[si+k];known[Y*w+X]=1;missing--;
  }
 }
 if(missing)throw Error('Unfinished floor repairs: '+missing);
 console.log('FLOOR_REPAIR '+steps+' bounded patches from '+candidates.length+' source candidates');
}

// 1. The wall keeps the source's camera, arches, chains, torch light and values.
// Separate assets cover the precise control silhouettes. Nothing moves to fit.
var wall=crop(N,0,0,470,180);
rect(wall,11,28,129,152,'141a2c'); // Covered by separate complete player panel.
var waveShape=[[175,22],[295,22],[303,29],[303,54],[296,59],[249,59],[249,66],[231,66],[228,59],[176,59],[169,52],[169,30]];
poly(wall,waveShape,'171421');rect(wall,330,30,79,39,'171421');rect(wall,414,30,45,40,'171421');
// Existing counter rectangles are removed locally, with matching doorway/wall
// planes. The visible source architecture outside them is never repainted.
clone(wall,181,91,17,26,181,117);clone(wall,181,91,17,15,181,143);
clone(wall,235,117,24,41,198,117);
clone(wall,295,153,46,24,295,116);clone(wall,296,154,45,18,296,140);
repairs.push({name:'enemy counters',rects:[[178,116,46,42],[295,116,47,42]],reason:'Sword/ability text conceals arch and masonry.'});
// Complete the few pixels concealed by the mage's hat above the floor line.
clone(wall,237,161,28,19,196,161);
clone(wall,160,140,30,17,160,163);
var archShape=[[223,58],[251,59],[273,71],[289,93],[294,117],[294,180],[180,180],[180,113],[188,92],[202,74]];
var architecture=cut(crop(wall,0,0,470,180),archShape,true);
cut(wall,archShape,false);
emit('FarWall',wall,0,0,'Original back-wall plane, lighting and chains','background');
emit('ArchDoor',crop(architecture,180,58,115,122),180,58,'Registered mid-layer arched door','background');

// 2. Floor reconstruction changes ONLY the silhouettes hidden by the actors.
// Preserve the original horizon, converging stone joints and shallow depth.
var floor=crop(N,137,177,333,121);
var actorMasks=[
 [[160,177],[164,164],[180,167],[188,174],[207,172],[223,174],[234,182],[239,193],[244,201],[245,215],[250,232],[248,251],[183,251],[180,227],[169,211],[166,192]],
 [[265,183],[273,181],[283,202],[292,195],[298,181],[311,175],[326,176],[336,193],[340,203],[352,206],[357,215],[358,237],[350,251],[288,251],[284,233],[275,222]]
];
// Trace the concealed stone planes. Growing course heights and converging
// joints follow the source, instead of repeating a narrow strip.
var restored=crop(N,137,177,333,121);
var courses=[177,182,187,193,201,210,219,232,246,254,272,290,298];
var cuts=[[-10,25,52,77,109,142,181,220,270,334],[-10,16,43,72,105,140,178,221,269,334],[-10,27,59,91,128,168,210,258,334],[-10,8,47,84,123,169,216,268,334],[-10,29,72,115,161,211,270,334],[-10,7,55,103,153,209,270,334],[-10,33,86,141,200,265,334],[-10,10,69,131,195,265,334],[-10,39,101,167,235,308,334],[-10,15,78,145,218,298,334],[-10,43,118,195,275,334],[-10,10,93,180,269,334]];
var bases=['29234c','2b254f','322959','3b2c60','523b73','593f77','51396e','543a71','59416e','604370','604370','292342'];
for(var row=0;row<courses.length-1;row++){
 var y0=courses[row]-177,y1=courses[row+1]-177,cs=cuts[row];
 rect(restored,0,y0,333,y1-y0,'2d244c');
 for(var c=0;c<cs.length-1;c++){
  var x0=cs[c],x1=cs[c+1],drift=Math.round((x0-100)*.025);
  var shape=[[x0+2,y0+1],[x1-3,y0+1],[x1+drift-1,y1-3],[x1+drift-4,y1-1],[x0+drift+1,y1-1],[x0+drift-1,y1-3]];
  poly(restored,shape,bases[row]);
  line(restored,x0+3,y0+1,x1-5,y0+1,row>5?'77517f':row>3?'64447d':'463365');
  if(row>3){line(restored,x0+5,y1-3,x0+11,y1-3,'664578');line(restored,x1-13,y0+3,x1-9,y0+3,'493466');}
 }
}
// Restore texture from the immutable reference, then use the traced planes to
// remove broken seams inside the same mask. Visible original floor stays intact.
floor=crop(N,137,177,333,121);repairFloor(floor,actorMasks);
// Clean the cloned patches into connected stone faces. Keep the source grain
// within each face; remove the patch seams and retrace the concealed joints.
for(var y=177;y<274;y++)for(var x=137;x<470;x++){
 var hidden=inside(x+.5,y+.5,actorMasks[0])||inside(x+.5,y+.5,actorMasks[1])||(y>=251&&x>=160&&x<378);
 if(!hidden)continue;
 var row=0;while(row<courses.length-2&&courses[row+1]<=y)row++;
 var X=x-137,Y=y-177,i=(Y*333+X)*4,face=color(restored,X,Y)===bases[row],near=true;
 for(var k=0;k<3;k++)if(Math.abs(floor.p[i+k]-restored.p[i+k])>16)near=false;
 if(!face||!near)for(var k=0;k<4;k++)floor.p[i+k]=restored.p[i+k];
}
repairs.push({name:'battle actors and HP',polygons:actorMasks,rects:[[160,252,94,22],[267,252,111,22]],reason:'Only hidden floor patches reconstructed; original visible floor retained.'});
emit('BattleFloor',floor,137,177,'Shallow perspective floor; no extra rows','background');
emit('PlayerFootMasonry',crop(N,0,280,137,18),0,280,'Original masonry below left panel','background');
var surround=crop(N,0,28,140,252);rect(surround,11,0,129,252,'141a2c');
emit('PlayerSurround',surround,0,28,'Left outer masonry and under-frame backing','background');

// 3. Preserve the actual irregular panel masonry, restoring its occluded areas.
var panel=crop(N,22,42,105,224);
var panelRepair=crop(panel,0,0,105,224);
var panelCourses=[0,14,37,61,84,106,129,151,172,194,217,224];
var panelCuts=[[-3,28,66,110],[-2,45,86,110],[-3,23,65,110],[-4,43,83,110],[-2,20,65,110],[-3,45,85,110],[-2,22,63,110],[-2,46,86,110],[-3,23,63,110],[-4,43,84,110],[-2,22,66,110]];
for(var r=0;r<panelCourses.length-1;r++){
 var y0=panelCourses[r],y1=panelCourses[r+1],xs=panelCuts[r];
 rect(panelRepair,0,y0,105,y1-y0,'0e1324');
 for(var c=0;c<xs.length-1;c++){
  var x0=xs[c],x1=xs[c+1];
  poly(panelRepair,[[x0+1,y0+2],[x1-2,y0+1],[x1-1,y0+3],[x1-1,y1-2],[x0+2,y1-1],[x0+1,y1-3]],(r+c)%3===0?'1b2238':'192036');
  line(panelRepair,x0+3,y0+2,x1-4,y0+2,'30324b');
  line(panelRepair,x0+2,y0+4,x0+2,y1-4,'252a42');
  // Sparse, connected chips rather than a repeated texture stamp.
  if((r+c)%2===0){rect(panelRepair,x0+7,y0+7,3,2,'22273f');px(panelRepair,x0+9,y0+6,'22273f');}
  else {line(panelRepair,x1-10,y1-6,x1-7,y1-6,'12192c');px(panelRepair,x1-8,y1-7,'12192c');}
 }
}
// Source courses are matched vertically; the banner/gem/actor will be separate.
for(var y=0;y<224;y++)for(var x=0;x<105;x++){
 var gx=x+22,gy=y+42;
 var hidden=(gx>=48&&gx<101&&gy<107)||(gx>=52&&gx<96&&gy>=108&&gy<147)||
  (gx>=28&&gx<123&&gy>=147&&gy<238)||(gy>=237&&gy<257);
 if(!hidden)continue;
 var j=(y*105+x)*4;for(var k=0;k<4;k++)panel.p[j+k]=panelRepair.p[j+k];
}
repairs.push({name:'player panel',rects:[[48,42,53,65],[52,108,44,39],[28,147,95,110]],reason:'Original mascot, affinity and HP are excluded from production background.'});
emit('PlayerMasonry',panel,22,42,'Reference masonry with local repairs behind live content','panel');
var pf=crop(N,11,28,129,252);
rect(pf,11,14,105,223,null);
cut(pf,[[10,0],[22,5],[106,5],[119,0],[129,10],[125,21],[125,235],[129,244],[119,252],[108,247],[22,247],[10,252],[0,243],[5,233],[5,22],[0,11]],true);
emit('PlayerFrame',pf,11,28,'Complete faithful thin player frame; transparent center','frame');
var banner=crop(N,51,42,49,65);cut(banner,[[0,0],[49,0],[48,58],[39,53],[24,65],[10,54],[1,60]],true);
emit('PlayerBanner',banner,51,42,'Original hanging skull banner','prop');

// 4. Exact frame positions and source thickness. Labels remain live later.
var wave=crop(N,169,21,135,47);cut(wave,waveShape.map(function(p){return [p[0]-169,p[1]-21];}),true);
rect(wave,18,9,104,20,'171721');
emit('WaveHeader',wave,169,21,'Purple plaque and skull; no baked wave text','frame');
var guide=crop(N,330,30,79,39);rect(guide,9,12,61,16,'1b1d2b');
cut(guide,[[5,0],[74,0],[79,6],[79,33],[73,39],[5,39],[0,33],[0,6]],true);
emit('SharedGuide',guide,330,30,'Shared silver Guide plate; no label','shared');
var settings=crop(N,414,30,45,40);cut(settings,[[5,0],[40,0],[45,5],[45,35],[40,40],[5,40],[0,35],[0,5]],true);
emit('SharedSettings',settings,414,30,'Existing reference silver gear control','shared');

// 5. Board modules keep ALL four independent corners and unique beam segments.
// This is an eight-by-eight art layout study, not a board-dimension code change.
var boardFrame=crop(N,37,295,400,406);rect(boardFrame,16,18,364,369,null);
cut(boardFrame,[[7,0],[388,0],[400,10],[400,396],[389,406],[7,406],[0,397],[0,8]],true);
emit('BoardFrame',boardFrame,37,295,'Reference frame at exact source proportions','frame');
var cells=blank(364,369);
for(var row=0;row<8;row++)for(var col=0;col<8;col++){
 var x=Math.round(col*364/8),y=Math.round(row*369/8),w=Math.round((col+1)*364/8)-x,h=Math.round((row+1)*369/8)-y;
 var cell=crop(N,53+x,313+y,w,h);
 // Remove bright gem tips that touch the bevel before emptying the center.
 for(var yy=0;yy<h;yy++)for(var xx=0;xx<w;xx++){
  var i=(yy*w+xx)*4,R=cell.p[i],G=cell.p[i+1],B=cell.p[i+2];
  if(Math.max(R,G,B)>105&&(Math.max(R,G,B)-Math.min(R,G,B)>32||Math.min(R,G,B)>90))px(cell,xx,yy,yy<4?'273044':'111a2b');
 }
 // Empty the gem well while retaining the original uneven perimeter pixels.
 rect(cell,4,4,w-8,h-8,'172133');
 // Source cells are quiet rough stone, without a large new diagonal bevel.
 rect(cell,4,4,w-8,2,'1e293c');
 line(cell,4,6,4,h-9,'1e293c');
 rect(cell,7+(col*3)%9,8+(row*5)%12,2,2,'1d2739');
 line(cell,w-12,h-10,w-9,h-10,'131d2e');
 line(cell,6,h-6,w-6,h-6,'111a2b');
 copy(cells,cell,x,y);
 if(row===0&&col<3)emit('EmptyCell'+col,cell,53+x,313+y,'Empty stone cell sample; source rim retained','cell',false);
}
emit('BoardWell',cells,53,313,'Empty eight-by-eight well; no gem pixels','board');
// Cropped kit pieces are exported additionally, with exact master offsets.
var parts=[['BoardTL',0,0,24,24],['BoardTR',376,0,24,24],['BoardBL',0,382,24,24],['BoardBR',376,382,24,24],
 ['BoardTop',24,0,352,18],['BoardBottom',24,387,352,19],['BoardLeft',0,24,16,358],['BoardRight',380,24,20,358]];
for(var n=0;n<parts.length;n++){var p=parts[n];emit(p[0],crop(boardFrame,p[1],p[2],p[3],p[4]),37+p[1],295+p[2],'Source-registered frame module','frame-module',false);}

// 6. Outer strips use their own exact source pixels, not mirrored/repeated art.
emit('LeftBorderDressing',crop(N,0,298,37,403),0,298,'Left chains, torch, banner and skull masonry','prop');
emit('RightBorderDressing',crop(N,437,298,33,403),437,298,'Distinct right chains, torch, banner and skull masonry','prop');
// Isolated ornaments remain available with conservative native cutout contours.
var props=[['LeftTorch',0,397,36,72,[[10,0],[23,4],[29,21],[35,33],[35,48],[29,54],[28,67],[16,72],[6,65],[4,53],[0,45],[1,27],[9,18]]],
 ['RightTorch',440,396,30,72,[[10,0],[22,7],[28,25],[30,43],[28,51],[24,54],[24,68],[13,72],[4,64],[4,52],[0,46],[1,29],[8,19]]],
 ['LeftSkullBanner',0,477,28,91,[[0,0],[28,0],[27,91],[15,81],[0,87]]],
 ['RightSkullBanner',442,477,28,91,[[0,0],[28,0],[28,87],[13,81],[1,91]]],
 ['LeftBones',138,196,34,33,[[3,2],[12,0],[22,8],[34,15],[30,24],[21,29],[4,33],[0,24],[5,16],[0,8]]],
 ['RightBarrel',399,196,36,57,[[4,0],[31,0],[34,9],[34,41],[29,51],[5,51],[0,47],[0,11]]]];
for(var n=0;n<props.length;n++){var p=props[n],a=cut(crop(N,p[1],p[2],p[3],p[4]),p[5],true);emit(p[0],a,p[1],p[2],'Optional isolated ornament at source location; master uses registered backing','prop-module',false);}

// 7. The lower wall and corner pillars retain source placement. There is no
// new outer bottom enclosure. Only the three shared button rims and energy rim.
var bottom=crop(N,0,701,470,135);
var hiddenBottom=[[163,707,147,24],[69,732,70,64],[149,729,176,66],[333,733,70,64],[121,797,278,22]];
for(var b=0;b<hiddenBottom.length;b++){var q=hiddenBottom[b];repeatedMaterial(bottom,92,705,65,25,q[0],q[1]-701,q[2],q[3]);}
// The caption repair uses one continuous clean course, not repeated small stamps.
clone(bottom,121,817,278,19,121,797-701);
clone(bottom,121,833,278,3,121,816-701);
repairs.push({name:'under bottom controls',rects:hiddenBottom,reason:'Mostly covered by separate button assets; lower live caption excluded.'});
emit('BottomMasonry',bottom,0,701,'Original lower wall and distinct foreground corners','background');
var energy=crop(N,163,707,147,24);rect(energy,13,8,121,9,'130e20');
cut(energy,[[7,0],[139,0],[147,7],[147,19],[140,24],[6,24],[0,17],[0,7]],true);
emit('SharedEnergy',energy,163,707,'Empty shared energy frame','shared');
var ability=crop(N,149,729,176,66);rect(ability,14,10,148,44,'16071f');
// Preserve the reference's purple inset and source lightning away from the icon.
clone(ability,165,740,42,39,16,11);clone(ability,279,740,29,39,130,11);
cut(ability,[[10,0],[165,0],[176,10],[176,58],[167,66],[8,66],[0,58],[0,10]],true);
emit('SharedAbility',ability,149,729,'Shared silver/purple frame; character icon removed','shared');
for(var n=0;n<2;n++){
 var x=n===0?69:333,a=crop(N,x,733,70,63);
 rect(a,11,9,49,45,'777786');
 poly(a,[[11,9],[57,9],[57,13],[19,13],[19,48],[11,48]],'94939e');
 rect(a,19,18,4,2,'80808e');rect(a,48,40,5,2,'6c6b7b');
 // Restore the obscured lower-right bolt/rim from the opposite local corner.
 for(var yy=39;yy<63;yy++)for(var xx=48;xx<70;xx++){
  var si=(yy*70+69-xx)*4,di=(yy*70+xx)*4;
  for(var k=0;k<4;k++)a.p[di+k]=a.p[si+k];
 }
 cut(a,[[6,0],[64,0],[70,7],[70,55],[63,63],[6,63],[0,56],[0,6]],true);
 emit(n===0?'SharedPotionHolder':'SharedBombHolder',a,x,733,'Shared reference holder; live item and quantity excluded','shared');
}

// Review master is composed from the actual registered exported modules.
makeDocument(composite,OUTPUT+'DungeonArtOnly.aseprite','DungeonArtOnly');
console.log('KIT_MANIFEST '+JSON.stringify({size:[470,836],modules:modules,repairs:repairs}));
console.log('KIT_COMPLETE');
