// Native LibreSprite authoring. One continuous cliff/corner image per side.
var root=__ROOT__, specs=__SPECS__, pc=app.pixelColor;
function col(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
var ink=col('0a0d11'),deep=col('1d3245'),shade=col('35526c'),base=col('577993'),light=col('668fa6'),snow=col('ecfafd'),blue=col('b8d9e8');
function read(name){app.open(root+'Inputs/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image,a={w:s.width,h:s.height,p:[]};for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)a.p.push(im.getPixel(x,y));app.activeDocument.close();return a;}
var sources={Left:read('Left_Complete'),Right:read('Right_Complete'),Corner_Left:read('Corner_Left'),Corner_Right:read('Corner_Right')};
function canvas(w,h){var a={w:w,h:h,p:[]};for(var i=0;i<w*h;i++)a.p.push(0);return a;}
function put(a,x,y,c){if(x>=0&&x<a.w&&y>=0&&y<a.h)a.p[y*a.w+x]=c;}
function poly(a,points,c){var minY=a.h,maxY=0;for(var n=0;n<points.length;n++){minY=Math.min(minY,points[n][1]);maxY=Math.max(maxY,points[n][1]);}for(var y=Math.max(0,minY);y<Math.min(a.h,maxY);y++)for(var x=0;x<a.w;x++){var inside=false;for(var i=0,j=points.length-1;i<points.length;j=i++){var u=points[i],v=points[j];if(((u[1]>y)!=(v[1]>y))&&(x<(v[0]-u[0])*(y-u[1])/(v[1]-u[1])+u[0]))inside=!inside;}if(inside)put(a,x,y,c);}}
function rect(a,x,y,w,h,c){for(var j=y;j<y+h;j++)for(var i=x;i<x+w;i++)put(a,i,j,c);}
function line(a,x0,y0,x1,y1,c){var dx=Math.abs(x1-x0),sx=x0<x1?1:-1,dy=-Math.abs(y1-y0),sy=y0<y1?1:-1,err=dx+dy;while(true){put(a,x0,y0,c);if(x0===x1&&y0===y1)break;var e=2*err;if(e>=dy){err+=dy;x0+=sx;}if(e<=dx){err+=dx;y0+=sy;}}}
function paste(a,src,ox,oy){for(var y=0;y<src.h;y++)for(var x=0;x<src.w;x++){var c=src.p[y*src.w+x];if(pc.rgbaA(c))put(a,ox+x,oy+y,c);}}
function rockBody(a,src,L,g,H){
 var x0=L?0:108-g,ox=L?g-44:x0,overlap=18,len=76;
 function sample(x,y){
  var sx=x-ox,sy=28+y,c=sx>=0&&sx<44?src.p[sy*44+sx]:0;
  if(pc.rgbaA(c))return c;
  // Extend each existing rock plane to the offscreen side within this asset.
  var nearest=L?3:39,step=L?1:-1;
  for(var k=0;k<36;k++,nearest+=step){c=src.p[sy*44+nearest];if(pc.rgbaA(c))return c;}
  return shade;
 }
 for(var top=22,segment=0;top<H;top+=len-overlap,segment++){
  var seam=[],cost=[],prev=[];
  if(segment){
   for(var x=0;x<g;x++){
    cost[x]=[];prev[x]=[];
    for(var y=0;y<overlap;y++){
     var c=a.p[(top+y)*a.w+x0+x],d=sample(x0+x,y);
     var v=Math.abs(pc.rgbaR(c)-pc.rgbaR(d))+Math.abs(pc.rgbaG(c)-pc.rgbaG(d))+Math.abs(pc.rgbaB(c)-pc.rgbaB(d));
     var best=1e20,by=y;
     if(x)for(var q=Math.max(0,y-1);q<=Math.min(overlap-1,y+1);q++)if(cost[x-1][q]<best){best=cost[x-1][q];by=q;}
     cost[x][y]=v+(x?best:0);prev[x][y]=by;
    }
   }
   var last=0;for(var y=1;y<overlap;y++)if(cost[g-1][y]<cost[g-1][last])last=y;
   for(var x=g-1;x>=0;x--){seam[x]=last;last=prev[x][last];}
  }
  for(var x=0;x<g;x++)for(var y=segment?seam[x]:0;y<len&&top+y<H;y++)put(a,x0+x,top+y,sample(x0+x,y));
 }
}
function build(s){
 var a=canvas(s.width,s.height),g=s.shaft,H=s.height,L=s.side==='Left',x0=L?0:108-g,x1=L?g:108;
 // Copy approved native cliff planes into a single opaque body, cutting along
 // matching material boundaries. No repeated cap or foot is included.
 rect(a,x0,0,g,H,shade);
 rect(a,x0,0,g,12,snow);rect(a,x0,12,g,5,blue);
 rockBody(a,sources[s.side],L,g,H);
 // Retain the exact approved crown, placed once. Complete only its outer edge.
 var crown=sources[s.side];for(var y=0;y<27;y++)for(var x=0;x<crown.w;x++){var c=crown.p[y*crown.w+x];if(pc.rgbaA(c))put(a,(L?g-44:x0)+x,y,c);}
 if(L){poly(a,[[0,4],[Math.max(4,g-26),3],[Math.max(6,g-20),8],[0,17]],snow);poly(a,[[0,17],[Math.max(6,g-20),8],[Math.max(4,g-22),12],[0,22]],blue);}
 else {rect(a,108-5,10,5,3,snow);rect(a,108-5,13,5,2,blue);}
 // The cliff continues into the original lower formation without a second
 // shaft, snow apron or intersecting renderer. Preserve its foreground detail.
 paste(a,sources['Corner_'+s.side],L?-2:0,H-(L?76:88));
 // Physical screen edges are x=4 and x=104 in these overscanned assets.
 // Continue the existing rock material to the edge inside the same image.
 for(var y=0;y<H;y++){
  if(L){for(var x=0;x<=4;x++)if(!pc.rgbaA(a.p[y*a.w+x]))put(a,x,y,y>H-6?blue:base);}
  else{for(var x=104;x<108;x++)if(!pc.rgbaA(a.p[y*a.w+x]))put(a,x,y,y>H-6?blue:shade);}
 }
 app.open(root+'Inputs/Blank108x'+H+'.png');var spr=app.activeSprite,im=spr.layer(0).cel(0).image;
 for(var y=0;y<H;y++)for(var x=0;x<a.w;x++)im.putPixel(x,y,a.p[y*a.w+x]);
 spr.layer(0).name='Continuous native cliff + original timber/corner';spr.commit();
 spr.saveAs(root+'Native/'+s.name+'.ase',true);spr.saveAs(root+'Exports/'+s.name+'.png',true);app.activeDocument.close();console.log('CONNECTED '+s.name+' PASS');
}
try{for(var i=0;i<specs.length;i++)build(specs[i]);console.log('SNOW_CONNECTED_PASS');}catch(e){console.log('SNOW_CONNECTED_FAIL '+e);}
app.command.Exit();
