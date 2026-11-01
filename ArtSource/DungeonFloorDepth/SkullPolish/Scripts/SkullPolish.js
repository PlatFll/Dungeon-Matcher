var root="C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DungeonFloorDepth/SkullPolish/";
app.open(root+'Before/Skull.png');
var spr=app.activeSprite;
if(spr.filename.replace(/\\/g,'/').indexOf('Before/Skull.png')<0)throw new Error('Wrong input');
var im=spr.layer(0).cel(0).image,pc=app.pixelColor,source=[];
for(var y=0;y<30;y++)for(var x=0;x<31;x++)source.push(im.getPixel(x,y));
var palette={
 '10,13,17':[36,31,46], '80,70,61':[67,59,77],
 '128,114,99':[91,83,99], '158,146,127':[112,105,118],
 '182,170,146':[130,124,136], '213,203,181':[151,146,153]
};
for(var y=0;y<30;y++)for(var x=0;x<31;x++)im.putPixel(x,y,0);
// Smaller native footprint, preserving its center, shape and bottom ground row.
for(var y=0;y<20;y++)for(var x=0;x<21;x++){
 var sx=2+Math.floor((x+.5)*27/21),sy=2+Math.floor((y+.5)*26/20);
 var p=source[sy*31+sx];
 if(pc.rgbaA(p)){
  var key=[pc.rgbaR(p),pc.rgbaG(p),pc.rgbaB(p)].join(',');
  var c=palette[key];if(!c)throw new Error('Unmapped source color');
  im.putPixel(5+x,8+y,pc.rgba(c[0],c[1],c[2],255));
 }
}
spr.layer(0).name='Smaller weathered ground skull - muted violet gray';
spr.commit();spr.saveAs(root+'Native/Skull.ase',true);spr.saveAs(root+'Exports/Skull.png',true);app.activeDocument.close();
app.open(root+'Native/Skull.ase');app.activeSprite.saveAs(root+'Roundtrip/Skull.png',true);app.activeDocument.close();
app.open(root+'Before/Floor_ContactSmall.png');
spr=app.activeSprite;if(spr.filename.replace(/\\/g,'/').indexOf('Before/Floor_ContactSmall.png')<0)throw new Error('Wrong contact input');
im=spr.layer(0).cel(0).image;
var inset=[11,8,5,4,8,11];
for(var y=0;y<6;y++)for(var x=0;x<32;x++){
 var p=0;
 if(x>=inset[y]&&x<32-inset[y])p=(y===2||y===3)&&x>=10&&x<22?pc.rgba(56,45,85,255):pc.rgba(78,61,108,255);
 im.putPixel(x,y,p);
}
spr.layer(0).name='Tighter ground contact for smaller skull';
spr.commit();spr.saveAs(root+'Native/Floor_ContactSkull.ase',true);spr.saveAs(root+'Exports/Floor_ContactSkull.png',true);app.activeDocument.close();
app.open(root+'Native/Floor_ContactSkull.ase');app.activeSprite.saveAs(root+'Roundtrip/Floor_ContactSkull.png',true);app.activeDocument.close();
console.log('SKULL_NATIVE_ROUNDTRIP_COMPLETE');app.command.Exit();
