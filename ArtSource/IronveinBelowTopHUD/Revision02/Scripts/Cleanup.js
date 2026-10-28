
var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/IronveinBelowTopHUD/Revision02/",pc=app.pixelColor,src={};
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
try{
 ['FloorSurfaceA','FloorSurfaceB'].forEach(function(n){app.open(root+'Prepared/Source'+n+'.png');var s=app.activeSprite,c=s.layer(0).cel(0),im=c.image;src[n]=[];for(var y=0;y<s.height;y++){src[n][y]=[];for(var x=0;x<s.width;x++)src[n][y][x]=(x>=c.x&&y>=c.y&&x<c.x+im.width&&y<c.y+im.height)?im.getPixel(x-c.x,y-c.y):0;}app.activeDocument.close();});
 ["FloorSurfaceA", "FloorSurfaceB", "SeparatorBlockA", "SeparatorBlockB"].forEach(function(name){
  app.open(root+'Prepared/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
  function px(x,y,v){if(x>=0&&y>=0&&x<s.width&&y<s.height)im.putPixel(x,y,typeof v==='string'?rgb(v):v);}
  function box(x1,y1,x2,y2,v){for(var y=y1;y<=y2;y++)for(var x=x1;x<=x2;x++)px(x,y,v);}
  if(name.indexOf('FloorSurface')===0){
   box(0,0,31,19,'10131f');
   // Add native rows behind the old walking edge, with broad cave-tone planes.
   box(0,4,31,10,'171e2e');box(0,11,31,18,'171e2e');
   var b=name==='FloorSurfaceB';
   box(b?2:8,2,b?19:26,3,'171e2e');
   box(b?4:10,3,b?22:29,4,'171e2e');
   box(b?17:3,8,b?29:13,9,'10131f');
   box(b?20:5,10,b?31:16,10,'10131f');
   var ramp={'665b51':'171e2e','8b7861':'242e42','49424a':'10131f','464052':'1c1c2b'};
   for(var y=0;y<8;y++)for(var x=0;x<32;x++)px(x,y+12,ramp[hex(src[name][y][x])]||'171e2e');
   // Keep both ends identical for an uninterrupted native repeat.
   for(var y=0;y<20;y++){var c=y<4?'10131f':y<18?'171e2e':'10131f';px(0,y,c);px(31,y,c);}
  }else{
   // One broad hand-shaped charcoal block, not a stretched procedural rectangle.
   // Four restrained planes and two-pixel joints stay readable behind the bars.
   box(0,0,39,18,'171c28');box(2,1,37,17,'353d4b');
   box(2,2,37,14,'454e5d');box(3,1,36,2,'626c7a');
   box(1,3,2,15,'505b6a');box(3,3,36,4,'505b6a');
   box(37,3,38,15,'2a303d');box(3,15,36,16,'353d4b');box(4,17,35,17,'2a303d');
   // A few connected chips; no scattered texture.
   if(name==='SeparatorBlockA'){
    box(27,1,30,1,'353d4b');box(28,2,30,2,'505b6a');box(8,14,11,14,'353d4b');box(7,15,10,15,'2a303d');
   }else{
    box(9,1,11,1,'353d4b');box(10,2,12,2,'505b6a');box(25,13,27,13,'353d4b');box(27,14,31,14,'353d4b');
   }
  }
  s.layer(0).name='Revision02 cave floor and block separator';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
  app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('REVISION02_CLEAN '+name);
 });console.log('REVISION02_CLEANUP_PASS');
}catch(e){console.log('REVISION02_CLEANUP_FAIL '+e);}app.command.Exit();
