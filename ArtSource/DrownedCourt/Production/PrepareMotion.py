"""Native frame selection, palette matching and sprite-sheet assembly; no generation or resizing."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
r=Path(__file__).resolve().parent;out=r/'Selected/Motion';out.mkdir(exist_ok=True)
ids=['reef_spearman','hammerhead_bruiser','needlefin_skirmisher','shellback_porter','pearl_thief','pearl_cantor','conch_marshal','moray_siphoner','reef_netweaver','puffer_sentinel','breakwater_captain','lantern_warden','queen_nacre','skittercrab']
clips=[];checks=[];selections=[]
def images(job):
 fs=sorted((r/'MotionRaw'/job).glob('*.png'))
 if not fs:raise FileNotFoundError(job)
 return [Image.open(p).convert('RGBA') for p in fs]
def ready(name):
 im=Image.open(r/'Selected'/(name+'.png')).convert('RGBA')
 if name in ['reef_spearman','breakwater_captain']:
  wide=Image.new('RGBA',(128,80));wide.paste(im,(24,0));im=wide
 return im
def clean(im,name,job):
 base=ready(name);palette=list(dict.fromkeys(p[:3] for p in base.get_flattened_data() if p[3]))
 # Rejected generated impact sparks are isolated saturated red/yellow pixels,
 # absent from the character's restrained material palette. Preserve the body.
 pixels=[];mapping={}
 for p in im.get_flattened_data():
  if p[3]<128 or (p[0]>=235 and p[2]<90 and p[0]-p[2]>150):pixels.append((0,0,0,0));continue
  if p[:3] not in mapping:mapping[p[:3]]=min(palette,key=lambda c:sum((c[k]-p[k])**2 for k in range(3)))
  pixels.append((*mapping[p[:3]],255))
 im.putdata(pixels)
 canvas=Image.new('RGBA',base.size)
 dx=(base.width-im.width)//2
 if job=='reef_spearman_attack_v2':dx=-8
 bounds=im.getbbox();dy=base.height-1-bounds[3] if bounds else 0
 canvas.paste(im,(dx,dy))
 return canvas
def clip(name,state,job,indices,durations,impact=0,loop=False,special=False,endReady=False):
 raw=images(job);frames=[clean(raw[i].copy(),name,job) if i or state in ['InflatedAttack','DepthsRelease'] else ready(name) for i in indices]
 if endReady:frames.append(ready(name));durations=durations+[80]
 assert len(frames)==len(durations)
 w,h=frames[0].size;folder=out/name;folder.mkdir(exist_ok=True)
 strip=Image.new('RGBA',(w*len(frames),h))
 for i,frame in enumerate(frames):strip.paste(frame,(i*w,0))
 path=folder/(state+'.png');strip.save(path)
 clips.append(dict(name=name,state=state,width=w,height=h,frameCount=len(frames),durationsMs=durations,impactFrame=impact,loop=loop,special=special))
 selections.append(dict(name=name,state=state,job=job,indices=indices,end_ready=endReady))
 colors=set(p[:3] for p in strip.get_flattened_data() if p[3]);alpha=sorted(set(strip.getchannel('A').get_flattened_data()))
 checks.append(dict(name=name,state=state,size=strip.size,frame_size=(w,h),frames=len(frames),colors=len(colors),alpha=alpha,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
 # Real GIFs use a neutral background; the production PNG keeps binary alpha.
 gif=[]
 for frame in frames:
  bg=Image.new('RGBA',(w,h),(84,84,84,255));bg.alpha_composite(frame);gif.append(bg.convert('RGB'))
 gif[0].save(folder/(state+'.gif'),save_all=True,append_images=gif[1:],duration=durations,loop=0,disposal=2)
 preview=Image.new('RGB',(min(9,len(frames))*(w+4),((len(frames)+8)//9)*(h+16)),(84,84,84));d=ImageDraw.Draw(preview)
 for i,frame in enumerate(frames):
  x=i%9*(w+4);y=i//9*(h+16);preview.paste(frame,(x,y),frame);d.text((x,y+h),str(i),fill='white')
 preview.save(folder/(state+'_native.png'))
for name in ids:
 clip(name,'Idle',name+'_idle',[0,1,2,3,4],[200,160,180,160,200],loop=True)
 attack=name+'_attack';indices=list(range(9));impact=5
 if name=='reef_spearman':attack+='_v2';indices=[0,1,2,3,4,5,6,7,8,7,5,3,2,1];impact=6
 elif name=='needlefin_skirmisher':attack+='_v2';indices=[0,1,2,3,4,5,6,8,9,10,12,13,14,15,16];impact=8
 elif name=='breakwater_captain':attack+='_v3';indices=[0,1,2,3,4,5,6,7,11,12,13,14,15,16];impact=8
 elif name=='lantern_warden':attack+='_v2';indices=[0,1,2,4,5,6,11,12,13,14,15,16];impact=3
 elif name=='queen_nacre':indices=[0,1,2,3,4,5,6,7,9,10,11,12,13,14,15,16];impact=6
 elif name=='moray_siphoner':indices=[0,1,2,3,4,3,2,1];impact=4
 elif name=='reef_netweaver':indices=[0,1,3,4,5,6,7,8,12,13,14,15,16];impact=8
 elif name=='puffer_sentinel':attack+='_v2';indices=[0,1,2,4,5,6,9,10,11,12,13,14,15,16];impact=6
 elif name in ['hammerhead_bruiser','shellback_porter']:impact=7
 clip(name,'AutoAttack',attack,indices,[45 if i!=impact else 65 for i in range(len(indices))],impact=impact,endReady=True)
 death=name+'_death';indices=[0,4,5,6,7,9,10,11,12,13,14,15,16]
 if name=='lantern_warden':indices=[0,5,6,7,8,9,10,11,12,13,14,15,16]
 if name=='skittercrab':indices=[0,4,5,6,7,8,13,14,15,16]
 clip(name,'Death',death,indices,[50]*(len(indices)-1)+[180])
 hit=5 if name=='lantern_warden' else 4
 if name=='queen_nacre':clip(name,'Hit',name+'_attack',[0,2,3,2],[35,40,40,35],endReady=True)
 else:clip(name,'Hit',death,[0,hit,hit],[35,40,40],endReady=True)
 if name in ['pearl_thief','pearl_cantor','conch_marshal','moray_siphoner','reef_netweaver','puffer_sentinel','breakwater_captain','lantern_warden','queen_nacre']:
  job=name+'_ability'
  if name in ['puffer_sentinel','conch_marshal']:job+='_v2'
  if name=='pearl_cantor':job=name+'_channel'
  hold=5 if name=='puffer_sentinel' else 7
  clip(name,'Ability',job,list(range(17)),[50]*17,impact=hold,endReady=True)
  clip(name,'ChannelStart',job,list(range(hold+1)),[55]*(hold+1),impact=hold,special=True)
  clip(name,'ChannelHold',job,[hold,hold+1,hold],[180,180,180],loop=True)
  clip(name,'Release',job,list(range(hold,17)),[45]*(17-hold),impact=1,special=True,endReady=True)
  clip(name,'Interrupt',job,list(range(hold,-1,-1)),[30]*(hold+1),endReady=True)
  # Instant casts also deliver one owned impact through the generic presenter.
  clips[-5]['special']=True
  if name=='puffer_sentinel':
   # Stay visibly inflated for its whole warning; deflation belongs to Release.
   clip(name,'InflatedIdle',job,[4,5,6,5],[160]*4,loop=True)
   inflated=[0,1,2,3,4,5,6,7,8,9,12,13,14,15,16]
   clip(name,'InflatedAttack','puffer_sentinel_inflated_attack',inflated,[45]*len(inflated),impact=8)
   clip(name,'Ability',job,list(range(6)),[55]*6,impact=5,special=True)
   # Remove the earlier full-cycle Ability entry; the final PNG above is the held cast.
   matching=[i for i,c in enumerate(clips) if c['name']==name and c['state']=='Ability'];del clips[matching[0]]
  if name=='queen_nacre':
   strikes=[0,1,2,7,8,9,10,11,12,13,14,15,16]
   clip(name,'DepthsRelease','queen_nacre_depths_release',strikes,[55]*len(strikes),impact=3,special=True,endReady=True)
(out/'animation-manifest.json').write_text(json.dumps({'clips':clips},indent=2))
checks=list({(x['name'],x['state']):x for x in checks}.values());selections=list({(x['name'],x['state']):x for x in selections}.values())
(out/'NativeChecks.json').write_text(json.dumps({'operations':'Native palette mapping to selected still, isolated generated spark removal, canvas padding and baseline translation. No resizing or interpolation. Exact input/ready frames return between actions.','checks':checks,'selections':selections},indent=2))
print(len(clips),'motion clips assembled; inspect NativeChecks and actual frame sheets before importing.')
