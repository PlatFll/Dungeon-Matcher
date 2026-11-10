"""Prepare explicit edits, execute them inside LibreSprite, verify native exports."""
from pathlib import Path
from PIL import Image
from collections import Counter,deque
import json,hashlib,subprocess
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[2]
EXE='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def rgb(h):return tuple(bytes.fromhex(h.lstrip('#')))
plans=[];expected={};native={}
for key in ['A','B','CWave','CSettings']:
    source=HERE/'Raw'/f'{key}.png';im=Image.open(source).convert('RGBA');out=im.copy();changes=[]
    if key in ['A','B']:
        mapping={rgb('241f3a'):rgb('11101b')}
        if key=='B':mapping[rgb('151423')]=rgb('171622')
    else:mapping={}
    for y in range(im.height):
        for x in range(im.width):
            c=im.getpixel((x,y))
            if c[3] and c[:3] in mapping:out.putpixel((x,y),(*mapping[c[:3]],255))
    # C's inner recess is a separate connected near-black region. Never remap its outline.
    if key.startswith('C'):
        center=(96,30) if key=='CWave' else (32,15)
        source_color=out.getpixel(center)
        if source_color[:3] in [rgb('171622'),rgb('151423'),rgb('241f3a')]:
            q=deque([center]);seen={center}
            while q:
                x,y=q.popleft();out.putpixel((x,y),(*rgb('11101b'),255))
                for p in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)]:
                    if 0<=p[0]<im.width and 0<=p[1]<im.height and p not in seen and out.getpixel(p)==source_color:
                        seen.add(p);q.append(p)
    # Tiny warm studs tie A's Settings to its wave plaque; all stay on existing rim pixels.
    if key=='A':
        for cx,cy in [(40,72),(40,112),(21,92),(61,92)]:
            for dy in range(-2,3):
                for dx in range(-2,3):
                    if abs(dx)+abs(dy)>2:continue
                    x,y=cx+dx,cy+dy
                    if not im.getpixel((x,y))[3]:continue
                    tone='8a5622' if dx+dy>1 else 'c58826' if dx+dy>=0 else 'fbea90'
                    out.putpixel((x,y),(*rgb(tone),255))
    if key=='CWave':
        gold={rgb(c) for c in ['fbea90','c58826','8a5622','60340f','e8b84d']}
        for y in range(44,min(im.height,60)):
            for x in range(87,106):
                c=out.getpixel((x,y))
                if c[3] and c[:3] in gold:out.putpixel((x,y),(*rgb('c2c1d0' if c[:3] in [rgb('60340f'),rgb('8a5622')] else 'eceaf2'),255))
    # Remove only disconnected 1-2px flecks. Keep full skull, ornament and bevel clusters.
    mask={(x,y) for y in range(im.height) for x in range(im.width) if out.getpixel((x,y))[3]};seen=set();removed=[]
    for p in sorted(mask):
        if p in seen:continue
        q=[p];comp=[];seen.add(p)
        while q:
            x,y=q.pop();comp.append((x,y))
            for dx in (-1,0,1):
                for dy in (-1,0,1):
                    n=(x+dx,y+dy)
                    if n in mask and n not in seen:seen.add(n);q.append(n)
        if len(comp)<=2:
            for p in comp:out.putpixel(p,(0,0,0,0));removed.append(p)
    for y in range(im.height):
        for x in range(im.width):
            if im.getpixel((x,y))!=out.getpixel((x,y)):changes.append([x,y,*out.getpixel((x,y))])
    regions={'Wave':(0,0,192,68),'Settings':(0,68,100,128)} if key in ['A','B'] else {'Wave' if key=='CWave' else 'Settings':(0,0,im.width,im.height)}
    crops=[]
    for role,region in regions.items():
        region_im=out.crop(region);b=region_im.getbbox();assert b,(key,role)
        bbox=(b[0]+region[0],b[1]+region[1],b[2]+region[0],b[3]+region[1])
        # Add two native transparent pixels; no source scaling or resampling.
        crop=[bbox[0]-2,bbox[1]-2,bbox[2]-bbox[0]+4,bbox[3]-bbox[1]+4]
        if role=='Settings':
            # Match the live square canvas by transparent padding, never stretch.
            crop=[bbox[0]-(56-(bbox[2]-bbox[0]))//2,bbox[1]-(56-(bbox[3]-bbox[1]))//2,56,56]
        name=key[0]+'_'+role
        crops.append({'name':name,'crop':crop});native[name]=out.crop((crop[0],crop[1],crop[0]+crop[2],crop[1]+crop[3]))
    plans.append({'key':key,'source':source.as_posix(),'width':im.width,'height':im.height,'changes':changes,'removed_flecks':removed,'crops':crops})
    expected[key]=out
(HERE/'QA'/'NativeEditPlan.json').write_text(json.dumps(plans,indent=2)+'\n')
script='var plans='+json.dumps(plans)+';\nvar dest='+json.dumps(HERE.as_posix()+'/')+';\n'
script+=(HERE/'cleanup_body.js').read_text()
(HERE/'cleanup_generated.js').write_text(script)
startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
run=subprocess.run(['rtk',EXE,'--script',str(HERE/'cleanup_generated.js')],cwd=ROOT,startupinfo=startup,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=90)
(HERE/'QA'/'LibreSprite.log').write_bytes(run.stdout)
assert run.returncode==0 and b'SILVER_RELIC_CLEANUP_COMPLETE' in run.stdout,run.stdout.decode(errors='replace')
# This installed build's JS crop method returned unchanged dimensions. Use its
# native batch crop command, and verify the resulting pixels and ASE roundtrip.
for p in plans:
    for c in p['crops']:
        name=c['name'];ase=HERE/'Editable'/f'{name}.ase';png=HERE/'Cleaned'/f'{name}.png'
        subprocess.run(['rtk',EXE,'--batch',str(HERE/'Editable'/f"{p['key']}_Sheet.ase"),
                        '--crop',','.join(map(str,c['crop'])),'--save-as',str(ase)],
                       cwd=ROOT,startupinfo=startup,check=True,stdout=subprocess.DEVNULL,timeout=30)
        subprocess.run(['rtk',EXE,'--batch',str(ase),'--save-as',str(png)],
                       cwd=ROOT,startupinfo=startup,check=True,stdout=subprocess.DEVNULL,timeout=30)
        subprocess.run(['rtk',EXE,'--batch',str(ase),'--save-as',str(HERE/'Roundtrip'/f'{name}.png')],
                       cwd=ROOT,startupinfo=startup,check=True,stdout=subprocess.DEVNULL,timeout=30)
def visible_bytes(im):
    # PNG encoders may retain arbitrary RGB under alpha=0; compare exact alpha
    # and every visible color, without accepting any visible color tolerance.
    return bytes(v for c in im.get_flattened_data() for v in (c if c[3] else (0,0,0,0)))
report={}
for name,im in native.items():
    file=HERE/'Cleaned'/f'{name}.png';actual=Image.open(file).convert('RGBA');rt=Image.open(HERE/'Roundtrip'/f'{name}.png').convert('RGBA')
    assert actual.size==im.size and visible_bytes(actual)==visible_bytes(im),name
    assert actual.size==rt.size and visible_bytes(actual)==visible_bytes(rt),name
    assert set(actual.getchannel('A').get_flattened_data())=={0,255}
    b=actual.getbbox();assert b[0]>=2 and b[1]>=2 and actual.width-b[2]>=2 and actual.height-b[3]>=2,name
    colors=Counter(c[:3] for c in actual.get_flattened_data() if c[3]);assert len(colors)<=16
    report[name]={'dimensions':list(actual.size),'bounds':b,'colors':{'#%02X%02X%02X'%c:n for c,n in colors.items()},
                  'color_count':len(colors),'binary_alpha':True,'minimum_clearance':2,'libresprite_exact_roundtrip':True,'sha256':sha(file)}
    actual.resize((actual.width*4,actual.height*4),Image.Resampling.NEAREST).save(HERE/'Preview'/f'{name}-4x.png')
(HERE/'QA'/'NativeValidation.json').write_text(json.dumps(report,indent=2)+'\n')
print('LibreSprite cleanup/export/reopen verified for',len(report),'native UI candidates.')
print(json.dumps({k:{'dimensions':v['dimensions'],'colors':v['color_count']} for k,v in report.items()}))
