// Runs inside LibreSprite after BuildFrames.js. Source crops are from the clean
// originals, not the annotated comparison. No baked label or scene is imported.
var iconPalette=['0b0b15','1c2235','403f55','676079','9891a5','bfbdcd','d4d2df','e8e6ee','fbf7fa',
 '6d3319','a55b18','d38b22','f4ad25','ffce43','ffe17a','fff1b4','b49b86','dfcab2','f7e7d3',
 '49045b','6a048c','9904c3','c216ee','e343ff','ef8eff','fbc7ff','750c30','a10c37','c11140','ed4b61'];
function iconColor(r,g,b){var best=0,score=1e20;for(var n=0;n<iconPalette.length;n++){var c=iconPalette[n],e=0;for(var k=0;k<3;k++)e+=Math.pow([r,g,b][k]-parseInt(c.substr(k*2,2),16),2);if(e<score){score=e;best=n;}}return iconPalette[best];}
// Flood only the pale external field at original resolution. The source's dark
// contour is a barrier; enclosed sockets and shading remain part of the icon.
function sourceMask(src,b){var w=b[2],h=b[3],seen=new Uint8Array(w*h),q=[],p=0;
 function visit(x,y){if(x<0||y<0||x>=w||y>=h)return;var j=y*w+x,i=((b[1]+y)*src.w+b[0]+x)*4;if(seen[j])return;
  if(Math.max(src.p[i],src.p[i+1],src.p[i+2])<68)return;seen[j]=1;q.push(j);}
 for(var x=0;x<w;x++){visit(x,0);visit(x,h-1);}for(var y=0;y<h;y++){visit(0,y);visit(w-1,y);}
 while(p<q.length){var j=q[p++],x=j%w,y=Math.floor(j/w);visit(x-1,y);visit(x+1,y);visit(x,y-1);visit(x,y+1);}
 return seen;
}
function icon(spec){var src=spec.source==='main'?main:settings,b=spec.crop,W=spec.canvas[0],H=spec.canvas[1],w=spec.ink[0],h=spec.ink[1],a=canvas(W,H),raw=canvas(W,H),solid=new Uint8Array(W*H);
 var ox=Math.floor((W-w)/2),oy=Math.floor((H-h)/2),exterior=spec.mode==='pale'?sourceMask(src,b):null;
 for(var y=0;y<h;y++)for(var x=0;x<w;x++){
  var X=Math.floor((x+.5)*b[2]/w),Y=Math.floor((y+.5)*b[3]/h),i=((b[1]+Y)*src.w+b[0]+X)*4;
  var r=src.p[i],g=src.p[i+1],blue=src.p[i+2],j=(y+oy)*W+x+ox;
  dot(raw,x+ox,y+oy,iconColor(r,g,blue));
  if(exterior)solid[j]=exterior[Y*b[2]+X]?0:1;
  else solid[j]=spec.mode==='white'?(Math.min(r,g,blue)>132?1:0):(Math.max(r,g,blue)>100?1:0);
 }
 if(exterior){for(var j=0;j<solid.length;j++)if(solid[j])for(var k=0;k<4;k++)a.p[j*4+k]=raw.p[j*4+k];}
 else {
  // Retain internal dark negative shapes; the surrounding reference background
  // becomes transparent. One native dark contour consolidates sampled AA edges.
  var edge=new Uint8Array(W*H),queue=[],at=0;
  function visit(x,y){if(x<0||y<0||x>=W||y>=H)return;var j=y*W+x;if(edge[j]||solid[j])return;edge[j]=1;queue.push(j);}
  for(var x=0;x<W;x++){visit(x,0);visit(x,H-1);}for(var y=0;y<H;y++){visit(0,y);visit(W-1,y);}
  while(at<queue.length){var j=queue[at++];visit(j%W-1,Math.floor(j/W));visit(j%W+1,Math.floor(j/W));visit(j%W,Math.floor(j/W)-1);visit(j%W,Math.floor(j/W)+1);}
  for(var y=0;y<H;y++)for(var x=0;x<W;x++){var j=y*W+x;if(!edge[j])dot(a,x,y,OUT);if(solid[j]){dot(a,x-1,y,OUT);dot(a,x+1,y,OUT);dot(a,x,y-1,OUT);dot(a,x,y+1,OUT);}}
  for(var j=0;j<solid.length;j++)if(solid[j])for(var k=0;k<4;k++)a.p[j*4+k]=raw.p[j*4+k];
 }
 // Restore neutral black inside apertures instead of capturing navy/green/red
 // button paint through a white icon's negative spaces.
 if(spec.mode==='white')for(var j=0;j<solid.length;j++)if(a.p[j*4+3]&&!solid[j])dot(a,j%W,Math.floor(j/W),OUT);
 // Discard tiny detached flecks sampled from the surrounding material. Retain
 // separate meaningful waves/handles; use eight-neighbor connectivity.
 var seen=new Uint8Array(W*H);
 for(var start=0;start<seen.length;start++)if(a.p[start*4+3]&&!seen[start]){
  var group=[start],cursor=0;seen[start]=1;
  while(cursor<group.length){var j=group[cursor++],X=j%W,Y=Math.floor(j/W);
   for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){var x=X+dx,y=Y+dy,k=y*W+x;if(x>=0&&x<W&&y>=0&&y<H&&!seen[k]&&a.p[k*4+3]){seen[k]=1;group.push(k);}}}
  if(group.length<=6)for(var q=0;q<group.length;q++)dot(a,group[q]%W,Math.floor(group[q]/W),null);
 }
 save(a,spec.name);return a;
}
var iconSpecs=[
 {name:'CrownedSkull',source:'main',crop:[267,981,70,84],canvas:[28,32],ink:[23,28],mode:'dark'},
 {name:'Characters',source:'main',crop:[145,1126,72,78],canvas:[28,32],ink:[25,28],mode:'pale'},
 {name:'Shop',source:'main',crop:[522,1125,73,79],canvas:[24,24],ink:[20,22],mode:'pale'},
 {name:'Mastery',source:'main',crop:[146,1260,76,76],canvas:[28,28],ink:[26,26],mode:'pale'},
 {name:'Practice',source:'main',crop:[531,1266,61,62],canvas:[28,28],ink:[22,23],mode:'dark'},
 {name:'Challenges',source:'main',crop:[315,1397,70,64],canvas:[28,28],ink:[24,22],mode:'dark'},
 {name:'Music',source:'settings',crop:[251,475,50,53],canvas:[24,24],ink:[18,20],mode:'white'},
 {name:'Sfx',source:'settings',crop:[252,565,55,49],canvas:[24,24],ink:[21,19],mode:'white'},
 {name:'Motion',source:'settings',crop:[250,649,57,57],canvas:[24,24],ink:[20,20],mode:'white'},
 {name:'Vibration',source:'settings',crop:[248,739,61,55],canvas:[24,24],ink:[21,19],mode:'white'},
 {name:'Resume',source:'settings',crop:[357,903,46,53],canvas:[24,24],ink:[17,20],mode:'white'},
 {name:'Suspend',source:'settings',crop:[300,1011,68,64],canvas:[24,24],ink:[22,21],mode:'white'},
 {name:'Retry',source:'settings',crop:[288,1126,57,61],canvas:[24,24],ink:[20,21],mode:'white'},
 {name:'Skull',source:'settings',crop:[344,1238,65,67],canvas:[28,32],ink:[21,22],mode:'white'}
];
for(var n=0;n<iconSpecs.length;n++)icon(iconSpecs[n]);
// Reference gear includes its silver corner bolts; keep the four button states.
var gear=extract(main,826,26,94,87,32,32,15);
mask(gear,[[4,0],[28,0],[32,4],[32,28],[28,32],[4,32],[0,28],[0,4]]);
states(gear,'Gear');
