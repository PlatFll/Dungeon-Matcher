// Concatenate cast_spec.js before running this script in LibreSprite.
// Native pixel/cel authoring only. The inspected Rattlebones reference defines
// the acting and 9x130ms cadence and is never modified. The existing Pan idle
// supplies full-size transparent cel storage only; every pixel is replaced.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/RemainingCast/';
var OUT='0A0D11';
function box(x0,y0,x1,y1){return [[x0,y0],[x1+1,y0],[x1+1,y1+1],[x0,y1+1]];}
var ACTING={
 CrossbowGuard:{head:[[14,6],[44,6],[50,25],[43,25],[43,34],[38,37],[19,36],[17,26],[12,26]],eyes:[[22,28,23,30],[31,28,33,30]],props:[[[10,35],[13,38],[12,45],[30,43],[43,43],[43,49],[29,52],[23,59],[15,60],[11,57],[3,57],[2,51],[8,49]]],propDy:'carry',center:31},
 BarricadeGuard:{head:[[22,8],[41,8],[47,21],[47,33],[40,34],[35,37],[24,37],[19,33],[17,22]],props:[[[8,34],[15,34],[18,39],[17,44],[28,44],[28,50],[15,50],[13,54],[6,52],[2,46]]],propDy:'carry',center:32},
 SpearGuard:{head:[[23,7],[38,7],[48,19],[51,25],[44,25],[43,35],[38,38],[23,37],[19,32],[18,26],[13,26]],eyes:[[23,28,24,30],[32,28,34,30]],props:[[[5,2],[9,2],[12,34],[16,40],[20,40],[20,47],[16,48],[19,64],[15,64],[11,43],[7,24]],[[41,37],[49,38],[57,43],[58,54],[49,60],[42,61],[35,54],[34,44],[38,38]]],propDy:'planted',center:32},
 TownMarshal:{head:[[17,8],[47,8],[47,28],[44,29],[44,34],[39,39],[38,42],[24,42],[19,37],[18,33],[17,32]],eyes:[[23,23,25,25],[32,23,35,25]],props:[[[2,25],[15,25],[17,32],[14,38],[16,43],[15,49],[10,50],[9,39],[4,36]],[[57,25],[63,28],[64,35],[60,40],[57,52],[53,61],[50,61],[51,49],[48,49],[48,41],[53,39],[56,35],[54,30]]],propDy:'carry',center:32},
 SiegeSergeant:{head:[[29,1],[37,1],[44,11],[46,20],[48,24],[42,25],[43,31],[40,39],[36,42],[25,41],[20,35],[23,27],[21,25],[20,21]],eyes:[[26,25,28,26],[36,25,38,26]],props:[[[14,32],[20,32],[22,39],[19,42],[18,54],[23,54],[24,62],[21,64],[7,64],[7,57],[12,54],[14,42],[10,39],[10,34]]],propDy:'planted',center:33},
 SwordKnight:{head:box(18,5,43,35),props:[[[59,20],[63,22],[49,44],[49,49],[45,51],[40,51],[38,48],[43,44],[46,40]]],propDy:'carry',center:31},
 SpearKnight:{head:[[33,1],[45,1],[54,8],[55,20],[46,19],[46,30],[41,34],[39,38],[25,38],[22,34],[23,25],[22,22],[26,14],[30,10]],props:[[[5,3],[9,7],[11,18],[15,19],[18,21],[16,26],[12,28],[16,39],[20,40],[20,47],[17,50],[19,63],[15,64],[11,44],[9,31],[6,27],[3,25],[5,22],[5,17]]],propDy:'planted',center:32,tail:box(44,7,55,20)},
 ShieldKnight:{head:box(22,5,44,35),props:[[[46,28],[52,31],[59,33],[63,35],[61,54],[58,59],[51,64],[45,62],[39,56],[36,46],[37,37],[41,32]]],propDy:'planted',center:31,cape:box(8,49,16,62)},
 KnightCaptain:{head:[[24,8],[38,8],[39,3],[48,3],[54,8],[56,21],[53,26],[55,34],[54,37],[47,35],[45,29],[42,32],[37,36],[26,37],[19,33],[18,22],[20,15]],eyes:[[24,26,26,28],[33,26,35,28]],props:[[[1,17],[5,21],[17,39],[20,42],[23,44],[22,48],[18,51],[14,49],[15,44],[11,42],[3,27]]],propDy:'carry',center:32,cape:box(49,45,61,58),tail:box(47,22,56,36)},
 RoyalSwordsman:{head:box(22,7,47,37),props:[[[1,18],[5,19],[21,43],[25,44],[25,50],[19,53],[15,51],[14,47],[11,48],[9,46],[15,42],[2,26]]],propDy:'carry',center:33,cape:box(51,43,62,61)},
 RoyalLancer:{head:[[36,4],[43,4],[46,8],[49,18],[47,21],[45,25],[46,34],[43,38],[37,40],[29,39],[24,34],[24,25],[27,18],[31,13]],props:[[[4,5],[8,7],[17,25],[19,31],[18,35],[23,40],[27,42],[27,48],[24,50],[28,62],[27,64],[23,64],[18,50],[14,48],[12,42],[12,34],[8,32]]],propDy:'planted',center:35,tail:box(43,9,49,21)},
 RoyalArbalist:{head:[[30,2],[39,2],[44,7],[49,8],[50,14],[39,18],[40,22],[41,30],[36,35],[29,37],[22,35],[19,29],[19,22],[22,18],[27,13]],props:[[[11,34],[14,36],[9,43],[9,45],[22,42],[32,39],[37,40],[40,44],[37,47],[30,48],[25,52],[20,56],[9,56],[0,51],[0,49],[6,46],[6,40]]],propDy:'carry',center:31,cape:box(44,47,49,62),tail:box(38,4,50,17)},
 RoyalStandardBearer:{head:[[33,12],[42,12],[46,18],[47,30],[43,35],[37,39],[29,38],[26,34],[24,27],[26,21],[28,17]],props:[[[8,0],[14,0],[14,28],[22,39],[23,48],[20,64],[12,64],[10,34],[8,28]]],propDy:'planted',center:34,cape:box(49,40,63,57),banner:true},
 RoyalArcanist:{head:[[30,5],[41,6],[46,10],[49,17],[49,24],[45,26],[44,31],[38,35],[29,35],[24,30],[22,25],[23,15],[25,9]],eyes:[[26,20,28,22],[34,20,36,22]],props:[[[12,11],[17,11],[22,15],[22,23],[18,26],[20,64],[16,64],[14,44],[11,43],[11,35],[13,33],[12,26],[7,23],[6,18],[8,14]]],propDy:'planted',center:34,cape:box(48,47,53,62)},
 RoyalMage:{head:[[25,8],[40,8],[46,12],[49,19],[49,32],[45,35],[41,35],[38,38],[29,38],[25,34],[21,34],[21,21],[23,14]],eyes:[[26,24,28,26],[34,24,36,26]],props:[[[11,13],[14,11],[17,18],[20,20],[19,28],[17,32],[19,64],[15,64],[14,45],[11,44],[11,38],[14,35],[13,31],[9,29],[6,23],[8,21],[9,16]]],propDy:'planted',center:34,cape:box(49,46,55,62)},
 King:{head:[[21,4],[45,4],[46,17],[47,20],[48,28],[45,30],[45,34],[39,38],[31,39],[25,36],[22,31],[21,27],[23,21],[22,17]],eyes:[[25,22,28,22],[34,22,37,22]],props:[[[10,17],[14,17],[17,23],[16,31],[20,33],[23,35],[22,39],[18,39],[18,59],[14,63],[10,64],[6,60],[5,40],[3,39],[4,35],[9,33]]],propDy:'planted',center:34,cape:box(52,44,61,63),heavy:true},
 Minotaur:{head:[[5,0],[56,0],[56,26],[49,26],[46,33],[43,35],[25,36],[16,30],[8,24]],eyes:[[25,21,29,23],[40,21,42,23]],eyeFill:['B07A43','7A4D2E'],props:[[[50,26],[55,27],[60,31],[59,35],[55,37],[53,43],[53,50],[52,58],[48,62],[46,60],[47,49],[43,48],[44,42],[48,42],[49,36],[47,34],[48,29]]],propDy:'carry',center:31,cape:[[10,44],[13,46],[14,51],[22,54],[23,59],[8,60],[8,53]],heavy:true}
};
// Include the complete outer contour above each grip. Diagonal selection edges
// must not leave a thin blade/staff outline behind in the moving torso layer.
ACTING.SpearGuard.props.push(box(2,0,12,33));
ACTING.SpearGuard.props.push(box(3,34,20,63));
ACTING.RoyalLancer.props.push(box(2,0,19,34));
ACTING.RoyalLancer.props.push(box(9,34,22,50));
ACTING.RoyalLancer.props.push(box(14,49,27,63));
ACTING.RoyalArcanist.props.push(box(4,9,21,31));
ACTING.RoyalArcanist.props.push(box(9,32,20,63));
ACTING.RoyalMage.props.push(box(5,9,20,35));
ACTING.RoyalMage.props.push(box(10,35,20,63));
ACTING.SpearKnight.props.push(box(2,0,20,63));
ACTING.King.props.push(box(7,15,18,32));
function ix(x,y){return (y*64+x)*4;}
function cp(a,i,b,j){for(var c=0;c<4;c++)b[j+c]=a[i+c];}
function px(a,x,y,c){if(x<0||x>63||y<0||y>63)throw Error('Out of canvas');var i=ix(x,y);for(var k=0;k<3;k++)a[i+k]=c?parseInt(c.slice(k*2,k*2+2),16):0;a[i+3]=c?255:0;}
function within(x,y,poly){if(!poly)return false;var hit=false;x+=.5;y+=.5;for(var i=0,j=poly.length-1;i<poly.length;j=i++){var a=poly[i],b=poly[j];if(((a[1]>y)!==(b[1]>y))&&(x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0]))hit=!hit;}return hit;}
function inParts(x,y,parts){for(var i=0;parts&&i<parts.length;i++)if(within(x,y,parts[i]))return true;return false;}
function readReady(name){app.open(ROOT+'Recolored/'+name+'.aseprite');var s=app.activeSprite,c=s.layer(0).cel(0),im=c.image,src=im.getImageData(),out=new Uint8Array(64*64*4);for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++)cp(src,(y*im.width+x)*4,out,ix(x+c.x,y+c.y));app.activeDocument.close();return out;}
function stamp(a,out,dx,dy){for(var y=0;y<64;y++)for(var x=0;x<64;x++){var i=ix(x,y);if(!a[i+3])continue;var xx=x+dx,yy=y+dy;if(xx<0||xx>63||yy<0||yy>63)throw Error('Clipped rigid part '+x+','+y+' offset '+dx+','+dy);cp(a,i,out,ix(xx,yy));}}
function extract(src,ac){var parts={body:new Uint8Array(src.length),head:new Uint8Array(src.length),prop:new Uint8Array(src.length),cape:new Uint8Array(src.length),tail:new Uint8Array(src.length),flag:new Uint8Array(src.length)};for(var y=0;y<64;y++)for(var x=0;x<64;x++){var k=inParts(x,y,ac.props)?'prop':within(x,y,ac.tail)?'tail':within(x,y,ac.head)?'head':within(x,y,ac.cape)?'cape':ac.banner&&x>=12&&y<29?'flag':'body';cp(src,ix(x,y),parts[k],ix(x,y));}return parts;}
function blink(src,ac,amount){var a=new Uint8Array(src);if(!ac.eyes||!amount)return a;for(var e=0;e<ac.eyes.length;e++){var b=ac.eyes[e],fill=ac.eyeFill?ac.eyeFill[e]:'E6B08A';for(var y=b[1];y<=b[3];y++)for(var x=b[0];x<=b[2];x++){
 if(amount===1){if(y===b[1]&&b[3]>b[1])px(a,x,y,fill);}
 else px(a,x,y,y===b[3]?OUT:fill);
 }}return a;}
function deformBody(src,out,dy,lean,roll,center){
 // Inverse nearest-pixel sampling prevents detached rows when a knee opens.
 // The soles (last four rows) stay exact. Only torso/knee spacing compresses.
 for(var y=0;y<64;y++)for(var x=0;x<64;x++){
  var t=Math.max(0,Math.min(1,(60-y)/13));
  var sx=x-Math.round(lean*t);
  var extra=(x>center+3&&y>=33&&y<48)?roll:0;
  var yy=y-extra,sy=yy<48+dy?yy-dy:yy<60?48+(yy-48-dy)*12/(12-dy):yy;
  sy=Math.round(sy);if(sx<0||sx>63||sy<0||sy>63)continue;
  if(src[ix(sx,sy)+3])cp(src,ix(sx,sy),out,ix(x,y));
 }
}
function capePose(src,out,ac,amount,dy){
 for(var y=0;y<64;y++)for(var x=0;x<64;x++){
  var i=ix(x,y);if(!src[i+3])continue;
  var dx=Math.round(amount*Math.min(1,Math.max(0,(y-40)/16)));
  if(x<ac.center)dx=-dx;
  // A hem can fold inward at a canvas edge; never discard opaque pixels.
  var xx=Math.max(0,Math.min(63,x+dx)),yy=y+Math.round(dy*Math.max(0,(61-y)/26));
  yy=Math.max(0,Math.min(63,yy));cp(src,i,out,ix(xx,yy));
 }
}
function solidBounds(a){var b=[64,64,-1,-1];for(var y=0;y<64;y++)for(var x=0;x<64;x++)if(a[ix(x,y)+3]){b[0]=Math.min(b[0],x);b[1]=Math.min(b[1],y);b[2]=Math.max(b[2],x);b[3]=Math.max(b[3],y);}return b;}
function pose(ready,ac,f){
 if(f===0||f===8)return new Uint8Array(ready);
 var hd=(ac.heavy?[0,-1,0,1,2,2,1,0,0]:[0,-1,0,1,3,3,2,1,0])[f];
 var bd=(ac.heavy?[0,-1,0,1,1,1,1,0,0]:[0,-1,0,1,2,2,1,0,0])[f];
 var lean=[0,0,0,0,-1,-1,0,0,0][f],roll=[0,0,1,1,1,1,0,0,0][f];
 var p=extract(blink(ready,ac,f===5?2:(f===4||f===6)?1:0),ac),out=new Uint8Array(ready.length);
 var lag=[0,0,0,1,1,2,1,0,0][f];
 capePose(p.cape,out,ac,-lag,bd);
 if(ac.banner){for(var y=0;y<29;y++)for(var x=12;x<64;x++){if(p.flag[ix(x,y)+3]){var fy=y+Math.round(lag*Math.max(0,(x-17)/46));cp(p.flag,ix(x,y),out,ix(x,fy));}}}
 deformBody(p.body,out,bd,lean,roll,ac.center);
 // Complete the unseen banner backing exposed by a dipping helmet.
 if(ac.banner){for(var y=13;y<25;y++)for(var x=30;x<46;x++)if(!out[ix(x,y)+3])px(out,x,y,y<16?'7C1A30':'4B1127');}
 var pb=solidBounds(p.prop),pdx=ac.propDy==='planted'?0:lean;
 if(pb[0]+pdx<0||pb[2]+pdx>63)pdx=0;
 var pdy=ac.propDy==='planted'?0:Math.min(bd,63-pb[3]);
 // Planted spears/staves stay rigid; the flexing elbow carries the torso dip.
 stamp(p.prop,out,pdx,pdy);
 var hb=solidBounds(p.head);if(hb[1]+hd<0)hd=-hb[1];
 var hdx=lean;
 stamp(p.head,out,hdx,hd);
 var tb=solidBounds(p.tail);if(tb[2]>=0){var taildx=hdx+(f>=4&&f<=6?-1:0),taildy=Math.min(hd+(f===5?1:0),63-tb[3]);stamp(p.tail,out,taildx,taildy);}
 return out;
}
function palette(a){var cs={},out=[];for(var i=0;i<a.length;i+=4)if(a[i+3]){var c=('0'+a[i].toString(16)).slice(-2)+('0'+a[i+1].toString(16)).slice(-2)+('0'+a[i+2].toString(16)).slice(-2);cs[c.toUpperCase()]=true;}for(var c in cs)out.push(c);return out;}
function write(name,frames,colors){
 app.open('C:/UnityProjects/Dungeon Matcher/ArtSource/CombatIdles/PanVillager_Idle.aseprite');var s=app.activeSprite;
 s.saveAs(ROOT+'Idles/'+name+'_Idle.aseprite',false);
 if(s.layer(0).celCount!==9)throw Error('Reference exposures changed');
 for(var f=0;f<9;f++){var c=s.layer(0).cel(f);if(c.x||c.y||c.image.width!==64||c.image.height!==64)throw Error('Full canvas required');c.image.putImageData(frames[f]);}
 s.layer(0).name=name+' idle';var pal=s.palette;pal.length=colors.length+1;pal.set(0,app.pixelColor.rgba(0,0,0,0));for(var i=0;i<colors.length;i++){var c=colors[i];pal.set(i+1,app.pixelColor.rgba(parseInt(c.slice(0,2),16),parseInt(c.slice(2,4),16),parseInt(c.slice(4,6),16),255));}
 s.commit();s.save();app.activeDocument.close();
}
for(var n=0;n<CAST.length;n++){
 var name=CAST[n].name,ready=readReady(name),frames=[];
 for(var f=0;f<9;f++)frames.push(pose(ready,ACTING[name],f));
 var colors=palette(ready);write(name,frames,colors);console.log('AUTHORED '+name+' 9x130ms');
}
