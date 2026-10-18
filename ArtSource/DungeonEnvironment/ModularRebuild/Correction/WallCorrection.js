// Native LibreSprite masonry reconstruction from the supplied mockup.
var wallRecords=[];
var wallRamps=[
 ['101020','14172a','191c31','20233a','272a43','30314c'],
 ['101020','171421','211b30','29213d','312747','3b2d50'],
 ['101020','171421','211b30','29213d','382b4d','45335a'],
 ['101020','171421','211b30','29213d','3b2d50','503b66']
];
function stone(a,x,y,w,h,r,n){
 // Chipped corners, broad upper-left planes, dark undersides. Never stipple.
 rect(a,x+1,y,w-2,h,r[1]);rect(a,x,y+1,w,h-2,r[1]);
 rect(a,x+2,y+1,w-4,h-3,r[2+n%2]);rect(a,x+1,y+2,w-2,h-5,r[2+n%2]);
 line(a,x+2,y+1,x+w-4,y+1,r[4]);line(a,x+1,y+2,x+1,y+Math.min(6,h-4),r[3]);
 line(a,x+2,y+h-2,x+w-3,y+h-2,r[1]);
 if(n%3===0){line(a,x+3,y+2,x+Math.min(w-5,9),y+2,r[5]);rect(a,x+w-5,y+h-5,3,2,r[1]);}
 if(n%4===1){line(a,x+w-6,y+3,x+w-4,y+5,r[1]);px(a,x+w-4,y+6,r[1]);}
 if(n%5===2){rect(a,x+4,y+h-5,4,2,r[3]);px(a,x+3,y+h-4,r[3]);}
}
var jointSets=[[[10,30,48],[21,42],[10,34,51],[21,39]],[[10,27,45],[21,44],[10,31,48],[21,41]],[[10,33,49],[21,40],[10,28,47],[21,43]],[[10,29,46],[21,43],[10,33,50],[21,38]]];
for(var m=0;m<4;m++)for(var v=0;v<4;v++){
 var a=blank(64,64),r=wallRamps[m];rect(a,0,0,64,64,r[0]);
 var broad=m===0||m===3,courses=broad?[0,21,43,64]:[0,16,32,48,64];
 for(var row=0;row<courses.length-1;row++){
  var y=courses[row]+1,h=courses[row+1]-courses[row]-2;
  var joints=broad?(row%2===0?[[18,55],[18,51],[18,53],[18,49]][v]:[[35],[32],[37],[34]][v]):jointSets[v][row];
  var edges=[-24].concat(joints).concat([85]);
  for(var j=0;j<edges.length-1;j++)stone(a,edges[j]+1,y,edges[j+1]-edges[j]-1,h,r,(row*5+j*3+v)%17);
 }
 // Identical 3px seam signatures keep any A/B/C/D combination connected;
 // the complete interior joint layouts and stone wear remain independent.
 for(var y=0;y<64;y++){
  var rowH=broad?(y<21?21:y<43?22:21):16;var q=broad?(y<21?y:y<43?y-21:y-43):y%16,c=q===0||q===rowH-1?r[0]:q===1||q===rowH-2?r[1]:q===2?r[4]:r[2];
  for(var x=0;x<3;x++){px(a,x,y,c);px(a,63-x,y,c);}
 }
 var name=['PortraitWall','BattleWall','OuterWall','HudWall'][m]+String.fromCharCode(65+v);save(a,'WallTiles',name);
 wallRecords.push({group:'WallTiles',name:name,width:64,height:64,source:'Brief/01_REFERENCE/full_scene_reference.png',changes:'Reconstructed native masonry with distinct joint geometry and chipped connected bevels. '+(m===3?'Separate violet bottom-HUD masonry, alternating 21/22px courses.':m===0?'Quiet blue portrait stones, alternating 21/22px courses.':'Smaller layered violet stones, 16px courses.')});
}
// Discrete warm stone planes behind each brazier, not a blurred light halo.
for(var side=0;side<2;side++){
 var a=blank(64,96);
 var warm=['171421','29213d','45335a','543542','704653','8b5a66'];
 for(var row=0;row<6;row++){
  var y=row*16+1,left=[22,12,5,4,13,22][row],right=64-left;
  var mid=side?35:29;
  var lit=(row===2||row===3)?["171421","46202b","543542","692831","8d332e","b34429"]:warm;
  stone(a,left,y,mid-left,14,lit,row+side);stone(a,mid+1,y,right-mid,14,lit,row+3);
 }
 var name=side?'TorchLitMasonryRight':'TorchLitMasonryLeft';save(a,'Props',name);
 wallRecords.push({group:'Props',name:name,width:64,height:96,source:'Brief/01_REFERENCE/full_scene_reference.png',changes:'Separate chamfered warm stone planes for local brazier illumination, binary silhouette and no soft halo.'});
}
console.log('WALL_RECORDS ' +JSON.stringify(wallRecords));console.log('CORRECTION_COMPLETE');
