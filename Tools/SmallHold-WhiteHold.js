// Run in LibreSprite after opening the original ident. The held final pose
// uses the existing pixel silhouette and wordmark on a white background.
// API reference: https://github.com/LibreSprite/LibreSprite/blob/master/SCRIPTING.md
var document = app.open('ArtSource/SmallHoldGames/SmallHold_Games_Ident.aseprite');
var sprite = document ? document.sprite : app.activeSprite;
if (!sprite || Number(sprite.width) !== 480 || Number(sprite.height) !== 270) throw new Error('Expected the SmallHold ident');
var changed = 0;
var finalBackground = sprite.layer(0).cel(29).image.getImageData();
var alreadyWhite = finalBackground[0] > 200 && finalBackground[1] > 200 && finalBackground[2] > 200;
for (var layerIndex = 0; layerIndex < sprite.layerCount; layerIndex++) {
    var layer = sprite.layer(layerIndex);
    if (alreadyWhite || !layer.isImage) continue;
    for (var frame = 29; frame < 30; frame++) {
        var cel = layer.cel(frame);
        if (!cel || cel.frame !== 29) continue;
        var img = cel.image;
        var pixels = img.getImageData();
        for (var pixel = 0; pixel < pixels.length; pixel += 4) {
            var ink = pixels[pixel] > 200 && pixels[pixel+1] > 200 && pixels[pixel+2] > 200;
            pixels[pixel] = ink ? 17 : 255;
            pixels[pixel+1] = ink ? 29 : 255;
            pixels[pixel+2] = ink ? 48 : 255;
        }
        img.putImageData(pixels);
        changed++;
    }
}
console.log('SmallHold final-pose cels changed: ' + changed);
sprite.saveAs('ArtSource/SmallHoldGames/SmallHold_Games_Ident.aseprite', true);
