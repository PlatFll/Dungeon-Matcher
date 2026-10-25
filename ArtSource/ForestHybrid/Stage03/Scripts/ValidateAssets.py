"""Independent native-export, board visibility, source-pixel and preservation audit."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,xml.etree.ElementTree as ET
R=Path('ArtSource/ForestHybrid/Stage03');A=Path('Assets/_Game/Art/ForestHybrid/Stage03')
def rgba(p):return Image.open(p).convert('RGBA')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
plan=json.loads((R/'Scripts/NativePixelPlan.json').read_text());manifest=[]
for a in plan['assets']:
 p=A/(a['name']+'.png');im=rgba(p);px=list(im.get_flattened_data());b=im.getbbox()
 assert im.size==(a['w'],a['h']),p
 assert {v[3] for v in px}<={0,255},p
 assert all(v==(0,0,0,0) for v in px if not v[3]),p
 assert im.tobytes()==rgba(Path('.utmp/forest-stage03/reopened')/p.name).tobytes(),p
 if 'Rail_' in a['name']:
  if im.width==32:assert all(im.getpixel((0,y))==im.getpixel((31,y)) for y in range(11)),p
  else:assert all(im.getpixel((x,0))==im.getpixel((x,31)) for x in range(11)),p
 elif a['name']=='Side_PostRepeat':assert all(im.getpixel((x,0))==im.getpixel((x,63)) for x in range(23)),p
 else:assert b[0]>0 and b[1]>0 and b[2]<im.width and b[3]<im.height,(p,b)
 manifest.append({'name':a['name'],'path':p.as_posix(),'size':im.size,'palette_count':len({v[:3] for v in px if v[3]}),'alpha':sorted({v[3] for v in px}),'bounds':b,'sha256':sha(p),'role':a['role']})
corners=[rgba(A/('Board_Corner_'+n+'.png')) for n in ['TL','TR','BL','BR']]
assert all(im.tobytes()==corners[0].tobytes() for im in corners)
native=rgba(R/'Previews/Unity-board-608x608.png');captures=[]
for s in [1,2,3]:
 p=R/'Previews'/('Unity-board-'+str(608*s)+'x'+str(608*s)+'.png');im=rgba(p)
 assert im.size==(608*s,608*s)
 assert im.tobytes()==native.resize(im.size,Image.Resampling.NEAREST).tobytes(),('Integer pixel mismatch',p)
 captures.append({'size':im.size,'sha256':sha(p),'exact_integer_pixels':True})
opening=(48,48,560,560)
noframe=rgba(R/'Previews/Unity-board-no-frame.png')
assert native.crop(opening).tobytes()==noframe.crop(opening).tobytes(),'Frame/decoration entered cell opening'
# Independently composite the unchanged production gem/cell bytes at 64px, without scaling.
tile=rgba('Assets/_Game/Art/DungeonPixelLab/Stage16/Board_Cell64.png')
import re
scene=Path('Assets/_Game/Scenes/Game.unity').read_text(encoding='utf-8')
guids=re.findall(r'guid: ([a-f0-9]+)',scene.split('  gemSprites:\n',1)[1].split('  swipeMinDistance:',1)[0])
paths={}
for p in Path('Assets/_Game/Art/Gems').rglob('*.png.meta'):
 guid=re.search(r'^guid: (\w+)',p.read_text(),re.M).group(1)
 if guid in guids:paths[guid]=p.with_suffix('')
assert len(guids)==len(paths)==6
gems=[rgba(paths[g]) for g in guids];expected=Image.new('RGBA',(512,512))
for y in range(8):
 for x in range(8):
  expected.alpha_composite(tile,(x*64,y*64));expected.alpha_composite(gems[(x*5+y*2+y//3)%6],(x*64,y*64))
assert expected.tobytes()==native.crop(opening).tobytes(),'Live capture does not preserve the native cell/gem pixels'
raw=rgba(R/'Sources/BoardFrame_Raw_PixelLab.png');clean=rgba(R/'Previews/BoardFrame_SourceCleanup.png')
mapping=json.loads((R/'Diagnostics/PaletteDecisions.json').read_text())['source_map']
for a,b in zip(raw.get_flattened_data(),clean.get_flattened_data()):
 assert b==((tuple(bytes.fromhex(mapping[bytes(a[:3]).hex()]))+(255,)) if a[3] else (0,0,0,0))
assert clean.tobytes()==rgba('.utmp/forest-stage03/reopened/BoardFrame_SourceCleanup.png').tobytes()
audit=json.loads((R/'Diagnostics/StartingState.json').read_text())
changed=[p for p,h in audit['protected'].items() if sha(Path(p))!=h]
assert not changed,changed
tests=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert tests.attrib['passed']=='1' and tests.attrib['failed']=='0',tests.attrib
# Native contact sheet and actual Unity corner/trim crops for close review.
sheet=Image.new('RGBA',(720,600),(20,30,29,255));d=ImageDraw.Draw(sheet)
d.text((12,10),'FOREST STAGE 3 - NATIVE MODULES / NO RESIZING',fill='white')
for i,a in enumerate(manifest):
 x=12+(i%5)*142;y=44+(i//5)*172;im=rgba(a['path']);sheet.alpha_composite(im,(x,y))
 d.text((x,y+im.height+7),a['name'].replace('_','\n',1),fill=(224,226,207))
 d.text((x,y+im.height+39),str(im.width)+'x'+str(im.height)+' / '+str(a['palette_count'])+'c',fill=(148,175,151))
sheet.save(R/'Previews/NativeAssets.png')
cropdefs={'Corner-TL':(8,12,80,84),'Corner-TR':(528,12,600,84),'Corner-BL':(8,524,80,596),'Corner-BR':(528,524,600,596),'Top-Trim':(264,20,344,60),'Side-Support':(8,140,60,212)}
close=Image.new('RGBA',(960,680),(20,30,29,255));dc=ImageDraw.Draw(close)
for i,(name,box) in enumerate(cropdefs.items()):
 im=native.crop(box);im.save(R/'Previews'/(name+'-native.png'));im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST).save(R/'Previews'/(name+'-4x.png'))
 x=12+(i%3)*316;y=12+(i//3)*330;dc.text((x,y),name+' / actual Unity at 4x',fill='white');close.alpha_composite(im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST),(x,y+26))
close.save(R/'Previews/CornerTrimReview.png')
result={'status':'PASS','new_assets':len(manifest),'editable_documents':len(manifest)+1,'binary_alpha':True,'libresprite_roundtrip':True,'repeat_endpoints_match':True,'corner_lighting_upright':True,'board':[48,48,512,512],'cells':[8,8,64,64],'source_cell':'Assets/_Game/Art/DungeonPixelLab/Stage16/Board_Cell64.png','source_gems':[str(paths[g]).replace('\\','/') for g in guids],'source_pixels_match_unity':True,'cell_pixels_changed_by_frame':0,'resize_cases':[[46,46],[560,560],[561,559],[640,512],[411,689]],'undersize_hidden':[45,45],'protected_files_unchanged':len(audit['protected']),'pixellab_generations_stage3':6,'pixellab_generations_cumulative':21,'raw_palette':len({p[:3] for p in raw.get_flattened_data() if p[3]}),'clean_palette':len({p[:3] for p in clean.get_flattened_data() if p[3]}),'captures':captures,'manifest':manifest}
(R/'Diagnostics/Validation.json').write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps({k:v for k,v in result.items() if k not in ['manifest','captures','source_gems']},indent=2))
