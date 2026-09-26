// Run inside LibreSprite after the helper portion of animate_native.js.
// Reuses its inspected anatomy/material selections, NOT its rejected motion.
// The accepted guard displacement/exposure pattern is the motion authority.
var SELECTED=['TownMarshal','SwordKnight','SpearKnight','ShieldKnight','KnightCaptain','RoyalSwordsman','RoyalLancer','RoyalArbalist','RoyalStandardBearer','RoyalArcanist','RoyalMage','King'];
var FAMILY_HD=[0,-1,0,1,3,3,2,1,0],FAMILY_BD=[0,-1,0,1,2,2,1,0,0];
function familyPose(ready,ac,f){
 if(f===0||f===8)return new Uint8Array(ready);
 var hd=(ac.heavy?[0,-1,0,1,2,2,1,0,0]:FAMILY_HD)[f];
 var bd=(ac.heavy?[0,-1,0,1,1,1,1,0,0]:FAMILY_BD)[f];
 var lean=[0,0,0,0,-1,-1,0,0,0][f],roll=[0,0,1,1,1,1,0,0,0][f];
 var lag=[0,0,0,1,1,2,1,0,0][f];
 var p=extract(blink(ready,ac,f===5?2:(f===4||f===6)?1:0),ac),out=new Uint8Array(ready.length);
 deformBody(p.body,out,bd,lean,roll,ac,lag);
 if(ac.banner){
  for(var y=0;y<29;y++)for(var x=12;x<64;x++)if(p.flag[ix(x,y)+3])cp(p.flag,ix(x,y),out,ix(x,y+Math.round(lag*Math.max(0,(x-17)/46))));
  for(var y=13;y<25;y++)for(var x=30;x<46;x++)if(!out[ix(x,y)+3])px(out,x,y,y<16?'7C1A30':'4B1127');
 }
 var pb=solidBounds(p.prop),pdx=ac.propDy==='planted'?0:lean;
 if(pb[0]+pdx<0||pb[2]+pdx>63)pdx=0;
 stamp(p.prop,out,pdx,ac.propDy==='planted'?0:Math.min(bd,63-pb[3]));
 var sb=solidBounds(p.shield);if(sb[2]>=0)stamp(p.shield,out,0,Math.min(bd,63-sb[3]));
 var hb=solidBounds(p.head);hd=Math.max(hd,-hb[1]);
 // Exact rigid face/helmet translation. No extra head pitch or sideways shake.
 stamp(p.head,out,lean,hd);
 return out;
}
for(var n=0;n<SELECTED.length;n++){
 var name=SELECTED[n],ac=ACTING[name],ready=readReady(name),frames=[];
 // The two enlarged helmets are already in the approved static source. Use
 // their complete saved head part to retain proportions without rescaling.
 if(name==='RoyalLancer'||name==='RoyalArbalist'){
  app.open(ROOT+'Review/Correction/RigParts/'+name+'.aseprite');
  ac.grownHead=new Uint8Array(app.activeSprite.layer(0).cel(2).image.getImageData());app.activeDocument.close();
 }
 for(var f=0;f<9;f++)frames.push(familyPose(ready,ac,f));
 write(name,frames,palette(ready),ROOT+'SelectedIdles/'+name+'_Idle.aseprite');
 console.log('FAMILY '+name);
}

// Crossbow repair retains the accepted head rhythm and complete source bow.
// Its lower wood/string contour must not enter the leg deformation region.
var ready=readReady('CrossbowGuard'),frames=[];
for(var f=0;f<9;f++)frames.push(familyPose(ready,ACTING.CrossbowGuard,f));
write('CrossbowGuard',frames,palette(ready),ROOT+'../GuardIdles/CrossbowGuard_Idle.aseprite');

// Local characters: the same settled guard poses on their exact ready drawings.
// Their action cels/controllers are not rewritten by this native script.
var LOCAL='C:/UnityProjects/Dungeon Matcher/ArtSource/LocalEnemies/';
function li(x,y,w){return(y*w+x)*4;}
function lcopy(a,i,b,j){for(var k=0;k<4;k++)b[j+k]=a[i+k];}
function lstamp(a,b,w,h,dx,dy){for(var y=0;y<h;y++)for(var x=0;x<w;x++)if(a[li(x,y,w)+3]){
 if(x+dx<0||x+dx>=w||y+dy<0||y+dy>=h)throw Error('Local rigid part clipped');
 lcopy(a,li(x,y,w),b,li(x+dx,y+dy,w));
}}
function lset(a,w,x,y,c){var i=li(x,y,w);for(var k=0;k<3;k++)a[i+k]=parseInt(c.slice(k*2,k*2+2),16);a[i+3]=255;}
var LOCALS={
 Miner:{w:96,h:80,head:[[30,12],[64,12],[64,47],[58,50],[56,53],[42,54],[37,52],[33,49],[33,40],[29,38]],eyes:[[36,42,40,44],[47,42,51,44]],props:[[[28,50],[33,50],[31,55],[28,59],[26,62],[32,62],[40,63],[47,64],[60,67],[61,63],[67,62],[71,64],[71,70],[75,70],[75,75],[61,75],[60,72],[47,71],[38,70],[33,69],[29,67],[27,67],[27,71],[29,73],[30,78],[26,78],[21,73],[20,68],[20,59],[24,54]]],center:49,neck:[42,51,55,57],nodes:[0,51,63,71,76,79]},
 BasketVillager:{w:64,h:64,head:[[7,5],[56,5],[56,27],[47,27],[47,34],[41,37],[25,37],[20,35],[18,31],[18,27],[7,27]],eyes:[[22,27,24,30],[33,27,35,30]],props:[[[19,48],[25,48],[25,53],[25,56],[25,64],[9,64],[9,55],[12,52],[18,50]]],center:33,neck:[26,35,39,41],nodes:[0,35,47,55,60,63]},
 BarricadeVillager:{w:64,h:64,head:[[15,6],[49,6],[49,31],[45,35],[39,38],[27,38],[23,35],[20,31],[20,25],[15,25]],eyes:[[23,26,26,29],[33,26,36,29]],props:[box(5,46,16,61),box(15,49,25,54),[[50,40],[54,38],[62,40],[63,44],[61,49],[50,50]],[[28,44],[36,42],[43,44],[47,48],[49,48],[50,53],[45,56],[29,56]]],center:34,neck:[27,36,40,42],nodes:[0,35,47,55,60,63]}
};
for(var name in LOCALS){
 var ac=LOCALS[name],w=ac.w,h=ac.h;
 app.open(LOCAL+'Originals/BeforeGroundedFamily/'+name+'_Idle.aseprite');var s=app.activeSprite;
 var ready=new Uint8Array(s.layer(0).cel(0).image.getImageData());app.activeDocument.close();
 var frames=[];
 for(var f=0;f<9;f++){
  if(f===0||f===8){frames.push(new Uint8Array(ready));continue;}
  var input=new Uint8Array(ready);
  if(f===4||f===5||f===6)for(var e=0;e<ac.eyes.length;e++){
   var b=ac.eyes[e];for(var y=b[1];y<=b[3];y++)for(var x=b[0];x<=b[2];x++){
    if(f===5)lset(input,w,x,y,y===b[3]?'0A0D11':'E6B08A');
    else if(y===b[1])lset(input,w,x,y,'E6B08A');
   }
  }
  var head=new Uint8Array(input.length),prop=new Uint8Array(input.length),body=new Uint8Array(input.length),out=new Uint8Array(input.length),maxPropY=0;
  for(var y=0;y<h;y++)for(var x=0;x<w;x++){
   var target=within(x,y,ac.head)?head:inParts(x,y,ac.props)?prop:body;
   lcopy(input,li(x,y,w),target,li(x,y,w));
   if(target===prop&&input[li(x,y,w)+3])maxPropY=Math.max(maxPropY,y);
  }
  var neck=ac.neck;
  for(var y=neck[1];y<=neck[3];y++)for(var x=neck[0];x<=neck[2];x++)if(!body[li(x,y,w)+3]){
   for(var yy=y+1;yy<=neck[3]+4;yy++)if(body[li(x,yy,w)+3]){lcopy(body,li(x,yy,w),body,li(x,y,w));break;}
  }
  // Continue the sleeve/torso behind the held object. Different shoulder and
  // prop offsets must reveal cloth/skin, never a transparent horizontal tear.
  var joins=name==='Miner'?[[35,61,66,72]]:name==='BasketVillager'?[[22,48,26,56]]:[[22,48,27,55],[30,44,43,56]];
  for(var j=0;j<joins.length;j++){
   var b=joins[j];for(var x=b[0];x<=b[2];x++){
    var sample=-1;for(var y=b[1];y>=b[1]-4;y--)if(body[li(x,y,w)+3]){sample=y;break;}
    if(sample<0)continue;
    for(var y=b[1]+1;y<=b[3];y++)if(!body[li(x,y,w)+3]&&prop[li(x,y,w)+3])lcopy(body,li(x,sample,w),body,li(x,y,w));
   }
  }
  var bd=FAMILY_BD[f],hd=FAMILY_HD[f],lean=[0,0,0,0,-1,-1,0,0,0][f];
  var nd=ac.nodes,nodes=[[nd[0],nd[0]+bd],[nd[1],nd[1]+bd],[nd[2],nd[2]+bd*.65],[nd[3],nd[3]+bd*.25],[nd[4],nd[4]],[nd[5],nd[5]]];
  for(var y=0;y<h;y++)for(var x=0;x<w;x++){
   var sy=inverseRow(y,nodes),sx=x-Math.round(lean*clamp((nd[4]-sy)/18,0,1));
   if(sx>=0&&sx<w&&sy>=0&&sy<h&&body[li(sx,sy,w)+3])lcopy(body,li(sx,sy,w),out,li(x,y,w));
  }
  lstamp(prop,out,w,h,lean,Math.min(bd,h-1-maxPropY));
  lstamp(head,out,w,h,lean,hd);
  frames.push(out);
 }
 app.open(LOCAL+'Originals/BeforeGroundedFamily/'+name+'_Idle.aseprite');s=app.activeSprite;
 s.saveAs(LOCAL+name+'_Idle.aseprite',false);
 for(var f=0;f<9;f++)s.layer(0).cel(f).image.putImageData(frames[f]);
 s.commit();s.save();app.activeDocument.close();console.log('FAMILY '+name);
}
