"""Lossless frame selection/packing. All drawings and palette edits originate in PixelLab."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake';out=root/'production';out.mkdir(exist_ok=True)
manifest=[];checks=[]
def frames(actor,state):return sorted((root/'motion'/actor/state/'palette').glob('*.png'))
def write(actor,state,source,indices,durations,impact=0,special=False,loop=False):
    files=frames(actor,source);assert files,(actor,source)
    ims=[Image.open(files[i]).convert('RGBA') for i in indices];assert len(ims)==len(durations)
    dst=out/actor;dst.mkdir(exist_ok=True);sheet=Image.new('RGBA',(128*len(ims),104))
    for i,im in enumerate(ims):sheet.paste(im,(128*i,0))
    path=dst/(state+'.png');sheet.save(path)
    spec=dict(name=actor,state=state,width=128,height=104,frameCount=len(ims),impactFrame=impact,durationsMs=durations,loop=loop,special=special,attack=state=='AutoAttack')
    manifest.append(spec)
    previews=[]
    for im in ims:
        bg=Image.new('RGBA',im.size,'#303238');bg.alpha_composite(im);previews.append(bg.convert('RGB').resize((384,312),Image.Resampling.NEAREST))
    previews[0].save(dst/(state+'.gif'),save_all=True,append_images=previews[1:],duration=durations,loop=0,disposal=2)
    checks.append(dict(**spec,selectedSources=[str(files[i].relative_to(root)) for i in indices],sha256=hashlib.sha256(path.read_bytes()).hexdigest(),colors=len({p for p in sheet.get_flattened_data() if p[3]}),alpha=sorted(set(sheet.getchannel('A').get_flattened_data()))))
actors=sorted(p.name for p in (root/'motion').iterdir())
contacts={'iceborn_raider':3,'harpoon_hunter':3,'frostfang_wolf':3,'rimecarver':2,'shieldmaiden':3,'rime_skald':3,'hearthkeeper':3,'frostblade_reaver':4,'glacier_troll':4,'iceborn_champion':3,'jarl_haldra':5}
for actor in actors:
    write(actor,'Idle','Idle',[0,1,2,3,4,3,2,1,0],[130]*9,loop=True)
    impact=contacts[actor];durations=[320//impact]*impact+[360//(9-impact)]*(9-impact)
    durations[impact-1]+=320-sum(durations[:impact]);durations[-1]+=680-sum(durations)
    if actor=='frostblade_reaver':durations=[80,80,80,80,120,50,50,60,80]
    write(actor,'AutoAttack','AutoAttack',list(range(9)),durations,impact)
    write(actor,'Hit','Hit',list(range(5)),[40,70,70,70,70])
    write(actor,'Death','Death',list(range(9)),[60,70,80,90,100,100,100,100,100])
    if frames(actor,'Ability'):
        write(actor,'Ability','Ability',list(range(9)),[90,90,90,90,100,100,100,100,120],4,True)
    if actor in ('harpoon_hunter','iceborn_champion','jarl_haldra'):
        pose=2 if actor=='harpoon_hunter' else 8
        selection=list(range(pose+1)) if pose==2 else [0,1,2,3,4,6,8]
        write(actor,'ChannelStart','ChannelStart',selection,[80]*(len(selection)-1)+[120],len(selection)-1,True)
        write(actor,'ChannelHold','ChannelStart',[pose],[130],loop=True)
        write(actor,'Interrupt','ChannelStart',list(reversed(selection)),[60]*len(selection))
        contact=2 # the reviewed heavy/downward contact is frame 2 in all three releases
        write(actor,'Release','Release',list(range(9)),[180,180,60,60,60,60,80,100,100],contact,True)
    elif actor=='glacier_troll':
        write(actor,'ChannelStart','AutoAttack',[0,1,2,3],[100,100,100,120],3,True)
        write(actor,'ChannelHold','AutoAttack',[3],[130],loop=True)
        write(actor,'Interrupt','AutoAttack',[3,2,1,0],[80]*4)
        write(actor,'Release','AutoAttack',[3,4,5,6,7,8],[220,100,100,100,100,100],1,True)
    elif actor=='hearthkeeper':
        write(actor,'ChannelStart','Ability',[0,1,2,3],[100,100,100,120])
        write(actor,'ChannelHold','Ability',[3,4,5,4],[150]*4,loop=True)
        write(actor,'Release','Ability',[5,6,7,8],[100]*4)
        write(actor,'Interrupt','Ability',[3,2,1,0],[80]*4)
(out/'animation-manifest.json').write_text(json.dumps({'clips':manifest},indent=2))
(out/'technical-checks.json').write_text(json.dumps(checks,indent=2));print(f'{len(manifest)} clips exported, exact source cels and approved palettes.')
