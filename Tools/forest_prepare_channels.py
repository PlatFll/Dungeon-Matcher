"""Exact palette repair and held-pose inputs; never resample approved bodies."""
from pathlib import Path
import json, base64
import numpy as np
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'ArtSource/Forest/Production'
def palette_repair(image, reference):
    data=np.array(image.convert('RGBA')); ref=np.array(reference.convert('RGBA'))
    palette=np.unique(ref[ref[:,:,3]>0,:3],axis=0).astype(np.int32)
    visible=data[:,:,3]>=128
    colors=data[visible,:3].astype(np.int32)
    data[visible,:3]=palette[((colors[:,None,:]-palette[None,:,:])**2).sum(2).argmin(1)]
    data[:,:,3]=np.where(visible,255,0);data[~visible,:3]=0
    return Image.fromarray(data)

def root_staff_repair(pose, reference, center):
    """Restore wood and the split tip on the generated upright staff only."""
    a=np.array(pose.convert('RGBA'));x=center
    region=a[3:27,x-1:x+3];visible=region[:,:,3]>0
    # Generated light blade-like pixels belong to the staff, outside the head.
    light=(region[:,:,:3].mean(2)>55)&visible
    region[light,:3]=[135,94,53]
    rows=np.where(a[3:27,x:x+2,3].max(1)>0)[0]
    if not len(rows):return pose
    top=int(rows[0])+3
    out=Image.fromarray(a);d=ImageDraw.Draw(out)
    d.line([(x-3,top),(x-3,top+3),(x,top+6),(x+1,top+7)],fill=(10,13,17,255),width=3)
    d.line([(x-3,top+1),(x-3,top+3),(x,top+6)],fill=(135,94,53,255),width=1)
    return palette_repair(out,reference)
if __name__=='__main__':
    result={}
    for name,index in [('Barkhide_Warden',8),('Orc_Rootbinder',8),('Briar_Matriarch',4)]:
        reference=Image.open(ROOT/f'ArtSource/Forest/Approved/{name}.png')
        pose=palette_repair(Image.open(ART/f'Raw/{name}/ChannelStart/{index:02}.png'),reference)
        if name=='Orc_Rootbinder':
            pose=root_staff_repair(pose,reference,9)
        path=ART/f'Inputs/{name}_Held.png';pose.save(path)
        result[name]='data:image/png;base64,'+base64.b64encode(path.read_bytes()).decode()
    (ART/'Inputs/held-data.json').write_text(json.dumps(result),encoding='utf-8')
