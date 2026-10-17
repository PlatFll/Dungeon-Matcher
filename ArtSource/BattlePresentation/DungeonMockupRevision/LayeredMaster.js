// Both a flat review export and an actual independently editable layered ASE.
makeDocument(composite,OUTPUT+'DungeonArtOnly.aseprite','DungeonArtOnly');
var top=blank(470,298);
for(var n=0;n<layerArt.length;n++){
 var p=layerArt[n];
 if(['FarWall','ArchDoor','BattleFloor','PlayerFootMasonry','PlayerSurround','ForegroundProps'].indexOf(p.name)>=0)copy(top,p.a,p.x,p.y);
}
makeDocument(top,OUTPUT+'TopHudBackground.aseprite','TopHudBackground');
app.open(PROJECT+'Assets/_Game/Resources/UI/Consumables/Potion.png');
var master=app.activeSprite;
master.saveAs(OUTPUT+'DungeonLayered.aseprite',false);master.resize(470,836);master.commit();
app.command.BackgroundFromLayer();master.commit();app.command.LayerFromBackground();master.commit();
for(var n=0;n<layerArt.length;n++){
 if(n){app.command.DuplicateLayer();master.commit();}
 var p=layerArt[n],plane=blank(470,836);copy(plane,p.a,p.x,p.y);
 var layer=master.layer(app.activeLayerNumber);
 layer.name=p.name;layer.cel(0).image.putImageData(plane.p);master.commit();
}
master.saveAs(OUTPUT+'DungeonLayered.aseprite',true);
console.log('LAYERED_MASTER '+master.layerCount+' layers');app.activeDocument.close();
console.log('KIT_MANIFEST '+JSON.stringify({size:[470,836],modules:modules,repairs:repairs}));
console.log('KIT_COMPLETE');
