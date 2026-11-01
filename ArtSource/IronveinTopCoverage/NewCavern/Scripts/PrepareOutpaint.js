var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/IronveinTopCoverage/NewCavern/",pc=app.pixelColor;
app.open(root+'Inputs/ApprovedCavern.png');var s=app.activeSprite,im=s.layer(0).cel(0).image,original=[];
for(var y=0;y<160;y++)for(var x=0;x<240;x++)original.push(im.getPixel(x,y));app.activeDocument.close();
app.open(root+'Inputs/Blank480x352.png');s=app.activeSprite;im=s.layer(0).cel(0).image;
for(var y=0;y<352;y++)for(var x=0;x<480;x++)im.putPixel(x,y,x>=180&&x<420&&y>=192?original[(y-192)*240+x-180]:pc.rgba(16,19,31,255));
s.layer(0).name='Approved cave, exact native placement';s.commit();s.saveAs(root+'Native/OutpaintInput.ase',true);s.saveAs(root+'Inputs/OutpaintInput.png',true);app.activeDocument.close();
app.open(root+'Inputs/Blank480x352.png');s=app.activeSprite;im=s.layer(0).cel(0).image;
for(var y=0;y<352;y++)for(var x=0;x<480;x++)im.putPixel(x,y,x>=180&&x<420&&y>=192?pc.rgba(0,0,0,255):pc.rgba(255,255,255,255));
s.layer(0).name='White generate; black preserve original';s.commit();s.saveAs(root+'Inputs/OutpaintMask.png',true);app.activeDocument.close();console.log('OUTPAINT_INPUT_COMPLETE');app.command.Exit();