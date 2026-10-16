function inside(x,y,poly){var b=false;for(var i=0,j=poly.length-1;i<poly.length;j=i++){var a=poly[i],c=poly[j];if((a[1]>y)!=(c[1]>y)&&x<(c[0]-a[0])*(y-a[1])/(c[1]-a[1])+a[0])b=!b;}return b;}
function hex(p,i){var s='';for(var k=0;k<3;k++){var h=p[i+k].toString(16);s+=(h.length<2?'0':'')+h;}return s;}
function clean(name,height){
 app.open(ROOT+'before/'+name+'.aseprite');var s=app.activeSprite;
 if(s.width!==360||s.height!==height)throw 'Unexpected source dimensions';
 var c=s.layer(0).cel(0),data=c.image.getImageData(),source=new Uint8Array(data.length);source.set(data);
 if(c.x!==0||c.y!==0||c.image.width!==360||c.image.height!==height)throw 'Unexpected cel bounds';
 for(var n=0;n<patches.length;n++){
  var q=patches[n];
  for(var y=0;y<640;y++)for(var x=0;x<360;x++){
   if(!inside(x+.5,y+.5,q.points))continue;var i=(y*360+x)*4;
   if(q.allowed.indexOf(hex(source,i))<0)continue;
   var edge=false;
   for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){
    if(x+dx<0||x+dx>=360||y+dy<0||y+dy>=height){edge=true;continue;}
    if(hex(source,((y+dy)*360+x+dx)*4)==='110624')edge=true;
   }
   if(edge)continue;
   // The original deepest seams and other materials never enter allowed lists.
   for(var k=0;k<3;k++)data[i+k]=parseInt(q.color.substr(k*2,2),16);
  }
 }
 c.image.putImageData(data);s.commit();s.saveAs(ROOT+STAGE+'/'+name+'.aseprite',false);
 app.activeDocument.close();app.open(ROOT+STAGE+'/'+name+'.aseprite');
 app.activeSprite.saveAs(ROOT+STAGE+'/'+name+'.png',false);app.activeDocument.close();
}
try{clean('Hall',640);clean('HallTall',800);console.log('MENU_PATCHES '+JSON.stringify(patches));console.log('MENU_CLEANUP_V2_OK');}catch(e){console.log('MENU_CLEANUP_V2_FAILED '+e);}
app.command.Exit();
