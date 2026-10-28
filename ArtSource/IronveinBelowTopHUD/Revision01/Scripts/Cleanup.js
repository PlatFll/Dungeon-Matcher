
var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/IronveinBelowTopHUD/Revision01/",pc=app.pixelColor,src={};
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
try{
 ['SelectedLeft','SelectedRight','SelectedFloor'].forEach(function(n){app.open(root+'Prepared/'+n+'.png');var s=app.activeSprite,c=s.layer(0).cel(0),im=c.image;src[n]=[];for(var y=0;y<s.height;y++){src[n][y]=[];for(var x=0;x<s.width;x++)src[n][y][x]=(x>=c.x&&y>=c.y&&x<c.x+im.width&&y<c.y+im.height)?im.getPixel(x-c.x,y-c.y):0;}app.activeDocument.close();});
 var ramp={'382c34':'32303f','2a212a':'22232f','45363e':'464052','7b6d62':'665b51','958574':'8b7861','554b47':'49424a'};
 function floor(v){return pc.rgbaA(v)<128?rgb('32303f'):rgb(ramp[hex(v)]||'32303f');}
 ["FloorSurfaceA", "FloorSurfaceB", "FloorFaceA", "FloorFaceB", "CrystalLeft", "CrystalRight"].forEach(function(name){
  app.open(root+'Prepared/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
  function px(x,y,v){if(x>=0&&y>=0&&x<s.width&&y<s.height)im.putPixel(x,y,typeof v==='string'?rgb(v):v);}
  if(name.indexOf('Crystal')===0){
   var a=src[name==='CrystalLeft'?'SelectedLeft':'SelectedRight'];
   for(var y=0;y<64;y++)for(var x=0;x<56;x++){var v=a[y][x];if(pc.rgbaA(v)>=128)px(x,y,pc.rgba(pc.rgbaR(v),pc.rgbaG(v),pc.rgbaB(v),255));}
   // Preserve the generated facets. Only consolidate accidental isolated stone
   // highlights into the nearest existing stone plane at the grounded base.
   for(var y=49;y<61;y++)for(var x=7;x<49;x++){
    var v=im.getPixel(x,y),h=hex(v);if(h!=='706478')continue;
    var same=0;[[1,0],[-1,0],[0,1],[0,-1]].forEach(function(d){if(hex(im.getPixel(x+d[0],y+d[1]))===h)same++;});
    if(same===0)px(x,y,'575164');
   }
  }else if(name.indexOf('FloorSurface')===0){
   // Native row additions create an 8px walking plane; no interpolation.
   var rows=[15,16,17,17,18,19,20,21];
   for(var y=0;y<8;y++)for(var x=0;x<32;x++){var sx=(x+(name==='FloorSurfaceB'?12:0))%32;px(x,y,floor(src.SelectedFloor[rows[y]][96+sx]));}
   // Large quiet slab joints and a short shadow at the turning edge.
   var split=name==='FloorSurfaceA'?21:11;
   for(var y=1;y<6;y++)px(split+(y>3?1:0),y,'49424a');
   for(var y=0;y<8;y++){var c=['665b51','665b51','665b51','665b51','665b51','8b7861','49424a','49424a'][y];px(0,y,c);px(31,y,c);}
  }else{
   var offset=name==='FloorFaceB'?13:0;
   for(var y=0;y<19;y++)for(var x=0;x<32;x++)px(x,y,floor(src.SelectedFloor[32+y][64+(x+offset)%32]));
   for(var x=0;x<32;x++){px(x,0,'22232f');px(x,18,'22232f');}
   for(var y=1;y<18;y++){var c=y<7?'464052':(y<15?'32303f':'22232f');px(0,y,c);px(31,y,c);}
  }
  s.layer(0).name='Revision01 native PixelLab cleanup';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
  app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('REVISION01_CLEAN '+name);
 });console.log('REVISION01_CLEANUP_PASS');
}catch(e){console.log('REVISION01_CLEANUP_FAIL '+e);}app.command.Exit();
