from pathlib import Path
from PIL import Image
import shutil
root=Path(__file__).resolve().parents[1]
im=Image.open(root/'Exports/TimberSupport.png').convert('RGBA')
print('post row 10',[(x,im.getpixel((x,10))) for x in range(28,44)])
Image.new('RGBA',(48,96)).save(root/'Prepared/TimberSupportExtended.png')
script='var root='+repr(str(root).replace('\\','/')+'/')+''', pc=app.pixelColor;
function rgb(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
try{
 app.open(root+'Native/TimberSupport.ase');var s=app.activeSprite,c=s.layer(0).cel(0),im=c.image,a=[];
 for(var y=0;y<s.height;y++){a[y]=[];for(var x=0;x<s.width;x++)a[y][x]=(x>=c.x&&y>=c.y&&x<c.x+im.width&&y<c.y+im.height)?im.getPixel(x-c.x,y-c.y):0;}
 app.activeDocument.close();app.open(root+'Prepared/TimberSupportExtended.png');s=app.activeSprite;im=s.layer(0).cel(0).image;
 for(var y=0;y<72;y++)for(var x=0;x<48;x++)im.putPixel(x,y+24,a[y][x]);
 // Continue the approved candidate's upright to the existing board-side support.
 // All original brace, footing, steel band and wood pixels remain below this joint.
 for(var y=1;y<34;y++)for(var x=33;x<=40;x++){
  var h=x===33||x===40?'0a0d11':x===34?'7a4d2e':x<38?'6f3b1d':'4a2c1c';im.putPixel(x,y,rgb(h));
 }
 for(var y=3;y<=7;y++)for(var x=31;x<=42;x++)im.putPixel(x,y,rgb(y===3||y===7||x===31||x===42?'0a0d11':y===4?'55606e':'2c293d'));
 im.putPixel(33,5,rgb('93a1b0'));im.putPixel(40,5,rgb('93a1b0'));
 s.layer(0).name='PixelLab brace + native wall anchor extension';s.commit();s.saveAs(root+'Native/TimberSupportExtended.ase',true);s.saveAs(root+'Exports/TimberSupportExtended.png',true);app.activeDocument.close();
 app.open(root+'Native/TimberSupportExtended.ase');app.activeSprite.saveAs(root+'Roundtrip/TimberSupportExtended.png',true);app.activeDocument.close();console.log('CORNER_ANCHOR_PASS');
}catch(e){console.log('CORNER_ANCHOR_FAIL '+e);}app.command.Exit();
'''
(root/'Scripts/JoinSupport.js').write_text(script,encoding='utf-8')
runner=(root/'Scripts/RunLibreSprite.ps1').read_text().replace('Cleanup.js','JoinSupport.js').replace('LibreSprite.log','JoinSupport.log').replace('LibreSprite-error.log','JoinSupport-error.log')
(root/'Scripts/RunJoinSupport.ps1').write_text(runner,encoding='utf-8')
