var root="SET_ON_BUILD";
var pc=app.pixelColor;
var source=[];
function col(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function sample(x,y){var v=source[y][x],b=pc.rgbaB(v);return col(b>240?"ecfafd":b>190?"b8d9e8":"668fa6");}
function save(name,pixels){
 app.open(root+"Raw/Blank8.png");var s=app.activeSprite,im=s.layer(0).cel(0).image;
 s.layer(0).name="PixelLab motif / hand-cleaned native effect";
 pixels.forEach(function(p){im.putPixel(p[0],p[1],p[2]);});
 s.commit();s.saveAs(root+"Native/"+name+".ase",true);s.saveAs(root+"Exports/"+name+".png",true);app.activeDocument.close();
 app.open(root+"Native/"+name+".ase");app.activeSprite.saveAs(root+"Roundtrip/"+name+".png",true);app.activeDocument.close();console.log("SNOW8_MODULE "+name+" PASS");
}
try{
 app.open(root+"Raw/SnowEffects.png");var raw=app.activeSprite.layer(0).cel(0).image;
 for(var y=0;y<32;y++){var row=[];for(var x=0;x<32;x++)row.push(raw.getPixel(x,y));source.push(row);}app.activeDocument.close();
 // Select the generated motifs' connected cores, removing gray background and fringe shades.
 // Do not resize: every output texel is a deliberately chosen native texel.
 save("MoteSmall",[[3,3,sample(8,7)],[3,4,sample(8,8)]]);
 save("MoteWide",[[4,2,sample(24,6)],[3,3,sample(23,7)],[2,4,sample(22,8)]]);
 save("Snowflake",[[3,2,sample(8,20)],[2,3,sample(7,21)],[3,3,sample(8,21)],[4,3,sample(9,21)],[3,4,sample(8,22)]]);
 var ramps=[0,1,2,1,0,-1];
 for(var f=0;f<6;f++){
   var r=ramps[f],p=[];
   if(r>=0){
     p.push([3,3,col("ecfafd")]);
     if(r>=1){p.push([2,3,sample(21,22)],[4,3,sample(23,22)],[3,2,sample(22,21)],[3,4,sample(22,23)]);}
     if(r>=2){p.push([1,3,col("668fa6")],[5,3,col("668fa6")],[3,1,col("b8d9e8")],[3,5,col("668fa6")]);}
   }
   save("Glint_0"+f,p);
 }
 console.log("SNOW8_LIBRESPRITE_PASS");
}catch(e){console.log("SNOW8_FAIL "+e);}
app.command.Exit();
