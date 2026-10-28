from pathlib import Path
from PIL import Image
import hashlib,json,shutil,subprocess
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Revision02';W=Path(__file__).parent
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
initial=subprocess.check_output(['git','status','--porcelain'],cwd=F,text=True).strip()
for d in ['Prepared','Native','Exports','Roundtrip','Scripts','Preview','QA']:(R/d).mkdir(parents=True,exist_ok=True)
locks=json.loads((R.parent/'Revision01/QA/ApprovedSourceLock.json').read_text())
for folder in [F/'Assets/_Game/Art/IronveinBelowTopHUD/Revision01',R.parent/'Revision01/Exports',R.parent/'Revision01/Native']:
 for p in folder.rglob('*'):
  if p.is_file():locks[p.relative_to(F).as_posix()]=sha(p)
for name in ['Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision01.unity','Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision01.unity.meta','Assets/_Game/Scripts/Editor/IronveinBelowTopHUDRevision01.cs']:
 locks[name]=sha(F/name)
(R/'QA/ApprovedSourceLock.json').write_text(json.dumps(locks,indent=2))
(R/'QA/StartingState.json').write_text(json.dumps(dict(head=subprocess.check_output(['git','rev-parse','HEAD'],cwd=F,text=True).strip(),branch='codex/ironvein-below-tophud-pixellab',initial_status=initial,scope='Only upper walking plane and stone separator. Increase surface depth into the cave, recolor near-black, replace front face with lighter large stone blocks. Preserve actor floor line and every other placement. No merge.'),indent=2))
refs=json.loads((R.parent/'Revision01/QA/GoldenInspection.json').read_text())
for p,h in refs.items():assert sha(F/p)==h,p
(R/'QA/GoldenInspection.json').write_text(json.dumps(refs,indent=2))
for n in ['FloorSurfaceA','FloorSurfaceB']:shutil.copyfile(R.parent/'Revision01/Exports'/(n+'.png'),R/'Prepared'/('Source'+n+'.png'))
sizes={'FloorSurfaceA':(32,20),'FloorSurfaceB':(32,20),'SeparatorBlockA':(40,19),'SeparatorBlockB':(40,19)}
for n,sz in sizes.items():Image.new('RGBA',sz).save(R/'Prepared'/(n+'.png'))
js=r'''
var root=__ROOT__,pc=app.pixelColor,src={};
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
function hex(v){return ('0'+pc.rgbaR(v).toString(16)).slice(-2)+('0'+pc.rgbaG(v).toString(16)).slice(-2)+('0'+pc.rgbaB(v).toString(16)).slice(-2);}
try{
 ['FloorSurfaceA','FloorSurfaceB'].forEach(function(n){app.open(root+'Prepared/Source'+n+'.png');var s=app.activeSprite,c=s.layer(0).cel(0),im=c.image;src[n]=[];for(var y=0;y<s.height;y++){src[n][y]=[];for(var x=0;x<s.width;x++)src[n][y][x]=(x>=c.x&&y>=c.y&&x<c.x+im.width&&y<c.y+im.height)?im.getPixel(x-c.x,y-c.y):0;}app.activeDocument.close();});
 __NAMES__.forEach(function(name){
  app.open(root+'Prepared/'+name+'.png');var s=app.activeSprite,im=s.layer(0).cel(0).image;
  function px(x,y,v){if(x>=0&&y>=0&&x<s.width&&y<s.height)im.putPixel(x,y,typeof v==='string'?rgb(v):v);}
  function box(x1,y1,x2,y2,v){for(var y=y1;y<=y2;y++)for(var x=x1;x<=x2;x++)px(x,y,v);}
  if(name.indexOf('FloorSurface')===0){
   box(0,0,31,19,'10131f');
   // Add native rows behind the old walking edge, with broad cave-tone planes.
   box(0,4,31,10,'171e2e');box(0,11,31,18,'171e2e');
   var b=name==='FloorSurfaceB';
   box(b?2:8,2,b?19:26,3,'171e2e');
   box(b?4:10,3,b?22:29,4,'171e2e');
   box(b?17:3,8,b?29:13,9,'10131f');
   box(b?20:5,10,b?31:16,10,'10131f');
   var ramp={'665b51':'171e2e','8b7861':'242e42','49424a':'10131f','464052':'1c1c2b'};
   for(var y=0;y<8;y++)for(var x=0;x<32;x++)px(x,y+12,ramp[hex(src[name][y][x])]||'171e2e');
   // Keep both ends identical for an uninterrupted native repeat.
   for(var y=0;y<20;y++){var c=y<4?'10131f':y<18?'171e2e':'10131f';px(0,y,c);px(31,y,c);}
  }else{
   // One broad hand-shaped charcoal block, not a stretched procedural rectangle.
   // Four restrained planes and two-pixel joints stay readable behind the bars.
   box(0,0,39,18,'171c28');box(2,1,37,17,'353d4b');
   box(2,2,37,14,'454e5d');box(3,1,36,2,'626c7a');
   box(1,3,2,15,'505b6a');box(3,3,36,4,'505b6a');
   box(37,3,38,15,'2a303d');box(3,15,36,16,'353d4b');box(4,17,35,17,'2a303d');
   // A few connected chips; no scattered texture.
   if(name==='SeparatorBlockA'){
    box(27,1,30,1,'353d4b');box(28,2,30,2,'505b6a');box(8,14,11,14,'353d4b');box(7,15,10,15,'2a303d');
   }else{
    box(9,1,11,1,'353d4b');box(10,2,12,2,'505b6a');box(25,13,27,13,'353d4b');box(27,14,31,14,'353d4b');
   }
  }
  s.layer(0).name='Revision02 cave floor and block separator';s.commit();s.saveAs(root+'Native/'+name+'.ase',true);s.saveAs(root+'Exports/'+name+'.png',true);app.activeDocument.close();
  app.open(root+'Native/'+name+'.ase');app.activeSprite.saveAs(root+'Roundtrip/'+name+'.png',true);app.activeDocument.close();console.log('REVISION02_CLEAN '+name);
 });console.log('REVISION02_CLEANUP_PASS');
}catch(e){console.log('REVISION02_CLEANUP_FAIL '+e);}app.command.Exit();
'''.replace('__ROOT__',json.dumps(R.as_posix()+'/')).replace('__NAMES__',json.dumps(list(sizes)))
(R/'Scripts/Cleanup.js').write_text(js)
shutil.copyfile(R.parent/'Revision01/Scripts/RunLibreSprite.ps1',R/'Scripts/RunLibreSprite.ps1')
shutil.copyfile(__file__,R/'Scripts/PrepareNative.py')
(R/'Lineage.json').write_text(json.dumps(dict(method='Direct native LibreSprite edit of copied Revision01 PixelLab floor; separate hand-authored separator blocks.',pixellab_generations=0,source_job='c0054523-2015-4f7c-8775-6c0c5b91304f',source_images={n:sha(R.parent/'Revision01/Exports'/(n+'.png')) for n in ['FloorSurfaceA','FloorSurfaceB']},palette_reference='Assets/_Game/Art/IronveinStaged/CavernDepth.png',palette_reference_sha256=sha(F/'Assets/_Game/Art/IronveinStaged/CavernDepth.png'),geometry='Surface depth 8 to 20 native pixels, back edge 198 to 186; front edge 206 and separator bottom 225 fixed. Enemy/UI transforms unchanged.',no_resampling=True),indent=2))
print('Prepared four new modules; zero generation spending; source originals locked')
