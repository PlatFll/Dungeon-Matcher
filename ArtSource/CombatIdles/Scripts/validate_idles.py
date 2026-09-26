"""Byte-level validation of native LibreSprite deliverables; no sprite edits."""
from inspect_idles import ROOT,read_ase,color_counts
from PIL import Image
import json,hashlib

palettes=json.loads((ROOT/'Scripts/palettes.json').read_text())
sources={'Rattlebones':'RattleBones_FluidIdle','Farmer':'FarmerFluidAnim2','PanVillager':'PanVillager_Idle_Final','Bardley':'SlimeBard_Idle1'}
def normalized(im):
    return bytes(c for p in im.convert('RGBA').getdata() for c in (p if p[3] else (0,0,0,0)))
report={}
for name,allowed in palettes.items():
    frames,ds=read_ase(ROOT/(name+'_Idle.aseprite'))
    original,_=read_ase(ROOT/'Originals'/(sources[name]+'.aseprite'))
    assert len(frames)==9 and ds==[130]*9,(name,ds)
    assert all(im.size==(64,64) for im in frames)
    used=set(color_counts(frames))
    assert used<=set('#'+h for h in allowed),(name,used)
    assert all(p[3] in (0,255) for im in frames for p in im.getdata())
    unique_count=len(set(normalized(im) for im in frames))
    # Settled key poses may be held. Do not add motion noise to force uniqueness.
    assert unique_count>1,(name,'static animation')
    if name=='Rattlebones':
        assert all(a.getchannel('A').tobytes()==b.getchannel('A').tobytes() for a,b in zip(frames,original))
    if name=='Farmer':
        # The rigid tool can descend beside the boots; compare the foot region itself.
        assert all(im.getchannel('A').crop((20,62,43,64)).tobytes()==original[0].getchannel('A').crop((20,62,43,64)).tobytes() for im in frames)
    if name=='PanVillager':
        # The pan can descend beside the foot; the boot sole stays planted.
        assert len(set(im.crop((19,63,47,64)).tobytes() for im in frames))==1
    if name=='Bardley':
        assert len(set(im.crop((0,63,64,64)).tobytes() for im in frames))==1
        assert all(im.getbbox()[3]==64 for im in frames)
    atlas=Image.open(ROOT/(name+'_Idle.png')).convert('RGBA')
    assert atlas.size==(576,64),(name,atlas.size)
    assert all(normalized(im)==normalized(atlas.crop((i*64,0,(i+1)*64,64))) for i,im in enumerate(frames)),(name,'sheet pixels')
    gif=Image.open(ROOT/(name+'_Idle.gif'))
    assert gif.n_frames==9 and gif.info.get('loop')==0
    gif_durations=[]
    for i,im in enumerate(frames):
        gif.seek(i);gif_durations.append(gif.info['duration'])
        assert normalized(im)==normalized(gif.convert('RGBA')),(name,'GIF frame',i)
    assert gif_durations==ds
    metadata=json.loads((ROOT/(name+'_Idle.json')).read_text())
    assert len(metadata['frames'])==9
    assert all(fr['duration']==130 and fr['sourceSize']=={'w':64,'h':64} for fr in metadata['frames'])
    diffs=[sum(a!=b for a,b in zip(frames[i].getdata(),frames[(i+1)%9].getdata())) for i in range(9)]
    report[name]={'frames':9,'frame_ms':ds,'loop_ms':sum(ds),'canvas':[64,64],'sheet':[576,64],
      'opaque_colors':len(used),'off_palette_pixels':0,'alpha':[0,255],
      'unique_frames':unique_count,'png_matches_aseprite':True,'gif_matches_aseprite':True,'gif_loops_forever':True,
      'bounds':[im.getbbox() for im in frames],
      'adjacent_pixel_differences_including_seam':diffs,
      'source_sha256':hashlib.sha256((ROOT/'Originals'/(sources[name]+'.aseprite')).read_bytes()).hexdigest(),
      'files':{ext:hashlib.sha256((ROOT/(name+'_Idle.'+ext)).read_bytes()).hexdigest() for ext in ['aseprite','png','gif','json']}}
(ROOT/'Validation.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({n:{k:v for k,v in r.items() if k in ['frames','loop_ms','opaque_colors','off_palette_pixels','unique_frames','png_matches_aseprite','gif_matches_aseprite','adjacent_pixel_differences_including_seam']} for n,r in report.items()},indent=2))
