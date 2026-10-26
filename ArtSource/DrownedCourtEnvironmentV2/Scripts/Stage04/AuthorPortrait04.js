var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/';
var pc=app.pixelColor,clear=pc.rgba(0,0,0,0);
function rgb(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
var outline=rgb('0a0d11'),stone=['081e2b','0f3141','164759','205b69','286b78','367d86','518f94'].map(rgb);
var water=['08151e','0c1f2b','102a38','153542','1b404d'].map(rgb),green=['123f37','23584a','44775a','75956d'].map(rgb),coral=['58384c','804c64','be7887'].map(rgb);
function output(s,n){s.commit();s.saveAs(root+'Native/Stage04/'+n+'.ase',true);s.saveAs(root+'Exports/Stage04/'+n+'.png',true);console.log('SAVED '+n+' '+s.width+'x'+s.height);app.activeDocument.close();}
function rect(im,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)im.putPixel(xx,yy,c);}
function line(im,x0,y0,x1,y1,c){var dx=Math.abs(x1-x0),sx=x0<x1?1:-1,dy=-Math.abs(y1-y0),sy=y0<y1?1:-1,err=dx+dy;for(;;){im.putPixel(x0,y0,c);if(x0===x1&&y0===y1)break;var e=2*err;if(e>=dy){err+=dy;x0+=sx;}if(e<=dx){err+=dx;y0+=sy;}}}
function native(n,w,h,fn){app.open(root+'Native/Stage04/Work-'+n+'.png');var s=app.activeSprite,im=app.activeImage;if(!s||s.width!==w||s.height!==h||s.filename.indexOf('Work-'+n+'.png')<0)throw Error('Wrong canvas '+n);fn(im,w,h);s.layer(0).name='Court portrait - native modular pixels';output(s,n);}
try{
app.open(root+'RawPixelLab/Stage04/CornerMotif.png');var s=app.activeSprite,im=app.activeImage;
if(!s||s.width!==64||s.height!==64||s.filename.indexOf('CornerMotif.png')<0)throw Error('Wrong corner source');
var seed=[];
for(var y=0;y<64;y++)for(var x=0;x<64;x++){var c=im.getPixel(x,y);if(!pc.rgbaA(c))c=clear;else if(c===stone[6])c=stone[5];seed.push(c);im.putPixel(x,y,c);}
s.layer(0).name='PixelLab cornerstone - quiet bevels, original silhouette';output(s,'CornerMotif');
function src(x,y){return seed[y*64+x];}
app.open(root+'RawPixelLab/Stage04/RecessSeed.png');s=app.activeSprite;im=app.activeImage;
if(!s||s.width!==64||s.height!==64||s.filename.indexOf('RecessSeed.png')<0)throw Error('Wrong recess source');
var recess=[];
for(var y=0;y<64;y++)for(var x=0;x<64;x++){var c=im.getPixel(x,y);if(c===water[4])c=water[3];recess.push(c);}
var quiet=recess.slice();
for(var y=1;y<63;y++)for(var x=1;x<63;x++){var i=y*64+x,c=recess[i],same=0;var ns=[recess[i-1],recess[i+1],recess[i-64],recess[i+64]];for(var k=0;k<4;k++)if(c===ns[k])same++;if(same===0)quiet[i]=water[2];}
// Explicit repeat boundaries. Continuous material, no straight vertical dark seam at tile boundaries.
for(var y=0;y<64;y++)for(var x=0;x<64;x++){
 var c=quiet[y*64+x];if(y===0||y===63)c=water[0];else if(x===0||x===63)c=(y%16>=14||y%16===0)?water[0]:y%16===1?water[3]:water[2];
 im.putPixel(x,y,c);quiet[y*64+x]=c;
}
s.layer(0).name='PixelLab dark stone recess - native seamless edge repairs';output(s,'RecessA');
native('RecessB',64,64,function(im){for(var y=0;y<64;y++)for(var x=0;x<64;x++){var c=quiet[y*64+(x+32)%64];if(x===0||x===63)c=quiet[y*64];im.putPixel(x,y,c);}});
var rail=[outline,stone[5],stone[4],stone[3],stone[1],stone[3],outline];
function frameCorner(im,right,down){
 for(var j=0;j<7;j++){
  for(var a=7;a<15;a++){im.putPixel(right?a:14-a,4+j,rail[j]);im.putPixel(4+j,down?a:14-a,rail[j]);}
 }
 for(var y=1;y<=13;y++)for(var x=1;x<=13;x++){
  if((x<3&&y<3)||(x>11&&y<3)||(x<3&&y>11)||(x>11&&y>11))continue;
  var c=x===1||y===1||x===13||y===13?outline:y<4||x<3?stone[5]:y>10||x>11?stone[2]:stone[3];im.putPixel(x,y,c);
 }
 // Original generated metal socket, native 9x9 crop: no reflection or resampling.
 for(var y=0;y<9;y++)for(var x=0;x<9;x++)im.putPixel(x+3,y+3,src(x+16,y+16));
}
native('CornerTL',15,15,function(im){frameCorner(im,true,true);});native('CornerTR',15,15,function(im){frameCorner(im,false,true);});
native('CornerBL',15,15,function(im){frameCorner(im,true,false);});native('CornerBR',15,15,function(im){frameCorner(im,false,false);});
function hrail(im){for(var y=0;y<7;y++)rect(im,0,y,16,1,rail[y]);}
function vrail(im){for(var x=0;x<7;x++)rect(im,x,0,1,16,rail[x]);}
native('RailTop',16,7,hrail);native('RailBottom',16,7,hrail);native('RailLeft',7,16,vrail);native('RailRight',7,16,vrail);
native('Clamp',9,9,function(im){for(var y=0;y<9;y++)for(var x=0;x<9;x++)im.putPixel(x,y,src(x+16,y+16));});
native('Barnacles',12,16,function(im){var boxes=[[8,8,14,14],[6,16,11,21]];for(var b=0;b<boxes.length;b++){var a=boxes[b];for(var y=a[1];y<=a[3];y++)for(var x=a[0];x<=a[2];x++){var c=src(x,y);if(pc.rgbaA(c))im.putPixel(x-6,y-8,c);}}});
function kelp(im,right){
 var stem=right?[[3,31],[3,26],[5,22],[6,17],[5,12],[7,7],[7,2]]:[[8,31],[7,27],[6,23],[7,18],[5,13],[5,7],[3,2]];
 for(var i=1;i<stem.length;i++){var a=stem[i-1],b=stem[i];line(im,a[0],a[1],b[0],b[1],green[0]);line(im,a[0]+1,a[1],b[0]+1,b[1],green[2]);}
 var leaves=right?[[5,22,10,16],[6,15,2,10],[7,8,10,3]]:[[7,25,2,20],[7,18,2,12],[5,10,8,5]];
 for(var i=0;i<leaves.length;i++){var a=leaves[i];line(im,a[0],a[1],a[2],a[3],green[0]);line(im,a[0]+1,a[1],a[2]+1,a[3],green[1]);line(im,a[0],a[1]-1,a[2],a[3]-1,green[2]);}
}
native('AlgaeLeft',12,32,function(im){kelp(im,false);});native('AlgaeRight',12,32,function(im){kelp(im,true);});
native('CoralSprig',12,20,function(im){
 var branches=[[6,18,6,7],[6,13,2,9],[6,10,10,6],[6,7,4,3],[2,9,2,5],[10,6,10,2]];
 for(var i=0;i<branches.length;i++){var a=branches[i];line(im,a[0],a[1],a[2],a[3],coral[0]);line(im,a[0]+1,a[1],a[2]+1,a[3],coral[1]);im.putPixel(a[2],a[3],coral[2]);}
 rect(im,5,18,4,2,stone[2]);
});
console.log('STAGE04_NATIVE_COMPLETE');
}catch(e){console.log('STAGE04_NATIVE_FAILED '+e);}
app.command.Exit();
