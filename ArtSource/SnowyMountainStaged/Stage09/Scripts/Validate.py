from pathlib import Path
from PIL import Image,ImageChops
import json,hashlib,xml.etree.ElementTree as ET
R=Path(__file__).resolve().parents[4]
S=R/'ArtSource/SnowyMountainStaged/Stage09';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
baseline=json.loads((S/'QA/StartingHashes.json').read_text());changed=[p for p,h in baseline.items() if not (R/p).exists() or sha(R/p)!=h]
assert not changed,changed
assets=json.loads((S/'AssetCatalog.json').read_text())['assets']
for a in assets:
 p=R/a['path'];im=Image.open(p).convert('RGBA');rt=Image.open(S/'QA/NativeRoundtripBatch'/(a['name']+'.png')).convert('RGBA')
 assert sha(p)==a['sha256'] and sha(R/a['native_source'])==a['source_sha256']
 assert im.size==rt.size and im.tobytes()==rt.tobytes(),a['name']
 assert set(im.getchannel('A').getdata())<={0,255}
sky=Image.open(S/'Exports/SkyExtension.png').convert('RGBA');vista=Image.open(R/'ArtSource/SnowyMountainStaged/Stage02/Exports/AlpinePass.png').convert('RGBA')
assert sky.crop((0,95,384,96)).tobytes()==vista.crop((0,0,384,1)).tobytes()
assert set(sky.getdata())<=set(vista.getdata()),'No new color outside approved vista palette'
def region(r,h):return(round(r['x']),h-round(r['y']+r['height']),round(r['x']+r['width']),h-round(r['y']))
def check(a,b,m):
 diff=ImageChops.difference(a,b);w,h=a.size
 rects=[m['boardFrame'],m['geometry']['portrait']]+m['geometry']['health']+[c['rect'] for c in m['geometry']['controls']]
 for r in rects:assert diff.crop(region(r,h)).getbbox() is None
 bottom=m['geometry']['bottom'];assert diff.crop((round(w*.25),h-round(bottom['y']+bottom['height']),round(w*.75),h)).getbbox() is None
 return sum(p!=(0,0,0) for p in diff.getdata())
measure=json.loads((S/'Preview/Measurements.json').read_text())['items'];views=[]
for m in measure:
 n=m['size'];a=Image.open(S/f'Preview/Before/{n}.png').convert('RGB');b=Image.open(S/f'Preview/After/{n}.png').convert('RGB');reduced=Image.open(S/f'Preview/Reduced/{n}.png').convert('RGB')
 assert a.tobytes()==reduced.tobytes();count=check(a,b,m)
 views.append({'size':n,'pixel_scale':m['nativePixelScale'],'protected_regions':'exact','atmosphere_changed_pixels':count,'reduced_motion':'exact static assembly'})
m=next(x for x in measure if x['size']=='720x1280');a=Image.open(S/'Preview/Before/720x1280.png').convert('RGB')
frames=sorted((S/'Preview/Motion').glob('*.png'));assert len(frames)==96
counts=[check(a,Image.open(p).convert('RGB'),m) for p in frames]
live=json.loads((S/'QA/LiveTiming.json').read_text());assert len(live['frames'])==96 and all(live['enemyIdlesAdvanced'])
assert all((S/'Preview/Live'/f['file']).exists() for f in live['frames'])
xml=ET.parse(S/'QA/UnityTestResults.xml').getroot();assert xml.attrib['result']=='Passed' and xml.attrib['failed']=='0'
report={'status':'review validation passed; 360x640 shared HUD limitations remain','protected_files':len(baseline),'changed_protected_files':changed,'native_assets':len(assets),'libresprite_roundtrips':'all exact','new_sky_extension':{'size':[384,96],'source':'Stage02/AlpinePass','bottom_seam':'exact source top row','palette':'subset of original vista','original_artwork':'unchanged'},'views':views,'frozen_atmosphere':{'frames':len(frames),'protected_regions':'every frame exact','changed_pixels_range':[min(counts),max(counts)]},'live_preview':{'frames':96,'seconds':live['endSeconds'],'enemy_idles_advanced':live['enemyIdlesAdvanced'],'board_checks':'logical occupancy, identity, sprite, scale, color and fixed cells; existing hint movement preserved'},'unity':'passed','fixture_sha256':sha(R/'Assets/_Game/Scripts/Editor/SnowyMountainStage09Tests.cs'),'shared_assembler_sha256':sha(R/'Assets/_Game/Scripts/Editor/SnowyMountainStage08Tests.cs'),'limitations':['360x640 retains shared HUD crowding and Unity development/editor pixel-camera warning','Legacy gem-cell art and shared board sampling unchanged','No production binding, Android build or real-device test','Final assembled design awaits user review; nothing merged']}
(S/'QA/Validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
