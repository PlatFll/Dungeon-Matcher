var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/';
var pc=app.pixelColor;
function cleanup(name) {
 app.open(root+'RawPixelLab/Stage01/'+name+'-raw.png');
 var s=app.activeSprite,im=app.activeImage;
 if(!s||s.width!==256||s.height!==192||s.filename.indexOf(name+'-raw.png')<0)throw new Error('Wrong authorized source');
 var far=name==='FarRuins';
 var ramp=far?[[14,48,62],[18,60,73],[23,73,84],[29,87,95]]:[[7,28,40],[10,39,51],[15,52,63],[22,66,76],[31,82,90]];
 var changes=0,removed=0;
 for(var y=0;y<192;y++)for(var x=0;x<256;x++) {
  var old=im.getPixel(x,y),a=pc.rgbaA(old),r=pc.rgbaR(old),g=pc.rgbaG(old),b=pc.rgbaB(old),c;
  if(a===0)c=pc.rgba(0,0,0,0);
  else if(far && x>=70 && x<=174 && y>=105 && r>=142) {c=pc.rgba(0,0,0,0);removed++;}
  else {
   var v=(r*.2126+g*.7152+b*.0722);
   var n=far?(v<105?0:v<140?1:v<175?2:3):(v<39?0:v<58?1:v<79?2:v<105?3:4);
   var rgb=ramp[n];c=pc.rgba(rgb[0],rgb[1],rgb[2],255);
  }
  if(c!==old)changes++;
  im.putPixel(x,y,c);
 }
 s.layer(0).name=far?'PixelLab far ruins - matte four-tone depth':'PixelLab mid ruins - five-tone teal';
 s.commit();s.saveAs(root+'Native/Stage01/'+name+'.ase',true);s.saveAs(root+'Exports/Stage01/'+name+'.png',true);
 console.log(name+' edited='+changes+' centralMassRemoved='+removed+' palette='+ramp.length);app.activeDocument.close();
}
cleanup('FarRuins');cleanup('MidRuins');app.command.Exit();
