var ROOT='__ART_ROOT__UI/';
var names=['PanelInset','PanelTall','WavePlaque','HealthFrame','HealthStart','HealthMiddle','HealthEnd','HealthFill','HealthTrack','EnergyFrame','EnergyFill','EnergyTrack','DungeonMatcherLogo','SplitStoryGem'];
var families=['ButtonLarge','ButtonSmall','Settings'],states=['Normal','Highlighted','Pressed','Disabled'],ranks=['Player','Normal','Special','Miniboss','Boss'];
for(var i=0;i<families.length;i++)for(var j=0;j<states.length;j++)names.push(families[i]+states[j]);
for(var i=0;i<ranks.length;i++){names.push(ranks[i]+'Badge');for(var j=0;j<3;j++)names.push(ranks[i]+'Health'+['Start','Middle','End'][j]);}
for(var i=0;i<names.length;i++){app.open(ROOT+names[i]+'.aseprite');app.command.LayerFromBackground();var s=app.activeSprite;s.commit();s.save();s.saveAs(ROOT+names[i]+'.png',false);app.activeDocument.close();}
app.command.Exit();
