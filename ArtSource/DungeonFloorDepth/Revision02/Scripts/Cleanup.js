var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DungeonFloorDepth/Revision02/';
app.open(root+'Raw/Candidate3.png');
var spr=app.activeSprite;
if(spr.filename.replace(/\\/g,'/').indexOf('Raw/Candidate3.png')<0)throw new Error('Wrong input');
var im=spr.layer(0).cel(0).image, pc=app.pixelColor;
if(im.width!==128||im.height!==32)throw new Error('Wrong native canvas');
var base=pc.rgba(103,83,125,255), dark=pc.rgba(56,45,85,255), recess=pc.rgba(36,31,58,255);
var light=pc.rgba(132,106,149,255), low=pc.rgba(78,61,108,255);
// Remove the generated four-pixel side band: the horizontal repeat is a regular
// narrow mortar joint, not a vertical bright stripe on each module.
for(var y=0;y<32;y++){
 for(var x=0;x<4;x++)im.putPixel(x,y,x===0?dark:(x===1&&y<28?light:(y>=29?low:base)));
 for(var x=124;x<128;x++)im.putPixel(x,y,y>=29?low:base);
}
// Deep near-black speckles read like holes. Retain connected slab joints, quiet
// the incidental isolated cracks, and reduce the front face to a subdued plane.
for(var y=0;y<32;y++)for(var x=4;x<124;x++){
 var p=im.getPixel(x,y);
 if(pc.rgbaR(p)===21)im.putPixel(x,y,(x===43||x===85)?recess:low);
 if(y>=29&&pc.rgbaR(p)===56)im.putPixel(x,y,low);
}
spr.layer(0).name='PixelLab slab surface - native seam and cluster cleanup';
spr.commit();spr.saveAs(root+'Native/Floor_Standing.ase',true);
spr.saveAs(root+'Exports/Floor_Standing.png',true);app.activeDocument.close();
app.open(root+'Native/Floor_Standing.ase');
app.activeSprite.saveAs(root+'Roundtrip/Floor_Standing.png',true);
app.activeDocument.close();console.log('FLOOR_NATIVE_ROUNDTRIP_COMPLETE');app.command.Exit();
