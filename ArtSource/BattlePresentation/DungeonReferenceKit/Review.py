"""Read-only native verification and review documents; no production art edits."""
import csv,hashlib,html,json,struct,subprocess,zlib
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont

KIT=Path(__file__).resolve().parent
ROOT=KIT.parents[2]
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def ase_pixels(path):
    data=path.read_bytes(); magic,frames,w,h,depth=struct.unpack_from('<5H',data,4)
    assert (magic,frames,depth)==(0xA5E0,1,32),path
    canvas=bytearray(w*h*4); offset=144; cels=0
    while offset<len(data):
        length,kind=struct.unpack_from('<IH',data,offset)
        assert length>=6 and offset+length<=len(data)
        if kind==0x2004: assert struct.unpack_from('<H',data,offset+6)[0]&12==0,path
        if kind==0x2005:
            p=offset+6; layer,x,y,opacity,ctype=struct.unpack_from('<HhhBH',data,p)
            assert (layer,opacity,ctype)==(0,255,2),path
            cw,ch=struct.unpack_from('<HH',data,p+16)
            pixels=zlib.decompress(data[p+20:offset+length]);assert len(pixels)==cw*ch*4
            for row in range(ch):canvas[((row+y)*w+x)*4:((row+y)*w+x+cw)*4]=pixels[row*cw*4:(row+1)*cw*4]
            cels+=1
        offset+=length
    assert cels==1,path
    return (w,h),bytes(canvas)

provenance=json.loads((KIT/'provenance.json').read_text())
layout=json.loads((KIT/'layout.json').read_text())
receipt=json.loads((KIT/'build-receipt.json').read_text(encoding='utf-8-sig'))
assert receipt['status']=='Completed' and receipt['sourceSha256']==sha(KIT/'Build.js')
records=[]; by_name={}
for path in sorted((KIT/'native').glob('*.png')):
    im=Image.open(path).convert('RGBA'); size,pixels=ase_pixels(path.with_suffix('.aseprite'))
    assert im.size==size and im.tobytes()==pixels,path
    alpha=sorted(set(im.getchannel('A').tobytes()));assert set(alpha)<={0,255},path
    by_name[path.stem]=im
    records.append(dict(name=path.stem,size=im.size,alpha=alpha,bounds=im.getbbox(),
        colors=len({p for p in im.get_flattened_data() if p[3]}),sha256=sha(path),aseSha256=sha(path.with_suffix('.aseprite')),asePixelsMatch=True))
assert len(records)==len(layout['modules'])+1
assert receipt['aseCount']==len(records)
composite=Image.new('RGBA',layout['size'])
for module in layout['modules']:
    im=by_name[module['name']]; assert im.size==(module['width'],module['height'])
    if module['composite']:composite.alpha_composite(im,(module['x'],module['y']))
assert composite.tobytes()==by_name['DungeonArtOnly'].tobytes(),'Module placement differs from LibreSprite master'
for item in provenance['verifiedAuthority']:assert sha(ROOT/item['path'])==item['sha256'],item['path']
for name,expected in provenance['protectedLocalFiles'].items():assert sha(ROOT/name)==expected,name
assert sha(KIT/'reference/ApprovedDungeon.png')==provenance['reference']['sha256']
diff=subprocess.check_output(['rtk','proxy','git','diff','--name-only',provenance['startCommit'],'--','Assets','ProjectSettings','Packages'],cwd=ROOT,text=True).splitlines()
assert set(diff)<=set(provenance['protectedLocalFiles']),diff
assert not subprocess.check_output(['rtk','proxy','git','ls-files','--others','--exclude-standard','--','Assets','ProjectSettings','Packages'],cwd=ROOT,text=True).strip()
source=Image.open(KIT/'reference/ApprovedDungeon.png').convert('RGBA')
native=Image.new('RGBA',(470,836));native.putdata([source.getpixel((2*x+1,2*y+1)) for y in range(836) for x in range(470)])
native.save(KIT/'review/SampledTarget.png')

delta=Image.new('RGBA',native.size)
dp=[];equal=0
for a,b in zip(native.get_flattened_data(),composite.get_flattened_data()):
    same=a==b;equal+=same;dp.append((0,0,0,0) if same else (255,159,57,180))
delta.putdata(dp);delta.save(KIT/'review/ChangedPixels.png')
overlay=Image.blend(native,composite,0.5);overlay.save(KIT/'review/Overlay50.png')

# Exact landmarks, not a misleading whole-screen similarity score.
landmarks=[('Door bars',[211,99,243,115]),('Left arch stones',[193,78,201,97]),
 ('Left banner emblem',[160,78,177,108]),('Right banner emblem',[296,78,311,107]),
 ('Warm right torch',[348,86,376,145]),('Near floor joints',[176,276,333,287]),
 ('Board top beam',[100,297,395,309]),('Board bottom beam',[100,684,395,697]),
 ('Left foreground pillar',[0,747,67,836]),('Right foreground pillar',[409,748,470,836])]
checks=[]
for label,box in landmarks:
    ok=native.crop(box).tobytes()==composite.crop(box).tobytes()
    checks.append(dict(name=label,box=box,exactPixelsPreserved=ok))
    assert ok,label

fontpath='C:/Windows/Fonts/segoeui.ttf'
def font(n):return ImageFont.truetype(fontpath,n)
def title(draw,pos,text,size=22,fill='#ece7f2'):draw.text(pos,text,font=font(size),fill=fill)

pair=Image.new('RGB',(990,915),'#16131d');d=ImageDraw.Draw(pair)
title(d,(18,12),'APPROVED TARGET',23);title(d,(508,12),'REBUILT ASSET KIT',23)
title(d,(18,43),'Shared sampling grid · comparison only',14,'#baafc6')
title(d,(508,43),'Art-only assembly · not a Unity capture',14,'#baafc6')
pair.paste(native,(15,70),native);pair.paste(composite,(505,70),composite);pair.save(KIT/'review/Comparison.png')

parts=[
 (1,'Wave header',(169,21,304,68),'Purple plaque, skull and separate live-label well.'),
 (2,'Guide',(330,30,409,69),'Shared silver control; text remains live.'),
 (3,'Settings',(414,30,459,70),'Shared silver gear control.'),
 (4,'Player panel',(11,28,140,280),'Thin frame, irregular masonry and skull banner.'),
 (5,'Far background',(140,0,470,180),'Torch-lit wall, chains, pillars and hanging banners.'),
 (6,'Mid background',(180,58,295,180),'Separate central arch and barred doorway.'),
 (7,'Floor / foreground',(137,177,470,298),'Original shallow floor and perspective. Hidden patches repaired.'),
 (8,'Board frame',(37,295,437,701),'Four unique corners, four beams and empty cell well.'),
 (9,'Bottom HUD',(69,707,403,797),'Three control rims plus energy rim; no outer enclosure.'),
 (10,'Side props',(0,298,37,701),'Distinct side strips; torch, banner, bone and barrel cutouts.')]
breakdown=Image.new('RGB',(1040,920),'#16131d');d=ImageDraw.Draw(breakdown)
title(d,(20,14),'DUNGEON · REFERENCE BREAKDOWN',26)
breakdown.paste(native,(20,65));d=ImageDraw.Draw(breakdown)
colors=['#ffdb72','#80d9ff','#7cf1bd','#ffb7ce','#c1b0ff','#80d9ff','#ffdb72','#7cf1bd','#ffb7ce','#c1b0ff']
for (num,name,b,desc),c in zip(parts,colors):
    x0,y0,x1,y1=b;d.rectangle((20+x0,65+y0,20+x1,65+y1),outline=c,width=1)
    cx,cy=20+x0+9,65+y0+10;d.ellipse((cx-10,cy-10,cx+10,cy+10),fill='#1e1726',outline=c)
    title(d,(cx-6,cy-10),str(num),14,c)
    yy=70+(num-1)*78;title(d,(515,yy),f'{num:02}  {name}',21,c)
    words=desc.split();lines=['']
    for word in words:
        if len(lines[-1]+word)>52:lines.append('')
        lines[-1]+=word+' '
    for i,line in enumerate(lines):title(d,(515,yy+28+i*18),line,15,'#c6bece')
title(d,(515,865),'470 × 836 production grid. One scale for every part.',16)
breakdown.save(KIT/'review/Breakdown.png')

with (KIT/'AssetInventory.csv').open('w',newline='',encoding='utf-8') as f:
    writer=csv.DictWriter(f,fieldnames=list(layout['modules'][0]));writer.writeheader();writer.writerows(layout['modules'])
report=dict(status='Candidate; awaiting Dungeon visual approval; no integration',assets=records,
    exactModuleComposition=True,landmarks=checks,binaryAlpha=True,providerCalls=0,buildSourceMatchesReceipt=True,
    protectedFilesVerified=list(provenance['protectedLocalFiles']),unityValidation='Not run: source-only assets outside Assets; no runtime/serialization changes',
    sourceGrid='470x836; original 941x1672 sampled at (2x+1,2y+1); no claim of original native dimensions',
    changedPixelMap='review/ChangedPixels.png',
    exclusions='Live characters, gems, counters, labels, HP bars and item/ability icons are deliberately absent from production art.')
(KIT/'checks.json').write_text(json.dumps(report,indent=2)+'\n')

notes=[
 'Composition: door, banners, torches, board beams and lower foreground stay at their sampled source coordinates. No enlarged arena or extra floor rows.',
 'Concealed surfaces: the source does not contain the wall/floor pixels behind actors, text or controls. Those local patches are authored repairs and need visual approval; the orange difference map exposes them. Repaired stone faces are cleaner and less irregular than the visible reference floor; the player-panel repairs also use quieter shading.',
 'Grid: the supplied mockup is 941×1672. The kit uses one 470×836 grid with point sampling; the far-right source column is omitted. This is a production choice, not a measurement of the mockup’s native art resolution.',
 'Board fit: the reference well is 364×369 on this grid. The study preserves that geometry. A later integration must fit the existing square gameplay grid without changing its dimensions; this small aspect difference is still unresolved.',
 'Runtime: this is an art-only assembly. Actor/HP/gem/text/icon placement, portrait resizing, UI hitboxes and animation are not validated here. No game assets have been replaced.',
 'Layering: assets are registered for this fixed composition. Back-wall and side-prop modules preserve their original backings; the kit does not promise unrestricted parallax behind every extracted ornament.',
 'Global UI: Guide, settings, supply and ability rims remain the same silver family. The bottom HUD has no additional enclosing frame.'
]
(KIT/'MismatchReport.txt').write_text('DUNGEON REFERENCE KIT — REVIEW CANDIDATE\n\n'+'\n\n'.join(notes)+'\n',encoding='utf-8')
sheet=Image.new('RGBA',(900,720),'#231d2c');sd=ImageDraw.Draw(sheet)
title(sd,(18,12),'NATIVE KIT DETAILS · EXACT 2× ENLARGEMENT',24)
details=[('PlayerFrame',20,65),('WaveHeader',290,65),('SharedGuide',580,65),('SharedSettings',760,65),
 ('BoardTL',290,230),('BoardTR',410,230),('BoardBL',530,230),('BoardBR',650,230),
 ('LeftTorch',290,340),('RightTorch',410,340),('LeftSkullBanner',530,340),('RightSkullBanner',690,340),
 ('LeftBones',290,570),('RightBarrel',410,570)]
for name,xx,yy in details:
    im=by_name[name];w,h=im.width*2,im.height*2
    title(sd,(xx,yy-23),name,14)
    sheet.alpha_composite(im.resize((w,h),Image.Resampling.NEAREST),(xx,yy))
sheet.convert('RGB').save(KIT/'review/Details.png')
cards=''.join(f'<article><h3>{html.escape(m["name"])}</h3><div class="asset"><img src="native/{m["name"]}.png" width="{m["width"]}" height="{m["height"]}"></div><p>{m["width"]} × {m["height"]} · anchor ({m["x"]}, {m["y"]})<br>{html.escape(m["role"])}</p><a href="native/{m["name"]}.aseprite">LibreSprite source</a> · <a href="native/{m["name"]}.png">PNG</a></article>' for m in layout['modules'])
doc='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Dungeon — locked-reference rebuild</title>
<style>*{box-sizing:border-box}body{margin:0;background:#131019;color:#e9e3f0;font:16px/1.55 system-ui}main{max-width:1500px;margin:auto;padding:32px}h1{line-height:1.15;font-size:36px}h2{margin-top:48px}p{max-width:1000px;color:#c4bacf}a{color:#bcabff}img{image-rendering:pixelated;max-width:none}.tag{color:#f1cc81}.pair{display:flex;gap:24px;overflow:auto}.pair figure{margin:0;min-width:470px}.pair img{display:block;width:470px;height:836px}.pair figcaption{margin:12px 0}.small{font-size:14px}.stage{width:470px;height:836px;position:relative;background:#25202d;flex:none}.stage img{position:absolute;inset:0;width:470px;height:836px}.compare{display:flex;gap:32px;align-items:flex-start;overflow:auto}input{width:300px}label{display:block;margin:16px 0}.wide{max-width:100%;height:auto}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(320px,1fr));gap:18px}article{background:#211c2a;border:1px solid #42354e;border-radius:8px;padding:18px;overflow:auto}.asset{background:repeating-conic-gradient(#302a39 0% 25%,#241f2d 0% 50%) 50%/16px 16px;overflow:auto;max-height:440px}.asset img{display:block}article p{font-size:14px}li{margin:12px 0;max-width:1100px}button{padding:8px 14px;background:#3c2c58;color:white;border:1px solid #8e71b0;border-radius:5px;cursor:pointer}</style>
<main><div class="tag">DUNGEON ONLY · REVIEW CANDIDATE · ZERO GENERATION CALLS</div><h1>Rebuild the approved composition</h1><p>These are extracted and repaired native LibreSprite modules. The assembled preview is for art review; it is not an in-game screenshot. Forest, Drowned Court and the current Unity assets have not been changed.</p>
<p><a href="review/Breakdown.png">Breakdown sheet</a> · <a href="AssetInventory.csv">Asset inventory</a> · <a href="MismatchReport.txt">Mismatch report</a> · <a href="checks.json">Technical checks</a> · <a href="layout.json">Native placement manifest</a></p>
<h2>Target and rebuilt kit</h2><p class="small">Both views use the same sampling grid. Labels, actors, gems, health bars and item/ability icons are excluded from background assets. <a href="reference/ApprovedDungeon.png">Open the untouched original, 941×1672</a>.</p>
<div class="pair"><figure><figcaption>Approved target · sampled for comparison</figcaption><img src="review/SampledTarget.png"></figure><figure><figcaption>Rebuilt modules · native 470×836</figcaption><img src="native/DungeonArtOnly.png"></figure><figure><figcaption>Previous Unity result · reduced to this comparison size</figcaption><img src="reference/PreviousUnity.png"></figure></div>
<h2>Inspect the registration</h2><div class="compare"><div class="stage"><img src="review/SampledTarget.png"><img id="result" src="native/DungeonArtOnly.png" style="opacity:.5"><img id="diff" src="review/ChangedPixels.png" style="display:none"></div><div><label>Rebuilt opacity <input id="opacity" type="range" min="0" max="100" value="50"><output id="value">50%</output></label><button id="difference">Show changed pixels</button><p>At 0%, inspect the approved target. At 100%, inspect the actual asset assembly. The orange mask shows every changed pixel, including intentionally removed gameplay content. It is not a similarity score.</p><p>Ten landmark patches are checked for exact pixel preservation after sampling. All native PNGs are compared byte-for-byte with the reopened LibreSprite cels, then reassembled independently using the manifest.</p></div></div>
<h2>Reference breakdown</h2><img class="wide" src="review/Breakdown.png"><h2>Remaining differences and integration limits</h2><ul>__NOTES__</ul><h2>Editable asset kit</h2><p>Images below are shown at native 1× size. Each has a separate editable LibreSprite document and PNG. Transparent regions use a checker background. The placement manifest preserves the original composition.</p><div class="grid">__CARDS__</div><p class="tag">Next gate: Dungeon visual approval before integration or work on the other zones.</p></main>
<script>const slider=document.querySelector('#opacity'),result=document.querySelector('#result'),value=document.querySelector('#value'),diff=document.querySelector('#diff');slider.oninput=()=>{result.style.opacity=slider.value/100;value.value=slider.value+'%'};document.querySelector('#difference').onclick=()=>{diff.style.display=diff.style.display==='none'?'block':'none'};</script></html>'''
doc=doc.replace('__NOTES__',''.join('<li>'+html.escape(n)+'</li>' for n in notes)).replace('__CARDS__',cards)
(KIT/'Review.html').write_text(doc,encoding='utf-8')
print(f'PASS: {len(records)} exact ASE/PNG pairs; manifest assembly; {len(checks)} exact landmark patches; protected files and authority unchanged. Review generated.')
