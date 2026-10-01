// LibreSprite: articulate the user-selected native design without redrawing it.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/GideonGlass/';
var HEX=['0A0D11','755335','B18A4B','DAC080','25212D','443949','685565','54283D','894254','24425C','397BAB','A9E0FF','B7A393','FDF5E5','5B3627','8E7868'];
var colors=HEX.map(function(h){return [parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255];});
app.open(ROOT+'Gideon_Ready.aseprite');
var original=new Uint8Array(app.activeSprite.layer(0).cel(0).image.getImageData());
app.activeDocument.close();
function pixel(data,x,y,c){for(var k=0;k<4;k++)data[(y*64+x)*4+k]=colors[c][k];}
function pose(headDip,bodyDip,tick){
 var result=new Uint8Array(16384);
 // Compress the upper coat toward fixed hips. The planted hand and cane are fixed.
 for(var y=38;y<64;y++)for(var x=0;x<64;x++){
  var yy=y;
  if(y<58 && !(x>=42 && y>=44))yy=y+Math.round(bodyDip*(58-y)/20);
  var i=(y*64+x)*4;if(original[i+3])for(var k=0;k<4;k++)result[(yy*64+x)*4+k]=original[i+k];
 }
 // Head, lens, hat and feather stay rigidly connected.
 for(var y=0;y<=37;y++)for(var x=0;x<64;x++){
  var i=(y*64+x)*4;if(original[i+3])for(var k=0;k<4;k++)result[((y+headDip)*64+x)*4+k]=original[i+k];
 }
 if(tick){pixel(result,36,29+headDip,9);pixel(result,37,29+headDip,9);pixel(result,37,30+headDip,10);}
 return result;
}
function write(name,frames){
 app.open('C:/UnityProjects/Dungeon Matcher/ArtSource/CombatIdles/Originals/FarmerFluidAnim2.aseprite');
 var s=app.activeSprite;s.saveAs(ROOT+name+'.aseprite',false);
 while(s.layer(0).celCount>frames.length){app.command.GotoLastFrame();app.command.RemoveFrame();}
 for(var f=0;f<frames.length;f++)s.layer(0).cel(f).image.putImageData(frames[f]);
 s.layer(0).name=name;s.palette.length=HEX.length+1;s.palette.set(0,app.pixelColor.rgba(0,0,0,0));
 for(var i=0;i<colors.length;i++){var c=colors[i];s.palette.set(i+1,app.pixelColor.rgba(c[0],c[1],c[2],255));}
 s.commit();s.save();app.activeDocument.close();
}
var headDip=[0,1,1,1,2,2,1,1,0],bodyDip=[0,1,1,1,1,1,1,0,0],frames=[];
for(var f=0;f<9;f++)frames.push(pose(headDip[f],bodyDip[f],f===5));
write('Gideon_Idle',frames);
app.command.Exit();
