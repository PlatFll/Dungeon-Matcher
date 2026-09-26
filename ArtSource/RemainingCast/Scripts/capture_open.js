// Native LibreSprite capture. Save copies only; never edit or save source tabs.
var root='C:/UnityProjects/Dungeon Matcher/ArtSource/RemainingCast/Originals/';
var first=app.activeSprite.filename;
var seen={};
for(var n=0;n<100;n++){
 var s=app.activeSprite;
 if(s){
  var original=s.filename;
  if(seen[original])break;
  seen[original]=true;
  var base=original.replace(/\\/g,'/').split('/').pop().replace(/\.[^.]+$/,'');
  console.log('CAPTURE '+original+' '+s.width+'x'+s.height+' layers='+s.layerCount);
  s.saveAs(root+base+'.aseprite',true);
 }
 app.command.GotoNextTab();
}
console.log('Capture complete; original documents are unchanged.');
