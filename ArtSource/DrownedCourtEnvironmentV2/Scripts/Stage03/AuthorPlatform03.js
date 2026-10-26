var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/';
var pc=app.pixelColor, clear=pc.rgba(0,0,0,0);
function rgb(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
var stone=['081e2b','0f3141','164759','205b69','286b78','367d86','518f94'].map(rgb);
function save(s,name){s.commit();s.saveAs(root+'Native/Stage03/'+name+'.ase',true);s.saveAs(root+'Exports/Stage03/'+name+'.png',true);console.log('SAVED '+name+' '+s.width+'x'+s.height);app.activeDocument.close();}
app.open(root+'RawPixelLab/Stage03/PlatformSeed.png');
var s=app.activeSprite,im=app.activeImage;
if(!s||s.width!==128||s.height!==64||s.filename.indexOf('PlatformSeed.png')<0)throw Error('Wrong seed');
var seed=[];
for(var y=0;y<64;y++)for(var x=0;x<128;x++)seed.push(im.getPixel(x,y));
// Quiet the generated overbright bevels and collapse isolated color specks. Keep the source silhouette exact.
var clean=seed.slice();
for(var y=0;y<64;y++)for(var x=0;x<128;x++){
 var i=y*128+x,c=seed[i];if(!pc.rgbaA(c)){clean[i]=clear;continue;}
 if(c===stone[6])clean[i]=stone[4];
 if(y>14&&y<52&&x>6&&x<121){var n=[seed[i-1],seed[i+1],seed[i-128],seed[i+128]];
  if(n[0]===n[1]&&n[0]===n[2]&&n[0]===n[3]&&pc.rgbaA(n[0]))clean[i]=n[0]===stone[6]?stone[4]:n[0];}
 im.putPixel(x,y,clean[i]);
}
s.layer(0).name='PixelLab platform - subdued bevels and connected planes';save(s,'PlatformSeed');
function rect(im,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)im.putPixel(xx,yy,c);}
function source(x,y){return clean[y*128+x];}
function native(name,w,h,draw){app.open(root+'Native/Stage03/Work-'+name+'.png');var s=app.activeSprite,im=app.activeImage;
 if(!s||s.width!==w||s.height!==h||s.filename.indexOf('Work-'+name+'.png')<0)throw Error('Wrong canvas '+name);
 draw(im,w,h);s.layer(0).name='Native platform module - hand-authored joins, PixelLab stone planes';save(s,name);}
function topProfile(y){return stone[y===0?1:y===1?2:y<=10?3:y===11?4:y===12?5:y===13?4:y===14?2:1];}
function frontProfile(y){return stone[y===0?0:y===1?1:y<=3?3:y<=18?2:y<=22?1:0];}
function top(im,w,h,joint,wear,end){
 for(var y=0;y<h;y++)rect(im,0,y,w,1,topProfile(y));
 if(joint>=0){for(var y=1;y<15;y++){var xx=joint+(y>=7?1:0);im.putPixel(xx,y,stone[1]);if(y<11)im.putPixel(xx+1,y,stone[4]);}}
 if(wear){rect(im,6,5,7,1,stone[2]);rect(im,11,4,3,1,stone[2]);rect(im,w-14,9,5,1,stone[4]);rect(im,w-10,8,3,1,stone[4]);}
 if(end){var x=end<0?0:w-1;rect(im,x,0,1,h,stone[0]);im.putPixel(x+(end<0?1:-1),0,stone[1]);}
}
function front(im,w,h,joint,wear,end){
 for(var y=0;y<h;y++)rect(im,0,y,w,1,frontProfile(y));
 if(joint>=0){
  rect(im,joint,1,2,23,stone[0]);
  // Native copy of the generated worn upper-left stone bevel, softened to the fascia value range.
  for(var y=2;y<21;y++)for(var x=0;x<5;x++){
   var c=source(8+x,26+y-2);if(pc.rgbaA(c)){
    if(c===stone[4])c=stone[3];else if(c===stone[3])c=stone[2];
    im.putPixel(joint+2+x,y,c);
   }
  }
  rect(im,joint-1,4,1,18,stone[1]);
 }
 if(wear){rect(im,8,17,6,1,stone[1]);rect(im,11,18,6,1,stone[1]);rect(im,w-11,6,4,1,stone[3]);}
 if(end){var x=end<0?0:w-1;rect(im,x,0,1,h,stone[0]);rect(im,x+(end<0?1:-1),3,1,20,stone[1]);}
}
var variants=[['A',23,true],['B',42,false],['C',15,true]];
for(var i=0;i<variants.length;i++){
 var v=variants[i];native('FloorTop'+v[0],64,16,function(im,w,h){top(im,w,h,v[1],v[2],0);});
 native('Fascia'+v[0],64,26,function(im,w,h){front(im,w,h,v[1],v[2],0);});
}
native('FloorTopFill',8,16,function(im,w,h){top(im,w,h,-1,false,0);});
native('FasciaFill',8,26,function(im,w,h){front(im,w,h,-1,false,0);});
native('FloorTopLeft',16,16,function(im,w,h){top(im,w,h,-1,false,-1);});
native('FloorTopRight',16,16,function(im,w,h){top(im,w,h,-1,false,1);});
native('FasciaLeft',16,26,function(im,w,h){front(im,w,h,-1,false,-1);});
native('FasciaRight',16,26,function(im,w,h){front(im,w,h,-1,false,1);});
app.command.Exit();
