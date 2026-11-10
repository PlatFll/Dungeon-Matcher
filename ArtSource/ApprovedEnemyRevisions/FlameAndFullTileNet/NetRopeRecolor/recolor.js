// Run inside LibreSprite on the preserved Before.png copy. Geometry is locked.
app.open('ArtSource/ApprovedEnemyRevisions/FlameAndFullTileNet/NetRopeRecolor/Before.png');
var sprite = app.activeSprite;
var image = sprite ? sprite.layer(0).cel(0).image : null;
var pixel = app.pixelColor;
if (!sprite || !image || Number(sprite.width) !== 64 || Number(sprite.height) !== 64)
    throw new Error('Expected the 64x64 Marine Net working copy.');
var filename = sprite.filename.replace(/\\/g, '/');
if (filename.indexOf('/NetRopeRecolor/Before.png') < 0)
    throw new Error('Refusing to edit any document other than the net working copy.');
var folder = filename.substring(0, filename.lastIndexOf('/') + 1);
// Exact colors sampled from the current Reef Netweaver held net:
// deep brown rope, brown rope, warm rope light, pale attached weights.
var colors = {
    '10,13,17': [10,13,17],
    '36,63,72': [74,44,28],
    '66,102,107': [122,77,46],
    '113,145,142': [122,77,46],
    '169,187,182': [176,122,67],
    '176,160,184': [184,200,190]
};
for (var y = 0; y < image.height; y++) {
    for (var x = 0; x < image.width; x++) {
        var c = image.getPixel(x, y);
        if (pixel.rgbaA(c) === 0) continue;
        if (pixel.rgbaA(c) !== 255) throw new Error('Unexpected nonbinary alpha.');
        var key = pixel.rgbaR(c)+','+pixel.rgbaG(c)+','+pixel.rgbaB(c);
        var mapped = colors[key];
        if (!mapped) throw new Error('Unexpected source color: '+key);
        image.putPixel(x,y,pixel.rgba(mapped[0],mapped[1],mapped[2],255));
    }
}
sprite.commit();
sprite.saveAs(folder+'MarineSnare.ase',true);
sprite.saveAs(folder+'MarineSnare.png',true);
app.activeDocument.close();
app.open(folder+'MarineSnare.ase');
app.activeSprite.saveAs(folder+'Roundtrip.png',true);
app.activeDocument.close();
console.log('NET_ROPE_RECOLOR_COMPLETE');
app.command.Exit();
