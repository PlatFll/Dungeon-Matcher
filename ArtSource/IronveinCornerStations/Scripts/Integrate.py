from pathlib import Path
import shutil,json,re,uuid,hashlib
from PIL import Image
F=Path(__file__).resolve().parents[3];root=F/'ArtSource/IronveinCornerStations'
dest=F/'Assets/_Game/Art/IronveinCornerStations';dest.mkdir(exist_ok=True)
template=(F/'Assets/_Game/Art/IronveinBelowTopHUD/Revision01/CrystalLeft.png.meta').read_text()
template=re.sub(r'  internalIDToNameTable:.*?  externalObjects:', '  internalIDToNameTable: []\n  externalObjects:',template,flags=re.S)
template=re.sub(r'  spriteSheet:.*?  mipmapLimitGroupName:', '''  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {}
  mipmapLimitGroupName:''',template,flags=re.S)
template=template.replace('spritePixelsToUnits: 100','spritePixelsToUnits: 64')
guid={}
for p in (root/'Exports').glob('*.png'):
 shutil.copyfile(p,dest/p.name);meta=dest/(p.name+'.meta')
 guid[p.stem]=re.search(r'^guid: (\w+)',meta.read_text(),re.M)[1] if meta.exists() else uuid.uuid4().hex
 meta.write_text(re.sub(r'^guid: \w+','guid: '+guid[p.stem],template,flags=re.M),encoding='utf-8')
placements=[('FoundationLeft',0,610),('FoundationRight',296,609),('RailSleepers',0,604),('TimberSupportExtended',312,534),('OreCrate',0,563),('SupplyCrate',304,597),('Pickaxe',326,584),('OreFragments',34,610),('EmbeddedOre',312,610)]
baseline=(root/'QA/IronveinAssemblyBefore.asset.txt').read_text()
parts=re.split(r'(?=^  - name: )',baseline,flags=re.M)
removed=[];kept=[]
for block in parts:
 if '\n    region: IronveinLowerModules\n' in block or '\n    region: Revision01_LowerCrystals\n' in block: removed.append(block.splitlines()[0])
 else: kept.append(block)
assert len(removed)==5
for name,x,y in placements:
 w,h=Image.open(root/'Exports'/f'{name}.png').size
 kept.append(f'''  - name: IronveinLowerModules/CornerStation_{name}
    region: IronveinLowerModules
    sprite: {{fileID: 21300000, guid: {guid[name]}, type: 3}}
    rect:
      serializedVersion: 2
      x: {x}
      y: {y}
      width: {w}
      height: {h}
    type: 0
    color: {{r: 1, g: 1, b: 1, a: 1}}
    reflection: {{x: 1, y: 1}}
''')
(F/'Assets/_Game/Resources/UI/BattlePresentation/IronveinAssembly.asset').write_text(''.join(kept),encoding='utf-8')
(root/'QA/PlacementChanges.json').write_text(json.dumps(dict(removed=removed,added=placements,protectedPieces=len(parts)-1-len(removed),runtimeCodeChanged=False),indent=2),encoding='utf-8')
print('Imported',len(guid),'native sprites; replaced only',len(removed),'corner placements; retained',len(parts)-1-len(removed),'other placements.')
