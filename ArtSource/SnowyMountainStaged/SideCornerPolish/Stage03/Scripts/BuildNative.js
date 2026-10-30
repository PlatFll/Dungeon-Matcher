// All material edits and new drawing pixels are authored in LibreSprite.
var root=__ROOT__,pc=app.pixelColor;
function rgba(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
function hex(c){return ('0'+pc.rgbaR(c).toString(16)).slice(-2)+('0'+pc.rgbaG(c).toString(16)).slice(-2)+('0'+pc.rgbaB(c).toString(16)).slice(-2);}
var maps={
Left:{'567992':'577993','eaf9fd':'ecfafd','afd2e2':'b8d9e8','010305':'0a0d11','365269':'35526c','000102':'0a0d11','39556d':'35526c','76adca':'649fbe','675647':'624937','3e5b73':'35526c','44332b':'4d382d','c3e2ed':'b8d9e8','382d2b':'3d2c27','fdfefd':'ecfafd','2d465c':'2b445e','58463a':'4d382d','83c4e4':'83bedc','11344c':'1d3245','7198b1':'668fa6','141b22':'192330'},
Right:{'eaf9fd':'ecfafd','385871':'35526c','a5c9da':'b8d9e8','5f88a1':'577993','092a43':'1d3245','030812':'0a0d11','02050b':'0a0d11','0c2e49':'1d3245','f6fdfe':'ecfafd','403531':'3d2c27','1b3950':'2b445e','515050':'555051','161f2a':'192330','244761':'2b445e','041f36':'192330','7fb2cc':'668fa6','826b56':'887468','4e7188':'577993','a0edfb':'c5eafa','6f5541':'624937'}
};
function blank(w,h){var a=[];for(var y=0;y<h;y++){var row=[];for(var x=0;x<w;x++)row.push(0);a.push(row);}return a;}
function read(file,w,h,map){app.open(root+'Raw/'+file);var im=app.activeSprite.layer(0).cel(0).image,a=blank(w,h);for(var y=0;y<h;y++)for(var x=0;x<w;x++){var c=im.getPixel(x,y);if(pc.rgbaA(c)){if(map){var m=map[hex(c)];if(!m)throw 'Unmapped color '+hex(c);c=rgba(m);}a[y][x]=c;}}app.activeDocument.close();return a;}
function poly(a,p,h){var v=rgba(h);for(var y=0;y<a.length;y++)for(var x=0;x<a[0].length;x++){var hit=false;for(var i=0,j=p.length-1;i<p.length;j=i++){if(((p[i][1]>y)!=(p[j][1]>y))&&(x<(p[j][0]-p[i][0])*(y-p[i][1])/(p[j][1]-p[i][1])+p[i][0]))hit=!hit;}if(hit)a[y][x]=v;}}
function put(a,x,y,h){a[y][x]=rgba(h);}
function save(name,a,w,h,ox,oy){app.open(root+'Raw/Blank'+w+'x'+h+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;for(var y=0;y<a.length;y++)for(var x=0;x<a[0].length;x++)im.putPixel(x+ox,y+oy,a[y][x]);s.layer(0).name='Native PixelLab forms; LibreSprite material and contour cleanup';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();console.log('EXPORTED '+name);}
function finishRight(raw){
 var a=blank(100,84);for(var y=0;y<80;y++)for(var x=0;x<96;x++)a[y][x]=raw[y][x];
 // The generated right snowbank meets the source boundary. Complete its tapered
 // outer contour and close the underside in newly added native canvas space.
 var edges=[96,97,98,99,99,98,97,97,97,97,97,97,96,96,96,96,95,95,95,94,94,94,93,93,
  95,97,99,99,99,99,98,97,96,95,94,94,94,93,93,93,92,92,92,
  94,96,98,99,99,99,99,99,99,99,98,97,96,94,93];
 for(var y=22;y<80;y++){
  var edge=edges[y-22],snow=y<28||(y>=46&&y<52)||y>=65;
  for(var x=92;x<100;x++){
   if(x>edge)a[y][x]=0;
   else if(x===edge)a[y][x]=rgba('0a0d11');
   else if(x>=95)a[y][x]=rgba(snow?'b8d9e8':x>=edge-2?'1d3245':'35526c');
  }
 }
 // Preserve all original snow surfaces; only the previously cut bottom closes.
 for(var y=80;y<84;y++){
  var lo=8+(y-80)*3,hi=92-(y-80)*3;
  for(var x=lo;x<=hi;x++)a[y][x]=rgba(y===83||x===lo||x===hi?'0a0d11':y===80?'b8d9e8':'668fa6');
 }
 return a;
}
function apron(side){var a=blank(64,24);
 // Broad sculpted snow planes with a low icy-rock underside, separately editable.
 if(side==='Left'){
 poly(a,[[2,15],[7,12],[12,10],[17,10],[21,5],[27,3],[34,4],[41,8],[45,11],[51,12],[58,16],[62,18],[61,21],[11,22],[3,20]],'0a0d11');
 poly(a,[[3,15],[12,11],[18,11],[22,6],[28,4],[34,5],[41,9],[45,12],[51,13],[59,17],[61,19],[59,20],[12,21],[4,19]],'b8d9e8');
 poly(a,[[4,15],[12,11],[18,11],[22,6],[28,4],[34,5],[39,8],[34,8],[31,7],[25,8],[23,11],[19,14],[12,15],[9,17],[4,17]],'ecfafd');
 poly(a,[[17,19],[25,17],[29,15],[34,16],[38,19],[44,18],[51,18],[59,19],[59,20],[12,21]],'668fa6');
 }else{
 poly(a,[[2,18],[8,14],[14,13],[19,13],[23,10],[31,10],[37,5],[43,3],[49,5],[53,10],[58,12],[61,16],[62,20],[57,22],[9,22],[2,20]],'0a0d11');
 poly(a,[[3,18],[8,15],[14,14],[20,14],[24,11],[32,11],[38,6],[43,4],[48,6],[52,11],[57,13],[60,17],[61,19],[56,21],[9,21],[3,20]],'b8d9e8');
 poly(a,[[4,18],[9,15],[20,14],[24,11],[32,11],[38,6],[43,4],[48,6],[50,9],[45,8],[41,9],[37,13],[30,14],[26,15],[22,16],[15,16],[10,18]],'ecfafd');
 poly(a,[[7,20],[18,19],[23,18],[28,18],[34,19],[42,17],[48,17],[56,20],[55,21],[9,21]],'668fa6');
 }
 return a;
}
try{
 var l=read('SelectedLeft.png',96,80,maps.Left),r=read('SelectedRight.png',96,80,maps.Right);
 save('Corner_Left',l,104,88,4,4);save('Corner_Right',finishRight(r),108,92,4,4);
 for(var i=0;i<2;i++){
  var side=i?'Right':'Left';
  // Lossless approved overlap courses: quiet structural continuation behind cores.
  var c=read('Approved'+side+'Connector.png',44,60,null);
  save('Connector_'+side,c,44,60,0,0);save('SnowApron_'+side,apron(side),64,24,0,0);
 }
 console.log('SNOW_CORNER_NATIVE_PASS');
}catch(e){console.log('SNOW_CORNER_NATIVE_FAIL '+e);}
app.command.Exit();
