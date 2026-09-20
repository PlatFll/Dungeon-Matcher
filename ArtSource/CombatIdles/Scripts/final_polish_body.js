// Native LibreSprite edits, scoped to the scarf tail and two cheek contours.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/CombatIdles/';
function idx(x,y){return(y*64+x)*4;}
function put(a,x,y,h){var i=idx(x,y);if(h==='.') {for(var c=0;c<4;c++)a[i+c]=0;return;}a[i]=parseInt(h.substr(0,2),16);a[i+1]=parseInt(h.substr(2,2),16);a[i+2]=parseInt(h.substr(4,2),16);a[i+3]=255;}
app.open(ROOT+'Originals/BeforeFinalPolish/Farmer_Idle.aseprite');
var farmer=app.activeSprite;farmer.saveAs(ROOT+'Review/Farmer_FinalPolish.aseprite',false);
// Extend the lower cheeks by one native pixel, retain eyes, mouth and chin height.
var cheekEdits=[[21,31,'0A0D11'],[22,31,'E6B08A'],[22,32,'0A0D11'],[23,32,'E6B08A'],[22,33,'0A0D11'],[23,33,'E6B08A'],[24,33,'E6B08A'],[24,34,'0A0D11'],[26,34,'E6B08A'],[27,34,'E6B08A'],[38,33,'E6B08A'],[39,33,'B9825D'],[40,33,'0A0D11'],[37,34,'E6B08A']];
for(var j=0;j<2;j++){var f=j?8:0,im=farmer.layer(0).cel(f).image,a=im.getImageData();for(var i=0;i<cheekEdits.length;i++){var e=cheekEdits[i];put(a,e[0],e[1],e[2]);}im.putImageData(a);}
farmer.commit();farmer.save();
app.open(ROOT+'Originals/BeforeFinalPolish/PanVillager_Idle.aseprite');
var pan=app.activeSprite;pan.saveAs(ROOT+'Review/PanVillager_FinalPolish.aseprite',false);
var colors={A:'0A0D11',F:'5B5145',C:'3E312A',L:'9F907A',K:'9A856F','.':'.'};
var rest=['FA....','FA....','CLA...','ALA...','.KA...','.A....','......'];
var lifted=['FA....','FLA...','CLLA..','AALA..','..A...','......','......'];
var trailing=['FA....','FA....','CLA...','ALA...','.LA...','.KA...','.A....'];
var heads=[0,0,1,2,4,5,4,2,0],poses=[rest,rest,rest,lifted,lifted,rest,trailing,trailing,rest];
for(var f=0;f<9;f++){var im=pan.layer(0).cel(f).image,a=im.getImageData();
 for(var y=0;y<7;y++)for(var x=0;x<6;x++)if(rest[y][x]!=='.'||poses[f][y][x]!=='.')put(a,43+x,25+heads[f]+y,colors[poses[f][y][x]]);
 im.putImageData(a);
}
pan.commit();pan.save();app.command.GotoFirstFrame();app.command.PlayAnimation();
