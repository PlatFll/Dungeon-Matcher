// Finish PixelLab native candidates in LibreSprite, preserving their pixel grids.
var ROOT='__ART_ROOT__';
var hex=['060709','151423','241f3a','3a2d51','4e3d6c','67537e','846a95','aa8bb9','d3b9e0','ffe0ff','591e36','982541','d63850','ef666d','ffb6b9','6e4a2f','ad793d','ddb75d','ffe3a0','39414b','69737a','a9b2a8','e0e2cb'];
var palette=hex.map(function(h){return [parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255];});
function nearest(r,g,b){var best=0,dist=1e9;for(var j=0;j<palette.length;j++){var c=palette[j],d=(r-c[0])*(r-c[0])+(g-c[1])*(g-c[1])+(b-c[2])*(b-c[2]);if(d<dist){dist=d;best=j;}}return palette[best];}
function finish(input,name,state){
 app.open(ROOT+'Candidates/'+input+'.png');var s=app.activeSprite;var p=new Uint8Array(s.layer(0).cel(0).image.getImageData());
 for(var i=0;i<p.length;i+=4){if(p[i+3]<128){p[i]=p[i+1]=p[i+2]=p[i+3]=0;continue;}
  var factor=state==='Highlighted'?1.15:state==='Pressed'?.72:state==='Disabled'?.52:1;
  var c=nearest(p[i]*factor,p[i+1]*factor,p[i+2]*factor);for(var k=0;k<4;k++)p[i+k]=c[k];
 }
 s.saveAs(ROOT+'UI/'+name+'.aseprite',false);app.command.BackgroundFromLayer();s.commit();s.layer(0).cel(0).image.putImageData(p);s.layer(0).name=name;s.commit();s.save();app.activeDocument.close();
}
finish('PowerupTileFixed','Slot','Normal');
['Highlighted','Pressed','Disabled'].forEach(function(state){finish('PowerupTileFixed','Slot'+state,state);});
finish('Potion','Potion','Normal');finish('Bomb','Bomb','Normal');app.command.Exit();
