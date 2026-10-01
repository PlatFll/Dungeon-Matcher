// Execute in LibreSprite. References remain unchanged; output cels use native pixels.
var ROOT = 'C:/UnityProjects/Dungeon Matcher/ArtSource/GideonGlass/';
var HEX = ['0A0D11','755335','B18A4B','DAC080','25212D','443949','685565','54283D','894254','24425C','397BAB','A9E0FF','B7A393','FDF5E5','5B3627','8E7868'];
var palette=HEX.map(function(h){return [parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255];});
function nearest(r,g,b){var best=0,dist=1e10;for(var i=0;i<palette.length;i++){var p=palette[i],d=(r-p[0])*(r-p[0])+(g-p[1])*(g-p[1])+(b-p[2])*(b-p[2]);if(d<dist){dist=d;best=i;}}return best;}
app.open(ROOT+'References/Gideon_UserDesign.png');
var input=app.activeSprite.layer(0).cel(0).image,raw=input.getImageData(),width=input.width,height=input.height;
var output=new Uint8Array(64*64*4);
// Native reconstruction into a 64px grid. Each cell gets one deliberate palette
// entry; no interpolated color or partial alpha reaches the output document.
for(var y=0;y<64;y++)for(var x=0;x<64;x++){
 var sx=Math.floor((x+.5-32)*17.2+644),sy=Math.floor((y+.5-64)*17.2+1174);
 if(sx<0||sy<0||sx>=width||sy>=height)continue;
 var votes=[],visible=0;
 for(var v=0;v<palette.length;v++)votes[v]=0;
 for(var yy=-3;yy<=3;yy++)for(var xx=-3;xx<=3;xx++){
  var si=((sy+yy)*width+sx+xx)*4;
  if(raw[si+3]<128)continue;
  visible++;votes[nearest(raw[si],raw[si+1],raw[si+2])]++;
 }
 if(visible<25)continue;
 var selected=0;for(var v=1;v<votes.length;v++)if(votes[v]>votes[selected])selected=v;
 for(var k=0;k<4;k++)output[(y*64+x)*4+k]=palette[selected][k];
}

// Preserve the one-native-pixel wood shaft that fell between sample centers.
function pixel(x,y,index){for(var k=0;k<4;k++)output[(y*64+x)*4+k]=palette[index][k];}
for(var y=50;y<=61;y++)pixel(46,y,14);
pixel(46,62,3);
// The supplied design has one broad square lens glint, not scattered sparkles.
pixel(36,29,11);pixel(37,29,11);pixel(36,30,11);pixel(37,30,11);
app.activeDocument.close();
// Established full-size native cels provide an editable document, never source art.
app.open('C:/UnityProjects/Dungeon Matcher/ArtSource/CombatIdles/Originals/FarmerFluidAnim2.aseprite');
var sprite=app.activeSprite;
sprite.saveAs(ROOT+'Candidates/Gideon_Approved_03.aseprite',false);
while(sprite.layer(0).celCount>1){app.command.GotoLastFrame();app.command.RemoveFrame();}
var cel=sprite.layer(0).cel(0);
if(cel.image.width!==64||cel.image.height!==64)throw Error('Expected full 64px cel');
cel.image.putImageData(output);sprite.layer(0).name='Gideon ready';
sprite.palette.length=palette.length+1;sprite.palette.set(0,app.pixelColor.rgba(0,0,0,0));
for(var i=0;i<palette.length;i++)sprite.palette.set(i+1,app.pixelColor.rgba(palette[i][0],palette[i][1],palette[i][2],255));
sprite.commit();sprite.save();sprite.saveAs(ROOT+'Candidates/Gideon_Approved_03.png',true);
app.command.Exit();
