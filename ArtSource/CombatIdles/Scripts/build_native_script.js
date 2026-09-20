// Builds text-only LibreSprite scripts/palettes. Pixel mutations run inside LibreSprite.
const fs=require('fs'),path=require('path');
const dir=__dirname,root=path.dirname(dir);
const palettes=JSON.parse(fs.readFileSync(path.join(dir,'palettes.json')));
const groups=JSON.parse(fs.readFileSync(path.join(dir,'material_map.json')));
const maps={};
for(const name of Object.keys(palettes)){
  maps[name]={};for(const c of palettes[name]) maps[name][c]=c;
  for(const [to,from] of Object.entries(groups[name])) for(const c of from.split(' ')) maps[name][c]=to;
  fs.writeFileSync(path.join(dir,name+'.gpl'),'GIMP Palette\nName: Dungeon Matcher '+name+'\nColumns: 4\n#\n'+palettes[name].map(c=>c.match(/../g).map(v=>parseInt(v,16)).join(' ')+' '+c).join('\n')+'\n');
}
const measurements=JSON.parse(fs.readFileSync(path.join(root,'Review/source_measurements.json')));
const names={Rattlebones:'RattleBones_FluidIdle',Farmer:'FarmerFluidAnim2',PanVillager:'PanVillager_Idle_Final',Bardley:'SlimeBard_Idle1'};
for(const n of Object.keys(names)) for(const c of Object.keys(measurements[names[n]].colors)) if(!maps[n][c.slice(1)]) throw Error(n+' unmapped '+c);
const preamble='var ROOT = '+JSON.stringify(root.replaceAll('\\','/')+'/')+';\nvar MAPS = '+JSON.stringify(maps)+';\nvar SOURCES = '+JSON.stringify(names)+';\n';
fs.writeFileSync(path.join(dir,'RestoreIdleMaterials.js'),preamble+fs.readFileSync(path.join(dir,'restore_materials_body.js'),'utf8'));
fs.writeFileSync(path.join(dir,'UnifyIdleFamily.js'),'var ROOT = '+JSON.stringify(root.replaceAll('\\','/')+'/')+';\nvar PALETTES = '+JSON.stringify(palettes)+';\n'+fs.readFileSync(path.join(dir,'family_motion_body.js'),'utf8'));
fs.writeFileSync(path.join(dir,'RefineIdleMotion.js'),'var ROOT = '+JSON.stringify(root.replaceAll('\\','/')+'/')+';\n'+fs.readFileSync(path.join(dir,'refine_motion_body.js'),'utf8'));
fs.writeFileSync(path.join(dir,'RestoreFarmerHead.js'),preamble+fs.readFileSync(path.join(dir,'restore_farmer_head_body.js'),'utf8'));
fs.writeFileSync(path.join(dir,'TuneBattleIdles.js'),preamble+fs.readFileSync(path.join(dir,'turn_based_motion_body.js'),'utf8'));
