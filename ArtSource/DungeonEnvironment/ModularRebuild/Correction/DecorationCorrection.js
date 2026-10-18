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

var decorationRecords=[];
function inside(x,y,p){var b=false;for(var i=0,j=p.length-1;i<p.length;j=i++){var A=p[i],B=p[j];if((A[1]>y)!=(B[1]>y)&&x<(B[0]-A[0])*(y-A[1])/(B[1]-A[1])+A[0])b=!b;}return b;}
function poly(a,p,c){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(inside(x+.5,y+.5,p))px(a,x,y,c);}
function cut(a,p){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(!inside(x+.5,y+.5,p))px(a,x,y,null);return a;}
function emitD(group,name,a,change){save(a,group,name);decorationRecords.push({group:group,name:name,width:a.w,height:a.h,source:'Brief/01_REFERENCE/full_scene_reference.png',changes:change});}
var source=read(K+'Brief/01_REFERENCE/full_scene_reference.png'),N=blank(470,836);
// Retain the established native extraction grid. All selection/palette edits
// occur through LibreSprite's RGBA cel API; no image resizing/interpolation.
for(var y=0;y<N.h;y++)for(var x=0;x<N.w;x++)for(var c=0;c<4;c++)N.p[(y*N.w+x)*4+c]=source.p[((y*2+1)*source.w+x*2+1)*4+c];
var icons=[
 ['AbilityEmblem',214,734,44,48,[[16,0],[25,0],[26,6],[28,7],[28,12],[31,12],[31,6],[39,6],[39,14],[37,14],[37,18],[41,18],[43,21],[43,34],[39,36],[36,39],[32,39],[32,46],[25,47],[24,45],[18,46],[17,47],[10,47],[9,40],[5,39],[2,36],[0,34],[0,21],[3,18],[3,14],[2,14],[2,6],[10,6],[10,12],[14,12],[14,7],[16,6]]],
 ['PotionIcon',85,742,33,39,[[10,0],[23,0],[25,9],[26,13],[32,19],[32,30],[26,38],[7,38],[1,31],[0,20],[7,13],[8,9]]],
 ['BombIcon',347,741,39,43,[[24,0],[33,0],[36,6],[34,13],[38,20],[39,28],[34,37],[26,42],[14,42],[5,37],[0,28],[1,19],[7,10],[16,7],[24,7]]]
];
// Add role colors for the small warm-gold/bone and ruby glass icons. These are
// deliberate material assignments; no UI-wide palette reduction.
paletteHex=paletteHex.concat(['8a5622','c58826','fbea90','8d725c','b7a393','fdf5e5','fefbef','4b1127','7c1a30','c74a5a','ff98a0','960438','d3064d','fb2b78','fff1d9']);
ramps=paletteHex.map(function(h){return [parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16)];});colorCache={};
for(var i=0;i<icons.length;i++){
 var q=icons[i],a=cut(crop(N,q[1],q[2],q[3],q[4]),q[5]);cleanColors(a);
 if(q[0]==='AbilityEmblem'){
  // Remove the source panel's tiny violet rim above the crown; the emblem's
  // own top contour begins below it. Normalize purple background residue on
  // the selected skull contour while retaining all bone/gold facial clusters.
  rect(a,0,0,a.w,2,null);
  for(var yy=2;yy<a.h;yy++)for(var xx=0;xx<a.w;xx++){
   var ii=(yy*a.w+xx)*4;if(a.p[ii+3]&&a.p[ii+2]>a.p[ii]*1.1&&a.p[ii+2]>a.p[ii+1]*1.2)px(a,xx,yy,'0a0d11');
  }
 }
 emitD('HudFrames',q[0],a,'Isolated original mockup ornament on the existing native grid; connected contour selection and material palette cleanup.');
}
// Retain the actual purple electrical drawing; remove the central flat repair
// and the enclosing silver frame to leave independently placeable flank motifs.
var panel=read(P+'ArtSource/BattlePresentation/DungeonMockupRevision/native/SharedAbility.png');
for(var side=0;side<2;side++){
 var a=crop(panel,side?118:14,10,44,44);
 for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){
  var i=(y*a.w+x)*4,r=a.p[i],g=a.p[i+1],b=a.p[i+2];
  if(!(b>r*1.1&&b>g*1.3&&b>65))px(a,x,y,null);
 }
 emitD('HudFrames',side?'AbilityLightningRight':'AbilityLightningLeft',a,'Separate native purple lightning clusters retained from the source ability inset.');
}
var fill=blank(8,8);rect(fill,0,0,8,8,'8918b4');rect(fill,0,0,8,2,'d34ce9');rect(fill,0,2,8,1,'ab2ad3');rect(fill,0,7,8,1,'68108e');emitD('HudFrames','EnergyPurpleFill',fill,'Native eight-pixel purple bar with controlled top highlight and lower recess.');
// Match the mockup's small resonance diamond as an independent UI ornament.
var diamond=cut(crop(N,121,796,20,22),[[9,0],[17,4],[20,10],[11,22],[0,10],[2,4]]);cleanColors(diamond);emitD('HudFrames','ResonanceDiamond',diamond,'Separated reference resonance jewel; live text remains Unity typography.');
// Remove accidental copied skull/bones from the barrel asset, completing its
// occluded lower-right wood with the same native stave/band material.
var barrel=read(K+'Exports/Props/FrontBarrel.png');
cut(barrel,[[3,0],[31,0],[34,4],[34,39],[29,45],[7,45],[2,40],[0,5]]);
for(var y=26;y<44;y++)for(var x=22;x<34;x++){
 if(x>=33||y>=42&&x>29)continue;
 var c=(x===24||x===30)?'291d28':(x<25?'543542':'3c2935');px(barrel,x,y,c);
}
line(barrel,22,33,32,33,'291d28');line(barrel,22,34,32,34,'7a7287');line(barrel,22,35,32,35,'494350');line(barrel,22,36,32,36,'291d28');
emitD('Props','FrontBarrel',barrel,'Removed duplicated attached skull/bone contamination; reconstructed occluded right staves and lower iron band.');
// Replace flat counter-occlusion patches in the lower gateway piers with
// actual chamfered violet stones, keeping the original contour/door.
var gate=read(K+'Exports/Props/Gateway.png');
for(var side=0;side<2;side++)for(var row=0;row<3;row++){
 var x=side?99:1,y=75+row*14,w=15,h=13;
 rect(gate,x+1,y+1,w-2,h-2,'312747');line(gate,x+2,y+1,x+w-3,y+1,'503b66');line(gate,x+1,y+2,x+1,y+h-4,'45335a');line(gate,x+2,y+h-2,x+w-2,y+h-2,'211b30');
 if(row===1){line(gate,x+10,y+3,x+12,y+5,'29213d');px(gate,x+12,y+6,'29213d');}
}
emitD('Props','Gateway',gate,'Reconstructed the six flat lower pier patches as shaded/chamfered native stones; arch silhouette, door slats and iron hinges retained.');
// Repair alpha in the chain section that was previously copied as RGB only.
var chain=read(K+'Exports/Props/FarRightChain.png'),link=crop(read(P+'ArtSource/BattlePresentation/DungeonMockupRevision/native/LeftBorderDressing.png'),0,26,16,24);
for(var y=25;y<77;y++)for(var x=0;x<21;x++){
 var xx=x-3,yy=(y-25)%24,i=(yy*link.w+xx)*4;
 if(xx<0||xx>=16||!link.p[i+3])px(chain,x,y,null);else px(chain,x,y,color(link,xx,yy));
}
emitD('Props','FarRightChain',chain,'Restored source transparency through the previously RGB-only reconstructed link band.');
console.log('DECORATION_RECORDS '+JSON.stringify(decorationRecords));console.log('CORRECTION_COMPLETE');
