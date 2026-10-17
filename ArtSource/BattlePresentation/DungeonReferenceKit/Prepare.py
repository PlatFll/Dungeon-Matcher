"""Read-only reference audit and local working directories. No game assets written."""
import hashlib,json,shutil,subprocess
from pathlib import Path
from PIL import Image

KIT=Path(__file__).resolve().parent
ROOT=KIT.parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for folder in ('native','review','reference','logs'): (KIT/folder).mkdir(exist_ok=True)
source=KIT.parent/'brief/references/DUNGEON_ZONE_TARGET.png'
shutil.copyfile(source,KIT/'reference/ApprovedDungeon.png')
shutil.copyfile(KIT.parent/'review/after/dungeon.png',KIT/'reference/PreviousUnity.png')
brief=Path('C:/Users/USER/.codex/attachments/812ce617-6f8a-4ec9-a54a-eae0db515ddc/Pasted text.txt')
if brief.exists():shutil.copyfile(brief,KIT/'reference/Request.txt')
else:assert (KIT/'reference/Request.txt').exists(),'The saved task brief is missing.'
previous=json.loads((KIT.parent/'provenance.json').read_text())
verified=[]
for item in previous['references']:
    if item['path'].startswith(('Docs/ArtDirection/','Assets/_Game/Art/CombatIdles/')):
        p=ROOT/item['path'];actual=sha(p)
        assert actual==item['sha256'],str(p)+' changed; reread current authority'
        verified.append(dict(path=item['path'],sha256=actual))
protected=json.loads((KIT.parent/'native-checks.json').read_text())['preservedLocalSha256']
for p,expected in protected.items():assert sha(ROOT/p)==expected,p
im=Image.open(source);assert im.size==(941,1672)
data=dict(status='Dungeon-only art review; no integration; not approved',reference=dict(path='reference/ApprovedDungeon.png',sha256=sha(source),size=im.size),
    startCommit=subprocess.check_output(['rtk','git','rev-parse','HEAD'],cwd=ROOT,text=True).strip(),
    nativeGrid=dict(size=[470,836],method='One consistent 2x source sampling grid: source(2*x+1,2*y+1). Rightmost source column omitted. No per-part resizing.',sourceNativeSizeClaim=False),
    verifiedAuthority=verified,protectedLocalFiles=protected,providerCalls=0)
(KIT/'provenance.json').write_text(json.dumps(data,indent=2)+'\n')
print('Verified current guide, register, six golden/production images and five protected changes. Target: 941x1672; working grid: 470x836.')
