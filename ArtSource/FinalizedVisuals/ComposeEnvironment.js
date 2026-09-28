// LibreSprite native assembly from generated and finished PixelLab modules.
// No reference-mockup pixels are used. Pixel coordinates are top-left native pixels.
var ROOT='__ART_ROOT__';
function read(path,w,h){app.open(ROOT+path);var data=new Uint8Array(app.activeSprite.layer(0).cel(0).image.getImageData());app.activeDocument.close();if(data.length!==w*h*4)throw new Error('Unexpected canvas '+path+' '+data.length);return {w:w,h:h,p:data};}
function blank(w,h){return {w:w,h:h,p:new Uint8Array(w*h*4)};}
function copy(a,b,x,y,X,Y){if(x<0||x>=a.w||y<0||y>=a.h)return;for(var k=0;k<4;k++)a.p[(y*a.w+x)*4+k]=b.p[(Y*b.w+X)*4+k];}
function crop(a,x,y,w,h){var b=blank(w,h);for(var Y=0;Y<h;Y++)for(var X=0;X<w;X++)copy(b,a,X,Y,x+X,y+Y);return b;}
function stamp(a,b,x,y){for(var Y=0;Y<b.h;Y++)for(var X=0;X<b.w;X++)if(b.p[(Y*b.w+X)*4+3])copy(a,b,x+X,y+Y,X,Y);}
function save(a,name){
 app.open(ROOT+'Candidates/Environment_01.png');var s=app.activeSprite;s.saveAs(ROOT+name+'.aseprite',false);s.resize(a.w,a.h);s.commit();
 app.command.BackgroundFromLayer();s.commit();s.layer(0).cel(0).image.putImageData(a.p);s.layer(0).name=name.split('/').pop();s.commit();s.save();app.activeDocument.close();
}
var HEX=['090618','161227','1f1934','26203f','312c4d','42395c','544766','6f5a77','504153','5f4a52','6b5056','896c84','563739','744435','896348','ac8050','c49662','9b8062','b89a6e','542c42','754153','935467','b36e45','e4944b','ffca75','fff0b0'];
var palette=HEX.map(function(h){return [parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255];});
function paletteFinish(a,dim){for(var j=0;j<a.p.length;j+=4){if(a.p[j+3]<128){for(var k=0;k<4;k++)a.p[j+k]=0;continue;}var best=0,dist=1e9;for(var c=0;c<palette.length;c++){var d=0;for(var k=0;k<3;k++)d+=Math.pow(a.p[j+k]*dim-palette[c][k],2);if(d<dist){dist=d;best=c;}}for(var k=0;k<4;k++)a.p[j+k]=palette[best][k];}return a;}
var names=['Chain','Banner','Torch','Crate','CrateStack','Barrel','Pot','SkullBones','Rocks','Lock','SkullRelief','ArchFinial','Bones'];
var props={};
for(var i=0;i<names.length;i++){var name=names[i];props[name]=paletteFinish(read('Candidates/Objects/'+(name==='Crate'?'CrateFixed':name)+'.png',64,64),name==='SkullBones'||name==='Bones'?.72:name==='Torch'?1:.88);save(props[name],'Environment/Props/'+name);}
props.ArchCrown=paletteFinish(read('Candidates/Objects/ArchCrownOpen.png',192,128),.9);
props.IronGrille=paletteFinish(read('Candidates/Objects/IronGrille.png',128,128),.85);
props.ArchSupport=paletteFinish(crop(read('Candidates/Objects/ArchSupport.png',128,128),46,0,36,128),.9);
var pillar=paletteFinish(crop(read('Candidates/Objects/OuterPillar.png',128,128),40,0,48,128),.9);
props.PillarCapital=crop(pillar,0,0,48,32);props.PillarShaft=crop(pillar,0,32,48,32);props.PillarBase=crop(pillar,0,96,48,32);
var architecture=['ArchCrown','IronGrille','ArchSupport','PillarCapital','PillarShaft','PillarBase'];
for(var i=0;i<architecture.length;i++)save(props[architecture[i]],'Environment/Props/'+architecture[i]);
var master=blank(512,384),tiles={};
for(var i=0;i<4;i++){tiles['Wall'+i]=read('Environment/Tiles/Wall'+String.fromCharCode(65+i)+'.png',64,64);tiles['Floor'+i]=read('Environment/Tiles/Floor'+String.fromCharCode(65+i)+'.png',64,64);}
for(var i=0;i<3;i++)tiles['Foundation'+i]=read('Environment/Tiles/Foundation'+String.fromCharCode(65+i)+'.png',64,64);
for(var i=0;i<2;i++)tiles['Warm'+i]=read('Environment/Tiles/WallWarm'+String.fromCharCode(65+i)+'.png',64,64);
for(var y=0;y<4;y++)for(var x=0;x<8;x++)stamp(master,tiles['Wall'+(x+y*3)%4],x*64,y*64);
stamp(master,tiles.Warm0,128,128);stamp(master,tiles.Warm1,320,128);
for(var x=0;x<8;x++){stamp(master,tiles['Floor'+x%4],x*64,256);stamp(master,tiles['Foundation'+x%3],x*64,320);}
// The dark recess is a structural backing, kept below separate grille/arch sprites.
for(var y=46;y<252;y++)for(var x=184;x<328;x++)if(y>128||Math.abs(x-256)<(y-28))for(var k=0;k<4;k++)master.p[(y*512+x)*4+k]=palette[0][k];
stamp(master,props.IronGrille,192,134);
stamp(master,props.ArchSupport,160,128);stamp(master,props.ArchSupport,316,128);
stamp(master,props.ArchCrown,160,0);
for(var side=0;side<2;side++){var x=side?456:8;stamp(master,props.PillarCapital,x,0);for(var y=32;y<224;y+=32)stamp(master,props.PillarShaft,x,y);stamp(master,props.PillarBase,x,224);}
stamp(master,props.Banner,60,88);stamp(master,props.Banner,388,88);
stamp(master,props.Torch,96,132);stamp(master,props.Torch,352,132);
for(var y=-2;y<80;y+=60){stamp(master,props.Chain,92,y);stamp(master,props.Chain,356,y);}
// Compact locks terminate the long suspended chains. Existing gate locks stay in its grille.
stamp(master,crop(props.Lock,20,16,24,35),112,94);stamp(master,crop(props.Lock,20,16,24,35),376,94);
stamp(master,props.CrateStack,4,196);stamp(master,props.SkullBones,40,208);stamp(master,props.Bones,52,220);
stamp(master,props.Barrel,418,200);stamp(master,props.Pot,458,204);stamp(master,props.Rocks,442,220);
save(master,'Environment/DungeonMaster');
for(var row=0;row<6;row++)for(var col=0;col<8;col++)save(crop(master,col*64,row*64,64,64),'Environment/Baked/Cell'+String.fromCharCode(65+row)+String.fromCharCode(65+col));
app.command.Exit();
