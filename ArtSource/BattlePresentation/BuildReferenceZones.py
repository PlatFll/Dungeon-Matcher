"""Prepare native LibreSprite scripts. Pixel authoring runs in LibreSprite."""
import hashlib,json,shutil
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'ArtSource/BattlePresentation'
DUNGEON=BASE/'DungeonReferenceKit'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
history=json.loads((DUNGEON/'provenance.json').read_text())
for ref in history['verifiedAuthority']:assert sha(ROOT/ref['path'])==ref['sha256'],ref['path']
for p,h in history['protectedLocalFiles'].items():assert sha(ROOT/p)==h,p
original=(DUNGEON/'Build.js').read_text()
helpers=original[:original.index('var input=')]
document=original[original.index('function makeDocument'):original.index('function clone')]
for zone,filename in [('Forest','FOREST_ZONE_TARGET.png'),('Court','DROWNED_COURT_TARGET.png')]:
    kit=BASE/(zone+'ReferenceKit')
    for part in ['native','reference','review','logs']:(kit/part).mkdir(parents=True,exist_ok=True)
    shutil.copyfile(BASE/'brief/references'/filename,kit/'reference/Approved.png')
    launcher=(DUNGEON/'Build.ps1').read_text().replace('Dungeon-only native kit saved','Native reference kit saved')
    (kit/'Build.ps1').write_text(launcher)
    (kit/'.gitignore').write_text('logs/\n*.zip\n')
    (kit/'provenance.json').write_text(json.dumps(dict(zone=zone,status='Candidate source kit; awaits visual review',source=filename,sourceSha256=sha(kit/'reference/Approved.png'),verifiedAuthority=history['verifiedAuthority'],protectedLocalFiles=history['protectedLocalFiles'],grid=[470,836],sampling='source(2*x+1,2*y+1); same grid for every module; not a claim of original native resolution',generationCalls=0),indent=2)+'\n')
    setup="""
var ZONE='__ZONE__';
var input=read(ROOT+'reference/Approved.png'),N=blank(470,836);
for(var y=0;y<836;y++)for(var x=0;x<470;x++)for(var k=0;k<4;k++)N.p[(y*470+x)*4+k]=input.p[((y*2+1)*941+x*2+1)*4+k];
var modules=[],repairs=[],composite=blank(470,836);
""".replace('__ZONE__',zone)
    (kit/'Build.js').write_text(helpers+setup+document+(BASE/'ReferenceZones.js').read_text())
print('Prepared Forest and Court scripts; current authority and protected files verified.')
