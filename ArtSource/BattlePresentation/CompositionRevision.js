// Third review: authored material repairs and coherent perspective, inside LibreSprite.
// Sources stay separate from runtime controls, actors and board cells.
var oldMaterials=materials, oldFrame=frame;
var nativeRef=extract(ref,0,0,941,1672,512,910);
var seed=zone==='Forest'?3427:zone==='Court'?8131:1949;
function random(){seed=(seed*1664525+1013904223)>>>0;return seed/4294967296;}
function sample(a,x,y){var i=(Math.max(0,Math.min(a.h-1,Math.round(y)))*a.w+Math.max(0,Math.min(a.w-1,Math.round(x))))*4,c='';for(var k=0;k<3;k++)c+=('0'+a.p[i+k].toString(16)).slice(-2);return c;}
function repairMarineCounters(a){
 // Remove complete counter silhouettes, including their dark backing. Clone
 // neighbouring native column/arm planes; extending a boundary pixel across
 // a row also extended stray white glyph pixels into visible horizontal bars.
 copy(a,crop(nativeRef,194,80,56,42),194,126);
 copy(a,extract(nativeRef,297,123,29,47,43,47),328,123);
 // The adjacent column is still visible to the right of the statue repair.
 copy(a,crop(nativeRef,378,123,7,47),371,123);
}
function texturedPolygon(a,points,src,sx,sy,sw,sh){
 var xs=points.map(function(p){return p[0];}),ys=points.map(function(p){return p[1];});
 var x0=Math.floor(Math.min.apply(null,xs)),x1=Math.ceil(Math.max.apply(null,xs));
 var y0=Math.floor(Math.min.apply(null,ys)),y1=Math.ceil(Math.max.apply(null,ys));
 for(var y=Math.max(0,y0);y<Math.min(a.h,y1);y++)for(var x=Math.max(0,x0);x<Math.min(a.w,x1);x++)if(inside(x+.5,y+.5,points))
  px(a,x,y,sample(src,sx+(x-x0)*sw/Math.max(1,x1-x0),sy+(y-y0)*sh/Math.max(1,y1-y0)));
}
// Quilted local material: source-sized patches joined along minimum-error seams.
// It copies real color clusters, without blurry interpolation or a repeating strip.
function quilt(w,h,pools,patchSize,overlap){
 var out=blank(w,h),step=patchSize-overlap;
 for(var y=0;y<h;y+=step)for(var x=0;x<w;x+=step){
  var best=null,bestCost=1e30;
  for(var n=0;n<36;n++){
   var p=pools[Math.floor(random()*pools.length)],sw=Math.min(p[2],patchSize*2),sh=Math.min(p[3],patchSize*2);
   var sx=p[0]+Math.floor(random()*Math.max(1,p[2]-sw)),sy=p[1]+Math.floor(random()*Math.max(1,p[3]-sh));
   var candidate=extract(ref,sx,sy,sw,sh,patchSize,patchSize),cost=0;
   for(var Y=0;Y<patchSize&&y+Y<h;Y++)for(var X=0;X<patchSize&&x+X<w;X++){
    if((x===0||X>=overlap)&&(y===0||Y>=overlap))continue;
    var i=((y+Y)*w+x+X)*4,j=(Y*patchSize+X)*4;
    for(var k=0;k<3;k++){var d=out.p[i+k]-candidate.p[j+k];cost+=d*d;}
   }
   if(cost<bestCost){bestCost=cost;best=candidate;}
  }
  // Per-row best seam avoids rectangular joins without smearing source pixels.
  var seam=[];
  if(x>0){
   var costs=[],prev=[];
   for(var Y=0;Y<patchSize&&y+Y<h;Y++){
    costs[Y]=[];prev[Y]=[];
    for(var X=0;X<overlap;X++){
     var err=0,i=((y+Y)*w+x+X)*4,j=(Y*patchSize+X)*4;
     for(var k=0;k<3;k++){var d=out.p[i+k]-best.p[j+k];err+=d*d;}
     var min=0,idx=X;if(Y>0){min=1e30;for(var k=Math.max(0,X-1);k<=Math.min(overlap-1,X+1);k++)if(costs[Y-1][k]<min){min=costs[Y-1][k];idx=k;}}
     costs[Y][X]=err+min;prev[Y][X]=idx;
    }
   }
   var row=costs.length-1,index=0;for(var k=1;k<overlap;k++)if(costs[row][k]<costs[row][index])index=k;
   for(var Y=row;Y>=0;Y--){seam[Y]=index;index=prev[Y][index];}
  }
  for(var Y=0;Y<patchSize&&y+Y<h;Y++)for(var X=0;X<patchSize&&x+X<w;X++){
   if(x>0&&X<seam[Y])continue;
   if(y>0&&Y<overlap/2)continue;
   var i=((y+Y)*w+x+X)*4,j=(Y*patchSize+X)*4;for(var k=0;k<4;k++)out.p[i+k]=best.p[j+k];
  }
 }
 return out;
}
function floorStones(a,y0,y1,court){
 var depth=y1-y0,rows=[0,7,15,24,34,45,57,70,84,99,115,132,150,169,189,210,232,255,279,304,330,357];
 rect(a,145,y0,367,depth,court?'123c4d':'171323');
 for(var r=0;r<rows.length-1&&y0+rows[r]<y1;r++){
  var y=y0+rows[r],Y=Math.min(y1,y0+rows[r+1]),grow=1+(y-y0)/100;
  var width=(court?27:25)*grow,x=145-width*(r%2?.6:.15);
  while(x<512){
   var W=width*(.7+random()*.7),spread=(Y-y)*.18;
   var pts=[[Math.round(x+2),y+2],[Math.round(x+W-2),y+1],[Math.round(x+W+spread-3),Y-3],[Math.round(x-spread+1),Y-2]];
   var tones=court?['365e68','416b74','315662','3b626d']:['3e344e','483a54','42354e','503e59'];
   var tone=tones[Math.floor(random()*tones.length)];poly(a,pts,tone);
   var pool=court?[[285,1640,96,25],[381,1643,80,22],[541,1651,63,18]]:[[287,551,71,26],[420,552,77,26],[552,552,80,25],[681,552,86,25]];
   var stone=pool[Math.floor(random()*pool.length)];
   texturedPolygon(a,pts,ref,stone[0],stone[1],stone[2],stone[3]);
   var light=court?'597f85':'6a526f',dark=court?'173c50':'241b32';
   if(random()<.4){var crack=x+W*(.3+random()*.4);line(a,crack,y+2,crack+2,y+5,dark);line(a,crack+2,y+5,crack+1,y+7,dark);}
   x+=W;
  }
 }
}
function restoreProps(a){
 if(zone==='Dungeon'){
  copyPolygon(a,nativeRef,[440,177,72,110],[440,177,72,110],[[0,0],[72,0],[72,110],[33,110],[33,83],[9,80],[0,57]]);
  copyPolygon(a,nativeRef,[150,176,22,74],[150,176,22,74],[[0,0],[21,0],[21,38],[22,40],[22,56],[18,62],[6,74],[0,74]]);
  copyPolygon(a,nativeRef,[395,197,43,47],[395,197,43,47],[[5,7],[15,0],[31,2],[40,22],[43,37],[23,47],[0,43],[0,22]]);
 }else if(zone==='Forest'){
  copyPolygon(a,nativeRef,[443,149,69,176],[443,149,69,176],[[0,8],[68,0],[69,176],[7,176],[0,152],[5,130],[0,118],[12,94],[5,66],[0,41]]);
  copyPolygon(a,nativeRef,[150,166,33,138],[150,166,33,138],[[0,0],[28,0],[33,48],[15,59],[25,78],[17,94],[28,123],[15,138],[0,138]]);
 }else{
  copyPolygon(a,nativeRef,[434,158,78,166],[434,158,78,166],[[11,0],[78,0],[78,166],[0,166],[0,132],[10,116],[9,93],[0,69],[4,34]]);
  copyPolygon(a,nativeRef,[150,209,26,68],[150,209,26,68],[[0,0],[14,0],[18,10],[26,28],[18,36],[19,58],[10,68],[0,68]]);
 }
}
function masonry(w,h,z){
 var a=blank(w,h),base=z==='Forest'?['30261e','493727','59412c','6b4d30']:z==='Court'?['081e2d','143448','1e4053','2d5261']:['100f1c','242037','302744','443454'];
 if(z==='Dungeon'&&ONLY===z+'/Panel')base=['0b1020','162034','1d2b44','35425c'];
 rect(a,0,0,w,h,base[0]);
 for(var y=0,r=0;y<h;y+=28,r++)for(var x=-(r%2)*29;x<w;x+=58){
  var color=base[1+(r+Math.floor(x/58)+12)%2];
  bevel(a,x+1,y+1,56,26,color);
  // Reference stone/wood faces retain their irregular material texture.
  var face=z==='Forest'?[52,163,40,38]:z==='Court'?[50,117,34,35]:ONLY===z+'/Panel'?[48,115,38,34]:[164,1418,64,35];
  texturedPolygon(a,[[x+3,y+3],[x+54,y+3],[x+54,y+23],[x+3,y+23]],ref,face[0],face[1],face[2],face[3]);
  poly(a,[[x+4,y+2],[x+54,y+2],[x+51,y+5],[x+5,y+5],[x+3,y+20],[x+2,y+21],[x+2,y+5]],base[3]);
  line(a,x+4,y+25,x+52,y+25,base[0]);
  var cx=x+9+Math.floor(random()*30),cy=y+9+Math.floor(random()*9);
  line(a,cx,cy,cx+4,cy,base[1]);line(a,cx+2,cy+1,cx+7,cy+1,base[1]);
  if(random()<.35){line(a,x+40,y+2,x+38,y+6,base[0]);line(a,x+38,y+6,x+41,y+9,base[1]);}
 }
 return a;
}
function clearing(w,h){
 var a=blank(w,h);rect(a,0,0,w,h,'aa8557');
 for(var n=0;n<90;n++){
  var x=Math.floor(random()*w),y=Math.floor(random()*h),W=12+Math.floor(random()*35),H=3+Math.floor(random()*8);
  poly(a,[[x,y],[x+W-8,y],[x+W-5,y+2],[x+W,y+2],[x+W,y+H-2],[x+W-10,y+H-2],[x+W-13,y+H],[x-3,y+H],[x-3,y+3]],n%3===0?'bd9463':n%3===1?'95764c':'b38b5d');
 }
 for(var n=0;n<36;n++){
  var x=Math.floor(random()*w),y=Math.floor(random()*h),W=12+Math.floor(random()*28);
  poly(a,[[x,y+3],[x+6,y+3],[x+8,y],[x+13,y],[x+15,y+2],[x+W-2,y+2],[x+W,y+5],[x+W-5,y+7],[x+4,y+7]],'7f873e');
  line(a,x+8,y+2,x+13,y+2,'9b9c49');line(a,x+17,y+4,x+W-3,y+4,'969b44');
 }
 return a;
}
scenery=function(){
 var a=blank(512,512);copy(a,crop(nativeRef,0,0,512,324),0,0);
 // Only a quiet material field is needed beneath the independent player panel.
 copy(a,masonry(150,512,zone),0,0);copy(a,crop(nativeRef,0,0,150,32),0,0);
 if(zone==='Dungeon'){
  // Roof courses behind the live wave/Guide panels, with no repeated long band.
  copy(a,masonry(151,54,zone),182,20);copy(a,masonry(140,44,zone),359,32);
  // Complete the original doorway behind the two counters and the mage.
  var door=crop(nativeRef,239,115,52,75);copy(a,door,212,117);copy(a,door,239,117);
  var pier=crop(nativeRef,200,97,16,29);copy(a,pier,199,120);copy(a,pier,199,149);copy(a,pier,199,178);
  copy(a,crop(nativeRef,297,149,28,45),326,124);copy(a,crop(nativeRef,297,149,19,45),354,124);
  copy(a,crop(nativeRef,239,115,52,75),216,117);
  copy(a,crop(nativeRef,239,115,52,75),216,174);
  copy(a,crop(nativeRef,198,97,23,29),176,174);
  floorStones(a,194,512,false);restoreProps(a);
 }else if(zone==='Forest'){
  // Continue the overhead canopy only where the target contains HUD lettering.
  copy(a,quilt(151,54,[[277,2,65,90]],22,6),182,20);
  copy(a,quilt(140,44,[[653,146,220,38]],18,5),359,32);
  copy(a,quilt(40,42,[[416,153,30,72],[468,132,33,90]],10,3),199,125);
  copy(a,extract(ref,649,170,48,57,34,42),328,124);
  // Rebuild a varied distant bank behind the removed figures, retaining the
  // actual rune tree, bridge, deer and waterfall around that occlusion.
  var bank=quilt(280,74,[[278,216,76,96],[475,303,36,95]],14,5);copy(a,bank,166,173);
  copyPolygon(a,nativeRef,[239,117,43,113],[239,117,43,113],[[0,0],[26,0],[29,26],[40,53],[43,113],[24,113],[18,68],[3,38]]);
  copyPolygon(a,nativeRef,[385,170,40,72],[385,170,40,72],[[9,0],[40,0],[40,52],[28,55],[20,72],[0,72],[3,56],[17,44]]);
  copy(a,quilt(362,276,[[485,451,48,38],[349,546,182,24],[548,545,171,28],[283,479,35,28]],12,4),150,236);
  // Trace the clearing's irregular grassy edges, not a repeated turf rectangle.
 }else{
  // Water above the UI is restored from complete nearby shafts, avoiding
  // interpolation between unrelated rows (the previous vertical streaks).
  copy(a,quilt(165,54,[[336,1,290,36]],16,5),173,20);
  copy(a,quilt(140,44,[[639,147,250,34]],16,5),359,32);
  copy(a,extract(ref,380,157,46,65,42,43),198,125);
  copy(a,extract(ref,540,205,35,71,44,44),328,124);
  // Native, weathered arch stones surrounding an open submerged recess.
  var ruin=quilt(286,100,[[495,325,40,99]],18,5);copy(a,ruin,161,168);
  rect(a,282,169,64,96,'074c68');
  poly(a,[[282,179],[294,175],[294,252],[309,264],[282,264]],'095875');
  copyPolygon(a,nativeRef,[267,154,65,28],[267,154,65,28],[[0,14],[17,4],[33,0],[49,5],[65,17],[65,28],[51,23],[28,18],[8,27],[0,27]]);
  var pillar=crop(nativeRef,377,104,19,54);copy(a,pillar,269,181);copy(a,pillar,269,226);copy(a,pillar,342,181);copy(a,pillar,342,226);
  floorStones(a,242,512,true);
  // The reference's front ledge cannot be baked into the middle of an extended
  // vista: on tall screens it reads as a second floor behind the live actors.
 }
 {
  // Restore every visible original pixel outside traced actor/equipment/HP
  // masks. Synthesis is restricted to genuinely hidden regions.
  var shapes=zone==='Dungeon'?[
   [[319,323],[362,322],[378,344],[420,340],[460,355],[478,378],[481,421],[497,456],[497,505],[362,507],[349,451],[338,401],[320,374]],
   [[532,366],[560,362],[602,392],[606,363],[634,345],[657,346],[678,375],[693,403],[715,406],[725,424],[726,483],[709,505],[579,506],[572,466],[555,429]]
  ]:zone==='Forest'?[
   [[302,329],[367,329],[424,335],[471,357],[477,380],[499,398],[503,488],[480,511],[351,513],[332,484],[324,440],[302,403]],
   [[511,322],[541,318],[545,303],[584,303],[617,311],[649,302],[677,312],[681,338],[722,356],[749,400],[750,470],[722,504],[691,517],[552,515],[540,482],[535,441],[509,399]]
  ]:[
   [[299,309],[350,307],[377,326],[400,319],[420,326],[425,341],[457,343],[466,373],[500,382],[513,414],[515,487],[498,511],[440,520],[351,516],[331,460],[330,408],[302,375]],
   [[541,313],[579,324],[603,327],[626,309],[678,313],[724,339],[747,389],[763,435],[798,446],[823,443],[821,487],[794,515],[640,525],[571,516],[574,443],[554,399],[533,361]]
  ];
  var restoreUntil=zone==='Dungeon'?194:zone==='Forest'?236:242;
  for(var y=165;y<restoreUntil;y++)for(var x=150;x<512;x++){
   var X=x*941/512,Y=y*1672/910;
   if(inside(X,Y,shapes[0])||inside(X,Y,shapes[1])||(Y>=504&&Y<548&&((X>=321&&X<514)||(X>=534&&X<754))))continue;
   var i=(y*512+x)*4;for(var k=0;k<4;k++)a.p[i+k]=nativeRef.p[i+k];
  }
  restoreProps(a);
 }
 // Native top scene remains one authored vista, with perspective-controlled
 // extension for tall screens. The prefab no longer repeats border tiles.
 if(zone==='Court')repairMarineCounters(a);
 // Keep the untouched scene around the cutout silhouettes. Rectangular repair
 // patches must not leave bright water/canopy bands beneath live HUD corners.
 var wave=zone==='Court'?[[178,24],[335,24],[343,31],[343,59],[333,67],[266,67],[263,75],[249,75],[245,67],[179,67],[171,59],[171,31]]:
  [[188,23],[321,23],[329,30],[329,59],[320,65],[265,65],[263,74],[248,74],[245,65],[188,65],[182,59],[182,30]];
 for(var y=20;y<76;y++)for(var x=170;x<501;x++){
  var edit=inside(x,y,wave)||(x>=359&&x<443&&y>=32&&y<72)||(x>=450&&x<500&&y>=32&&y<75);
  if(edit)continue;
  var i=(y*512+x)*4;for(var k=0;k<4;k++)a.p[i+k]=nativeRef.p[i+k];
 }
 // Extra native side courses and foreground serve wider/taller viewports.
 // They are authored once; no clamped last tile or repeated scenery row.
 var expanded=masonry(768,512,zone);copy(expanded,a,128,0);
 save(expanded,'Scene/'+zone);
};
materials=function(){
 oldMaterials();
 if(ONLY===zone+'/Panel'){
  var a=masonry(128,256,zone);
  save(a,zone+'/Panel');
 }
 if(ONLY===zone+'/General'){
  var a=masonry(256,256,zone);save(a,zone+'/General');
 }
 if(ONLY===zone+'/BottomPanel'){
  var a=masonry(256,256,zone);
  save(a,zone+'/BottomPanel');
 }
 if(ONLY===zone+'/BottomLeft'||ONLY===zone+'/BottomRight'){
  var right=ONLY===zone+'/BottomRight';save(extract(ref,right?805:0,1394,right?136:139,278,76,152),ONLY);
 }
 if(ONLY===zone+'/SideDressing'||ONLY===zone+'/SideRight'){
  var right=ONLY===zone+'/SideRight';save(extract(ref,right?867:0,596,74,804,40,432),ONLY);
 }
};
frame=function(){
 oldFrame();
 // Wider, continuous source beam with the original staggered stone/wood units.
 // The shared frame geometry remains 64x16 / 80x80.
 var forest=zone==='Forest',edge=extract(ref,forest?167:181,forest?585:596,forest?112:112,forest?32:29,64,16);
 var corner=blank(80,80);tile(corner,edge,0,0,80,16);
 var side=extract(ref,forest?79:78,forest?661:656,forest?27:27,125,16,64);
 tile(corner,side,0,16,16,64);
 var cap=extract(ref,forest?52:75,forest?565:589,forest?76:53,forest?73:57,forest?39:30,forest?38:32);
 mask(cap,[[0,7],[8,0],[cap.w-8,0],[cap.w,7],[cap.w,cap.h-8],[cap.w-8,cap.h],[5,cap.h-3],[0,cap.h-12]]);copy(corner,cap,0,0);
 save(edge,zone+'/Edge');save(corner,zone+'/Corner');
};
function frameAtlas(){
 var a=blank(544,544),forest=zone==='Forest';
 copy(a,extract(ref,108,forest?583:594,726,forest?42:31,512,16),16,0);
 copy(a,extract(ref,108,forest?1356:1363,726,forest?45:32,512,16),16,528);
 copy(a,extract(ref,forest?77:76,625,forest?33:32,734,16,512),0,16);
 copy(a,extract(ref,833,625,forest?38:35,734,16,512),528,16);
 var spots=forest?[[51,566],[814,565],[52,1329],[813,1329]]:[[74,589],[828,589],[75,1350],[828,1350]];
 for(var n=0;n<4;n++){
  var cap=extract(ref,spots[n][0],spots[n][1],forest?76:47,forest?78:47,forest?37:26,forest?38:26);
  mask(cap,[[7,0],[cap.w-7,0],[cap.w,7],[cap.w,cap.h-7],[cap.w-7,cap.h],[7,cap.h],[0,cap.h-7],[0,7]]);
  copy(a,cap,n%2?544-cap.w:0,n>=2?544-cap.h:0);
 }
 if(forest){
  var crest=extract(ref,416,581,115,44,78,26);mask(crest,[[0,5],[22,5],[28,0],[49,0],[55,5],[78,5],[76,18],[50,18],[43,26],[29,26],[23,18],[2,18]]);copy(a,crest,233,0);
  var lower=extract(ref,368,1353,192,48,124,30);mask(lower,[[0,0],[124,0],[124,15],[87,15],[77,30],[49,30],[35,15],[0,15]]);copy(a,lower,210,514);
 }
 save(a,zone+'/FrameAtlas');
}
if(ONLY===zone+'/FrameAtlas')frameAtlas();
