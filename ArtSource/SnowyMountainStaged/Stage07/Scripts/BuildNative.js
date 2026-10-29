var root="SET_ON_BUILD";
var pc=app.pixelColor;
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ("0"+pc.rgbaR(v).toString(16)).slice(-2)+("0"+pc.rgbaG(v).toString(16)).slice(-2)+("0"+pc.rgbaB(v).toString(16)).slice(-2);}
var rock={};
function group(map,values,to){values.split(" ").forEach(function(h){map[h]=to;});}
group(rock,"01020a 020508 000104 030810 020408","0a0d11");
group(rock,"eaf9ff eefbfe e9f9fe e2f7fd d6edf7","ecfafd");
group(rock,"b6d5eb b2d3ec b2d2ea c0dcef","b8d9e8");
group(rock,"446177 456379 49697e 37536a 3e5b71 3b566d 47667b","35526c");
group(rock,"618596 658798 5b7e90 597a8c","577993");
group(rock,"263d55 334e66 2d465e 273f57","2b445e");
group(rock,"21364e 040c1a 12253e 07172e 1e334c 142841","1d3245");
group(rock,"96b7d0 7096b3","668fa6");
var crate={};
group(crate,"15152a","0a0d11");
group(crate,"23232f 1a1d26 1a1a2a 161926","192330");
group(crate,"2a2938 2e2d39","293b4b");
group(crate,"495a58 3e4242 647182 758a8e","526c7c");
group(crate,"8098b3 8ba3bb","839ca9");
group(crate,"f7f9fc d6e5ed e5f1f4 dbe9ef","ecfafd");
group(crate,"aec8d9 c5d9e4 a4bdd0 9cb3ca","b8d9e8");
group(crate,"272128 2e242a 352525 32252a 392828 2e2326 37282a","302627");
group(crate,"422c28 3d2a28 472d26 4d2f24","3d2c27");
group(crate,"623621 5e3521 573322","4d382d");
group(crate,"683921 6e3d23 76411c","624937");
group(crate,"874e18 d2812f 915519","7b6047");
function save(name,s){s.layer(0).name="PixelLab source / native material cleanup";s.commit();s.saveAs(root+"Native/"+name+".ase",true);s.saveAs(root+"Exports/"+name+".png",true);app.activeDocument.close();app.open(root+"Native/"+name+".ase");app.activeSprite.saveAs(root+"Roundtrip/"+name+".png",true);app.activeDocument.close();console.log("SNOW7_MODULE "+name+" PASS");}
function module(source,name,x,y,w,h,map){
 app.open(root+"Raw/"+source+".png");var im=app.activeSprite.layer(0).cel(0).image,a=[];
 for(var j=0;j<h;j++){var row=[];for(var i=0;i<w;i++)row.push(im.getPixel(x+i,y+j));a.push(row);}app.activeDocument.close();
 app.open(root+"Raw/Blank"+(w+2)+"x"+(h+2)+".png");var s=app.activeSprite,im=s.layer(0).cel(0).image;
 for(var j=0;j<h;j++)for(var i=0;i<w;i++){var v=a[j][i];if(pc.rgbaA(v)){var c=hex(v);if(!map[c])throw "Unmapped color "+source+" "+c;im.putPixel(i+1,j+1,rgb(map[c]));}}
 save(name,s);
}
try{
 module("SelectedLeftRock","LowerRock_Left",3,5,60,53,rock);
 module("SelectedRightRock","LowerRock_Right",2,5,60,55,rock);
 module("Crate","SupplyCrate",3,3,34,33,crate);
 app.open(root+"Raw/ExistingIce.png");save("LowerIce",app.activeSprite);
 console.log("SNOW7_LIBRESPRITE_PASS");
}catch(e){console.log("SNOW7_FAIL "+e);}
app.command.Exit();
