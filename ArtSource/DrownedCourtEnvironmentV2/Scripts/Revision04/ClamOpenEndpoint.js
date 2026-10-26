var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/',pc=app.pixelColor;
function rgb(s){return pc.rgba(parseInt(s.substr(0,2),16),parseInt(s.substr(2,2),16),parseInt(s.substr(4,2),16),255);}
var pal=['3aa3ae','258999','176c7d','0a0d11','8f9e8d','66516f','062432','65c1be','083d50','cfabbc','352b50','9d7b96','d5d6b0','f2e9ce'].map(rgb);
function nearest(c){var best=pal[0],score=1e9;for(var i=0;i<pal.length;i++){var q=pal[i],r=pc.rgbaR(c)-pc.rgbaR(q),g=pc.rgbaG(c)-pc.rgbaG(q),b=pc.rgbaB(c)-pc.rgbaB(q),v=2*r*r+3*g*g+2*b*b;if(v<score){score=v;best=q;}}return best;}
try{
app.open(root+'Exports/Revision04/ClamClosed.png');var im=app.activeImage,base=[];for(var y=0;y<64;y++)for(var x=0;x<64;x++)base.push(im.getPixel(x,y));app.activeDocument.close();
app.open(root+'RawPixelLab/Revision04/Animation/G/Sprite.png');im=app.activeImage;var source=[];
for(var y=0;y<64;y++)for(var x=0;x<64;x++)source.push(im.getPixel(x,y));
for(var y=0;y<64;y++)for(var x=0;x<64;x++){var c=source[y*64+x];if(pc.rgbaA(c)){c=nearest(c);if(pc.rgbaR(c)>=180&&pc.rgbaG(c)>=180&&pc.rgbaB(c)>=170)c=rgb('66516f');}im.putPixel(x,y,y>=47?base[y*64+x]:c);}
// A recessed rear hinge connects the two valves. Pearl belongs in the lower bowl.
for(var y=34;y<=43;y++)for(var x=28;x<=35;x++)if(!pc.rgbaA(im.getPixel(x,y)))im.putPixel(x,y,rgb('352b50'));
for(var y=39;y<=44;y++)for(var x=29;x<=34;x++)if(!((y===39||y===44)&&(x===29||x===34)))im.putPixel(x,y,rgb('d5d6b0'));
for(var y=40;y<=42;y++)for(var x=30;x<=32;x++)im.putPixel(x,y,rgb('f2e9ce'));
for(var x=30;x<=33;x++)im.putPixel(x,44,rgb('8f9e8d'));
var s=app.activeSprite;s.layer(0).name='PixelLab open endpoint - fixed bowl, connected hinge, seated pearl';s.commit();s.saveAs(root+'Native/Revision04/ClamOpenEndpoint.ase',false);s.saveAs(root+'RawPixelLab/Revision04/ClamOpenEndpoint.png',true);console.log('CLAM_OPEN_ENDPOINT_PASS');app.activeDocument.close();
}catch(e){console.log('CLAM_OPEN_ENDPOINT_FAILED '+e);}app.command.Exit();
