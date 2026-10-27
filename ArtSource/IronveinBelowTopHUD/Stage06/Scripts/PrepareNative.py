from pathlib import Path
from PIL import Image
import json,shutil,sys
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage06'
selected=int(sys.argv[1]) if len(sys.argv)>1 else json.loads((R/'Generation.json').read_text())['selected_candidate']
shutil.copyfile(R/'Raw'/f'Bracket-{selected:02}.png',R/'Prepared/SelectedCorbel.png')
shutil.copyfile(R.parent/'Stage02/Exports/BeamRepeat.png',R/'Prepared/ApprovedTimber.png')
sizes={'CorbelLeft':(32,32),'CorbelRight':(32,32),'SeatRail':(24,7),'SeatPost':(7,24),'SeatBolt':(5,5)}
for n,sz in sizes.items():Image.new('RGBA',sz).save(R/'Prepared'/(n+'.png'))
js=r'''
var root=__ROOT__,pc=app.pixelColor,src={};
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
try{
['SelectedCorbel','ApprovedTimber'].forEach(function(n){app.open(root+'Prepared/'+n+'.png');var s=app.activeSprite,c=s.layer(0).cel(0),im=c.image;src[n]=[];for(var y=0;y<s.height;y++){src[n][y]=[];for(var x=0;x<s.width;x++)src[n][y][x]=(x>=c.x&&y>=c.y&&x<c.x+im.width&&y<c.y+im.height)?im.getPixel(x-c.x,y-c.y):0;}app.activeDocument.close();});
__NAMES__.forEach(function(name){
 app.open(root+'Prepared/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
 function px(x,y,v){if(x>=0&&y>=0&&x<s.width&&y<s.height)im.putPixel(x,y,typeof v==='string'?rgb(v):v);}
 function box(x1,y1,x2,y2,v){for(var y=y1;y<=y2;y++)for(var x=x1;x<=x2;x++)px(x,y,v);}
 if(name==='CorbelLeft'||name==='CorbelRight'){
  for(var y=0;y<32;y++)for(var x=0;x<32;x++){
   var v=src.SelectedCorbel[y][name==='CorbelLeft'?x:31-x];
   if(pc.rgbaA(v)>=128)px(x,y,pc.rgba(pc.rgbaR(v),pc.rgbaG(v),pc.rgbaB(v),255));
  }
 }
 var rows=[0,1,2,4,6,8,9];
 if(name==='SeatRail'){
  for(var y=0;y<7;y++)for(var x=0;x<24;x++)px(x,y,src.ApprovedTimber[rows[y]][x]);
  for(var y=0;y<7;y++)px(23,y,im.getPixel(0,y));
 }
 if(name==='SeatPost'){
  for(var y=0;y<24;y++)for(var x=0;x<7;x++)px(x,y,src.ApprovedTimber[rows[x]][y]);
  for(var x=0;x<7;x++)px(x,23,im.getPixel(x,0));
 }
 if(name==='SeatBolt'){
  box(0,0,4,4,'0a0d11');box(1,1,3,3,'303447');box(1,1,3,1,'55606e');px(1,2,'55606e');px(2,2,'c58826');px(2,1,'fbea90');px(2,3,'8a5622');
 }
 s.layer(0).name='Stage06 native HUD support';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
 app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('STAGE06_CLEAN '+name);
});console.log('STAGE06_CLEANUP_PASS');}catch(e){console.log('STAGE06_CLEANUP_FAIL '+e);}app.command.Exit();
'''.replace('__ROOT__',json.dumps(R.as_posix()+'/')).replace('__NAMES__',json.dumps(list(sizes)))
(R/'Scripts/Cleanup.js').write_text(js);shutil.copyfile(R.parent/'Stage05/Scripts/RunLibreSprite.ps1',R/'Scripts/RunLibreSprite.ps1')
target=R/'Scripts/PrepareNative.py'
if Path(__file__).resolve()!=target.resolve():shutil.copyfile(__file__,target)
g=json.loads((R/'Generation.json').read_text());g['selected_candidate']=selected;g['cleanup']='Native binary-alpha corbel, mirrored partner, shallow rails adapted by removing redundant rows from an unchanged copy of approved Stage02 timber; exact repeat-seam repair, separate iron/amber bolt. No scaling or interpolation.';(R/'Generation.json').write_text(json.dumps(g,indent=2))
print('Prepared five native modules')
