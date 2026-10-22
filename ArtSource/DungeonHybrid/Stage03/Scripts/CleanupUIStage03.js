// Native LibreSprite pixel editing. Stage 03 copies only; never resize approved art.
var R='ArtSource/DungeonHybrid/Stage03/', col=app.pixelColor;
function rgba(h){return col.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),255);}
function blank(w,h){return {w:w,h:h,p:[]};}
function set(a,x,y,v){if(x<0||y<0||x>=a.w||y>=a.h)throw Error('Outside piece');a.p[y*a.w+x]=typeof v==='string'?rgba(v):v;}
function read(n){var d=app.open(R+'Sources/'+n+'_Raw.png'),s=d.sprite,im=s.layer(0).cel(0).image,a=blank(s.width,s.height);for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++){var v=im.getPixel(x,y);set(a,x,y,col.rgbaA(v)?v:0);}return a;}
var raw={Wave:read('Wave'),Control:read('Controls'),Resonance:read('Resonance')};
function save(a,n){var d=app.open(R+'Sources/Wave_Raw.png'),s=d.sprite,im=s.layer(0).cel(0).image;if(s.width!==128||s.height!==64)throw Error('Wrong source');for(var y=0;y<64;y++)for(var x=0;x<128;x++)im.putPixel(x,y,x<a.w&&y<a.h?(a.p[y*a.w+x]||0):0);s.layer(0).name=n;s.commit();s.saveAs('.utmp/hybrid-stage03/'+n+'.png',true);}
// Resolve irregular source corner widths into upright 9px sockets. Keep the
// source's distinct jewel (wave) or brass rivet (controls/resonance) center.
function corner(f){var a=blank(9,9),r=raw[f],ox=f==='Wave'?13:f==='Control'?4:9,oy=f==='Wave'?13:f==='Control'?4:8;
 for(var y=0;y<9;y++)for(var x=0;x<9;x++){
  if((x===0||x===8)&&(y===0||y===8))continue;
  var v=r.p[(oy+y)*r.w+ox+x];
  if(x===0||y===0||x===8||y===8)v=rgba('0a0d11');
  else if(y===1||x===1)v=rgba('b5a9bc');
  else if(x===7||y===7)v=rgba('67537d');
  else if(x===2||y===2)v=rgba('e0d8e3');
  else if(x===6||y===6)v=rgba('4e3d6c');
  else v=rgba('83718d');
  if(x>=3&&x<=5&&y>=3&&y<=5)v=rgba('302940');
  if(x===4&&y===4)v=rgba(f==='Wave'?'c22bea':'b07a43');
  set(a,x,y,v);
 }return a;}
function rails(f){var a=blank(16,5),b=blank(5,16),profile=['0a0d11','b5a9bc','83718d','4e3d6c','0a0d11'];
 for(var y=0;y<5;y++)for(var x=0;x<16;x++){set(a,x,y,profile[y]);set(b,y,x,profile[y]);}
 save(a,f+'_Rail_Top');save(a,f+'_Rail_Bottom');save(b,f+'_Rail_Left');save(b,f+'_Rail_Right');}
function clamp(f){var n=f==='Resonance'?7:5,a=blank(n,n),m=(n-1)/2;for(var y=0;y<n;y++)for(var x=0;x<n;x++){
 if(f==='Resonance'&&Math.abs(x-m)+Math.abs(y-m)>m)continue;
 if(f!=='Resonance'&&(x===0||x===n-1)&&(y===0||y===n-1))continue;
 var edge=f==='Resonance'?Math.abs(x-m)+Math.abs(y-m)===m:x===0||y===0||x===n-1||y===n-1;
 var c=edge?'0a0d11':x+y<n-1?'b5a9bc':'67537d';if(x===m&&y===m)c=f==='Control'?'b07a43':'c22bea';
 if(f==='Resonance'&&!edge&&x+y>n-1)c='68168a';set(a,x,y,c);
 }return a;}
['Wave','Control','Resonance'].forEach(function(f){['TL','TR','BL','BR'].forEach(function(q){save(corner(f),f+'_Corner_'+q);});rails(f);save(clamp(f),f+'_Clamp');var fill=blank(16,16);for(var y=0;y<16;y++)for(var x=0;x<16;x++)set(fill,x,y,'151423');save(fill,f+'_Inset');});
// The generated wave skull is preserved at source resolution, with a clean
// dark contour replacing its transparent dark-key artefact. No facial redesign.
var skull=blank(15,18),src=raw.Wave;
for(var y=0;y<18;y++)for(var x=0;x<15;x++){var v=src.p[(40+y)*128+56+x];if(col.rgbaA(v))set(skull,x,y,v);}
// Remove the source bottom-rail fragment from this independent hanging piece.
for(var y=0;y<4;y++)for(var x=0;x<15;x++)set(skull,x,y,x===7?'0a0d11':x===8?'67537d':0);
save(skull,'Wave_Skull');
// Complete compact side ornaments retain the raw design's hooked metal or
// violet lozenge, with paired geometry and no noisy separated specks.
function wing(f){var a=blank(7,15),rows=f==='Wave'?['...##..','..###..','..#.#..','.##.#..','##..#..','#.###..','##..###','.######','##..###','#.###..','##..#..','.##.#..','..#.#..','..###..','...##..']:['.....#.','....##.','...###.','..####.','.#####.','######.','#######','#######','#######','######.','.#####.','..####.','...###.','....##.','.....#.'];
 for(var y=0;y<15;y++)for(var x=0;x<7;x++)if(rows[y][x]==='#'){
 var c='0a0d11';if(x>0&&x<6&&y>0&&y<14&&rows[y][Math.max(0,x-1)]==='#')c=y<6?'b5a9bc':'67537d';
 if(f==='Resonance'&&x>=3&&x<=5&&y>=5&&y<=9)c=x===3&&y<=6?'c22bea':'68168a';set(a,x,y,c);
 }return a;}
['Wave','Resonance'].forEach(function(f){var a=wing(f);save(a,f+'_Wing_Left');var b=blank(a.w,a.h);for(var y=0;y<a.h;y++)for(var x=0;x<a.w;x++)set(b,x,y,a.p[y*a.w+a.w-1-x]||0);save(b,f+'_Wing_Right');});
