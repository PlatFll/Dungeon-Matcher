var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DungeonFloorDepth/Revision03/";
var specs=[['Floor_ContactSmall',32],['Floor_ContactLarge',40]];
for(var n=0;n<specs.length;n++){
 var name=specs[n][0],w=specs[n][1];app.open(root+'Canvas/'+name+'.png');
 var spr=app.activeSprite;if(spr.filename.replace(/\\/g,'/').indexOf('Canvas/'+name+'.png')<0)throw new Error('Wrong canvas');
 var im=spr.layer(0).cel(0).image,pc=app.pixelColor;
 var low=pc.rgba(78,61,108,255),dark=pc.rgba(56,45,85,255);
 var inset=[8,4,1,0,3,7];
 for(var y=0;y<6;y++)for(var x=inset[y];x<w-inset[y];x++)
  im.putPixel(x,y,(y===2||y===3)&&x>=6&&x<w-6?dark:low);
 spr.layer(0).name='Ground contact - existing Dungeon floor shadow colors';
 spr.commit();spr.saveAs(root+'Native/'+name+'.ase',true);
 spr.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
 app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();
}
console.log('CONTACT_NATIVE_ROUNDTRIP_COMPLETE');app.command.Exit();
