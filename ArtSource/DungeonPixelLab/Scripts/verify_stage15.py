from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/Stage15';V=R/'Previews/Stage15'
def load(p):return Image.open(p).convert('RGBA')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def equal(a,b):return a.size==b.size and a.tobytes()==b.tobytes()
t=ET.parse(D/'UnityTests.xml').getroot();assert t.get('result')=='Passed'
protected=json.loads((D/'ApprovedArtHashes.json').read_text(encoding='utf8'));assert all(sha(P/f)==h for f,h in protected.items())
m=json.loads((R/'Progress/Asset_Manifest.json').read_text(encoding='utf8'));native=[a for a in m['artifacts'] if a['path'].endswith('.png') and '/Exports/' in a['path']]
for a in native:assert sha(P/a['path'])==a['sha256'],a['path']
one=load(V/'Assembly-360x640.png');tall=load(V/'Assembly-360x800.png')
for f,s in [('Assembly-720x1280',2),('Assembly-1080x1920',3)]:assert equal(one.resize((360*s,640*s),Image.Resampling.NEAREST),load(V/(f+'.png'))),f
assert equal(tall.resize((1080,2400),Image.Resampling.NEAREST),load(V/'Assembly-1080x2400.png'))
before=load(R/'Previews/Stage14/Assembly-360x640.png');changed=[(x,y) for y in range(640) for x in range(360) if one.getpixel((x,y))!=before.getpixel((x,y))]
assert all(324<=x<352 and 210<=y<230 for x,y in changed),changed[:20]
assert equal(one.crop((0,0,360,210)),tall.crop((0,0,360,210)))
assert equal(one.crop((40,238,320,518)),tall.crop((40,318,320,598)))
from check_stage15_ui import check_ui
ui_translation_checks=check_ui()
e=json.loads((D/'AssemblyEvidence.json').read_text());sources=e['sources'];checks=[]
selected=['Clutter_Skull','Clutter_Bones','Clutter_Barrel','Torch_Flame','Hanging_Banner','Board_Cell','Portrait_Corner_TL','Plaque_Gear','Resonance_Corner_TL','Fascia_Mid_A','Potion','Bomb','CrownedSkull']
contact=Image.new('RGBA',(840,((len(selected)+2)//3)*192),(18,20,29,255));draw=ImageDraw.Draw(contact)
for i,name in enumerate(selected):
 uses=[u for u in sources if Path(u['path']).stem==name]
 assert uses,name
 u=next((v for v in uses if name!='Fascia_Mid_A' or round(v['rect']['x'])==234),uses[0]);r=u['rect'];x,y,w,h=[round(r[k]) for k in ['x','y','width','height']]
 src=load(P/u['path']);assert src.size==(w,h),(name,src.size,(w,h));crop=one.crop((x,y,x+w,y+h));opaque=[(xx,yy) for yy in range(h) for xx in range(w) if src.getpixel((xx,yy))[3]==255]
 errors=[(xx,yy) for xx,yy in opaque if src.getpixel((xx,yy))!=crop.getpixel((xx,yy))]
 assert not errors,(name,errors[:8])
 checks.append(dict(name=name,path=u['path'],scene_rect=[x,y,w,h],opaque_pixels=len(opaque),all_opaque_pixels_exact=True))
 cx=(i%3)*280;cy=(i//3)*192;draw.text((cx+8,cy+5),name,fill='white');draw.text((cx+8,cy+23),'Source 2x',fill='#c4bddb');draw.text((cx+148,cy+23),'Unity crop 2x',fill='#c4bddb')
 # Diagnostic presentation only. No production asset is resampled or rewritten.
 contact.alpha_composite(src.resize((w*2,h*2),Image.Resampling.NEAREST),(cx+8,cy+40));contact.alpha_composite(crop.resize((w*2,h*2),Image.Resampling.NEAREST),(cx+148,cy+40))
contact.save(V/'Source-vs-Unity.png')
safe=load(V/'SafeArea-1080x2400.png').crop((0,90,1080,2310));odd=load(V/'SafeArea-1084x2404.png').crop((2,93,1082,2313));assert equal(safe,odd)
native740=safe.resize((360,740),Image.Resampling.NEAREST);assert equal(native740.resize(safe.size,Image.Resampling.NEAREST),safe)
native740.save(V/'SafeArea-native-content.png')
# Wall/material crops expose repetition, seams and any geometry mismatch for review.
for n,box in [('Top',(0,0,360,225)),('Lower-masonry',(0,600,360,800))]:
 im=(one if n=='Top' else tall).crop(box);im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST).save(V/(n+'-3x.png'))
report=dict(result='PASS',protected_art_files=len(protected),unchanged_current_native_exports=len(native),generation_requests=0,tests={k:t.get(k) for k in ['result','total','passed','failed']},exact_native_2x_3x=True,exact_tall_3x=True,odd_safe_area_content_exact=True,standard_changed_pixels=len(changed),standard_changed_bounds=[min(x for x,y in changed),min(y for x,y in changed),max(x for x,y in changed)+1,max(y for x,y in changed)+1] if changed else None,change_reason='Only the user-highlighted right corbel instance is removed; source sprite and every other instance remain unchanged.',native_source_comparisons=checks,ui_translation_checks=ui_translation_checks,captures=e['captures'])
(D/'PixelValidation.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print(json.dumps({k:v for k,v in report.items() if k not in ['native_source_comparisons','captures']}))
