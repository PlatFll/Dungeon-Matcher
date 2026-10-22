var R='ArtSource/DungeonHybrid/SideMasonry/',c=app.pixelColor;
var map={'0c0914':'151423','1d172b':'241f3a','272038':'302940','362e49':'382d55','443b58':'382d55','534a69':'4e3d6c','5d5372':'4e3d6c','645b7a':'4e3d6c','6d6383':'67537d','7d7394':'67537d','8f84a5':'67537d','b5acc8':'846a95'};
function hex(v){function h(n){return ('0'+n.toString(16)).slice(-2);}return h(c.rgbaR(v))+h(c.rgbaG(v))+h(c.rgbaB(v));}
function color(h){return c.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
var doc=app.open(R+'Sources/Masonry_Raw.png'),s=doc.sprite,im=s.layer(0).cel(0).image,p=[];
for(var y=0;y<128;y++)for(var x=0;x<128;x++){var v=im.getPixel(x,y);if(c.rgbaA(v)){var h=hex(v);if(!map[h])throw Error('Unmapped '+h);v=color(map[h]);}else v=0;p[y*128+x]=v;im.putPixel(x,y,v);}
s.layer(0).name='Dungeon stone - seven controlled material colors';s.commit();s.saveAs(R+'Native/Masonry_Cleanup.aseprite',true);s.saveAs(R+'Previews/Masonry_Clean128.png',true);
function save(name,sx,sy,w,h,padx,pady,mirror){
 var d=app.open(R+'Sources/Masonry_Raw.png'),t=d.sprite,a=t.layer(0).cel(0).image;
 for(var y=0;y<128;y++)for(var x=0;x<128;x++)a.putPixel(x,y,0);
 for(var y=0;y<h;y++)for(var x=0;x<w;x++)a.putPixel(padx+(mirror?w-1-x:x),pady+y,p[(sy+y)*128+sx+x]);
 t.layer(0).name=name;t.commit();t.saveAs('.utmp/hybrid-masonry/'+name+'.png',true);
}
// A paired surround uses inward-lit edges and shared top-lit stone planes.
// Both sides preserve the generated silhouette; only the palette changes.
save('Masonry_Base_Right',33,23,44,82,2,2,false);
save('Masonry_Base_Left',33,23,44,82,2,2,true);
// Two complete upper stone courses. Connection edges touch; outer sides retain padding.
save('Masonry_Repeat_Right',57,36,20,16,2,0,false);
save('Masonry_Repeat_Left',57,36,20,16,2,0,true);
