// LibreSprite native-pixel finishing. Pinned PixelLab poses supply both hands.
// Add a short draw-back and straight extension while keeping the boots planted.
var ROOT='__ROOT__';
var frames=[0,2,3,4,5,6,8,7,5,4,2,0];
var shifts=[0,0,2,4,6,-3,-8,-4,2,2,0,0];
function footCenter(c){
 var p=new Uint8Array(c.image.getImageData()),bottom=0,left=128,right=0;
 for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)
  if(x+c.x>=55&&p[(y*c.image.width+x)*4+3]>127)bottom=Math.max(bottom,y+c.y);
 for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)
  if(x+c.x>=55&&y+c.y>=bottom-3&&p[(y*c.image.width+x)*4+3]>127){left=Math.min(left,x+c.x);right=Math.max(right,x+c.x);}
 return Math.round((left+right)/2);
}
['SpearGuard','SpearKnight','RoyalLancer'].forEach(function(name){
 app.open(ROOT+'.utmp/'+name+'Candidate.aseprite');var src=app.activeSprite,poses=[];
 var anchor=footCenter(src.layer(0).cel(0));
 for(var f=0;f<frames.length;f++){
  var c=src.layer(0).cel(frames[f]),p=new Uint8Array(c.image.getImageData());
  var out=new Uint8Array(192*80*4),pad=32+anchor-footCenter(c);
  for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++){
   var yy=y+c.y,weight=yy<65?1:Math.max(0,(78-yy)/13);
   var xx=x+c.x+pad+Math.round(shifts[f]*weight),i=(y*c.image.width+x)*4;
   if(xx<0||xx>=192||yy<0||yy>=80){if(p[i+3]>127)throw Error('Clipped thrust '+name);continue;}
   var q=(yy*192+xx)*4;for(var k=0;k<4;k++)out[q+k]=p[i+k];
  }
  poses.push(out);
 }
 app.activeDocument.close();
 app.open(ROOT+'ArtSource/CombatActions/Originals/Farmer_Idle.aseprite');var s=app.activeSprite;
 s.saveAs(ROOT+'ArtSource/EnemyAttacks/Refinements/'+name+'Prepared.aseprite',false);
 s.resize(192,80);app.command.BackgroundFromLayer();
 while(s.layer(0).celCount>poses.length){app.command.GotoLastFrame();app.command.RemoveFrame();}
 while(s.layer(0).celCount<poses.length){app.command.GotoLastFrame();app.command.NewFrame();}
 for(var f=0;f<poses.length;f++){
  var c=s.layer(0).cel(f);
  if(c.x!==0||c.y!==0||c.image.width!==192||c.image.height!==80)throw Error('Invalid full canvas '+name);
  c.image.putImageData(poses[f]);
 }
 app.command.LayerFromBackground();s.layer(0).name='Two-handed thrust';s.commit();s.save();app.activeDocument.close();
});app.command.Exit();
