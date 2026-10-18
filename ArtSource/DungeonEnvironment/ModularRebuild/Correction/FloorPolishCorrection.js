// PixelLab A/B supply genuinely different stones; all cleanup, seam construction,
// palette assignment and source/export work below runs inside LibreSprite.
var mapping={'0e0a2a':'171421','110e34':'211b30','261b52':'29213d','31215c':'312747','382564':'382b4d',
 '4a3272':'45335a','523673':'503b66','563876':'503b66','5c3d79':'5e4473','61427d':'674c75',
 '6e4675':'674c75','764c79':'70517f','785285':'70517f','865782':'83628f','895b89':'83628f','916089':'83628f'};
function polish(a){
 for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){
  var c=color(a,x,y);
  if(c==='fcf8ff'||c==='fdfcff'){
   // Generated sheet's white outer frame is not floor material. Continue the
   // nearest interior source pixel; no trimming or scaling of the native art.
   c=color(a,Math.max(2,Math.min(a.w-3,x)),Math.max(2,Math.min(a.h-3,y)));
  }
  px(a,x,y,mapping[c]||'503b66');
 }
 // Remove isolated one-pixel bright flecks only where embedded in a single
 // stone plane. Preserve joints, cracks and connected bevel highlights.
 var before=crop(a,0,0,a.w,a.h);
 for(var y=2;y<a.h-2;y++)for(var x=2;x<a.w-2;x++){
  var c=color(before,x,y);if(c!=='83628f'&&c!=='70517f')continue;
  var ns=[color(before,x-1,y),color(before,x+1,y),color(before,x,y-1),color(before,x,y+1)];
  if(ns.indexOf(c)>=0)continue;
  if(ns[0]===ns[1]&&ns[0]===ns[2]&&ns[0]===ns[3])px(a,x,y,ns[0]);
 }
 return a;
}
var A=polish(read(K+'Correction/PixelLab/FloorProA.png')),B=polish(read(K+'Correction/PixelLab/FloorProB.png'));
var floorRecords=[];
for(var back=0;back<3;back++){
 var sy=[64,0,32][back],common=crop(A,0,sy,64,64);
 for(var v=0;v<4;v++){
  var src=v<2?A:B,sx=v%2*64,a=crop(src,sx,sy,64,64);
  // Two-pixel shared stitch bands. The source stone topology beyond these
  // bands stays intact; different candidates are never cyclic offsets.
  for(var y=1;y<63;y++){
   var c=color(common,32,y);px(a,0,y,c);px(a,63,y,c);
   var near=color(common,33,y);px(a,1,y,near);px(a,62,y,near);
  }
  rect(a,0,0,64,1,'211b30');rect(a,0,63,64,1,'211b30');
  var name=(['Floor','FloorBack','FloorMiddle'][back])+String.fromCharCode(65+v);save(a,'FloorTiles',name);
  floorRecords.push({group:'FloorTiles',name:name,width:64,height:64,source:'ArtSource/DungeonEnvironment/ModularRebuild/Correction/PixelLab/FloorPro'+(v<2?'A':'B')+'.png',crop:[sx,sy,64,64],changes:'Reference-guided PixelLab floor, finalized in LibreSprite: 11-color material map, removal of generated white border and isolated highlight flecks, 2px horizontal stitch bands and 1px course-edge seams. '+(back?'Compressed distant courses.':'Larger near-ground courses.')});
 }
}
console.log('FLOOR_RECORDS '+JSON.stringify(floorRecords));
console.log('CORRECTION_COMPLETE');
