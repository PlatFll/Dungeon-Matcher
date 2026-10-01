var ROOT='__ART_ROOT__UI/';
var HEX=['060709','0a0d11','151423','241f3a','3a2d51','4e3d6c','67537e','846a95','aa8bb9','653285','8c44b3','b466d9','e5a9f4','ffe0ff'];
var colors=HEX.map(function(h){return [parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255];});
app.open(ROOT+'ButtonLargeNormal.png');var s=app.activeSprite;var p=new Uint8Array(s.layer(0).cel(0).image.getImageData());
for(var j=0;j<p.length;j+=4){if(!p[j+3])continue;var best=0,dist=1e9;
 if(p[j]===76&&p[j+1]===59&&p[j+2]===103)best=3;
 else if(p[j]>p[j+2]*1.1){best=p[j]>180?7:p[j]>100?6:4;}
 else for(var c=0;c<colors.length;c++){var d=0;for(var k=0;k<3;k++)d+=Math.pow(p[j+k]-colors[c][k],2);if(d<dist){dist=d;best=c;}}
 for(var k=0;k<4;k++)p[j+k]=colors[best][k];}
s.layer(0).cel(0).image.putImageData(p);s.layer(0).name='Purple frame and blank face';s.commit();s.saveAs(ROOT+'ButtonLargeNormal.aseprite',false);s.saveAs(ROOT+'ButtonLargeNormal.png',false);app.activeDocument.close();app.command.Exit();
