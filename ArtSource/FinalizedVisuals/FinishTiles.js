// Exact native-pixel finishing of PixelLab tiles. Run with LibreSprite --script.
var ROOT='__ART_ROOT__';
var HEX=['090618','161227','1f1934','26203f','312c4d','42395c','544766','6f5a77','504153','5f4a52','6b5056'];
var palette=HEX.map(function(h){return [parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255];});
function read(i){app.open(ROOT+'Candidates/Tiles_01/Tile'+String.fromCharCode(65+i)+'.png');var p=new Uint8Array(app.activeSprite.layer(0).cel(0).image.getImageData());app.activeDocument.close();return p;}
function copy(p,q,x,y,X,Y){for(var k=0;k<4;k++)p[(y*64+x)*4+k]=q[(Y*64+X)*4+k];}
function finish(i,group,canonical){
 var p=read(i),factor=group==='wall'?.78:group==='floor'?.9:1;
 var end=group==='wall'?6:group==='warm'?11:group==='floor'?8:4;
 for(var j=0;j<p.length;j+=4){var best=0,dist=1e9;for(var c=0;c<end;c++){var d=0;for(var k=0;k<3;k++)d+=Math.pow(p[j+k]*factor-palette[c][k],2);if(d<dist){dist=d;best=c;}}for(var k=0;k<4;k++)p[j+k]=palette[best][k];}
 // Preserve a common brick course for the mismatched D and warm candidates.
 // Transfer their broad material patches only onto the canonical stone faces.
 if(canonical&&(i===3||group==='warm'))for(var y=0;y<64;y++)for(var x=0;x<64;x++){
  var j=(y*64+x)*4,face=canonical[j]===49&&canonical[j+1]===44;
  var accent=group==='warm'?(p[j]>70?8:4):(p[j]<45?3:4);
  if(face){for(var k=0;k<4;k++)p[j+k]=palette[accent][k];}else copy(p,canonical,x,y,x,y);
 }
 // Shared seam strips retain native masonry clusters, without resampling.
 if(canonical)for(var y=0;y<64;y++)for(var x=0;x<64;x++)if(x<4||x>59||(group!=='floor'&&(y<2||y>61)))copy(p,canonical,x,y,x,y);
 // Anchor the walkable plane / vertical ledge boundary at row 48 exactly.
 if(group==='floor')for(var x=0;x<64;x++)for(var k=0;k<4;k++)p[(48*64+x)*4+k]=palette[2][k];
 // Exact outer pixels match on every intended adjacent edge.
 for(var y=0;y<64;y++)copy(p,p,63,y,0,y);
 if(group!=='floor')for(var x=0;x<64;x++)copy(p,p,x,63,x,0);
 return p;
}
function save(p,name){
 app.open(ROOT+'Candidates/Tiles_01/TileA.png');var s=app.activeSprite;
 s.saveAs(ROOT+'Environment/Tiles/'+name+'.aseprite',false);
 s.layer(0).cel(0).image.putImageData(p);s.layer(0).name=name;
 s.palette.length=palette.length;for(var i=0;i<palette.length;i++)s.palette.set(i,app.pixelColor.rgba(palette[i][0],palette[i][1],palette[i][2],255));
 s.commit();s.save();s.saveAs(ROOT+'Environment/Tiles/'+name+'.png',false);app.activeDocument.close();
}
var wall=finish(0,'wall'),floor=finish(4,'floor'),foundation=finish(9,'foundation');
for(var i=0;i<4;i++)save(finish(i,'wall',wall),'Wall'+String.fromCharCode(65+i));
for(var i=0;i<4;i++)save(finish(i+4,'floor',floor),'Floor'+String.fromCharCode(65+i));
for(var i=0;i<3;i++)save(finish(i+8,'foundation',foundation),'Foundation'+String.fromCharCode(65+i));
for(var i=0;i<2;i++)save(finish(i+11,'warm',wall),'WallWarm'+String.fromCharCode(65+i));
app.command.Exit();
