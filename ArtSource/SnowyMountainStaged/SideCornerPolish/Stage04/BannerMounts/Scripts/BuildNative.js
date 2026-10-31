var root=__ROOT__,pc=app.pixelColor;
function c(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
app.open(root+'Inputs/Banner.png');var original=app.activeSprite.layer(0).cel(0).image,source=[];
for(var y=0;y<70;y++)for(var x=0;x<40;x++)source.push(original.getPixel(x,y));app.activeDocument.close();
function build(side){
 app.open(root+'Inputs/Blank.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
 function rect(x,y,w,h,color){for(var j=y;j<y+h;j++)for(var i=x;i<x+w;i++)im.putPixel(i,j,c(color));}
 function line(x0,y0,x1,y1,color){var dx=Math.abs(x1-x0),sx=x0<x1?1:-1,dy=-Math.abs(y1-y0),sy=y0<y1?1:-1,err=dx+dy;while(true){im.putPixel(x0,y0,c(color));if(x0===x1&&y0===y1)break;var e=2*err;if(e>=dy){err+=dy;x0+=sx;}if(e<=dx){err+=dx;y0+=sy;}}}
 var anchor=side==='Left'?31:19,lo=Math.min(anchor,24),hi=Math.max(anchor,24);
 // A plate screwed into the structural upright, an arm and a diagonal brace.
 // Draw them behind the original pole; every original opaque pixel is retained.
 rect(anchor-2,4,6,17,'0a0d11');rect(anchor-1,5,4,15,'302627');rect(anchor-1,5,3,13,'555051');
 rect(anchor-1,5,3,1,'887468');rect(anchor,7,1,1,'b8d9e8');rect(anchor,17,1,1,'b8d9e8');
 rect(lo,8,hi-lo+1,4,'0a0d11');rect(lo,8,hi-lo+1,1,'887468');rect(lo,9,hi-lo+1,1,'555051');
 line(anchor,17,24,10,'0a0d11');line(anchor,16,24,9,'555051');
 // Short hanging clevis visibly joins the banner's own center pole.
 rect(22,8,5,8,'0a0d11');rect(23,9,3,7,'555051');rect(23,9,1,4,'887468');
 for(var y=0;y<70;y++)for(var x=0;x<40;x++){var v=source[y*40+x];if(pc.rgbaA(v))im.putPixel(x+4,y+12,v);}
 s.layer(0).name='Original flag + bolted timber mount';s.commit();s.saveAs(root+'Native/Banner_'+side+'_Mounted.ase',true);s.saveAs(root+'Exports/Banner_'+side+'_Mounted.png',true);app.activeDocument.close();
}
try{build('Left');build('Right');console.log('SNOW_BANNER_MOUNTS_PASS');}catch(e){console.log('SNOW_BANNER_MOUNTS_FAIL '+e);}
app.command.Exit();
