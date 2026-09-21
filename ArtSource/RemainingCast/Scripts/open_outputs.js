// Concatenate cast_spec.js before running inside LibreSprite.
// Opens finished sources without closing or replacing the user's original tabs.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/RemainingCast/';
for(var n=0;n<CAST.length;n++)app.open(ROOT+'Idles/'+CAST[n].name+'_Idle.aseprite');
app.open(ROOT+'Idles/RoyalArbalist_Idle.aseprite');
app.command.GotoFirstFrame();app.command.ScrollCenter();
