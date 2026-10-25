// Executed by LibreSprite. Raw sources and existing game artwork stay read-only.
var R='ArtSource/DungeonArchitecturePolish/',c=app.pixelColor;
function color(h){return c.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
function hex(v){function h(n){return ('0'+n.toString(16)).slice(-2);}return h(c.rgbaR(v))+h(c.rgbaG(v))+h(c.rgbaB(v));}
var map0={'000002':'151423','020106':'151423','0b0712':'241f3a','241830':'241f3a','2b1e38':'302940','332641':'382d55','3b2f49':'382d55','443852':'4e3d6c','524761':'4e3d6c','685c7b':'67537d','776a88':'67537d','7e728f':'83718d'};
var map2={'000002':'151423','110919':'241f3a','141020':'241f3a','1a0f26':'241f3a','241831':'302940','2a1d38':'302940','342842':'382d55','423650':'4e3d6c','493e57':'4e3d6c','524861':'4e3d6c','766a88':'67537d','7e728f':'83718d'};
function read(n,map){var s=app.open(R+'Sources/'+n+'.png').sprite,im=s.layer(0).cel(0).image,a=[];
 for(var y=0;y<s.height;y++)for(var x=0;x<s.width;x++){var v=im.getPixel(x,y);if(c.rgbaA(v)){var key=hex(v);if(!map[key])throw Error('Unmapped '+key);v=color(map[key]);}else v=0;im.putPixel(x,y,v);a[y*32+x]=v;}
 s.layer(0).name='Stone planes - Dungeon material ramp';s.commit();s.saveAs(R+'Native/'+n+'_Clean.aseprite',true);s.saveAs(R+'Previews/'+n+'_Clean.png',true);return a;}
var raw=read('PillarMain',map0),relief=read('RecessMain',map2);
function save(n,a,sx,sy,w,h){var s=app.open(R+'Sources/PillarMain.png').sprite,im=s.layer(0).cel(0).image;
 for(var y=0;y<128;y++)for(var x=0;x<32;x++)im.putPixel(x,y,x<w&&y<h?a[(sy+y)*32+sx+x]||0:0);
 if((n==='Shaft'||n==='Shaft_Worn'))for(var x=5;x<=26;x++){im.putPixel(x,0,raw[29*32+x]);im.putPixel(x,h-1,color('241f3a'));}
 if(n==='Shaft_Worn'){im.putPixel(24,4,color('302940'));im.putPixel(24,5,color('302940'));}
 s.layer(0).name=n;s.commit();s.saveAs(R+'Previews/'+n+'_Canvas.png',true);}
save('Capital',raw,0,6,32,22);
save('Shaft',raw,0,29,32,15);save('Shaft_Worn',raw,0,29,32,15);
save('Base',raw,0,99,32,23);
save('Ledge',raw,0,16,32,12);
save('Recess',relief,0,26,32,21);
console.log('Stone material mapping and six native architectural parts complete.');
