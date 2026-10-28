from pathlib import Path
from PIL import Image
import json,hashlib,shutil,subprocess
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Revision03'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
state=subprocess.check_output(['git','status','--porcelain'],cwd=F,text=True).strip()
for d in ['Prepared','Native','Exports','Roundtrip','Scripts','Preview','QA']:(R/d).mkdir(parents=True,exist_ok=True)
locks=json.loads((R.parent/'Revision02/QA/ApprovedSourceLock.json').read_text())
for folder in [F/'Assets/_Game/Art/IronveinBelowTopHUD/Revision02',R.parent/'Revision02/Exports',R.parent/'Revision02/Native']:
 for p in folder.rglob('*'):
  if p.is_file():locks[p.relative_to(F).as_posix()]=sha(p)
for rel in ['Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision02.unity','Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision02.unity.meta','Assets/_Game/Scripts/Editor/IronveinBelowTopHUDRevision02.cs']:locks[rel]=sha(F/rel)
(R/'QA/ApprovedSourceLock.json').write_text(json.dumps(locks,indent=2))
(R/'QA/StartingState.json').write_text(json.dumps(dict(head=subprocess.check_output(['git','rev-parse','HEAD'],cwd=F,text=True).strip(),initial_status=state,scope='Broad walking slab surface modeled on approved underwater floor. Separator locked. User explicitly retains the exact enemy floor line; no actor/UI movement.',new_generation_spending=0),indent=2))
refs=json.loads((R.parent/'Revision02/QA/GoldenInspection.json').read_text());assert all(sha(F/p)==h for p,h in refs.items());(R/'QA/GoldenInspection.json').write_text(json.dumps(refs,indent=2))
sources={}
for v in 'ABC':
 for course in ['Rear','Front']:
  rel='Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision04/Floor'+course+v+'.png';p=F/rel;shutil.copyfile(p,R/'Prepared'/p.name);sources[rel]=sha(p)
 Image.new('RGBA',(64,32)).save(R/'Prepared'/('WalkingSlab'+v+'.png'))
js=r'''
var root=__ROOT__,pc=app.pixelColor,src={};
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
try{
 ['FloorRearA','FloorFrontA','FloorRearB','FloorFrontB','FloorRearC','FloorFrontC'].forEach(function(n){app.open(root+'Prepared/'+n+'.png');var s=app.activeSprite,c=s.layer(0).cel(0),im=c.image;src[n]=[];for(var y=0;y<s.height;y++){src[n][y]=[];for(var x=0;x<s.width;x++)src[n][y][x]=(x>=c.x&&y>=c.y&&x<c.x+im.width&&y<c.y+im.height)?im.getPixel(x-c.x,y-c.y):0;}app.activeDocument.close();});
 // Preserve the approved underwater surface geometry, with a cool cave-stone ramp.
 var ramp={'083d50':'10131f','0d5368':'1b2030','176c7d':'252b39','258999':'36435a','3aa3ae':'46556e'};
 ['A','B','C'].forEach(function(v){var name='WalkingSlab'+v;app.open(root+'Prepared/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
  for(var y=0;y<32;y++)for(var x=0;x<64;x++){
   var c=src['Floor'+(y<16?'Rear':'Front')+v][y%16][x];var h=hex(c);if(!ramp[h])throw new Error('Unexpected source color '+h);im.putPixel(x,y,rgb(ramp[h]));
  }
  // Identical edge colors keep all three native slab variants compatible.
  for(var y=0;y<32;y++){var c=y===31?'36435a':(y===30?'46556e':'1b2030');im.putPixel(0,y,rgb(c));im.putPixel(63,y,rgb(c));}
  s.layer(0).name='Cave slab - native underwater-floor adaptation';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
  app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('REVISION03_CLEAN '+name);
 });console.log('REVISION03_CLEANUP_PASS');
}catch(e){console.log('REVISION03_CLEANUP_FAIL '+e);}app.command.Exit();
'''.replace('__ROOT__',json.dumps(R.as_posix()+'/'))
(R/'Scripts/Cleanup.js').write_text(js);shutil.copyfile(R.parent/'Revision02/Scripts/RunLibreSprite.ps1',R/'Scripts/RunLibreSprite.ps1');shutil.copyfile(__file__,R/'Scripts/PrepareNative.py')
(R/'Lineage.json').write_text(json.dumps(dict(method='Native LibreSprite adaptation of approved PixelLab-derived Court floor modules. Rear and front courses joined without scaling, cave palette mapping, compatible tile-edge cleanup.',sources=sources,reference_preview='ArtSource/DrownedCourtEnvironmentV2/Preview/Revision04/Unity-RevisedOpen-720x1280.png',native_tile=[64,32],source_rows='Rear 0-15 followed by Front 0-15; no resampling',floor_start=174,floor_end=206,separator_locked_band=[206,225],enemy_floor_line='Retained exactly per explicit user answer',pixellab_generations=0),indent=2))
print('Prepared three 64x32 walking-slab variants; separator and enemy floor line locked')
