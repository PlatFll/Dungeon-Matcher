// Run in LibreSprite. Preserve supplied drawings and colors, fit the complete
// effect footprint to a 64px tile using nearest-neighbor native pixel clusters.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/TileVfx/';
var jobs=[['Explosion3','Explosion',1.50],['Poisonexplosion2','PoisonExplosion',1.24],['ShieldExplosion','ShieldExplosion',1.44],['HealingExplosion','HealingExplosion',1.30]];
for(var j=0;j<jobs.length;j++){
 var job=jobs[j];app.open(ROOT+'Originals/'+job[0]+'.ase');var s=app.activeSprite,frames=[];
 for(var f=0;f<s.layer(0).celCount;f++){
  var cel=s.layer(0).cel(f),src=cel.image.getImageData(),w=cel.image.width,h=cel.image.height;
  var out=new Uint8Array(64*64*4);
  for(var y=0;y<64;y++)for(var x=0;x<64;x++){
   var sx=Math.round((x-31.5)/job[2]+24)-cel.x,sy=Math.round((y-31.5)/job[2]+24)-cel.y;
   if(sx<0||sy<0||sx>=w||sy>=h)continue;
   var i=(sy*w+sx)*4,k=(y*64+x)*4;
   for(var c=0;c<4;c++)out[k+c]=src[i+c];
  }
  frames.push(out);
 }
 s.saveAs(ROOT+job[1]+'.aseprite',false);s.resize(64,64);s.commit();
 app.command.BackgroundFromLayer();s.commit();
 for(var f=0;f<frames.length;f++){
  var cel=s.layer(0).cel(f);
  if(cel.x||cel.y||cel.image.width!==64||cel.image.height!==64)throw Error('Expected full source cel: '+job[1]);
  cel.image.putImageData(frames[f]);
 }
 s.layer(0).name=job[1];s.commit();s.save();app.command.GotoFirstFrame();app.command.ScrollCenter();
}
