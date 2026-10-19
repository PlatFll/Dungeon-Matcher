// Stage04: native construction from the Stage03 PixelLab side-view support brick.
// Extends its quiet planes at 1px pitch; no scaled artwork or perspective rows.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage04/Floor_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage04/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage04/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE04_SAVED "+name);}
var raw=read(R+"Sources/Stage04/Floor_Raw_PixelLab.png");
if(raw.w!==128||raw.h!==128)throw new Error("Unexpected raw grid");
var D="151423",M="241f3a",S="382d55",B="4e3d6c",T="67537d";
var map={"665592":T,"41305d":B,"8881bb":T,"332648":B,"3c2c55":B,"2b1934":S,"34224b":B,"24112a":S,"36284d":B,"312245":S,"382245":S,"210f22":M,"1e0d1e":D};
function make(name,w,joints,variant,end){
 var a=blank(w,24);
 // The raw side-view support brick is x96..111,y23..27.
 // Preserve its two-pixel edge clusters; author wider/taller interior planes.
 for(var y=0;y<22;y++)for(var x=0;x<w;x++){
  var sx=96+(x%16),sy=23+Math.min(y,4),c=map[hex(raw,sx,sy)];
  if(!c)throw new Error("Unrecognized source color");
  // The new facade is one substantial course rather than four tiny raw rows.
  if(y===0)c=S;
  else if(y===1)c=B;
  else if(y===2)c=T;
  else if(y>=3&&y<=14)c=B;
  else if(y>=15&&y<=17)c=S;
  else if(y===18)c=M;
  else c=D;
  pixel(a,x,y,c);
 }
 // Interior mortar joints: staggered independently from capstone joints.
 for(var n=0;n<joints.length;n++){
  var j=joints[n];
  for(var y=2;y<=17;y++)pixel(a,j,y,M);
  pixel(a,j,1,S);pixel(a,j-1,2,S);pixel(a,j+1,2,B);
  pixel(a,j-1,3,S);pixel(a,j+1,3,T);
  for(var y=4;y<=7;y++)pixel(a,j+1,y,T);
  pixel(a,j-1,16,M);pixel(a,j+1,17,M);
  // Retain the source's lower-left bevel cluster, mapped to the new material.
  pixel(a,j+1,15,map[hex(raw,96,26)]);
  pixel(a,j+2,16,map[hex(raw,97,26)]);
 }
 // Sparse connected top-edge chips, never standalone speckle.
 if(variant===1){for(var x=24;x<=27;x++)pixel(a,x,2,B);pixel(a,25,3,S);pixel(a,26,3,S);}
 if(variant===2){for(var x=34;x<=37;x++)pixel(a,x,15,B);pixel(a,36,14,S);pixel(a,37,14,S);}
 // Full-width open connection profiles are shared for every legal neighbor.
 for(var y=0;y<22;y++){var c=y===0?S:y===1?B:y===2?T:y<=14?B:y<=17?S:y===18?M:D;pixel(a,0,y,c);pixel(a,w-1,y,c);}
 // Ends use dark turned-away planes; no sprite is clipped to fit the viewport.
 if(end==="left"){for(var y=1;y<18;y++)pixel(a,0,y,S);pixel(a,1,2,B);pixel(a,0,18,D);}
 if(end==="right"){for(var y=1;y<18;y++)pixel(a,w-1,y,S);pixel(a,w-2,2,B);pixel(a,w-1,18,D);}
 // Two transparent footer rows let the unchanged wall continue behind the contact shadow.
 save(a,name);
}
make("Fascia_Mid_A",64,[16,48],0,"");
make("Fascia_Mid_B",64,[12,44],1,"");
make("Fascia_Anchor",64,[16],2,"");
make("Fascia_Left",42,[16],0,"left");
make("Fascia_Right",62,[16,48],1,"right");
console.log("STAGE04_COMPLETE 5 native front-face documents");
