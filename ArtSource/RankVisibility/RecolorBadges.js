// Exact native-pixel recolor in LibreSprite. Preserve the established rank silhouettes.
var ROOT='__ROOT__';
var ramps={Normal:['101b18','185c42','27a95b','75e85c','d9ff9a','f4ffda'],Special:['101b26','174d77','168ecd','35dafa','a0f7ff','edffff'],Miniboss:['231813','824321','d27920','ffb83f','ffe486','fff6d0']};
Object.keys(ramps).forEach(function(rank){
 app.open(ROOT+'ArtSource/FinalizedVisuals/UI/'+rank+'Badge.png');
 var s=app.activeSprite,p=new Uint8Array(s.layer(0).cel(0).image.getImageData());
 for(var i=0;i<p.length;i+=4){
  if(p[i+3]===0)continue;
  var y=.2126*p[i]+.7152*p[i+1]+.0722*p[i+2];
  var index=y<30?0:y<65?1:y<105?2:y<150?3:y<205?4:5;
  var c=ramps[rank][index];p[i]=parseInt(c.substr(0,2),16);p[i+1]=parseInt(c.substr(2,2),16);p[i+2]=parseInt(c.substr(4,2),16);
 }
 s.saveAs(ROOT+'ArtSource/RankVisibility/'+rank+'Badge.aseprite',false);
 app.command.BackgroundFromLayer();s.commit();s.layer(0).cel(0).image.putImageData(p);s.layer(0).name=rank+' rank';s.commit();s.save();app.activeDocument.close();
});app.command.Exit();
