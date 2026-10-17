"""Independent source-pixel checks and native-scale comparison documents."""
import hashlib,json,struct,zlib,csv,shutil
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
BASE=Path(__file__).resolve().parent
ROOT=BASE.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def ase_pixels(path):
    data=path.read_bytes();magic,frames,w,h,depth=struct.unpack_from('<5H',data,4)
    assert (magic,frames,depth)==(0xA5E0,1,32),path
    canvas=bytearray(w*h*4);offset=144;cels=0
    while offset<len(data):
        length,kind=struct.unpack_from('<IH',data,offset);assert length>=6 and offset+length<=len(data)
        if kind==0x2004:assert struct.unpack_from('<H',data,offset+6)[0]&12==0,path
        if kind==0x2005:
            p=offset+6;layer,x,y,opacity,ctype=struct.unpack_from('<HhhBH',data,p)
            assert (layer,opacity,ctype)==(0,255,2),path
            cw,ch=struct.unpack_from('<HH',data,p+16);pixels=zlib.decompress(data[p+20:offset+length])
            assert len(pixels)==cw*ch*4
            for row in range(ch):canvas[((row+y)*w+x)*4:((row+y)*w+x+cw)*4]=pixels[row*cw*4:(row+1)*cw*4]
            cels+=1
        offset+=length
    assert cels==1,path
    return (w,h),bytes(canvas)
font=lambda n:ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',n)
for zone in ['Forest','Court']:
    kit=BASE/(zone+'ReferenceKit');layout=json.loads((kit/'layout.json').read_text())
    provenance=json.loads((kit/'provenance.json').read_text());receipt=json.loads((kit/'build-receipt.json').read_text(encoding='utf-8-sig'))
    assert receipt['sourceSha256']==sha(kit/'Build.js')
    assert sha(kit/'reference/Approved.png')==provenance['sourceSha256']
    for ref in provenance['verifiedAuthority']:assert sha(ROOT/ref['path'])==ref['sha256'],ref['path']
    for p,h in provenance['protectedLocalFiles'].items():assert sha(ROOT/p)==h,p
    records=[];images={}
    for path in sorted((kit/'native').glob('*.png')):
        im=Image.open(path).convert('RGBA');size,raw=ase_pixels(path.with_suffix('.aseprite'))
        assert im.size==size and im.tobytes()==raw,path
        alpha=sorted(set(im.getchannel('A').tobytes()));assert set(alpha)<={0,255},path
        images[path.stem]=im
        records.append(dict(name=path.stem,size=list(im.size),colors=len({p for p in im.get_flattened_data() if p[3]}),alpha=alpha,bounds=im.getbbox(),sha256=sha(path),aseSha256=sha(path.with_suffix('.aseprite')),reopenedPixelsMatch=True))
    assert len(records)==len(layout['modules'])+1==receipt['aseCount']
    assembled=Image.new('RGBA',(470,836))
    for m in layout['modules']:
        im=images[m['name']];assert im.size==(m['width'],m['height'])
        if m['composite']:assembled.alpha_composite(im,(m['x'],m['y']))
    assert assembled.tobytes()==images[zone+'ArtOnly'].tobytes()
    for name in ['SharedGuide','SharedSettings','SharedEnergy','SharedAbility','SharedPotionHolder','SharedBombHolder']:
        assert images[name].tobytes()==Image.open(BASE/'DungeonReferenceKit/native'/(name+'.png')).convert('RGBA').tobytes(),name
    source=Image.open(kit/'reference/Approved.png').convert('RGBA');native=Image.new('RGBA',(470,836))
    native.putdata([source.getpixel((x*2+1,y*2+1)) for y in range(836) for x in range(470)])
    native.save(kit/'review/SampledTarget.png')
    # Test the exposed column crown above y=22; the panel backing below that
    # edge is intentionally reconstructed and covered by a separate frame.
    landmarks=([('Rune',(220,75,248,119)),('Deer',(374,113,408,156)),('Signpost',(410,125,466,200)),('Left lantern',(147,77,176,110)),('Canopy',(140,0,325,20)),('Right mushrooms',(440,580,470,700))] if zone=='Forest' else [('Statue',(253,69,309,112)),('Left crown of column',(45,0,96,22)),('Right statue upper body',(399,135,443,200)),('Trident banner',(424,75,468,174)),('Left coral',(0,325,30,387)),('Right foreground stone',(410,793,470,834))])
    checks=[]
    for name,box in landmarks:
        same=native.crop(box).tobytes()==assembled.crop(box).tobytes();assert same,(zone,name)
        checks.append(dict(name=name,rect=box,exactPixels=True))
    comparison=Image.new('RGB',(990,915),'#16131d');d=ImageDraw.Draw(comparison)
    d.text((18,12),'APPROVED TARGET',font=font(23),fill='#eee8f4');d.text((508,12),'REBUILT ASSET KIT',font=font(23),fill='#eee8f4')
    d.text((18,43),'Shared sampling grid · comparison only',font=font(14),fill='#bcb3c8');d.text((508,43),'Art-only candidate · not a Unity capture',font=font(14),fill='#bcb3c8')
    comparison.paste(native,(15,70));comparison.paste(assembled,(505,70));comparison.save(kit/'review/Comparison.png')
    Image.blend(native,assembled,.5).save(kit/'review/Overlay50.png')
    delta=Image.new('RGBA',native.size);delta.putdata([(0,0,0,0) if a==b else (255,160,50,210) for a,b in zip(native.get_flattened_data(),assembled.get_flattened_data())]);delta.save(kit/'review/ChangedPixels.png')
    parts=[('Wave header',(157,20,314,68)),('Shared Guide',(330,30,409,69)),('Shared settings',(414,30,459,70)),('Player panel',(8,24,143,282)),('Far scenery',(143,0,470,160)),('Mid scenery',(143,160,470,211)),('Path / floor',(143,211,470,298)),('Board frame',(30,288,440,702)),('Shared bottom controls',(65,704,405,796)),('Side props',(0,298,30,701))]
    breakdown=Image.new('RGB',(950,910),'#16131d');breakdown.paste(native,(15,60));d=ImageDraw.Draw(breakdown)
    d.text((15,14),zone.upper()+' · ASSET BREAKDOWN',font=font(24),fill='#eee8f4')
    for i,(name,box) in enumerate(parts,1):
        x,y,X,Y=box;d.rectangle((15+x,60+y,15+X,60+Y),outline='#ffd66f');d.text((18+x,60+y),str(i),font=font(16),fill='#fff')
        d.text((515,70+(i-1)*75),f'{i:02}  {name}',font=font(20),fill='#ffd66f')
        d.text((515,99+(i-1)*75),str(box),font=font(14),fill='#c6bed1')
    breakdown.save(kit/'review/Breakdown.png')
    checks_document=dict(status='Source checks passed; visual approval pending',generationCalls=0,assets=records,landmarks=checks,sharedControlsPixelIdentical=True,assemblyMatchesModules=True)
    (kit/'checks.json').write_text(json.dumps(checks_document,indent=2)+'\n')
    notes=('The surfaces hidden by the elf/orc are reconstructed from source foliage, wood, water and ground clusters. They are not recovered original pixels; the path and foliage bank need visual approval.' if zone=='Forest' else 'The surfaces hidden by the merfolk are reconstructed from source ruin textures and traced paving courses. Paving in those areas is more regular than the visible reference stones.')
    (kit/'MismatchReport.txt').write_text(notes+'\nPlayer-panel masonry hidden by the hero/affinity/HP was rebuilt. Shared controls use the approved Dungeon kit, intentionally identical in all zones. New kit is outside Assets and is not integrated. No whole-screen texture is intended for gameplay.\n')
    gallery=''.join(f'<figure><img src="native/{r["name"]}.png"><figcaption>{r["name"]} · {r["size"][0]}×{r["size"][1]} · {r["colors"]} opaque colors</figcaption></figure>' for r in records if not r['name'].endswith('ArtOnly'))
    (kit/'Review.html').write_text(f'''<!doctype html><meta charset="utf-8"><title>{zone} reference kit</title><style>body{{background:#17131f;color:#e7deef;font:16px system-ui;margin:24px}}img{{image-rendering:pixelated;max-width:100%}}.overlay{{position:relative;width:470px;height:836px}}.overlay img{{position:absolute;inset:0}}.assets{{display:flex;flex-wrap:wrap;gap:16px}}figure{{margin:0;padding:12px;border:1px solid #51445f}}figcaption{{font-size:12px;max-width:300px}}a{{color:#c4aaff}}</style><h1>{zone} · reference kit for approval</h1><p>{notes}</p><p>Zero generations. Native PNGs and reopened LibreSprite files checked pixel-for-pixel. Technical checks establish file properties, not visual approval.</p><img src="review/Comparison.png"><h2>Overlay</h2><label>Rebuild opacity <input type="range" min="0" max="100" value="50" oninput="document.getElementById('rebuilt').style.opacity=this.value/100"></label><div class="overlay"><img src="review/SampledTarget.png"><img id="rebuilt" style="opacity:.5" src="native/{zone}ArtOnly.png"></div><h2>Breakdown</h2><img src="review/Breakdown.png"><h2>Native assets</h2><p>One PNG pixel per CSS pixel at 100% browser zoom. Dimensions and palette counts are verified from individual files.</p><div class="assets">{gallery}</div><p><a href="checks.json">Technical checks</a> · <a href="layout.json">Registration</a> · <a href="MismatchReport.txt">Remaining differences</a></p>''',encoding='utf-8')
    print(zone, len(records),'native documents verified; six original landmarks retained; shared controls match.')
