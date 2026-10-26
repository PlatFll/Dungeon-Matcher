// Native LibreSprite artwork. Independent architectural modules; no scene flattening.
var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/';
var pc=app.pixelColor,clear=pc.rgba(0,0,0,0),im,w,h,coralSource=[];
function rgb(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
var st=['062432','083d50','0d5368','176c7d','258999','3aa3ae','65c1be'].map(rgb),kelp=['102e2c','294c36','537543','8a9e50'].map(rgb),rose=['572b4a','a75271','db8b97'].map(rgb),violet=['352b50','775789','ab83b1'].map(rgb),orange=['70413f','bd705e','edab80'].map(rgb),shell=['253b40','8f9e8d','d5d6b0'].map(rgb);
function pixel(x,y,c){if(x>=0&&x<w&&y>=0&&y<h)im.putPixel(x,y,c);}
function rect(x,y,ww,hh,c){for(var yy=y;yy<y+hh;yy++)for(var xx=x;xx<x+ww;xx++)pixel(xx,yy,c);}
function poly(pts,c){var lo=h,hi=0;for(var i=0;i<pts.length;i++){lo=Math.min(lo,pts[i][1]);hi=Math.max(hi,pts[i][1]);}for(var y=lo;y<=hi;y++){var cuts=[];for(var i=0,j=pts.length-1;i<pts.length;j=i++){var a=pts[i],b=pts[j];if((a[1]<=y&&b[1]>y)||(b[1]<=y&&a[1]>y))cuts.push(a[0]+(y-a[1])*(b[0]-a[0])/(b[1]-a[1]));}cuts.sort(function(a,b){return a-b;});for(var i=0;i+1<cuts.length;i+=2)for(var x=Math.ceil(cuts[i]);x<=Math.floor(cuts[i+1]);x++)pixel(x,y,c);}}
function line(x,y,X,Y,c,size){var dx=Math.abs(X-x),sx=x<X?1:-1,dy=-Math.abs(Y-y),sy=y<Y?1:-1,e=dx+dy;while(true){rect(x-Math.floor(size/2),y-Math.floor(size/2),size,size,c);if(x===X&&y===Y)break;var ee=2*e;if(ee>=dy){e+=dy;x+=sx;}if(ee<=dx){e+=dx;y+=sy;}}}
function path(p,c,size){for(var i=1;i<p.length;i++)line(p[i-1][0],p[i-1][1],p[i][0],p[i][1],c,size);}
function column(height,variant){
 rect(5,0,22,height,st[0]);rect(6,0,19,height,st[2]);rect(7,0,6,height,st[3]);rect(8,0,2,height,st[4]);rect(14,0,1,height,st[1]);rect(16,0,5,height,st[3]);rect(23,0,2,height,st[1]);
 for(var y=26;y<height;y+=32){rect(5,y,22,2,st[0]);rect(6,y+2,18,1,st[4]);rect(23,y+2,3,1,st[2]);}
 // Sparse broad chips; darkness is concentrated on the joints and turning planes.
 var yy=variant?14:8;poly([[5,yy],[9,yy+1],[10,yy+5],[7,yy+8],[5,yy+8]],st[1]);path([[6,yy+1],[8,yy+2],[8,yy+5]],st[3],1);
 if(height>40){path([[23,42],[21,45],[22,48],[19,50]],st[1],1);path([[22,42],[20,45],[21,48]],st[4],1);rect(11,80,2,6,st[2]);}
}
function ledge(y,side){
 // The ledge continues the shaft: broad cap plane, dark contact recess, supported underside.
 poly([[3,y],[24,y-1],[28,y+2],[29,y+6],[26,y+9],[6,y+10],[2,y+7],[1,y+3]],st[0]);
 poly([[3,y+1],[23,y],[27,y+3],[25,y+5],[4,y+5],[2,y+3]],st[4]);
 path([[4,y+1],[14,y+1],[16,y],[23,y]],st[5],1);poly([[4,y+6],[25,y+6],[25,y+8],[7,y+9]],st[2]);rect(7,y+7,15,1,st[3]);
 path([[13,y+1],[12,y+3],[14,y+4],[13,y+6]],st[1],1);rect(side?21:6,y+3,4,2,st[0]);
}
function collar(y,side){
 // Narrow complete capital clears the existing frame diamonds on both sides.
 poly([[5,y],[22,y-1],[25,y+2],[26,y+6],[24,y+9],[7,y+10],[4,y+7],[4,y+3]],st[0]);
 poly([[5,y+1],[22,y],[24,y+3],[23,y+5],[6,y+5],[5,y+3]],st[4]);
 path([[6,y+1],[14,y+1],[16,y],[22,y]],st[5],1);poly([[6,y+6],[24,y+6],[23,y+8],[8,y+9]],st[2]);rect(8,y+7,14,1,st[3]);
 path([[13,y+1],[12,y+3],[14,y+4],[13,y+6]],st[1],1);
}
function leaf(x,y,dir){poly([[x,y+12],[x+dir*4,y+8],[x+dir*5,y+5],[x+dir*4,y+2],[x+dir*2,y],[x+dir*3,y+5],[x+dir,y+7]],kelp[0]);poly([[x,y+11],[x+dir*3,y+7],[x+dir*4,y+5],[x+dir*3,y+3],[x+dir*2,y+6],[x+dir,y+7]],kelp[2]);path([[x+dir,y+8],[x+dir*3,y+5],[x+dir*3,y+3]],kelp[3],1);}
function wrapKelp(y,side){
 // Alternating front/back depth. The tip stays visible; the foot disappears inside a stone joint.
 var p=side?[[8,y],[7,y+5],[9,y+11],[16,y+16],[23,y+20],[24,y+25],[20,y+30],[12,y+34],[8,y+39],[10,y+45],[14,y+49]]:[[23,y],[24,y+5],[21,y+10],[14,y+14],[7,y+19],[6,y+24],[10,y+28],[18,y+33],[23,y+37],[23,y+43],[19,y+49]];
 path(p,kelp[0],5);path(p,kelp[1],3);
 for(var i=1;i<p.length-1;i++)line(p[i][0]-1,p[i][1],p[i+1][0]-1,p[i+1][1],i>=4&&i<8?kelp[2]:kelp[1],1);
 var foot=p[p.length-1];rect(foot[0]-3,foot[1]+1,7,2,st[0]);rect(foot[0]-2,foot[1]+3,6,1,st[3]);
 leaf(side?9:21,y+6,side?-1:1);leaf(side?22:8,y+21,side?1:-1);leaf(side?10:21,y+36,side?-1:1);
}
function coral(cx,cy,kind,flip){var palette=kind===0?rose:kind===1?violet:orange;var branches=[[[0,0],[-1,-6],[-3,-13],[-2,-21],[0,-27]],[[0,-5],[5,-10],[6,-17]], [[-2,-11],[-7,-14],[-9,-21]],[[-1,-18],[3,-22],[3,-28]], [[3,-8],[9,-10],[10,-14]]];
 for(var k=0;k<branches.length;k++){var p=branches[k].map(function(p){return[cx+p[0]*(flip?-1:1),cy+p[1]];});path(p,palette[0],4);path(p.map(function(p){return[p[0]-1,p[1]-1];}),palette[1],2);var end=p[p.length-1];rect(end[0]-1,end[1]-1,2,1,palette[2]);}
 // Continuous contact shadow joins coral roots directly into the cracked shelf.
 rect(cx-4,cy,9,2,st[0]);line(cx,cy-3,cx,cy+1,palette[0],3);rect(cx-2,cy,3,1,palette[1]);
}
function grownColony(cy,kind,flip){
 var palette=kind===0?rose:kind===1?violet:orange;
 // Preserve the PixelLab branching silhouette at native scale. Its old isolated rock foot is replaced
 // by a socket cut into the continuous column ledge, so there is no pasted-on second pedestal.
 var old=[rgb('63334f'),rgb('944a68'),rgb('c5728b')];
 for(var sy=0;sy<48;sy++)for(var sx=0;sx<33;sx++){
  var c=coralSource[sy*33+sx];if(!pc.rgbaA(c))continue;
  var idx=old.indexOf(c);if(idx<0){if(c===rgb('0a0d11')||c===rgb('2f203d'))idx=0;else continue;}
  var dx=(flip?32-sx:sx)-1,dy=cy-47+sy;pixel(dx,dy,palette[idx]);
 }
 rect(11,cy,11,2,st[0]);rect(14,cy-2,4,4,palette[0]);rect(14,cy-1,2,2,palette[1]);
}
function barnacle(x,y){poly([[x-2,y-1],[x,y-3],[x+2,y-2],[x+3,y],[x+2,y+3],[x-1,y+3],[x-3,y+1]],shell[0]);poly([[x-2,y],[x-1,y-2],[x+1,y-2],[x+2,y],[x+1,y+2],[x-1,y+2]],shell[1]);path([[x-2,y],[x-1,y-2],[x+1,y-2]],shell[2],1);rect(x,y-1,1,2,st[0]);}
function smallMoss(x,y){poly([[x,y],[x+3,y-1],[x+7,y],[x+7,y+3],[x+4,y+2],[x+3,y+5],[x+2,y+2]],kelp[1]);rect(x+1,y,3,1,kelp[2]);pixel(x+5,y+1,kelp[2]);}
function save(name,height,fn){w=31;h=height;app.open(root+'Native/Revision01/Work-'+name+'.png');var s=app.activeSprite;im=app.activeImage;if(s.width!==w||s.height!==h)throw Error('Wrong canvas '+name);fn();s.layer(0).name='Connected stone, recessed growth and native material clusters';s.commit();s.saveAs(root+'Native/Revision01/'+name+'.ase',true);s.saveAs(root+'Exports/Revision01/'+name+'.png',true);console.log('SAVED '+name);app.activeDocument.close();}
try{
 app.open(root+'Exports/Stage06/CoralColony.png');var ci=app.activeImage;
 for(var cy=0;cy<57;cy++)for(var cx=0;cx<33;cx++)coralSource.push(ci.getPixel(cx,cy));app.activeDocument.close();
 ['Left','Right'].forEach(function(side){var right=side==='Right';
  save('Cap'+side,16,function(){column(16,0);collar(2,right);smallMoss(right?19:5,4);});
  save('Column'+side+'A',96,function(){column(96,right?1:0);wrapKelp(2,right);ledge(77,right);grownColony(78,right?0:2,right);barnacle(right?8:22,88);smallMoss(right?23:4,79);});
  save('Column'+side+'B',96,function(){column(96,right?0:1);ledge(49,right);grownColony(50,1,!right);wrapKelp(44,!right);barnacle(right?23:8,58);barnacle(right?20:11,62);smallMoss(right?5:19,51);});
  save('Quiet'+side+'A',32,function(){column(32,0);barnacle(right?9:22,16);barnacle(right?12:19,20);smallMoss(right?20:5,27);});
  save('Quiet'+side+'B',32,function(){column(32,1);path([[right?8:23,5],[right?10:21,9],[right?9:22,14],[right?12:19,18]],kelp[0],3);path([[right?8:23,5],[right?10:21,9],[right?9:22,14],[right?12:19,18]],kelp[2],1);leaf(right?10:21,7,right?-1:1);rect(9,26,15,2,st[0]);});
  save('Short'+side,18,function(){column(18,right?1:0);smallMoss(right?6:20,9);});
  save('Base'+side,18,function(){column(18,0);collar(7,right);barnacle(right?21:9,6);});
 });console.log('INTEGRATED_SIDES_COMPLETE');
}catch(e){console.log('INTEGRATED_SIDES_FAILED '+e);}app.command.Exit();
