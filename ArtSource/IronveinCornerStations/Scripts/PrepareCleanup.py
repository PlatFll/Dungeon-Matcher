from pathlib import Path
from collections import Counter
import json,shutil
from PIL import Image
F=Path(__file__).resolve().parents[3];CW=Path(__file__).parent
root=F/'ArtSource/IronveinCornerStations'
selected={'OreCrate':('OreCrateWide',0),'FoundationLeft':('RockFoundation',11),'FoundationRight':('RockFoundation',9),'RailSleepers':('RailSleepers',21),'OreFragments':('OreFragments',37),'TimberSupport':('TimberSupport',7),'Pickaxe':('Pickaxe',0),'SupplyCrate':('SupplyCrate',28),'EmbeddedOre':('EmbeddedOre',25)}
notes={'OreCrate':'Wide practical crate; first 48px batch rejected as undersized. Broad faces avoid texture noise.', 'FoundationLeft':'Continuous level rock ledge; rejected detached rock islands and noisy strata.', 'FoundationRight':'Connected asymmetrical low stone wedge.', 'RailSleepers':'Two clear rails on joined sleepers; no floating components.', 'OreFragments':'Low integrated ore crevice, restrained warm accent.', 'TimberSupport':'Functional diagonal brace with two clear metal brackets; rejected noisy grain and disconnected feet.', 'Pickaxe':'Natural curved steel head and readable handle; rejected wooden/orange heads.', 'SupplyCrate':'Dark wood and iron corners; rejected pale all-metal and golden crates.', 'EmbeddedOre':'A few small embedded ore tips in one connected stone mass.'}
for d in ['Roundtrip','Prepared']: (root/d).mkdir(exist_ok=True)
manifest=[]
for name,(family,index) in selected.items():
 raw=root/'Raw'/family/f'{index:02d}.png';shutil.copyfile(raw,root/'Selected'/f'{name}.png')
 im=Image.open(raw).convert('RGBA');colors=Counter(im.getdata());print(name,im.size,im.getbbox(),len([c for c in colors if c[3]]))
 manifest.append(dict(name=name,family=family,index=index,nativeSize=list(im.size),selection=notes[name]))
(root/'Selected/Selection.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
# Generation receipts are preserved in Raw/GenerationReceipts.json.
ramp={'10131d':'10121b','1b2030':'1c1c2b','2b3044':'2c293d','414255':'42374f','57566b':'55606e','737383':'737784','241b20':'30202a','3c2929':'4a2c1c','5b3a2b':'6f3b1d','825334':'7a4d2e','ad7542':'b07a43','342a2d':'30202a','4a444d':'42374f','686472':'55606e','95929a':'93a1b0'}
script='var root='+json.dumps(str(root).replace('\\','/')+'/')+', names='+json.dumps(list(selected))+', ramp='+json.dumps(ramp)+''';
var pc=app.pixelColor;
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
try{names.forEach(function(name){
 app.open(root+'Selected/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image,changed=0;
 for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++){
  var v=im.getPixel(x,y),out=pc.rgbaA(v)<128?0:rgb(ramp[hex(v)]||hex(v));
  if(v!==out)changed++;im.putPixel(x,y,out);
 }
 // Remove only isolated interior texture flecks in large wood/stone planes.
 // Metal rivets, ore glints, tool edges and silhouette pixels are preserved.
 var edits=[],plane=['1c1c2b','2c293d','42374f','55606e','4a2c1c','6f3b1d','7a4d2e'];
 for(var y=1;y<im.height-1;y++)for(var x=1;x<im.width-1;x++){
  var v=im.getPixel(x,y);if(plane.indexOf(hex(v))<0||pc.rgbaA(v)!==255)continue;
  var counts={},opaque=0,best=v,mx=0;
  for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){
   if(dx===0&&dy===0)continue;var z=im.getPixel(x+dx,y+dy);if(pc.rgbaA(z)===255)opaque++;
   counts[z]=(counts[z]||0)+1;if(counts[z]>mx){mx=counts[z];best=z;}
  }
  if(opaque===8&&(counts[v]||0)===0&&mx>=6&&plane.indexOf(hex(best))>=0)edits.push([x,y,best]);
 }
 edits.forEach(function(e){im.putPixel(e[0],e[1],e[2]);});
 s.layer(0).name='PixelLab native / material-ramp cleanup';s.commit();
 s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
 app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();
 console.log('CORNER_CLEAN '+name+' recolor='+changed+' texture='+edits.length);
});console.log('CORNER_CLEANUP_PASS');}catch(e){console.log('CORNER_CLEANUP_FAIL '+e);}app.command.Exit();
'''
(root/'Scripts/Cleanup.js').write_text(script,encoding='utf-8')
ps='''$cornerRoot=(Split-Path -Parent $PSScriptRoot)
$cornerJob=Start-Process -FilePath 'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe' -ArgumentList @('--script',('"'+$cornerRoot+'/Scripts/Cleanup.js"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput ($cornerRoot+'/QA/LibreSprite.log') -RedirectStandardError ($cornerRoot+'/QA/LibreSprite-error.log')
$cornerJob.WaitForExit();$cornerJob.Refresh()
Get-Content ($cornerRoot+'/QA/LibreSprite.log') -Tail 15
Get-Content ($cornerRoot+'/QA/LibreSprite-error.log') -Tail 5
exit $cornerJob.ExitCode
'''
(root/'Scripts/RunLibreSprite.ps1').write_text(ps,encoding='utf-8')
