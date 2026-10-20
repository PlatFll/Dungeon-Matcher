// Stage05: direct native LibreSprite cleanup/separation of the PixelLab gate.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage05/Gate_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage05/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage05/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE05_SAVED "+name);}
var raw=read(R+"Sources/Stage05/Gate_Raw_PixelLab.png");if(raw.w!==96||raw.h!==112)throw new Error("Unexpected raw grid");
var arch=blank(96,112),leaf=blank(96,112),hardware=blank(96,112);
var left=[40,37,34,32,31,30,30,29,28,27],right=[56,58,61,63,64,65,65,66,67,68];
function opening(x,y){if(y<29||y>=108)return false;var n=Math.min(y-29,9);return x>=left[n]&&x<right[n];}
var stone={"67537d":"4e3d6c","846a95":"67537d","413451":"382d55","302940":"302940","151423":"151423","10131f":"151423","181c2d":"151423","28283f":"382d55","35324f":"382d55","5b3627":"382d55","b07a43":"4e3d6c"};
// Twelve new native rows extend only straight lower sections; the curved crown stays unscaled.
for(var y=0;y<108;y++)for(var x=0;x<96;x++){
 var sy=y<66?y:y<78?65:y-12;
 if(raw.p[(sy*96+x)*4+3]&&!opening(x,y)){
  var c=stone[hex(raw,x,sy)];if(!c)throw new Error("Unexpected source material");pixel(arch,x,y,c);
 }
 if(opening(x,y))pixel(leaf,x,y,"10131f");
}
// Reconcile accidental exterior color chatter, retaining the generated silhouette.
for(var y=54;y<91;y++)for(var x=12;x<=25;x++)if(arch.p[(y*96+x)*4+3]){
 if(x===12)pixel(arch,x,y,"151423");else if(x===13)pixel(arch,x,y,"67537d");else if(x>=24)pixel(arch,x,y,"382d55");
}
for(var y=54;y<91;y++)for(var x=68;x<=81;x++)if(arch.p[(y*96+x)*4+3]){
 if(x===81)pixel(arch,x,y,"151423");else if(x>=79)pixel(arch,x,y,"382d55");
}
// One restrained stone-course joint within the authored extension.
// Remove four isolated source specks on the broad lower pier faces.
for(var n=0;n<4;n++)pixel(arch,[14,22,70,78][n],84,"4e3d6c");
for(var x=13;x<=25;x++){pixel(arch,x,72,"302940");pixel(arch,x,73,x<23?"67537d":"382d55");}
for(var x=68;x<=80;x++){pixel(arch,x,72,"302940");pixel(arch,x,73,x<78?"4e3d6c":"382d55");}
// Preserve the generated five bar positions; clean each to straight indigo planes.
var bars=[30,38,46,53,61];
for(var y=31;y<106;y++)for(var n=0;n<bars.length;n++)for(var k=0;k<3;k++){
 var x=bars[n]+k;if(opening(x,y))pixel(leaf,x,y,k===0?"35324f":k===1?"28283f":"181c2d");
}
// Retain the raw brace placement; no exaggerated metallic rim or extra braces.
for(var y=56;y<=62;y++)for(var x=27;x<=68;x++)if(opening(x,y))
 pixel(leaf,x,y,y===56||y===62?"151423":y===57?"35324f":"28283f");
// The two source bronze fitting silhouettes remain independent overlays.
for(var y=56;y<=61;y++)for(var x=26;x<=69;x++){
 var c=hex(raw,x,y);if(c==="5b3627"||c==="b07a43"){
  var warm=c==="b07a43"?"b07a43":"5b3627";
  if(c==="5b3627"&&y>=58&&y<=60&&((x>=28&&x<=30)||(x>=65&&x<=67)))warm="8a5622";
  pixel(hardware,x,y,warm);
 }
}
// Close generated pinholes directly under the piers with one consistent contact edge.
for(var x=10;x<29;x++)pixel(arch,x,107,"151423");
for(var x=67;x<84;x++)pixel(arch,x,107,"151423");
save(arch,"Gate_Arch");save(leaf,"Gate_Leaf");save(hardware,"Gate_Hardware");
console.log("STAGE05_COMPLETE 3 native architectural layers");
