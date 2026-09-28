// Reopen native transactions before converting background-layer flags.
var ROOT='__ART_ROOT__Environment/';
function finish(name){app.open(ROOT+name+'.aseprite');app.command.LayerFromBackground();var s=app.activeSprite;s.commit();s.save();s.saveAs(ROOT+name+'.png',false);app.activeDocument.close();}
var names=['Chain','Banner','Torch','Crate','CrateStack','Barrel','Pot','SkullBones','Rocks','Lock','SkullRelief','ArchFinial','Bones','ArchCrown','IronGrille','ArchSupport','PillarCapital','PillarShaft','PillarBase'];
for(var i=0;i<names.length;i++)finish('Props/'+names[i]);
finish('DungeonMaster');
for(var y=0;y<6;y++)for(var x=0;x<8;x++)finish('Baked/Cell'+String.fromCharCode(65+y)+String.fromCharCode(65+x));
app.command.Exit();
