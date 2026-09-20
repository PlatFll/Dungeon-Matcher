// Native LibreSprite pass: a connected stance cycle with a blink at compression.
function ix(x,y){return(y*64+x)*4;}
function cp(a,i,b,j){for(var k=0;k<4;++k)b[j+k]=a[i+k];}
function hx(a,i){return('0'+a[i].toString(16)).slice(-2).toUpperCase()+('0'+a[i+1].toString(16)).slice(-2).toUpperCase()+('0'+a[i+2].toString(16)).slice(-2).toUpperCase();}
function ink(a,x,y,h){var i=ix(x,y);a[i]=parseInt(h.substr(0,2),16);a[i+1]=parseInt(h.substr(2,2),16);a[i+2]=parseInt(h.substr(4,2),16);a[i+3]=255;}
function read(s){var frames=[];for(var f=0;f<s.layer(0).celCount;++f){var c=s.layer(0).cel(f),raw=c.image.getImageData(),a=new Uint8Array(16384);for(var y=0;y<c.image.height;++y)for(var x=0;x<c.image.width;++x)cp(raw,(y*c.image.width+x)*4,a,ix(x+c.x,y+c.y));frames.push(a);}return frames;}
function source(path){app.open(ROOT+path);var a=read(app.activeSprite);app.activeDocument.close();return a;}
function write(name,frames){app.open(ROOT+'Originals/BeforeTurnBasedPass/'+name+'_Idle.aseprite');var s=app.activeSprite;s.saveAs(ROOT+'Review/'+name+'_TurnBased.aseprite',false);for(var f=0;f<9;++f)s.layer(0).cel(f).image.putImageData(frames[f]);s.commit();s.save();app.command.GotoFirstFrame();}
function recolor(a,name){for(var i=0;i<a.length;i+=4)if(a[i+3]){var h=MAPS[name][hx(a,i)];if(!h)throw Error(name+' unmapped '+hx(a,i));ink(a,(i/4)%64,Math.floor(i/256),h);}return a;}

var ref=source('Originals/TurnBasedReference/Farmer_OpenReference.aseprite');
// Keep the whole original head, shoulders and torso drawing together. The same
// settling poses play back in reverse on recovery; no late nod is spliced in.
var order=[4,4,5,6,7,8,6,5,4],farmer=[];
var shaftRows={41:[26,28],42:[27,30],43:[28,32],44:[30,34],45:[32,35],46:[34,37],47:[35,39],48:[37,41],49:[39,42]};
for(var f=0;f<9;++f){var a=recolor(new Uint8Array(ref[order[f]]),'Farmer');
 for(var y=0;y<39;++y)for(var x=11;x<55;++x){var i=ix(x,y),h=hx(a,i);if(!a[i+3])continue;
  if(y<23&&h==='B07A43')ink(a,x,y,'B88A46');
  if(y<28&&'E6B08A B9825D 7C5238'.indexOf(h)>=0)ink(a,x,y,y<25?'E7C979':'B88A46');
  if(y>=27&&y<33&&h==='E6B08A'&&'0A0D11 7A5A2C 7C5238'.indexOf(hx(a,ix(x,y-1)))>=0)ink(a,x,y,'B9825D');
 }
 for(var ys in shaftRows){var span=shaftRows[ys];for(var x=span[0];x<=span[1];++x){var h=hx(a,ix(x,+ys));if(h==='7C5238')ink(a,x,+ys,'7A4D2E');else if(h==='B9825D')ink(a,x,+ys,'B07A43');}}
 // Half-close, close, and reopen with the bottom stance, never after the rise.
 if(f>=4&&f<=6){var top=f===4?30:f===5?31:28,bottom=top+2,height=f===5?1:2;
  var eyes=[[24,25],[34,36]];
  for(var e=0;e<eyes.length;++e)for(var y=top;y<=bottom;++y)for(var x=eyes[e][0];x<=eyes[e][1];++x){
   if(y>bottom-height)ink(a,x,y,'0A0D11');else if(hx(a,ix(x,y))==='0A0D11')ink(a,x,y,y===top?'B9825D':'E6B08A');
  }
 }
 farmer.push(a);
}
write('Farmer',farmer);

var base=source('Originals/BeforeTurnBasedPass/PanVillager_Idle.aseprite')[0];
var upper=[0,0,1,2,3,3,2,1,0],waist=[0,0,1,1,2,2,1,0,0],head=[0,0,1,2,4,5,4,2,0];
var prop={},iron={};
for(var y=0;y<64;++y)for(var x=0;x<64;++x){var i=ix(x,y);if(base[i+3]&&'2B313A 55606E 93A1B0'.indexOf(hx(base,i))>=0)iron[y*64+x]=true;}
for(var key in iron){var n=+key,x=n%64,y=Math.floor(n/64);prop[n]=true;for(var yy=y-1;yy<=y+1;++yy)for(var xx=x-1;xx<=x+1;++xx){if(xx>=0&&xx<64&&yy>=0&&yy<64&&base[ix(xx,yy)+3]&&hx(base,ix(xx,yy))==='0A0D11')prop[yy*64+xx]=true;}}
// Pan, handle, gripping hand and its arm travel together as one held assembly.
for(var y=33;y<=52;++y)for(var x=36;x<64;++x)if(base[ix(x,y)+3])prop[y*64+x]=true;
var pan=[];
for(var f=0;f<9;++f){var a=new Uint8Array(16384),src=new Uint8Array(base),u=upper[f],w=waist[f],h=head[f];
 if(f>=4&&f<=6){var height=f===5?1:2;
  for(var e=0;e<2;++e){var left=e?28:19;
   for(var y=22;y<=25;++y)for(var x=left;x<=left+5;++x)ink(src,x,y,'E6B08A');
   for(var y=26-height;y<=25;++y)for(var x=left+(y===26-height?0:1);x<=left+(y===26-height?5:4);++x)ink(src,x,y,'0A0D11');
  }
 }
 for(var y=33;y<64;++y){var yy;
  if(y<53)yy=y+u-Math.round((u-w)*(y-33)/20);
  else if(y<60)yy=53+w+Math.floor((y-53)*(7-w)/7);
  else yy=y;
  for(var x=0;x<64;++x){var i=ix(x,y);if(!src[i+3]||prop[y*64+x])continue;cp(src,i,a,ix(x,yy));}
 }
 for(var key in prop){var n=+key,x=n%64,y=Math.floor(n/64);if(y+u<64)cp(src,n*4,a,ix(x,y+u));}
 // The scarf, face, hair and jaw stay one head assembly; neck space compresses.
 for(var y=0;y<=32;++y)for(var x=0;x<64;++x)if(src[ix(x,y)+3])cp(src,ix(x,y),a,ix(x,y+h));
 pan.push(a);
}
write('PanVillager',pan);app.command.PlayAnimation();
