var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/SnowyMountainStaged/Stage02/";
var ramp={"d6f9fb":"ddfafc","1e3155":"20385c","14264b":"192b4f","151e3f":"101f44","0b1f3b":"0a1738","07172e":"0a1738","112643":"102c45","243e60":"284262","497092":"45688d","3e6085":"45688d","a4cce1":"9ec7e2","c9ecf5":"ddfafc"};
var pc=app.pixelColor;
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
try{
 app.open(root+"Selected/AlpineB.png");var s=app.activeSprite,im=s.layer(0).cel(0).image,n=0;
 for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++){var c=im.getPixel(x,y),h=hex(c);if(ramp[h]){im.putPixel(x,y,rgb(ramp[h]));n++;}}
 // Only fully surrounded one-pixel plane flecks, never edges or separate scenery.
 var edits=[];
 for(var y=1;y<im.height-1;y++)for(var x=1;x<im.width-1;x++){
  var v=im.getPixel(x,y),next=im.getPixel(x-1,y);if(v===next)continue;
  var all=true;
  for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++)if((dx||dy)&&im.getPixel(x+dx,y+dy)!==next)all=false;
  if(all)edits.push([x,y,next]);
 }
 edits.forEach(function(e){im.putPixel(e[0],e[1],e[2]);});
 s.layer(0).name="Alpine pass - disciplined blue ramps";s.commit();
 s.saveAs(root+"Native/AlpinePass.ase",true);s.saveAs(root+"Exports/AlpinePass.png",true);app.activeDocument.close();
 app.open(root+"Native/AlpinePass.ase");app.activeSprite.saveAs(root+"Roundtrip/AlpinePass.png",true);app.activeDocument.close();
 console.log("SNOW_STAGE02_CLEANUP_PASS recolored="+n+" isolated_plane_flecks="+edits.length);
}catch(e){console.log("SNOW_STAGE02_CLEANUP_FAIL "+e);}
app.command.Exit();
