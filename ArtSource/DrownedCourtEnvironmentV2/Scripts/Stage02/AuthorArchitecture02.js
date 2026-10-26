var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/';
var pc=app.pixelColor, transparent=pc.rgba(0,0,0,0);
function rgb(hex){return pc.rgba(parseInt(hex.substr(0,2),16),parseInt(hex.substr(2,2),16),parseInt(hex.substr(4,2),16),255);}
function key(c){return [pc.rgbaR(c),pc.rgbaG(c),pc.rgbaB(c)].join(',');}
function luma(c){return pc.rgbaR(c)*.2126+pc.rgbaG(c)*.7152+pc.rgbaB(c)*.0722;}
var stone=['081e2b','0f3141','164759','205b69','286b78','367d86','518f94'].map(rgb);
var distant=['123d4c','1a4b59','225b68','2c6c76','397c83','4b9092'].map(rgb);
function output(s,name){s.commit();s.saveAs(root+'Native/Stage02/'+name+'.ase',true);s.saveAs(root+'Exports/Stage02/'+name+'.png',true);console.log('SAVED '+name+' '+s.width+'x'+s.height);app.activeDocument.close();}
function source(name,w,h,ramp){
 app.open(root+'RawPixelLab/Stage02/'+name+'-raw.png');var s=app.activeSprite,im=app.activeImage;
 if(!s||s.width!==w||s.height!==h||s.filename.indexOf(name+'-raw.png')<0)throw Error('Wrong source '+name);
 var unique=[],seen={},raw=[],out=[];
 for(var y=0;y<h;y++)for(var x=0;x<w;x++){var c=im.getPixel(x,y);raw.push(c);if(pc.rgbaA(c)&&!seen[key(c)]){seen[key(c)]=1;unique.push(c);}}
 unique.sort(function(a,b){return luma(a)-luma(b);});var mapping={};for(var i=0;i<unique.length;i++)mapping[key(unique[i])]=ramp[Math.round(i*(ramp.length-1)/(unique.length-1))];
 for(var i=0;i<raw.length;i++)out[i]=pc.rgbaA(raw[i])?mapping[key(raw[i])]:transparent;
 // Collapse isolated interior micro-highlights only; preserve silhouette and face/trident carving.
 var simplified=out.slice(),edits=0;
 for(var y=1;y<h-1;y++)for(var x=1;x<w-1;x++){
  if(name==='MonarchStatue'&&y<65)continue;
  if(name==='CourtArch'&&x<48&&y>52&&y<90)continue;
  var i=y*w+x,c=out[i];if(!pc.rgbaA(c))continue;
  var counts={},same=0,opaque=0,best=c,max=0;
  for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++)if(dx||dy){var v=out[(y+dy)*w+x+dx];if(pc.rgbaA(v))opaque++;if(v===c)same++;counts[v]=(counts[v]||0)+1;if(counts[v]>max){max=counts[v];best=v;}}
  if(opaque===8&&same<=1&&max>=5){simplified[i]=best;edits++;}
 }
 for(var y=0;y<h;y++)for(var x=0;x<w;x++)im.putPixel(x,y,simplified[y*w+x]);
 s.layer(0).name='PixelLab architecture - native connected stone planes';output(s,name);
 console.log(name+' interior pixels simplified '+edits+'; alpha unchanged');return {w:w,h:h,p:simplified};
}
var statue=source('MonarchStatue',128,160,distant),pillar=source('PillarSeed',64,160,stone),arch=source('CourtArch',160,128,stone);
function at(a,x,y){return x>=0&&x<a.w&&y>=0&&y<a.h?a.p[y*a.w+x]:transparent;}
function native(name,w,h,draw){app.open(root+'Native/Stage02/Work-'+name+'.png');var s=app.activeSprite,im=app.activeImage;if(!s||s.width!==w||s.height!==h||s.filename.indexOf('Work-'+name+'.png')<0)throw Error('Wrong working canvas');for(var y=0;y<h;y++)for(var x=0;x<w;x++)im.putPixel(x,y,transparent);draw(im);s.layer(0).name='Native modular construction from PixelLab pillar seed';output(s,name);}
function profile(x){return x>=22&&x<=41?at(pillar,x-2,40):transparent;}
function joinrow(im,y,w){for(var x=0;x<w;x++)im.putPixel(x,y,profile(x));}
native('PillarCapital',64,20,function(im){for(var y=2;y<20;y++)for(var x=0;x<64;x++){var sy=y+15;im.putPixel(x,y,at(pillar,x-(sy>=34?2:1),sy));}joinrow(im,19,64);});
function shaft(im,worn){
 for(var y=0;y<24;y++)for(var x=22;x<=41;x++)im.putPixel(x,y,profile(x));
 for(var x=23;x<=40;x++)im.putPixel(x,17,stone[1]);for(var x=24;x<=29;x++)im.putPixel(x,18,stone[4]);
 if(worn){var pts=[[23,7],[24,8],[25,8],[26,9],[27,10],[26,11],[26,12],[27,13]];for(var i=0;i<pts.length;i++)im.putPixel(pts[i][0],pts[i][1],stone[0]);im.putPixel(28,10,stone[4]);im.putPixel(27,14,stone[4]);}
 joinrow(im,0,64);joinrow(im,23,64);
}
native('PillarShaft',64,24,function(im){shaft(im,false);});native('PillarShaftWorn',64,24,function(im){shaft(im,true);});
native('PillarBase',64,24,function(im){for(var y=0;y<21;y++)for(var x=0;x<64;x++)im.putPixel(x,y,at(pillar,x-(y<2?2:1),121+y));joinrow(im,0,64);});
native('BrokenPillar',96,128,function(im){
 // A snapped, leaning reuse of the same stone source; native row offsets, no rotated/resampled texture.
 for(var y=24;y<=120;y++)for(var x=0;x<64;x++){
  var sy=45+(y-24),c=at(pillar,x,sy),lean=Math.floor((120-y)/5);
  if(y<27&&x>27&&x<35)continue;if(pc.rgbaA(c))im.putPixel(x+18+lean,y,c);
 }
 for(var x=20;x<=39;x++){var yy=24+(x%4<2?0:1);im.putPixel(x+18+Math.floor((120-yy)/5),yy,stone[4]);}
});
native('RuinedLedge',64,16,function(im){
 for(var y=2;y<=13;y++)for(var x=0;x<64;x++){
  var c=y===2?stone[5]:y===3?stone[4]:y>=11?stone[1]:stone[3];
  if(y>=4&&y<=10&&(x===15||x===31||x===47))c=stone[1];
  if(y===8&&((x>=7&&x<=9)||(x>=39&&x<=42)))c=stone[2];im.putPixel(x,y,c);
 }
});
native('RubbleJoint',32,24,function(im){
 var chunks=[[4,15,17,21],[12,9,24,18],[22,16,28,21]];
 for(var i=0;i<chunks.length;i++){var b=chunks[i];for(var y=b[1];y<=b[3];y++)for(var x=b[0];x<=b[2];x++){if((x===b[0]||x===b[2])&&y===b[1])continue;im.putPixel(x,y,y===b[1]?stone[4]:y===b[3]||x===b[2]?stone[1]:stone[3]);}}
});
app.command.Exit();
