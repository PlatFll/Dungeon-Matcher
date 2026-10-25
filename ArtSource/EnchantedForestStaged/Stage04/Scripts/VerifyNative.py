from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/EnchantedForestStaged/Stage04';a=p/'Assets/_Game/Art/EnchantedForestStaged/Stage04';t=p/'.utmp/enchanted-stage04'
selection=json.loads((r/'Diagnostics/Selection.json').read_text());modules=[];ims={}
for item in selection['modules']:
 name=item['name'];im=Image.open(a/(name+'.png')).convert('RGBA');ex=Image.open(t/(name+'-expected.png')).convert('RGBA');op=Image.open(t/'reopened'/(name+'.png')).convert('RGBA')
 assert im.size==ex.size==op.size and im.tobytes()==ex.tobytes()==op.tobytes(),name
 assert set(im.getchannel('A').get_flattened_data())<={0,255},name
 if 'Ivy' in name or 'Swag' in name:
  x,y,x2,y2=im.getbbox();assert x>0 and y>0 and x2<im.width and y2<im.height,name
 ims[name]=im;modules.append(dict(name=name,dimensions=im.size,opaque_colors=len({c for c in im.get_flattened_data() if c[3]}),bounds=im.getbbox(),binary_alpha=True,libresprite_reopen_exact=True,sha256=hashlib.sha256((a/(name+'.png')).read_bytes()).hexdigest()))
for side in ['Top','Bottom']:
 im=ims['Board_Rail_'+side];assert im.crop((0,0,1,11)).tobytes()==im.crop((31,0,32,11)).tobytes()
for side in ['Left','Right']:
 im=ims['Board_Rail_'+side];assert im.crop((0,0,11,1)).tobytes()==im.crop((0,31,11,32)).tobytes()
for corner,edge,box in [('TL','Top',(22,6,23,17)),('TL','Left',(6,22,17,23)),('TR','Top',(0,6,1,17)),('TR','Right',(6,22,17,23)),('BL','Bottom',(22,6,23,17)),('BL','Left',(6,0,17,1)),('BR','Bottom',(0,6,1,17)),('BR','Right',(6,0,17,1))]:
 rail=ims['Board_Rail_'+edge];port=(0,0,1,11) if edge in ['Top','Bottom'] else (0,0,11,1)
 assert ims['Board_Corner_'+corner].crop(box).tobytes()==rail.crop(port).tobytes(),(corner,edge)
for n,ref in [('Board_Side_Swag','Ivy_LeftSwag')]:
 assert ims[n].tobytes()==Image.open(p/('Assets/_Game/Art/EnchantedForestStaged/Stage02/FoliageRevision/'+ref+'.png')).convert('RGBA').tobytes()
layout=json.loads((r/'Scripts/Placements.json').read_text())['pieces'];decor=Image.new('RGBA',(360,352))
for q in layout:decor.alpha_composite(ims[q['file']],(int(35+q['anchorX']*290+q['x']),24+(0 if q['anchorY']==1 else 290)+q['y']))
assert decor.crop((52,41,308,297)).getbbox() is None
report=dict(modules=modules,repeat_endpoint_profiles_exact=True,all_eight_corner_ports_exact=True,reused_approved_ivy_exact=True,opaque_foliage_pixels_over_board=0,native_opening=[256,256],rendered_opening=[512,512],corner_size=23,rail_thickness=11,frame_inset=17,frame_target=[290,290])
(r/'Diagnostics/NativeValidation.json').write_text(json.dumps(report,indent=2))
# Native artwork with ample cell spacing for the tall ivy pieces.
board=Image.new('RGBA',(420,330),'#182526');d=ImageDraw.Draw(board)
for i,(n,im) in enumerate(ims.items()):
 if i<8:x,y=(i%4)*100+8,(i//4)*72+22
 elif i<12:x,y=(i-8)*100+8,167
 elif i==12:x,y=8,126
 else:x,y=300,292
 # A dedicated tall-swag view is linked separately; avoid covering other native modules.
 if n=='Board_Side_Swag':continue
 board.alpha_composite(im,(x,y));d.text((x,y-13),n.replace('Board_','').replace('Corner_','C '),fill='#e0e3c5')
board.save(r/'Previews/Modules-Native.png');board.resize((840,660),Image.Resampling.NEAREST).save(r/'Previews/Modules-2x.png')
print('PASS: thirteen native/reopened exports, binary alpha, padded foliage, seamless repeat endpoints, exact reused ivy and zero board overlap.')
