// One-time preparation before the correction pass. Only our previous output
// tabs are saved to the correction archive and closed; user original tabs stay.
var ROOT='C:/UnityProjects/Dungeon Matcher/ArtSource/RemainingCast/';
var seen={};
for(var n=0;n<150;n++){
 var s=app.activeSprite;if(!s)break;
 var filename=s.filename.replace(/\\/g,'/');
 if(filename.indexOf(ROOT+'Idles/')===0){
  var base=filename.split('/').pop();
  s.saveAs(ROOT+'BeforeCorrection/Idles/'+base,false);app.activeDocument.close();
 }else{
  if(seen[filename])break;seen[filename]=true;app.command.GotoNextTab();
 }
}
