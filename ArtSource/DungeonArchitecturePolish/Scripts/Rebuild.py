from pathlib import Path
import argparse,subprocess,shutil
R=Path(__file__).resolve().parents[1];F=R.parents[1]
p=argparse.ArgumentParser();p.add_argument('--libresprite',required=True);args=p.parse_args()
def run(*argv):
 r=subprocess.run(['rtk','proxy',args.libresprite,'-b',*map(str,argv)],cwd=F,capture_output=True,text=True)
 assert r.returncode==0 and 'File not found' not in r.stdout+r.stderr and 'Error' not in r.stdout+r.stderr,r.stdout+r.stderr
run('--script',R/'Scripts/CleanStone.js')
for name,w,h in [('Capital',32,22),('Shaft',32,15),('Shaft_Worn',32,15),('Base',32,23),('Ledge',32,12),('Recess',32,21)]:
 run('--crop',f'0,0,{w},{h}',R/'Previews'/f'{name}_Canvas.png','--save-as',R/'Native'/f'{name}.aseprite')
 run(R/'Native'/f'{name}.aseprite','--save-as',R/'Exports'/f'{name}.png')
run('--script',R/'Scripts/CleanProps.js')
for name in ['Torch','Chain_Anchor','Chain_Repeat','Chain_End']:
 shutil.copy2(F/'Assets/_Game/Art/DungeonHybrid/Stage04'/f'{name}.png',R/'Exports'/f'{name}.png')
 run(R/'Exports'/f'{name}.png','--save-as',R/'Native'/f'{name}.aseprite')
run('--script',R/'Scripts/FinishLighting.js')
run('--script',R/'Scripts/ReviseTorch.js')
for src in (R/'Exports').glob('*.png'):run(R/'Native'/f'{src.stem}.aseprite','--save-as',src)
print('Rebuilt 19 independent native/editable files through LibreSprite. No original production PNG was edited.')
