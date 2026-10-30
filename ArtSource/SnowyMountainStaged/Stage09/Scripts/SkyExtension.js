var base=__BASE__;
var work=__WORK__;
try{
app.open(work+'AlpinePass.png');var src=app.activeSprite.layer(0).cel(0).image,row=[];
for(var x=0;x<384;x++)row.push(src.getPixel(x,0));app.activeDocument.close();
app.open(work+'SkyBlank.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
s.layer(0).name='Native overscan: exact source edge, continued cliff planes and completed distant peak';
for(var y=0;y<96;y++){
 var d=95-y,left=64-Math.floor(d/2),right=323+Math.floor(d/3);
 for(var x=0;x<384;x++){
   var c=row[100];
   if(x<left)c=row[Math.min(63,Math.floor(x*64/left))];
   if(x>=right)c=row[Math.min(383,323+Math.floor((x-right)*61/(384-right)))];
   if(d<12){var l=199+Math.floor(d/2),r=210-Math.floor(d/2);if(x>=l&&x<r)c=x<204?row[200]:row[206];}
   if(d===0)c=row[x];
   im.putPixel(x,y,c);
 }
}
s.commit();s.saveAs(base+'Stage09/Native/SkyExtension.ase',true);s.saveAs(base+'Stage09/Exports/SkyExtension.png',true);app.activeDocument.close();
console.log('SNOW9_SKY_PASS');
}catch(e){console.log('SNOW9_SKY_FAIL '+e);}
app.command.Exit();
