// Executes inside LibreSprite. All motion edits use whole source pixels.
function pix(a,x,y){return(y*64+x)*4;}
function copyPixel(src,si,dst,di){for(var k=0;k<4;++k)dst[di+k]=src[si+k];}
function hx(a,i){return('0'+a[i].toString(16)).slice(-2).toUpperCase()+('0'+a[i+1].toString(16)).slice(-2).toUpperCase()+('0'+a[i+2].toString(16)).slice(-2).toUpperCase();}
function paint(a,i,h){a[i]=parseInt(h.substr(0,2),16);a[i+1]=parseInt(h.substr(2,2),16);a[i+2]=parseInt(h.substr(4,2),16);a[i+3]=255;}
function readCanvas(cel){
 var out=new Uint8Array(16384),a=cel.image.getImageData(),w=cel.image.width,h=cel.image.height;
 for(var y=0;y<h;++y)for(var x=0;x<w;++x)copyPixel(a,(y*w+x)*4,out,pix(out,x+cel.x,y+cel.y));return out;
}
function copy(a){return new Uint8Array(a);}
function getFrames(name){
 app.open(ROOT+'Review/'+name+'_Recolor.aseprite');
 var l=app.activeSprite.layer(0),out=[];
 for(var f=0;f<l.celCount;++f)out.push(readCanvas(l.cel(f)));
 return out;
}
function swatches(s,name){
 var p=s.palette,cs=PALETTES[name];p.length=cs.length+1;p.set(0,app.pixelColor.rgba(0,0,0,0));
 for(var i=0;i<cs.length;++i){var h=cs[i];p.set(i+1,app.pixelColor.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255));}
}
// Rattlebones remains the gold-standard motion, including all original cels and timings.
app.open(ROOT+'Review/Rattlebones_Recolor.aseprite');
swatches(app.activeSprite,'Rattlebones');app.activeSprite.commit();app.activeSprite.saveAs(ROOT+'Rattlebones_Idle.aseprite',false);
function writeNine(name,frames){
 // The source Farmer import has full 64x64 cels at 130ms, which avoids cropped-cel clipping.
 // Only the new output document is edited; source snapshots are preserved.
 app.open(ROOT+'Originals/FarmerFluidAnim2.aseprite');
 var s=app.activeSprite;s.saveAs(ROOT+name+'_Idle.aseprite',false);
 app.command.GotoLastFrame();app.command.RemoveFrame();app.command.GotoLastFrame();app.command.RemoveFrame();
 var l=s.layer(0);if(l.celCount!==9)throw Error('Expected nine full-canvas cels');
 l.name='Idle - '+name;
 for(var f=0;f<9;++f){var c=l.cel(f);if(c.image.width!==64||c.image.height!==64)throw Error('Unexpected cel dimensions');c.image.putImageData(frames[f]);}
 swatches(s,name);s.commit();s.save();app.command.GotoFirstFrame();
}
function skinAndHat(a){
 // Repair material leakage: a few generated flesh pixels were on the hat.
 for(var y=0;y<28;++y)for(var x=11;x<55;++x){var i=pix(a,x,y),h=hx(a,i);if(a[i+3]&&'E6B08A B9825D 7C5238'.indexOf(h)>=0)paint(a,i,y<25?'E7C979':'B88A46');}
 // Restore the broad shaded skin plane immediately below the brim, plus the right cheek turn.
 for(var y=27;y<39;++y)for(var x=20;x<45;++x){var i=pix(a,x,y);if(!a[i+3]||hx(a,i)!=='E6B08A')continue;
  var up=y>0?hx(a,pix(a,x,y-1)):'';var right=hx(a,pix(a,x+1,y));
  if((y<33&&'0A0D11 7A5A2C 7C5238'.indexOf(up)>=0)||(x>37&&right==='7C5238'))paint(a,i,'B9825D');
 }
 return a;
}
function forkFollow(a,dy){
 if(!dy)return a;
 var out=copy(a),mask={},queue=[],seed=-1;
 // Flood the separated pitchfork head and the first shaft pixels. The hat stays excluded.
 for(var y=23;y<40&&seed<0;++y)for(var x=0;x<11;++x)if(a[pix(a,x,y)+3]){seed=y*64+x;break;}
 if(seed<0)return out;queue.push(seed);mask[seed]=true;
 for(var q=0;q<queue.length;++q){var v=queue[q],x=v%64,y=(v/64)|0;
  for(var yy=y-1;yy<=y+1;++yy)for(var xx=x-1;xx<=x+1;++xx){
   if(xx<0||xx>18||yy<23||yy>=40)continue;var p=yy*64+xx;
   if(!mask[p]&&a[p*4+3]){mask[p]=true;queue.push(p);}
  }
 }
 for(var q=0;q<queue.length;++q){var i=queue[q]*4;out[i]=out[i+1]=out[i+2]=out[i+3]=0;}
 for(var q=0;q<queue.length;++q){var v=queue[q],x=v%64,y=(v/64)|0;
  // Taper to the unchanged grip through the last three shaft columns.
  var d=x<16?dy:0;copyPixel(a,v*4,out,((y+d)*64+x)*4);
 }
 return out;
}
var farmer=getFrames('Farmer'),farmerOut=[],farmerOrder=[4,4,5,0,1,7,8,6,3],forkLag=[0,1,1,1,1,0,0,-1,-1];
for(var f=0;f<9;++f){var a=skinAndHat(copy(farmer[farmerOrder[f]]));a=forkFollow(a,forkLag[f]);
 // Both soles stay exactly on their original contact pixels.
 for(var y=62;y<64;++y)for(var x=0;x<64;++x)copyPixel(farmer[0],pix(a,x,y),a,pix(a,x,y));
 farmerOut.push(a);
}
writeNine('Farmer',farmerOut);

function stance(a,dy,hinge,ground){
 var out=new Uint8Array(16384);
 for(var y=0;y<64;++y){
  var sy=y;
  if(y<hinge+dy)sy=y-dy;
  else if(y<ground)sy=hinge+Math.round((y-hinge-dy)*(ground-hinge)/(ground-hinge-dy));
  if(sy<0||sy>=64)continue;
  for(var x=0;x<64;++x)copyPixel(a,pix(a,x,sy),out,pix(out,x,y));
 }
 return out;
}
function panStance(a,dy){
 var iron={},pan={},body=copy(a);
 for(var y=0;y<64;++y)for(var x=0;x<64;++x){var i=pix(a,x,y);if(a[i+3]&&'2B313A 55606E 93A1B0'.indexOf(hx(a,i))>=0)iron[y*64+x]=true;}
 for(var v in iron){var n=+v,x=n%64,y=(n/64)|0;pan[n]=true;
  for(var yy=y-1;yy<=y+1;++yy)for(var xx=x-1;xx<=x+1;++xx){if(xx<0||xx>=64||yy<0||yy>=64)continue;var q=yy*64+xx;if(a[q*4+3]&&hx(a,q*4)==='0A0D11')pan[q]=true;}
 }
 for(var v in pan){var i=+v*4;body[i]=body[i+1]=body[i+2]=body[i+3]=0;}
 var out=stance(body,dy,52,60);
 // Keep the pan rigid while the stance compresses under it. Never stretch the iron disk.
 for(var v in pan){var n=+v,x=n%64,y=(n/64)|0;if(y+dy<64)copyPixel(a,n*4,out,((y+dy)*64+x)*4);}
 return out;
}
var woman=getFrames('PanVillager'),womanOut=[],womanOrder=[4,5,6,7,8,7,6,5,10],dip=[0,0,1,2,3,3,2,1,0];
for(var f=0;f<9;++f)womanOut.push(panStance(woman[womanOrder[f]],dip[f]));
writeNine('PanVillager',womanOut);

var bard=getFrames('Bardley'),bardOut=[],bardOrder=[4,5,3,3,1,2,0,6,4],bardShift=[0,0,-1,0,0,0,0,0,1];
for(var f=0;f<9;++f){var a=copy(bard[bardOrder[f]]);
 // The pipe-tip glint is gold; pale feather colors belong only to the feather/eyes.
 for(var y=0;y<34;++y)for(var x=0;x<14;++x){var i=pix(a,x,y);if(a[i+3]&&hx(a,i)==='FDF5E5')paint(a,i,'FBEA90');}
 if(bardShift[f])a=stance(a,bardShift[f],42,50);
 // Lock the three original slime contact patches; keep all original world placement.
 for(var x=0;x<64;++x)copyPixel(bard[0],pix(a,x,51),a,pix(a,x,51));
 for(var y=52;y<64;++y)for(var x=0;x<64;++x){var i=pix(a,x,y);a[i]=a[i+1]=a[i+2]=a[i+3]=0;}
 bardOut.push(a);
}
writeNine('Bardley',bardOut);
