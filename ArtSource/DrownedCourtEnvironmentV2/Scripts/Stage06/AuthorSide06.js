var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/';
var pc=app.pixelColor,clear=pc.rgba(0,0,0,0);
function rgb(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
var outline=rgb('0a0d11'),stone=['081e2b','0f3141','164759','205b69','286b78','367d86','518f94'].map(rgb),pink=['63334f','944a68','c5728b'].map(rgb),shell=[rgb('968e71'),rgb('ded4af')];
function rect(im,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)im.putPixel(xx,yy,c);}
function save(n){var s=app.activeSprite;s.commit();s.saveAs(root+'Native/Stage06/'+n+'.ase',true);s.saveAs(root+'Exports/Stage06/'+n+'.png',true);console.log('SAVED '+n+' '+s.width+'x'+s.height);app.activeDocument.close();}
function native(n,w,h,fn){app.open(root+'Native/Stage06/Work-'+n+'.png');var s=app.activeSprite;if(!s||s.width!==w||s.height!==h||s.filename.indexOf('Work-'+n+'.png')<0)throw Error('Wrong working canvas '+n);fn(app.activeImage);s.layer(0).name='Court side architecture - native pixels';save(n);}
var sources={};
function load(n){app.open(root+'RawPixelLab/Stage06/'+n+'.png');var s=app.activeSprite,im=app.activeImage;if(s.filename.indexOf(n+'.png')<0)throw Error('Wrong source');var a=[];
 for(var y=0;y<s.height;y++)for(var x=0;x<s.width;x++){var c=im.getPixel(x,y);if(!pc.rgbaA(c))c=clear;
  else if(n==='PillarSeed'){if(c===shell[1]||c===stone[6])c=stone[5];else if(c===stone[4])c=stone[3];else if(c===rgb('192e2e'))c=stone[1];}
  else if(n==='CoralSeed'){if(y<47&&c===shell[1])c=pink[2];else if(c===pink[2])c=pink[1];else if(c===stone[6])c=stone[5];}
  else if(c===stone[6])c=stone[5];a.push(c);im.putPixel(x,y,c);
 }sources[n]={pixels:a,w:s.width,h:s.height};s.layer(0).name='PixelLab source - controlled material ramps';save(n);}
function copy(im,n,sx,sy,w,h,dx,dy){var a=sources[n];for(var y=0;y<h;y++)for(var x=0;x<w;x++){var c=a.pixels[(sy+y)*a.w+sx+x];if(pc.rgbaA(c))im.putPixel(dx+x,dy+y,c);}}
var profile=[stone[0],stone[4],stone[3],stone[3],stone[4],stone[3],stone[2],stone[3],stone[3],stone[3],stone[3],stone[2],stone[2],stone[0]];
function shaftrow(im,y){for(var x=0;x<14;x++)im.putPixel(9+x,y,profile[x]);}
try{
load('PillarSeed');load('KelpSeed');load('CoralSeed');
native('ColumnCap',31,16,function(im){for(var y=0;y<12;y++)for(var x=0;x<27;x++){var c=sources.PillarSeed.pixels[(y+8)*64+x+18];if(!pc.rgbaA(c))continue;var dx=x+2;if(dx>=24)dx-=2;im.putPixel(dx,y+2,c);}shaftrow(im,14);shaftrow(im,15);});
native('ColumnBase',31,18,function(im){
 // Shorter foot wings clear the approved frame corners without trimming any placed sprite.
 for(var y=0;y<16;y++)for(var x=0;x<27;x++){var c=sources.PillarSeed.pixels[(y+106)*64+x+18];if(!pc.rgbaA(c))continue;var dx=x+2;if(dx<=7)dx+=3;else if(dx>=24)dx-=3;im.putPixel(dx,y,c);}shaftrow(im,0);shaftrow(im,1);
});
['A','B','C'].forEach(function(v){native('ColumnShaft'+v,31,32,function(im){
 for(var y=0;y<32;y++)shaftrow(im,y);
 if(v==='B')copy(im,'PillarSeed',25,47,14,16,9,6);
 if(v==='C')copy(im,'PillarSeed',25,83,14,12,9,9);
 rect(im,9,26,14,1,stone[0]);rect(im,10,27,12,1,stone[2]);
 shaftrow(im,0);shaftrow(im,31);
});});
native('ColumnShaftShort',31,18,function(im){for(var y=0;y<18;y++)shaftrow(im,y);rect(im,9,10,14,1,stone[0]);rect(im,10,11,12,1,stone[2]);shaftrow(im,17);});
native('ShelfRight',31,12,function(im){copy(im,'PillarSeed',18,36,27,8,2,2);});
native('ShelfLeft',31,12,function(im){copy(im,'PillarSeed',18,72,27,8,2,2);});
native('KelpTall',31,68,function(im){
 // Repose the two outer blades inward by three pixels as rigid clusters. No resampling.
 for(var y=0;y<62;y++)for(var x=0;x<33;x++){var c=sources.KelpSeed.pixels[(y+2)*64+x+14];if(!pc.rgbaA(c))continue;var dx=x;if(y+2>=22){if(x+2<=12)dx+=3;else if(x+2>=25)dx-=3;}im.putPixel(dx,y+2,c);}
 // Source touched its lower edge. Complete a dark contact contour in new padding.
 rect(im,10,64,14,1,outline);rect(im,12,65,10,1,stone[0]);
});
// Register an editable whole-body and a 2px overlapping tip/stem split for a later motion stage.
app.open(root+'Exports/Stage06/KelpTall.png');var im=app.activeImage,buf=[];for(var y=0;y<68;y++)for(var x=0;x<31;x++)buf.push(im.getPixel(x,y));sources.KelpTall={pixels:buf,w:31,h:68};app.activeDocument.close();
native('KelpTip',31,22,function(im){copy(im,'KelpTall',0,0,31,22,0,0);});
native('KelpStem',31,48,function(im){copy(im,'KelpTall',0,20,31,48,0,0);});
native('CoralColony',33,57,function(im){for(var y=0;y<53;y++)for(var x=0;x<34;x++){var c=sources.CoralSeed.pixels[(y+7)*64+x+15];if(!pc.rgbaA(c))continue;var dx=x;if(y+2<48){if(x+2<=12)dx+=3;else if(x+2>=24)dx-=3;}im.putPixel(dx,y+2,c);}});
native('BarnaclePatch',9,13,function(im){
 function ring(cx,cy){for(var y=-2;y<=2;y++)for(var x=-2;x<=2;x++){if(Math.abs(x)+Math.abs(y)>3)continue;var c=Math.abs(x)+Math.abs(y)===0?outline:x<0||y<0?shell[1]:shell[0];im.putPixel(cx+x,cy+y,c);}}
 ring(3,4);ring(5,8);rect(im,2,10,5,1,stone[1]);
});
native('VentOutlet',11,11,function(im){rect(im,2,2,7,7,stone[2]);rect(im,3,2,5,1,stone[5]);rect(im,2,3,1,4,stone[3]);rect(im,4,4,4,3,outline);rect(im,3,7,5,1,stone[0]);rect(im,5,8,3,1,stone[1]);});
console.log('STAGE06_NATIVE_COMPLETE');
}catch(e){console.log('STAGE06_NATIVE_FAILED '+e);}
app.command.Exit();
