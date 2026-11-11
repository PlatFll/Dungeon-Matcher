from pathlib import Path
import hashlib,json,shutil
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for d in ('Inputs','Raw','Editable','PNG','Roundtrip','QA','Preview'):(HERE/d).mkdir(exist_ok=True)
refs=json.loads((HERE.parent/'Stage01/QA/ReferenceLineage.json').read_text())
for name in ('Rattlebones','RattlebonesProduction','Farmer','Pan','Wave','Settings','Supply','Ability','Energy','Emerald','Ruby'):
    p=Path(refs[name]['source']);assert sha(p)==refs[name]['sha256'],name
    if name in ('Rattlebones','Farmer','Pan','Supply','Ability','Energy'):shutil.copy2(p,HERE/'Inputs'/f'{name}.png')
for n in ('WaveBody','WaveDiamond','WavePendant','Settings'):
    shutil.copy2(HERE.parent/'Stage02/PNG'/f'{n}.png',HERE/'Inputs'/f'Approved{n}.png')
shutil.copy2(HERE.parent/'Stage01/Raw/A.png',HERE/'Inputs/ApprovedA_PixelLab.png')
baseline=HERE/'QA/ProtectedProduction.json'
if not baseline.exists():baseline.write_text(json.dumps({p.relative_to(ROOT).as_posix():sha(p) for folder in ('Assets','ProjectSettings','Packages') for p in (ROOT/folder).rglob('*') if p.is_file()},indent=2)+'\n')
lineage={'user_approval':'Continue after Stage 2; Stage 3 only','primary_reference':refs['Rattlebones'],'supporting_reference_hashes':{n:refs[n]['sha256'] for n in ('Farmer','Pan')},'direct_provider_style_image':{'job_id':'b5ac3e3d-febe-4f11-a90c-ebe56791e271','gallery_id':'73ca07eb-cbc3-54ec-a8a0-4a39b7f7dcc7','source':'../Stage01/Raw/A.png','sha256':sha(HERE.parent/'Stage01/Raw/A.png'),'parent_reference':'Stage01/GenerationRequests.json: Rattlebones actual image input'},'current_docs':{str(p.relative_to(ROOT)):sha(p) for p in (ROOT/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt',ROOT/'Docs/ArtDirection/Golden_Reference_Register.md')},'starting_balance':3685.90,'initial_quote':18,'scope':'Art-source production and review; no Assets/runtime changes or merge'}
(HERE/'QA/ReferenceLineage.json').write_text(json.dumps(lineage,indent=2)+'\n')
print('Verified current reference hashes; preserved baseline and copied approved source inputs.')
