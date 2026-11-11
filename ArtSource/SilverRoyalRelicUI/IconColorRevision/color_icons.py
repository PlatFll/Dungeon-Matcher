"""Palette-only revision of Ali's saved icons; originals remain immutable."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib,json,re,shutil,subprocess,uuid
ROOT=Path(__file__).resolve().parents[3]
OUT=Path(__file__).parent
LIVE=ROOT/'Assets/_Game/Resources/UI/BattlePresentation/Silver'
EDITOR=r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
ITEMS=[('Minotaur_Bull','BullAbilityIcon','DungeonAttender'),('Kobold_Revolver','RevolverAbilityIcon','Ricochet'),('Bardley_Flute','FluteAbilityIcon','CrackedGems')]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def rgb(s):return tuple(bytes.fromhex(s))
def native(src,dst):subprocess.run([EDITOR,'--batch',str(src),'--save-as',str(dst)],check=True,timeout=30,creationflags=0x08000000)
for folder in ['Inputs','Editable','Exports','QA','Preview']:(OUT/folder).mkdir(parents=True,exist_ok=True)
baseline=OUT/'QA/BeforeHashes.json'
if not baseline.exists():baseline.write_text(json.dumps({str(p.relative_to(ROOT)).replace('\\','/'):sha(p) for folder in ['Assets','ProjectSettings','Packages'] for p in (ROOT/folder).rglob('*') if p.is_file()},indent=2))
template=(LIVE/'RattleBonesAbilityIcon.png.meta').read_text()
def meta(p):
 m=Path(str(p)+'.meta')
 if not m.exists():m.write_text(re.sub(r'^guid: .*$','guid: '+uuid.uuid4().hex,template,flags=re.M),encoding='utf-8')
 return re.search(r'^guid: (.*)$',m.read_text(),re.M)[1]
manifest={'method':'Exact material recolor; native LibreSprite input/export/reopen','pixellab_generations':0,'references':{},'icons':{}}
for p in ['Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','Docs/ArtDirection/Golden_Reference_Register.md','Docs/ArtDirection/Audit2026-10/media/skeleton-still.png','Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png','Assets/_Game/Art/FrostwakePlayables/Minotaur_Portrait.png','Assets/_Game/Art/CombatIdles/Bardley_Idle.png']:manifest['references'][p]=sha(ROOT/p)
board=Image.new('RGB',(840,440),'#101722');draw=ImageDraw.Draw(board)
for i,(name,asset,definition) in enumerate(ITEMS):
 source=ROOT/'ArtSource/SilverRoyalRelicUI/Production/OtherCharacterIcons'/(name+'.ase');original=OUT/'Inputs'/source.name
 if original.exists():assert sha(source)==sha(original),'Saved user edit changed; review before replacing input'
 else:shutil.copy2(source,original)
 before=original.with_suffix('.png');native(original,before)
 im=Image.open(before).convert('RGBA');result=im.copy()
 shades=[rgb(x) for x in ['262333','3C374A','524D61','6F6B7E','A7A1B6']]
 for y in range(im.height):
  for x in range(im.width):
   color=im.getpixel((x,y))
   if not color[3]:continue
   s=shades.index(color[:3])
   if name=='Minotaur_Bull':
    # Approved Minotaur: warm fur, ivory horns, darker amber eyes.
    if y<=19:target=['0A0D11','79502E','B49872','F0DDAF','D1BE95'][s]
    else:target=['0A0D11','79502E','C99D6F','CDA12E' if y<=22 else 'C99D6F','F0DDAF'][s]
   elif name=='Kobold_Revolver':target=['0A0D11','2B313A','55606E','93A1B0','FDF5E5'][s]
   else:target=['0A0D11','8A5622','C58826','FBEA90','FBEA90'][s]
   result.putpixel((x,y),(*rgb(target),color[3]))
 exported=OUT/'Exports'/(asset+'.png');result.save(exported)
 editable=OUT/'Editable'/(asset+'.ase');native(exported,editable)
 reopened=OUT/'QA'/(asset+'-reopened.png');native(editable,reopened)
 assert Image.open(reopened).convert('RGBA').tobytes()==result.tobytes()
 assert result.getchannel('A').tobytes()==im.getchannel('A').tobytes()
 assert result.size==im.size==(48,40)
 assert set(result.getchannel('A').getdata())=={0,255}
 shutil.copy2(reopened,LIVE/exported.name);base_guid=meta(LIVE/exported.name)
 # Same edited drawing; dark outline and chambers remain unlit.
 glow=Image.new('RGBA',im.size);glow.putdata([(255,255,255,255) if a and (r,g,b)!=rgb('262333') else (0,0,0,0) for r,g,b,a in im.getdata()])
 gp=OUT/'Exports'/(asset+'Ready.png');glow.save(gp);shutil.copy2(gp,LIVE/gp.name);ready_guid=meta(LIVE/gp.name)
 p=ROOT/'Assets/_Game/Data/Player Abilities'/('Ability_'+definition+'.asset');t=p.read_text()
 for field,guid in [('stoneEngraving',base_guid),('readyStoneEngraving',ready_guid)]:
  t,n=re.subn(r'  '+field+r': .*','  '+field+': {fileID: 21300000, guid: '+guid+', type: 3}',t);assert n==1
 p.write_text(t,encoding='utf-8')
 palette=sorted({'#%02X%02X%02X'%c[:3] for c in result.getdata() if c[3]})
 manifest['icons'][name]={'source_ase_sha256':sha(original),'input_png_sha256':sha(before),'production_asset':str((LIVE/exported.name).relative_to(ROOT)),'output_sha256':sha(LIVE/exported.name),'canvas':result.size,'bounds':result.getbbox(),'palette':palette,'opaque_colors':len(palette),'alpha':[0,255],'alpha_mask_and_placement_preserved':True,'libresprite_roundtrip_exact':True}
 ox=i*280;draw.text((ox+12,12),name,fill='white');draw.text((ox+12,35),'Your edit / recolored - native 1x',fill='#aab6c7')
 board.paste(im,(ox+60,55),im);board.paste(result,(ox+155,55),result);draw.text((ox+12,110),'Recolored - nearest-neighbor 5x',fill='#aab6c7')
 big=result.resize((240,200),Image.Resampling.NEAREST);board.paste(big,(ox+20,130),big)
 frame=Image.open(LIVE/'AbilityButtonSilver.png').convert('RGBA');b=result.getbbox();px=round(88-(b[0]+b[2])/2);py=round(36-(b[1]+b[3])/2)
 frame.alpha_composite(result,(px,py));board.paste(frame,(ox+52,350),frame);draw.text((ox+16,422),f'48 x 40 | {len(palette)} colors | exact silhouette',fill='#aab6c7')
board.save(OUT/'Preview/ColoredIcons.png');(OUT/'Manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps(manifest['icons'],indent=2))
