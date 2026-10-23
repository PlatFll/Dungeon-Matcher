var names=["Lantern_Hanging", "Mushroom_Cluster", "Rune_Plaque"];
names.forEach(function(n){var d=app.open('ArtSource/ForestHybrid/Stage04/Native/'+n+'.aseprite');d.sprite.saveAs('.utmp/forest-stage04/reopened/'+n+'.png',true);});
