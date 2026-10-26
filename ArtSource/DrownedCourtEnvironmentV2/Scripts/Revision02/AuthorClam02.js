var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/',pc=app.pixelColor,buf=[],im,w=37,h=38,clear=pc.rgba(0,0,0,0);
function rgb(s){return pc.rgba(parseInt(s.substr(0,2),16),parseInt(s.substr(2,2),16),parseInt(s.substr(4,2),16),255);}
function put(x,y,c){if(x>=0&&x<w&&y>=0&&y<h)im.putPixel(x,y,c);}
function line(x,y,X,Y,c){var dx=Math.abs(X-x),sx=x<X?1:-1,dy=-Math.abs(Y-y),sy=y<Y?1:-1,e=dx+dy;while(true){put(x,y,c);if(x===X&&y===Y)break;var ee=2*e;if(ee>=dy){e+=dy;x+=sx;}if(ee<=dx){e+=dx;y+=sy;}}}
try{
 app.open(root+'RawPixelLab/Revision02/ClamSeed.png');im=app.activeImage;for(var y=0;y<64;y++)for(var x=0;x<64;x++)buf.push(im.getPixel(x,y));app.activeDocument.close();
 for(var f=0;f<5;f++){
  app.open(root+'Native/Revision02/Work-Clam'+f+'-canvas.png');im=app.activeImage;
  // Fixed lower shell and stone foot; hinge elevation changes only the upper lid.
  for(var y=35;y<52;y++)for(var x=14;x<47;x++){var c=buf[y*64+x];if(pc.rgbaA(c)){if(f===0&&c===rgb('f2e9ce'))c=rgb('66516f');put(x-12,y-16,c);}}
  var q=[0.25,0.4,0.6,0.8,1][f];
  for(var y=18;y<35;y++)for(var x=14;x<47;x++){var c=buf[y*64+x];if(pc.rgbaA(c))put(x-12,2+Math.round(16+(y-18-16)*q),c);}
  // Hand-authored closed lip and ribs keep the compressed pose readable and opaque.
  if(f===0){
   for(var x=5;x<33;x++){put(x,17,rgb('0d5368'));put(x,18,rgb('0a0d11'));}
   line(5,16,10,14,rgb('258999'));line(10,14,25,14,rgb('3aa3ae'));line(25,14,32,16,rgb('258999'));
   for(var x=10;x<30;x+=5)line(x,15,x+2,17,rgb('65c1be'));
  }
  // Connect the generated shell to its ground foot; no floating pedestal pixels.
  for(var y=28;y<32;y++)for(var x=14;x<20;x++)if(!pc.rgbaA(im.getPixel(x,y)))put(x,y,rgb('083d50'));
  app.activeSprite.layer(0).name='PixelLab clam - native hinge pose '+f;app.activeSprite.commit();app.activeSprite.saveAs(root+'Native/Revision02/Clam'+f+'.ase',true);app.activeSprite.saveAs(root+'Exports/Revision02/Clam'+f+'.png',true);app.activeDocument.close();
 }
 console.log('CLAM_NATIVE_POSES_COMPLETE');
}catch(e){console.log('CLAM_NATIVE_FAILED '+e);}app.command.Exit();
