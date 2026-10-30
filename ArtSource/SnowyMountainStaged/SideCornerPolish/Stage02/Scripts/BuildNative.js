var root=__ROOT__;
var pc=app.pixelColor;
function rgba(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
var maps={
Left:{'5d8096':'577993','ebf9fc':'ecfafd','35546a':'35526c','b1d1de':'b8d9e8','3c2d27':'3d2c27','614c3c':'624937','01070e':'0a0d11','010207':'0a0d11','02040b':'0a0d11','11202e':'192330','93b8ca':'b8d9e8','82a7bb':'668fa6','233544':'1d3245','2c475b':'2b445e','05101b':'0a0d11','545250':'555051','4b3c34':'4d382d','46667d':'35526c','7f6c5b':'887468','c7e5ee':'b8d9e8'},
Right:{'e8f8fb':'ecfafd','2f485e':'35526c','afd3e2':'b8d9e8','000205':'0a0d11','3b312c':'3d2c27','595b59':'555051','597d92':'577993','696053':'624937','423a36':'302627','614f41':'624937','503c31':'4d382d','000003':'0a0d11','132431':'192330','755b46':'624937','152e3f':'1d3245','03101c':'0a0d11','87705d':'887468','899fab':'668fa6','987e68':'887468','436479':'35526c'}
};
function raw(file,map){app.open(root+'Raw/'+file);var im=app.activeSprite.layer(0).cel(0).image,a=[];for(var y=0;y<142;y++){var row=[];for(var x=0;x<44;x++){var c=x<40&&y<128?im.getPixel(x,y):0;if(pc.rgbaA(c)){var h=hex(c);if(!map[h])throw 'Unknown material '+h;c=rgba(map[h]);}else c=0;row.push(c);}a.push(row);}app.activeDocument.close();return a;}
function polygon(a,pts,h){var v=rgba(h);for(var y=0;y<142;y++)for(var x=0;x<40;x++){var inside=false;for(var i=0,j=pts.length-1;i<pts.length;j=i++){var xi=pts[i][0],yi=pts[i][1],xj=pts[j][0],yj=pts[j][1];if(((yi>y)!=(yj>y))&&(x<(xj-xi)*(y-yi)/(yj-yi)+xi))inside=!inside;}if(inside)a[y][x]=v;}}
function patch(a,side){
 if(side==='Left'){
  // Remove the unrelated tiny faceted sphere; reconnect the existing rock plane.
  polygon(a,[[21,78],[29,73],[34,77],[34,91],[24,96],[20,89]],'577993');
  polygon(a,[[29,73],[34,77],[34,91],[24,96],[26,83]],'35526c');
 }else{
  // Quiet shadowed cliff behind the open brace; no floating rock shelves.
  for(var y=25;y<108;y++){
   var edge=y<44?24+Math.floor((y-25)/3):y<76?31:29-Math.floor((y-76)/12);
   for(var x=5;x<=edge;x++)if(!pc.rgbaA(a[y][x]))a[y][x]=rgba(x===edge?'0a0d11':x>edge-4?'35526c':'1d3245');
  }
 }
 // Finish the cut base as a shallow grounded rock mass. Never resample source pixels.
 for(var y=128;y<137;y++){
  var inset=Math.floor((y-128)/2),lo=inset,hi=39-inset;
  for(var x=lo;x<=hi;x++)a[y][x]=rgba((x===lo||x===hi||y===136)?'0a0d11':x<18&&y<132?'577993':y>133?'1d3245':'35526c');
 }
}
function save(name,a,side,start,height,joinTop,joinBottom){
 app.open(root+'Raw/Blank44x'+(height+4)+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
 for(var y=0;y<height;y++)for(var x=-1;x<=40;x++){
  var c=x>=0&&x<40?a[start+y][x]:0;
  if(x===-1&&pc.rgbaA(a[start+y][0]))c=rgba('0a0d11');
  if(x===40&&pc.rgbaA(a[start+y][39]))c=rgba('0a0d11');
  // Preserve the sculpted PixelLab stone/snow planes through the module face.
  // Structural connections use native overlapping slices, not flat collar bars.
  im.putPixel(x+2,y+2,c);
 }
 s.layer(0).name='PixelLab native shapes; material cleanup and authored structural joins';s.commit();
 s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();console.log('MODULE '+name+' PASS');
}
try{
 var left=raw('Left_0.png',maps.Left),right=raw('Right_1.png',maps.Right);
 patch(left,'Left');patch(right,'Right');
 for(var q=0;q<2;q++){
  var side=q?'Right':'Left',a=q?right:left;
  save(side+'_Complete',a,side,0,138,false,false);
  save(side+'_Upper',a,side,0,56,false,true);
  save(side+'_MiddleA',a,side,40,56,true,true);
  save(side+'_MiddleB',a,side,56,56,true,true);
  save(side+'_Foot',a,side,90,48,true,false);
 }
 console.log('SNOW_SIDE_POLISH_NATIVE_PASS');
}catch(e){console.log('SNOW_SIDE_POLISH_FAIL '+e);}
app.command.Exit();
