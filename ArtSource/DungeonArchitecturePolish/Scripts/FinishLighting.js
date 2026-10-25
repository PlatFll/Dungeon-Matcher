
var R='ArtSource/DungeonArchitecturePolish/',c=app.pixelColor;
function color(h){return c.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
function hex(v){function h(n){return ('0'+n.toString(16)).slice(-2);}return h(c.rgbaR(v))+h(c.rgbaG(v))+h(c.rgbaB(v));}
function stone(name,strong){var s=app.open(R+'Exports/Shaft.png').sprite,im=s.layer(0).cel(0).image;
 var m=strong?{'4e3d6c':'70474a','67537d':'926258','382d55':'4e3544','302940':'3d2938'}:{'4e3d6c':'5c4057','67537d':'79566a','382d55':'42304b','302940':'352b3c'};
 // Hand-grouped local firelight stays inside the original stone faces and seam geometry.
 for(var y=1;y<13;y++)for(var x=7;x<25;x++){var v=im.getPixel(x,y),key=hex(v);if(c.rgbaA(v)&&m[key])im.putPixel(x,y,color(m[key]));}
 s.layer(0).name='Local torch bounce - native connected stone planes';s.commit();s.saveAs(R+'Native/'+name+'.aseprite',true);s.saveAs(R+'Exports/'+name+'.png',true);}
stone('Shaft_Warm',true);stone('Shaft_Warm_Faint',false);
function end(name,dx){var s=app.open(R+'Exports/Chain_End.png').sprite,im=s.layer(0).cel(0).image,a=[];for(var y=0;y<14;y++)for(var x=0;x<14;x++)a[y*14+x]=im.getPixel(x,y);
 for(var y=0;y<14;y++)for(var x=0;x<14;x++){var sx=x-(y>=4?dx:0);im.putPixel(x,y,sx>=0&&sx<14?a[y*14+sx]:0);}
 s.layer(0).name='Iron loop - subtle natural hanging offset';s.commit();s.saveAs(R+'Native/'+name+'.aseprite',true);s.saveAs(R+'Exports/'+name+'.png',true);}
end('Chain_End_Left',-1);end('Chain_End_Right',1);
