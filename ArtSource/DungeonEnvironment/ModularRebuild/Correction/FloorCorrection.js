// Native LibreSprite drawing: small beveled dungeon stones, authored from the
// approved mockup. Distinct topology per variation, not offsets of one texture.
function inside(x,y,p){var b=false;for(var i=0,j=p.length-1;i<p.length;j=i++){var A=p[i],B=p[j];if((A[1]>y)!=(B[1]>y)&&x<(B[0]-A[0])*(y-A[1])/(B[1]-A[1])+A[0])b=!b;}return b;}
function poly(a,p,c){var lo=10000,hi=-10000;for(var i=0;i<p.length;i++){lo=Math.min(lo,p[i][1]);hi=Math.max(hi,p[i][1]);}for(var y=Math.max(0,Math.floor(lo));y<Math.min(a.h,Math.ceil(hi));y++)for(var x=0;x<a.w;x++)if(inside(x+.5,y+.5,p))px(a,x,y,c);}
var rowsFront=[0,8,17,26,35,45,54,64],rowsBack=[0,5,10,16,22,28,34,40,46,52,58,64];
var cuts=[
 [[-11,12,36,60,83],[-4,20,47,72],[-16,9,34,61,86],[-5,22,49,76]],
 [[-11,16,39,60,83],[-4,23,44,72],[-16,13,37,61,86],[-5,19,45,76]],
 [[-11,10,33,60,83],[-4,17,43,72],[-16,8,32,61,86],[-5,25,51,76]],
 [[-11,14,41,60,83],[-4,26,49,72],[-16,15,39,61,86],[-5,17,44,76]]
];
var baseFront=['45335a','4f3c68','503b66','463359'];
var lightFront=['674c75','70517f','674c75','5e4473'];
var floorRecords=[];
function stone(a,x0,x1,y0,y1,variant,row,col,back){
 var w=x1-x0,h=y1-y0,kind=(variant*3+row+col*2)%4;
 var base=back?['312747','382b4d','3b2d50','32294c'][kind]:baseFront[kind];
 var light=back?['4d385e','503b66','5a4169','4d385e'][kind]:lightFront[kind];
 var shadow=back?'25223f':['32294c','382b4d','312747','292441'][kind];
 var l=x0+1,r=x1-1,t=y0+1,b=y1-1;
 var notch=1+((variant+row+col)%3===0?1:0);
 poly(a,[[l+1,t],[r-2,t],[r,t+1],[r,b-1],[r-1,b],[l+notch,b],[l,b-2],[l,t+1]],base);
 // One connected turning-away plane, not a bevel around every edge.
 poly(a,[[l+2,b-2],[r-4,b-2],[r-2,b-3],[r,b-3],[r,b-1],[r-1,b],[l+notch,b]],shadow);
 line(a,l+2,t,r-3,t,light);
 if(h>6)line(a,l+1,t+1,l+1,t+Math.min(3,h-4),back?'45335a':'5a4169');
 if(!back && h>=8 && w>16){
  // Deliberate connected highlight facet and occasional chipped joint.
  if((row+col+variant)%3===0)poly(a,[[l+4,t+1],[l+11,t+1],[l+9,t+3],[l+4,t+3]],light);
  if((row*2+col+variant)%5===0){line(a,r-5,b-1,r-3,b-3,'25223f');px(a,r-4,b-2,shadow);}
  if((row+col+variant)%4===1)line(a,l+6,b-3,l+10,b-3,shadow);
 }
}
function makeFloor(v,back){
 var a=blank(64,64);rect(a,0,0,64,64,'211b30');var rows=back?rowsBack:rowsFront;
 for(var row=0;row<rows.length-1;row++){
  var xs=cuts[v][row%4];
  if(back){
   // Different, individually staggered smaller masonry in the distant band.
   var configs=[[-8,6,21,37,51,69],[-3,13,28,44,60,77],[-8,9,25,40,54,69],[-3,10,26,43,60,77]];
   xs=configs[(row+v)%4].slice();
   if(row%3===1){xs[2]+=v-1;xs[3]-=v%2;}
  }
  for(var c=0;c<xs.length-1;c++)stone(a,xs[c],xs[c+1],rows[row],rows[row+1],v,row,c,back);
 }
 return a;
}
for(var back=0;back<2;back++){
 var common=makeFloor(0,!!back);
 for(var v=0;v<4;v++){
  var a=makeFloor(v,!!back);
  // Shared seam signatures only; complete interiors are distinct drawings.
  for(var y=0;y<64;y++)for(var x=0;x<2;x++)for(var c=0;c<4;c++){
   a.p[(y*64+x)*4+c]=common.p[(y*64+0)*4+c];
   a.p[(y*64+63-x)*4+c]=common.p[(y*64+0)*4+c];
  }
  rect(a,0,0,64,1,'211b30');rect(a,0,63,64,1,'211b30');
  var name=(back?'FloorBack':'Floor')+String.fromCharCode(65+v);
  save(a,'FloorTiles',name);
  floorRecords.push({group:'FloorTiles',name:name,width:64,height:64,source:'ArtSource/DungeonEnvironment/ModularRebuild/Brief/01_REFERENCE/full_scene_reference.png',crop:[270,350,670,200],changes:'Hand-authored smaller beveled stone courses from actual mockup; distinct joint layout, per-stone connected planes and chips. Two-pixel shared seam signature. '+(back?'Compressed distant courses.':'Foreground course depth and stone variation.')});
 }
}
console.log('FLOOR_RECORDS '+JSON.stringify(floorRecords));
console.log('CORRECTION_COMPLETE');
