from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/Stage04'
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',14);small=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12)
names=['Torch','Chain','Banner','Barrel','Jar','Bones','Rubble']
board=Image.new('RGB',(7*160,340),'#171923');d=ImageDraw.Draw(board)
for j,n in enumerate(names):
 raw=Image.open(R/'Sources'/f'{n}_Raw.png').convert('RGBA');clean=Image.open(R/'Previews'/f'{n}_Clean64.png').convert('RGBA')
 x=j*160+16;d.text((x,12),n,font=font,fill='#eee8f1')
 d.text((x,36),'PixelLab / native',font=small,fill='#a6a0b1');board.paste(raw,(x,52),raw)
 d.text((x,122),'LibreSprite / 2x',font=small,fill='#b9a4d5');zoom=clean.resize((128,128),Image.Resampling.NEAREST);board.paste(zoom,(x,140),zoom)
 a=len({v[:3] for v in raw.get_flattened_data() if v[3]});b=len({v[:3] for v in clean.get_flattened_data() if v[3]})
 d.text((x,280),f'{a} -> {b} colors',font=font,fill='#ddd2bc');d.text((x,303),'Shape preserved',font=small,fill='#b5c8ad')
board.save(R/'Previews/Palette-review.png')
# Native assets catalog: thumbnails shown at actual source dimensions.
mods=json.loads((R/'Modules.json').read_text())['parts'];out=Image.new('RGB',(720,260),'#171923');d=ImageDraw.Draw(out)
for j,m in enumerate(mods):
 x=(j%5)*144+10;y=(j//5)*125+10;im=Image.open(R/'Exports'/f"{m['name']}.png").convert('RGBA')
 d.text((x,y),m['name'],font=small,fill='#eee8f1');out.paste(im,(x+40,y+22),im)
 d.text((x,y+94),f"{m['width']}x{m['height']}  {m['colors']} colors",font=small,fill='#ada3bf')
out.save(R/'Previews/Native-assets.png')
