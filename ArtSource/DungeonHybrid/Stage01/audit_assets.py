"""Read-only audit of retained production art; writes only this stage's review evidence."""
from pathlib import Path
from PIL import Image, ImageDraw
import argparse, csv, hashlib, json, subprocess

def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p): return Image.open(p).convert('RGBA')
def same(a,b): return a.size == b.size and a.tobytes() == b.tobytes()

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--project',type=Path,default=Path(__file__).resolve().parents[3]);ap.add_argument('--libresprite',required=True)
    args=ap.parse_args();p=args.project;r=p/'ArtSource/DungeonHybrid/Stage01'
    for d in ['Diagnostics','Previews','Reexports']: (r/d).mkdir(parents=True,exist_ok=True)
    manifest=json.loads((p/'ArtSource/DungeonPixelLab/Progress/Asset_Manifest.json').read_text())
    assets=[a for a in manifest['stage15_assets'] if a['name'].startswith(('Wall_','Floor_','Fascia_','Support_','Recess_'))]
    assert len(assets)==33
    protected={}
    for a in manifest['stage15_assets']:
        for key in ['unity_path','export_path','editable_path']:
            path=a[key];protected[path]=sha(p/path)
        path=a['unity_path']+'.meta';protected[path]=sha(p/path)
    for path in ['Assets/_Game/Scenes/Game.unity','Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity','ProjectSettings/EditorBuildSettings.asset','Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','Docs/ArtDirection/Golden_Reference_Register.md']:
        protected[path]=sha(p/path)
    register=(p/'Docs/ArtDirection/Golden_Reference_Register.md').read_text(encoding='utf8')
    refs=['Docs/ArtDirection/Audit2026-10/media/skeleton-still.png','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png','Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png','Assets/_Game/Art/CombatIdles/Farmer_Idle.png','Assets/_Game/Art/CombatIdles/PanVillager_Idle.png']
    for path in refs:
        protected[path]=sha(p/path);assert protected[path] in register,path
    snapshot=r/'Diagnostics/ProtectedHashes.json'
    if snapshot.exists():
        old=json.loads(snapshot.read_text());assert all(sha(p/f)==h for f,h in old.items()),'Protected file changed'
    else:snapshot.write_text(json.dumps(protected,indent=2)+'\n')
    report=[]
    for a in assets:
        output=r/'Reexports'/(a['name']+'.png')
        subprocess.run([args.libresprite,'-b',str(p/a['editable_path']),'--save-as',str(output)],check=True,capture_output=True,timeout=25)
        src=load(p/a['unity_path']);fresh=load(output)
        assert same(src,fresh),a['name'];assert same(src,load(p/a['export_path'])),a['name']
        alpha=sorted(set(src.getchannel('A').getdata()));assert set(alpha)<={0,255}
        colors=sorted({c[:3] for c in src.getdata() if c[3]})
        report.append(dict(name=a['name'],family=a['family'],size=list(src.size),colors=len(colors),alpha=alpha,unity_path=a['unity_path'],editable_path=a['editable_path'],sha256=sha(p/a['unity_path']),fresh_libresprite_pixels_equal=True,decision='RETAIN_UNCHANGED',lineage=a['lineage']))
    (r/'Diagnostics/AssetAudit.json').write_text(json.dumps(dict(result='PASS',retained=33,new_generations=0,golden_hashes_verified=6,assets=report),indent=2)+'\n')
    with (r/'AssetLedger.csv').open('w',newline='',encoding='utf8') as f:
        w=csv.writer(f);w.writerow(['asset_id','asset_name','category','source_method','pixellab_used','cleanup_in_libresprite','status','notes'])
        for i,a in enumerate(report):w.writerow([f'S01-{i+1:02}',a['name'],a['family'],'Retained PR203 native source','Historical only; 0 new generations','Existing approved cleanup; fresh native export verified','RETAIN_UNCHANGED',a['unity_path']])
    board=Image.new('RGBA',(800,9*210),(16,19,28,255));d=ImageDraw.Draw(board)
    for i,a in enumerate(report):
        x=(i%4)*200;y=(i//4)*210;d.text((x+8,y+6),a['name'],fill='white');d.text((x+8,y+22),f"{a['size'][0]}x{a['size'][1]} | {a['colors']} colors | unchanged",fill='#bdb8ce');board.alpha_composite(load(p/a['unity_path']),(x+8,y+40))
    board.save(r/'Previews/Retained-assets-native-1x.png')
    for family in sorted({a['family'] for a in report}):
        subset=[a for a in report if a['family']==family]
        b=Image.new('RGBA',(600,((len(subset)+2)//3)*210),(16,19,28,255));draw=ImageDraw.Draw(b)
        for i,a in enumerate(subset):
            x=i%3*200;y=i//3*210;draw.text((x+8,y+6),a['name'],fill='white');draw.text((x+8,y+22),str(a['size'])+' / '+str(a['colors'])+' colors',fill='#bdb8ce');b.alpha_composite(load(p/a['unity_path']),(x+8,y+40))
        b.save(r/'Previews'/(family+'-native-1x.png'))
    # Diagnostic style board: actual current sources, never production derivatives.
    samples=[('Rattlebones',refs[0]),('Farmer',refs[1]),('Pan Villager',refs[2])]
    samples += [(name,'Assets/_Game/Art/Gems/'+name+'.png') for name in ['Amber_tri','Amethyst','Emerald','Ruby','Sapphire (1)','Topaz']]
    samples += [(a['name'],a['unity_path']) for a in manifest['stage15_assets'] if a['name'] in ['Portrait_Corner_TL','Portrait_Clamp','Board_Corner_TL','Plaque_Corner_TL']]
    b=Image.new('RGBA',(600,((len(samples)+3)//4)*130),(16,19,28,255));draw=ImageDraw.Draw(b)
    for i,(name,path) in enumerate(samples):
        x=i%4*150;y=i//4*130;draw.text((x+5,y+5),name,fill='white');im=load(p/path);b.alpha_composite(im,(x+5,y+24));protected[path]=sha(p/path)
    b.save(r/'Previews/Current-style-authorities-native-1x.png')
    snapshot.write_text(json.dumps(protected,indent=2)+'\n')
    approved=json.loads((p/'Assets/_Game/Art/DungeonPixelLab/Stage15FinalPlacement.json').read_text())
    lookup={a['name']:a for a in report}
    placements=[]
    for v in approved['placements']:
        if v['name'] not in lookup:continue
        w,h=lookup[v['name']]['size']
        if v['y']+h>240:continue
        # Preserve established far-structure / foreground order.
        layer=0 if v['layer']==1 else 2 if v['name'].startswith('Support_') else 3
        placements.append(dict(name=v['name'],path=lookup[v['name']]['unity_path'],x=v['x'],y=v['y'],layer=layer))
    (r/'Placement.json').write_text(json.dumps(dict(width=360,height=240,placements=placements,portrait_mask=[18,29,81,172],portrait_repeat=lookup['Recess_Center']['unity_path']),indent=2)+'\n')
    print(json.dumps(dict(result='PASS',retained=len(report),protected_files=len(protected),placements=len(placements),new_generations=0)))

if __name__=='__main__':main()
