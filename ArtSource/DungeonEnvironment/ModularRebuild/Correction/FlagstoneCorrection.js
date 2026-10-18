// PixelLab tiles dfc8b65c-28be-4db0-a59e-cf35ed299d4a, finalized in LibreSprite.
// Only native integer pixel copy, local cluster cleanup and deliberate color assignments.
var F=K+'Correction/FlagstoneRebuild/',floorRecords=[];
var ramps=[
 ['181c2e','2b304c','484d75','4d527b','63678e'], // Near: readable quiet slate faces.
 ['171b2b','252b43','3b4263','41496b','535c7c'], // Middle.
 ['151925','22283b','303952','353e59','454f6b']  // Rear: cooler and darker.
];
var sourceKinds={'12182d':0,'282d50':1,'653d46':2,'494e7c':3,'9b6253':4,'575c8c':3,'814e4c':2,'b87c5c':4};
var nearLetters=['A','B','D','M'];
var midCuts=[[0,13,26,40,53,64],[0,13,25,38,53,64],[0,13,25,38,53,64],[0,13,26,40,53,64]];
var rearCuts=[[0,7,15,23,32,40,49,57,64],[0,7,15,23,32,40,49,58,64],[0,6,14,22,31,40,49,57,64],[0,6,14,23,31,40,49,57,64]];
function kind(a,x,y){return sourceKinds[color(a,x,y)];}
function mappedRow(y,h,sh){
 // Preserve both contour ends; insert/remove only central interior pixel rows.
 var d=h-sh,mid=Math.floor(sh/2);
 if(d>=0)return y<mid?y:y<mid+d?mid:y-d;
 return y<Math.floor(h/2)?y:y-d;
}
for(var depth=0;depth<3;depth++)for(var v=0;v<4;v++){
 var letter=depth===0?nearLetters[v]:String.fromCharCode((depth===1?69:73)+v);
 var source=read(F+'Raw/Tile'+letter+'.png'),a=blank(64,64),ramp=ramps[depth];
 var destCuts=depth===0?[0,21,42,64]:depth===1?[0,13,26,39,52,64]:[0,8,16,24,32,40,48,56,64];
 var cuts=depth===0?null:depth===1?midCuts[v]:rearCuts[v],courseRecords=[];
 for(var row=0;row<destCuts.length-1;row++){
  var y0=destCuts[row],h=destCuts[row+1]-y0,sy=depth===0?(letter==='B'?40:20):cuts[row];
  var sh=depth===0?20:cuts[row+1]-sy,shift=depth===0?[0,28,12][row]:0;
  var indices=[],base=2+(row+v)%2;
  for(var y=0;y<h;y++)for(var x=0;x<64;x++){
   var X=(x+shift)%64,Y=sy+mappedRow(y,h,sh),c=kind(source,X,Y);
   if(c===undefined)throw 'Unexpected generated color '+color(source,X,Y);
   indices[y*64+x]=c;
  }
  // Interior surface flecks become quiet connected planes. Keep structural
  // joints and the generated corner contours, never blur or filter pixels.
  var cleaned=indices.slice();
  for(var y=2;y<h-3;y++)for(var x=0;x<64;x++){
   var pos=y*64+x,c=indices[pos];if(c<2)continue;
   var nearJoint=false;
   for(var yy=y-1;yy<=y+1;yy++)for(var xx=x-2;xx<=x+2;xx++)if(indices[yy*64+(xx+64)%64]<2)nearJoint=true;
   if(!nearJoint)cleaned[pos]=base;
  }
  for(var y=0;y<h;y++)for(var x=0;x<64;x++){
   var c=cleaned[y*64+x];
   if(y===0||y===h-1)c=0;
   else if(y===h-2)c=1;
   // Shared course-height end faces are full stone, never a vertical tile grid.
   if(x<3||x>60){if(y>0&&y<h-2)c=y===1?4:2;}
   // Simple top-left bevel, separated by the source's vertical joints.
   if(y===1&&c>=2)c=4;
   px(a,x,y0+y,ramp[c]);
  }
  courseRecords.push({source_y:sy,source_height:sh,destination_y:y0,destination_height:h,integer_x_offset:shift});
 }
 // Discreet reflected torch warmth in A, repeated under the two existing torches.
 // Use the existing first-course bevel only; never tint tile boundaries.
 if(v===0&&depth>0){
  for(var x=7;x<32;x++)if(color(a,x,1)===ramp[4])px(a,x,1,depth===1?'766071':'63505f');
  for(var y=2;y<5;y++)for(var x=8;x<29;x++)if(color(a,x,y)===ramp[2]||color(a,x,y)===ramp[3])px(a,x,y,depth===1?'51455f':'423b4e');
 }
 var name=['Floor','FloorMiddle','FloorBack'][depth]+String.fromCharCode(65+v);
 save(a,'FloorTiles',name);
 floorRecords.push({group:'FloorTiles',name:name,width:64,height:64,
  source:'ArtSource/DungeonEnvironment/ModularRebuild/Correction/FlagstoneRebuild/Raw/Tile'+letter+'.png',
  source_job:'dfc8b65c-28be-4db0-a59e-cf35ed299d4a',courses:courseRecords,
  changes:'Reference-conditioned PixelLab broad flagstone courses. LibreSprite native integer course assembly, quiet surface clusters, cool slate depth palette, narrow bevels, common course seams; tiny warm torch reflection on rear/middle A. No interpolation or changes outside the floor.'});
}
console.log('FLOOR_RECORDS '+JSON.stringify(floorRecords));
console.log('CORRECTION_COMPLETE');
