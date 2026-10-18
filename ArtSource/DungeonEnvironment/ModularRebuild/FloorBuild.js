var P='__PROJECT__/',K=P+'ArtSource/DungeonEnvironment/ModularRebuild/';
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=blank(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function crop(a,x,y,w,h){var b=blank(w,h);for(var Y=0;Y<h;Y++)for(var X=0;X<w;X++)for(var k=0;k<4;k++)b.p[(Y*w+X)*4+k]=a.p[((y+Y)*a.w+x+X)*4+k];return b;}
function copy(a,b,x,y){for(var Y=0;Y<b.h;Y++)for(var X=0;X<b.w;X++){var i=(Y*b.w+X)*4,j=((y+Y)*a.w+x+X)*4;if(x+X<0||x+X>=a.w||y+Y<0||y+Y>=a.h||!b.p[i+3])continue;for(var k=0;k<4;k++)a.p[j+k]=b.p[i+k];}}
function px(a,x,y,c){if(x<0||y<0||x>=a.w||y>=a.h)return;var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=c?parseInt(c.substr(2*k,2),16):0;a.p[i+3]=c?255:0;}
function color(a,x,y){var s='',i=(y*a.w+x)*4;for(var k=0;k<3;k++)s+=('0'+a.p[i+k].toString(16)).slice(-2);return s;}
function rect(a,x,y,w,h,c){for(var Y=y;Y<y+h;Y++)for(var X=x;X<x+w;X++)px(a,X,Y,c);}
function line(a,x,y,X,Y,c){var n=Math.max(Math.abs(X-x),Math.abs(Y-y));for(var t=0;t<=n;t++)px(a,Math.round(x+(X-x)*t/Math.max(n,1)),Math.round(y+(Y-y)*t/Math.max(n,1)),c);}
function save(a,group,name){
 var path=K+'Editable/'+group+'/'+name+'.aseprite';app.open(P+'Assets/_Game/Resources/UI/Consumables/Potion.png');var s=app.activeSprite;
 s.saveAs(path,false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();s.layer(0).cel(0).image.putImageData(a.p);s.layer(0).name=name;s.commit();s.saveAs(path,true);app.activeDocument.close();
 var b=read(path);for(var i=0;i<a.p.length;i++)if(a.p[i]!==b.p[i])throw Error('Reopen mismatch '+name);
 console.log('ASSET_OK '+group+'/'+name);
}
var floor=read(P+'Assets/_Game/Resources/UI/BattlePresentation/ReferenceDungeon/BattleFloor.png');
var source=crop(floor,64,34,64,64);
// Preserve the interior slab geometry; reconstruct only the seam bands. The
// top/bottom crop cuts two different perspective courses, so their 3px join
// becomes a shared mortar course. All variants retain the same edge signature.
var common=crop(source,0,0,64,64);
for(var y=0;y<64;y++){
 var edge=color(source,32,y);
 for(var x=0;x<4;x++){px(common,x,y,edge);px(common,63-x,y,edge);}
}
rect(common,0,0,64,2,'29233e');rect(common,0,63,64,1,'29233e');
line(common,0,2,63,2,'5a4169');
// Continue the mortar heights across the four-pixel seam-repair bands.
for(var i=0;i<4;i++){var y=[11,23,36,49][i];rect(common,0,y-1,4,2,'29233e');rect(common,60,y-1,4,2,'29233e');}
for(var v=0;v<4;v++){
 var a=crop(common,0,0,64,64);
 // Cyclic native offsets retain each original slab contour while moving the
 // joins between variations. Only the outer seam bands are reconstructed.
 for(var y=3;y<63;y++)for(var x=4;x<60;x++)for(var c=0;c<4;c++)a.p[(y*64+x)*4+c]=source.p[(y*64+(x+v*16)%64)*4+c];
 // Quiet material wear is confined to large existing face planes. No new
 // slab outline is introduced; variants differ only in interior shading.
 var specs=[[[12,7,10],[40,18,7]],[[25,29,9],[8,43,8]],[[43,57,9],[19,18,7]],[[8,57,10],[42,43,7]]][v];
 for(var i=0;i<specs.length;i++){var q=specs[i],base=color(a,q[0],q[1]);for(var x=q[0];x<q[0]+q[2];x++)if(color(a,x,q[1])===base)px(a,x,q[1],q[1]>35?'4d385e':'382b4d');}
 save(a,'FloorTiles','Floor'+String.fromCharCode(65+v));
}
console.log('FLOOR_COMPLETE');
