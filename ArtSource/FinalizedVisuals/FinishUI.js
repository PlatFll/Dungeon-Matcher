// Native LibreSprite pixel finishing and exact tiled component derivation.
// Generated candidates remain intact under Candidates/UI.
var ROOT='__ART_ROOT__';
var HEX=['060709','0a0d11','151423','241f3a','3a2d51','4e3d6c','67537e','846a95','aa8bb9','653285','8c44b3','b466d9','e5a9f4','ffe0ff',
 '591e36','982541','d63850','ef666d','ffb6b9','6e4a2f','ad793d','ddb75d','ffe3a0','39414b','69737a','a9b2a8','e0e2cb','0d3c47','176171','279bab','68d4cb'];
var colors=HEX.map(function(h){return [parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255];});
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function read(name,w,h){app.open(ROOT+name+'.png');var a={w:w,h:h,p:new Uint8Array(app.activeSprite.layer(0).cel(0).image.getImageData())};app.activeDocument.close();if(a.p.length!==w*h*4)throw new Error(name+' canvas mismatch');return a;}
function pixel(a,x,y,c){for(var k=0;k<4;k++)a.p[(y*a.w+x)*4+k]=c?c[k]:0;}
function copy(a,b,x,y,X,Y){for(var k=0;k<4;k++)a.p[(y*a.w+x)*4+k]=b.p[(Y*b.w+X)*4+k];}
function crop(a,x,y,w,h){var b=blank(w,h);for(var Y=0;Y<h;Y++)for(var X=0;X<w;X++)copy(b,a,X,Y,x+X,y+Y);return b;}
function finish(a,frameOnly){for(var i=0;i<a.p.length;i+=4){if(a.p[i+3]<128){for(var k=0;k<4;k++)a.p[i+k]=0;continue;}var best=0,dist=1e9;for(var c=0;c<(frameOnly?14:colors.length);c++){var d=0;for(var k=0;k<3;k++)d+=Math.pow(a.p[i+k]-colors[c][k],2);if(d<dist){dist=d;best=c;}}for(var k=0;k<4;k++)a.p[i+k]=colors[best][k];}return a;}
function nine(a,w,h,b){var n=blank(w,h);for(var y=0;y<h;y++)for(var x=0;x<w;x++){var X=x<b?x:x>=w-b?a.w-(w-x):b+(x-b)%(a.w-2*b);var Y=y<b?y:y>=h-b?a.h-(h-y):b+(y-b)%(a.h-2*b);copy(n,a,x,y,X,Y);}return n;}
function save(a,name){app.open(ROOT+'Candidates/UI/ButtonBlank.png');var s=app.activeSprite;s.saveAs(ROOT+'UI/'+name+'.aseprite',false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();s.layer(0).cel(0).image.putImageData(a.p);s.layer(0).name=name;s.commit();s.save();app.activeDocument.close();}
function states(a,name){
 save(a,name+'Normal');
 for(var state=0;state<3;state++){var b=blank(a.w,a.h);b.p.set(a.p);for(var i=0;i<b.p.length;i+=4){if(!b.p[i+3])continue;var v=(b.p[i]+b.p[i+1]+b.p[i+2])/3;var c;
  if(state===2)c=colors[v>160?7:v>90?6:v>50?4:2];
  else if(state===1)c=colors[v>180?7:v>115?6:v>70?4:v>35?3:1];
  else{var best=0,d=1e9;for(var z=0;z<14;z++){var q=0;for(var k=0;k<3;k++)q+=Math.pow(Math.min(255,b.p[i+k]*1.2+6)-colors[z][k],2);if(q<d){d=q;best=z;}}c=colors[best];}
  for(var k=0;k<4;k++)b.p[i+k]=c[k];}
  save(b,name+['Highlighted','Pressed','Disabled'][state]);
 }
}
function blankFace(a,b,color){for(var y=b;y<a.h-b;y++)for(var x=b;x<a.w-b;x++)pixel(a,x,y,color);return a;}
var large=blankFace(read('UI/ButtonLargeNormal',176,64),16,colors[3]);states(large,'ButtonLarge');
states(blankFace(finish(read('Candidates/UI/ButtonSmallNormal',96,40),true),8,colors[3]),'ButtonSmall');
states(finish(read('Candidates/UI/SettingsNormal',48,48),true),'Settings');
function panel(w,h){var a=nine(large,w,h,16);blankFace(a,16,colors[2]);
 // Repeat the quiet straight edge sample; preserve all four generated corners.
 for(var y=16;y<h-16;y++)for(var x=0;x<16;x++){copy(a,large,x,y,x,24);copy(a,large,w-16+x,y,large.w-16+x,24);}
 for(var x=16;x<w-16;x++)for(var y=0;y<16;y++){copy(a,large,x,y,40,y);copy(a,large,x,h-16+y,40,large.h-16+y);}
 return a;}
var inset=panel(176,128),tall=panel(176,256);
for(var i=0;i<inset.p.length;i+=4)if(inset.p[i]===36&&inset.p[i+1]===31)for(var k=0;k<4;k++)inset.p[i+k]=colors[2][k];
for(var i=0;i<tall.p.length;i+=4)if(tall.p[i]===36&&tall.p[i+1]===31)for(var k=0;k<4;k++)tall.p[i+k]=colors[2][k];
save(inset,'PanelInset');save(tall,'PanelTall');
save(finish(read('Candidates/UI/WavePlaque',128,32),true),'WavePlaque');
var names=['Player','Normal','Special','Miniboss','Boss'];
for(var i=0;i<names.length;i++){var name=names[i];save(finish(read('Candidates/UI/'+name+'Badge'+(name==='Normal'||name==='Boss'?'Fixed':''),24,24),false),name+'Badge');}
// Separate HP frame/track/fill. Shift only the extreme decorative finials inward;
// the central 8px fill and three-piece frame preserve their original pixel rows.
var hp=finish(read('Candidates/UI/HealthBarBase',128,32),false);
var frame=crop(hp,0,4,128,24);
for(var y=0;y<24;y++)for(var x=0;x<128;x++){
 var idx=(y*128+x)*4;
 // Red pixels belong to the fill asset, never to the fixed frame.
 if(frame.p[idx]>frame.p[idx+1]*1.8&&frame.p[idx]>frame.p[idx+2]*1.2)pixel(frame,x,y,null);
}
for(var x=0;x<128;x++){pixel(frame,x,0,null);pixel(frame,x,23,null);}
// Pure center strip is repeated/cropped by Unity; no horizontal resampling.
var fill=crop(hp,32,13,1,8),track=blank(1,8);
var hpRamp=[18,17,16,16,16,15,15,14];for(var y=0;y<8;y++)pixel(fill,0,y,colors[hpRamp[y]]);
for(var y=0;y<8;y++)pixel(track,0,y,colors[y===0||y===7?2:14]);
save(frame,'HealthFrame');save(crop(frame,0,0,16,24),'HealthStart');save(crop(frame,32,0,1,24),'HealthMiddle');save(crop(frame,112,0,16,24),'HealthEnd');save(fill,'HealthFill');save(track,'HealthTrack');
// Keep rank emphasis in frame material, with one shared red fill.
for(var rank=0;rank<names.length;rank++){
 var tint=blank(frame.w,frame.h);tint.p.set(frame.p);
 for(var j=0;j<tint.p.length;j+=4){if(!tint.p[j+3])continue;var v=(tint.p[j]+tint.p[j+1]+tint.p[j+2])/3;var c=null;
  if(names[rank]==='Normal'||names[rank]==='Miniboss')c=colors[v>135?25:v>80?24:v>45?23:2];
  if(names[rank]==='Boss')c=colors[v>135?22:v>80?21:v>45?20:3];
  if(c)for(var k=0;k<4;k++)tint.p[j+k]=c[k];
 }
 save(crop(tint,0,0,16,24),names[rank]+'HealthStart');save(crop(tint,32,0,1,24),names[rank]+'HealthMiddle');save(crop(tint,112,0,16,24),names[rank]+'HealthEnd');
}
var energy=finish(read('Candidates/UI/EnergyBarFixed',144,32),false);
var energyFill=crop(energy,48,13,1,8),energyTrack=blank(1,8);
for(var y=0;y<8;y++)pixel(energyTrack,0,y,colors[y===0||y===7?2:27]);
for(var j=0;j<energy.p.length;j+=4)if(energy.p[j+1]>energy.p[j]*1.5&&energy.p[j+2]>energy.p[j]*1.5){for(var k=0;k<4;k++)energy.p[j+k]=0;}
save(energy,'EnergyFrame');save(energyFill,'EnergyFill');save(energyTrack,'EnergyTrack');
var sourceLogo=finish(read('Candidates/UI/Logo',212,88),false),logo=blank(160,92);
for(var y=0;y<88;y++)for(var x=27;x<187;x++)copy(logo,sourceLogo,x-27,y+2,x,y);
save(logo,'DungeonMatcherLogo');
var gem=crop(sourceLogo,86,29,40,36);
// Keep the native opposed facets and seam; exclude adjacent wordmark pixels.
for(var y=0;y<36;y++)for(var x=0;x<40;x++)if(y<2||y>30||Math.abs(x-19.5)>21-Math.abs(y-16)*1.25)pixel(gem,x,y,null);
save(gem,'SplitStoryGem');
app.command.Exit();
