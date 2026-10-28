
var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/IronveinBelowTopHUD/Revision03/",pc=app.pixelColor,src={};
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
try{
 ['FloorRearA','FloorFrontA','FloorRearB','FloorFrontB','FloorRearC','FloorFrontC'].forEach(function(n){app.open(root+'Prepared/'+n+'.png');var s=app.activeSprite,c=s.layer(0).cel(0),im=c.image;src[n]=[];for(var y=0;y<s.height;y++){src[n][y]=[];for(var x=0;x<s.width;x++)src[n][y][x]=(x>=c.x&&y>=c.y&&x<c.x+im.width&&y<c.y+im.height)?im.getPixel(x-c.x,y-c.y):0;}app.activeDocument.close();});
 // Preserve the approved underwater surface geometry, with a cool cave-stone ramp.
 var ramp={'083d50':'10131f','0d5368':'1b2030','176c7d':'252b39','258999':'36435a','3aa3ae':'46556e'};
 ['A','B','C'].forEach(function(v){var name='WalkingSlab'+v;app.open(root+'Prepared/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
  for(var y=0;y<32;y++)for(var x=0;x<64;x++){
   var c=src['Floor'+(y<16?'Rear':'Front')+v][y%16][x];var h=hex(c);if(!ramp[h])throw new Error('Unexpected source color '+h);im.putPixel(x,y,rgb(ramp[h]));
  }
  // Identical edge colors keep all three native slab variants compatible.
  for(var y=0;y<32;y++){var c=y===31?'36435a':(y===30?'46556e':'1b2030');im.putPixel(0,y,rgb(c));im.putPixel(63,y,rgb(c));}
  s.layer(0).name='Cave slab - native underwater-floor adaptation';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
  app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('REVISION03_CLEAN '+name);
 });console.log('REVISION03_CLEANUP_PASS');
}catch(e){console.log('REVISION03_CLEANUP_FAIL '+e);}app.command.Exit();
