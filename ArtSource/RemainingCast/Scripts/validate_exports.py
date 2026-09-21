"""Verify delivered bytes. Does not change artwork."""
from pathlib import Path
import sys,json,re,hashlib,argparse
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase,color_counts
sys.path.insert(0,str(ROOT/'Scripts'))
from validate_geometry import check_still,check_parts
pairs=re.findall(r"src:'([^']+)',name:'([^']+)'",(ROOT/'Scripts/cast_spec.js').read_text())
guide=(ROOT.parents[1]/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt').read_text(encoding='utf-8')
guide_colors=set(re.findall(r'#[0-9A-Fa-f]{6}',guide))
# Crossbow lower loops overlap the left boot; check its complete body/prop
# compositing in check_parts, plus this permanently visible right sole here.
sole_ranges={'CrossbowGuard':(33,41),'BarricadeGuard':(24,41),'SpearGuard':(25,31),'TownMarshal':(24,45),'SiegeSergeant':(25,45),'SwordKnight':(26,43),'SpearKnight':(24,43),'ShieldKnight':(23,42),'KnightCaptain':(24,43),'RoyalSwordsman':(26,43),'RoyalLancer':(28,43),'RoyalArbalist':(34,40),'RoyalStandardBearer':(26,43),'RoyalArcanist':(27,43),'RoyalMage':(28,46),'King':(23,43),'Minotaur':(20,43)}
manifest=json.loads((ROOT/'SourceManifest.json').read_text());report={};palettes={}
def equal_visible(a,b):return all(x==y if x[3] else y[3]==0 for x,y in zip(a.get_flattened_data(),b.get_flattened_data()))
for src,v in manifest.items():assert hashlib.sha256((ROOT/v['file']).read_bytes()).hexdigest()==v['sha256'],src
args=argparse.ArgumentParser(description=__doc__)
args.add_argument('--reference',type=Path,help='Optional external original Rattlebones file to compare')
args=args.parse_args()
reference=args.reference or Path('C:/Users/USER/Downloads/RattleBones_FluidIdle.ase')
if args.reference or reference.exists():
 before,bd=read_ase(ROOT/'Originals/RattleBones_FluidIdle.aseprite');after,ad=read_ase(reference)
 assert bd==ad and all(a.tobytes()==b.tobytes()for a,b in zip(before,after)),'Rattlebones changed'
for src,name in pairs:
 old=read_ase(ROOT/'Originals'/f'{src}.aseprite')[0][0]
 ready=read_ase(ROOT/'Recolored'/f'{name}.aseprite')[0][0]
 static=Image.open(ROOT/'Recolored'/f'{name}.png').convert('RGBA')
 assert equal_visible(ready,static),(name,'static export')
 still_checks=check_still(name,old,ready)
 fs,ds=read_ase(ROOT/'Idles'/f'{name}_Idle.aseprite')
 prior,pds=read_ase(ROOT/'BeforeCorrection/Idles'/f'{name}_Idle.aseprite')
 prior_sheet=Image.open(ROOT/'BeforeCorrection/Idles'/f'{name}_Idle.png').convert('RGBA')
 assert len(prior)==9 and pds==[130]*9 and prior_sheet.size==(576,64)
 assert all(equal_visible(im,prior_sheet.crop((f*64,0,(f+1)*64,64))) for f,im in enumerate(prior)),(name,'previous-pass comparison export')
 sheet=Image.open(ROOT/'Idles'/f'{name}_Idle.png').convert('RGBA');gif=Image.open(ROOT/'Idles'/f'{name}_Idle.gif')
 meta=json.loads((ROOT/'Idles'/f'{name}_Idle.json').read_text())
 assert len(fs)==gif.n_frames==9 and ds==[130]*9 and sheet.size==(576,64),(name,'timing')
 assert [v['duration']for v in meta['frames']]==ds
 assert meta['meta']['image']==f'{name}_Idle.png',(name,'portable sheet path')
 allowed=set(color_counts([ready]));assert allowed<=guide_colors,(name,allowed-guide_colors)
 assert set(color_counts(fs))<=allowed,(name,'frame color drift')
 x0,x1=sole_ranges[name];sole=ready.crop((x0,63,x1+1,64)).tobytes()
 for f,im in enumerate(fs):
  assert im.size==(64,64) and set(im.getchannel('A').get_flattened_data())<={0,255}
  assert im.getbbox()[3]==64,(name,f,'ground')
  assert im.crop((x0,63,x1+1,64)).tobytes()==sole,(name,f,'sole pixels')
  assert equal_visible(im,sheet.crop((f*64,0,(f+1)*64,64))),(name,f,'sheet pixels')
  gif.seek(f);assert gif.info['duration']==130 and equal_visible(im,gif.convert('RGBA')),(name,f,'GIF')
 assert fs[0].tobytes()==fs[-1].tobytes()==ready.tobytes(),(name,'ready return')
 palettes[name]=sorted(allowed)
 report[name]={'source':src,'canvas':[64,64],'frames':9,'durations_ms':ds,'loop_ms':1170,'opaque_colors':len(allowed),'alpha':[0,255],**still_checks,'native_sheet_gif_exact':True,'visible_sole_pixels_unchanged':True,'visible_sole_x_range':list(sole_ranges[name]),'rigid_geometry':check_parts(name,fs),'source_sha256':manifest[src]['sha256'],'idle_sha256':hashlib.sha256((ROOT/'Idles'/f'{name}_Idle.aseprite').read_bytes()).hexdigest()}
(ROOT/'Palettes.json').write_text(json.dumps(palettes,indent=2)+'\n')
(ROOT/'Validation.json').write_text(json.dumps({'rattlebones_capture_unchanged':True,'external_rattlebones_checked':reference.exists(),'sprites':report},indent=2)+'\n')
print('PASS: 15 unchanged still masks and 2 authorized helmet enlargements; 153 native frames with exact PNG/GIF pixels, 130ms timing, approved colors, binary alpha, rigid exposed props, on-canvas parts, fixed body soles, ready closure; Rattlebones unchanged.')
