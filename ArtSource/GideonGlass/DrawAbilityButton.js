// Keep the game's existing 176x64 button frame and replace its central emblem.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/GideonGlass/';
app.open(ROOT+'Gideon_LensIcon.png');
var lens=new Uint8Array(app.activeSprite.layer(0).cel(0).image.getImageData());
app.activeDocument.close();
app.open('C:/UnityProjects/Dungeon Matcher/Assets/_Game/Art/UI/Ability UI/Royal_Decree_Button_new.png');
var sprite=app.activeSprite,im=sprite.layer(0).cel(0).image;
if(im.width!==176||im.height!==64)throw Error('Expected current native button frame');
var data=new Uint8Array(im.getImageData()),original=new Uint8Array(data);
for(var y=0;y<64;y++)for(var x=59;x<=116;x++)for(var k=0;k<4;k++)data[(y*176+x)*4+k]=original[(y*176+56)*4+k];
for(var y=0;y<64;y++)for(var x=0;x<64;x++)if(lens[(y*64+x)*4+3])
 for(var k=0;k<4;k++)data[(y*176+x+56)*4+k]=lens[(y*64+x)*4+k];
// The old frame has near-opaque 253/254 alpha and a few faint fringe pixels.
// Normalize the new production copy to the project's hard pixel alpha rule.
for(var i=0;i<data.length;i+=4){data[i+3]=data[i+3]>=128?255:0;if(!data[i+3])data[i]=data[i+1]=data[i+2]=0;}
sprite.saveAs(ROOT+'Gideon_AbilityButton.aseprite',false);
im.putImageData(data);sprite.layer(0).name='ChronoShutter button';sprite.commit();sprite.save();
sprite.saveAs(ROOT+'Gideon_AbilityButton.png',true);app.command.Exit();
