"""Independent native-asset and protected-production checks for Stage 3."""
from pathlib import Path
from PIL import Image
import hashlib,json
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def visible(im):return bytes(v for c in im.get_flattened_data() for v in (c if c[3] else (0,0,0,0)))
before=json.loads((HERE/'QA/ProtectedProduction.json').read_text())
after={p.relative_to(ROOT).as_posix():sha(p) for folder in ('Assets','ProjectSettings','Packages') for p in (ROOT/folder).rglob('*') if p.is_file()}
assert before==after,{'changed':[k for k in before if before[k]!=after.get(k)],'added':list(after.keys()-before.keys())}
manifest=json.loads((HERE/'Manifest.json').read_text());reports={}
expected={'AbilityFrame':(176,66),'AbilityFlourish':(176,66),'RattlebonesEmblem':(30,40),'ItemFrame':(64,64),'EnergyCasing':(147,24),'EnergyFill':(107,8)}
for name,size in expected.items():
 p=HERE/'PNG'/f'{name}.png';im=Image.open(p).convert('RGBA');rt=Image.open(HERE/'Roundtrip'/f'{name}.png').convert('RGBA')
 assert im.size==rt.size==size and visible(im)==visible(rt),name
 assert set(im.getchannel('A').get_flattened_data())=={0,255},name
 assert sha(p)==manifest['assets'][name]['sha256'],name
 b=im.getbbox();assert b and (name=='EnergyFill' or min(b[0],b[1],size[0]-b[2],size[1]-b[3])>=1),name
 assert (HERE/'Editable'/f'{name}.ase').stat().st_size>100,name
 reports[name]={'native_size':size,'binary_alpha':True,'visible_rgba_roundtrip_equal':True,'sha256':sha(p),'editable_sha256':sha(HERE/'Editable'/f'{name}.ase')}
# Every fill pixel sits in the dark native trough, never on silver jewelry.
frame=Image.open(HERE/'PNG/EnergyCasing.png').convert('RGBA');fill=Image.open(HERE/'PNG/EnergyFill.png').convert('RGBA')
allowed={(17,16,27,255),(23,22,34,255),(21,20,35,255)}
for y in range(8):
 for x in range(107):
  if fill.getpixel((x,y))[3]:assert frame.getpixel((20+x,7+y)) in allowed,(x,y,frame.getpixel((20+x,7+y)))
refs=json.loads((HERE.parent/'Stage01/QA/ReferenceLineage.json').read_text())
for n in ('Rattlebones','RattlebonesProduction','Farmer','Pan','Wave','Settings','Supply','Ability','Energy','Emerald','Ruby'):assert sha(Path(refs[n]['source']))==refs[n]['sha256'],n
for n in ('Potion','Bomb'):assert sha(HERE/'Inputs'/f'{n}.png')==sha(ROOT/'Assets/_Game/Resources/UI/Consumables'/f'{n}.png'),n
for n in ('WaveBody','WaveDiamond','WavePendant','Settings'):assert sha(HERE/'Inputs'/f'Approved{n}.png')==sha(HERE.parent/'Stage02/PNG'/f'{n}.png'),n
results=json.loads((HERE/'GenerationResults.json').read_text());assert sum(r['charged_generations'] for r in results)==18
report={'status':'PASS','scope':'Stage 3 native sources and review only','protected_production_files_unchanged':len(before),'production_files_added':0,'approved_stage2_sources_unchanged':True,'golden_and_prior_ui_hashes_unchanged':True,'existing_potion_bomb_icons_unchanged':True,'energy_fill_fits_empty_trough':True,'assets':reports,'pixellab_generations':18,'starting_balance':3685.90,'ending_balance':3667.90,'unity_run_this_stage':False,'reason':'No Assets, runtime, editor, import or serialization changes; Stage 4 integration awaits approval','device_testing':False}
(HERE/'QA/Validation.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k!='assets'}))
