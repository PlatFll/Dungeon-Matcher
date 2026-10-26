var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/';
var pc=app.pixelColor,clear=pc.rgba(0,0,0,0);
function rgb(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
var stone=['081e2b','0f3141','164759','205b69','286b78','367d86','518f94'].map(rgb);
function rect(im,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)im.putPixel(xx,yy,c);}
function save(n){var s=app.activeSprite;s.commit();s.saveAs(root+'Native/Stage08/'+n+'.ase',true);s.saveAs(root+'Exports/Stage08/'+n+'.png',true);console.log('SAVED '+n+' '+s.width+'x'+s.height);app.activeDocument.close();}
function native(n,w,h,fn){app.open(root+'Native/Stage08/Work-'+n+'.png');var s=app.activeSprite;if(s.width!==w||s.height!==h||s.filename.indexOf('Work-'+n+'.png')<0)throw Error('Wrong working canvas');fn(app.activeImage);s.layer(0).name='Native marine ornament';save(n);}
try{
 app.open(root+'RawPixelLab/Stage08/SpongePocketSeed.png');var s=app.activeSprite,im=app.activeImage,buf=[];if(s.width!==64||s.height!==64)throw Error('Wrong seed size');
 for(var y=0;y<64;y++)for(var x=0;x<64;x++){var c=im.getPixel(x,y);if(!pc.rgbaA(c))c=clear;else if(c===stone[6])c=stone[5];im.putPixel(x,y,c);buf.push(c);}
 s.layer(0).name='PixelLab pocket - quieter stone highlight';save('SpongePocketSeed');
 native('SpongePocket',28,33,function(im){for(var y=0;y<29;y++)for(var x=0;x<24;x++){var c=buf[(y+21)*64+x+20];im.putPixel(x+2,y+2,c);}});
 native('TridentShard',18,26,function(im){
  // Asymmetric broken royal stone emblem, anchored in one low rubble foot.
  rect(im,7,2,3,17,stone[0]);rect(im,3,4,3,7,stone[0]);rect(im,11,5,3,6,stone[0]);rect(im,3,9,11,3,stone[0]);
  rect(im,8,3,1,14,stone[4]);rect(im,4,5,1,5,stone[3]);rect(im,12,6,1,4,stone[3]);rect(im,4,10,9,1,stone[3]);
  rect(im,8,3,1,3,stone[5]);rect(im,4,5,1,2,stone[5]);rect(im,11,10,2,1,stone[2]);rect(im,8,14,1,4,stone[2]);
  rect(im,2,22,14,2,stone[0]);rect(im,3,19,12,3,stone[1]);rect(im,5,17,7,3,stone[1]);rect(im,6,17,4,1,stone[3]);rect(im,4,19,6,2,stone[2]);rect(im,10,20,4,2,stone[2]);
 });
 console.log('STAGE08_NATIVE_COMPLETE');
}catch(e){console.log('STAGE08_NATIVE_FAILED '+e);}
app.command.Exit();
