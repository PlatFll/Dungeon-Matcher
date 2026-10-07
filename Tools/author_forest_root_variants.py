"""Native pixel additions to the approved B/B roots; no resampling of source assets."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, json

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'ArtSource/Forest/RosterRevision'
OUT.mkdir(parents=True,exist_ok=True)
records=[]
preview=[]
for level in (1,2):
    source=ROOT/f'Assets/_Game/Art/Board/ApprovedBlockers/Root{level}_B.png'
    original=Image.open(source).convert('RGBA')
    assert original.size==(64,64)
    palette=set(original.getdata())
    palette={p for p in palette if p[3]==255}
    def wood(rgb):
        return min(palette,key=lambda p:sum((a-b)**2 for a,b in zip(p[:3],rgb)))
    ink=wood((26,23,24)); dark=wood((66,43,34)); shadow=wood((100,66,40))
    base=wood((148,102,68)); light=wood((189,148,109)); highlight=wood((226,196,159))
    preview.append((f'Approved {level}',original))
    for kind in ('Shield','Heart'):
        im=original.copy();d=ImageDraw.Draw(im)
        if kind=='Shield':
            # One broad timber growth, interwoven with the roots at the foot.
            d.polygon([(18,23),(23,19),(39,19),(45,23),(44,36),(41,43),(32,51),(23,44),(19,36)],fill=ink)
            d.polygon([(20,24),(24,21),(38,21),(42,24),(41,36),(38,42),(32,47),(26,42),(22,35)],fill=shadow)
            d.polygon([(21,24),(25,22),(37,22),(40,25),(38,35),(32,44),(25,37)],fill=base)
            d.polygon([(22,24),(25,22),(31,22),(29,29),(29,40),(25,36)],fill=light)
            d.line([(24,24),(27,23),(30,23)],fill=highlight,width=1)
            d.line([(33,25),(32,32),(34,37),(32,42)],fill=shadow,width=2)
            d.line([(37,24),(36,29),(37,32)],fill=light,width=1)
            d.line([(24,32),(26,35),(27,39)],fill=shadow,width=1)
            # Grain continues out of the pointed base into existing roots.
            d.line([(32,46),(34,49),(40,52),(43,55)],fill=dark,width=3)
            d.line([(32,46),(34,48),(40,51),(43,54)],fill=base,width=1)
            if level==1:
                d.polygon([(39,21),(42,24),(41,31),(37,29),(39,27),(36,26)],fill=dark)
                d.line([(31,29),(29,33),(31,36),(29,41)],fill=dark,width=1)
                d.line([(32,29),(30,33)],fill=highlight,width=1)
        else:
            # Two rounded growth lobes join a rooted stem, with continuous grain.
            outer=[(17,28),(17,22),(21,18),(26,18),(31,23),(35,18),(41,18),(46,23),(46,30),(43,35),(32,47),(21,36)]
            d.polygon(outer,fill=ink)
            d.polygon([(19,23),(22,20),(26,20),(31,26),(35,22),(37,20),(41,20),(44,24),(44,30),(40,36),(32,44),(24,36),(20,30)],fill=shadow)
            d.polygon([(20,23),(23,21),(26,21),(31,28),(36,23),(38,21),(40,21),(42,24),(41,30),(36,35),(32,41),(24,33),(21,28)],fill=base)
            d.polygon([(21,23),(24,21),(26,23),(25,27),(27,32),(31,36),(31,40),(24,33),(21,28)],fill=light)
            d.line([(22,24),(24,23),(26,25)],fill=highlight,width=1)
            d.line([(36,25),(38,23),(40,24)],fill=light,width=1)
            d.line([(24,27),(25,31),(30,35)],fill=shadow,width=1)
            d.line([(39,27),(37,31),(33,35)],fill=shadow,width=1)
            d.line([(32,44),(31,48),(26,52),(22,53)],fill=dark,width=3)
            d.line([(32,44),(31,47),(26,51),(22,52)],fill=base,width=1)
            if level==1:
                d.polygon([(41,20),(44,24),(44,29),(40,28),(41,26),(38,24)],fill=dark)
                d.line([(33,29),(31,32),(33,35),(31,38)],fill=dark,width=1)
                d.line([(34,29),(32,32)],fill=highlight,width=1)
        # A thin original root crosses the growth's lower edge, tying it into
        # the existing knot instead of enclosing it as a separate badge.
        for x in range(19,31):
            y=41+(x-19)//2
            for dy in range(3):
                pixel=original.getpixel((x,y+dy))
                if pixel[3]: im.putpixel((x,y+dy),pixel)
        name=f'{kind}Root_{level}'
        path=OUT/(name+'.png');im.save(path)
        assert set(p[3] for p in im.getdata())<={0,255}
        assert set(p for p in im.getdata() if p[3])<=palette
        records.append(dict(name=name,native=[64,64],source=str(source.relative_to(ROOT)).replace('\\','/'),
            sha256=hashlib.sha256(path.read_bytes()).hexdigest(),opaque_colors=len(set(p for p in im.getdata() if p[3])),
            source_preserved_outside_motif=True))
        preview.append((f'{kind} {level}',im))
canvas=Image.new('RGB',(3*280,2*300),(38,38,43));d=ImageDraw.Draw(canvas)
for i,(label,im) in enumerate(preview):
    x=i%3*280;y=i//3*300
    d.text((x+12,y+6),label,fill=(240,228,202))
    canvas.paste(im.resize((256,256),Image.Resampling.NEAREST),(x+12,y+30),im.resize((256,256),Image.Resampling.NEAREST))
canvas.save(OUT/'RootVariantReview.png')
(OUT/'root-variants.json').write_text(json.dumps(dict(method='Direct native pixel edits; approved source palette only; zero generated images',assets=records),indent=2)+'\n')
print(json.dumps(records,indent=2))
