// Run inside LibreSprite. Enter 80, 120, 120 (and 40 for attacks) in the
// successive native Frame Properties dialogs. The frame target is explicit.
var names=['Farmer_AutoAttack','PanVillager_AutoAttack','Rattlebones_Ability','Bardley_Ability'];
for(var n=0;n<names.length;n++){
 app.open('C:/UnityProjects/Dungeon Matcher/ArtSource/CombatActions/Review/'+names[n]+'.aseprite');
 var targets=n<2?['all','3','5','4']:['all','3','5'];
 for(var i=0;i<targets.length;i++){
  app.command.clearParameters();app.command.setParameter('frame',targets[i]);
  app.command.FrameProperties();
 }
 app.activeSprite.save();
}
app.command.clearParameters();app.command.GotoFirstFrame();
