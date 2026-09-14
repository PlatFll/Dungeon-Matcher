// LibreSprite script. Redraws the reference flask without its baked quantity.
var image = app.activeImage;
image.clear(0);
var colors = {
    o: [25,16,33,255], c:[255,171,41,255], g:[227,93,22,255],
    h:[212,251,255,255], b:[32,161,229,255], d:[36,81,158,255],
    r:[244,29,70,255], R:[190,12,42,255], s:[135,10,39,255]
};
var rows = [
'    occco    ', '    ocgco    ', '    ohbho    ', '    obbdo    ',
'    obbdo    ', '    obbdo    ', '   ohbbbdo   ', '  ohhbbbbdo  ',
' ohhrrrrrbdo ', ' ohrrrrrrbdo ', ' obrrrrrrrdo ', ' obrrrrRRRdo ',
' obrrrRRRRdo ', '  odRRRRsdo  ', '  oddRRssdo  ', '   odddddo   ', '    ooooo    '
];
for(var y=0;y<rows.length;y++)for(var x=0;x<rows[y].length;x++){
    var c=colors[rows[y][x]];if(c)image.putPixel(x+5,y+3,app.pixelColor.rgba(c[0],c[1],c[2],c[3]));
}
app.activeSprite.commit();
// Build-ConsumableArt.ps1 performs the final native-size crop with LibreSprite CLI.
app.activeSprite.saveAs('ArtSource/Consumables/Potion.ase');
app.activeSprite.saveAs('Assets/_Game/Resources/UI/Consumables/Potion.png',true);
app.command.Exit();
