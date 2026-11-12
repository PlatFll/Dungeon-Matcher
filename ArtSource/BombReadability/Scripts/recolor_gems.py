from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil,subprocess
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'ArtSource/BombReadability/Palette'
LIBRE='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
def rgb(s):return tuple(bytes.fromhex(s.lstrip('#')))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
maps={
'Ruby':{'412C35':'412C35','63374A':'632733','A43838':'8F3340','E95050':'D64C57','FF8D99':'F06B70','F3B3B3':'FF98A0'},
'Emerald':{'35835E':'176747','359E6C':'218C50','40B640':'31B45D','66D330':'56D76B','ABFF2D':'D5FFAD'},
'Sapphire':{'20284E':'223A6B','2C4A78':'2E568F','3875A1':'4E96DB','8BCADD':'A9E0FF'},
'Amethyst':{'29007B':'2B0B5F','5C0EA2':'4A1590','8F2DBC':'6E27BF','C658D9':'9139D9','E781DE':'DD92F7'},
'Topaz':{},'Amber':{}}
names=['Ruby','Amber','Topaz','Emerald','Sapphire','Amethyst']
main_names=['Ruby','Amber_tri','Topaz','Emerald','Sapphire (1)','Amethyst']
for sub in ['Originals','Editable','Exports']: (OUT/sub).mkdir(parents=True,exist_ok=True)
records=[];board=Image.new('RGB',(800,6*150+36),'#202433');draw=ImageDraw.Draw(board)
for j,t in enumerate(['Current board gem','Old 16px','New 16px','Old weakness','New weakness']):draw.text((j*160+8,9),t,fill='white')
for row,(name,main_name) in enumerate(zip(names,main_names)):
    master=ROOT/'Assets/_Game/Art/Gems'/f'{main_name}.png';large=Image.open(master).convert('RGBA');allowed={p[:3] for p in large.getdata() if p[3]}
    board.paste(large,(40,row*150+52),large)
    for size,sub in [(16,'Gesm16'),(32,'Gems32')]:
        target=ROOT/'Assets/_Game/Art/Gems/Small gems'/sub/f'{name}{size}.png';original=OUT/'Originals'/target.name
        if not original.exists():shutil.copy2(target,original)
        im=Image.open(original).convert('RGBA');mapping={rgb(k):rgb(v) for k,v in maps[name].items()}
        for c in {p[:3] for p in im.getdata() if p[3]}:
            if name=='Amber' and c not in [rgb('000000'),rgb('303030'),rgb('863904'),rgb('E0B35A')]:
                mapping[c]=rgb('7C2400' if c[0]<150 else 'A33200' if c[0]<190 else 'C94D08' if c[0]<220 else 'F5923D' if c[0]<246 and c[1]>110 else 'E56B12' if c[0]<246 else 'FFB870')
        if size==16: mapping[rgb('000000')]=rgb('303030')
        result=Image.new('RGBA',im.size);result.putdata([(*mapping.get(p[:3],p[:3]),p[3]) for p in im.getdata()])
        assert result.getchannel('A').tobytes()==im.getchannel('A').tobytes()
        assert all(c in allowed for c in mapping.values())
        # Golden outline and the small gold weakness rim are independent from gem facets.
        if size==32:
            for p,q in zip(im.getdata(),result.getdata()):
                if p[:3] in [rgb('000000'),rgb('863904'),rgb('E0B35A')]:assert p==q
        export=OUT/'Exports'/target.name;result.save(export)
        ase=OUT/'Editable'/f'{name}{size}.aseprite'
        subprocess.run([LIBRE,'-b',str(export),'--save-as',str(ase)],check=True,capture_output=True)
        roundtrip=OUT/'Exports'/f'{name}{size}-roundtrip.png'
        subprocess.run([LIBRE,'-b',str(ase),'--save-as',str(roundtrip)],check=True,capture_output=True)
        assert Image.open(roundtrip).convert('RGBA').tobytes()==result.tobytes()
        shutil.copy2(export,target)
        records.append({'path':target.relative_to(ROOT).as_posix(),'original_sha256':sha(original),'new_sha256':sha(target),'master':master.relative_to(ROOT).as_posix(),'master_sha256':sha(master),'alpha_unchanged':True,'gold_rim_unchanged':size==32,'palette_map':{'#%02X%02X%02X'%k:'#%02X%02X%02X'%v for k,v in mapping.items()}})
        x=160 if size==16 else 480;scale=3
        for n,pic in enumerate([im,result]):
            zoom=pic.resize((size*scale,size*scale),Image.Resampling.NEAREST);board.paste(zoom,(x+n*160+25,row*150+52),zoom)
    draw.text((8,row*150+32),name,fill='white')
board.save(OUT/'PaletteComparison.png');(OUT/'Manifest.json').write_text(json.dumps(records,indent=2)+'\n')
print('12 exact-shape recolors exported through LibreSprite; current board gems and gold rims unchanged.')
