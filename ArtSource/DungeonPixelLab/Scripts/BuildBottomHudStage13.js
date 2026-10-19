// Stage13: direct native LibreSprite cleanup of new PixelLab environmental props. No scaling.
var R="__ROOT__/";
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function hex(a,x,y){var i=(y*a.w+x)*4,s="";for(var k=0;k<3;k++)s+=("0"+a.p[i+k].toString(16)).slice(-2);return s;}
function pixel(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)throw new Error("Drawing outside canvas");var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=parseInt(c.substr(k*2,2),16);a.p[i+3]=255;}
function save(a,name){app.open(R+"Sources/Stage13/Resonance_Raw_PixelLab.png");var s=app.activeSprite;s.saveAs(R+"Native/Stage13/"+name+".aseprite",false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();var c=s.layer(0).cel(0);if(c.image.width!==a.w||c.image.height!==a.h)throw new Error("Wrong allocation");c.image.putImageData(a.p);s.layer(0).name=name;s.commit();var p=s.layer(0).cel(0).image.getImageData();for(var i=0;i<a.p.length;i++)if(p[i]!==a.p[i])throw new Error("Native byte mismatch");s.saveAs(R+"Native/Stage13/"+name+".aseprite",true);app.activeDocument.close();console.log("STAGE13_SAVED "+name);}



var raw=read(R+'Sources/Stage13/Resonance_Raw_PixelLab.png');
if(raw.w!==64||raw.h!==64)throw new Error('Wrong seed canvas');
// Native source corner is 9x9. Preserve the cut corner and bolt vocabulary,
// connect the metal planes and center the bolt on the rail axis.
var colors={'101114':'0a0d11','261924':'151423','301c29':'241f3a','392737':'30203e','4f5f72':'615b6b','647385':'615b6b','738497':'958ba1','9aa9b9':'958ba1','adbac9':'ccc3d5','c4d1df':'ccc3d5','eff5fa':'e2dcea','975a27':'985e2b','eec45d':'e5ae4f','a042e4':'9139d9'};
var corner=blank(9,9);
for(var y=0;y<9;y++)for(var x=0;x<9;x++){
 if((x===0||x===8)&&(y===0||y===8))continue;
 var c=colors[hex(raw,2+x,10+y)]||'958ba1';
 if(x===0||y===0||x===8||y===8)c='0a0d11';
 else if(x===1||y===1)c='ccc3d5';
 else if(x===7||y===7)c='615b6b';
 else c='958ba1';
 if(x>=3&&x<=5&&y>=3&&y<=5)c=x===5||y===5?'985e2b':'e5ae4f';
 if(x===3&&y===3)c='ffdb8f';
 pixel(corner,x,y,c);
}
['TL','TR','BL','BR'].forEach(function(n){save(corner,'Resonance_Corner_'+n);});
// Extract the actual source top-rail profile; remap metal to the approved family.
var profile=[];for(var y=11;y<=15;y++)profile.push(colors[hex(raw,20,y)]);
profile[0]=profile[4]='0a0d11';profile[1]='ccc3d5';
function rails(prefix,size,prof){var a=blank(16,size),b=blank(size,16);for(var i=0;i<size;i++)for(var k=0;k<16;k++){pixel(a,k,i,prof[i]);pixel(b,i,k,prof[i]);}save(a,prefix+'_Rail_Top');save(a,prefix+'_Rail_Bottom');save(b,prefix+'_Rail_Left');save(b,prefix+'_Rail_Right');}
rails('Resonance',5,profile);
var bolt=blank(7,7);for(var y=0;y<7;y++)for(var x=0;x<7;x++){if((x===0||x===6)&&(y===0||y===6))continue;var c=x===0||y===0||x===6||y===6?'0a0d11':x===1||y===1?'ccc3d5':x===5||y===5?'615b6b':'958ba1';if(x===3&&y===3)c='e5ae4f';pixel(bolt,x,y,c);}save(bolt,'Resonance_Clamp');
function flat(name,c){var a=blank(16,16);for(var y=0;y<16;y++)for(var x=0;x<16;x++)pixel(a,x,y,c);save(a,name);}
flat('Resonance_Inset','151023');flat('Supply_Inset','958ba1');flat('Progress_Inset','151423');
// Compact rail uses the same metal, separate 5px native caps and 3px rails.
var cap=blank(5,5);for(var y=0;y<5;y++)for(var x=0;x<5;x++){if((x===0||x===4)&&(y===0||y===4))continue;pixel(cap,x,y,x===0||y===0||x===4||y===4?'0a0d11':x===1||y===1?'ccc3d5':'615b6b');}
['TL','TR','BL','BR'].forEach(function(n){save(cap,'Progress_Corner_'+n);});save(cap,'Progress_Clamp');rails('Progress',3,['0a0d11','958ba1','0a0d11']);
var fill=blank(16,4);var ramp=['dd92f7','b75aeb','9139d9','6e27bf'];for(var y=0;y<4;y++)for(var x=0;x<16;x++)pixel(fill,x,y,ramp[y]);save(fill,'Progress_Fill');
// Small arcane ornaments: source's violet diamond plus connected native sparks.
var motif=blank(19,17);var mask=['...................','...................','........#..........','.......#.#.........','......#...#........','...###.....##......','..#...#...#..##....','.#.....#.#....#....','#.......#......###.','.#.....#.#....#....','..#...#...#..##....','...###.....##......','......#...#........','.......#.#.........','........#..........','...................','...................'];
for(var y=0;y<17;y++)for(var x=0;x<19;x++)if(mask[y][x]==='#')pixel(motif,x,y,x<9&&y<8?'9139d9':'4a1590');save(motif,'Resonance_Motif');
console.log('STAGE13_COMPLETE 23 native bottom HUD components');
