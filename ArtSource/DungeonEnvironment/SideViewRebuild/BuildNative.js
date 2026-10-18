// All final pixel edits execute inside LibreSprite. Reference sampling is nearest only.
var P='__PROJECT__/',K=P+'ArtSource/DungeonEnvironment/SideViewRebuild/';
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function crop(a,x,y,w,h){var b=blank(w,h);for(var Y=0;Y<h;Y++)for(var X=0;X<w;X++)for(var k=0;k<4;k++)b.p[(Y*w+X)*4+k]=a.p[((y+Y)*a.w+x+X)*4+k];return b;}
function px(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)return;var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=c?parseInt(c.substr(k*2,2),16):0;a.p[i+3]=c?255:0;}
function color(a,x,y){var i=(y*a.w+x)*4,s='';for(var k=0;k<3;k++)s+=('0'+a.p[i+k].toString(16)).slice(-2);return s;}
function rect(a,x,y,w,h,c){for(var Y=y;Y<y+h;Y++)for(var X=x;X<x+w;X++)px(a,X,Y,c);}
function line(a,x,y,X,Y,c){var n=Math.max(Math.abs(x-X),Math.abs(y-Y));for(var t=0;t<=n;t++)px(a,Math.round(x+(X-x)*t/Math.max(1,n)),Math.round(y+(Y-y)*t/Math.max(1,n)),c);}
var pal={
 wall:['101019','171522','211c30','292139','322740','3d304a','493b57'],
 blue:['0a0e19','111725','182032','202a3d','293348','354058'],
 stone:['12101c','21182e','32243f','453053','593e67','6d4e7b','84608f','9b7aaa','b196bb'],
 metal:['100f19','292132','453a50','65566f','897791','aa97b1','c4b5cd','e3d8e7','f4edf3'],
 jewel:['1b0c29','371144','5c186d','8a249d','b741d2','e47af0','f4c1ff','fff0ff'],
 cloth:['1d102b','32143f','4b1b5f','62236f','81318d','a54ab1'],
 gold:['48303b','72504e','987262','bb967c','dfb997'],
 wood:['20141e','3c2330','59343b','7b4645','9d5b4a','bc7a57'],
 fire:['191424','302036','4b2c3a','6b363a','8d4131','b75622','e07820','ff9b23','ffc43c','ffe366','fff2ad','ffffd4'],
 pink:['310e27','650f36','a2194b','d42b63','ed5a89','ff9fb6','ffe2df'],
 bone:['2a1a28','60444d','957577','d7baaa','f4e3ce'],
 energy:['190c29','360e50','611382','9221c4','c945ec','ef91ff']
};
function palette(names){var out=[];for(var i=0;i<names.length;i++)for(var j=0;j<pal[names[i]].length;j++)if(out.indexOf(pal[names[i]][j])<0)out.push(pal[names[i]][j]);return out;}
function quant(a,names){var colors=palette(names),cache={};for(var i=0;i<a.p.length;i+=4){if(!a.p[i+3])continue;var key=a.p[i]+','+a.p[i+1]+','+a.p[i+2],best=cache[key];if(best===undefined){var dist=1e10;for(var c=0;c<colors.length;c++){var rgb=colors[c],dr=a.p[i]-parseInt(rgb.substr(0,2),16),dg=a.p[i+1]-parseInt(rgb.substr(2,2),16),db=a.p[i+2]-parseInt(rgb.substr(4,2),16),d=dr*dr*2+dg*dg*3+db*db*2;if(d<dist){dist=d;best=rgb;}}cache[key]=best;}for(var k=0;k<3;k++)a.p[i+k]=parseInt(best.substr(k*2,2),16);a.p[i+3]=255;}return a;}
function inside(x,y,pts){var yes=false;for(var i=0,j=pts.length-1;i<pts.length;j=i++){var A=pts[i],B=pts[j];if((A[1]>y)!==(B[1]>y)&&x<(B[0]-A[0])*(y-A[1])/(B[1]-A[1])+A[0])yes=!yes;}return yes;}
function mask(a,pts){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(!inside(x+.5,y+.5,pts))px(a,x,y,null);return a;}
var ref=read(K+'References/NativeReference.png'),assets=[],placements=[],images={};
function save(a,group,name,source){
 var path=K+'Editable/'+group+'/'+name+'.aseprite';app.open(P+'Assets/_Game/Resources/UI/Consumables/Potion.png');var s=app.activeSprite;s.saveAs(path,false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();s.layer(0).cel(0).image.putImageData(a.p);s.layer(0).name=name;s.commit();s.saveAs(path,true);app.activeDocument.close();
 images[name]=a;assets.push({name:name,group:group,width:a.w,height:a.h,source:source,changes:'Native LibreSprite material palette, exact contour masking and local pixel correction; no blur or interpolation.'});console.log('ASSET_OK '+name);return name;
}
function clean(a){for(var pass=0;pass<2;pass++){var b=crop(a,0,0,a.w,a.h);for(var y=1;y<a.h-1;y++)for(var x=1;x<a.w-1;x++){var counts={},solid=true;for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){if(!dx&&!dy)continue;if(!b.p[((y+dy)*a.w+x+dx)*4+3])solid=false;var c=color(b,x+dx,y+dy);counts[c]=(counts[c]||0)+1;}if(solid)for(var c in counts)if(counts[c]>=4)px(a,x,y,c);}}return a;}
function part(name,group,box,mats,poly){var a=crop(ref,box[0],box[1],box[2],box[3]);if(poly)mask(a,poly);quant(a,mats);if(group.indexOf('Tiles')===0||group.indexOf('Frames')===0||group==='Structure'||group==='Lighting')clean(a);if(name==='GuideShell'||name==='GearShell'){var orig=crop(ref,box[0],box[1],box[2],box[3]);for(var Y=7;Y<a.h-6;Y++)for(var X=7;X<a.w-6;X++){var z=(Y*a.w+X)*4;if(orig.p[z]+orig.p[z+1]+orig.p[z+2]>465)px(a,X,Y,'e3d8e7');}}save(a,group,name,{reference:'References/NativeReference.png',crop:box,mask:poly||null});return a;}
function at(name,x,y,layer){placements.push({name:name,x:x,y:y,layer:layer});}
function extract(name,group,box,mats,layer,poly){part(name,group,box,mats,poly);at(name,box[0],box[1],layer);}
function field(name,box,layer){var a=images[name];for(var y=0;y<box[3];y+=a.h)for(var x=0;x<box[2];x+=a.w)placements.push({name:name,x:box[0]+x,y:box[1]+y,layer:layer,clip:[box[0],box[1],box[2],box[3]]});}
// Background families are independent reusable surfaces. Their hidden areas can be
// exposed by moving a prop; neither UI silhouettes nor prop fragments are baked in.
var back=quant(read(K+'PixelLab_Source/TileA.png'),['wall']);
// Rebuild the accepted tile's two staggered courses at the reference's mortar
// thickness. PixelLab's broad dark gutters and random chips are not retained.
function brickSurface(a,pitchH,colors){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var row=Math.floor(y/pitchH),Y=y%pitchH,X=(x+(row%2)*16)%32,c=colors[0];if(X>0&&Y>0){c=colors[1];if(Y===1&&X>2&&X<30)c=colors[2];else if(X===1&&Y>2&&Y<pitchH-2)c=colors[2];else if(Y===pitchH-1||X===31)c=colors[3];else if((X===1||X===31)&&(Y===1||Y===pitchH-1))c=colors[0];}px(a,x,y,c);}return a;}
brickSurface(back,16,['171522','292139','322740','211c30']);
save(back,'Tiles_Wall','BackWallReference',{pixellab_job:'59bc82c9-7774-4084-b362-2a407edbc3c5',file:'PixelLab_Source/TileA.png',changes:'Native reconstruction of generated staggered courses: narrowed gutters, reference-scale bricks, controlled bevel clusters and seamless periodic edges.'});
var portrait=brickSurface(blank(64,56),14,['0e1421','182032','202a3d','111725']);save(portrait,'Tiles_Wall','PortraitWall',{reference:'References/NativeReference.png',crop:[20,90,64,56],changes:'Native reconstruction of the reference blue brick field; four continuous courses without sampling residue.'});
var lower=crop(back,0,0,64,32);save(lower,'Tiles_Wall','LowerWall',{pixellab_job:'59bc82c9-7774-4084-b362-2a407edbc3c5',file:'PixelLab_Source/TileA.png',changes:'Clean structural background below the board, free of progress-bar fragments.'});
field('BackWallReference',[0,0,360,640],0);
field('PortraitWall',[18,30,80,167],1);
field('LowerWall',[0,210,360,430],0);
// Standing plane stays a 12px strip, never a large receding floor.
for(var n=0;n<4;n++){
 var x=106+n*64,w=Math.min(64,360-x);extract('StandingStrip'+String.fromCharCode(65+n),'Tiles_Floor',[x,178,w,12],['stone','wall'],10);
 extract('LedgeFace'+String.fromCharCode(65+n),'Tiles_Platform',[x,190,w,19],['stone','wall'],11);
}
// Left/center/right structural pieces and lower connector masonry, split from UI.
extract('UpperLeftPier','Structure',[0,0,17,179],['stone','wall'],12);
extract('PortraitBackPier','Structure',[99,0,21,180],['stone','wall'],12);
extract('RightPierTop','Structure',[325,0,32,85],['stone','wall'],12);
extract('RightPierBase','Structure',[323,148,31,31],['stone','wall'],12);
part('RightPierShaft','Structure',[329,69,14,16],['stone','wall']);field('RightPierShaft',[329,85,14,64],12);
for(var j=0;j<5;j++){var sx=[111,156,201,246,291][j];extract('PlatformSupport'+String.fromCharCode(65+j),'Structure',[sx,209,18,17],['stone','wall'],12);}
extract('LeftSideMasonry','Structure',[0,210,27,325],['stone','wall'],12);
extract('RightSideMasonry','Structure',[331,210,29,325],['stone','wall'],12);
extract('LowerLeftConnector','Structure',[0,535,31,105],['stone','wall'],12);
extract('LowerRightConnector','Structure',[328,535,32,105],['stone','wall'],12);
// Remove decorative content from side masonry underlays using clean wall surfaces.
// Masks are repaired before exporting; the independent decorative sprites below own it.
function repair(name,boxes){var a=images[name],tile=images.BackWallReference;for(var i=0;i<boxes.length;i++){var b=boxes[i];for(var y=b[1];y<b[1]+b[3];y++)for(var x=b[0];x<b[0]+b[2];x++)px(a,x,y,color(tile,(x+7)%tile.w,(y+11)%tile.h));}var rec=assets.filter(function(r){return r.name===name;})[0];assets=assets.filter(function(r){return r.name!==name;});save(a,rec.group,name,{reference:rec.source,repair_boxes:boxes});}
repair('LeftSideMasonry',[[0,12,20,77],[0,87,27,69],[0,154,24,52],[0,229,23,38]]);
repair('RightSideMasonry',[[7,14,22,76],[0,87,29,69],[5,155,24,51],[0,229,25,38]]);
// Local warmed masonry is a discrete material patch, kept behind every torch.
function warmWall(name,box,cx,cy){var a=blank(box[2],box[3]);for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var gx=x+box[0],gy=y+box[1],dx=Math.abs(gx-cx),dy=Math.abs(gy-cy),d=Math.floor(dx/10)+Math.floor(dy/18);if(d>3)continue;var base=color(back,gx%64,gy%32),c;if(base==='171522')c=d<2?'39232a':'211925';else if(base==='322740')c=['b77745','8d5138','59383a','3d2934'][d];else if(base==='211c30')c=['754132','553032','38232d','29202e'][d];else c=['9d593b','754335','553338','322432'][d];px(a,x,y,c);}save(a,'Lighting',name,{reference:'References/NativeReference.png',crop:box,changes:'Discrete warm brick clusters on the same wall grid; no torch, banner or chain pixels baked in.'});at(name,box[0],box[1],14);}
warmWall('WarmWallLeft',[113,88,36,67],130,119);
warmWall('WarmWallMiddle',[264,103,36,48],277,130);
// Warm the existing column shaft rather than flattening its contour with a
// rectangular extraction containing the flame.
var wp=blank(14,64),shaft=images.RightPierShaft;for(var y=0;y<64;y++)for(var x=0;x<14;x++){var c=color(shaft,x,y%16),Y=Math.abs(y-34);if(Y<28){var base=parseInt(c.substr(0,2),16);c=base>75?(Y<17?'8d5140':'6d424b'):(base>35?'49313c':c);}px(wp,x,y,c);}save(wp,'Lighting','WarmPierRight',{method:'Native warm column planes, torch separated'});at('WarmPierRight',329,85,14);
// Arch is separated from its barred opening and the reusable piers.
extract('ArchCrown','Props_Door',[182,81,83,45],['stone','wall'],20,[[0,45],[2,31],[9,18],[21,6],[40,0],[58,4],[72,15],[80,28],[83,45],[71,45],[67,30],[57,20],[40,15],[25,20],[16,30],[13,45]]);
extract('ArchPierLeft','Props_Door',[182,125,13,54],['stone','wall'],20);
extract('ArchPierRight','Props_Door',[252,125,13,54],['stone','wall'],20);
extract('BarredDoor','Props_Door',[195,96,57,83],['wall','stone','metal'],18,[[0,83],[0,29],[3,16],[11,7],[27,0],[43,6],[53,17],[57,29],[57,83]]);
// Main torches, banners and barrel props keep the supplied contours and anchors.
extract('TorchLeft','Props_Torches',[120,100,19,52],['fire','metal','wood'],30,[[7,0],[11,6],[13,14],[17,22],[18,34],[14,41],[13,51],[7,52],[6,42],[1,35],[0,25],[4,15]]);
extract('TorchMiddle','Props_Torches',[269,111,17,45],['fire','metal','wood'],30,[[8,0],[11,9],[15,14],[17,24],[14,31],[12,44],[7,45],[5,32],[1,25],[0,17],[4,10]]);
extract('TorchRight','Props_Torches',[331,92,19,58],['fire','metal','wood'],30,[[8,0],[12,11],[16,20],[19,32],[15,42],[13,57],[7,58],[6,43],[1,34],[0,25],[4,15]]);
extract('BannerPlayer','Props_Banners',[38,32,31,43],['cloth','gold','metal'],35,[[0,0],[31,0],[29,43],[16,34],[2,43]]);
extract('BannerLeft','Props_Banners',[140,53,29,60],['cloth','gold','metal'],25,[[0,0],[29,0],[27,59],[15,49],[3,60]]);
extract('BannerRight','Props_Banners',[274,54,29,60],['cloth','gold','metal'],25,[[0,0],[29,0],[27,60],[15,49],[2,59]]);
extract('BarrelLarge','Props_Barrels',[299,141,27,38],['wood','metal'],27,[[5,0],[23,0],[26,8],[27,27],[23,37],[5,38],[1,32],[0,12]]);
extract('BarrelSmall','Props_Barrels',[289,158,14,21],['wood','metal'],28,[[3,0],[12,0],[14,8],[13,19],[3,21],[0,15],[1,4]]);
// Native chain sprites have real alpha. Retain their bends instead of straightening.
function chain(name,box){var a=crop(ref,box[0],box[1],box[2],box[3]);for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var c=(y*a.w+x)*4,R=a.p[c],G=a.p[c+1],B=a.p[c+2];if(!(R>G*1.12&&R>B*.84&&R>35))px(a,x,y,null);}quant(a,['metal','wood']);save(a,'Props_Chains',name,{reference:'References/NativeReference.png',crop:box,mask:'Warm chain metal, with pixel-native silhouette cleanup'});at(name,box[0],box[1],24);}
chain('ChainWaveLeft',[116,0,20,78]);chain('ChainWaveRight',[223,0,26,78]);chain('ChainDoor',[174,42,10,96]);chain('ChainGuide',[256,0,11,88]);chain('ChainGear',[330,0,10,24]);
chain('ChainSideLeft',[3,226,12,76]);chain('ChainSideRight',[346,225,14,76]);
// Side ornaments are separate reusable sprites, not baked into the wall panels.
extract('BrazierLeft','Props_Torches',[0,299,25,66],['fire','metal','stone'],30,[[11,0],[15,11],[21,24],[25,41],[22,51],[18,61],[12,66],[5,61],[2,50],[0,36],[5,22]]);
extract('BrazierRight','Props_Torches',[335,299,25,66],['fire','metal','stone'],30,[[12,0],[17,13],[22,24],[25,40],[23,51],[18,62],[12,66],[5,61],[2,50],[0,37],[5,23]]);
extract('SideBannerLeft','Props_Banners',[0,369,24,56],['cloth','gold','metal'],25,[[0,0],[24,0],[22,55],[12,46],[1,56]]);
extract('SideBannerRight','Props_Banners',[337,369,23,56],['cloth','gold','metal'],25,[[0,0],[23,0],[23,56],[12,47],[1,55]]);
extract('SkullLeft','Props_Bones',[0,449,24,36],['stone','wall'],25,[[2,0],[20,0],[24,8],[21,21],[17,24],[17,33],[7,36],[6,25],[2,23],[0,10]]);
extract('SkullRight','Props_Bones',[335,449,25,36],['stone','wall'],25,[[4,0],[21,0],[25,8],[23,21],[18,25],[18,34],[8,36],[6,25],[1,22],[0,10]]);
// Exact frame silhouettes: independent corners and edge spans, no large frame image.
var frameM=['stone','metal','jewel','wall'];
function frame(prefix,group,parts){var mats=prefix==='Board'?['stone','jewel','wall']:['metal','jewel','wall'];if(prefix==='Wave')mats=['stone','metal','wall'];for(var n=0;n<parts.length;n++){var f=parts[n],box=f.slice(1,5),a=crop(ref,box[0],box[1],box[2],box[3]),raw=crop(a,0,0,a.w,a.h);quant(a,mats);clean(a);for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var keep=false;for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){var X=x+dx,Y=y+dy;if(X<0||Y<0||X>=a.w||Y>=a.h)continue;var i=(Y*a.w+X)*4;if(Math.max(raw.p[i],raw.p[i+1],raw.p[i+2])>68)keep=true;}if(!keep)px(a,x,y,null);}save(a,group,prefix+f[0],{reference:'References/NativeReference.png',crop:box,changes:'Material cleanup; remove surrounding wall/cell pixels while retaining a one-pixel dark contour around frame metal or stone.'});at(prefix+f[0],box[0],box[1],40);}}
frame('Player','Frames_Player', [['TL',9,20,14,14],['Top',23,23,72,8],['TR',95,20,13,14],['Left',10,34,9,160],['Right',97,34,10,160],['BL',9,194,14,16],['Bottom',23,198,72,11],['BR',95,194,13,16]]);
frame('Board','Frames_Board', [['TL',25,224,20,20],['Top',45,226,267,12],['TR',312,224,22,20],['Left',26,244,13,263],['Right',320,244,14,263],['BL',25,507,20,22],['Bottom',45,517,267,12],['BR',312,507,22,22]]);
// 8x8 empty board, no gem sprites. Cells contain no surrounding frame pixels.
for(var i=0;i<3;i++){var cell=blank(35,35);rect(cell,0,0,35,35,'0a0e19');rect(cell,1,1,33,33,'182032');rect(cell,2,2,31,30,'1b2233');rect(cell,2,2,2,28,'202a3d');line(cell,3,1,31,1,'293348');line(cell,1,3,1,31,'293348');line(cell,33,2,33,32,'111725');line(cell,2,33,32,33,'111725');if(i===1)line(cell,3,2,12,2,'202a3d');if(i===2)line(cell,3,30,12,30,'182032');save(cell,'Tiles_Cell','Cell'+String.fromCharCode(65+i),{reference:'References/NativeReference.png',crop:[40+i*35,239,35,35],changes:'Hand-cleaned simple cell rim and quiet face; native color clusters, no resampling.'});}
for(var y=0;y<8;y++)for(var x=0;x<8;x++)at('Cell'+String.fromCharCode(65+(x+y)%3),40+x*35,238+y*35,2);
extract('PlayerHeart','Hud',[19,181,13,15],['pink','metal'],42,[[4,0],[7,2],[11,0],[13,4],[12,8],[6,15],[1,8],[0,4]]);
extract('PlayerHealthFrame','Hud',[31,183,65,11],['metal','pink','wall'],41);
// Top signs preserve exact shapes and native labels. Label artwork remains separate.
frame('Wave','Frames_Top', [['TL',130,15,12,12],['Top',142,15,83,6],['TR',225,15,14,12],['Left',130,27,6,8],['Right',233,27,6,8],['BL',130,35,12,11],['Bottom',142,38,83,8],['BR',225,35,14,11]]);
var fill=blank(16,3);rect(fill,0,0,16,3,'171522');save(fill,'Tiles_Wall','SignFill',{method:'Native dark plaque interior matching reference'});field('SignFill',[136,21,97,17],31);
var wave=crop(ref,154,24,68,12);for(var y=0;y<wave.h;y++)for(var x=0;x<wave.w;x++){var i=(y*wave.w+x)*4;px(wave,x,y,wave.p[i]+wave.p[i+1]+wave.p[i+2]>490?'f4edf3':null);}save(wave,'Labels','WaveLabel',{reference:'References/NativeReference.png',crop:[154,24,68,12],changes:'Crisp isolated lettering; no plaque background residue.'});at('WaveLabel',154,24,43);
extract('WaveSkull','Hud',[175,44,14,12],['metal','wall'],41,[[2,0],[12,0],[14,5],[10,7],[8,12],[5,10],[4,7],[0,5]]);
extract('GuideShell','Frames_Top',[254,23,58,28],['metal','wall'],40);
extract('GearShell','Frames_Top',[314,22,36,30],['metal','wall'],40);
// Lower UI shells and icons split so interiors can change without redrawing frames.
frame('Progress','Frames_Bottom',[['Left',123,537,10,15],['Top',133,538,94,4],['Right',227,537,11,15],['Bottom',133,547,94,5]]);
part('ProgressTrack','Hud',[133,542,94,5],['wall']);at('ProgressTrack',133,542,41);
extract('ProgressFill','Hud',[134,542,47,5],['energy'],42);
frame('Ability','Frames_Bottom',[['TL',112,557,12,12],['Top',124,557,111,7],['TR',235,557,13,12],['Left',112,569,8,26],['Right',240,569,8,26],['BL',112,595,12,11],['Bottom',124,599,111,7],['BR',235,595,13,11]]);
var af=blank(15,15);rect(af,0,0,15,15,'190c29');save(af,'Tiles_Wall','AbilityFill',{method:'Clean native dark-violet ability-panel interior'});field('AbilityFill',[120,564,120,35],31);
extract('CrownedSkull','Hud',[167,563,31,35],['bone','gold','wall'],43,[[6,0],[13,7],[16,0],[20,8],[27,1],[29,14],[31,19],[28,29],[23,30],[22,35],[10,35],[9,30],[3,29],[0,19],[3,14]]);
function lightning(name,box){var a=crop(ref,box[0],box[1],box[2],box[3]);for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var i=(y*a.w+x)*4;if(!(a.p[i+2]>50&&a.p[i+2]>a.p[i+1]*1.7&&a.p[i]>a.p[i+1]*1.4))px(a,x,y,null);}quant(a,['energy']);save(a,'Hud',name,{reference:'References/NativeReference.png',crop:box,changes:'Separate energy clusters; dark panel pixels removed.'});at(name,box[0],box[1],42);}
lightning('LightningLeft',[123,567,37,27]);lightning('LightningRight',[202,567,35,27]);
frame('Potion','Frames_Bottom',[['TL',55,561,10,10],['Top',65,561,33,6],['TR',98,561,10,10],['Left',55,571,6,26],['Right',102,571,6,26],['BL',55,597,10,10],['Bottom',65,601,33,6],['BR',98,597,10,10]]);
frame('Bomb','Frames_Bottom',[['TL',256,561,10,10],['Top',266,561,31,6],['TR',297,561,10,10],['Left',256,571,6,26],['Right',301,571,6,26],['BL',256,597,10,10],['Bottom',266,601,31,6],['BR',297,597,10,10]]);
part('ButtonFill','Hud',[61,568,10,10],['metal']);field('ButtonFill',[61,567,41,34],31);field('ButtonFill',[262,567,39,34],31);
extract('PotionIcon','Hud',[70,569,23,29],['pink','gold','metal'],43,[[7,0],[16,0],[16,8],[22,15],[23,24],[17,29],[5,29],[0,24],[0,15],[7,8]]);
extract('BombIcon','Hud',[267,568,29,31],['stone','metal','gold','fire'],43,[[20,0],[26,1],[28,6],[26,10],[23,11],[27,18],[27,25],[20,31],[7,31],[1,25],[0,18],[6,10],[13,7],[19,8],[22,5],[19,4]]);
extract('ResonanceGem','Hud',[91,610,15,15],['jewel'],43,[[5,0],[11,0],[15,5],[14,9],[8,15],[1,9],[0,5]]);
var textSupport=blank(214,16);rect(textSupport,0,0,214,16,'171522');rect(textSupport,1,1,212,14,'211c30');save(textSupport,'Hud','TextSupport',{method:'Native quiet text-support panel; blurred source lettering removed.'});at('TextSupport',110,609,32);
// This tiny reference text is rebuilt as deliberate 3x5 glyphs instead of blurred pixels.
var glyph={A:'010101111101101',B:'110101110101110',C:'011100100100011',D:'110101101101110',E:'111100110100111',F:'111100110100100',G:'011100101101011',H:'101101111101101',I:'111010010010111',J:'001001001101010',K:'101101110101101',L:'100100100100111',M:'101111111101101',N:'101111111111101',O:'010101101101010',P:'110101110100100',Q:'010101101111011',R:'110101110101101',S:'011100010001110',T:'111010010010010',U:'101101101101111',V:'101101101101010',W:'101101111111101',X:'101101010101101',Y:'101101010010010',Z:'111001010100111','1':'010110010010111','5':'111100110001110','+':'000010111010000','%':'101001010100101',':':'000010000010000',' ':'000000000000000'};
var text='PHASE RESONANCE: +15% GEM DAMAGE',label=blank(text.length*4-1,5);for(var n=0;n<text.length;n++){var g=glyph[text[n]];for(var y=0;y<5;y++)for(var x=0;x<3;x++)if(g[y*3+x]==='1')px(label,n*4+x,y,'e3d8e7');}save(label,'Labels','ResonanceLabel',{method:'Native 3x5 hand-authored glyphs',text:text});at('ResonanceLabel',140,614,43);
console.log('MANIFEST '+JSON.stringify({width:360,height:640,pixelsPerUnit:64,layers:['Base wall','Floor and platform','Structure','Props','Top UI','Bottom UI'],assets:assets,placements:placements,reference:'References/Source.png',status:'review candidate'}));
console.log('SIDEVIEW_COMPLETE');
