var root="SET_ON_BUILD";
var pc=app.pixelColor;
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ("0"+pc.rgbaR(v).toString(16)).slice(-2)+("0"+pc.rgbaG(v).toString(16)).slice(-2)+("0"+pc.rgbaB(v).toString(16)).slice(-2);}
var ramp={"020001":"0a0d11","020204":"0a0d11","0b080a":"0a0d11","131b28":"192330","45322b":"4d382d","61493b":"624937","392621":"3d2c27","2e1b1b":"302627","e6f9fc":"ecfafd","b0cede":"b8d9e8","8dd3d4":"83bedc","57a9b5":"4a8db5","98e1d8":"83bedc","c5f1f1":"c5eafa","75b9c1":"649fbe"};
function clean(v){if(pc.rgbaA(v)<128)return 0;var h=hex(v);return rgb(ramp[h]||h);}
function save(name,s){s.layer(0).name="PixelLab shape / native material cleanup";s.commit();s.saveAs(root+"Native/"+name+".ase",true);s.saveAs(root+"Exports/"+name+".png",true);app.activeDocument.close();app.open(root+"Native/"+name+".ase");app.activeSprite.saveAs(root+"Roundtrip/"+name+".png",true);app.activeDocument.close();console.log("SNOW6_MODULE "+name+" PASS");}
try{
 app.open(root+"Raw/SelectedKit.png");var src=app.activeSprite.layer(0).cel(0).image,a=[];
 for(var y=0;y<128;y++){var row=[];for(var x=0;x<128;x++)row.push(src.getPixel(x,y));a.push(row);}app.activeDocument.close();
 function part(name,x,y,w,h,cols,flip){var width=cols?cols.length:w;app.open(root+"Raw/Blank"+(width+2)+"x"+(h+2)+".png");var s=app.activeSprite,im=s.layer(0).cel(0).image;
  for(var j=0;j<h;j++)for(var i=0;i<width;i++)im.putPixel(1+i,1+j,clean(a[y+j][x+(cols?cols[i]:(flip?w-1-i:i))]));
  save(name,s);
 }
 part("Support_Right",14,6,33,68,null,false);part("Support_Left",14,6,33,68,null,true);
 part("Lantern",57,7,19,44,null,false);
 part("Banner",86,7,38,68,null,false);
 // Remove six redundant interior rock-plane columns without resampling edges or snow.
 var columns=[];for(var i=0;i<42;i++)if([12,13,20,21,30,31].indexOf(i)<0)columns.push(i);
 part("SnowRock",5,82,42,39,columns,false);
 part("IceCluster",51,69,37,52,null,false);
 part("RopePosts",94,86,30,33,null,false);
 part("TimberRepeat",15,54,12,16,null,false);
 console.log("SNOW6_LIBRESPRITE_PASS");
}catch(e){console.log("SNOW6_FAIL "+e);}
app.command.Exit();
