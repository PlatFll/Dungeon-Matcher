"""Native-pixel selection, palette repair, modular exports and review manifests."""
from pathlib import Path
import json, hashlib, shutil
import numpy as np
from PIL import Image, ImageDraw
from forest_prepare_channels import palette_repair,root_staff_repair
ROOT=Path(__file__).resolve().parents[1]; ART=ROOT/'ArtSource/Forest/Production'
OUT=ART/'Selected'; OUT.mkdir(exist_ok=True)
NAMES=['Elven_Scout','Orc_Trailguard','Elven_Mender','Orc_Rootbinder','Barkhide_Warden','Briar_Matriarch']
PILOT=ART/'Inputs/MenderPilot'
def padded(im):
    im=im.convert('RGBA')
    if im.width==96:return im.copy()
    dst=Image.new('RGBA',(96,im.height));dst.paste(im,(16,0));return dst
def load(name,state,index):
    return Image.open(ART/f'Raw/{name}/{state}/{index:02}.png').convert('RGBA')
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def frames_export(name,state,frames,durations,source,indices,impact=-1):
    folder=OUT/name/state;folder.mkdir(parents=True,exist_ok=True)
    w,h=frames[0].size;sheet=Image.new('RGBA',(w*len(frames),h));records=[]
    for i,im in enumerate(frames):
        assert im.size==(w,h)
        im.save(folder/f'{i:02}.png');sheet.paste(im,(i*w,0))
        a=np.array(im);visible=a[:,:,3]>0
        records.append(dict(index=i,durationMs=durations[i],bounds=im.getbbox(),
            paletteCount=len(np.unique(a[visible,:3],axis=0)),alphaValues=np.unique(a[:,:,3]).tolist(),
            bottomOccupiedRow=int(np.where(visible)[0].max()) if visible.any() else None,
            hash=sha(folder/f'{i:02}.png')))
    sheet.save(OUT/name/f'{state}.png')
    # Animated native preview uses declared timing and a neutral backdrop.
    previews=[]
    for im in frames:
        bg=Image.new('RGB',im.size,'#777777');bg.paste(im,(0,0),im);previews.append(bg)
    previews[0].save(OUT/name/f'{state}.gif',save_all=True,append_images=previews[1:],duration=durations,loop=0,disposal=2)
    row=dict(name=name,state=state,width=w,height=h,frameCount=len(frames),durationsMs=durations,
             totalMs=sum(durations),loop=state in ['Idle','ChannelHold'],impactFrame=impact,source=source,
             selectedIndices=indices,pivot=[.5,0],ppu=64,resampled=False,frames=records)
    (OUT/name/f'{state}.json').write_text(json.dumps(row,indent=2),encoding='utf-8')
    # Wrap contact sheet to avoid viewer downscaling wide strips.
    cols=6;plate=Image.new('RGB',(cols*w*2,((len(frames)+cols-1)//cols)*(h*2+18)),'#777777');draw=ImageDraw.Draw(plate)
    for i,im in enumerate(frames):
        x=i%cols*w*2;y=i//cols*(h*2+18)
        draw.text((x+2,y+2),f'{i}: {durations[i]} ms',fill='white')
        zoom=im.resize((w*2,h*2),Image.Resampling.NEAREST);plate.paste(zoom,(x,y+18),zoom)
    plate.save(OUT/name/f'{state}-inspection.png')
    return row
def character_exports():
    allrows=[]
    for name in NAMES:
        ref=Image.open(ROOT/f'ArtSource/Forest/Approved/{name}.png').convert('RGBA')
        states=['Idle','AutoAttack','Hit','Death']
        if name in ['Elven_Mender','Orc_Rootbinder','Barkhide_Warden','Briar_Matriarch']:
            states+=['ChannelStart','ChannelHold','Release','Interrupt']
        for state in states:
            usepilot=name=='Elven_Mender' and state in ['Idle','ChannelStart','ChannelHold','Release','Interrupt']
            source='Release' if state=='Interrupt' else state
            if state=='AutoAttack':source='Attack_v3' if name in ['Orc_Rootbinder','Briar_Matriarch'] else 'Attack_v2'
            if usepilot:
                paths=sorted((PILOT/source).glob('[0-9][0-9].png'))
                frames=[Image.open(p).convert('RGBA') for p in paths]
                durations={'Idle':[130]*9,'ChannelStart':[70,70,90,70,70,90,70,90,80],
                           'ChannelHold':[210,170,170,170],'Release':[60,60,80,60,60,80,60,80,70]}[source]
                indices=list(range(len(frames)));origin='approved-phase-03-pilot/'+source
            else:
                if not (ART/f'Raw/{name}/{source}/00.png').exists():continue
                count=len(list((ART/f'Raw/{name}/{source}').glob('[0-9][0-9].png')))
                indices=list(range(count));origin='Raw/'+name+'/'+source
                if state=='Idle':indices=list(range(8));durations=[145]*8
                elif state=='Hit':durations=[45,55,70,65,45]
                elif state=='Death':
                    indices=list(range(7 if name=='Briar_Matriarch' else 8));durations=[65]*(len(indices)-1)+[100]
                elif state=='AutoAttack':
                    if source=='Attack_v2':
                        indices=[0,2,4,5,8,9,10,12,14,16] if name=='Orc_Trailguard' else [0,2,4,6,8,9,11,13,15,16]
                        durations=[60,70,75,65,45,55,65,75,75,75]
                    else:durations=[60,70,80,70,50,60,70,85,75]
                elif state=='ChannelStart':
                    indices=list(range(5 if name=='Briar_Matriarch' else 9));durations=[100]*len(indices)
                elif state=='ChannelHold':indices=list(range(4));durations=[180]*4
                else:durations=[55,60,65,65,65,65,70,75,80]
                frames=[]
                for i in indices:
                    raw=load(name,source,i)
                    if name=='Orc_Rootbinder':
                        centers={'ChannelStart':{4:9,5:8,6:8,7:9,8:9},'ChannelHold':{0:9,1:8,2:8,3:8},'Release':{0:9,1:10,2:13}}
                        if i in centers.get(source,{}):raw=root_staff_repair(raw,ref,centers[source][i])
                    if name=='Briar_Matriarch' and source=='Release':
                        # Remove unrequested cyan magic before palette mapping;
                        # healing effects are a separate gameplay-driven layer.
                        a=np.array(raw);r,g,b=[a[:,:,k].astype(int) for k in range(3)]
                        magic=(g>r+15)&(b>r+15)&(g>150)&(b>150);a[magic]=0
                        raw=Image.fromarray(a)
                    frames.append(palette_repair(raw,ref))
                if state in ['Idle','AutoAttack','Hit','Death','ChannelStart']:frames[0]=ref.copy() if frames[0].width==64 or ref.width==96 else padded(ref)
                if state in ['AutoAttack','Hit','Release','Interrupt']:frames[-1]=ref.copy() if frames[-1].width==64 or ref.width==96 else padded(ref)
                if state=='ChannelStart':
                    frames[-1]=Image.open(ART/f'Inputs/{name}_Held.png').convert('RGBA')
                if state in ['Release','Interrupt','ChannelHold']:
                    frames[0]=Image.open(ART/f'Inputs/{name}_Held.png').convert('RGBA')
            frames=[padded(f) for f in frames]
            if state=='Idle':
                for index,frame in enumerate(frames):
                    bottom=frame.getbbox()[3]
                    if bottom==frame.height-1:
                        # Exact one-pixel contact correction, no resampling or crop.
                        grounded=Image.new('RGBA',frame.size);grounded.paste(frame,(0,1));frames[index]=grounded
            # Straight transparent padding adds reach without changing source-pixel scale.
            impact=4 if state=='AutoAttack' else -1
            allrows.append(frames_export(name,state,frames,durations,origin,indices,impact))
    (OUT/'animation-manifest.json').write_text(json.dumps(dict(clips=allrows),indent=2),encoding='utf-8')
def modular_exports():
    folder=OUT/'Environment';folder.mkdir(exist_ok=True)
    for name in ['Distant_Woodland','Woodland_Ground','Ancient_Tree','Forest_Fern','Forest_Ruin','Tall_Woodland']:
        if not (ART/f'Raw/{name}/Art/00.png').exists():continue
        image=load(name,'Art',0)
        if name=='Tall_Woodland':
            a=np.array(image);bright=a[:,:,:3].mean(2)>110
            a[bright,:3]=[57,85,80];image=Image.fromarray(a)
        image.save(folder/f'{name}.png')
    load('Distant_Woodland','Art',0).crop((0,0,256,64)).save(folder/'Distant_Canopy.png')
    folder=OUT/'UI';folder.mkdir(exist_ok=True)
    cell=load('Square_Log_Cell','Art',0)
    # Fill only transparent corner pixels with dark endgrain, so seams reveal
    # timber instead of the global backdrop. No resizing, circle mask or gutters.
    base=Image.new('RGBA',(64,64),'#503622');base.alpha_composite(cell);cell=base
    for index,op in enumerate([None,Image.Transpose.FLIP_LEFT_RIGHT,Image.Transpose.ROTATE_180],1):
        tile=cell if op is None else cell.transpose(op);tile.save(folder/f'Log_Cell_0{index}.png')
    for state,factor in [('Normal',1.25),('Highlighted',1.45),('Pressed',1),('Disabled',.75)]:
        a=np.array(cell);a[:,:,:3]=np.clip(a[:,:,:3].astype(float)*factor,0,255).astype('uint8')
        Image.fromarray(a).save(folder/f'Supply{state}.png')
    timber=Image.open(ROOT/'ArtSource/Forest/UI/Panel_Timber_v1.png').convert('RGBA')
    frame=Image.open(ROOT/'ArtSource/Forest/UI/Wood_Frame_Accepted.png').convert('RGBA').crop((20,20,108,108))
    def shell(w,h):
        out=Image.new('RGBA',(w,h))
        for y in range(0,h,timber.height):
            for x in range(0,w,timber.width):out.paste(timber,(x,y))
        # Nine native pieces; tile edge centers, never stretch corners.
        b=12
        for y in range(b,h-b):
            out.paste(frame.crop((0,44,b,45)),(0,y));out.paste(frame.crop((76,44,88,45)),(w-b,y))
        for x in range(b,w-b):
            out.paste(frame.crop((44,0,45,b)),(x,0));out.paste(frame.crop((44,76,45,88)),(x,h-b))
        for box,xy in [((0,0,b,b),(0,0)),((76,0,88,b),(w-b,0)),((0,76,b,88),(0,h-b)),((76,76,88,88),(w-b,h-b))]:
            piece=frame.crop(box);out.paste(piece,xy,piece)
        return out
    panel=shell(128,128);panel.save(folder/'PanelShell.png')
    for name,path in [('Royal','UI/Ability UI/Royal_Decree_Button_new.png'),('Harmony','UI/Ability UI/Horrible_Harmony.png'),('Camera','GideonGlass/Gideon_AbilityButton.png')]:
        original=Image.open(ROOT/'Assets/_Game/Art'/path).convert('RGBA')
        a=np.array(original);r,g,b=[a[:,:,k].astype(int) for k in range(3)]
        background=(r>g*1.2)&(b>r*.75)&(g<110)
        a[background]=0
        # Retain the approved center glyph at exactly its native size and position.
        box={'Royal':(68,7,108,53),'Harmony':(58,7,127,53),'Camera':(66,9,110,54)}[name]
        glyph=Image.fromarray(a).crop(box);button=Image.new('RGBA',(176,64))
        button.alpha_composite(glyph,box[:2]);button.save(folder/f'Ability{name}.png')
    button=shell(96,32)
    for state,factor in [('Normal',1),('Highlighted',1.18),('Pressed',.8),('Disabled',.58)]:
        a=np.array(button);a[:,:,:3]=np.clip(a[:,:,:3].astype(float)*factor,0,255).astype('uint8')
        Image.fromarray(a).save(folder/f'Button{state}.png')
        gear=Image.open(ROOT/f'Assets/_Game/Resources/UI/Finalized/Settings{state}.png').convert('RGBA')
        # Existing gear silhouette stays readable; a separate forest copy uses
        # the approved warm wood palette without changing opened settings.
        warm=palette_repair(gear,Image.open(ROOT/'ArtSource/Forest/UI/Wood_Frame_Accepted.png'))
        warm.save(folder/f'Settings{state}.png')
    energy=shell(144,32)
    # Preserve the actual authored fill rectangle and its padding.
    ImageDraw.Draw(energy).rectangle((18,11,124,18),fill=(0,0,0,0));energy.save(folder/'EnergyFrame.png')
    fx=OUT/'Effects';fx.mkdir(exist_ok=True)
    for name in ['Vine_Overlay','Vine_Anchor','Leaf_Heal']:
        image=load(name,'Art',0)
        if name=='Vine_Anchor':
            a=np.array(image);r,g,b=a[:,:,0],a[:,:,1],a[:,:,2];mask=(r.astype(int)>g.astype(int)*1.4)&(r>75)&(a[:,:,3]>0)
            a[mask,:3]=[181,127,56];image=Image.fromarray(a)
        # Generated art arrives binary-alpha; normalize hidden RGB only.
        a=np.array(image);a[a[:,:,3]==0,:3]=0;Image.fromarray(a).save(fx/f'{name}.png')
    warning=Image.new('RGBA',(64,64));vine=Image.open(fx/'Vine_Overlay.png')
    a=np.array(vine);visible=a[:,:,3]>0;a[visible,:3]=[222,182,89];Image.fromarray(a).save(fx/'Vine_Warning.png')
    # Reuse the seed leaves for the zone badge and a warm severed-link cue.
    leaf=Image.open(fx/'Leaf_Heal.png');leaf.save(fx/'Resonance.png')
    a=np.array(leaf);a[a[:,:,3]>0,:3]=[238,185,99];Image.fromarray(a).save(fx/'Interrupt.png')
    records=[]
    for directory in ['UI','Environment','Effects']:
        for p in sorted((OUT/directory).glob('*.png')):
            im=Image.open(p).convert('RGBA');a=np.array(im);visible=a[:,:,3]>0
            records.append(dict(path=str(p.relative_to(OUT)).replace('\\','/'),width=im.width,height=im.height,
                paletteCount=len(np.unique(a[visible,:3],axis=0)),alphaValues=np.unique(a[:,:,3]).tolist(),
                ppu=64,resampled=False,hash=sha(p)))
    (OUT/'modular-manifest.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
if __name__=='__main__':
    character_exports();modular_exports();print('Native selections, manifests and modular assets exported.')
