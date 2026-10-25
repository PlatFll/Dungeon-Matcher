var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/';
var pc=app.pixelColor,clear=pc.rgba(0,0,0,0);
function rgb(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
var outline=rgb('0a0d11'),stone=['081e2b','0f3141','164759','205b69','286b78','367d86','518f94'].map(rgb),shell=['968e71','ded4af'].map(rgb);
function rect(im,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)im.putPixel(xx,yy,c);}
function save(n){var s=app.activeSprite;s.commit();s.saveAs(root+'Native/Stage07/'+n+'.ase',true);s.saveAs(root+'Exports/Stage07/'+n+'.png',true);console.log('SAVED '+n+' '+s.width+'x'+s.height);app.activeDocument.close();}
function native(n,w,h,fn){app.open(root+'Native/Stage07/Work-'+n+'.png');var s=app.activeSprite;if(!s||s.width!==w||s.height!==h||s.filename.indexOf('Work-'+n+'.png')<0)throw Error('Wrong working canvas '+n);fn(app.activeImage);s.layer(0).name='Native reef transition';save(n);}
try{
 ['LeftReef','RightReef'].forEach(function(n){app.open(root+'RawPixelLab/Stage07/'+n+'.png');var s=app.activeSprite,im=app.activeImage;if(s.width!==80||s.height!==96)throw Error('Unexpected reef size');
  for(var y=0;y<96;y++)for(var x=0;x<80;x++){var c=im.getPixel(x,y);if(!pc.rgbaA(c)){im.putPixel(x,y,clear);continue;}
   // Keep creature/relic and marine material colors. Suppress warm salt speckles only on column cap/shaft.
   var masonry=n==='LeftReef'?(x>=5&&x<=28&&y<42):(x>=39&&x<=64&&y<36);
   if(masonry&&(c===shell[0]||c===shell[1]||c===rgb('6d8450')))im.putPixel(x,y,stone[5]);
  }
  s.layer(0).name='PixelLab reef - native cap/material cleanup';save(n);
 });
 // Extension preserves the accepted shaft's exact native profile; its height is repeated, never stretched.
 app.open(root+'Exports/Stage06/ColumnShaftA.png');app.activeSprite.layer(0).name='Accepted column profile - unchanged native copy';save('RuinConnector');
 native('Seafloor',32,6,function(im){rect(im,0,0,32,6,stone[0]);rect(im,0,1,32,1,stone[1]);rect(im,0,3,32,3,stone[1]);rect(im,3,3,8,1,stone[2]);rect(im,17,4,9,1,stone[2]);});
 native('SeafloorShort',8,6,function(im){rect(im,0,0,8,6,stone[0]);rect(im,0,1,8,1,stone[1]);rect(im,0,3,8,3,stone[1]);});
 native('RubbleA',16,8,function(im){rect(im,2,6,12,1,stone[0]);rect(im,3,3,9,3,stone[1]);rect(im,5,2,5,1,stone[1]);rect(im,4,3,5,2,stone[3]);rect(im,5,2,4,1,stone[4]);rect(im,10,4,3,2,stone[2]);});
 native('RubbleB',12,6,function(im){rect(im,1,4,10,1,stone[0]);rect(im,2,2,8,2,stone[1]);rect(im,3,1,4,1,stone[2]);rect(im,3,2,4,1,stone[3]);rect(im,7,3,3,1,stone[2]);});
 console.log('STAGE07_NATIVE_COMPLETE');
}catch(e){console.log('STAGE07_NATIVE_FAILED '+e);}
app.command.Exit();
