from pathlib import Path
import urllib.request,json
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/LivingForest'
jobs=json.loads((Path(__file__).parent/'LivingForestJobs.json').read_text());jobs.update(json.loads((Path(__file__).parent/'LivingForestSeedJob.json').read_text()))
for name,q in jobs.items():
 p=R/'Raw'/(name+'.png')
 if not p.exists():
  with urllib.request.urlopen('https://api.pixellab.ai/mcp/images/'+q['job_id']+'/download?index=0',timeout=60)as r:p.write_bytes(r.read())
 (R/'Diagnostics'/(name+'-Generation.json')).write_text(json.dumps(q,indent=2))
 print(name,p.stat().st_size)
