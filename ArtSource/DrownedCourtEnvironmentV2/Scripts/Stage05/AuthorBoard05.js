var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/';
var pc=app.pixelColor,clear=pc.rgba(0,0,0,0);
function rgb(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
var outline=rgb('0a0d11'),stone=['081e2b','0f3141','164759','205b69','286b78','367d86','518f94'].map(rgb),metal=['2b313a','55606e','93a1b0','ced6cc'].map(rgb);
function output(s,n){s.commit();s.saveAs(root+'Native/Stage05/'+n+'.ase',true);s.saveAs(root+'Exports/Stage05/'+n+'.png',true);console.log('SAVED '+n+' '+s.width+'x'+s.height);app.activeDocument.close();}
function rect(im,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)im.putPixel(xx,yy,c);}
function native(n,w,h,fn){app.open(root+'Native/Stage05/Work-'+n+'.png');var s=app.activeSprite,im=app.activeImage;if(!s||s.width!==w||s.height!==h||s.filename.indexOf('Work-'+n+'.png')<0)throw Error('Wrong canvas '+n);fn(im,w,h);s.layer(0).name='Court board - native modular stone';output(s,n);}
try{
app.open(root+'RawPixelLab/Stage05/BoardStoneSeed.png');var s=app.activeSprite,im=app.activeImage;
if(!s||s.width!==128||s.height!==64||s.filename.indexOf('BoardStoneSeed.png')<0)throw Error('Wrong source');
var seed=[];
for(var y=0;y<64;y++)for(var x=0;x<128;x++){var c=im.getPixel(x,y);if(!pc.rgbaA(c))c=clear;else if(c===stone[6])c=stone[5];else if(c===stone[4])c=stone[3];seed.push(c);im.putPixel(x,y,c);}
// Dark drilled center in the original silver socket, retaining its native shape and alpha.
rect(im,35,12,5,5,metal[0]);rect(im,36,13,3,3,outline);rect(im,35,12,2,1,metal[1]);
s.layer(0).name='PixelLab stone seed - calm bevels and socket opening';output(s,'BoardStoneSeed');
var profile=[outline,stone[5],stone[4],stone[3],stone[3],stone[3],stone[3],stone[2],stone[2],stone[1],outline];
function rail(im,vertical,worn){
 for(var a=0;a<32;a++)for(var b=0;b<11;b++){
  var c=profile[b];if(a===16&&b>=2&&b<=8)c=stone[1];if(a===17&&b>=2&&b<=7)c=stone[4];
  if(worn&&a>=7&&a<=9&&b===3)c=stone[2];if(worn&&a>=8&&a<=10&&b===4)c=stone[1];
  im.putPixel(vertical?b:a,vertical?a:b,c);
 }
}
native('RailTop',32,11,function(im){rail(im,false,false);});native('RailBottom',32,11,function(im){rail(im,false,false);});
native('RailLeft',11,32,function(im){rail(im,true,false);});native('RailRight',11,32,function(im){rail(im,true,false);});
native('RailHorizontalWorn',32,11,function(im){rail(im,false,true);});native('RailVerticalWorn',11,32,function(im){rail(im,true,true);});
function socket(im,cx,cy,size){
 var r=(size-1)/2;
 for(var y=-r;y<=r;y++)for(var x=-r;x<=r;x++){
  var edge=Math.max(Math.abs(x),Math.abs(y)),diag=Math.abs(x)+Math.abs(y);if(diag>r*1.5+1)continue;
  var c=edge===r||diag>=r*1.5?outline:y<=-r+1||x<=-r+1?metal[2]:y>=r-1||x>=r-1?metal[0]:metal[1];
  if(Math.abs(x)<=1&&Math.abs(y)<=1)c=outline;else if(y===-2&&Math.abs(x)<=1)c=metal[3];
  im.putPixel(cx+x,cy+y,c);
 }
}
native('Joint',9,9,function(im){socket(im,4,4,9);});native('AttachmentSocket',11,11,function(im){socket(im,5,5,11);});
native('InnerShadow',1,1,function(im){im.putPixel(0,0,outline);});
function corner(im,right,down){
 for(var j=0;j<11;j++)for(var a=12;a<25;a++){im.putPixel(right?a:24-a,7+j,profile[j]);im.putPixel(7+j,down?a:24-a,profile[j]);}
 for(var y=2;y<=22;y++)for(var x=2;x<=22;x++){
  var dx=Math.abs(x-12),dy=Math.abs(y-12);if(dx+dy>16)continue;
  var c=dx===10||dy===10||dx+dy===16?outline:y<6||x<5?stone[5]:y>18||x>19?stone[1]:stone[3];im.putPixel(x,y,c);
 }
 // Preserve the generated silver core's native pixels inside a compact octagonal stone socket.
 for(var y=0;y<15;y++)for(var x=0;x<15;x++){
  var c=seed[(y+7)*128+x+30],dx=Math.abs(x-7),dy=Math.abs(y-7);
  if(dx+dy<=10&&(c===metal[1]||c===metal[2]||c===metal[3]||c===outline))im.putPixel(x+5,y+5,c);
 }
 rect(im,10,10,5,5,metal[0]);rect(im,11,11,3,3,outline);rect(im,10,10,2,1,metal[1]);
 // Keep the entire playable opening unobstructed: the internal L quadrant is transparent.
 for(var y=0;y<25;y++)for(var x=0;x<25;x++)if((right?x>=18:x<7)&&(down?y>=18:y<7))im.putPixel(x,y,clear);
}
native('CornerTL',25,25,function(im){corner(im,true,true);});native('CornerTR',25,25,function(im){corner(im,false,true);});
native('CornerBL',25,25,function(im){corner(im,true,false);});native('CornerBR',25,25,function(im){corner(im,false,false);});
console.log('STAGE05_NATIVE_COMPLETE');
}catch(e){console.log('STAGE05_NATIVE_FAILED '+e);}
app.command.Exit();
