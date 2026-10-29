var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/SnowyMountainStaged/Stage03/";
var names=["Portrait_Corner_TL","Portrait_Corner_TR","Portrait_Corner_BL","Portrait_Corner_BR","Portrait_Rail_Top","Portrait_Rail_Bottom","Portrait_Rail_Left","Portrait_Rail_Right","Portrait_Clamp","Backing","SnowCap_Left","SnowCap_Right","SnowCap_Middle","SnowFoot_Left","SnowFoot_Right"],pc=app.pixelColor;
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
var ramp={"050c2a":"0a0d11","020a26":"0a0d11","0a0a22":"0a0d11","120c1e":"0a0d11","243349":"233246","394f65":"354c62","4b2e23":"492f29","452b24":"492f29","412720":"492f29","442e2a":"492f29","57392d":"704d38","feffff":"ddfafc","cde8f7":"b8dbe5","84afd1":"6f99ba","b0d6ee":"b8dbe5","a3cae2":"b8dbe5"};
function line(im,x0,y0,x1,y1,h){for(var y=y0;y<=y1;y++)for(var x=x0;x<=x1;x++)im.putPixel(x,y,rgb(h));}
function plate(im){
 for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++)im.putPixel(x,y,0);
 var z=im.width-2;line(im,2,1,z-1,z,'0a0d11');line(im,1,2,z,z-1,'0a0d11');
 line(im,2,2,z-1,z-1,'52758b');line(im,2,2,z-1,2,'7c9eb3');line(im,2,2,2,z-2,'7c9eb3');
 line(im,z-1,3,z-1,z-1,'354c62');line(im,3,z-1,z-1,z-1,'354c62');
 var m=im.width/2;
 line(im,m-1,m-2,m,m+1,'0a0d11');line(im,m-2,m-1,m+1,m,'0a0d11');
 line(im,m-1,m-1,m,m,'354c62');im.putPixel(m-1,m-1,rgb('7c9eb3'));im.putPixel(m,m-1,rgb('52758b'));
}
try{
 names.forEach(function(name){
  app.open(root+"Selected/"+name+".png");var s=app.activeSprite,im=s.layer(0).cel(0).image;
  for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++){
   var v=im.getPixel(x,y),h=hex(v);im.putPixel(x,y,pc.rgbaA(v)<128?0:rgb(ramp[h]||h));
  }
  // The source beam hid these isolated tips; remove them from the modular snow layer.
  if(name==="SnowCap_Left")for(var y=25;y<im.height;y++)for(var x=0;x<im.width;x++)im.putPixel(x,y,0);
  if(name.indexOf("Corner_")>=0||name==="Portrait_Clamp")plate(im);
  if(name.indexOf("Rail_")>=0){
   for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++)if(pc.rgbaA(im.getPixel(x,y))===0)im.putPixel(x,y,rgb('492f29'));
   if(im.width===8){
    line(im,0,0,0,31,'0a0d11');line(im,7,0,7,31,'0a0d11');
    line(im,1,0,1,31,'8b654d');line(im,2,0,2,31,'704d38');line(im,6,0,6,31,'492f29');
    // Native repeating grain stays inside the structural edges.
    line(im,4,5,4,12,'704d38');line(im,3,22,3,27,'704d38');
   }else{
    line(im,0,0,31,0,'0a0d11');line(im,0,7,31,7,'0a0d11');
    line(im,0,1,31,1,'8b654d');line(im,0,2,31,2,'704d38');line(im,0,6,31,6,'492f29');
    // Identical first/last columns make repeated beam joins exact.
    for(var y=0;y<8;y++)im.putPixel(31,y,im.getPixel(0,y));
   }
  }
  if(name==="Backing"){
   // Rebuild only the repeat boundaries from the generated masonry's material
   // planes: two staggered courses, four connected colors, no tiny texture noise.
   for(var y=0;y<32;y++)for(var x=0;x<64;x++){
    var row=Math.floor(y/16),xx=(x+(row?16:0))%32,yy=y%16,h='233246';
    if(xx===0||yy===0||yy===15)h='0a0d11';
    else if(yy===1&&xx>1)h='354c62';
    else if(xx===31||yy===14)h='1c2b3d';
    im.putPixel(x,y,rgb(h));
   }
  }
  s.layer(0).name="Native modular cleanup / centered structural joins";s.commit();
  s.saveAs(root+"Native/"+name+".ase",true);s.saveAs(root+"Exports/"+name+".png",true);app.activeDocument.close();
  app.open(root+"Native/"+name+".ase");app.activeSprite.saveAs(root+"Roundtrip/"+name+".png",true);app.activeDocument.close();
  console.log("SNOW_PORTRAIT_MODULE "+name+" PASS");
 });
 console.log("SNOW_STAGE03_CLEANUP_PASS");
}catch(e){console.log("SNOW_STAGE03_CLEANUP_FAIL "+e);}
app.command.Exit();
