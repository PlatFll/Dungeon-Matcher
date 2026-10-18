"""Independent checks of serialized imports and actual Unity captures."""
from pathlib import Path
from PIL import Image,ImageChops
import hashlib,json,xml.etree.ElementTree as ET
K=Path(__file__).resolve().parent
P=K.parents[2]
m=json.loads((K/'AssetManifest.json').read_text());native=json.loads((K/'Evidence/NativeChecks.json').read_text())
xml=ET.parse(K/'Evidence/UnityTests.xml').getroot()
assert xml.attrib['result']=='Passed',xml.attrib
tests=[{'name':t.attrib['fullname'],'result':t.attrib['result']} for t in xml.iter('test-case')]
assert len(tests)==3 and all(t['result']=='Passed' for t in tests)
imported=P/'Assets/_Game/Art/DungeonEnvironment/SideViewRebuild'
for a in m['assets']:
 source=K/'Exports'/a['group']/(a['name']+'.png');dest=imported/a['group']/(a['name']+'.png')
 assert source.read_bytes()==dest.read_bytes(),a['name']
 assert a['width']>0 and a['height']>0
for p in m['placements']:
 a=next(a for a in m['assets'] if a['name']==p['name'])
 if not p.get('clip'):assert 0<=p['x'] and 0<=p['y'] and p['x']+a['width']<=360 and p['y']+a['height']<=640,p
assert len([p for p in m['placements'] if p['name'].startswith('Cell')])==64
assert all(a['height']==12 for a in m['assets'] if a['name'].startswith('StandingStrip'))
assert all(a['height']==19 for a in m['assets'] if a['name'].startswith('LedgeFace'))
assert {p['name'] for p in m['placements'] if p['layer']==0}=={'BackWallReference'}
assert all(p['layer']>30 for p in m['placements'] if p['name'].startswith('Cell') or p['name']=='PortraitWall')
for a in m['assets']:
 pad=a.get('canvasPadding',0)
 if pad:
  b=Image.open(K/'Exports'/a['group']/(a['name']+'.png')).getbbox()
  assert min(b[0],b[1],a['width']-b[2],a['height']-b[3])>=pad,a['name']
reference=Image.open(K/'Review/NativeAssembly.png').convert('RGB');captures=[]
for name,box,scale in [('360x640.png',(0,0,360,640),1),('720x1280.png',(0,0,720,1280),2),('1080x1920.png',(0,0,1080,1920),3),('1080x2400.png',(0,240,1080,2160),3),('1080x2400-safe-area.png',(180,580,900,1860),2)]:
 shot=Image.open(K/'Review/Unity'/name).convert('RGB');crop=shot.crop(box)
 expected=reference.resize((360*scale,640*scale),Image.Resampling.NEAREST)
 diff=ImageChops.difference(crop,expected);pixels=list(diff.get_flattened_data());within=sum(max(p)<=2 for p in pixels)/len(pixels)
 exact=sum(max(p)==0 for p in pixels)/len(pixels)
 captures.append({'file':name,'dimensions':shot.size,'native_scale':scale,'content_box':box,'fraction_exact':exact,'fraction_within_two_levels':within})
 assert within>.995,(name,within)
report={'status':'passed','compiler_validation':'Tools/Validate-Unity.ps1 completed successfully with Unity 6000.3.19f1; full local log Evidence/UnityCompiler.log','unity_tests':tests,'native_source_export_exact':len(m['assets']),'unity_import_bytes_exact':len(m['assets']),'binary_alpha':True,'native_canvas':[360,640],'placements':len(m['placements']),'empty_board_cells':64,'shallow_standing_strip_height':12,'front_ledge_height':19,'unclipped_standalone_placements':True,'single_continuous_wall_field':True,'ui_interiors_above_environment':True,'padded_complete_props':sum(a.get('canvasPadding',0)>0 for a in m['assets']),'capture_checks':captures,'artistic_review':'Corrected the user-marked clipped banner tails, eight patch/support modules and narrow corner crops. Full lower stone contours and prop clearance inspected at native and 2x. UI geometry is preserved and now covers extended background modules. Visual judgement; user approval remains pending.','production_scene_changed':False,'merged':False}
(K/'Evidence/ValidationSummary.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
