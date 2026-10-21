// Pixel edits run inside LibreSprite; source PNGs and approved stages are read-only.
var R='ArtSource/DungeonHybrid/Stage04/', c=app.pixelColor;
var maps={
Torch:{'3b4053':'424c60','060606':'0a0d11','f78818':'f78818','e24f0c':'e24f0c','292c37':'2b313a','1d1c21':'2b313a','feea9c':'feea9c','121213':'0a0d11','526177':'65748a','6d8094':'65748a','010101':'0a0d11','5e7085':'65748a','485166':'424c60','8395a7':'93a1b0','fdbe48':'fdbe48','723d2d':'7a4d2e'},
Chain:{'060607':'0a0d11','323449':'302940','494b65':'4e435c','12121a':'241f3a','71708a':'776784','9d9dbd':'a096aa','010000':'0a0d11','babbd8':'a096aa'},
Banner:{'712d4d':'644064','531f3a':'482c50','0c0c10':'0a0d11','cf9e51':'a78857','301924':'382642','3e1e2b':'382642','974662':'7b526f','656e76':'55606e','e1c189':'c4a773','754633':'7a4d2e','434850':'2b313a','9da0a4':'93a1b0'},
Barrel:{'734023':'7a4d2e','925529':'98633e','b2722f':'b07a43','5b311e':'4a2c1c','101012':'0a0d11','3b221b':'4a2c1c','756374':'55606e','4d4251':'2b313a','332e38':'2b313a','8b7987':'55606e','ca8b3e':'b07a43','b29fab':'93a1b0','cbbbc2':'93a1b0','e6b560':'c49b65'},
Jar:{'8c7485':'67537d','51424e':'4e3d6c','0c0f11':'0a0d11','6d5865':'67537d','c0a9bc':'a096aa','5a4855':'4e3d6c','7b6474':'67537d','3f313d':'302940','281d28':'302940','63515d':'4e3d6c','977f90':'83718d','a78c9d':'83718d'},
Bones:{'fbf0de':'d5cbb5','b39d8c':'9e927f','0d1214':'0a0d11','f8e7cd':'d5cbb5','090b0c':'0a0d11','866954':'807263','9d836f':'807263','725641':'50463d','4c362a':'50463d','d8bea1':'b6aa92'},
Rubble:{'555065':'4e3d6c','372e49':'302940','a49eb1':'83718d','01000f':'0a0d11','06060d':'0a0d11','9691a5':'83718d','6e687f':'67537d','29223a':'302940'}
};
function hex(v){function h(n){return ('0'+n.toString(16)).slice(-2);}return h(c.rgbaR(v))+h(c.rgbaG(v))+h(c.rgbaB(v));}
function color(h){return c.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
function blank(w,h){return {w:w,h:h,p:[]};}
function get(a,x,y){return x>=0&&y>=0&&x<a.w&&y<a.h?(a.p[y*a.w+x]||0):0;}
function put(a,x,y,v){if(x<0||y<0||x>=a.w||y>=a.h)throw Error('Out of bounds');a.p[y*a.w+x]=v;}
function read(n){
 var doc=app.open(R+'Sources/'+n+'_Raw.png'),s=doc.sprite,im=s.layer(0).cel(0).image,a=blank(s.width,s.height);
 for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var v=im.getPixel(x,y);if(c.rgbaA(v)){var h=hex(v);if(!maps[n][h])throw Error('Unmapped '+n+' '+h);v=color(maps[n][h]);}else v=0;put(a,x,y,v);im.putPixel(x,y,v);}
 s.layer(0).name=n+' - deliberate material ramps';s.commit();s.saveAs(R+'Native/'+n+'_Cleanup.aseprite',true);s.saveAs(R+'Previews/'+n+'_Clean64.png',true);return a;
}
// Read the existing 128px source only as an empty editing canvas. Never save it.
function save(a,n){
 var doc=app.open('ArtSource/DungeonHybrid/Stage02/Sources/Gate_Raw_PixelLab.png'),s=doc.sprite,im=s.layer(0).cel(0).image;
 if(a.w>s.width||a.h>s.height)throw Error('Canvas too small');
 for(var y=0;y<s.height;y++)for(var x=0;x<s.width;x++)im.putPixel(x,y,get(a,x,y));
 s.layer(0).name=n;s.commit();s.saveAs('.utmp/hybrid-stage04/'+n+'.png',true);
}
function crop(a,x,y,w,h,pad){var b=blank(w+pad*2,h+pad*2);for(var yy=0;yy<h;yy++)for(var xx=0;xx<w;xx++)put(b,xx+pad,yy+pad,get(a,x+xx,y+yy));return b;}
var raw={};Object.keys(maps).forEach(function(n){raw[n]=read(n);});
save(crop(raw.Torch,20,8,20,50,2),'Torch');
save(crop(raw.Banner,15,8,33,48,2),'Banner');
save(crop(raw.Barrel,13,16,36,41,2),'Barrel');
save(crop(raw.Jar,13,21,36,33,2),'Jar');
save(crop(raw.Rubble,10,28,44,25,2),'Rubble');
// Link exits intentionally reach modular connection edges. The completed chain
// has two clear pixels on all four sides; no source pixel is cropped.
var anchor=blank(14,25),repeat=blank(14,14),end=blank(14,14);
for(var y=0;y<23;y++)for(var x=0;x<10;x++)put(anchor,x+2,y+2,get(raw.Chain,x+27,y+1));
for(var y=0;y<14;y++)for(var x=0;x<10;x++)put(repeat,x+2,y,get(raw.Chain,x+27,y+24));
for(var y=0;y<12;y++)for(var x=0;x<10;x++)put(end,x+2,y,get(raw.Chain,x+27,y+52));
save(anchor,'Chain_Anchor');save(repeat,'Chain_Repeat');save(end,'Chain_End');
// Separate the naturally disconnected skull and fat bones by 8-connected mask,
// preserving every original opaque pixel, including their overlapping X bounds.
var seen={},groups=[];
for(var y=0;y<64;y++)for(var x=0;x<64;x++)if(get(raw.Bones,x,y)&&!seen[y*64+x]){
 var q=[[x,y]],pts=[],minx=x,miny=y,maxx=x,maxy=y;seen[y*64+x]=true;
 while(q.length){var p=q.pop(),xx=p[0],yy=p[1];pts.push(p);minx=Math.min(minx,xx);miny=Math.min(miny,yy);maxx=Math.max(maxx,xx);maxy=Math.max(maxy,yy);
 for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){var nx=xx+dx,ny=yy+dy,k=ny*64+nx;if(get(raw.Bones,nx,ny)&&!seen[k]){seen[k]=true;q.push([nx,ny]);}}}
 groups.push({pts:pts,x:minx,y:miny,w:maxx-minx+1,h:maxy-miny+1});
}
if(groups.length!==2)throw Error('Expected isolated skull and bone groups');
groups.sort(function(a,b){return a.x-b.x;});
groups.forEach(function(g,i){var a=blank(g.w+4,g.h+4);g.pts.forEach(function(p){put(a,p[0]-g.x+2,p[1]-g.y+2,get(raw.Bones,p[0],p[1]));});save(a,i===0?'Skull':'Bones');});
