from pathlib import Path
import subprocess,sys,shutil
p=Path(sys.argv[1]); w=Path(__file__).parent
editor=Path('C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe')
for name in ['AuthorNative','ReopenNative']:
    result=subprocess.run([str(editor),'-b','--script','ArtSource/ForestReworkV2/Checkpoint04/Scripts/'+name+'.js'],cwd=p,capture_output=True,text=True)
    (p/'ArtSource/ForestReworkV2/Checkpoint04/Diagnostics'/(name+'.log')).write_text(result.stdout+'\n'+result.stderr,encoding='utf-8')
    print(name,'exit',result.returncode);result.check_returncode()
validator=w/'ValidateAssetsAndCaptures.py'
if not validator.exists():validator=w/'ValidateForestC4.py'
subprocess.run([sys.executable,str(validator),str(p)],check=True)
