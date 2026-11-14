// Executed by LibreSprite. Native pixels only: no scale/resample operations.
var root="ArtSource/DungeonCornerFoundations/", pc=app.pixelColor;
function col(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
var outline=col('151423'),deep=col('241f3a'),shadow=col('382d55'),base=col('4e3d6c'),light=col('67537d');
function read(path){var doc=app.open(path),s=doc.sprite,im=s.layer(0).cel(0).image,a={w:s.width,h:s.height,p:[]};for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)a.p.push(im.getPixel(x,y));doc.close();return a;}
function clean(c){
 if(!pc.rgbaA(c))return 0;
 var r=pc.rgbaR(c),g=pc.rgbaG(c),b=pc.rgbaB(c);
 if(r>b+20&&g>80)return r>218?col('d6c3a4'):col('b5a184');
 if(g>r&&g>60)return col('494e5a');
 if(r<31)return outline;
 if(r<46)return deep;
 if(r<81)return shadow;
 if(r<113)return base;
 return light;
}
var shaft=read("Assets/_Game/Art/DungeonSideArchitecture/Side_Shaft.png");
function build(side){
 var src=read(root+'Raw/'+(side==='Left'?'Broad_Left_A':'Broad_Right')+'.png');
 var doc=app.open(root+'Scripts/Blank112x128.png'),s=doc.sprite,im=s.layer(0).cel(0).image;
 im.clear(0);
 // Preserve the generated connected foundation. Replace only the standalone
 // cap/short stem with a continuation of the actual production column.
 for(var y=47;y<116;y++)for(var x=8;x<120;x++)im.putPixel(x-8,y+10,clean(src.p[y*src.w+x]));
 var ox=side==='Left'?11:69;
 for(var y=0;y<45;y++)for(var x=0;x<32;x++)im.putPixel(ox+x,y,shaft.p[(y%15)*32+x]);
 // One continuous shoulder widens the 22px shaft into the 32px niche pier.
 // Broad top-left planes retain the same light direction on both sides.
 for(var y=45;y<58;y++){
  var t=(y-45)/12, x0=side==='Left'?16:74-Math.round(t*11),x1=side==='Left'?37+Math.round(t*10):95;
  for(var x=x0;x<=x1;x++)im.putPixel(x,y,x===x0||x===x1?outline:x<x0+2?light:x>x1-6?shadow:base);
 }
 s.layer(0).name='Connected foundation - native masonry cleanup';s.commit();
 s.saveAs(root+'Native/Dungeon_Foundation_'+side+'.ase',true);
 s.saveAs(root+'Exports/Dungeon_Foundation_'+side+'.png',true);doc.close();
 console.log('FOUNDATION_'+side+'_PASS');
}
try{build('Left');build('Right');}catch(e){console.log('FOUNDATION_FAIL '+e);}
app.command.Exit();
