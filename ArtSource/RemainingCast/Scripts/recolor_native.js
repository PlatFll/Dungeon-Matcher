// Concatenate cast_spec.js before this file and run inside LibreSprite.
// The original transparent silhouettes and all facial geometry remain exact.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/RemainingCast/';
function inside(x,y,boxes){for(var i=0;boxes&&i<boxes.length;i++){var b=boxes[i];if(x>=b[0]&&y>=b[1]&&x<=b[2]&&y<=b[3])return true;}return false;}
function nearestRamp(role,value,thresholds){var n=0;while(n<thresholds.length&&value>=thresholds[n])n++;return RAMPS[role][n];}
function colorFor(spec,r,g,b,x,y){
 var hi=Math.max(r,g,b),lo=Math.min(r,g,b),ch=hi-lo;
 var lum=.2126*r+.7152*g+.0722*b;
 if(hi<30)return RAMPS.outline[0];
 var warm=r>b*1.12&&r>=g;
 var red=r>g*1.55&&b>g*.65;
 var gold=r>g&&g>b*1.30&&(r-g)<(g-b)*1.20;
 var neutral=ch<28||ch/hi<.18;
 // The Minotaur's eyes/nose ring stay gold; fur is not human skin.
 if(spec.group==='minotaur'){
  if(hi<52)return RAMPS.outline[0];
  if(r>200&&g>170&&b<130)return nearestRamp('gold',lum,[110,185]);
  if(red&&r>70&&g<70)return nearestRamp('red',r,[85,165]);
  if(lum>160&&r>g&&g>b*1.08){
   if(y<34&&x<49)return nearestRamp('bone',lum,[165,220]);
   if(x>46)return nearestRamp('wood',lum,[100,165]);
   return nearestRamp('linen',lum,[100,190]);
  }
  if(neutral&&y>34)return nearestRamp('cloth',lum,[75,140]);
  return nearestRamp('fur',lum,[66,100]);
 }
 if(inside(x,y,spec.hair)&&!neutral&&lum<135)return nearestRamp('wood',lum,[67,123]);
 // Exposed faces and hands are assigned by anatomy, not global color distance.
 if(inside(x,y,spec.faces)&&warm&&(!red||r>145)&&ch>25&&lum>82){
  // Dark hair/beard clusters are protected from the skin conversion.
  return nearestRamp('skin',lum,[113,166]);
 }
 if(inside(x,y,spec.beard)&&neutral)return nearestRamp('silver',lum,[70,126,200]);
 // Mage robes are pale fabric; the crystal staff keeps its blue identity.
 if(spec.group==='mage'){
  if(b>r*1.1&&ch>45&&x<21)return nearestRamp('ice',lum,[130,207]);
  if(neutral||(b>=r&&ch<55)){
   if(y>=34&&y<=42&&((x>=20&&x<=26)||(x>=40&&x<=47)))return nearestRamp('iron',lum,[75,128]);
   return nearestRamp('bone',lum,[105,185]);
  }
 }
 if(spec.group==='arcanist'){
  if(b>r*1.08&&ch>25)return nearestRamp('blue',lum,[90,165]);
  if(neutral&&lum>150)return nearestRamp('linen',lum,[100,185]);
  if(warm&&!red&&x>=23&&y<31)return nearestRamp('wood',lum,[83,145]);
  if(warm&&!red&&x<23)return y<26?nearestRamp('gold',lum,[100,185]):nearestRamp('wood',lum,[83,148]);
 }
 if(spec.group==='marshal'&&inside(x,y,spec.linen)&&!red&&r>=b&&lum>87)return nearestRamp('linen',lum,[113,181]);
 if(b>r*1.05&&ch>23&&!neutral){
  if(r>g*1.25)return nearestRamp('plum',lum,[60,120]);
  return nearestRamp('blue',lum,[85,165]);
 }
 if(red)return nearestRamp('red',r,[92,158]);
 // Existing heraldic violet details remain a supporting plum accent.
 if(b>g*1.25&&r>g*1.25)return nearestRamp('plum',lum,[65,132]);
 if(neutral){
  if(inside(x,y,spec.cloth)&&spec.group==='guard'&&y>34&&r>g&&g>b)return nearestRamp('linen',lum,[95,180]);
  if(spec.group==='royal')return nearestRamp('silver',lum,[70,145,205]);
  return nearestRamp('iron',lum,[75,128]);
 }
 if(gold&&spec.group!=='guard'&&spec.group!=='minotaur')return nearestRamp('gold',lum,[110,185]);
 if(gold&&spec.src==='BarricadeGuard'&&y>40)return nearestRamp('gold',lum,[110,185]);
 if(warm)return nearestRamp('wood',lum,[83,148]);
 return nearestRamp('iron',lum,[86,168]);
}
for(var n=0;n<CAST.length;n++){
 var spec=CAST[n];app.open(ROOT+'Originals/'+spec.src+'.aseprite');var s=app.activeSprite;
 if(s.width!==64||s.height!==64||s.layerCount!==1||s.layer(0).celCount!==1)throw Error('Unexpected source '+spec.src);
 s.saveAs(ROOT+'Recolored/'+spec.name+'.aseprite',false);
 var cel=s.layer(0).cel(0),im=cel.image,a=im.getImageData(),used={};
 for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++){
  var i=(y*im.width+x)*4;if(!a[i+3])continue;
  var c=colorFor(spec,a[i],a[i+1],a[i+2],x+cel.x,y+cel.y);used[c]=true;
  a[i]=parseInt(c.slice(0,2),16);a[i+1]=parseInt(c.slice(2,4),16);a[i+2]=parseInt(c.slice(4,6),16);
 }
 im.putImageData(a);
 var colors=Object.keys(used),pal=s.palette;pal.length=colors.length+1;pal.set(0,app.pixelColor.rgba(0,0,0,0));
 for(var q=0;q<colors.length;q++){var c=colors[q];pal.set(q+1,app.pixelColor.rgba(parseInt(c.slice(0,2),16),parseInt(c.slice(2,4),16),parseInt(c.slice(4,6),16),255));}
 s.layer(0).name=spec.name+' recolor';s.commit();s.save();app.activeDocument.close();
 console.log('RECOLORED '+spec.name+' '+Object.keys(used).length+' colors');
}
