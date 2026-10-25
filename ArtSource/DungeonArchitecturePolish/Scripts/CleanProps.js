
var R='ArtSource/DungeonArchitecturePolish/',c=app.pixelColor;
function color(h){return c.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
function hex(v){function h(n){return ('0'+n.toString(16)).slice(-2);}return h(c.rgbaR(v))+h(c.rgbaG(v))+h(c.rgbaB(v));}
function clean(src,name,map,move){var s=app.open(R+'Sources/'+src+'.png').sprite,im=s.layer(0).cel(0).image,w=s.width,h=s.height,a=[],seen={},groups=[];
 for(var y=0;y<h;y++)for(var x=0;x<w;x++){var v=im.getPixel(x,y),i=y*w+x;a[i]=c.rgbaA(v)?color(map[hex(v)]):0;}
 // Remove only disconnected export specks; keep the connected wax group intact.
 if(move){for(var i=0;i<a.length;i++)if(a[i]&&!seen[i]){var stack=[i],g=[];seen[i]=1;while(stack.length){var t=stack.pop();g.push(t);var xx=t%w,yy=Math.floor(t/w);for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){var nx=xx+dx,ny=yy+dy,n=ny*w+nx;if(nx>=0&&nx<w&&ny>=0&&ny<h&&a[n]&&!seen[n]){seen[n]=1;stack.push(n);}}}groups.push(g);}
 groups.sort(function(a,b){return b.length-a.length;});var keep={};for(var k=0;k<groups[0].length;k++)keep[groups[0][k]]=1;for(var i=0;i<a.length;i++)if(!keep[i])a[i]=0;
 var minx=w,maxx=0,miny=h;for(var i=0;i<a.length;i++)if(a[i]){minx=Math.min(minx,i%w);maxx=Math.max(maxx,i%w);miny=Math.min(miny,Math.floor(i/w));}var ox=Math.floor((w-(maxx-minx+1))/2)-minx,oy=2-miny;
 for(var y=0;y<h;y++)for(var x=0;x<w;x++){var xx=x-ox,yy=y-oy;im.putPixel(x,y,xx>=0&&xx<w&&yy>=0&&yy<h?a[yy*w+xx]:0);}
 }else{for(var y=0;y<h;y++)for(var x=0;x<w;x++)im.putPixel(x,y,a[y*w+x]);}
 s.layer(0).name=name+' - native material cleanup';s.commit();s.saveAs(R+'Native/'+name+'.aseprite',true);s.saveAs(R+'Exports/'+name+'.png',true);
}
clean('SkullMain','SkullRelief',{'08041b':'151423','0c0622':'151423','170e2c':'241f3a','2b2640':'302940','433857':'382d55','544869':'4e3d6c','5d5072':'4e3d6c','736387':'67537d','806f97':'67537d','8e7ba6':'67537d'},false);
clean('CandleTall','Candles_Tall',{'130e0d':'151423','2e1f18':'302940','543e2f':'725e47','725e47':'725e47','b5a384':'b5a384','ddc9a5':'ddc9a5','f4e6c4':'f4e6c4','fcf5e2':'f4e6c4'},true);
clean('CandleMixed','Candles_Mixed',{'130e0d':'151423','402b20':'302940','5d4836':'725e47','705c45':'725e47','dcc8a4':'ddc9a5','e9d9b5':'ddc9a5','f4e5c2':'f4e6c4','fefaed':'f4e6c4'},true);
clean('CandleShort','Candles_Short',{'130e0d':'151423','261914':'302940','4f392a':'725e47','79644c':'725e47','c1ae8d':'b5a384','dfcba6':'ddc9a5','f4e5c1':'f4e6c4','fffbf1':'f4e6c4'},true);
