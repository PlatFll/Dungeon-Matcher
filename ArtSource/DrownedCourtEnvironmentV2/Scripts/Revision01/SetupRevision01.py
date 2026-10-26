from pathlib import Path
from PIL import Image
import hashlib,json,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for d in ['Native','Exports','Preview','QA','Scripts']:(R/d/'Revision01').mkdir(parents=True,exist_ok=True)
Q=R/'QA/Revision01'
if not (Q/'AcceptedStagesHashes.json').exists():
 paths=[p for p in R.rglob('*') if p.is_file() and any(f'Stage{s:02d}' in p.parts for s in range(1,9))]
 paths+=list((F/'Assets/_Game/Art/DrownedCourtEnvironmentV2').rglob('*'))
 paths+=[p for p in (F/'Assets/_Game/Scenes').glob('ArtPreview_DrownedCourtV2*') if p.is_file()]
 (Q/'AcceptedStagesHashes.json').write_text(json.dumps({p.relative_to(F).as_posix():sha(p) for p in paths if p.is_file()},indent=2))
auth=['Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','Docs/ArtDirection/Golden_Reference_Register.md','Docs/ArtDirection/Audit2026-10/media/skeleton-still.png','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png']
(Q/'AuthorityHashes.json').write_text(json.dumps({p:sha(F/p) for p in auth},indent=2))
stone={'081e2b':'062432','0f3141':'083d50','164759':'0d5368','205b69':'176c7d','286b78':'258999','367d86':'3aa3ae','518f94':'65c1be','1a4b59':'105568','2c6c76':'237b8b','397c83':'3695a1','123d4c':'0a4256','225b68':'166477','4b9092':'48a7af'}
water={'061824':'032332','08202c':'042f42','0b2935':'043c53','0f333e':'05516a','143f49':'07657e','1b4d56':'0a7b91','245e64':'138fa2','317174':'25a5b4',
'0e303e':'074961','123c49':'0a5970','174954':'0e6b7f','1d575f':'187e8d',
'071c28':'042736','0a2733':'053748','0f343f':'08495a','16424c':'0d5c6b','1f525a':'1b7280',
'0c2b37':'07465a','103540':'095c70','113742':'0b5e73','16434d':'0c7184','18454f':'0e7486','1f535c':'158797','20555e':'188a9a','225760':'1c8c9c','29666c':'299eaa','2b686e':'2ca2ae','2c6a70':'30a5b0','387b7e':'49b9c1','397d80':'4cbdc4','458188':'71cbd2'}
recess={'08151e':'061c29','0c1f2b':'092a38','102a38':'0c3a49','153542':'124a58'}
jobs=[]
for st,mapping in [('01',water),('02',stone),('03',stone),('04',recess),('07',stone)]:
 for p in sorted((R/f'Exports/Stage{st}').glob('*.png')):
  if st=='04' and not p.stem.startswith('Recess'):continue
  name='S'+st+'_'+p.stem;im=Image.open(p)
  jobs.append(dict(name=name,source=p.relative_to(F).as_posix(),oldAsset=f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage{st}/{p.name}',width=im.width,height=im.height,map=mapping,source_sha256=sha(p)))
(R/'Native/Revision01/RecolorPlan.json').write_text(json.dumps(jobs,indent=2)+'\n')
script="""var root=ROOT, jobs=JOBS, pc=app.pixelColor;
function color(h){return pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
try{jobs.forEach(function(j){app.open(root+j.source);var s=app.activeSprite,im=app.activeImage;if(s.width!==j.width||s.height!==j.height)throw Error(j.name+' dimensions');var map={};for(var key in j.map)map[color(key)]=color(j.map[key]);
 for(var y=0;y<s.height;y++)for(var x=0;x<s.width;x++){var c=im.getPixel(x,y);if(pc.rgbaA(c)&&map[c]!==undefined)im.putPixel(x,y,map[c]);}
 s.layer(0).name='Exact material recolor - original geometry';s.commit();s.saveAs(root+'ArtSource/DrownedCourtEnvironmentV2/Native/Revision01/'+j.name+'.ase',true);s.saveAs(root+'ArtSource/DrownedCourtEnvironmentV2/Exports/Revision01/'+j.name+'.png',true);app.activeDocument.close();console.log('RECOLORED '+j.name);
});console.log('BACKGROUND_RECOLOR_COMPLETE');}catch(e){console.log('BACKGROUND_RECOLOR_FAILED '+e);}app.command.Exit();
""".replace('ROOT',json.dumps(F.as_posix()+'/')).replace('JOBS',json.dumps(jobs))
(R/'Scripts/Revision01/RecolorBackground.js').write_text(script)
# Empty working documents only. All actual native artwork is drawn in LibreSprite.
for n,w,h in [('CapLeft',31,16),('CapRight',31,16),('BaseLeft',31,18),('BaseRight',31,18),('ColumnLeftA',31,96),('ColumnLeftB',31,96),('ColumnRightA',31,96),('ColumnRightB',31,96),('QuietLeftA',31,32),('QuietLeftB',31,32),('QuietRightA',31,32),('QuietRightB',31,32),('ShortLeft',31,18),('ShortRight',31,18)]:
 Image.new('RGBA',(w,h)).save(R/f'Native/Revision01/Work-{n}.png')
print('Prepared',len(jobs),'exact recolors; previous sources snapshotted.')
