// Run inside LibreSprite. Extract only reusable components from the authorized
// mockups, then clean silhouettes, remove baked content and repaint native faces.
// All pixel writes and ASE/PNG exports are performed by LibreSprite.
var ROOT='__ART_ROOT__', PROJECT='__PROJECT_ROOT__';
var OUT='0b0b15', DARK='141827', FACE='1c2235';
var palette=['0b0b15','141827','1c2235','242a40','30354b','403f55','535067','676079','7f778e','9891a5','aaa5b8','bfbdcd','d4d2df','e8e6ee','fbf7fa',
 '260435','49045b','6a048c','9904c3','c216ee','e343ff','ef8eff','fbc7ff','054025','006a32','008d38','00a641','0ac850','4ce962','91f87c',
 '480e26','750c30','a10c37','c11140','dc2249','ed4b61'];
var rgb=palette.map(function(c){return [parseInt(c.substr(0,2),16),parseInt(c.substr(2,2),16),parseInt(c.substr(4,2),16)];});
function canvas(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function dot(a,x,y,c){x=Math.round(x);y=Math.round(y);if(x<0||y<0||x>=a.w||y>=a.h)return;var i=(y*a.w+x)*4;for(var k=0;k<3;k++)a.p[i+k]=c?parseInt(c.substr(2*k,2),16):0;a.p[i+3]=c?255:0;}
function rect(a,x,y,w,h,c){for(var v=y;v<y+h;v++)for(var u=x;u<x+w;u++)dot(a,u,v,c);}
function line(a,x,y,X,Y,c){var n=Math.max(Math.abs(X-x),Math.abs(Y-y));for(var t=0;t<=n;t++)dot(a,x+(X-x)*t/Math.max(n,1),y+(Y-y)*t/Math.max(n,1),c);}
function inside(x,y,pts){var b=false;for(var i=0,j=pts.length-1;i<pts.length;j=i++){var p=pts[i],q=pts[j];if((p[1]>y)!=(q[1]>y)&&x<(q[0]-p[0])*(y-p[1])/(q[1]-p[1])+p[0])b=!b;}return b;}
function poly(a,pts,c){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(inside(x,y,pts))dot(a,x,y,c);}
function bevel(a,x,y,w,h,cut,c){poly(a,[[x+cut,y],[x+w-cut,y],[x+w,y+cut],[x+w,y+h-cut],[x+w-cut,y+h],[x+cut,y+h],[x,y+h-cut],[x,y+cut]],c);}
function mask(a,pts){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)if(!inside(x,y,pts))dot(a,x,y,null);return a;}
function copy(a,b,x,y){for(var Y=0;Y<b.h;Y++)for(var X=0;X<b.w;X++){var i=(Y*b.w+X)*4;if(!b.p[i+3]||X+x<0||Y+y<0||X+x>=a.w||Y+y>=a.h)continue;for(var k=0;k<4;k++)a.p[((Y+y)*a.w+X+x)*4+k]=b.p[i+k];}}
function read(path){app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),a=canvas(s.width,s.height),p=c.image.getImageData();for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)a.p[((y+c.y)*a.w+x+c.x)*4+k]=p[(y*c.image.width+x)*4+k];app.activeDocument.close();return a;}
function nearest(r,g,b,limit){var best=0,score=1e20;for(var n=0;n<(limit||rgb.length);n++){var c=rgb[n],v=(r-c[0])*(r-c[0])+1.2*(g-c[1])*(g-c[1])+(b-c[2])*(b-c[2]);if(v<score){score=v;best=n;}}return palette[best];}
// Center-sample at the new native grid, no blur or interpolated edge pixels.
function extract(src,x,y,w,h,W,H,limit){var a=canvas(W,H);for(var Y=0;Y<H;Y++)for(var X=0;X<W;X++){var i=(Math.floor(y+(Y+.5)*h/H)*src.w+Math.floor(x+(X+.5)*w/W))*4;dot(a,X,Y,nearest(src.p[i],src.p[i+1],src.p[i+2],limit));}return a;}
function save(a,name){app.open(PROJECT+'Assets/_Game/Resources/UI/Consumables/Potion.png');var s=app.activeSprite;s.saveAs(ROOT+'native/'+name+'.aseprite',false);s.resize(a.w,a.h);s.commit();app.command.BackgroundFromLayer();s.commit();s.layer(0).cel(0).image.putImageData(a.p);s.layer(0).name=name;s.commit();s.save();app.activeDocument.close();}
function states(a,name){save(a,name);for(var z=0;z<3;z++){var b=canvas(a.w,a.h);b.p.set(a.p);for(var i=0;i<b.p.length;i+=4){if(!b.p[i+3])continue;var gray=Math.round((a.p[i]*.25+a.p[i+1]*.5+a.p[i+2]*.25)*.65);for(var k=0;k<3;k++){var c=a.p[i+k];b.p[i+k]=z===0?Math.min(255,c+10):z===1?Math.round(c*.72):gray;}}save(b,name+['Hover','Pressed','Disabled'][z]);}}
var main=read(ROOT+'references/MainMenuTarget.png'), settings=read(ROOT+'references/SettingsTarget.png');
// Fine-grained texture is retained on the narrow extracted rims only. The new
// label faces use broad connected shades with a few deliberate edge chips.
function navigation(x,y,w,h,W,H,name){var a=extract(main,x,y,w,h,W,H,23),original=canvas(W,H);original.p.set(a.p);
 mask(a,[[9,0],[W-9,0],[W-6,5],[W-6,8],[W,13],[W,H-13],[W-6,H-8],[W-6,H-4],[W-10,H],[10,H],[6,H-4],[6,H-8],[0,H-13],[0,13],[6,8],[6,4]]);
 bevel(a,10,5,W-20,H-10,4,'9891a5');
 bevel(a,11,6,W-22,H-13,4,'bfbdcd');
 poly(a,[[15,7],[W-15,7],[W-12,10],[W-12,14],[W-18,14],[W-18,19],[W-20,19],[W-20,H-13],[W-25,H-13],[W-25,H-9],[15,H-9],[12,H-12],[12,10]],'d4d2df');
 // Connected chips around the perimeter keep the label field quiet.
 poly(a,[[14,12],[18,12],[18,16],[17,16],[17,20],[13,20],[13,17],[12,17],[12,14]],'bfbdcd');
 poly(a,[[W-23,7],[W-15,7],[W-12,10],[W-12,14],[W-16,14],[W-16,12],[W-20,12],[W-20,10],[W-23,10]],'e8e6ee');
 poly(a,[[W-33,H-10],[W-27,H-10],[W-27,H-12],[W-24,H-12],[W-24,H-9],[W-22,H-9],[W-22,H-7],[W-33,H-7]],'aaa5b8');
 // A restrained worn lip, outside the live icon and text area.
 line(a,16,6,W-17,6,'fbf7fa');line(a,15,H-7,W-16,H-7,'7f778e');
 for(var n=0;n<3;n++){var X=22+Math.floor((W-50)*n/3);line(a,X,H-9,X+3,H-9,'aaa5b8');dot(a,X+4,H-10,'bfbdcd');}
 line(a,W-21,9,W-18,9,'e8e6ee');dot(a,W-19,10,'aaa5b8');
 // Preserve the complete side diamonds after clearing the face. The Play and
 // Challenge bezels extend farther inward than the smaller secondary bezel.
 var cap=name==='Secondary'?10:16,half=name==='Secondary'?8:10;
 for(var Y=Math.floor(H/2)-half;Y<Math.floor(H/2)+half;Y++)for(var X=0;X<W;X++)
  if((X<cap||X>=W-cap)&&a.p[(Y*W+X)*4+3])for(var k=0;k<4;k++)a.p[(Y*W+X)*4+k]=original.p[(Y*W+X)*4+k];
 states(a,name);return a;
}
navigation(181,954,582,140,224,52,'Play');
navigation(107,1110,355,114,136,44,'Secondary');
navigation(256,1370,425,119,168,44,'Challenge');
function action(y,name,tones){var W=184,H=38,a=extract(settings,224,y,494,102,W,H,15);
 mask(a,[[7,0],[W-7,0],[W,7],[W,H-7],[W-7,H],[7,H],[0,H-7],[0,7]]);
 bevel(a,7,4,W-14,H-8,4,tones[0]);bevel(a,9,6,W-18,H-12,3,tones[1]);
 poly(a,[[13,7],[W-14,7],[W-10,11],[W-10,15],[W-35,15],[W-51,H-8],[13,H-8],[10,H-11],[10,10]],tones[2]);
 line(a,13,5,W-14,5,tones[3]);line(a,10,8,10,H-10,tones[2]);line(a,13,H-6,W-14,H-6,tones[0]);
 // Small corner glints resolve into solid native pixels.
 line(a,10,7,14,11,tones[3]);line(a,W-15,10,W-11,6,tones[3]);dot(a,12,7,'fbf7fa');
 line(a,W-16,H-8,W-12,H-12,tones[3]);dot(a,15,H-8,tones[3]);
 states(a,name);return a;
}
action(879,'Affirmative',['006a32','008d38','00a641','91f87c']);
action(993,'Neutral',['403f55','535067','676079','aaa5b8']);
action(1223,'Destructive',['750c30','a10c37','c11140','ed4b61']);
// Empty inset for the separate live navigation icon.
var plate=canvas(32,32);bevel(plate,0,0,32,32,6,'e8e6ee');bevel(plate,0,0,32,31,6,'403f55');bevel(plate,1,1,30,29,5,'141827');bevel(plate,2,2,28,27,4,'1c2235');line(plate,6,0,25,0,OUT);save(plate,'IconPlate');
// Row brackets retain their actual chipped silver silhouette. Delete baked icon,
// text and switch by replacing only the recessed face between those brackets.
var row=extract(settings,212,461,518,84,208,32,15);
mask(row,[[7,0],[201,0],[207,6],[207,12],[208,12],[208,20],[207,20],[207,26],[201,32],[7,32],[1,26],[1,20],[0,20],[0,12],[1,12],[1,6]]);
bevel(row,9,2,190,28,3,OUT);bevel(row,10,3,188,26,2,FACE);
line(row,13,3,194,3,'30354b');line(row,13,28,194,28,'141827');
poly(row,[[13,4],[81,4],[63,27],[12,27],[11,25],[11,6]],'242a40');
save(row,'SettingsRow');
function toggle(y,on){var a=extract(settings,555,y,147,62,52,24,on?30:15);
 mask(a,[[5,0],[46,0],[52,5],[52,19],[47,24],[5,24],[0,19],[0,5]]);
 // Keep the extracted faceted knob. Repaint the empty ON/OFF label recess.
 bevel(a,3,3,31,18,2,on?'054025':'30354b');
 rect(a,5,5,26,14,on?'006a32':'403f55');line(a,5,4,30,4,on?'008d38':'676079');
 // Consolidate noisy sampled knob highlights into the reference's six facets.
 var shadow=on?'006a32':'403f55',mid=on?'0ac850':'535067',light=on?'91f87c':'9891a5',edge=on?'4ce962':'7f778e';
 bevel(a,33,1,18,22,4,shadow);bevel(a,34,2,16,20,3,edge);
 poly(a,[[38,6],[46,6],[47,8],[47,17],[45,19],[38,19],[36,17],[36,8]],mid);
 poly(a,[[37,3],[47,3],[46,6],[38,6],[35,7],[35,6]],light);
 poly(a,[[47,4],[49,6],[49,17],[47,19],[47,8],[46,6]],on?'4ce962':'676079');
 poly(a,[[37,18],[46,18],[48,19],[46,21],[38,21],[35,18]],shadow);
 line(a,37,5,38,7,light);line(a,36,8,36,16,light);dot(a,47,4,'fbf7fa');line(a,45,4,49,4,'fbf7fa');line(a,47,2,47,6,'fbf7fa');
 save(a,on?'ToggleOn':'ToggleOff');
}
toggle(473,true);toggle(646,false);
// Nine-slice panel: narrow source stone bands, true tiled 24px middle, protected
// 12px corners. Both the edges and the recessed center join without seams.
var panel=canvas(48,48);rect(panel,0,0,48,48,OUT);rect(panel,11,11,26,26,FACE);
var top=extract(settings,299,327,61,24,24,10,15), bottom=extract(settings,299,1346,61,24,24,10,15);
var left=extract(settings,176,550,26,61,10,24,15), right=extract(settings,737,550,27,61,10,24,15);
copy(panel,top,12,1);copy(panel,bottom,12,37);copy(panel,left,1,12);copy(panel,right,37,12);
// Corners sit beneath the independent jewel objects; no repeating jeweled tile.
for(var y=0;y<12;y++)for(var x=0;x<12;x++){var c=x<2||y<2?OUT:x<3||y<3?'e8e6ee':x<10||y<10?'9891a5':OUT;dot(panel,x,y,c);dot(panel,47-x,y,c);dot(panel,x,47-y,c);dot(panel,47-x,47-y,c);}
// Cover center junctions with continuous inset shading, not a bright seam.
rect(panel,11,11,26,26,FACE);line(panel,11,11,36,11,'141827');line(panel,11,11,11,36,'141827');
// The center tile is flat to avoid repeated seams behind the text and controls.
rect(panel,12,12,24,24,FACE);save(panel,'Panel');
var rule=extract(settings,299,426,61,24,24,8,15);save(rule,'HeaderRule');
// Each jewel is a separate crisp diamond; no scene pixels remain around it.
var jewel=extract(settings,168,321,64,64,22,22,23);mask(jewel,[[11,0],[22,11],[11,22],[0,11]]);save(jewel,'CornerJewel');
// Crownless skull and flanking sculpted silver plates, extracted as one ornament.
var crest=extract(settings,390,282,164,78,64,32,15);
mask(crest,[[0,9],[5,4],[13,4],[15,8],[15,19],[18,23],[19,29],[21,29],[21,15],[21,8],[24,8],[24,4],[27,4],[28,0],[35,0],[37,5],[40,5],[40,8],[43,8],[43,22],[45,27],[48,27],[49,20],[49,6],[52,4],[57,5],[64,11],[64,23],[60,28],[45,30],[41,30],[41,32],[24,32],[24,30],[18,29],[1,29],[3,24],[2,20]]);save(crest,'SettingsCrest');
var divider=extract(settings,228,826,485,41,184,16,15);
// Remove the scene behind the thin rule and its skull, preserving dark sockets.
for(var y=0;y<16;y++)for(var x=0;x<184;x++){if(x<80||x>103){if(y<5||y>9)dot(divider,x,y,null);}else if(!inside(x,y,[[89,0],[96,0],[100,4],[100,10],[97,14],[88,14],[85,10],[85,4]]))dot(divider,x,y,null);}
save(divider,'Divider');
app.command.Exit();
