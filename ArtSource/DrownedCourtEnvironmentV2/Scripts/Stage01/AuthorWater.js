var root = 'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/';
var colors = [[6,24,36],[8,32,44],[11,41,53],[15,51,62],[20,63,73],[27,77,86],[36,94,100],[49,113,116]];
var pc=app.pixelColor;
function base(x,y) {
  // Deliberate broad bands; no interpolated gradients or randomized texture.
  var sx=Math.floor(x/3)*3,sy=Math.floor(y/3)*3;
  var d=Math.sqrt(Math.pow((sx-228)/1.18,2)+Math.pow(sy+35,2));
  var level=Math.max(0,Math.min(7,7-Math.floor((d-30)/37)));
  return colors[level];
}
function doc(name,paint) {
  app.open(root+'Native/Stage01/'+name+'-blank.png');
  var s=app.activeSprite,im=app.activeImage;
  console.log('DOCUMENT '+(s?s.filename+' '+s.width+'x'+s.height:'NO_ACTIVE_SPRITE'));
  if(!s || s.width!==360 || s.height!==240 || s.filename.indexOf(name+'-blank.png')<0)throw new Error('Wrong working document');
  for(var y=0;y<240;y++)for(var x=0;x<360;x++) {
    var c=paint(x,y);im.putPixel(x,y,c?pc.rgba(c[0],c[1],c[2],255):pc.rgba(0,0,0,0));
  }
  s.layer(0).name=name+' native clusters';s.commit();
  s.saveAs(root+'Native/Stage01/'+name+'.ase',true);s.saveAs(root+'Exports/Stage01/'+name+'.png',true);
  console.log('AUTHORED '+name+' '+s.width+'x'+s.height);app.activeDocument.close();
}
doc('DeepWater',base);
doc('LightShafts',function(x,y){
 if(y>=177)return null;
 var left=182-Math.floor(y*.24),right=203-Math.floor(y*.12);
 var left2=237+Math.floor(y*.13),right2=247+Math.floor(y*.20);
 var edge=(x>=left&&x<=right)||(x>=left2&&x<=right2);
 if(!edge)return null;
 var c=base(x,y),boost=12-Math.floor(y/32)*2;
 return [c[0]+Math.floor(boost*.7),c[1]+boost,c[2]+boost];
});
var points=[[151,49],[276,76],[193,98],[326,34],[243,138],[118,117],[305,155],[218,67]];
doc('Particles',function(x,y){for(var i=0;i<points.length;i++)if(x===points[i][0]&&y===points[i][1])return [69,129,136];return null;});
app.command.Exit();
