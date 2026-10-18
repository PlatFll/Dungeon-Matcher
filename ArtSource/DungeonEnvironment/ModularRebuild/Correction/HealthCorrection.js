// Authored material families for this environment/UI pass, not a character
// palette. The underlying contours are preserved; no dither or alpha blending.
var paletteHex=[
 // Outline, violet/blue masonry and iron recesses.
 '0a0d11','101020','14172a','191c31','20233a','272a43','30314c','3a3a57',
 '171421','211b30','29213d','312747','3b2d50','45335a','503b66','5e4473','70517f','83628f','997aa6',
 // Silver rims and stone highlights.
 '312d3b','494350','615b6b','7a7287','958ba1','b2a8bd','ccc3d5','e2dcea','f5f0f5',
 // Plum cloth, purple jewels and small magical inset accents.
 '210f31','321446','461c60','5e277b','793791','9954ac','b777c4',
 '250a3b','450e65','68108e','8918b4','ab2ad3','d34ce9','f183fb','fcd4ff',
 // Warm wood, bone and banner embroidery.
 '291d28','3c2935','543542','704653','8b5a66','a8757e','c499a3','e0bcc1',
 '4c352b','684a34','89653e','ac8454','cba571','ebc694','ffe4b6',
 // Flame/sharp local torch illumination.
 '46202b','692831','8d332e','b34429','d35c25','ec7a22','fa9d28','ffbf43','ffdd73','fff1b1',
 // Quiet floor planes and stone course edges.
 '25223f','292441','2d2646','32294c','382d52','3e3159','453660','4a3965','4f3c68','533e6a',
 '29233e','463359','5a4169','674c75','4d385e','382b4d'
];
var ramps=paletteHex.map(function(h){return [parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16)];});
var colorCache={};
function cleanColors(a){
 for(var i=0;i<a.p.length;i+=4){
  if(!a.p[i+3]){a.p[i]=a.p[i+1]=a.p[i+2]=0;continue;}
  var key=(a.p[i]<<16)|(a.p[i+1]<<8)|a.p[i+2],best=colorCache[key];
  if(best===undefined){
   var distance=1e20;
   for(var n=0;n<ramps.length;n++){
    var c=ramps[n],r=a.p[i]-c[0],g=a.p[i+1]-c[1],b=a.p[i+2]-c[2],d=r*r*2+g*g*3+b*b*2;
    if(d<distance){distance=d;best=n;}
   }
   colorCache[key]=best;
  }
  a.p[i]=ramps[best][0];a.p[i+1]=ramps[best][1];a.p[i+2]=ramps[best][2];a.p[i+3]=255;
 }
}

// Rebuild the reference heart/health frame, excluding numeric combat state.
var input=read(K+'Brief/01_REFERENCE/full_scene_reference.png'),a=blank(99,20);
for(var y=0;y<20;y++)for(var x=0;x<99;x++)for(var c=0;c<4;c++)a.p[(y*99+x)*4+c]=input.p[(((237+y)*2+1)*input.w+(25+x)*2+1)*4+c];
function inside(x,y,p){var b=false;for(var i=0,j=p.length-1;i<p.length;j=i++){var A=p[i],B=p[j];if((A[1]>y)!=(B[1]>y)&&x<(B[0]-A[0])*(y-A[1])/(B[1]-A[1])+A[0])b=!b;}return b;}
var contour=[[2,0],[10,0],[15,3],[95,3],[99,7],[99,17],[94,19],[14,19],[9,20],[0,11],[0,3]];
for(var y=0;y<20;y++)for(var x=0;x<99;x++)if(!inside(x+.5,y+.5,contour))px(a,x,y,null);
// Blank magenta inset: the art-only preview deliberately has no HP text/state.
rect(a,17,5,76,10,'960438');rect(a,17,5,76,2,'d3064d');rect(a,17,14,76,1,'7c1a30');
paletteHex=paletteHex.concat(['960438','d3064d','fb2b78','ff98a0','7c1a30']);ramps=paletteHex.map(function(h){return [parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16)];});colorCache={};cleanColors(a);
var records=[];
var pieces=[['PlayerHealthStart',0,18],['PlayerHealthMiddle',36,8],['PlayerHealthEnd',91,8]];
for(var i=0;i<pieces.length;i++){var q=pieces[i],b=crop(a,q[1],0,q[2],20);save(b,'CharacterFrames',q[0]);records.push({group:'CharacterFrames',name:q[0],width:b.w,height:b.h,source:'Brief/01_REFERENCE/full_scene_reference.png',changes:'Restored reference heart and straight silver/magenta track, replacing mismatched ornate empty health frame. No baked HP digits.'});}
console.log('HEALTH_RECORDS '+JSON.stringify(records));console.log('CORRECTION_COMPLETE');
