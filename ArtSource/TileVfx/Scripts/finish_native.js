// Run after fitting. The native frame-properties dialogs are set to 30 ms.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/TileVfx/';
var names=['Explosion','PoisonExplosion','ShieldExplosion','HealingExplosion'];
for(var i=0;i<names.length;i++){
 app.open(ROOT+names[i]+'.aseprite');
 app.command.LayerFromBackground();app.activeSprite.commit();
 app.command.setParameter('frame','all');app.command.FrameProperties();
 app.activeSprite.save();app.command.GotoFirstFrame();
}
