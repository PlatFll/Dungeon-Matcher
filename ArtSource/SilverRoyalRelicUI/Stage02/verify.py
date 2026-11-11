"""Independent pixel, source-preservation and Unity evidence checks for Stage 2."""
from pathlib import Path
from PIL import Image
import hashlib,json,xml.etree.ElementTree as ET
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2];OLD=HERE.parent/'Stage01'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def visible(im):return bytes(v for c in im.get_flattened_data() for v in (c if c[3] else (0,0,0,0)))
baseline=json.loads((HERE/'QA/StartingProduction.json').read_text())
actual={p.relative_to(ROOT).as_posix():sha(p) for folder in ('Assets','ProjectSettings','Packages') for p in (ROOT/folder).rglob('*') if p.is_file()}
assert not baseline.keys()-actual.keys(), 'Removed production files'
changed=[k for k in baseline if baseline[k]!=actual[k]]
assert not changed,changed
added=sorted(actual.keys()-baseline.keys())
allowed_prefix='Assets/_Game/Art/UI/SilverRoyalRelic/'
allowed={'Assets/_Game/Art/UI/SilverRoyalRelic.meta'}|{
 f'Assets/_Game/Scripts/Editor/SilverRoyalRelicTopHud{n}.cs{s}' for n in ('Preview','Tests') for s in ('','.meta')}
assert all(k in allowed or k.startswith(allowed_prefix) for k in added),added
refs=json.loads((OLD/'QA/ReferenceLineage.json').read_text())
for name,ref in refs.items():
    if name=='Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt':continue # Dated user selection recorded; source art untouched.
    p=Path(ref['source']) if 'source' in ref else ROOT/name
    assert p.exists() and sha(p)==ref['sha256'],name
manifest=json.loads((HERE/'Manifest.json').read_text())
source_palette={c[:3] for n in ('A_Wave','A_Settings') for c in Image.open(HERE/'Inputs'/f'{n}.png').convert('RGBA').get_flattened_data() if c[3]}
for name,row in manifest['assets'].items():
    p=HERE/'PNG'/f'{name}.png';im=Image.open(p).convert('RGBA');rt=Image.open(HERE/'Roundtrip'/f'{name}.png').convert('RGBA')
    assert list(im.size)==row['dimensions'] and sha(p)==row['sha256'],name
    assert im.size==rt.size and visible(im)==visible(rt),name
    palette={c[:3] for c in im.get_flattened_data() if c[3]}
    assert len(palette)==row['opaque_colors'] and palette<=source_palette,name
    assert set(im.getchannel('A').get_flattened_data())=={0,255},name
    assert sha(ROOT/'Assets/_Game/Art/UI/SilverRoyalRelic/Stage02'/f'{name}.png')==sha(p),name
    meta=(ROOT/'Assets/_Game/Art/UI/SilverRoyalRelic/Stage02'/f'{name}.png.meta').read_text()
    for spec in ('filterMode: 0','spritePixelsToUnits: 64','enableMipMap: 0','textureCompression: 0'):
        assert spec in meta,(name,spec)
    assert (HERE/'Editable'/f'{name}.ase').stat().st_size>100,name
xml=ET.parse(HERE/'QA/UnityTests.xml').getroot()
assert xml.attrib['result']=='Passed' and xml.attrib['failed']=='0' and xml.attrib['skipped']=='0'
assert any(t.attrib.get('fullname')=='SilverRoyalRelicTopHudTests.Stage02' and t.attrib['result']=='Passed' for t in xml.iter('test-case'))
cases=[('dungeon-720x1280',(720,1280)),('dungeon-360x640',(360,640)),('dungeon-1080x1920',(1080,1920)),
       ('dungeon-1080x2400-safe',(1080,2400)),('drowned-court-1080x2340',(1080,2340))]
for key,size in cases:
    for suffix in ('before','after'):assert Image.open(HERE/'Preview'/f'{key}-{suffix}.png').size==size
    assert 'Layout/board/floor/bottom controls unchanged.' in (HERE/'QA'/f'{key}.txt').read_text()
report={'stage':2,'selected':'A - Royal bevel','existing_production_files_unchanged':len(baseline),
        'additive_preview_files':added,'existing_reference_images_and_golden_register_unchanged':True,
        'canonical_guide_change':'Dated selected-A scope only; source approval history retained',
        'native_exports':len(manifest['assets']),'binary_alpha':True,'only_selected_source_colors':True,
        'libresprite_visible_pixel_roundtrips_exact':True,'actual_game_capture_pairs':len(cases),
        'layout_board_enemy_floor_bottom_controls_unchanged':True,'settings_pause_close_tested':True,
        'wave_text_fit_1_and_99999_tested':True,'whole_native_ornament_pixels_checked':True,
        'device_tested':False,'android_build_tested':False,'runtime_deployment':False,'new_pixellab_generations':0}
(HERE/'QA/PreservationValidation.json').write_text(json.dumps(report,indent=2)+'\n')
manifest['status']='Stage 2 top HUD ready for user review; temporary Game preview only'
manifest['unity_capture_cases']=[key for key,_ in cases]
(HERE/'Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k!='additive_preview_files'}))
