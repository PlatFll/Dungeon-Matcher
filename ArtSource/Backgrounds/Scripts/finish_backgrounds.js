// Run from the native Scripts menu after paint_backgrounds.js, NOT at CLI startup.
// Reopening after the authoring transaction allows the background-layer flag
// to be cleared before exporting reusable transparent props.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/Backgrounds/Modules/';
var names=['BannerSkull','BannerSwords','BarredGate','BoneShrine','Candles','Chain','CrateStack','Skulls','Torch'];
for(var i=0;i<names.length;i++){
 app.open(ROOT+names[i]+'.aseprite');
 app.command.LayerFromBackground();
 app.activeSprite.commit();
 app.activeSprite.save();
 app.activeDocument.close();
}
