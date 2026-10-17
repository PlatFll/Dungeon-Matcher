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
var input=read(ROOT+'reference/RequestedDungeon.png'),N=blank(470,836);
for(var y=0;y<836;y++)for(var x=0;x<470;x++)for(var k=0;k<4;k++)N.p[(y*470+x)*4+k]=input.p[((y*2+1)*941+x*2+1)*4+k];
var modules=[],repairs=[],composite=blank(470,836),layerArt=[];
function makeDocument(a,path,name){
 app.open(PROJECT+'Assets/_Game/Resources/UI/Consumables/Potion.png');
 var s=app.activeSprite;s.saveAs(path,false);s.resize(a.w,a.h);s.commit();
 app.command.BackgroundFromLayer();s.commit();
 s.layer(0).cel(0).image.putImageData(a.p);s.layer(0).name=name;s.commit();s.saveAs(path,true);app.activeDocument.close();
 var q=read(path);if(q.w!==a.w||q.h!==a.h)throw Error('Dimensions '+name);
 for(var i=0;i<a.p.length;i++)if(q.p[i]!==a.p[i])throw Error('Reopened pixels '+name+' '+i);
}
function emit(name,a,x,y,role,group,compose){
 cleanColors(a);
 makeDocument(a,OUTPUT+name+'.aseprite',name);
 modules.push({name:name,x:x,y:y,width:a.w,height:a.h,role:role,group:group,composite:compose!==false});
 if(compose!==false){copy(composite,a,x,y);layerArt.push({name:name,a:a,x:x,y:y});}
 console.log('KIT_ASSET_OK '+name);
}
function clone(a,sx,sy,w,h,dx,dy){copy(a,crop(N,sx,sy,w,h),dx,dy);}
function repeatedMaterial(a,sx,sy,w,h,x,y,W,H){for(var Y=0;Y<H;Y++)for(var X=0;X<W;X++){var i=(((sy+Y%h)*N.w)+sx+X%w)*4,j=((y+Y)*a.w+x+X)*4;for(var k=0;k<4;k++)a.p[j+k]=N.p[i+k];}}

__TOP_RECONSTRUCTION__
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
// The new mockup's crown slightly touches the upper rim: restore its hidden
// metal/inset rows from the same uninterrupted rim immediately to the left.
for(var yy=0;yy<11;yy++)for(var xx=60;xx<115;xx++){
 var ii=(yy*176+xx)*4;
 if(ability.p[ii]>ability.p[ii+2]*1.25 && ability.p[ii+1]>ability.p[ii+2]*1.2)
  for(var kk=0;kk<4;kk++)ability.p[ii+kk]=ability.p[(yy*176+40)*4+kk];
}
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


__LAYERED_MASTER__
