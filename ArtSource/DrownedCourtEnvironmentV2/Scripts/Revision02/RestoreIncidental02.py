from pathlib import Path
import subprocess
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher')
names=['Assets/_Game/Resources/UI/Consumables/'+n+'.png.meta' for n in ['SlotDisabled','SlotHighlighted','SlotPressed']]
for name in names:
 old=subprocess.check_output(['git','show','HEAD:'+name],cwd=F).decode();new=(F/name).read_text()
 norm=lambda s:'\n'.join(v.rstrip() for v in s.splitlines())
 assert norm(old)==norm(new),name+' non-whitespace change; do not restore'
subprocess.run(['git','restore','--',*names],cwd=F,check=True)
print('Only three known Unity whitespace-only metadata rewrites restored in the isolated checkout.')
