from pathlib import Path
from PIL import Image,ImageDraw
r=Path(__file__).resolve().parent
sheet=Image.new('RGB',(920,650),(120,120,120));d=ImageDraw.Draw(sheet)
specs=[('breakwater_captain',(20,8,38,24)),('conch_marshal',(8,9,26,25)),('shellback_porter',(8,9,26,25)),('needlefin_skirmisher',(8,9,26,25)),('queen_nacre',(23,28,41,44)),('lantern_warden',(20,32,38,48))]
for i,(name,box) in enumerate(specs):
 im=Image.open(r/'Raw'/(name+'.png')).convert('RGBA');x=(i%3)*305;y=(i//3)*320
 d.text((x,y),name,fill='white');v=im.crop(box).resize((252,224),Image.Resampling.NEAREST);sheet.paste(v,(x+25,y+35),v)
 for n in range(18):d.text((x+25+n*14,y+20),str(box[0]+n),fill='white')
 for n in range(16):d.text((x,y+35+n*14),str(box[1]+n),fill='white')
sheet.save(r/'EyeInspection.png')
