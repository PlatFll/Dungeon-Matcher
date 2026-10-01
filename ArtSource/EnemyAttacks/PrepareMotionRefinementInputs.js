// Run in LibreSprite. Copy native pixels into larger transparent canvases.
// No character pixels are scaled; the shared bottom-center anchor is retained.
var ROOT='__ROOT__';
var jobs=[
 ['SpearGuard','ArtSource/GuardIdles/SpearGuard_Idle.png',64,64,128,80],
 ['SpearKnight','ArtSource/RemainingCast/SelectedIdles/SpearKnight_Idle.png',64,64,128,80],
 ['RoyalLancer','ArtSource/RemainingCast/SelectedIdles/RoyalLancer_Idle.png',64,64,128,80],
 ['ShieldKnight','ArtSource/RemainingCast/SelectedIdles/ShieldKnight_Idle.png',64,64,96,80],
 ['KingIdle','ArtSource/RemainingCast/SelectedIdles/King_Idle.png',64,64,96,80]
];
jobs.forEach(function(j){
 app.open(ROOT+j[1]);var s=app.activeSprite,c=s.layer(0).cel(0);
 var data=new Uint8Array(c.image.getImageData()),sw=c.image.width,cx=c.x,cy=c.y;
 var out=new Uint8Array(j[4]*j[5]*4),dx=(j[4]-j[2])/2,dy=j[5]-j[3];
 for(var y=0;y<j[3];y++)for(var x=0;x<j[2];x++){
  if(x<cx||y<cy||x>=cx+sw||y>=cy+c.image.height)continue;
  var src=((y-cy)*sw+x-cx)*4,dst=((y+dy)*j[4]+x+dx)*4;
  for(var k=0;k<4;k++)out[dst+k]=data[src+k];
 }
 app.activeDocument.close();
 app.open(ROOT+'ArtSource/CombatActions/Originals/Farmer_Idle.aseprite');
 s=app.activeSprite;
 s.saveAs(ROOT+'.utmp/'+j[0]+'_padded.aseprite',false);
 s.resize(j[4],j[5]);app.command.BackgroundFromLayer();
 while(s.layer(0).celCount>1){app.command.GotoLastFrame();app.command.RemoveFrame();}
 c=s.layer(0).cel(0);if(c.image.width!==j[4]||c.image.height!==j[5])throw Error('Canvas mismatch '+j[0]+' '+c.image.width+'x'+c.image.height);
 c.image.putImageData(out);app.command.LayerFromBackground();s.commit();
 s.saveAs(ROOT+'ArtSource/EnemyAttacks/Refinements/Inputs/'+j[0]+'.png',false);app.activeDocument.close();
});app.command.Exit();
