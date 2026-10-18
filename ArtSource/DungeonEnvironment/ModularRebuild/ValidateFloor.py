from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,struct,zlib
K=Path(__file__).resolve().parent;P=K.parents[2]
im=[Image.open(K/f'Exports/FloorTiles/Floor{x}.png').convert('RGBA') for x in 'ABCD']
def decode(p):
 b=p.read_bytes();w,h=struct.unpack_from('<HH',b,8);i=144
 while i<len(b):
  size,kind=struct.unpack_from('<IH',b,i);s=i+6
  if kind==0x2005:
   x,y=struct.unpack_from('<hh',b,s+2);ctype=struct.unpack_from('<H',b,s+7)[0];cw,ch=struct.unpack_from('<HH',b,s+16);raw=b[s+20:i+size];raw=zlib.decompress(raw) if ctype==2 else raw
   out=Image.new('RGBA',(w,h));out.paste(Image.frombytes('RGBA',(cw,ch),raw),(x,y));return out
  i+=size
for x,a in zip('ABCD',im):
 assert a.size==(64,64) and set(a.getchannel('A').get_flattened_data())=={255}
 assert a.tobytes()==decode(K/f'Editable/FloorTiles/Floor{x}.aseprite').tobytes()
 for b in im:
  assert a.crop((0,0,1,64)).tobytes()==b.crop((63,0,64,64)).tobytes()
  assert a.crop((0,0,64,1)).tobytes()==b.crop((0,63,64,64)).tobytes()
grid=Image.new('RGBA',(256,256))
for y in range(4):
 for x in range(4):grid.paste(im[(x+y*3)%4],(x*64,y*64))
board=Image.new('RGB',(1050,690),'#242430');d=ImageDraw.Draw(board)
src=Image.open(P/'Assets/_Game/Resources/UI/BattlePresentation/ReferenceDungeon/BattleFloor.png').convert('RGBA').crop((64,34,128,98))
for i,(name,a) in enumerate([('Native source crop',src)]+list(zip('ABCD',im))):
 x=10+i*205;d.text((x,8),name,fill='white');board.paste(a,(x,30));board.paste(a.resize((192,192),Image.Resampling.NEAREST),(x,110))
d.text((10,320),'Alternating variants, 2x (all 16 pairings pass exact edge-strip compatibility)',fill='white')
board.paste(grid.resize((512,512),Image.Resampling.NEAREST),(10,345))
board.crop((0,0,1050,690)).save(K/'Review/FloorReview.png');grid.save(K/'Review/FloorRepeat.png')
record={'phase':'Floor','dimensions':[64,64],'variants':4,'binary_alpha':True,'ase_png_exact':True,'all_neighbor_edge_pairs':'16 horizontal and 16 vertical passed','colors':[len(set(a.get_flattened_data())) for a in im],'shape_changes':'Only crop boundary repair bands (4px left/right; 3px top; 1px bottom). Interior wear stays within source material planes.','visual_status':'Source-close native floor candidate passed internal 1x/3x and mixed-repeat review; in-scene check pending','source_rect':[64,34,64,64],'cyclic_offsets_x':[0,16,32,48]}
(K/'Evidence/FloorChecks.json').write_text(json.dumps(record,indent=2)+'\n');print(json.dumps(record))
