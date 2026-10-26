var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/',pc=app.pixelColor,im;try{
app.open(root+'RawPixelLab/Revision04/SelectedClam.png');var inputSprite=app.activeSprite;var source=app.activeImage;var pixels=[];var bottom=0;
for(var y=0;y<64;y++)for(var x=0;x<64;x++){var c=source.getPixel(x,y);if(pc.rgbaA(c)){pixels.push([x,y,c]);bottom=Math.max(bottom,y);}}app.activeDocument.close();
app.open(root+'Native/Revision04/Work-ClamClosed.png');var s=app.activeSprite;im=app.activeImage;var shift=59-bottom;
for(var i=0;i<pixels.length;i++)im.putPixel(pixels[i][0],pixels[i][1]+shift,pixels[i][2]);
var s=app.activeSprite;s.layer(0).name='PixelLab candidate 5; exact native translation only';s.commit();s.saveAs(root+'Native/Revision04/ClamClosed.ase',true);s.saveAs(root+'Exports/Revision04/ClamClosed.png',true);app.activeDocument.close();console.log('LIFE04_SEED_PASS shiftY='+shift);
}catch(e){console.log('LIFE04_SEED_FAILED '+e);}app.command.Exit();
