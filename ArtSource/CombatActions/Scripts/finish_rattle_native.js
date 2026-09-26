// Correct the source's shared gray/gold indices only inside the skull material.
// Native LibreSprite pixels and original frame durations are preserved.
app.open('C:/UnityProjects/Dungeon Matcher/ArtSource/CombatActions/Rattlebones_Ability.aseprite');
var s=app.activeSprite;
function hex(a,i){return ('0'+a[i].toString(16)).slice(-2)+('0'+a[i+1].toString(16)).slice(-2)+('0'+a[i+2].toString(16)).slice(-2);}
for(var f=1;f<9;f++){
 var c=s.layer(0).cel(f),im=c.image,b=im.getImageData(),seen={},queue=[[30,20]],count=0;
 while(queue.length){var p=queue.pop(),x=p[0],y=p[1],key=y*64+x;
  if(seen[key]||x<c.x||y<c.y||x>=c.x+im.width||y>=c.y+im.height||y<12||y>38)continue;
  seen[key]=true;var i=((y-c.y)*im.width+x-c.x)*4,color=hex(b,i);
  if(!b[i+3]||'8d725c b7a393 fdf5e5 fefbef 8a5622 c58826 fbea90'.indexOf(color)<0)continue;
  if('8a5622 c58826 fbea90'.indexOf(color)>=0){b[i]=183;b[i+1]=163;b[i+2]=147;count++;}
  queue.push([x-1,y],[x+1,y],[x,y-1],[x,y+1]);
 }
 im.putImageData(b);console.log('Skull material corrected frame '+(f+1)+': '+count+' pixels');
}
s.commit();s.save();app.command.GotoFirstFrame();
