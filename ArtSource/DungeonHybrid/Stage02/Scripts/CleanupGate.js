// Run inside LibreSprite against ONLY Stage02/Sources/Gate_Raw_PixelLab.png.
// Native palette/cluster cleanup; no resize or silhouette normalization.
var doc=app.open('ArtSource/DungeonHybrid/Stage02/Sources/Gate_Raw_PixelLab.png');
var sprite=doc.sprite, img=sprite.layer(0).cel(0).image, col=app.pixelColor;
if (!sprite || sprite.width!==128 || sprite.height!==128 || img.width!==128 || img.height!==128)
    throw new Error('Expected native 128x128 generated gate');
if (sprite.filename.indexOf('Gate_Raw_PixelLab.png')<0) throw new Error('Wrong target');
function rgb(hex){return col.rgba(parseInt(hex.substr(0,2),16),parseInt(hex.substr(2,2),16),parseInt(hex.substr(4,2),16),255);}
var stone={'35324f':'302940','67537d':'4e3d6c','83718d':'67537d','b7a393':'83718d','55606e':'67537d'};
var iron={'35324f':'241f3a','55606e':'35324f','b7a393':'55606e','83718d':'55606e','67537d':'35324f'};
function key(c){return ('0'+col.rgbaR(c).toString(16)).slice(-2)+('0'+col.rgbaG(c).toString(16)).slice(-2)+('0'+col.rgbaB(c).toString(16)).slice(-2);}
for(var y=0;y<128;y++)for(var x=0;x<128;x++){
    var c=img.getPixel(x,y);
    if(col.rgbaA(c)===0){img.putPixel(x,y,0);continue;}
    var map=(y<32||x<32||x>=89)?stone:iron;
    var k=key(c);if(map[k])img.putPixel(x,y,rgb(map[k]));
}
// Small, connected skull reliefs replace the source's fragmented side glyphs.
var face=['..#######..','.#########.','###########','###########','##..###..##','##..###..##','.####.####.','..#######..','...#.#.#...','...#.#.#...'];
function panel(left){
    for(var yy=67;yy<=85;yy++)for(var xx=left;xx<=left+14;xx++){
        var c='302940';
        if(xx===left||yy===67)c='67537d';
        else if(xx===left+14||yy===85)c='382d55';
        else if(xx>=left+2&&xx<=left+12&&yy>=69&&yy<=83)c='4e3d6c';
        img.putPixel(xx,yy,rgb(c));
    }
    for(var yy=0;yy<face.length;yy++)for(var xx=0;xx<11;xx++){
        if(face[yy].charAt(xx)==='#')img.putPixel(left+2+xx,71+yy,rgb(yy<4?'83718d':'67537d'));
        else if(yy>=4&&yy<=6&&xx>1&&xx<9)img.putPixel(left+2+xx,71+yy,rgb('241f3a'));
    }
}
panel(12);panel(94);
// Carry the dark interior behind the bottom of the ironwork; never expose wall
// strips between the bar tips. This changes only the internal doorway opening.
for(var yy=112;yy<=117;yy++)for(var xx=28;xx<=91;xx++)
    if(col.rgbaA(img.getPixel(xx,yy))===0)img.putPixel(xx,yy,rgb('10131f'));
sprite.commit();
sprite.saveAs('ArtSource/DungeonHybrid/Stage02/Native/Gate_Clean.aseprite',true);
sprite.saveAs('ArtSource/DungeonHybrid/Stage02/Exports/Gate_Clean.png',true);
