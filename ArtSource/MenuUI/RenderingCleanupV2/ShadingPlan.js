// Local tone consolidation only. Keep the original large shadow masses.
var ROOT='__ROOT__',STAGE='__STAGE__',patches=[];
function tone(name,points,from,to){patches.push({name:name,points:points,color:to,allowed:from});}
function band(name,x,y,w,h,from,to){tone(name,[[x,y],[x+w,y],[x+w,y+h],[x,y+h]],from,to);}
// Pilot: one pier, a stair face, and a small architectural face.
tone('Pier: consolidate warm mottling',[[4,347],[14,347],[17,351],[16,361],[14,364],[4,362]],['854d45','772f49','4a254a'],'5f2b43');
tone('Stair: remove inner stain',[[27,350],[34,350],[34,354],[31,356],[27,355]],['1f123c'],'2b1c4d');
tone('Stair: quiet ragged upper shadow',[[65,349],[72,349],[74,352],[69,352],[66,351]],['1f123c'],'2b1c4d');
band('Stair: consolidate inner bevel',24,346,85,2,['593479','6d48a2'],'3e2462');
tone('Pedestal: consolidate face highlights',[[92,307],[100,307],[100,310],[98,313],[93,314],[92,312]],['3e2462'],'2b1c4d');
if(STAGE==='native'){
 // Selected redundant secondary highlight bands. The main lit edges, black
 // joints and their neighboring pixels are protected by the editor engine.
 band('Left riser A: secondary sheen',27,362,97,2,['3e2462','593479'],'2b1c4d');
 band('Left riser B: inner bevel',28,377,91,2,['593479'],'3e2462');
 band('Left riser C: broken highlight strips',34,393,80,3,['593479','6d48a2'],'3e2462');
 band('Left riser D: secondary sheen',13,413,93,3,['3e2462','593479'],'2b1c4d');
 band('Right riser A: inner bevel',231,346,102,2,['593479','6d48a2'],'3e2462');
 band('Right riser B: secondary sheen',240,362,89,2,['3e2462','593479'],'2b1c4d');
 band('Right riser C: inner bevel',244,377,78,2,['593479'],'3e2462');
 band('Right riser D: broken highlight strips',252,393,73,3,['593479','6d48a2'],'3e2462');
 band('Right riser E: secondary sheen',258,413,86,3,['3e2462','593479'],'2b1c4d');
 // Floor face interiors, individually bounded by the existing perspective.
 tone('Left pavers: redundant lower bevel',[[7,433],[108,433],[108,436],[4,436]],['3e2462','593479'],'2b1c4d');
 tone('Right pavers: redundant lower bevel',[[250,433],[353,433],[356,436],[250,436]],['3e2462','593479'],'2b1c4d');
 tone('Left pavers: connected lit plane',[[2,456],[103,456],[103,460],[2,460]],['593479'],'3e2462');
 tone('Right pavers: connected lit plane',[[256,456],[357,456],[357,460],[256,460]],['593479'],'3e2462');
 tone('Left pavers: interior scratches',[[5,462],[20,462],[20,470],[5,467]],['3e2462'],'2b1c4d');
 tone('Left pavers: interior mottling',[[33,461],[59,461],[58,468],[34,468]],['2b1c4d'],'1f123c');
 tone('Right pavers: interior mottling',[[303,462],[327,462],[326,468],[303,468]],['2b1c4d'],'1f123c');
 tone('Left pavers: secondary reflected band',[[3,483],[103,483],[103,486],[2,486]],['3e2462','593479'],'2b1c4d');
 tone('Right pavers: secondary reflected band',[[255,483],[357,483],[357,486],[255,486]],['3e2462','593479'],'2b1c4d');
 tone('Left paver: disconnected wear',[[67,487],[96,487],[94,495],[67,495]],['3e2462'],'2b1c4d');
 tone('Right paver: disconnected wear',[[271,487],[298,487],[298,496],[271,496]],['3e2462'],'2b1c4d');
 tone('Left pavers: lower edge fragments',[[15,503],[95,503],[94,507],[15,507]],['3e2462','593479'],'2b1c4d');
 tone('Right pavers: lower edge fragments',[[264,503],[344,503],[345,507],[264,507]],['3e2462','593479'],'2b1c4d');
 tone('Left paver: secondary internal glints',[[66,508],[94,508],[93,516],[64,516]],['3e2462'],'2b1c4d');
 tone('Right paver: secondary internal glints',[[265,509],[290,509],[290,517],[268,517]],['3e2462'],'2b1c4d');
 tone('Left lower paver: interior chips',[[20,530],[45,530],[43,535],[26,537]],['3e2462'],'2b1c4d');
 tone('Right lower paver: interior chips',[[310,530],[335,530],[332,537],[315,536]],['3e2462'],'2b1c4d');
 // Small architecture refinements, retaining the original major shadow shapes.
 band('Arch: consolidate middle highlight',110,163,6,9,['593479'],'3e2462');
 band('Arch: consolidate lower highlight',108,189,7,9,['593479'],'3e2462');
 tone('Right pier: interior chips',[[328,374],[335,374],[335,385],[329,385]],['593479'],'3e2462');
}
