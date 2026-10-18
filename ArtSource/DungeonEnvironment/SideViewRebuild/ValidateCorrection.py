from pathlib import Path
from PIL import Image
from io import BytesIO
from collections import Counter
import json,subprocess,hashlib
K=Path(__file__).resolve().parent;P=K.parents[2]
before=json.loads((K/'Evidence/WallCorrection/BeforeManifest.json').read_text());now=json.loads((K/'AssetManifest.json').read_text())
old_assets={a['name']:a for a in before['assets']};changed={'BannerPlayer','BannerLeft','BannerRight','SideBannerLeft','SideBannerRight','LowerLeftConnector','LowerRightConnector'}
unchanged=[];padded=[]
for a in now['assets']:
 im=Image.open(K/'Exports'/a['group']/(a['name']+'.png')).convert('RGBA');pad=a.get('canvasPadding',0)
 core=im.crop((pad,pad,im.width-pad,im.height-pad)) if pad else im
 old=Image.open(BytesIO(subprocess.run(['git','-C',str(P),'show','1937af806742967b7323301c559d71dc74f6e8fe:ArtSource/DungeonEnvironment/SideViewRebuild/Exports/'+a['group']+'/'+a['name']+'.png'],capture_output=True,check=True).stdout)).convert('RGBA')
 if a['name'] not in changed:
  assert old.size==core.size and old.tobytes()==core.tobytes(),a['name'];unchanged.append(a['name'])
 if pad:
  bounds=im.getbbox();clear=[bounds[0],bounds[1],im.width-bounds[2],im.height-bounds[3]];assert min(clear)>=2
  padded.append({'name':a['name'],'canvas':im.size,'occupied':bounds,'clearance':clear})
 if a['name']=='BannerPlayer':
  assert core.crop((8,7,25,30)).tobytes()==old.crop((8,7,25,30)).tobytes(),'Banner crest must remain unchanged'
  assert core.getpixel((15,41))[3]==255,'The central flag tail must reach its full point'
ui_names={a['name'] for a in before['assets'] if a['group'].startswith(('Frames','Hud','Labels'))}
old_layout=Counter((p['name'],p['x'],p['y']) for p in before['placements'] if p['name'] in ui_names)
new_layout=Counter((p['name'],p['x'],p['y']) for p in now['placements'] if p['name'] in ui_names)
assert old_layout==new_layout,('UI placement mismatch',old_layout-new_layout,new_layout-old_layout)
production_hashes={'Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png':'59b04929e9fdc15ae9f1d70fd3c85c6ea5362bdac479eb12f45cfcbb0efe4306','Assets/_Game/Art/CombatIdles/Farmer_Idle.png':'f1ee772c518c6106617dcf782de95c431e6d90dd6d079a6a5d1d097ce1f10164','Assets/_Game/Art/CombatIdles/PanVillager_Idle.png':'cf2f8bcb3d7066e5837e933e105ed9ff70cd50780bf48d26cb8df9185833e51d'}
for path,h in production_hashes.items():assert hashlib.sha256((P/path).read_bytes()).hexdigest()==h,path
report={'status':'passed','base_commit':'1937af806742967b7323301c559d71dc74f6e8fe','retired_modules':sorted(set(old_assets)-{a['name'] for a in now['assets']}),'unchanged_core_pixels':len(unchanged),'retained_ui_anchors':True,'banner_crest_unchanged':True,'banner_point_complete':True,'complete_padded_assets':padded,'new_asset_count':len(now['assets']),'additional_generation_cost':0,'golden_production_hashes':production_hashes}
(K/'Evidence/WallCorrection/CorrectionChecks.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k not in ('complete_padded_assets','golden_production_hashes')},indent=2))
