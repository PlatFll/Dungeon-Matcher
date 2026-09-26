// Reload before converting the temporary full-canvas background layer to a
// normal transparent layer. LibreSprite commits the conversion on reopen.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/Presentation/';
var names=['DungeonBackdropTile','Torch','TorchAlt','RoyalBanner','Barrel','SkullPile','DungeonMatcherLogo','MenuDungeon','Potion','Bomb'];
for(var i=0;i<names.length;i++){
 app.open(ROOT+names[i]+'.aseprite');
 app.command.LayerFromBackground();app.activeSprite.commit();
 app.activeSprite.save();app.activeDocument.close();
}
