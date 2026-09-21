// Execute in LibreSprite, assembled with the existing native helpers.
// Reuse transparent full-canvas production cels and their approved timings.
function writeRefined(name,frames){
 app.open(ROOT+name+'.aseprite');var s=app.activeSprite;
 s.saveAs(ROOT+'Review/Refined/'+name+'.aseprite',false);
 if(s.layer(0).celCount!==frames.length)throw Error('Frame count '+name);
 for(var f=0;f<frames.length;f++){
  var c=s.layer(0).cel(f);
  if(c.x||c.y||c.image.width!==s.width||c.image.height!==s.height)throw Error('Full cel required '+name);
  c.image.putImageData(frames[f]);
 }
 s.commit();s.save();app.activeDocument.close();
}
function sheet(name,w,h){
 var a=read(ROOT+'Originals/IdleRefinement/'+name+'_Supplied.png')[0],sw=a.length/4/h,frames=[];
 for(var f=0;f<sw/w;f++){
  var b=new Uint8Array(w*h*4);
  for(var y=0;y<h;y++)for(var x=0;x<w;x++){
   var i=idx(f*w+x,y,sw),c=hex(a,i);
   if(!a[i+3])continue;
   if(!REFINEMENT_MAPS[name][c])throw Error(name+' missing material '+c);
   put(b,w,h,x,y,REFINEMENT_MAPS[name][c]);
  }
  frames.push(b);
 }return frames;
}
function rect(a,w,h,x0,y0,x1,y1,c){for(var y=y0;y<=y1;y++)for(var x=x0;x<=x1;x++)put(a,w,h,x,y,c);}
function patch(src,dst,w,h,x0,y0,x1,y1,dx,dy,opaque){
 for(var y=y0;y<=y1;y++)for(var x=x0;x<=x1;x++){
  var xx=x+dx,yy=y+dy;
  if(xx>=0&&xx<w&&yy>=0&&yy<h&&(opaque||src[idx(x,y,w)+3]))copy(src,idx(x,y,w),dst,idx(xx,yy,w));
 }
}
function ground(src,ready,w,h,row){patch(ready,src,w,h,0,row,w-1,h-1,0,0,true);return src;}
function shifted(src,w,h,dx,dy){return place(src,w,h,w,h,dx,dy);}

var mReady=read(ROOT+'Miner_Idle.aseprite')[0],bReady=read(ROOT+'BasketVillager_Idle.aseprite')[0],cReady=read(ROOT+'BarricadeVillager_Idle.aseprite')[0];
var mDraft=sheet('Miner',96,80),bDraft=sheet('BasketVillager',64,64),cDraft=sheet('BarricadeVillager',64,64);
var mIdle=[],bIdle=[],cIdle=[];
var mSelection=[1,1,2,3,4,5,6,8,1],bSelection=[0,0,1,2,3,6,7,8,0],cSelection=[0,0,1,3,4,5,7,8,0];
var mShift=[0,0,0,0,1,2,2,1,0];
for(var f=0;f<9;f++){
 var m=shifted(mDraft[mSelection[f]],96,80,mShift[f],0);
 // Mouth remains the approved small neutral mouth. Generated final poses
 // introduced an unrelated gasp and candle flare; retain neither.
 if(f===2){rect(m,96,80,39,49,47,51,SKIN);rect(m,96,80,41,50,45,50,OUT);}
 if(f===3){rect(m,96,80,38,51,47,52,SKIN);rect(m,96,80,40,52,44,52,OUT);}
 if(f===4){rect(m,96,80,38,51,47,53,SKIN);rect(m,96,80,40,52,44,52,OUT);put(m,96,80,36,47,OUT);put(m,96,80,48,47,OUT);}
 if(f===7){
  rect(m,96,80,38,49,47,51,SKIN);rect(m,96,80,41,51,45,51,OUT);
  rect(m,96,80,35,44,51,46,SKIN);
  rect(m,96,80,35,44,39,45,OUT);rect(m,96,80,36,46,38,46,OUT);
  rect(m,96,80,46,44,50,45,OUT);rect(m,96,80,47,46,49,46,OUT);
 }
 // Solid candle and restrained flame follow the hat rather than morphing.
 if(f>1&&f<8){
  var cy=[0,0,1,2,3,3,3,1,0][f],cx=[0,0,0,-1,-1,0,0,0,0][f];
  rect(m,96,80,31,13,43,30,null);
  patch(mReady,m,96,80,33,15,41,28,cx,cy,false);
 }
 ground(m,mReady,96,80,73);
 mIdle.push(f===0||f===1||f===8?new Uint8Array(mReady):m);
 var b=new Uint8Array(bDraft[bSelection[f]]);
 ground(b,bReady,64,64,58);
 bIdle.push(f===0||f===1||f===8?new Uint8Array(bReady):b);
 var c=new Uint8Array(cDraft[cSelection[f]]);
 // Shoulder/head acting is from the supplied animation. Keep the log load
 // rigid instead of accepting the generated switch to rectangular boards.
 if(f>1&&f<8){
  var dx=[0,0,0,0,-1,-1,-1,0,0][f],dy=[0,0,-1,0,2,2,1,0,0][f];
  rect(c,64,64,29,40,63,58,null);
  patch(cReady,c,64,64,29,38,62,56,dx,dy,false);
 }
 ground(c,cReady,64,64,58);
 cIdle.push(f===0||f===1||f===8?new Uint8Array(cReady):c);
}

// Extract only the original iron blade and wooden shaft, never the sleeve.
var rigidAxe=new Uint8Array(64*64*4);
patch(cReady,rigidAxe,64,64,5,45,14,60,0,0,false);
patch(cReady,rigidAxe,64,64,15,48,23,52,0,0,false);
// Idle hand follows the rigid prop through a compact shoulder lift.
for(var f=2;f<8;f++){
 var c=cIdle[f],wx=[25,25,25,25,24,24,24,25,25][f],wy=[50,50,49,49,48,48,49,50,50][f],a=[0,0,2,5,12,12,7,2,0][f];
 rect(c,64,64,0,30,17,59,null);
 rect(c,64,64,18,40,25,59,null);
 arm(c,64,64,27,39+(f>=4&&f<=6?1:0),25,44,wx,wy);
 rotated(rigidAxe,64,64,c,64,64,24,50,wx,wy,a);
 // Compact three-pixel grip rather than a five-pixel rounded blob.
 rect(c,64,64,wx-1,wy-1,wx+1,wy+1,OUT);
 rect(c,64,64,wx-1,wy-1,wx,wy,SKIN);
 ground(c,cReady,64,64,60);
}
writeRefined('Miner_Idle',mIdle);
writeRefined('BasketVillager_Idle',bIdle);
writeRefined('BarricadeVillager_Idle',cIdle);

function attackPose(source,dx,dy,wx,wy,angle){
 var body=new Uint8Array(source);
 rect(body,64,64,0,30,17,60,null);
 rect(body,64,64,18,40,25,60,null);
 var a=place(bop(body,64,64,dx,dy,52,63),64,64,96,64,16,0);
 // A bent elbow during anticipation, then a straightening striking arm.
 var sx=43+dx,sy=39+dy,ex=wx+18,ey=(sy+wy)/2+2;
 arm(a,96,64,sx,sy,ex,ey,wx+16,wy);
 rotated(rigidAxe,64,64,a,96,64,24,50,wx+16,wy,angle);
 rect(a,96,64,wx+15,wy-1,wx+17,wy+1,OUT);
 rect(a,96,64,wx+15,wy-1,wx+16,wy,SKIN);
 patch(place(cReady,64,64,96,64,16,0),a,96,64,0,60,95,63,0,0,true);
 return a;
}
var cAttack=[place(cReady,64,64,96,64,16,0),
 attackPose(cIdle[2],1,0,29,43,45),
 attackPose(cIdle[3],2,1,37,28,140),
 attackPose(cIdle[3],0,-1,27,29,65),
 attackPose(cIdle[4],-3,0,13,43,0),
 attackPose(cIdle[5],-2,1,16,46,-16),
 attackPose(cIdle[7],0,0,23,49,-3),
 place(cReady,64,64,96,64,16,0)];
writeRefined('BarricadeVillager_AutoAttack',cAttack);

// Local anatomical correction only: a forward planted foot, folded rear
// shin and rounded cloth knee. Preserve the existing head/tool/plank poses.
var cAbility=read(ROOT+'Originals/IdleRefinement/BarricadeVillager_Ability_Before.aseprite');
function knee(a,dx,dy,plank){
 var before=new Uint8Array(a);
 rect(a,96,64,45,58,68,63,null);
 // Rear thigh emerging beneath the log bundle; knee contacts the floor.
 line(a,96,64,51+dx,56+dy,53+dx,60,3,OUT);
 line(a,96,64,51+dx,56+dy,53+dx,60,2,'6C5A4A');
 line(a,96,64,50+dx,56+dy,52+dx,58,0,'9A856F');
 // Folded shin lies back along the floor, separated by a dark crease.
 line(a,96,64,54+dx,61,59+dx,61,1,OUT);
 line(a,96,64,55+dx,61,59+dx,61,0,'3E312A');
 rect(a,96,64,59+dx,60,62+dx,62,OUT);
 rect(a,96,64,60+dx,61,61+dx,62,'4A2C1C');
 rect(a,96,64,52+dx,63,62+dx,63,OUT);
 // Front knee is raised; the front sole remains on the shared baseline.
 line(a,96,64,47+dx,55+dy,44+dx,59,2,OUT);
 line(a,96,64,47+dx,55+dy,44+dx,59,1,'6C5A4A');
 rect(a,96,64,42+dx,60,45+dx,62,'4A2C1C');
 rect(a,96,64,41+dx,63,46+dx,63,OUT);
 // The foreground build plank occludes the planted front foot, as before.
 if(plank)patch(before,a,96,64,20,58,44,63,0,0,true);
 // Keep the original waist, hands and timber above the local knee edit.
 patch(before,a,96,64,0,0,95,57,0,0,true);
}
for(var f=2;f<=6;f++){var a=new Uint8Array(cAbility[f]);knee(a,f===6?1:0,f===6?-1:0,f<=5);cAbility[f]=a;}
writeRefined('BarricadeVillager_Ability',cAbility);
