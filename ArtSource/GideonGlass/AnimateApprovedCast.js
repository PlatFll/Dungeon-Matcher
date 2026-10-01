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

function rect(a,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)pixel(a,xx,yy,c);}
function castPose(dip,focus,flash,steam){
 var a=pose(dip,dip?1:0,false);
 if(focus){
  rect(a,35,29+dip,4,4,9);
  if(focus===1){rect(a,36,30+dip,2,2,10);pixel(a,36,30+dip,11);}
  if(focus===2){rect(a,36,31+dip,2,1,10);}
  if(focus===3){rect(a,35,30+dip,2,1,1);rect(a,37,31+dip,2,1,1);rect(a,36,31+dip,1,1,0);}
 }
 if(flash){rect(a,35,29+dip,4,4,13);rect(a,33,31+dip,8,1,13);rect(a,37,27+dip,1,8,13);}
 if(steam===1){rect(a,19,34,2,2,12);pixel(a,19,34,13);}
 if(steam===2){rect(a,17,32,3,2,12);pixel(a,17,32,13);}
 if(steam===3){rect(a,15,30,2,1,12);}
 return a;
}
write('Gideon_Cast',[castPose(0,0,0,0),castPose(1,0,0,0),castPose(1,1,0,0),castPose(1,2,0,0),castPose(1,3,0,0),castPose(1,0,1,0),castPose(1,1,0,1),castPose(1,1,0,2),castPose(1,1,0,3),castPose(1,1,0,0)]);
write('Gideon_Hold',[castPose(1,1,0,0)]);
write('Gideon_Recovery',[castPose(1,1,0,0),castPose(2,2,0,0),castPose(1,0,0,0),castPose(0,0,0,0)]);
app.command.Exit();
