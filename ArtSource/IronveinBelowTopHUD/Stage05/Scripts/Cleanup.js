
var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/IronveinBelowTopHUD/Stage05/",pc=app.pixelColor,src={};
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('000000'+((pc.rgbaR(v)<<16)|(pc.rgbaG(v)<<8)|pc.rgbaB(v)).toString(16)).slice(-6);}
try{
['Timber','RockA','RockB','Crystal','Course'].forEach(function(n){app.open(root+'Prepared/Selected'+n+'.png');var s=app.activeSprite,c=s.layer(0).cel(0),im=c.image;src[n]=[];for(var y=0;y<s.height;y++){src[n][y]=[];for(var x=0;x<s.width;x++)src[n][y][x]=(x>=c.x&&y>=c.y&&x<c.x+im.width&&y<c.y+im.height)?im.getPixel(x-c.x,y-c.y):0;}app.activeDocument.close();});
["TimberCap", "TimberRepeat", "TimberFoot", "IronCollar", "RockA", "RockB", "RockFoot", "RockCourse", "AmberDeposit"].forEach(function(name){
 app.open(root+'Prepared/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
 function px(x,y,v){if(x>=0&&y>=0&&x<s.width&&y<s.height)im.putPixel(x,y,typeof v==='string'?rgb(v):v);}
 function copy(which,sx,sy,w,h,dx,dy,map){for(var y=0;y<h;y++)for(var x=0;x<w;x++){var v=src[which][sy+y][sx+x];if(pc.rgbaA(v)){if(map&&map[hex(v)])v=rgb(map[hex(v)]);px(dx+x,dy+y,v);}}}
 function box(x1,y1,x2,y2,v){for(var y=y1;y<=y2;y++)for(var x=x1;x<=x2;x++)px(x,y,v);}
 if(name==='TimberCap')copy('Timber',6,5,20,18,0,0);
 if(name==='TimberRepeat'){
  copy('Timber',6,23,20,24,0,0);
  // Long quiet wood grain: repair the single discontinuity at the repeat seam.
  for(var x=0;x<20;x++)px(x,23,im.getPixel(x,0));
 }
 if(name==='TimberFoot')copy('Timber',6,46,20,15,0,0);
 if(name==='IronCollar'){
  box(0,1,21,6,'0a0d11');box(1,1,20,5,'2b313a');box(2,1,19,1,'55606e');box(2,2,19,4,'303447');
  [3,18].forEach(function(x){px(x,2,'93a1b0');px(x,3,'8a5622');px(x+1,3,'30202a');});
 }
 if(name==='RockA')copy('RockA',5,14,22,46,0,0);
 if(name==='RockB')copy('RockB',5,12,22,48,0,0,{'53556b':'41485b'});
 if(name==='RockFoot'){
  copy('RockA',5,45,22,15,1,1);copy('RockB',18,46,9,14,18,2,{'53556b':'41485b'});
 }
 if(name==='RockCourse'){
  var ramp=['0a0d11','141522','202332','303447','41485b'];
  for(var y=0;y<16;y++)for(var x=0;x<64;x++){
   var v=src.Course[y][x],r=pc.rgbaR(v),g=pc.rgbaG(v),b=pc.rgbaB(v),best=0,dist=1e9;
   for(var i=0;i<ramp.length;i++){var c=rgb(ramp[i]),d=Math.pow(r-pc.rgbaR(c),2)+Math.pow(g-pc.rgbaG(c),2)+Math.pow(b-pc.rgbaB(c),2);if(d<dist){dist=d;best=i;}}
   px(x,y,ramp[best]);
  }
  // Re-author the four main planes from the generated ledge, removing its noisy
  // gravel texture. Irregular boundaries remain stone rather than brick seams.
  function poly(points,h){for(var y=0;y<16;y++)for(var x=0;x<64;x++){var inside=false;for(var i=0,j=points.length-1;i<points.length;j=i++){var a=points[i],b=points[j];if(((a[1]>y+.5)!=(b[1]>y+.5))&&(x+.5<(b[0]-a[0])*(y+.5-a[1])/(b[1]-a[1])+a[0]))inside=!inside;}if(inside)px(x,y,h);}}
  box(0,0,63,15,'141522');box(0,14,63,15,'0a0d11');
  var faces=[[[1,3],[12,1],[16,4],[14,13],[3,15],[1,11]],[[17,1],[31,1],[34,5],[31,14],[17,14],[15,8]],[[35,3],[45,0],[49,3],[47,14],[34,13]],[[50,1],[60,2],[63,5],[62,14],[49,13],[48,7]]];
  faces.forEach(function(p){poly(p,'202332');});
  poly([[1,3],[12,1],[16,4],[12,5],[2,6]],'303447');poly([[17,1],[31,1],[34,5],[29,4],[17,5]],'303447');
  poly([[35,3],[45,0],[49,3],[45,5],[35,6]],'303447');poly([[50,1],[60,2],[63,5],[59,5],[50,4]],'303447');
  poly([[12,5],[16,4],[14,13],[11,14]],'141522');poly([[29,5],[34,5],[31,14],[28,14]],'141522');
  px(8,2,'41485b');px(9,2,'41485b');px(42,2,'41485b');px(43,1,'41485b');
  // Native repeat junction is a deliberately deep fissure, identical on both edges.
  for(var y=0;y<16;y++){px(0,y,y<2?'303447':y<13?'141522':'0a0d11');px(63,y,im.getPixel(0,y));}
 }
 if(name==='AmberDeposit'){
  var map={'8e3006':'8a5622','722707':'30202a','dc6d0d':'c94d08','b96402':'8a5622','f8d072':'ffdb8f','c57405':'c58826','fabd54':'f5923d','faad38':'f5923d','d59f2d':'c58826','e2e9e7':'ffdb8f','f3911f':'f5923d','1a1c2f':'141522','474c5a':'303447','5b616d':'41485b','353949':'202332'};
  copy('Crystal',4,1,24,30,0,0,map);
  // Replace noisy gravel speckles with quiet connected rock planes.
  for(var y=20;y<30;y++)for(var x=0;x<24;x++)if(pc.rgbaA(im.getPixel(x,y))){var v=im.getPixel(x,y);if(hex(v)==='41485b')px(x,y,'303447');}
 }
 s.layer(0).name='Stage05 native cleanup from PixelLab modules';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
 app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('STAGE05_CLEAN '+name);
});console.log('STAGE05_CLEANUP_PASS');}catch(e){console.log('STAGE05_CLEANUP_FAIL '+e);}app.command.Exit();
