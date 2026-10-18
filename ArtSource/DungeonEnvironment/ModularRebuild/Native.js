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
 // The native batch export reopens this document; independent QA decodes every cel.
 console.log('ASSET_OK '+group+'/'+name);
}
