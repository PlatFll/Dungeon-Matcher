"""Read native source/export/Game View bytes; no generation or production edits."""
from pathlib import Path
import json,hashlib,xml.etree.ElementTree as ET
from PIL import Image,ImageDraw

P=Path(__file__).resolve().parents[3] if Path(__file__).parent.name=='Scripts' else Path('C:/Users/USER/.codex/worktrees/dungeon-top-hud-pixellab/Dungeon Matcher')
R=P/'ArtSource/DungeonPixelLab'
def load(rel):return Image.open(P/rel).convert('RGBA')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def stats(im):
 px=list(im.get_flattened_data());return {'dimensions':list(im.size),'opaque_color_count':len({p[:3] for p in px if p[3]==255}),'alpha':sorted({p[3] for p in px}),'bounds':im.getbbox()}
probe=load('ArtSource/DungeonPixelLab/Diagnostics/PixelScaleProbe.png')
export=load('ArtSource/DungeonPixelLab/Exports/Stage00_PixelScaleProbe.png')
wall=load('Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Tiles_Wall/BackWallReference.png')
out={'libresprite_pixel_exact':probe.tobytes()==export.tobytes(),'probe':stats(probe),'wall':stats(wall),'pixel_tests':[],'baselines':[]}
assert out['libresprite_pixel_exact']
for scale in [1,3]:
 screen=Image.open(R/f'Previews/Stage00/PixelProbe-{360*scale}x{640*scale}.png').convert('RGBA')
 for name,source,x,y in [('probe',probe,32,32),('wall',wall,96,32)]:
  rect=(x*scale,(640-y-source.height)*scale,(x+source.width)*scale,(640-y)*scale)
  actual=screen.crop(rect);expected=source.resize(actual.size,Image.Resampling.NEAREST)
  pairs=[(a,b) for a,b in zip(expected.get_flattened_data(),actual.get_flattened_data()) if a[3]==255]
  bad=sum(a[:3]!=b[:3] for a,b in pairs)
  out['pixel_tests'].append({'name':name,'scale':scale,'screen_rect_top_left':rect,'opaque_pixels':len(pairs),'mismatched_rgb_pixels':bad})
  assert bad==0,(name,scale,bad)
  actual.save(R/f'Previews/Stage00/{name}-actual-{scale}x.png')
 for img,label in [(probe,'probe'),(wall,'wall')]:
  img.save(R/f'Previews/Stage00/{label}-source-1x.png');img.resize((img.width*4,img.height*4),Image.Resampling.NEAREST).save(R/f'Previews/Stage00/{label}-source-4x.png')
for f in sorted((R/'Diagnostics').glob('Baseline-*.json')):
 j=json.loads(f.read_text());simple=[s for s in j['sprites'] if s['rendering']=='Simple']
 frac=[s for s in simple if any(abs(s['pixelsPerSourcePixel'][k]-round(s['pixelsPerSourcePixel'][k]))>.001 for k in ['x','y'])]
 out['baselines'].append({'capture':j['label'],'dimensions':[j['width'],j['height']],'canvas_scale':j['canvasScale'],'sprites_audited':len(j['sprites']),'simple_sprites':len(simple),'noninteger_source_pitch_count':len(frac),'import_filters':sorted({s['filter'] for s in j['sprites']}),'ppu':sorted({s['ppu'] for s in j['sprites']}),'compression_enabled_count':sum(s['compressed'] for s in j['sprites']),'mipmap_count':sum(s['mipmaps'] for s in j['sprites']),'android_override_count':sum(s['androidOverride'] for s in j['sprites']),'cameras':j['cameras']})
# Direct source-versus-live comparison. Fully opaque pixels only; transparent pixels
# composite over other art and aren't valid source-byte comparisons.
j=json.loads((R/'Diagnostics/Baseline-1080x1920.json').read_text());screen=Image.open(R/'Previews/Stage00/Baseline-1080x1920.png').convert('RGBA')
for name in ['ApprovedDungeonBarredDoor','ApprovedDungeonTorchLeft']:
 s=next(s for s in j['sprites'] if s['name']==name);src=load(s['path']);a=s['screen'];x,y,w,h=[round(a[k]) for k in ['x','y','width','height']]
 crop=screen.crop((x,screen.height-y-h,x+w,screen.height-y));expected=src.resize((w,h),Image.Resampling.NEAREST)
 pairs=[(a,b) for a,b in zip(expected.get_flattened_data(),crop.get_flattened_data()) if a[3]==255]
 match=sum(a[:3]==b[:3] for a,b in pairs)
 record={'name':name,'path':s['path'],'sha256':sha(P/s['path']),**stats(src),'screen_rect':s['screen'],'pixel_pitch':s['pixelsPerSourcePixel'],'opaque_comparison_pixels':len(pairs),'nearest_prediction_matching_pixels':match,'nearest_prediction_matching_fraction':match/len(pairs)}
 out.setdefault('live_region_comparisons',[]).append(record)
 src.save(R/f'Previews/Stage00/{name}-source-1x.png');src.resize((src.width*4,src.height*4),Image.Resampling.NEAREST).save(R/f'Previews/Stage00/{name}-source-4x.png');crop.save(R/f'Previews/Stage00/{name}-live-crop.png')
tests=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();out['unity_tests']={k:tests.attrib[k] for k in ['result','total','passed','failed']}
out['historical_360_game_failure']='Diagnostics/Baseline360-Failure.xml: production board (568.26,568.26) cannot fit physical360x640; no 360px gameplay pass claimed.'
(R/'Diagnostics/PixelChecks.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf8')
print(json.dumps(out,indent=2))
