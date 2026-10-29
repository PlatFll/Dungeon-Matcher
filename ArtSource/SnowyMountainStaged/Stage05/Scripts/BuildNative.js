var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/SnowyMountainStaged/Stage05/';
var pc=app.pixelColor;
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
function read(name,w,h){app.open(root+'Raw/'+name+'.png');var im=app.activeSprite.layer(0).cel(0).image,a=[];for(var y=0;y<h;y++){var row=[];for(var x=0;x<w;x++)row.push(im.getPixel(x,y));a.push(row);}app.activeDocument.close();return a;}
function save(name,s){s.layer(0).name='PixelLab source / native material cleanup';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('SNOW5_MODULE '+name+' PASS');}
function makeModule(name,w,h,draw){app.open(root+'Raw/Blank'+w+'x'+h+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;draw(im);save(name,s);}
var wallMap={'324558':'27323e','3b5b72':'303c49','3e6278':'35424f','5283aa':'485b6b','6091b6':'516574','0b1d2e':'131c28','aea875':'525e62','1e3244':'1e2936','bbe2f9':'6f828e','192739':'192330','467596':'405363','273c4f':'222e3b','6599c3':'5a6d7c','071725':'101923','a19b6e':'4a565b','718279':'414d55','a4a681':'4c5a60'};
// Consolidate near-duplicate generator shades into readable cloth-free material ramps.
var ramp={'010103':'0a0d11','020207':'0a0d11','141b2e':'192330','172136':'192330','2e3648':'293b4b','2f2a30':'302627','3d2823':'3d2c27','3b241f':'3d2c27','412a23':'3d2c27','332221':'302627','4d392f':'4d382d','5e4535':'624937','644a39':'624937','705542':'7b6047','775e4c':'7b6047','b7d5e5':'b8d9e8','bfdfee':'b8d9e8','eafbfd':'ecfafd','526f81':'526c7c','2e4356':'344b5d','384e61':'344b5d','1f3248':'293b4b','7f9aab':'839ca9','90a8b6':'839ca9'};
function clean(v){if(!pc.rgbaA(v))return 0;var h=hex(v);return ramp[h]?rgb(ramp[h]):v;}
try{
 var a=read('SelectedFrame',128,128);
 app.open(root+'Raw/OriginalCobbleWall.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
 for(var y=0;y<52;y++)for(var x=0;x<154;x++){var v=im.getPixel(x,y);im.putPixel(x,y,rgb(wallMap[hex(v)]));}
 save('CobbleWall_Muted',s);
 // Remove four redundant columns from the broad generated plate's flat planes;
 // retain the generated central bolt, highlights and native pixels without interpolation.
 var columns=[16,17,18,19,20,23,24,25,26,27,28,29,30,33,34,35,36,37];
 ['TL','TR','BL','BR'].forEach(function(suffix){makeModule('Board_Corner_'+suffix,18,18,function(im){
  for(var y=0;y<18;y++)for(var x=0;x<18;x++)im.putPixel(x,y,rgb('526c7c'));
  for(var y=0;y<8;y++)for(var x=0;x<8;x++)im.putPixel(5+x,5+y,clean(a[24+y][23+x]));
  // Upright lighting on every corner; complete one-pixel contour and shared plate plane.
  for(var x=0;x<18;x++){im.putPixel(x,0,rgb('0a0d11'));im.putPixel(x,17,rgb('0a0d11'));}
  for(var y=0;y<18;y++){im.putPixel(0,y,rgb('0a0d11'));im.putPixel(17,y,rgb('0a0d11'));}
  for(var x=2;x<16;x++)im.putPixel(x,1,rgb('839ca9'));
  for(var y=2;y<16;y++){im.putPixel(1,y,rgb('839ca9'));im.putPixel(16,y,rgb('293b4b'));}
  for(var x=2;x<16;x++)im.putPixel(x,16,rgb('293b4b'));
  // The rail opening starts 13 pixels from the frame bounds. Notch each
  // inward 5x5 quadrant so even the corner cells remain fully uncovered.
  var innerRight=suffix==='TL'||suffix==='BL',innerDown=suffix==='TL'||suffix==='TR';
  for(var y=0;y<5;y++)for(var x=0;x<5;x++)im.putPixel(innerRight?13+x:x,innerDown?13+y:y,0);
  for(var k=0;k<5;k++){
   im.putPixel(innerRight?12:5,innerDown?13+k:k,rgb('0a0d11'));
   im.putPixel(innerRight?13+k:k,innerDown?12:5,rgb('0a0d11'));
  }
 });});
 var rows=[22,23,24,26,28,30,31,32];
 ['Top','Bottom'].forEach(function(side){makeModule('Board_Rail_'+side,32,8,function(im){
  for(var y=0;y<8;y++)for(var x=0;x<32;x++)im.putPixel(x,y,clean(a[rows[y]][40+x]));
  for(var x=0;x<32;x++){im.putPixel(x,0,rgb('7b6047'));im.putPixel(x,6,rgb('3d2c27'));im.putPixel(x,7,rgb('0a0d11'));}
  // Matching first/last columns make the modular join continuous.
  for(var y=1;y<6;y++){im.putPixel(0,y,rgb('624937'));im.putPixel(31,y,rgb('624937'));}
 });});
 ['Left','Right'].forEach(function(side){makeModule('Board_Rail_'+side,8,32,function(im){
  var cols=[20,21,23,25,27,29,32,34];
  for(var y=0;y<32;y++)for(var x=0;x<8;x++)im.putPixel(x,y,clean(a[40+y%16][cols[x]]));
  for(var y=0;y<32;y++){im.putPixel(0,y,rgb('0a0d11'));im.putPixel(1,y,rgb('7b6047'));im.putPixel(6,y,rgb('3d2c27'));im.putPixel(7,y,rgb('0a0d11'));}
  for(var x=2;x<6;x++){im.putPixel(x,0,rgb('624937'));im.putPixel(x,31,rgb('624937'));}
 });});
 makeModule('Board_Clamp',8,8,function(im){for(var y=0;y<8;y++)for(var x=0;x<8;x++){
  var h=(x==0||y==0||x==7||y==7)?'0a0d11':(x==1||y==1)?'839ca9':(x==6||y==6)?'293b4b':'526c7c';im.putPixel(x,y,rgb(h));
 }for(var y=3;y<5;y++)for(var x=3;x<5;x++)im.putPixel(x,y,rgb((x==3&&y==3)?'839ca9':'293b4b'));});
 makeModule('Board_SnowRail',32,8,function(im){for(var y=0;y<8;y++)for(var x=0;x<32;x++){
  var v=a[13+y][40+x],h=hex(v);if(['eafbfd','b7d5e5','bfdfee'].indexOf(h)>=0)im.putPixel(x,y,clean(v));
 }});
 makeModule('Board_SnowCorner',18,18,function(im){for(var y=0;y<8;y++)for(var x=0;x<18;x++){
  var v=a[13+y][columns[x]],h=hex(v);if(['eafbfd','b7d5e5','bfdfee'].indexOf(h)>=0)im.putPixel(x,y,clean(v));
 }});
 makeModule('Board_Icicles',16,8,function(im){
  // Small, coherent frost tongues; positioned outside the board opening only.
  for(var x=0;x<16;x++)im.putPixel(x,0,rgb('b8d9e8'));
  for(var y=1;y<7;y++)for(var x=2;x<(y<4?5:4);x++)im.putPixel(x,y,rgb(x==2?'ecfafd':'b8d9e8'));
  for(var y=1;y<4;y++)for(var x=10;x<(y<3?12:11);x++)im.putPixel(x,y,rgb(x==10?'ecfafd':'b8d9e8'));
 });
 console.log('SNOW5_LIBRESPRITE_PASS');
}catch(e){console.log('SNOW5_FAIL '+e);}
app.command.Exit();
