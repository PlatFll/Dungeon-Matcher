// LibreSprite script, input: 34x34 reference crop. Reuse its clean corner and
// mirror the frame; remove potion, quantity and the damaged lower-right edge.
var image=app.activeImage;
var pixels=[];
for(var y=0;y<image.height;y++)for(var x=0;x<image.width;x++)pixels.push(image.getPixel(x,y));
var width=image.width;
image.clear(0);
for(var y=0;y<32;y++)for(var x=0;x<32;x++){
    var a=x<16?x:31-x,b=y<16?y:31-y;
    var color=(a<5||b<5)?pixels[b*width+a]:app.pixelColor.rgba(17,15,25,255);
    image.putPixel(x,y,color);
}
app.activeSprite.commit();
// Build-ConsumableArt.ps1 performs the final native-size crop with LibreSprite CLI.
app.activeSprite.saveAs('ArtSource/Consumables/Slot.ase');
app.activeSprite.saveAs('Assets/_Game/Resources/UI/Consumables/Slot.png',true);
app.command.Exit();
