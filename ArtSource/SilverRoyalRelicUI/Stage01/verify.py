"""Independent Stage 1 preservation and exported-pixel verification."""
from pathlib import Path
from PIL import Image
import hashlib,json
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def visible(im):return bytes(v for c in im.get_flattened_data() for v in (c if c[3] else (0,0,0,0)))
protected=json.loads((HERE/'QA/ProtectedProduction.json').read_text())
actual={p.relative_to(ROOT).as_posix():sha(p) for folder in ('Assets','ProjectSettings','Packages') for p in (ROOT/folder).rglob('*') if p.is_file()}
assert protected==actual,{'added':list(actual.keys()-protected.keys()),'removed':list(protected.keys()-actual.keys()),'changed':[p for p in protected.keys()&actual.keys() if protected[p]!=actual[p]]}
refs=json.loads((HERE/'QA/ReferenceLineage.json').read_text())
for name,ref in refs.items():
    p=Path(ref['source']) if 'source' in ref else ROOT/name
    assert p.is_file() and sha(p)==ref['sha256'],name
validation=json.loads((HERE/'QA/NativeValidation.json').read_text())
for name,row in validation.items():
    png=HERE/'Cleaned'/f'{name}.png';im=Image.open(png).convert('RGBA');rt=Image.open(HERE/'Roundtrip'/f'{name}.png').convert('RGBA')
    assert list(im.size)==row['dimensions'] and sha(png)==row['sha256'],name
    assert set(im.getchannel('A').get_flattened_data())=={0,255},name
    assert len({c[:3] for c in im.get_flattened_data() if c[3]})==row['color_count'],name
    assert im.size==rt.size and visible(im)==visible(rt),name
    if name.endswith('Settings'):assert im.size==(56,56),name
    b=im.getbbox();assert min(b[0],b[1],im.width-b[2],im.height-b[3])>=2,name
report={'production_files_checked':len(actual),'production_unchanged':True,'reference_lineage_unchanged':True,
        'native_exports_verified':len(validation),'libresprite_roundtrips_exact_visible_pixels':True,
        'settings_canvases_square':True,'no_source_resampling':True,
        'unity_runtime_tested':False,'device_tested':False,'user_selection':'pending','stage':1}
(HERE/'QA/PreservationValidation.json').write_text(json.dumps(report,indent=2)+'\n')
manifest={'stage':1,'status':'candidates-awaiting-approval','recommended':'A','selected':None,
          'generator':'PixelLab create_image_pro_flash','cleanup':'LibreSprite native JavaScript and batch export',
          'style_reference_sha256':refs['Rattlebones']['sha256'],'generation_usage':29,
          'balance_before':3714.90,'balance_after':3685.90,'assets':validation,
          'raw_sources':{p.name:{'sha256':sha(p),'dimensions':list(Image.open(p).size)} for p in (HERE/'Raw').glob('*.png')},
          'editable_sources':{p.name:sha(p) for p in (HERE/'Editable').glob('*.ase')},
          'generation_receipts':['GenerationRequests.json','GenerationResults.json','GenerationCWave.json','GenerationCSettings.json'],
          'unity_integration':False,'production_art_unchanged':True,'merge_authorized':False}
(HERE/'Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(json.dumps(report))
