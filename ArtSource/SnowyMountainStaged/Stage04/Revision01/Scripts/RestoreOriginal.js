var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/SnowyMountainStaged/Stage04/Revision01/';
var pc=app.pixelColor;
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
function save(name,s){s.layer(0).name='Native source pixels / masonry continuation';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('SNOW_REVISION_MODULE '+name+' PASS');}
try{
 app.open(root+'Raw/OriginalPixelLab.png');var original=app.activeSprite.layer(0).cel(0).image;var src=[];
 for(var y=0;y<96;y++){var row=[];for(var x=0;x<320;x++)row.push(original.getPixel(x,y));src.push(row);}app.activeDocument.close();
 // Five native modules reconstruct the original snow cap, thin lip, shadow
 // course and first cobblestone row. No palette remap or resampling.
 for(var i=0;i<5;i++){
  app.open(root+'Raw/Blank64x84.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
  for(var y=0;y<84;y++)for(var x=0;x<64;x++){
   var sx=i*64+x;
   // Close the source's empty right export column beneath the snow edge.
   if(sx===319&&y>=40)sx=318;
   im.putPixel(x,y,src[y][sx]);
  }
  save('Ledge_'+i,s);
 }
 // Sample three complete original stones (including their native mortar).
 // Four offset courses avoid stacked vertical seams. Darker stone planes use
 // colors already present in the original lower masonry, not a new palette.
 app.open(root+'Raw/Blank154x52.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
 var offsets=[0,38,19,57];
 for(var y=0;y<52;y++)for(var x=0;x<154;x++){
  var course=Math.floor(y/13),u=(x+offsets[course])%77;
  var v=src[71+y%13][118+u],h=hex(v);
  // Shade complete stones, never a partial plane. Six stones per 154px
  // repeat makes the shade selection periodic across the horizontal join.
  var block=(u<24?0:(u<50?1:2))+3*Math.floor((x+offsets[course])/77);
  if((block+course)%6===2){if(h==='3e6278')v=rgb('324558');else if(h==='5283aa')v=rgb('3e6278');else if(h==='aea875'||h==='a4a681')v=rgb('467596');}
  im.putPixel(x,y,pc.rgbaA(v)?v:rgb('192739'));
 }
 save('CobbleWall',s);
 console.log('SNOW_REVISION_LIBRESPRITE_PASS');
}catch(e){console.log('SNOW_REVISION_FAIL '+e);}
app.command.Exit();
