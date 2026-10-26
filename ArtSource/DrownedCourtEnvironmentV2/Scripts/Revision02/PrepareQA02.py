from pathlib import Path
import json,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
data=json.loads((R/'Native/Revision02/Manifest.json').read_text());script='try{\n'
for a in data['assets']:
 n=a['name'];alias=R/f'QA/Revision02/{n}-native.ase';shutil.copy2(R/f'Native/Revision02/{n}.ase',alias);script+=f'app.open({json.dumps(alias.as_posix())});app.activeSprite.saveAs({json.dumps((R/f"QA/Revision02/{n}-roundtrip.png").as_posix())},true);app.activeDocument.close();\n'
script+='console.log("POLISH02_ROUNDTRIPS_COMPLETE");}catch(e){console.log("ROUNDTRIP_FAILED "+e);}app.command.Exit();'
(R/'Scripts/Revision02/Roundtrip.js').write_text(script);shutil.copy2(W/'VerifyPolish02.py',R/'Scripts/Revision02/VerifyPolish02.py')
