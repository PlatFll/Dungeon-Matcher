var modules=[],placements=[];
var native=P+'ArtSource/BattlePresentation/DungeonMockupRevision/native/';
function emit(group,name,a,src,area,change){save(a,group,name);modules.push({group:group,name:name,width:a.w,height:a.h,source:src,crop:area||null,changes:change||'Lossless native reuse; no pixel or silhouette change.'});return a;}
function extracted(group,name,src,r){return emit(group,name,crop(read(native+src+'.png'),r[0],r[1],r[2],r[3]),'ArtSource/BattlePresentation/DungeonMockupRevision/native/'+src+'.png',r);}
function inside(x,y,p){var b=false;for(var i=0,j=p.length-1;i<p.length;j=i++){var A=p[i],B=p[j];if((A[1]>y)!=(B[1]>y)&&x<(B[0]-A[0])*(y-A[1])/(B[1]-A[1])+A[0])b=!b;}return b;}
function cut(a,p){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(!inside(x+.5,y+.5,p))px(a,x,y,null);return a;}
function place(name,group,x,y,w,h,role){placements.push({name:name,group:group,x:x,y:y,width:w,height:h,role:role||'prop'});}
// Seam repairs are confined to the named edge band. Each material keeps its
// approved source ramp; this is not global palette reduction.
var panel=read(native+'PlayerMasonry.png'),base=crop(panel,25,64,64,64);
var wallMap={'14172a':'171421','191c31':'211b30','20233a':'29213d','272a43':'312747','30314c':'3b2d50','3a3a57':'45335a'};
for(var material=0;material<3;material++)for(var v=0;v<4;v++){
 var a=blank(64,64);
 for(var y=0;y<64;y++)for(var x=0;x<64;x++){
  var c=color(base,(x+v*16)%64,y);
  if(material!==0)c=wallMap[c]||c;
  if(material===2)c=({'45335a':'503b66','3b2d50':'45335a','312747':'3b2d50'})[c]||c;
  px(a,x,y,c);
 }
 var mortar=material===0?'14172a':'171421';
 for(var y=0;y<64;y++){
  var c=color(base,31,y);if(material!==0)c=wallMap[c]||c;
  if(material===2)c=({'45335a':'503b66','3b2d50':'45335a','312747':'3b2d50'})[c]||c;
  for(var x=0;x<4;x++){px(a,x,y,c);px(a,63-x,y,c);}
 }
 rect(a,0,0,64,2,mortar);rect(a,0,62,64,2,mortar);
 emit('WallTiles',['PortraitWall','BattleWall','OuterWall'][material]+String.fromCharCode(65+v),a,'ArtSource/BattlePresentation/DungeonMockupRevision/native/PlayerMasonry.png',[25,64,64,64],'Native cyclic crop, 4px horizontal/2px vertical seam reconstruction; scoped blue-to-violet material map for battle/outer walls.');
}
// Fixed corners remain byte-identical. Rails repeat at native size; only their
// first/last edge signature is reconstructed to connect to another rail.
function rail(group,name,src,r,horizontal){
 var a=crop(read(native+src+'.png'),r[0],r[1],r[2],r[3]);
 if(horizontal)for(var y=0;y<a.h;y++){var c=color(a,Math.floor(a.w/2),y);for(var x=0;x<2;x++){var i=(y*a.w+Math.floor(a.w/2))*4;if(a.p[i+3]){px(a,x,y,c);px(a,a.w-1-x,y,c);}else {px(a,x,y,null);px(a,a.w-1-x,y,null);}}}
 else for(var x=0;x<a.w;x++){var c=color(a,x,Math.floor(a.h/2)),i=(Math.floor(a.h/2)*a.w+x)*4;for(var y=0;y<2;y++){if(a.p[i+3]){px(a,x,y,c);px(a,x,a.h-1-y,c);}else {px(a,x,y,null);px(a,x,a.h-1-y,null);}}}
 return emit(group,name,a,'ArtSource/BattlePresentation/DungeonMockupRevision/native/'+src+'.png',r,'Lossless native extraction plus 2px repeat-end repair; all remaining contour and ornament pixels protected.');
}
for(var i=0;i<4;i++){var n=['TL','TR','BL','BR'][i],a=read(native+'Board'+n+'.png');emit('BoardFrames','Board'+n,a,'ArtSource/BattlePresentation/DungeonMockupRevision/native/Board'+n+'.png');}
rail('BoardFrames','BoardTopRail','BoardTop',[44,0,64,18],true);
rail('BoardFrames','BoardBottomRail','BoardBottom',[46,0,64,19],true);
rail('BoardFrames','BoardLeftRail','BoardLeft',[0,72,16,64],false);
rail('BoardFrames','BoardRightRail','BoardRight',[0,72,20,64],false);
extracted('BoardFrames','BoardCenterFinial','BoardTop',[167,4,20,14]);
// Individual empty cell assets are already approved production art. They remain
// distinct from the frame and are never replaced by a flattened board picture.
for(var v=0;v<3;v++){
 // Numbered PNG names trigger LibreSprite's interactive sequence-import prompt.
 // Open the existing editable cel to preserve pixels without that modal.
 var path=K+'Sources/EmptyCell'+String.fromCharCode(65+v)+'.aseprite';
 emit('BackgroundLayers','EmptyCell'+v,read(path),'ArtSource/BattlePresentation/DungeonMockupRevision/native/EmptyCell'+v+'.png');
}
function frame(group,name,src,corner,top,bottom,left,right,bodyHeight){
 var a=read(native+src+'.png'),w=a.w,h=bodyHeight||a.h;
 var cornerWidth=name==='Wave'?20:corner;
 var boxes=[[0,0,cornerWidth,corner],[w-cornerWidth,0,cornerWidth,corner],[0,h-corner,cornerWidth,corner],[w-cornerWidth,h-corner,cornerWidth,corner]];
 for(var i=0;i<4;i++)extracted(group,name+['TL','TR','BL','BR'][i],src,boxes[i]);
 var horizontalLength=name==='Portrait'?16:Math.min(32,w-2*cornerWidth);
 var verticalY=name==='Portrait'?60:corner,verticalLength=name==='Ability'?8:Math.min(32,h-2*corner);
 rail(group,name+'Top',src,[cornerWidth,0,horizontalLength,top],true);
 rail(group,name+'Bottom',src,[cornerWidth,h-bottom,horizontalLength,bottom],true);
 rail(group,name+'Left',src,[0,verticalY,left,verticalLength],false);
 rail(group,name+'Right',src,[w-right,verticalY,right,verticalLength],false);
}
frame('CharacterFrames','Portrait','PlayerFrame',18,13,13,11,13);
var collars=[['TopA',35,0,12,13],['TopB',84,0,12,13],['BottomA',38,239,14,13],['BottomB',84,239,14,13],['LeftA',0,42,11,14],['LeftB',0,116,11,14],['RightA',116,42,13,14],['RightB',116,116,13,14]];
for(var i=0;i<collars.length;i++){var c=collars[i];extracted('CharacterFrames','PortraitCollar'+c[0],'PlayerFrame',c.slice(1));}
frame('HudFrames','Wave','WaveHeader',17,9,9,10,10,38);
extracted('HudFrames','WaveSkull','WaveHeader',[57,35,27,12]);
frame('HudFrames','Guide','SharedGuide',13,8,8,9,9);
frame('HudFrames','Settings','SharedSettings',12,7,7,7,7);
extracted('HudFrames','SettingsGear','SharedSettings',[12,8,22,24]);
frame('HudFrames','Ability','SharedAbility',16,10,12,14,14);
extracted('HudFrames','AbilityLeftJewel','SharedAbility',[0,25,14,16]);
extracted('HudFrames','AbilityRightJewel','SharedAbility',[162,25,14,16]);
frame('HudFrames','Supply','SharedPotionHolder',20,9,16,10,10);
extracted('HudFrames','EnergyStart','SharedEnergy',[0,0,13,24]);
extracted('HudFrames','EnergyEnd','SharedEnergy',[132,0,15,24]);
rail('HudFrames','EnergyTopRail','SharedEnergy',[13,0,64,6],true);
rail('HudFrames','EnergyBottomRail','SharedEnergy',[13,18,64,6],true);
for(var i=0;i<3;i++){var n='PlayerHealth'+['Start','Middle','End'][i],src='Assets/_Game/Resources/UI/Finalized/'+n+'.png';emit('CharacterFrames',n,read(P+src),src);}
var fills=[['HudDark','171421'],['AbilityDark','210f31'],['SilverInset','7a7287']];
for(var i=0;i<fills.length;i++){var a=blank(8,8);rect(a,0,0,8,8,fills[i][1]);emit('BackgroundLayers',fills[i][0],a,'Approved native HUD material colors',null,'Dedicated repeatable recess fill; no pictorial source resampling.');}
var props=[['Gateway','ArchDoor',193,59],['LeftTorch','TopLeftTorch',135,95],['RightTorch','TopRightTorch',359,93],['RearBarrels','RearBarrels',319,146],['FrontBarrel','NearRightBarrel',421,189],['LeftBones','NearLeftBones',137,199],['RightBones','NearRightBones',436,210],['RearBones','RearBones',341,183],['PortraitBanner','PlayerBanner',51,42]];
for(var i=0;i<props.length;i++){var q=props[i],a=read(native+q[1]+'.png');emit('Props',q[0],a,'ArtSource/BattlePresentation/DungeonMockupRevision/native/'+q[1]+'.png');place(q[0],'Props',q[2],q[3],a.w,a.h);}
function prop(name,src,r,shape,x,y){
 var a=crop(read(native+src+'.png'),r[0],r[1],r[2],r[3]);if(shape)cut(a,shape);
 var change=shape?'Native isolation with an explicit silhouette selection; retained pixels unchanged.':'Lossless native extraction.',repairs=[];
 if(name==='RightTopPillar'){
  // The approved scene source has a flat occlusion patch behind the Guide HUD.
  // Reconstruct only that missing interior from the same pillar's intact stones.
  var patch=crop(a,0,72,23,24);
  for(var Y=28;Y<71;Y++)for(var X=0;X<23;X++)if(color(a,X,Y)==='171421')px(a,X,Y,color(patch,X,(Y-28)%24));
  change='Native pillar with only the Guide-occluded flat patch reconstructed from its intact stone band; original rectangular silhouette and visible pixels elsewhere unchanged.';repairs=[[0,28,23,43]];
 }
 if(name==='FarRightChain'){
  // The old flattened source omitted links hidden behind Settings. Complete
  // those links from the approved side-chain rhythm; no existing links moved.
  var link=crop(read(native+'LeftBorderDressing.png'),0,26,16,24);
  for(var Y=25;Y<77;Y++)for(var X=0;X<21;X++){
   if(X<3||X>=19)px(a,X,Y,null);else px(a,X,Y,color(link,X-3,(Y-25)%24));
  }
  change='Native chain isolation; missing Settings-occluded middle links reconstructed from the approved side-chain strip. Existing visible links outside the repair rectangle unchanged.';repairs=[[0,25,21,52]];
 }
 emit('Props',name,a,'ArtSource/BattlePresentation/DungeonMockupRevision/native/'+src+'.png',r,change);
 modules[modules.length-1].selection=shape;modules[modules.length-1].repairRects=repairs;
 place(name,'Props',x,y,a.w,a.h);
}
prop('LeftHangingBanner','FarWall',[169,63,25,63],[[1,0],[25,0],[24,62],[13,54],[1,63]],169,63);
prop('RightHangingBanner','FarWall',[307,65,26,61],[[1,0],[26,0],[25,61],[13,53],[1,61]],307,65);
prop('LeftHighChain','FarWall',[148,0,34,103],[[23,0],[34,0],[28,22],[23,34],[19,51],[18,68],[10,88],[10,100],[2,103],[0,93],[4,76],[4,60],[10,39],[13,18]],148,0);
prop('RightHighChain','FarWall',[294,0,28,83],[[0,0],[10,0],[13,22],[17,34],[18,48],[25,65],[28,81],[18,83],[15,67],[10,54],[7,35],[3,20]],294,0);
prop('FarRightChain','FarWall',[414,0,21,180],[[5,0],[16,0],[15,85],[20,110],[16,140],[15,180],[4,180],[5,143],[7,115],[3,89]],414,0);
prop('LeftSideChain','LeftBorderDressing',[0,0,16,111],null,0,298);
prop('RightSideChain','RightBorderDressing',[21,0,12,111],null,458,298);
prop('LeftSideTorch','LeftBorderDressing',[0,109,37,83],[[15,0],[25,17],[32,29],[37,46],[36,63],[29,67],[29,83],[11,83],[10,69],[1,62],[0,43],[7,30],[9,15]],0,407);
prop('RightSideTorch','RightBorderDressing',[0,109,33,83],[[18,0],[25,15],[26,30],[33,42],[32,63],[24,69],[23,83],[7,83],[6,67],[0,61],[0,43],[8,29],[10,16]],437,407);
prop('LeftSideBanner','LeftBorderDressing',[0,194,30,96],[[0,0],[30,0],[30,88],[15,96],[0,88]],0,492);
prop('RightSideBanner','RightBorderDressing',[4,194,29,96],[[0,0],[29,0],[29,88],[14,96],[0,88]],441,492);
prop('LeftSkullPillar','LeftBorderDressing',[0,289,37,114],null,0,587);
prop('RightSkullPillar','RightBorderDressing',[0,289,33,114],null,437,587);
prop('LeftBottomButtress','BottomMasonry',[0,0,67,135],[[0,0],[49,0],[51,26],[46,42],[53,62],[46,82],[52,102],[67,118],[67,135],[0,135]],0,701);
prop('RightBottomButtress','BottomMasonry',[407,0,63,135],[[14,0],[63,0],[63,135],[0,135],[1,117],[17,101],[11,83],[18,62],[12,42],[12,24]],407,701);
prop('LeftTopPillar','FarWall',[0,0,10,180],null,0,0);
prop('CenterTopPillar','FarWall',[108,0,38,28],null,108,0);
prop('RightTopPillar','FarWall',[354,0,23,180],null,354,0);
prop('FarRightPillar','FarWall',[458,0,12,180],null,458,0);
extracted('Props','FoundationStart','BattleFloor',[0,98,24,23]);
rail('Props','FoundationTopRail','BattleFloor',[20,98,64,23],true);
extracted('Props','FoundationEnd','BattleFloor',[303,98,30,23]);
console.log('KIT_MANIFEST '+JSON.stringify({modules:modules,placements:placements,sourceExtent:[470,836],sourceScale:'Approved native sources; no screenshot downscaling',zipCoordinateIssue:'ZIP crops use inconsistent bounding boxes; current registered native sources and pixel-identical full reference govern placement.'}));
console.log('KIT_COMPLETE');
