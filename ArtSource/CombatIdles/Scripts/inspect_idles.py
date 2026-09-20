"""Read-only ASE inspection and nearest-neighbor review renders. Never edits source art."""
from pathlib import Path
import struct, zlib, json, hashlib
from collections import Counter
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
def read_ase(path):
    b=Path(path).read_bytes()
    count,w,h,depth=struct.unpack_from('<4H',b,6)
    assert depth==32, (path,depth)
    pos=128; frames=[]; durations=[]; cels={}
    for f in range(count):
        size,magic,n,dur=struct.unpack_from('<I3H',b,pos)
        assert magic==0xF1FA
        cur=pos+16; out=Image.new('RGBA',(w,h))
        for k in range(n):
            cs,ct=struct.unpack_from('<IH',b,cur); p=cur+6
            if ct==0x2004:
                pass
            if ct==0x2005:
                layer,x,y,opacity,typ=struct.unpack_from('<HhhBH',b,p)
                if typ in (0,2):
                    cw,ch=struct.unpack_from('<HH',b,p+16)
                    raw=b[p+20:cur+cs]
                    if typ==2:raw=zlib.decompress(raw)
                    im=Image.frombytes('RGBA',(cw,ch),raw)
                    cels[(layer,f)]=(im,x,y)
                elif typ==1:
                    link=struct.unpack_from('<H',b,p+16)[0]
                    im,_,_=cels[(layer,link)]
                    cels[(layer,f)]=(im,x,y)
                else: raise ValueError(typ)
                out.alpha_composite(im,(x,y))
            cur+=cs
        frames.append(out);durations.append(dur);pos+=size
    return frames,durations

def color_counts(frames):
    return Counter('#%02X%02X%02X'%p[:3] for im in frames for p in im.getdata() if p[3])

def sheet(frames,durations,label,path):
    scale=4;cols=min(6,len(frames));rows=(len(frames)+cols-1)//cols
    out=Image.new('RGB',(cols*272,rows*290+30),'#292632');d=ImageDraw.Draw(out)
    d.text((8,8),label,fill='white')
    for f,im in enumerate(frames):
        x=(f%cols)*272+8;y=(f//cols)*290+52
        d.text((x,y-16),f'{f+1}: {durations[f]} ms',fill='white')
        out.paste(im.resize((256,256),Image.Resampling.NEAREST),(x,y),im.resize((256,256),Image.Resampling.NEAREST))
    out.save(path)

if __name__=='__main__':
    (ROOT/'Review').mkdir(exist_ok=True)
    info={}
    for p in (ROOT/'Originals').glob('*.aseprite'):
        frames,ds=read_ase(p)
        sheet(frames,ds,p.stem,ROOT/'Review'/f'{p.stem}_original.png')
        info[p.stem]={'canvas':frames[0].size,'durations':ds,'bounds':[im.getbbox() for im in frames],
          'alpha':sorted(set(p[3] for im in frames for p in im.getdata())),
          'colors':dict(color_counts(frames).most_common())}
    (ROOT/'Review'/'source_measurements.json').write_text(json.dumps(info,indent=2))
    for k,v in info.items(): print(k,json.dumps({a:v[a] for a in ['canvas','durations','bounds','alpha']}), '\nCOLORS', len(v['colors']), list(v['colors'].items())[:75])
    for p in (ROOT/'References').glob('*.png'):
        im=Image.open(p).convert('RGBA')
        print(p.name, hashlib.sha256(p.read_bytes()).hexdigest(), im.size, im.getbbox(), color_counts([im]))
