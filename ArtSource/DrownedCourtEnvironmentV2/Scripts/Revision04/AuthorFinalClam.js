var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/',pc=app.pixelColor;
function rgb(s){return pc.rgba(parseInt(s.substr(0,2),16),parseInt(s.substr(2,2),16),parseInt(s.substr(4,2),16),255);}
var pal=['3aa3ae','258999','176c7d','0a0d11','8f9e8d','66516f','062432','65c1be','083d50','cfabbc','352b50','9d7b96','d5d6b0','f2e9ce'].map(rgb);
function nearest(c){var best=pal[0],score=1e9;for(var i=0;i<pal.length;i++){var q=pal[i],r=pc.rgbaR(c)-pc.rgbaR(q),g=pc.rgbaG(c)-pc.rgbaG(q),b=pc.rgbaB(c)-pc.rgbaB(q),v=2*r*r+3*g*g+2*b*b;if(v<score){score=v;best=q;}}return best;}
try{
app.open(root+'Exports/Revision04/ClamClosed.png');var im=app.activeImage,base=[];for(var y=0;y<64;y++)for(var x=0;x<64;x++)base.push(im.getPixel(x,y));app.activeDocument.close();
for(var f=0;f<9;f++){
 var name='Clam'+String.fromCharCode(65+f);app.open(root+(f===0?'Exports/Revision04/ClamClosed.png':'RawPixelLab/Revision04/AnimationThird/'+String.fromCharCode(65+f)+'/Sprite.png'));im=app.activeImage;
 if(f>0){
  for(var y=0;y<64;y++)for(var x=0;x<64;x++){var c=im.getPixel(x,y);im.putPixel(x,y,y>=47?base[y*64+x]:pc.rgbaA(c)?nearest(c):pc.rgba(0,0,0,0));}
  // Remove the generated false pearl/socket on the upper valve, retaining its surrounding ribs.
  if(f>=4)for(var y=24;y<=35;y++)for(var x=26;x<=37;x++)im.putPixel(x,y,rgb('66516f'));
  if(f>=3){
   // Seat the pearl in the fixed bowl; it cannot ride upward with the lid.
   for(var y=34;y<=44;y++)for(var x=27;x<=36;x++){var c=im.getPixel(x,y);if(pc.rgbaR(c)>=180&&pc.rgbaG(c)>=180&&pc.rgbaB(c)>=170)im.putPixel(x,y,rgb('352b50'));}
   for(var y=34;y<=43;y++)for(var x=28;x<=35;x++)if(!pc.rgbaA(im.getPixel(x,y)))im.putPixel(x,y,rgb('352b50'));
   for(var y=39;y<=44;y++)for(var x=29;x<=34;x++)if(!((y===39||y===44)&&(x===29||x===34)))im.putPixel(x,y,rgb('d5d6b0'));
   for(var y=40;y<=42;y++)for(var x=30;x<=32;x++)im.putPixel(x,y,rgb('f2e9ce'));
   for(var x=30;x<=33;x++)im.putPixel(x,44,rgb('8f9e8d'));
  }
 }
 var s=app.activeSprite;s.layer(0).name='PixelLab hinge '+f+' - fixed ground, controlled palette, seated pearl';s.commit();s.saveAs(root+'Native/Revision04/'+name+'.ase',false);s.saveAs(root+'Exports/Revision04/'+name+'.png',true);app.activeDocument.close();
}
console.log('CLAM_HINGE_CLEAN_PASS');
}catch(e){console.log('CLAM_HINGE_CLEAN_FAILED '+e);}app.command.Exit();
