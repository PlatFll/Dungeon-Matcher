// Native LibreSprite source edits. Source PixelLab stones are never resampled.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage02/Recess_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage02/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong cel size");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Pixel write mismatch");s.saveAs(R+"Native/Stage02/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE02_SAVED "+name);}
var raw=read(R+"Sources/Stage02/Recess_Raw_PixelLab.png");if(raw.w!==64||raw.h!==64)throw new Error("Wrong PixelLab dimensions");
var M="0a0d11",S="10131f",B="181c2d",V="1d2032",H="22263a";
var ramp={"0a0d11":M,"10131f":S,"181c2d":B,"29253d":V};
var tile=blank(56,26);
for(var y=0;y<26;y++)for(var x=0;x<56;x++){
 var row=Math.floor(y/13),v=y%13,u=(x+row*14)%28,c=M;
 if(u>0&&v>0){
  // Retain the source's 28px course pitch and complete rectangular stone face.
  c=ramp[hex(raw,8+u,12+Math.min(v,11))];if(!c)throw new Error("Source palette changed");
  // Quiet alternating planes; remove generated isolated speckles and double grout.
  var dark=Math.floor((x+row*14)/28)%2===1;
  // Repair the isolated source notch at (12,13), retaining the other native planes.
  if(u===4&&v===1)c=V;
  if(dark&&c===V)c=B;
  if(v===1&&u>=2&&u<25)c=dark?V:H;
  if(u===27||v===12)c=S;
 }
 pixel(tile,x,y,c);
}
save(tile,"Recess_Center");
var panel=blank(80,167);
for(var y=0;y<167;y++)for(var x=0;x<80;x++){
 var c=hex(tile,(x+5)%56,(y+7)%26);
 // Discrete inner occlusion: stepped geometry, never alpha gradients or blur.
 var l=x,r=79-x,t=y,b=166-y;
 if(l<2||r<3||t<3||b<2)c=M;
 else if(l<5||r<7||t<6||b<5)c=S;
 else if(l<8||r<10||t<10||b<8){if(c===H||c===V)c=B;}
 pixel(panel,x,y,c);
}
save(panel,"Recess_Inset");
console.log("STAGE02_COMPLETE 2 native sources");
