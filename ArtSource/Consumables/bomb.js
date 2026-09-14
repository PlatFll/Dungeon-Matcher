// LibreSprite script. Separate shop Bomb artwork; no labels baked into pixels.
var image = app.activeImage;
image.clear(0);
var colors={o:[22,14,31,255],d:[36,32,43,255],b:[58,54,68,255],h:[100,98,119,255],w:[217,213,227,255],g:[244,157,36,255],y:[255,239,148,255],r:[245,78,28,255]};
var rows=[
'          r    ', '        r y r  ', '         yyr   ', '        gr y   ',
'       gg      ', '     oogoo     ', '   oobbbbbboo  ', '  obhhbbbbdddo ',
' obhwwhbbbddddo', ' obhwhbbbbddddo', ' obhhbbbbbddddo', ' obbbbbbbdddddo',
' obbbbbbddddddo', '  obbbbddddddo ', '  obbbdddddddo ', '   odddddddoo  ', '     ooooo     '
];
for(var y=0;y<rows.length;y++)for(var x=0;x<rows[y].length;x++){
    var c=colors[rows[y][x]];if(c)image.putPixel(x+4,y+3,app.pixelColor.rgba(c[0],c[1],c[2],c[3]));
}
app.activeSprite.commit();
// Build-ConsumableArt.ps1 performs the final native-size crop with LibreSprite CLI.
app.activeSprite.saveAs('ArtSource/Consumables/Bomb.ase');
app.activeSprite.saveAs('Assets/_Game/Resources/UI/Consumables/Bomb.png',true);
app.command.Exit();
