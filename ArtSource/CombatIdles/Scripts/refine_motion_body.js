// Native LibreSprite refinement of the reviewed first family pass.
// Integer pixel placement only. Preserve Rattlebones and Pan Villager byte-for-byte.
function idx(x,y){return (y*64+x)*4;}
function color(a,i){return ('0'+a[i].toString(16)).slice(-2).toUpperCase()+('0'+a[i+1].toString(16)).slice(-2).toUpperCase()+('0'+a[i+2].toString(16)).slice(-2).toUpperCase();}
function cp(a,i,b,j){for(var k=0;k<4;++k)b[j+k]=a[i+k];}
function put(a,i,h){a[i]=parseInt(h.substr(0,2),16);a[i+1]=parseInt(h.substr(2,2),16);a[i+2]=parseInt(h.substr(4,2),16);a[i+3]=255;}
function read(c){
 var a=new Uint8Array(16384),s=c.image.getImageData(),w=c.image.width,h=c.image.height;
 for(var y=0;y<h;++y)for(var x=0;x<w;++x)cp(s,(y*w+x)*4,a,idx(x+c.x,y+c.y));return a;
}
function openInput(name){
 app.open(ROOT+'Originals/FamilyPass1/'+name+'_Idle.aseprite');
 var s=app.activeSprite,frames=[];for(var f=0;f<s.layer(0).celCount;++f)frames.push(read(s.layer(0).cel(f)));
 s.saveAs(ROOT+'Review/'+name+'_Correction.aseprite',false);return {sprite:s,frames:frames};
}
function save(s,frames){
 for(var f=0;f<9;++f){var c=s.layer(0).cel(f);if(c.image.width!==64||c.image.height!==64)throw Error('Expected full cels');c.image.putImageData(frames[f]);}
 s.commit();s.save();app.command.GotoFirstFrame();
}
function isHead(x,y){return y<=35&&(y<23||x>=19);}
var farmer=openInput('Farmer'),pose=new Uint8Array(farmer.frames[0]);
// Correct a few existing material leaks inside the fixed reference pose.
for(var y=0;y<36;++y)for(var x=0;x<64;++x){var i=idx(x,y);if(pose[i+3]&&isHead(x,y)&&y<23&&color(pose,i)==='B07A43')put(pose,i,'B88A46');}
// The diagonal shaft is wood, not the matching-value skin shadow used by PixelLab.
var shaftRows={41:[26,28],42:[27,30],43:[28,32],44:[30,34],45:[32,35],46:[34,37],47:[35,39],48:[37,41],49:[39,42]};
for(var y in shaftRows){var span=shaftRows[y];for(var x=span[0];x<=span[1];++x){var i=idx(x,+y),h=color(pose,i);if(h==='7C5238')put(pose,i,'7A4D2E');else if(h==='B9825D')put(pose,i,'B07A43');}}
var upper=[0,0,1,2,3,3,2,1,0],hip=[0,1,2,3,3,2,1,0,0],head=[0,0,1,1,2,2,1,1,1],farmerOut=[];
for(var f=0;f<9;++f){
 var a=new Uint8Array(16384),u=upper[f],h=head[f],k=hip[f];
 // The entire tool, both hands, forearms and torso translate together. No tip-only mask.
 for(var y=0;y<64;++y)for(var x=0;x<64;++x){var i=idx(x,y);if(!pose[i+3]||isHead(x,y))continue;
  if(y>=54&&x<=42)continue;
  if(y>=36&&y<40&&x>=26)continue; // shoulder/neck bridge below
  cp(pose,i,a,idx(x,y+u));
 }
 // Connect the quieter head to the moving shoulders with existing pixel rows.
 for(var y=36+h;y<40+u;++y){var sy=36+Math.floor((y-36-h)*4/(4+u-h));
  for(var x=26;x<64;++x){var i=idx(x,sy);if(pose[i+3])cp(pose,i,a,idx(x,y));}
 }
 // Knees absorb the stance compression; the original boot/contact rows stay fixed.
 for(var y=54+u;y<60;++y){var sy;
  if(y<57+k)sy=54+Math.floor((y-54-u)*3/Math.max(1,3+k-u));
  else sy=57+Math.floor((y-57-k)*3/Math.max(1,3-k));
  for(var x=0;x<=42;++x)cp(pose,idx(x,sy),a,idx(x,y));
 }
 for(var y=60;y<64;++y)for(var x=0;x<=42;++x)cp(pose,idx(x,y),a,idx(x,y));
 for(var y=0;y<36;++y)for(var x=0;x<64;++x){var i=idx(x,y);if(pose[i+3]&&isHead(x,y))cp(pose,i,a,idx(x,y+h));}
 farmerOut.push(a);
}
save(farmer.sprite,farmerOut);

var bard=openInput('Bardley'),eyeTop=[28,27,30,31,32,32,31,30,29],faceTop=[28,28,29,30,31,31,30,29,29],bardOut=[];
var greens={'176747':true,'4A9B3F':true,'86C83C':true,'D5FFAD':true};
for(var f=0;f<9;++f){
 var src=bard.frames[f],a=new Uint8Array(src),features=[],mask={};
 // Only the existing eye/catchlight/smile pixels move, as one exact facial glyph.
 for(var y=eyeTop[f];y<=eyeTop[f]+6;++y)for(var x=26;x<=42;++x){var i=idx(x,y),c=color(src,i);
  if(c==='0A0D11'||c==='FDF5E5'){features.push([x,y]);mask[y*64+x]=true;}
 }
 var dy=faceTop[f]-eyeTop[f];
 if(dy){
  // Restore the local slime plane from its nearest horizontal same-material neighbors.
  for(var n=0;n<features.length;++n){var x=features[n][0],y=features[n][1],shade='86C83C';
   for(var d=1;d<=12;++d){var found=false;
    for(var sign=1;sign>=-1;sign-=2){var xx=x+sign*d;if(xx<0||xx>=64||mask[y*64+xx])continue;var c=color(src,idx(xx,y));if(greens[c]){shade=c;found=true;break;}}
    if(found)break;
   }
   put(a,idx(x,y),shade);
  }
  for(var n=0;n<features.length;++n){var x=features[n][0],y=features[n][1];cp(src,idx(x,y),a,idx(x,y+dy));}
 }
 bardOut.push(a);
}
save(bard.sprite,bardOut);
