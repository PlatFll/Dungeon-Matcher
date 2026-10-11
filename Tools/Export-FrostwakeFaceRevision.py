"""Fetch/verify PixelLab edits, then losslessly pack review candidates only."""
from pathlib import Path
from PIL import Image, ImageDraw
from urllib.request import urlopen
import json, hashlib

ROOT=Path(__file__).resolve().parents[1];BASE=ROOT/'ArtSource/Frostwake';OUT=BASE/'face-revision'
receipts=json.loads((OUT/'selected-receipts.json').read_text());sources=json.loads((OUT/'sources.json').read_text())
specs=json.loads((BASE/'production/technical-checks.json').read_text());records=[]
for actor in sources:
    name=actor['actor']
    if name not in receipts:continue
    receipt=receipts[name];path=OUT/'provider'/(receipt['id']+'.png');path.parent.mkdir(exist_ok=True)
    if not path.exists():
        with urlopen(receipt['url'],timeout=60) as r:path.write_bytes(r.read())
    before=Image.open(OUT/(name+'-source.png')).convert('RGBA');after=Image.open(path).convert('RGBA')
    (OUT/(name+'-candidate.png')).write_bytes(path.read_bytes())
    assert before.size==after.size
    allowed={(p[0],p[1]):p for d in json.loads((OUT/(name+'-decisions.json')).read_text()) for p in d['pixels']}
    count=0
    for y in range(before.height):
        for x in range(before.width):
            a,b=before.getpixel((x,y)),after.getpixel((x,y));assert a[3]==b[3]
            if a!=b:
                p=allowed[x,y];assert a==tuple(bytes.fromhex(p[2][1:]));assert b==tuple(bytes.fromhex(p[3][1:]));count+=1
    assert count==len(allowed)
    by_source={};folder=OUT/'candidates'/name;folder.mkdir(parents=True,exist_ok=True)
    for item in actor['items']:
        x,y=item['offset'];cel=after.crop((x,y,x+128,y+104));by_source[item['path']]=cel
        original=ROOT/item['path']
        if hashlib.sha256(original.read_bytes()).hexdigest()!=item['sha256']:
            assert (OUT/'integration.json').exists() and Image.open(original).convert('RGBA').tobytes()==cel.tobytes(),str(original)
        target=folder/'Still.png' if item['index']==0 else folder/'cels'/f"{item['index']:02}.png"
        target.parent.mkdir(exist_ok=True);cel.save(target)
    clip_records=[]
    for spec in [s for s in specs if s['name']==name]:
        sheet=Image.new('RGBA',(128*spec['frameCount'],104))
        for i,s in enumerate(spec['selectedSources']):sheet.paste(by_source[(BASE/s).relative_to(ROOT).as_posix()],(i*128,0))
        sheet.save(folder/(spec['state']+'.png'))
        clip_records.append(dict(state=spec['state'],sha256=hashlib.sha256((folder/(spec['state']+'.png')).read_bytes()).hexdigest(),frameCount=spec['frameCount'],durationsMs=spec['durationsMs']))
    pair=Image.new('RGB',(768,500),'#303238');d=ImageDraw.Draw(pair)
    d.text((12,8),name.replace('_',' ').title(),fill='white')
    for col,im in enumerate([before.crop((0,0,128,104)),by_source[actor['items'][0]['path']]]):
        pair.paste(im,(col*384+128,48),im);zoom=im.resize((384,312),Image.Resampling.NEAREST);pair.paste(zoom,(col*384,180),zoom)
        d.text((col*384+12,30),'BEFORE - original' if col==0 else 'AFTER - approved',fill='#c4d7df')
    pair.save(OUT/(name+'-comparison.png'))
    locs=json.loads((OUT/'face-locations.json').read_text())[name]
    atlas=Image.new('RGB',(220*9,250*((len(actor['items'])+8)//9)),'#303238');d=ImageDraw.Draw(atlas)
    widths={'frostblade_reaver':20,'hearthkeeper':21,'iceborn_raider':20,'jarl_haldra':23,'rime_skald':27,'frostfang_wolf':16,'iceborn_champion':20}
    heights={'frostblade_reaver':12,'hearthkeeper':17,'iceborn_raider':17,'jarl_haldra':17,'rime_skald':18,'frostfang_wolf':15,'iceborn_champion':12}
    for item,loc in zip(actor['items'],locs):
        im=by_source[item['path']];x,y=loc['position'];crop=im.crop((max(0,x-5),max(0,y-7),min(128,x+widths[name]+5),min(104,y+heights[name]+8)))
        zoom=crop.resize((crop.width*5,crop.height*5),Image.Resampling.NEAREST);ax,ay=item['index']%9*220,item['index']//9*250;atlas.paste(zoom,(ax+15,ay+35),zoom);d.text((ax+5,ay+4),str(item['index']),fill='white')
    atlas.save(OUT/(name+'-after-faces.png'))
    records.append(dict(actor=name,provider='PixelLab pixelart_workbench',jobId=receipt['id'],changedPixels=count,alphaUnchanged=True,outsideSelectedRegionsUnchanged=True,baselineSnapshotPreserved=True,clips=clip_records))
(OUT/'verification.json').write_text(json.dumps({'status':'approved by user for game integration, 2026-10-11','nativeCell':[128,104],'paidGenerations':0,'actors':records},indent=2))
print(json.dumps({r['actor']:dict(changedPixels=r['changedPixels'],clips=len(r['clips'])) for r in records}))
