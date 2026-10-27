
var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/IronveinBelowTopHUD/Stage02/",pc=app.pixelColor,source=[];
app.open(root+'Prepared/SelectedBeam.png');var rawCel=app.activeSprite.layer(0).cel(0),raw=rawCel.image;for(var y=0;y<32;y++){source[y]=[];for(var x=0;x<128;x++)source[y][x]=(x>=rawCel.x&&y>=rawCel.y&&x<rawCel.x+raw.width&&y<rawCel.y+raw.height)?raw.getPixel(x-rawCel.x,y-rawCel.y):0;}app.activeDocument.close();
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('000000'+((pc.rgbaR(v)<<16)|(pc.rgbaG(v)<<8)|pc.rgbaB(v)).toString(16)).slice(-6);}
var maps={'000001':'0a0d11','010101':'0a0d11','623926':'7a4d2e','693f29':'7a4d2e','4d2b1f':'4a2c1c','36201a':'30202a','562e23':'4a2c1c','200f13':'30202a','804b2a':'b07a43','494951':'42374f','7d7c92':'786477','35343b':'2b313a','adadb4':'93a1b0','242127':'2b313a','908f9b':'786477','ed903b':'f5923d','e4c370':'ffdb8f','be6222':'c94d08','ffdd4f':'fbea90','faeeb6':'fbea90'};
function mapped(v){return pc.rgbaA(v)?rgb(maps[hex(v)]||hex(v)):0;}
var names=['BeamRepeat','BeamCenter','CrystalNode','EndTransition','EndTransitionRight'];
try{names.forEach(function(name){
 app.open(root+'Prepared/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
 function px(x,y,h){im.putPixel(x,y,typeof h==='string'?rgb(h):h);}
 function box(a,b,c,d,h){for(var yy=b;yy<=d;yy++)for(var xx=a;xx<=c;xx++)px(xx,yy,h);}
 function wood(yy){var rows=[6,7,9,12,15,18,21,23,25,26];for(var y=0;y<10;y++)for(var x=0;x<s.width;x++){
   var v=mapped(source[rows[y]][18+x%12]);if(y===0||y===9)v=rgb('0a0d11');if(y===1)v=rgb('b07a43');if(y===8)v=rgb('30202a');px(x,y+yy,v);
 } // Align full native edge columns for an exact repeat, without resampling.
 for(var y=0;y<10;y++)px(s.width-1,y+yy,im.getPixel(0,y+yy));}
 function band(x,w){box(x,0,x+w-1,11,'0a0d11');box(x+1,1,x+w-2,10,'42374f');box(x+1,1,x+w-2,1,'786477');box(x+1,2,x+1,9,'55606e');box(x+2,9,x+w-2,10,'2b313a');var c=x+Math.floor(w/2);box(c-1,4,c+1,7,'0a0d11');box(c,4,c+1,5,'c58826');px(c,4,'fbea90');px(c,6,'8a5622');}
 if(name==='BeamRepeat')wood(0);
 if(name==='BeamCenter'){wood(1);band(0,6);band(34,6);}
 if(name==='EndTransition'){wood(1);band(0,8);}
 if(name==='EndTransitionRight'){wood(1);band(2,8);}
 if(name==='CrystalNode')for(var y=0;y<18;y++)for(var x=0;x<18;x++){
   var d=Math.abs(x-8.5)+Math.abs(y-8.5);if(d>9)continue;
   var v=mapped(source[y+7][x+55]);
   if(d>=8)v=rgb('0a0d11');else if(d>=6)v=rgb(x+y<17?'786477':'2b313a');
   else if(['7a4d2e','4a2c1c','30202a','b07a43'].indexOf(hex(v))>=0)v=rgb('42374f');
   px(x,y,v);
 }
 s.layer(0).name='PixelLab source 0; native material and joint cleanup';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
 app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('STAGE02_CLEAN '+name);
});console.log('STAGE02_CLEANUP_PASS');}catch(e){console.log('STAGE02_CLEANUP_FAIL '+e);}app.command.Exit();
