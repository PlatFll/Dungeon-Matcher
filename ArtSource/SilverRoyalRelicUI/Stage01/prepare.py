"""Read-only production audit and copied Stage 1 references. Never edits Assets."""
from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json,shutil,subprocess
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def info(p):
    im=Image.open(p).convert('RGBA')
    return {'source':str(p),'sha256':sha(p),'dimensions':list(im.size),'bounds':im.getbbox(),
            'opaque_colors':len({c for c in im.get_flattened_data() if c[3]==255}),
            'alpha':sorted(set(im.getchannel('A').get_flattened_data()))}
for folder in ('Inputs','Raw','Cleaned','Editable','Roundtrip','Preview','QA'):(HERE/folder).mkdir(exist_ok=True)
refs={
 'Rattlebones':('Docs/ArtDirection/Audit2026-10/media/skeleton-still.png','705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c'),
 'RattlebonesProduction':('Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png','59b04929e9fdc15ae9f1d70fd3c85c6ea5362bdac479eb12f45cfcbb0efe4306'),
 'Farmer':('Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png','6189bc2cfe01bab84bafada24a7b129a3fd8c2225f629eb8580386f461109e2d'),
 'Pan':('Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png','e1be89f8231483df5db6739dd2cae7bef83e11aaa9b028e4cb1b60ea55aba0ca')}
lineage={}
for name,(rel,expected) in refs.items():
    p=ROOT/rel;assert sha(p)==expected,(name,sha(p));lineage[name]=info(p)
    if name!='RattlebonesProduction':shutil.copy2(p,HERE/'Inputs'/f'{name}.png')
for name,rel in {
 'Wave':'Assets/_Game/Resources/UI/BattlePresentation/Stone/WaveBody.png',
 'Settings':'Assets/_Game/Resources/UI/BattlePresentation/Stone/Settings.png',
 'Supply':'Assets/_Game/Resources/UI/BattlePresentation/Stone/Supply.png',
 'Ability':'Assets/_Game/Resources/UI/BattlePresentation/Stone/AbilityPlate.png',
 'Energy':'Assets/_Game/Resources/UI/BattlePresentation/ReferenceDungeon/SharedEnergy.png',
 'Emerald':'Assets/_Game/Art/Gems/Emerald.png',
 'Ruby':'Assets/_Game/Art/Gems/Ruby.png',
}.items():
    p=ROOT/rel;lineage[name]=info(p);shutil.copy2(p,HERE/'Inputs'/f'Current{name}.png')
target=Path('C:/Users/USER/AppData/Local/Temp/codex-clipboard-89acfda4-cecf-446b-8ebd-dabe4ba31b95.png')
shutil.copy2(target,HERE/'Inputs'/'UserTarget.png');lineage['UserTarget']=info(target)
brief=Path('C:/Users/USER/.codex/attachments/081cb624-9536-4b0f-aad0-2790f9b9aba3/Pasted text.txt')
shutil.copy2(brief,HERE/'Brief.txt')
for rel in ['Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','Docs/ArtDirection/Golden_Reference_Register.md']:
    lineage[rel]={'sha256':sha(ROOT/rel)}
(HERE/'QA'/'ReferenceLineage.json').write_text(json.dumps(lineage,indent=2)+'\n')
protected={}
for rel in ['Assets','ProjectSettings','Packages']:
    for p in (ROOT/rel).rglob('*'):
        if p.is_file():protected[p.relative_to(ROOT).as_posix()]=sha(p)
(HERE/'QA'/'ProtectedProduction.json').write_text(json.dumps(protected,indent=2)+'\n')
base=subprocess.check_output(['rtk','proxy','git','rev-parse','HEAD'],cwd=ROOT).decode().strip()
(HERE/'QA'/'StartingState.json').write_text(json.dumps({'base':base,'status':subprocess.check_output(['rtk','proxy','git','status','--porcelain'],cwd=ROOT).decode()},indent=2)+'\n')
board=Image.new('RGBA',(600,300),'#20202c');draw=ImageDraw.Draw(board)
x=12
for name in ('Rattlebones','Farmer','Pan'):
    im=Image.open(HERE/'Inputs'/f'{name}.png').convert('RGBA');board.alpha_composite(im,(x,24));draw.text((x,6),name,fill='white');x+=130
for i,name in enumerate(('Wave','Settings','Supply','Ability','Energy')):
    im=Image.open(HERE/'Inputs'/f'Current{name}.png').convert('RGBA');x=12+(i%3)*190;y=140+(i//3)*85
    draw.text((x,y-14),name,fill='white');board.alpha_composite(im,(x,y))
board.save(HERE/'Preview'/'CurrentReferences.png')
print('Verified four golden image hashes; copied references. Production snapshot:',len(protected),'files.')
print(json.dumps({n:r.get('dimensions') for n,r in lineage.items() if 'dimensions' in r}))
