from pathlib import Path
import json,urllib.request,shutil,sys
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/PolishFinal';W=Path(__file__).parent
data={p.stem.removesuffix('-Generation'):json.loads(p.read_text())for p in (R/'Diagnostics').glob('*-Generation.json')}
for name in sys.argv[1:] or data:
 q=data[name];dest=R/'Raw'/(name+'.png')
 if not dest.exists():
  with urllib.request.urlopen('https://api.pixellab.ai/mcp/images/'+q['job_id']+'/download?index=0',timeout=90)as r:dest.write_bytes(r.read())
 (R/'Diagnostics'/(name+'-Generation.json')).write_text(json.dumps(q,indent=2))
 print(name,dest.stat().st_size)
