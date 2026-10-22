// Native additions in the exact approved Support_Corner material family.
var R='ArtSource/DungeonHybrid/SideMasonry/MatchedCorners/', c=app.pixelColor;
function color(h){return c.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
function make(right){
 var doc=app.open('ArtSource/DungeonHybrid/SideMasonry/Sources/Masonry_Raw.png');
 var s=doc.sprite,im=s.layer(0).cel(0).image;
 for(var y=0;y<128;y++)for(var x=0;x<128;x++)im.putPixel(x,y,0);
 var widths=[24,27,34,40,47,54];
 for(var row=0;row<6;row++){
  var width=widths[row],left=right?58-width:0,end=left+width;
  var joint=row<2?-1:left+(row%2===0?23:26);
  for(var y=0;y<14;y++)for(var x=left;x<end;x++){
   var col=y===0?'151423':y===13?'241f3a':x===left||x===end-1?'241f3a':x>=end-4?'302940':y===1?'4e3d6c':'382d55';
   if(y>=2&&y<7&&x===left+1)col='4e3d6c';
   // Staggered joints keep the wider courses the same brick size as the sides.
   if(joint>=0&&y>0&&y<13){
    if(x===joint)col='241f3a';
    if(x===joint-1)col='302940';
    if(x===joint+1&&y>=2&&y<7)col='4e3d6c';
   }
   im.putPixel(x,2+row*14+y,color(col));
  }
 }
 s.layer(0).name='Matched smooth stone courses - same five approved colors';s.commit();
 s.saveAs('.utmp/hybrid-masonry/Matched_'+(right?'Right':'Left')+'.png',true);
}
make(false);make(true);
