// Native LibreSprite pixel edits. Explicit per-material color mappings; no quantizer.
function hexAt(a,i){return ('0'+a[i].toString(16)).slice(-2).toUpperCase()+('0'+a[i+1].toString(16)).slice(-2).toUpperCase()+('0'+a[i+2].toString(16)).slice(-2).toUpperCase();}
function setHex(a,i,h){a[i]=parseInt(h.substr(0,2),16);a[i+1]=parseInt(h.substr(2,2),16);a[i+2]=parseInt(h.substr(4,2),16);a[i+3]=255;}
function readCanvas(cel){
 var out=new Uint8Array(64*64*4),a=cel.image.getImageData(),w=cel.image.width,h=cel.image.height;
 for(var y=0;y<h;++y)for(var x=0;x<w;++x){var i=(y*w+x)*4,j=((y+cel.y)*64+x+cel.x)*4;for(var k=0;k<4;++k)out[j+k]=a[i+k];}
 return out;
}
// Approved Bardley reference is 12px lower; read its shading in the ORIGINAL animation alignment.
app.open(ROOT+'References/Bardley_Palette_Reference_Placement_Pending.png');
var refLower=readCanvas(app.activeSprite.layer(0).cel(0));
var bardRef=new Uint8Array(16384);
for(var y=0;y<52;++y)for(var x=0;x<64;++x)for(var k=0;k<4;++k)bardRef[(y*64+x)*4+k]=refLower[((y+12)*64+x)*4+k];
app.activeDocument.close();
function material(h,x,y){
 if('176747 4A9B3F 86C83C D5FFAD'.indexOf(h)>=0) return y>=47?'base':x<19?'hand':'slime';
 if('450635 7F1650 BD2169'.indexOf(h)>=0) return y<28?'hat':'cape';
 if('8A5622 C58826 FBEA90'.indexOf(h)>=0) return y<26&&x>18?'hatgold':x<28&&y<44?'pipe':'capegold';
 if('2E568F A9E0FF'.indexOf(h)>=0)return y<28?'hatjewel':'capejewel';
 if('B7A393 FDF5E5'.indexOf(h)>=0)return y<24?'feather':'eye';
 return '';
}
function regionPoints(a){
 var groups={};
 for(var y=0;y<64;++y)for(var x=0;x<64;++x){var i=(y*64+x)*4;if(a[i+3]<128)continue;var h=hexAt(a,i),m=material(h,x,y);if(!m)continue;if(!groups[m])groups[m]=[];groups[m].push({x:x,y:y,h:h});}
 return groups;
}
function bounds(pts){var b=[64,64,0,0];for(var i=0;i<pts.length;++i){b[0]=Math.min(b[0],pts[i].x);b[1]=Math.min(b[1],pts[i].y);b[2]=Math.max(b[2],pts[i].x);b[3]=Math.max(b[3],pts[i].y);}return b;}
var refGroups=regionPoints(bardRef);
function bardleyShading(a){
 // Feather accents are pale feather shadows, never gold; jewel glints stay blue.
 for(var y=0;y<64;++y)for(var x=0;x<64;++x){var i=(y*64+x)*4;if(!a[i+3])continue;var h=hexAt(a,i);
  if(y<23&&x<26&&'8A5622 C58826 FBEA90'.indexOf(h)>=0)setHex(a,i,'B7A393');
  if(h==='FDF5E5'&&((y>=18&&y<=24)||(y>=42&&y<=47)))setHex(a,i,'A9E0FF');
 }
 var groups=regionPoints(a);
 for(var m in groups){
  if(m==='eye'||!refGroups[m])continue;
  var pts=groups[m],rp=refGroups[m],b=bounds(pts),rb=bounds(rp);
  for(var n=0;n<pts.length;++n){var p=pts[n];
   var rx=rb[0]+(p.x-b[0])*Math.max(1,rb[2]-rb[0])/Math.max(1,b[2]-b[0]);
   var ry=rb[1]+(p.y-b[1])*Math.max(1,rb[3]-rb[1])/Math.max(1,b[3]-b[1]);
   var best=1e9,shade=p.h;
   for(var q=0;q<rp.length;++q){var dx=rp[q].x-rx,dy=rp[q].y-ry,d=dx*dx+dy*dy;if(d<best){best=d;shade=rp[q].h;}}
   setHex(a,(p.y*64+p.x)*4,shade);
  }
 }
 return a;
}
for(var name in SOURCES){
 app.open(ROOT+'Originals/'+SOURCES[name]+'.aseprite');
 var spr=app.activeSprite,layer=spr.layer(0),map=MAPS[name];
 spr.saveAs(ROOT+'Review/'+name+'_Recolor.aseprite',false);
 for(var f=0;f<layer.celCount;++f){
  var cel=layer.cel(f),img=cel.image,a=img.getImageData();
  for(var i=0;i<a.length;i+=4){
   if(!a[i+3]){a[i]=a[i+1]=a[i+2]=0;continue;}
   if(name==='Bardley'&&a[i+3]<128){a[i]=a[i+1]=a[i+2]=a[i+3]=0;continue;}
   var old=hexAt(a,i),h=map[old];if(!h)throw Error(name+' unmapped '+old);
   setHex(a,i,h);
  }
  if(name==='Bardley')a=bardleyShading(a);
  img.putImageData(a);
 }
 layer.name='Idle - approved materials';spr.loadPalette(ROOT+'Scripts/'+name+'.gpl');spr.commit();spr.save();
}
