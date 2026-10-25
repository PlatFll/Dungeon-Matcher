var root='C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2/', cleanup=[{"name": "ColumnLeftA", "source": "Exports/Revision01/ColumnLeftA.png", "oldAsset": "Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/ColumnLeftA.png", "patches": [[22, 78, [112, 65, 63, 255], [37, 137, 153, 255]]], "width": 31, "height": 96}, {"name": "ColumnLeftB", "source": "Exports/Revision01/ColumnLeftB.png", "oldAsset": "Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/ColumnLeftB.png", "patches": [], "width": 31, "height": 96}, {"name": "ColumnRightA", "source": "Exports/Revision01/ColumnRightA.png", "oldAsset": "Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/ColumnRightA.png", "patches": [], "width": 31, "height": 96}, {"name": "ColumnRightB", "source": "Exports/Revision01/ColumnRightB.png", "oldAsset": "Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/ColumnRightB.png", "patches": [], "width": 31, "height": 96}, {"name": "S07_LeftReef", "source": "Exports/Revision01/S07_LeftReef.png", "oldAsset": "Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/S07_LeftReef.png", "patches": [[26, 39, [47, 32, 61, 255], [58, 163, 174, 255]], [25, 40, [47, 32, 61, 255], [58, 163, 174, 255]], [26, 38, [47, 32, 61, 255], [10, 13, 17, 255]], [20, 44, [47, 32, 61, 255], [10, 13, 17, 255]], [16, 74, [47, 32, 61, 255], [150, 142, 113, 255]], [13, 71, [47, 32, 61, 255], [150, 142, 113, 255]], [12, 72, [47, 32, 61, 255], [10, 13, 17, 255]], [27, 44, [47, 32, 61, 255], [150, 142, 113, 255]], [26, 45, [47, 32, 61, 255], [40, 72, 59, 255]], [25, 62, [47, 32, 61, 255], [150, 142, 113, 255]], [22, 40, [47, 32, 61, 255], [101, 193, 190, 255]], [20, 54, [47, 32, 61, 255], [40, 72, 59, 255]]], "width": 80, "height": 96}, {"name": "S07_RightReef", "source": "Exports/Revision01/S07_RightReef.png", "oldAsset": "Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/S07_RightReef.png", "patches": [[62, 35, [47, 32, 61, 255], [0, 0, 0, 0]], [62, 34, [99, 51, 79, 255], [0, 0, 0, 0]], [62, 31, [47, 32, 61, 255], [6, 36, 50, 255]]], "width": 80, "height": 96}];
var pc=app.pixelColor,clear=pc.rgba(0,0,0,0),im,w,h;
function rgb(s){return pc.rgba(parseInt(s.substr(0,2),16),parseInt(s.substr(2,2),16),parseInt(s.substr(4,2),16),255);}
function px(x,y,c){if(x>=0&&y>=0&&x<w&&y<h)im.putPixel(x,y,c);}
function rect(x,y,ww,hh,c){for(var Y=y;Y<y+hh;Y++)for(var X=x;X<x+ww;X++)px(X,Y,c);}
function line(x,y,X,Y,c){var dx=Math.abs(X-x),sx=x<X?1:-1,dy=-Math.abs(Y-y),sy=y<Y?1:-1,e=dx+dy;while(true){px(x,y,c);if(x===X&&y===Y)break;var ee=2*e;if(ee>=dy){e+=dy;x+=sx;}if(ee<=dx){e+=dx;y+=sy;}}}
function save(n){var s=app.activeSprite;s.commit();s.saveAs(root+'Native/Revision02/'+n+'.ase',true);s.saveAs(root+'Exports/Revision02/'+n+'.png',true);console.log('SAVED '+n);app.activeDocument.close();}
function make(n,fn){app.open(root+'Native/Revision02/Work-'+n+'-canvas.png');im=app.activeImage;w=app.activeSprite.width;h=app.activeSprite.height;fn();app.activeSprite.layer(0).name='Native pixel polish - '+n;save(n);}
var wall=['051e2c','082c3c','0b3443','10414f','17535d'].map(rgb),kelp=['153b39','2c5545','50724f'].map(rgb);
function brickwork(portrait,variant){
 rect(0,0,w,h,wall[0]);
 for(var y=0;y<h;y+=16){var shift=(y/16%2)*16;
  for(var x=-shift;x<w;x+=32){var face=((x+shift)/32+(y/16)+variant)%3===0?wall[1]:wall[2];rect(x+1,y+1,30,14,face);rect(x+2,y+1,28,1,wall[3]);rect(x+1,y+2,1,11,wall[3]);rect(x+2,y+14,28,1,wall[1]);px(x+1,y+1,wall[1]);px(x+30,y+14,wall[0]);
   // One quiet bevel break per occasional brick; no speckle texture.
   if(((x+shift)/32+y/16+variant)%4===0){line(x+23,y+1,x+20,y+3,wall[1]);line(x+24,y+2,x+21,y+4,wall[3]);}
  }
 }
 if(portrait){line(49,15,47,18,kelp[0]);line(47,18,48,23,kelp[0]);rect(45,16,3,1,kelp[1]);}
}
function bubble(sz,phase){var cx=Math.floor(w/2),cy=Math.floor(h/2),shade=rgb('287184'),mid=rgb('62afbc'),light=rgb('b3e4dc');
 if(phase===2){px(cx-3,cy-2,mid);px(cx+3,cy-1,shade);px(cx-2,cy+3,shade);px(cx+2,cy+2,mid);return;}
 var r=sz===0?2:3;if(phase===1)r++;
 // Hollow stepped native contours, bright upper-left quarter, no blurred halo.
 for(var dx=-r+1;dx<r;dx++){px(cx+dx,cy-r,mid);px(cx+dx,cy+r,shade);}
 for(var dy=-r+1;dy<r;dy++){px(cx-r,cy+dy,mid);px(cx+r,cy+dy,shade);}
 px(cx-r+1,cy-r+1,light);px(cx-r+2,cy-r,light);
 if(phase===1){px(cx+r,cy,clear);px(cx,cy+r,clear);}
}
try{
 // Stage1: exact, reviewed coral-fleck patches. All other pixels remain unchanged.
 cleanup.forEach(function(j){app.open(root+j.source);im=app.activeImage;w=app.activeSprite.width;h=app.activeSprite.height;j.patches.forEach(function(p){var old=pc.rgba(p[2][0],p[2][1],p[2][2],p[2][3]);if(im.getPixel(p[0],p[1])!==old)throw Error('cleanup source mismatch');im.putPixel(p[0],p[1],pc.rgba(p[3][0],p[3][1],p[3][2],p[3][3]));});save(j.name);});
 // Stage2: native, understated submerged blockwork for the existing overfilled portrait mask.
 make('PortraitWallA',function(){brickwork(true,0);});make('PortraitWallB',function(){brickwork(true,1);});
 // Stage3: continuous quiet masonry tiles, with sparse growth rooted on a mortar course.
 make('OceanMasonry',function(){brickwork(false,0);});
 make('MasonryGrowth',function(){brickwork(false,0);line(11,30,10,24,kelp[0]);line(10,24,13,18,kelp[0]);line(13,18,12,15,kelp[0]);line(12,28,11,24,kelp[1]);line(11,24,14,18,kelp[1]);line(13,20,17,17,kelp[1]);line(10,26,7,23,kelp[1]);px(12,16,kelp[2]);});
 // Stage4: small/medium rise and two-step pop cels.
 for(var i=0;i<3;i++){(function(k){make('BubbleSmall'+k,function(){bubble(0,k);});make('BubbleMedium'+k,function(){bubble(1,k);});})(i);}
 console.log('POLISH02_STATIC_AND_FX_COMPLETE');
}catch(e){console.log('POLISH02_FAILED '+e);}app.command.Exit();
