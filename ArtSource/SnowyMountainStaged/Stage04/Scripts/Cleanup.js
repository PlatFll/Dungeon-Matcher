var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/SnowyMountainStaged/Stage04/';
var names=['SnowFloor_A','SnowFloor_B','SnowFloor_C','Separator_A','Separator_B','Separator_C','Foundation','SnowOverhang_A','SnowOverhang_B'],pc=app.pixelColor;
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
function rect(im,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)im.putPixel(xx,yy,rgb(c));}
var snow={'ecfafd':'ddfafc','bbe2f9':'b8dbe5','acd5eb':'b8dbe5','99cceb':'b8dbe5','8ec0e3':'6f99ba','7ab4d9':'6f99ba','74bee7':'6f99ba','a2c6d6':'b8dbe5'};
var ramp={'5283aa':'52758b','3e6278':'354c62','192739':'172b40','273c4f':'233246','3b5b72':'354c62','324558':'233246','0b1d2e':'172b40','071725':'172b40','6599c3':'6f99ba','aea875':'6f99ba','6091b6':'52758b','467596':'52758b','a4a681':'52758b','a19b6e':'52758b','718279':'354c62','1e3244':'233246'};
try{names.forEach(function(name){
 app.open(root+'Selected/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
 for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++){
  var v=im.getPixel(x,y),h=hex(v);
  im.putPixel(x,y,pc.rgbaA(v)<128?0:rgb(snow[h]||ramp[h]||h));
 }
 if(name.indexOf('SnowFloor_')===0){
  // Common native edge profile: repeatable surface, no dark vertical dividers.
  for(var y=0;y<32;y++)for(var x=0;x<64;x++){
   if(y>=12&&pc.rgbaA(im.getPixel(x,y))===0)im.putPixel(x,y,rgb('b8dbe5'));
   if(x<2||x>61)im.putPixel(x,y,y<10?0:rgb(y<16?'ddfafc':'b8dbe5'));
   if(y===31)im.putPixel(x,y,rgb('6f99ba'));
  }
 }
 if(name.indexOf('Separator_')===0){
  // Complete the source's cropped block face as one clean 64x24 native module.
  // Two readable stone planes replace clipped half-stones and ochre residue.
  rect(im,0,0,64,24,'172b40');
  var split=name==='Separator_A'?32:name==='Separator_B'?40:24;
  var starts=[1,split+1],ends=[split-1,62];
  for(var i=0;i<2;i++){
   var x=starts[i],w=ends[i]-x+1;
   rect(im,x,1,w,21,'354c62');rect(im,x+1,1,w-2,2,'52758b');
   rect(im,x,3,1,17,'52758b');rect(im,x+w-1,4,1,18,'233246');rect(im,x+1,21,w-1,2,'233246');
   // A few connected chips, not scattered micro texture.
   rect(im,x+3,2,3,1,'6f99ba');rect(im,x+w-5,19,3,2,'233246');
  }
 }
 if(name==='Foundation'){
  for(var y=0;y<32;y++)for(var x=0;x<64;x++){
   var xx=(x+(y>=16?16:0))%32,yy=y%16;
   im.putPixel(x,y,rgb(xx===0||yy===0?'172b40':yy===1&&xx>1?'354c62':'233246'));
  }
 }
 if(name.indexOf('SnowOverhang_')===0){
  for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++){
   var h=hex(im.getPixel(x,y));if(h!=='ddfafc'&&h!=='b8dbe5')im.putPixel(x,y,0);
  }
  // Keep the complete connected snow mound; discard isolated source flecks.
  var seen={},groups=[];
  for(var yy=0;yy<im.height;yy++)for(var xx=0;xx<im.width;xx++){
   var id=xx+yy*im.width;if(seen[id]||pc.rgbaA(im.getPixel(xx,yy))===0)continue;
   var q=[[xx,yy]],group=[];seen[id]=true;
   while(q.length){var p=q.pop();group.push(p);var nn=[[p[0]+1,p[1]],[p[0]-1,p[1]],[p[0],p[1]+1],[p[0],p[1]-1]];
    nn.forEach(function(n){var x=n[0],y=n[1],j=x+y*im.width;if(x<0||y<0||x>=im.width||y>=im.height||seen[j]||pc.rgbaA(im.getPixel(x,y))===0)return;seen[j]=true;q.push(n);});}
   groups.push(group);
  }
  groups.sort(function(a,b){return b.length-a.length;});
  for(var g=1;g<groups.length;g++)groups[g].forEach(function(p){im.putPixel(p[0],p[1],0);});
  if(name==='SnowOverhang_A')for(var y=18;y<im.height;y++)for(var x=0;x<im.width;x++){if(y>21||x<18||x>20||(y===21&&x!==19))im.putPixel(x,y,0);}
 }
 s.layer(0).name='Snow platform / native repeat cleanup';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
 app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();
 console.log('SNOW_STAGE04_MODULE '+name+' PASS');
});console.log('SNOW_STAGE04_CLEANUP_PASS');}catch(e){console.log('SNOW_STAGE04_CLEANUP_FAIL '+e);}
app.command.Exit();
