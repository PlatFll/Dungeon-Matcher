// Reuses ReviewFrames' LibreSprite preview helpers. Native files, not this
// enlarged contact sheet, establish dimensions and alpha.
letters['4']=['10010','10010','10010','11111','00010','00010','00010'];
var icons=['CrownedSkull','Characters','Shop','Mastery','Practice','Challenges','Music','Sfx','Motion','Vibration','Resume','Suspend','Retry','Skull','Gear'];
var contact=canvas(1080,812);
label(contact,'BEFORE X1    AFTER X1    AFTER X4',12,12);
for(var n=0;n<icons.length;n++){
 var x=(n%4)*270,y=Math.floor(n/4)*192+46,name=icons[n];
 label(contact,name.toUpperCase(),x+8,y);
 var old=read(ROOT+'IconRevision/before/native/'+name+'.png'),fresh=read(ROOT+'native/'+name+'.png');
 stamp(contact,old,x+12,y+35);stamp(contact,fresh,x+52,y+35);stamp(contact,fresh,x+112,y+30,fresh.w*4,fresh.h*4);
}
save(contact,'icons-native');
