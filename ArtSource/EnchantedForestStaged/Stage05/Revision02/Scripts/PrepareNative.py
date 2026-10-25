from pathlib import Path
from PIL import Image,ImageDraw
import json,re,uuid,hashlib
p=Path(__file__).resolve().parents[5];r=p/'ArtSource/EnchantedForestStaged/Stage05/Revision02';a=p/'Assets/_Game/Art/EnchantedForestStaged/Stage05/Revision02';t=p/'.utmp/enchanted-stage05-r2'
t.mkdir(parents=True,exist_ok=True);(t/'reopened').mkdir(exist_ok=True)
groups={
'0c1014':'050904 030503 040604 040804 050804 12130c 0f0e0a 000301 020404 010103 030404 0b110e',
'172920':'081507 061105 071406 0d1609 0d1d0d',
'233f2b':'142413 1b2c17 192916 162715 273719 2d3c1b 33411e 243419 33421e 28371a',
'3c5f33':'32411d 3d4d22 3a4821 465526 405024 405123 41542b 44562a 495b2b',
'4c7037':'465826 4e622b 53662d 51642d 566b31',
'65863e':'5e7436 627838 607436 6a803b',
'7a9845':'6e8540 718741 728943 7e8d3c',
'241e1a':'1d1411',
'3b2c20':'271c19 231916 251c18 231a17 352a1f',
'563c26':'372b21 2d221c 382b22 2d241c 3e2f25 2c211c 403527 413527',
'765131':'483928 56472f 4c3e2a 423226 4a3c29 4c3d2c 574630 6c573d 6c563d',
'9a6d41':'685738 645636 635636 7f6246 877550 89744e',
'52733a':'595531 6e703f 566a32 566a36 5e805f',
'e7d9b5':'eae9e5 dadadb bddeef',
'd4bb85':'f2e093',
'a6a56c':'b0aa7c 929f8b',
'9b93bf':'ad9ad6 94b6e2',
'686071':'776187',
'23313a':'19272b 142022',
'314651':'283a4a 253645 304350',
'49616d':'394d60 3b5063',
'5d7982':'4a6373 455c6a 516b7c',
'739096':'668492 648291',
'8a9d9a':'78989d',
'96513a':'6f4734',
'c9a477':'b89177 ba896d'}
palette={h:tuple(bytes.fromhex(dst))+(255,)for dst,src in groups.items()for h in src.split()}
extra={'0c1014':'000302 020304 010100 0d1310 010202 010305 030705 0e1512','49616d':'3a4e60 394e60','739096':'688796 658391','7a9845':'808e3d 7f8d3d','563c26':'403426 3d3225','3c5f33':'43562a 44572a','52733a':'556930 5e785b','5d7982':'425a6a 4f6979 486272','23313a':'202a2f 1a272c','c9a477':'b6967c e0ae8a','314651':'263746 2f4252','9a6d41':'7c6245','765131':'53412d','9b93bf':'a4bbcf','e7d9b5':'fbfafb','96513a':'bc8265'}
for dst,src in extra.items():
 groups[dst]+=' '+src
 for h in src.split():palette[h]=tuple(bytes.fromhex(dst))+(255,)
sources={'Bough_A':'Broad_01','Bough_B':'Broad_04','Bough_C':'Broad_06','Bough_D':'Broad_08','Boulder_A':'Rock_08','Boulder_B':'Rock_12','Boulder_C':'Rock_04','Boulder_D':'Rock_09'}
items={};manifest=[]
for name,src in sources.items():
 raw=Image.open(r/'Raw'/(src+'.png')).convert('RGBA');im=Image.new('RGBA',raw.size)
 for y in range(im.height):
  for x in range(im.width):
   c=raw.getpixel((x,y))
   if c[3]:im.putpixel((x,y),palette[bytes(c[:3]).hex()])
 bounds=raw.getbbox();im=im.crop(bounds);pad=Image.new('RGBA',(im.width+4,im.height+4));pad.alpha_composite(im,(2,2));items[name]=pad
 manifest.append(dict(name=name,source=src,source_sha256=hashlib.sha256((r/'Raw'/(src+'.png')).read_bytes()).hexdigest(),source_rectangle=bounds,padding=2,dimensions=pad.size,opaque_colors=len({c for c in pad.get_flattened_data()if c[3]})))
# Overlapping native modules form continuous heavy branches. Existing UI occludes any underlap.
placements=[]
left=['A','C','B','A','D','C'];right=['B','D','A','C','B','D']
for side,sequence in [('Left',left),('Right',right)]:
 for i,key in enumerate(sequence):
  name='Bough_'+key;x=0 if side=='Left' else 360-items[name].width
  placements.append(dict(name=side+' connected bough '+str(i+1),file=name,x=x,y=240+i*69))
for side,key in [('Left','A'),('Right','B')]:
 for label,y in [('board foot',510),('lower foot',620)]:
  chosen=('C'if side=='Left'else'D')if label=='board foot'else key
  name='Boulder_'+chosen;x=0 if side=='Left'else 360-items[name].width
  placements.append(dict(name=side+' '+label+' boulder',file=name,x=x,y=y))
for q in placements:assert q['x']+items[q['file']].width<=360 and q['y']+items[q['file']].height<=680,(q,items[q['file']].size)
(r/'Scripts/Placements.json').write_text(json.dumps(dict(pieces=placements),indent=2))
plan=[]
for name,im in items.items():
 Image.new('RGBA',im.size).save(t/(name+'-canvas.png'));im.save(t/(name+'-expected.png'));rows=[]
 for y in range(im.height):
  row=[];x=0
  while x<im.width:
   c=im.getpixel((x,y));end=x+1
   while end<im.width and im.getpixel((end,y))==c:end+=1
   if c[3]:row.append([x,end-x,bytes(c).hex()])
   x=end
  rows.append(row)
 plan.append(dict(name=name,w=im.width,h=im.height,rows=rows))
 meta=(p/'Assets/_Game/Art/EnchantedForestStaged/Stage03/Lantern_Body.png.meta').read_text();meta=re.sub(r'^guid: .*$', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/EnchantedForestStaged/Stage05/Revision02/'+name).hex,meta,flags=re.M);meta=re.sub(r'^    spriteID: .*$', '    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/EnchantedForestStaged/Stage05/Revision02/Sprite/'+name).hex,meta,flags=re.M);(a/(name+'.png.meta')).write_text(meta)
data=json.dumps(plan,separators=(',',':'));(r/'Scripts/NativePixelPlan.json').write_text(data)
js='var pc=app.pixelColor;var items='+data+';\n'+'''items.forEach(function(a){var s=app.open('.utmp/enchanted-stage05-r2/'+a.name+'-canvas.png').sprite,im=s.layer(0).cel(0).image;a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2],c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});s.layer(0).name=a.name+' native rich woodland';s.commit();s.saveAs('ArtSource/EnchantedForestStaged/Stage05/Revision02/Native/'+a.name+'.aseprite',true);s.saveAs('Assets/_Game/Art/EnchantedForestStaged/Stage05/Revision02/'+a.name+'.png',true);});console.log('STAGE05R2_NATIVE_COMPLETE');'''
(r/'Scripts/AuthorNative.js').write_text(js);(r/'Scripts/RunNative.js').write_text('try{'+js+"}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Scripts/ReopenNative.js').write_text('try{'+json.dumps(list(items))+".forEach(function(n){app.open('ArtSource/EnchantedForestStaged/Stage05/Revision02/Native/'+n+'.aseprite').sprite.saveAs('.utmp/enchanted-stage05-r2/reopened/'+n+'.png',true);});console.log('STAGE05R2_NATIVE_COMPLETE');}catch(e){console.log('FAILED '+e);}app.command.Exit();")
(r/'Diagnostics/Selection.json').write_text(json.dumps(dict(revision_cost=30,jobs=['9e5ba07b-6dc0-463d-8f83-cb2f66e71ab6','ec00737f-f850-4fc1-9690-7456ed5022ff','dc9c4942-7670-4219-a2b2-9a6643b514c0'],rejected='Tall 64x192 bough study: too fine and narrow, clipped top/bottom. Prior Stage 5 rejected by user.',palette_groups=groups,modules=manifest,geometry='No warping/resampling. Complete source silhouettes with added padding. Modular overlap joins behind approved frame; board/HUD occlude underlap.'),indent=2))
# Review study: explicit UI restoration simulates the final behind-UI rendering order.
base=Image.open(p/'ArtSource/EnchantedForestStaged/Stage04/Previews/Unity-Stage04-720x1360.png').convert('RGBA');preview=base.copy();overlay=Image.new('RGBA',base.size)
for q in placements:
 im=items[q['file']];overlay.alpha_composite(im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST),(q['x']*2,q['y']*2))
preview.alpha_composite(overlay)
for b in [(104,528,616,1040),(110,1160,204,1248),(236,1160,484,1248),(516,1160,610,1248),(262,1100,458,1136),(220,1278,500,1308)]:preview.paste(base.crop(b),b[:2])
uses=json.loads((p/'ArtSource/EnchantedForestStaged/Stage04/Diagnostics/Unity-720x1360.json').read_text())['images']
for u in uses:
 if not u['frame']:continue
 im=Image.open(p/u['path']).convert('RGBA');scale=round(u['scale']);im=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST);rect=u['rect'];rw,rh=round(rect['width']),round(rect['height']);patch=Image.new('RGBA',(rw,rh))
 if u['type']=='Tiled':
  for yy in range(0,rh,im.height):
   for xx in range(0,rw,im.width):patch.alpha_composite(im,(xx,yy))
 else:patch=im
 preview.alpha_composite(patch,(round(rect['x']),1360-round(rect['y']+rect['height'])))
preview.save(r/'Previews/Layout-Study.png');overlay.save(r/'Previews/Dressing-Only.png')
sheet=Image.new('RGBA',(720,735),'#22332a');d=ImageDraw.Draw(sheet)
for i,(name,im)in enumerate(items.items()):
 x=i%3*240;y=i//3*245;d.text((x+6,y+4),name,fill='white');sheet.alpha_composite(im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST),(x+6,y+28));d.text((x+130,y+28),str(im.size),fill='white')
sheet.save(r/'Previews/Modules-2x.png');print('Prepared',len(items),'complete native modules and',len(placements),'placements:',[(n,im.size)for n,im in items.items()])
