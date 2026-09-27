import fs from 'node:fs';
const root='ArtSource/GideonGlass/';
const base=fs.readFileSync(root+'AnimateApproved.js','utf8').split('var headDip=[')[0];
const cast=String.raw`
function rect(a,x,y,w,h,c){for(var yy=y;yy<y+h;yy++)for(var xx=x;xx<x+w;xx++)pixel(a,xx,yy,c);}
function castPose(dip,focus,flash,steam){
 var a=pose(dip,dip?1:0,false);
 if(focus){
  rect(a,35,29+dip,4,4,9);
  if(focus===1){rect(a,36,30+dip,2,2,10);pixel(a,36,30+dip,11);}
  if(focus===2){rect(a,36,31+dip,2,1,10);}
  if(focus===3){rect(a,35,30+dip,2,1,1);rect(a,37,31+dip,2,1,1);rect(a,36,31+dip,1,1,0);}
 }
 if(flash){rect(a,35,29+dip,4,4,13);rect(a,33,31+dip,8,1,13);rect(a,37,27+dip,1,8,13);}
 if(steam===1){rect(a,19,34,2,2,12);pixel(a,19,34,13);}
 if(steam===2){rect(a,17,32,3,2,12);pixel(a,17,32,13);}
 if(steam===3){rect(a,15,30,2,1,12);}
 return a;
}
write('Gideon_Cast',[castPose(0,0,0,0),castPose(1,0,0,0),castPose(1,1,0,0),castPose(1,2,0,0),castPose(1,3,0,0),castPose(1,0,1,0),castPose(1,1,0,1),castPose(1,1,0,2),castPose(1,1,0,3),castPose(1,1,0,0)]);
write('Gideon_Hold',[castPose(1,1,0,0)]);
write('Gideon_Recovery',[castPose(1,1,0,0),castPose(2,2,0,0),castPose(1,0,0,0),castPose(0,0,0,0)]);
app.command.Exit();
`;
fs.writeFileSync(root+'AnimateApprovedCast.js',base+cast);
