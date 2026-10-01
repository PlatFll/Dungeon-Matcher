// PixelLab provides the poses; LibreSprite preserves their native pixel grid.
// Palette cleanup uses the exact source character's colors, with an optional hit glint.
var ROOT='__ROOT__', jobs=__JOBS__;
jobs.forEach(function(job){
 app.open(ROOT+job.source);var reference=app.activeSprite;
 var pixels=new Uint8Array(reference.layer(0).cel(0).image.getImageData()),colors=[],seen={};
 for(var i=0;i<pixels.length;i+=4){if(pixels[i+3]===0)continue;var k=pixels[i]+','+pixels[i+1]+','+pixels[i+2];if(!seen[k]){seen[k]=true;colors.push([pixels[i],pixels[i+1],pixels[i+2]]);}}
 app.activeDocument.close();
 var accents=job.accent==='cyan'?[[55,164,224],[119,225,255],[222,251,255]]:job.accent==='green'?[[83,164,70],[158,221,96],[234,249,172]]:job.accent==='steel'?[[222,238,247]]:[[255,224,128],[255,249,196]];
 accents.forEach(function(c){colors.push(c);});
 app.open(ROOT+job.assembly);var sprite=app.activeSprite;
 for(var f=0;f<job.frames.length;f++){
  var cel=sprite.layer(0).cel(f);if(!cel)continue;
  var p=new Uint8Array(cel.image.getImageData());
  for(var i=0;i<p.length;i+=4){
   if(p[i+3]<128){p[i]=p[i+1]=p[i+2]=p[i+3]=0;continue;}
   if(job.greenToGold && p[i+1]>p[i]*1.25 && p[i+1]>p[i+2]*1.3){p[i]=255;p[i+1]=224;p[i+2]=128;}
   var best=colors[0],distance=1e10;
   for(var c=0;c<colors.length;c++){var rgb=colors[c],delta=(p[i]-rgb[0])*(p[i]-rgb[0])+(p[i+1]-rgb[1])*(p[i+1]-rgb[1])+(p[i+2]-rgb[2])*(p[i+2]-rgb[2]);if(delta<distance){distance=delta;best=rgb;}}
   p[i]=best[0];p[i+1]=best[1];p[i+2]=best[2];p[i+3]=255;
  }
  cel.image.putImageData(p);
 }
 sprite.commit();sprite.saveAs(ROOT+'ArtSource/EnemyAttacks/'+job.name+'.aseprite',false);app.activeDocument.close();
});app.command.Exit();
