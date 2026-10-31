var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/IronveinTopCoverage/NewCavern/", pc=app.pixelColor, pal=[[23, 30, 46, 255], [16, 19, 31, 255], [10, 13, 17, 255], [28, 28, 43, 255], [44, 53, 77, 255], [48, 38, 56, 255], [18, 22, 41, 255], [18, 20, 35, 255], [36, 46, 66, 255], [48, 32, 42, 255], [66, 55, 79, 255], [70, 85, 110, 255], [106, 126, 152, 255], [89, 108, 135, 255], [54, 67, 90, 255], [122, 77, 46, 255], [69, 49, 58, 255], [90, 60, 60, 255], [74, 44, 28, 255], [90, 73, 100, 255]];
app.open(root+'Raw/OutpaintResult.png');var spr=app.activeSprite,im=spr.layer(0).cel(0).image,changed=0;
// Only generated pixels may be palette-cleaned. The approved core is immutable.
for(var y=0;y<352;y++)for(var x=0;x<480;x++){
 if(x>=180&&x<420&&y>=192)continue;
 var c=im.getPixel(x,y),r=pc.rgbaR(c),g=pc.rgbaG(c),b=pc.rgbaB(c),best=1e20,pick=pal[0];
 for(var i=0;i<pal.length;i++){var v=pal[i],dr=r-v[0],dg=g-v[1],db=b-v[2],d=dr*dr+dg*dg+db*db;if(d<best){best=d;pick=v;}}
 if(best){im.putPixel(x,y,pc.rgba(pick[0],pick[1],pick[2],255));changed++;}
}
// Remove only tiny disconnected generated-color islands, not connected cave
// contours. Four-connected clusters up to three pixels are native speckle QA.
var speckles=0;
function locked(x,y){return x>=180&&x<420&&y>=192;}
for(var pass=0;pass<2;pass++){
 var seen={},changes=[];
 for(var y=0;y<352;y++)for(var x=0;x<480;x++){
  var key=y*480+x;if(seen[key]||locked(x,y))continue;
  var color=im.getPixel(x,y),queue=[key],part=[],boundary={},touch=false;seen[key]=true;
  for(var n=0;n<queue.length;n++){
   var k=queue[n],px=k%480,py=Math.floor(k/480);part.push(k);
   var around=[[px-1,py],[px+1,py],[px,py-1],[px,py+1]];
   for(var j=0;j<4;j++){var nx=around[j][0],ny=around[j][1];if(nx<0||nx>=480||ny<0||ny>=352)continue;
    var nc=im.getPixel(nx,ny),nk=ny*480+nx;
    if(nc===color){if(locked(nx,ny)){touch=true;continue;}if(!seen[nk]){seen[nk]=true;queue.push(nk);}}
    else boundary[nc]=(boundary[nc]||0)+1;
   }
  }
  if(part.length<=3&&!touch){var best=0,target=color;for(var c in boundary)if(boundary[c]>best){best=boundary[c];target=+c;}
   if(best>=3)for(var j=0;j<part.length;j++)changes.push([part[j],target]);
  }
 }
 for(var n=0;n<changes.length;n++){var k=changes[n][0];im.putPixel(k%480,Math.floor(k/480),changes[n][1]);speckles++;}
}
spr.layer(0).name='PixelLab generated cave surround + exact approved core';spr.commit();
spr.saveAs(root+'Native/CavernWide.ase',true);spr.saveAs(root+'Exports/CavernWide.png',true);app.activeDocument.close();
app.open(root+'Native/CavernWide.ase');app.activeSprite.saveAs(root+'Roundtrip/CavernWide.png',true);app.activeDocument.close();
console.log('CLEANED_PALETTE_PIXELS '+changed);console.log('REMOVED_TINY_SPECKLE_PIXELS '+speckles);console.log('NATIVE_CAVERN_ROUNDTRIP_COMPLETE');app.command.Exit();