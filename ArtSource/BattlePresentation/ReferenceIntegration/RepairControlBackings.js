// LibreSprite: restore only target-background pixels exposed through the
// transparent outer corners of the top controls and player-frame contour.
var BASE='__BASE__/',OUT='__OUT__/';
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),r={w:s.width,h:s.height,p:c.image.getImageData()};if(c.x||c.y||c.image.width!==s.width||c.image.height!==s.height)throw 'Unexpected trimmed source';app.activeDocument.close();return r;}
function inside(x,y,p){var b=false;for(var i=0,j=p.length-1;i<p.length;j=i++){var A=p[i],B=p[j];if((A[1]>y)!=(B[1]>y)&&x<(B[0]-A[0])*(y-A[1])/(B[1]-A[1])+A[0])b=!b;}return b;}
var contour=[[15,0],[119,0],[135,14],[134,242],[120,258],[12,258],[0,244],[0,15]];
for(var z=0;z<2;z++){
 var zone=z?'Court':'Forest',kit=BASE+zone+'ReferenceKit/';
 var target=read(kit+'reference/Approved.png');
 var names=['WaveHeader','SharedGuide','SharedSettings'],pos=[[157,20],[330,30],[414,30]],plates=[];
 for(var n=0;n<names.length;n++)plates.push(read(kit+'native/'+names[n]+'.png'));
 for(var layer=0;layer<2;layer++){
 var name=layer?'BattleFloor':'FarScenery',offset=layer?211:0,height=layer?87:160;
 app.open(kit+'native/'+name+'.aseprite');var s=app.activeSprite,c=s.layer(0).cel(0),p=c.image.getImageData(),count=0;
 if(s.width!==470||s.height!==height||c.x||c.y)throw 'Unexpected scenery registration';
 if(!layer)
 for(var n=0;n<plates.length;n++){
  var plate=plates[n];
  for(var y=0;y<plate.h;y++)for(var x=0;x<plate.w;x++){
   if(plate.p[(y*plate.w+x)*4+3])continue;
   var X=pos[n][0]+x,Y=pos[n][1]+y,i=(Y*470+X)*4,j=((Y*2+1)*target.w+X*2+1)*4,changed=false;
   for(var k=0;k<4;k++){if(p[i+k]!==target.p[j+k])changed=true;p[i+k]=target.p[j+k];}
   if(changed)count++;
  }
 }
 // The placeholder rectangle starts two rows above the actual frame.
 // Restore its exposed exterior only; never copy the hero inside the panel.
 for(var Y=Math.max(22,offset);Y<Math.min(280,offset+height);Y++)for(var X=10;X<142;X++){
  if(inside(X-8+.5,Y-24+.5,contour))continue;
  var i=((Y-offset)*470+X)*4,j=((Y*2+1)*target.w+X*2+1)*4,changed=false;
  for(var k=0;k<4;k++){if(p[i+k]!==target.p[j+k])changed=true;p[i+k]=target.p[j+k];}
  if(changed)count++;
 }
 c.image.putImageData(p);s.commit();s.saveAs(OUT+zone+'/'+name+'.aseprite',false);app.activeDocument.close();
 app.open(OUT+zone+'/'+name+'.aseprite');app.activeSprite.saveAs(OUT+zone+'/'+name+'.png',false);app.activeDocument.close();
 console.log('REPAIRED '+zone+' '+name+' '+count);
 }
}
console.log('REPAIR_COMPLETE');app.command.Exit();
