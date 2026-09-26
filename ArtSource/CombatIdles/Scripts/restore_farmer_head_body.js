// Run inside LibreSprite. Keep the reviewed body/prop cels; replace only the head assembly.
function px(x,y){return (y*64+x)*4;}
function cp(a,i,b,j){for(var k=0;k<4;++k)b[j+k]=a[i+k];}
function hex(a,i){return ('0'+a[i].toString(16)).slice(-2).toUpperCase()+('0'+a[i+1].toString(16)).slice(-2).toUpperCase()+('0'+a[i+2].toString(16)).slice(-2).toUpperCase();}
function paint(a,i,h){a[i]=parseInt(h.substr(0,2),16);a[i+1]=parseInt(h.substr(2,2),16);a[i+2]=parseInt(h.substr(4,2),16);a[i+3]=255;}
function readFrames(s){var out=[];
 for(var f=0;f<s.layer(0).celCount;++f){var c=s.layer(0).cel(f),raw=c.image.getImageData(),a=new Uint8Array(16384),w=c.image.width;
  for(var y=0;y<c.image.height;++y)for(var x=0;x<w;++x)cp(raw,(y*w+x)*4,a,px(x+c.x,y+c.y));out.push(a);
 }return out;
}
app.open(ROOT+'Originals/FarmerFluidAnim2.aseprite');
var original=readFrames(app.activeSprite);app.activeDocument.close();
app.open(ROOT+'Originals/BeforeHeadRestore/Farmer_Idle.aseprite');
var output=app.activeSprite,latest=readFrames(output);
output.saveAs(ROOT+'Review/Farmer_HeadRestored.aseprite',false);
// Map the original raised / settling / deep nod / recovery poses to the existing stance beats.
// Original frame numbers are deliberately not copied one-for-one into the nine-frame loop.
var order=[4,10,5,0,1,8,7,6,3],jaw=[37,38,38,36,35,36,37,38,38,36,35];
var oldHeadShift=[0,0,1,1,2,2,1,1,1];
var first=latest[0],out=[];
for(var f=0;f<9;++f){var a=new Uint8Array(latest[f]),src=new Uint8Array(original[order[f]]),bottom=jaw[order[f]];
 // Erase the previous translated head, leaving the current body and rigid held assembly intact.
 for(var y=0;y<=35;++y)for(var x=0;x<64;++x){var i=px(x,y);if(first[i+3]&&(y<23||x>=19)){
  var j=px(x,y+oldHeadShift[f]);a[j]=a[j+1]=a[j+2]=a[j+3]=0;
 }}
 // Reapply the approved material mapping to the selected ORIGINAL head drawing.
 for(var i=0;i<src.length;i+=4)if(src[i+3]){var h=MAPS.Farmer[hex(src,i)];if(!h)throw Error('Unmapped Farmer color');paint(src,i,h);}
 for(var y=0;y<=bottom;++y){var left=y<23?0:19,right=63;
  // Follow the chin contour below the cheeks; exclude the source tool and shoulders.
  if(y>=33){var lo=64,hi=-1,skin={'7C5238':true,'B9825D':true,'E6B08A':true};
   var scanY=y===bottom?y-1:y;
   for(var x=20;x<=42;++x)if(src[px(x,scanY)+3]&&skin[hex(src,px(x,scanY))]){lo=Math.min(lo,x);hi=Math.max(hi,x);}
   if(hi>=0){left=Math.max(19,lo-1);if(y>=bottom-1)right=hi+1;}
  }
  for(var x=left;x<=right;++x){var i=px(x,y);if(!src[i+3])continue;var h=hex(src,i);
   // Material cleanup carried forward from the approved recolor, without flattening the nod poses.
   if(y<23&&h==='B07A43')paint(src,i,'B88A46');
   if(y<28&&'E6B08A B9825D 7C5238'.indexOf(h)>=0)paint(src,i,y<25?'E7C979':'B88A46');
   if(y>=27&&y<33&&h==='E6B08A'&&'0A0D11 7A5A2C 7C5238'.indexOf(hex(src,px(x,y-1)))>=0)paint(src,i,'B9825D');
   cp(src,i,a,i);
  }
 }
 output.layer(0).cel(f).image.putImageData(a);out.push(a);
}
output.commit();output.save();app.command.GotoFirstFrame();app.command.PlayAnimation();
