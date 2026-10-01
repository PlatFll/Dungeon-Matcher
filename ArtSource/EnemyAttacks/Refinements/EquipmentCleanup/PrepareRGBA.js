// Convert the exact PixelLab edits from indexed GIF to RGBA in LibreSprite.
// Keep frame pixels, dimensions and timing; finishing uses RGBA image buffers.
var ROOT='__ROOT__';
['SpearKnight','RoyalLancer','ShieldKnight'].forEach(function(name){
 app.open(ROOT+'ArtSource/EnemyAttacks/Refinements/EquipmentCleanup/'+name+'/Edited.gif');
 app.command.setParameter('format','rgb');app.command.ChangePixelFormat();app.command.clearParameters();
 var s=app.activeSprite;
 if(s.colorMode!==ColorMode.RGB)throw Error('RGBA conversion failed: '+name);
 s.layer(0).name='Equipment corrected';s.commit();
 s.saveAs(ROOT+'.utmp/'+name+'EquipmentRGBA.aseprite',false);app.activeDocument.close();
});app.command.Exit();
