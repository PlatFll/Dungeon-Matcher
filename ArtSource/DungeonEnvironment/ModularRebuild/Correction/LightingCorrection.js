// Locked-shape lighting pass. All pixels are edited through LibreSprite's native API.
// Read the immutable pre-lighting exports so rebuilding never compounds the correction.
var L=K+'Correction/Lighting/',records=[];
function remap(a,map){for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var i=(y*a.w+x)*4,c=color(a,x,y);if(a.p[i+3]&&map[c])px(a,x,y,map[c]);}}
function mapping(from,to){var m={};for(var i=0;i<from.length;i++)m[from[i]]=to[i];return m;}
function finish(a,g,n){
 var path=K+'Editable/'+g+'/'+n+'.aseprite';app.open(path);var s=app.activeSprite,c=s.layer(0).cel(0),data=c.image.getImageData();
 if(s.width!==a.w||s.height!==a.h)throw 'Canvas changed '+n;
 for(var y=0;y<c.image.height;y++)for(var x=0;x<c.image.width;x++)for(var k=0;k<4;k++)data[(y*c.image.width+x)*4+k]=a.p[((y+c.y)*a.w+x+c.x)*4+k];
 c.image.putImageData(data);s.commit();s.saveAs(path,true);app.activeDocument.close();
 records.push({group:g,name:n,width:a.w,height:a.h});console.log('ASSET_OK '+g+'/'+n);
}
var wallFrom=['101020','171421','211b30','29213d','312747','3b2d50'];
var wallTo=['0d101e','101222','171929','1d2032','26283c','302d47'];
for(var v=0;v<4;v++){var n='BattleWall'+String.fromCharCode(65+v),a=read(L+'Before/WallTiles/'+n+'.png');remap(a,mapping(wallFrom,wallTo));finish(a,'WallTiles',n);}
var floorFrom=['171421','211b30','29213d','312747','382b4d','45335a','503b66','5e4473','674c75','70517f','83628f'];
var floorRamps=[
 ['151624','211d30','30263f','3a2d4b','443354','533e65','604771','6e527f','775b85','81638f','95799f'],
 ['151622','1c1b2c','252237','2c2841','322d49','3e3554','493e60','55466c','605070','6a577a','7a698b'],
 ['151624','201d30','2b263e','352c48','3d3252','4b3d60','58466c','665278','705d80','7a668b','8b779b']
];
// Tint complete, existing connected stone faces immediately beneath the two torches.
// Variant A occurs at columns 0 and 4; boundary-connected stones remain unchanged.
function bounce(a,source,mid){
 var visited=[],warm=mid?['151624','201d30','34283e','403047','4a374f','5c425b','6a4b65','79576e','826276','8c6d80','9c8090']:['151622','1c1b2c','302337','3a2a3f','452e45','56364e','644057','74495f','805566','8a606f','9b7581'];
 for(var y=0;y<64;y++)for(var x=0;x<64;x++){
  var start=y*64+x,co=color(source,x,y);if(visited[start]||co===floorFrom[0]||co===floorFrom[1])continue;
  var todo=[start],island=[],sx=0,sy=0,touch=false;visited[start]=true;
  while(todo.length){var pos=todo.pop(),X=pos%64,Y=Math.floor(pos/64);island.push(pos);sx+=X;sy+=Y;if(X<3||X>60||Y<2||Y>61)touch=true;
   var ns=[X>0?pos-1:-1,X<63?pos+1:-1,Y>0?pos-64:-1,Y<63?pos+64:-1];
   for(var j=0;j<4;j++){var q=ns[j];if(q<0||visited[q])continue;var c=color(source,q%64,Math.floor(q/64));if(c===floorFrom[0]||c===floorFrom[1])continue;visited[q]=true;todo.push(q);}
  }
  var cx=sx/island.length,cy=sy/island.length;
  if(!touch&&island.length>=8&&cx<40&&cy<(mid?30:32))for(var j=0;j<island.length;j++){var X=island[j]%64,Y=Math.floor(island[j]/64),index=floorFrom.indexOf(color(source,X,Y));if(index>=2)px(a,X,Y,warm[index]);}
 }
}
for(var f=0;f<3;f++)for(var v=0;v<4;v++){
 var n=['Floor','FloorBack','FloorMiddle'][f]+String.fromCharCode(65+v),a=read(L+'Before/FloorTiles/'+n+'.png'),source=blank(a.w,a.h);source.p.set(a.p);
 remap(a,mapping(floorFrom,floorRamps[f]));if(v===0&&f>0)bounce(a,source,f===2);finish(a,'FloorTiles',n);
}
for(var side=0;side<2;side++){
 var n=side?'TorchLitMasonryRight':'TorchLitMasonryLeft',a=read(L+'Before/Props/'+n+'.png');
 var outer={'29213d':'211d2c','45335a':'332a40','543542':'46303d','704653':'64404b','8b5a66':'805454'};
 var middle={'29213d':'30222f','45335a':'46303d','543542':'593741','704653':'805047','8b5a66':'9c624b'};
 var core={'46202b':'472530','543542':'784334','692831':'8b4e36','8d332e':'b77340','b34429':'d5914d'};
 for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var row=Math.floor(y/16),m=row===2||row===3?core:row===1||row===4?middle:outer,c=color(a,x,y);if(a.p[(y*a.w+x)*4+3]&&m[c])px(a,x,y,m[c]);}
 finish(a,'Props',n);
}
var pillar=read(L+'Before/Props/RightTopPillar.png');
// Only existing torch-warmed material clusters, retaining the cold upper/lower stones.
remap(pillar,{'3c2935':'49303a','543542':'714238','704653':'95583f','8b5a66':'b87a51','8d332e':'a85335','b34429':'c77237'});
finish(pillar,'Props','RightTopPillar');
console.log('LIGHTING_RECORDS '+JSON.stringify(records));console.log('CORRECTION_COMPLETE');
