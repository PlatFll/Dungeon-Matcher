from pathlib import Path
import json,sys,colorsys,shutil
from PIL import Image
F=Path(__file__).resolve().parents[3];R=F/'ArtSource/IronveinStaged';W=Path(__file__).parent
cfg=json.loads((W/'Selections.json').read_text())
jobs=[]
for name,c in cfg.items():
 im=Image.open(R/'Raw'/(c['source']+'.png')).convert('RGBA')
 if 'crop' in c:im=im.crop(c['crop'])
 if c.get('trim'):im=im.crop(im.getbbox())
 if c.get('flip'):im=im.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
 if c.get('rotate'):im=im.transpose(Image.Transpose.ROTATE_90)
 if c.get('pad'):
  p=c['pad'];n=Image.new('RGBA',(im.width+2*p,im.height+2*p));n.paste(im,(p,p));im=n
 im.save(R/'Prepared'/f'{name}.png')
 mapping={}
 for r,g,b,a in set(im.get_flattened_data()):
  if not a:continue
  key=f'{r:02x}{g:02x}{b:02x}';l=.2126*r+.7152*g+.0722*b
  if c.get('ramp'):
   ramp=c['ramp'];idx=sum(l>v for v in c.get('thresholds',[35,65,95,130,180]));mapping[key]=ramp[min(idx,len(ramp)-1)]
  else:mapping[key]=c.get('map',{}).get(key,key)
 jobs.append(dict(name=name,width=im.width,height=im.height,map=mapping,seam=c.get('seam',''),light=c.get('light',False),source=c['source'],stage=c['stage'],custom=c.get('custom',''),despeck=c.get('despeck',False),note=c.get('note','native crop and restrained material cleanup')))
(R/'Scripts/Selections.json').write_text(json.dumps(cfg,indent=2))
(R/'Scripts/CleanupManifest.json').write_text(json.dumps(jobs,indent=2))
script='var root='+json.dumps(str(R).replace('\\','/')+'/')+',jobs='+json.dumps(jobs)+''',pc=app.pixelColor;
function col(h){return pc.rgba(parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16),255);}
try{jobs.forEach(function(j){app.open(root+'Prepared/'+j.name+'.png');var s=app.activeSprite,im=app.activeImage,m={};for(var h in j.map)m[col(h)]=col(j.map[h]);
for(var y=0;y<s.height;y++)for(var x=0;x<s.width;x++){var c=im.getPixel(x,y);if(pc.rgbaA(c)===0){im.putPixel(x,y,0);continue;}if(pc.rgbaA(c)<128){im.putPixel(x,y,0);continue;}var rgb=pc.rgba(pc.rgbaR(c),pc.rgbaG(c),pc.rgbaB(c),255);im.putPixel(x,y,m[rgb]===undefined?rgb:m[rgb]);}
if(j.seam.indexOf('x')>=0)for(var y=0;y<s.height;y++)im.putPixel(s.width-1,y,im.getPixel(0,y));
if(j.seam.indexOf('y')>=0)for(var x=0;x<s.width;x++)im.putPixel(x,s.height-1,im.getPixel(x,0));
if(j.despeck){var edits=[];for(var y=1;y<s.height-1;y++)for(var x=1;x<s.width-1;x++){var v=im.getPixel(x,y),count={},best=v,mx=0;if(pc.rgbaA(v)!==255)continue;for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){if(!dx&&!dy)continue;var z=im.getPixel(x+dx,y+dy);count[z]=(count[z]||0)+1;if(count[z]>mx&&pc.rgbaA(z)===255){mx=count[z];best=z;}}if((count[v]||0)<2&&mx>=5)edits.push([x,y,best]);}edits.forEach(function(e){im.putPixel(e[0],e[1],e[2]);});}
if(j.light)for(var y=57;y<119;y++)for(var x=0;x<s.width;x++){var d=Math.min(Math.abs(x-19)+Math.abs(y-88)*.8,Math.abs(x-230)+Math.abs(y-88)*.8);if(d>22)continue;var c=im.getPixel(x,y),v=(pc.rgbaR(c)+pc.rgbaG(c)+pc.rgbaB(c))/3;if(v>24&&v<110){var h=v<38?'30202a':v<61?(d<12?'4a2c1c':'45313a'):(d<12?'7a4d2e':'5a3c3c');im.putPixel(x,y,col(h));}}

if(j.custom){
 function px(x,y,h){im.putPixel(x,y,col(h));}
 function box(x1,y1,x2,y2,h){for(var yy=y1;yy<=y2;yy++)for(var xx=x1;xx<=x2;xx++)px(xx,yy,h);}
 if(j.custom==='portrait_corner'||j.custom==='portrait_joint'){
  var z=s.width;box(0,0,z-1,z-1,'0a0d11');box(1,1,z-2,z-2,'2b313a');box(1,1,z-2,1,'93a1b0');box(1,1,1,z-2,'55606e');box(2,2,z-3,z-3,'42374f');box(2,2,z-3,2,'786477');
  var q=Math.floor(z/2);box(q-2,q-2,q+2,q+2,'0a0d11');box(q-1,q-1,q+1,q+1,'8a5622');box(q-1,q-1,q+1,q,'c58826');px(q-1,q-1,'fbea90');
 }else if(j.custom.indexOf('board_rail')===0){var h=j.custom==='board_rail_h',r=['0a0d11','93a1b0','55606e','42374f','42374f','2b313a','7a4d2e','4a2c1c','0a0d11'];for(var yy=0;yy<s.height;yy++)for(var xx=0;xx<s.width;xx++)px(xx,yy,r[h?yy:xx]);}else{var h=j.custom==='portrait_rail_h',r=['0a0d11','786477','5a4964','42374f','42374f','2c293d','0a0d11'];for(var yy=0;yy<s.height;yy++)for(var xx=0;xx<s.width;xx++)px(xx,yy,r[h?yy:xx]);}
}
s.layer(0).name='Native cleanup: '+j.note;s.commit();s.saveAs(root+'Native/'+j.name+'.ase',true);s.saveAs(root+'Exports/'+j.name+'.png',true);app.activeDocument.close();console.log('CLEANED '+j.name);
});console.log('STAGED_CLEANUP_COMPLETE '+jobs.length);}catch(e){console.log('STAGED_CLEANUP_FAILED '+e);}app.command.Exit();'''
(R/'Scripts/NativeCleanup.js').write_text(script)
# Native preparation is reproducible from the stored selections and immutable raw sources.
print('Prepared',len(jobs),'native documents')
