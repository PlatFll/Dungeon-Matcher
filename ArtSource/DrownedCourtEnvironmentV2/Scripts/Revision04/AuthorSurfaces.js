var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/',pc=app.pixelColor,clear=pc.rgba(0,0,0,0);
function rgb(s){return pc.rgba(parseInt(s.substr(0,2),16),parseInt(s.substr(2,2),16),parseInt(s.substr(4,2),16),255);}
function read(path){app.open(root+path);var im=app.activeImage,s=app.activeSprite,a=[];for(var y=0;y<s.height;y++)for(var x=0;x<s.width;x++)a.push(im.getPixel(x,y));var o={w:s.width,h:s.height,a:a};app.activeDocument.close();return o;}
function save(name){var s=app.activeSprite;s.layer(0).name=name+' - native pixel edit';s.commit();s.saveAs(root+'Native/Revision04/'+name+'.ase',false);s.saveAs(root+'Exports/Revision04/'+name+'.png',true);console.log('SAVED '+name);app.activeDocument.close();}
try{
['A','B','C','Fill','Left','Right'].forEach(function(tile){
 var src='Exports/Revision01/S03_FloorTop'+tile+'.png';
 ['Rear','Front'].forEach(function(course){app.open(root+src);var s=app.activeSprite,im=app.activeImage,from=course==='Rear'?10:2,row=[];
 for(var x=0;x<s.width;x++)row.push(im.getPixel(x,from));
 var lo=course==='Rear'?11:0,hi=course==='Rear'?16:2;
 for(var y=lo;y<hi;y++)for(var x=0;x<s.width;x++)im.putPixel(x,y,row[x]);
 save('Floor'+course+tile);
 });
});
[{name:'ColumnLeftA',colors:['70413f','bd705e','edab80'],root:78},{name:'ColumnRightB',colors:['352b50','775789','ab83b1'],root:50}].forEach(function(coral){
 var src='Exports/Revision02/'+coral.name+'.png',original=read(src),under=read('Exports/Revision04/Underlay'+coral.name+'.png'),pal=coral.colors.map(rgb);
 [0,-1,1].forEach(function(direction,pose){app.open(root+src);var im=app.activeImage,s=app.activeSprite,points=[];
 if(direction!==0){
  for(var y=0;y<original.h;y++)for(var x=0;x<original.w;x++){var c=original.a[y*original.w+x];if(pal.indexOf(c)<0)continue;
   var d=y<coral.root-12?direction:0;
   // Fixed roots; tips move one whole pixel. Preserve any edge-touching source pixels.
   if(x+d<0||x+d>=original.w)d=0;
   if(d!==0)im.putPixel(x,y,under.a[y*original.w+x]);
   points.push([x+d,y,c]);
  }
  for(var i=0;i<points.length;i++){var q=points[i];im.putPixel(q[0],q[1],q[2]);}
 }
 save(coral.name+'Sway'+String.fromCharCode(65+pose));
 });
});console.log('LIFE04_SURFACES_PASS');
}catch(e){console.log('LIFE04_SURFACES_FAILED '+e);}app.command.Exit();
