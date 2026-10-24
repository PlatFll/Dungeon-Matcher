var pc=app.pixelColor;
var s=app.open('ArtSource/EnchantedForestStaged/Stage01/Raw/PixelLab-TopBackground.png').sprite;
var im=s.layer(0).cel(0).image;
function rgb(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
function key(p){return [pc.rgbaR(p),pc.rgbaG(p),pc.rgbaB(p)].map(function(v){return ('0'+v.toString(16)).slice(-2);}).join('');}
var map={'0b2729':'031f29','92cfc2':'83b8b0','696435':'69684c','78753d':'7e7c59'};
var mapped=0,cleaned=0;
for(var y=0;y<240;y++)for(var x=0;x<360;x++){var k=key(im.getPixel(x,y));if(map[k]){im.putPixel(x,y,rgb(map[k]));mapped++;}}
// Merge only tiny disconnected highlights in selected foliage/bank regions.
// Preserve all larger silhouettes, ground, bridge masonry and lamp/light cores.
var regions=[
{x0:110,y0:80,x1:251,y1:149,colors:{'549160':'264d33','4e7739':'264d33'},limit:3},
{x0:283,y0:1,x1:359,y1:194,colors:{'4e7739':'41612e','549160':'41612e'},limit:3},
{x0:111,y0:92,x1:252,y1:149,colors:{'2d7472':'246166'},limit:2}
];
regions.forEach(function(r){
 var visited={};
 for(var y=r.y0;y<r.y1;y++)for(var x=r.x0;x<r.x1;x++){
 var id=y*360+x,k=key(im.getPixel(x,y));if(visited[id]||!r.colors[k])continue;
 var q=[[x,y]],pts=[];visited[id]=true;
 while(q.length){var a=q.pop();pts.push(a);[[1,0],[-1,0],[0,1],[0,-1]].forEach(function(d){var nx=a[0]+d[0],ny=a[1]+d[1],nid=ny*360+nx;if(nx>=r.x0&&nx<r.x1&&ny>=r.y0&&ny<r.y1&&!visited[nid]&&key(im.getPixel(nx,ny))==k){visited[nid]=true;q.push([nx,ny]);}});}
 if(pts.length<=r.limit){pts.forEach(function(a){im.putPixel(a[0],a[1],rgb(r.colors[k]));cleaned++;});}
 }
});
s.layer(0).name='Cleaned forest native pixels';s.commit();
s.saveAs('ArtSource/EnchantedForestStaged/Stage01/Native/Top_Background_Clean.aseprite',true);
s.saveAs('ArtSource/EnchantedForestStaged/Stage01/Previews/Top_Background_Clean.png',true);
var modules=[["Grove_Background", 360, 200, 0, 0], ["Combat_Ground", 360, 40, 0, 200], ["Wisp_Far_A", 16, 16, 160, 121], ["Wisp_Far_B", 16, 20, 258, 92], ["Wisp_Near", 18, 22, 329, 161]];
modules.slice(2).forEach(function(a){
 var sp=app.open('.utmp/enchanted-stage01/'+a[0]+'.png').sprite, dest=sp.layer(0).cel(0).image;
 // Extract only glow pixels connected to the wisp core, never nearby waterfall pixels.
 var selected={},q=[];
 for(var y=0;y<a[2];y++)for(var x=0;x<a[1];x++)
 if(key(im.getPixel(a[3]+x,a[4]+y))=='54f1ec'){selected[y*a[1]+x]=true;q.push([x,y]);}
 while(q.length){
 var pt=q.pop();
 for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){
 var nx=pt[0]+dx,ny=pt[1]+dy,id=ny*a[1]+nx;
 if(nx<0||ny<0||nx>=a[1]||ny>=a[2]||selected[id])continue;
 if(['54f1ec','3c8a7f','83b8b0','55a6a2'].indexOf(key(im.getPixel(a[3]+nx,a[4]+ny)))>=0){selected[id]=true;q.push([nx,ny]);}
 }}
 for(var y=0;y<a[2];y++)for(var x=0;x<a[1];x++)if(selected[y*a[1]+x]){
 dest.putPixel(x,y,im.getPixel(a[3]+x,a[4]+y));im.putPixel(a[3]+x,a[4]+y,rgb('042f36'));
 }
 sp.layer(0).name=a[0]+' isolated light';sp.commit();
 sp.saveAs('ArtSource/EnchantedForestStaged/Stage01/Native/'+a[0]+'.aseprite',true);
 sp.saveAs('Assets/_Game/Art/EnchantedForestStaged/Stage01/'+a[0]+'.png',true);
});
modules.slice(0,2).forEach(function(a){
 var sp=app.open('.utmp/enchanted-stage01/'+a[0]+'.png').sprite,dest=sp.layer(0).cel(0).image;
 for(var y=0;y<a[2];y++)for(var x=0;x<a[1];x++)dest.putPixel(x,y,im.getPixel(a[3]+x,a[4]+y));
 sp.layer(0).name=a[0]+' native environment';sp.commit();
 sp.saveAs('ArtSource/EnchantedForestStaged/Stage01/Native/'+a[0]+'.aseprite',true);
 sp.saveAs('Assets/_Game/Art/EnchantedForestStaged/Stage01/'+a[0]+'.png',true);
});
console.log('EDIT_COUNTS mapped='+mapped+' localized_cleanup='+cleaned);
console.log('ENCHANTED_NATIVE_COMPLETE');
