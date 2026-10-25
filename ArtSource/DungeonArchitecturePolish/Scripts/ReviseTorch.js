
// Native LibreSprite correction. The approved upper-HUD torch remains untouched.
var R='ArtSource/DungeonArchitecturePolish/',c=app.pixelColor;
function color(h){return c.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
function hex(v){function h(n){return ('0'+n.toString(16)).slice(-2);}return h(c.rgbaR(v))+h(c.rgbaG(v))+h(c.rgbaB(v));}
var s=app.open('ArtSource/DungeonHybrid/Stage04/Exports/Torch.png').sprite,im=s.layer(0).cel(0).image,a=[];
for(var y=0;y<54;y++)for(var x=0;x<24;x++)a[y*24+x]=im.getPixel(x,y);
var fs=app.open('ArtSource/DungeonPixelLab/Exports/Stage07/Torch_Flame.png').sprite,flame=fs.layer(0).cel(0).image;
for(var y=0;y<54;y++)for(var x=0;x<24;x++)im.putPixel(x,y,0);
// Preserve the wrought-iron bowl; replace only its curled lower hanger and tiny flame.
for(var y=17;y<=32;y++)for(var x=0;x<24;x++)im.putPixel(x,y+8,a[y*24+x]);
var fire={'bc5426':'e24f0c','e97e27':'f78818','f6b739':'fdbe48','fff09a':'feea9c'};
for(var y=4;y<26;y++)for(var x=7;x<25;x++){var v=flame.getPixel(x,y);if(c.rgbaA(v))im.putPixel(x-3,y-1,color(fire[hex(v)]));}
// A compact straight wall pin with a restrained upper-left iron highlight.
var iron=['0a0d11','65748a','424c60','424c60','2b313a','0a0d11'];
for(var y=41;y<49;y++)for(var x=10;x<16;x++)im.putPixel(x,y,color(iron[x-10]));
for(var x=10;x<16;x++)im.putPixel(x,49,color('0a0d11'));
for(var x=11;x<15;x++)im.putPixel(x,50,color('0a0d11'));
for(var x=12;x<14;x++)im.putPixel(x,51,color('0a0d11'));
s.layer(0).name='Reference-style flame, iron bowl and straight wall bracket';s.commit();
s.saveAs(R+'Native/Torch_ReferenceStyle.aseprite',true);s.saveAs(R+'Exports/Torch_ReferenceStyle.png',true);
