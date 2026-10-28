from pathlib import Path
import urllib.request,json,sys
from PIL import Image,ImageDraw
R=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/IronveinBelowTopHUD/Revision01')
urls={'FloorTileset.png':'https://api.pixellab.ai/mcp/sidescroller-tilesets/01040804-df87-4e71-9311-743283373428/image','FloorMetadata.json':'https://api.pixellab.ai/mcp/sidescroller-tilesets/01040804-df87-4e71-9311-743283373428/metadata'}
if len(sys.argv)>1 and sys.argv[1]=='crystals':
 urls={f'Crystal-{i:02}.png':f'https://api.pixellab.ai/mcp/images/1bce3ffe-d8af-4bd9-9047-0a93ad62975b/download?index={i}' for i in range(16)}
elif len(sys.argv)>1:
 urls={n.replace('Floor','FloorPro'):u.replace('01040804-df87-4e71-9311-743283373428','c0054523-2015-4f7c-8775-6c0c5b91304f') for n,u in urls.items()}
for n,u in urls.items():
 p=R/'Raw'/n
 if not p.exists():
  with urllib.request.urlopen(urllib.request.Request(u,headers={'User-Agent':'Mozilla/5.0'}),timeout=90) as r:p.write_bytes(r.read())
 print(n,p.stat().st_size)
if len(sys.argv)>1 and sys.argv[1]=='crystals':
 board=Image.new('RGB',(560,640),'#202331');d=ImageDraw.Draw(board)
 for i in range(16):
  im=Image.open(R/'Raw'/f'Crystal-{i:02}.png').convert('RGBA');x=(i%4)*140;y=(i//4)*160
  board.paste(im.resize((112,128),Image.Resampling.NEAREST),(x+14,y+22),im.resize((112,128),Image.Resampling.NEAREST));d.text((x+10,y+4),str(i),fill='white')
 board.save(R/'Preview/CrystalCandidates.png')
else:
 n='FloorPro' if len(sys.argv)>1 else 'Floor'
 im=Image.open(R/f'Raw/{n}Tileset.png');print('Tileset',im.size);im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST).save(R/f'Preview/{n}Raw3x.png');print([(t['name'],t['bounding_box']) for t in json.loads((R/f'Raw/{n}Metadata.json').read_text())['tileset_data']['tiles']])
