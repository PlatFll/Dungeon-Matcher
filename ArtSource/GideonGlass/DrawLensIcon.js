// Native LibreSprite icon using the approved eye's brass and blue palette.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/GideonGlass/';
var HEX=['0A0D11','755335','B18A4B','DAC080','24425C','397BAB','A9E0FF'];
var colors=HEX.map(function(h){return [parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255];});
var data=new Uint8Array(64*64*4);
function pixel(x,y,c){for(var k=0;k<4;k++)data[(y*64+x)*4+k]=colors[c][k];}
function inside(x,y,r){var dx=Math.abs(x-31.5),dy=Math.abs(y-31.5);return dx<=r&&dy<=r&&dx+dy<=r*1.45;}
for(var y=0;y<64;y++)for(var x=0;x<64;x++){
 if(!inside(x,y,21))continue;
 var c=0;
 if(inside(x,y,19))c=(x+y<61)?3:1;
 if(inside(x,y,17))c=2;
 if(inside(x,y,15))c=0;
 if(inside(x,y,13))c=4;
 if(inside(x,y,11)&&y>=32)c=5;
 if(x>=25&&x<=32&&y>=23&&y<=30)c=6;
 pixel(x,y,c);
}
// A small top-left brass catch and lower lens rim echo the character's lens.
app.open(ROOT+'Gideon_Ready.aseprite');
var sprite=app.activeSprite;sprite.saveAs(ROOT+'Gideon_LensIcon.aseprite',false);
sprite.layer(0).cel(0).image.putImageData(data);sprite.layer(0).name='ChronoShutter lens';
sprite.commit();sprite.save();sprite.saveAs(ROOT+'Gideon_LensIcon.png',true);
app.command.Exit();
