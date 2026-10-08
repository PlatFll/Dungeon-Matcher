using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Provisional weighted teaching bands. Never rewrites kit balance or native bindings.</summary>
public static class IronveinEncounterImporter
{
    [MenuItem("Dungeon Matcher/Ironvein/Import live encounters")]
    public static void Import()
    {
        EditorUtility.audioMasterMute=true;
        var zone=Resources.Load<ZoneDefinition>("Zones/ironvein-excavation");
        var profile=DrownedCourtImporter.Load<WaveSpawnProfile>("Assets/_Game/Resources/Zones/IronveinEncounterBudget.asset");
        var so=new SerializedObject(profile);
        so.FindProperty("threatBudgetByWave").animationCurveValue=new AnimationCurve(
            new Keyframe(1,3),new Keyframe(4,4),new Keyframe(6,5),new Keyframe(8,7),
            new Keyframe(12,7),new Keyframe(20,8),new Keyframe(28,10),new Keyframe(999,10));
        so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(profile);zone.encounterBudget=profile;
        EnemyDefinition E(string id)=>zone.enemies.Single(e=>e.EnemyId==id);
        var recipes=new List<ZoneTestEncounter>();
        void Add(string label,int first,int weight,params string[] ids)=>recipes.Add(new ZoneTestEncounter{
            label=label,firstLocalWave=first,weight=weight,members=ids.Select(E).ToArray()});
        Add("Rail patrol",1,3,"pickaxe_delver");Add("Rivet lookout",1,2,"rivet_gunner");
        Add("Entrance patrol",1,2,"pickaxe_delver","rivet_gunner");
        Add("Loaded cart",2,2,"packbeetle");Add("Ore escort",3,3,"packbeetle","pickaxe_delver");
        Add("Rivet convoy",3,3,"packbeetle","rivet_gunner");
        Add("Shift change",6,3,"pickaxe_delver","rivet_gunner","packbeetle");
        void Lesson(string id,int first,int deadline,string prerequisite,bool milestone,params string[] escorts)
        {
            foreach(string escort in new[]{""}.Concat(escorts))
            {
                Add(E(id).DisplayName+(escort==""?" alone":" + "+escort),first,milestone?2:3,
                    escort==""?new[]{id}:new[]{id,escort});
                var entry=recipes.Last();entry.introduction=E(id);entry.introduceByLocalWave=deadline;
                entry.oncePerVisit=milestone;entry.requiredSeen=prerequisite==null?new EnemyDefinition[0]:new[]{E(prerequisite)};
            }
        }
        Lesson("stonewright",3,4,null,false,"pickaxe_delver","rivet_gunner");
        Lesson("ore_hauler",5,6,"stonewright",false,"pickaxe_delver","packbeetle");
        Lesson("siege_machinist",8,10,"ore_hauler",true,"rivet_gunner");
        Lesson("vein_surveyor",11,12,"siege_machinist",false,"pickaxe_delver");
        Lesson("powder_sapper",13,14,"vein_surveyor",false,"rivet_gunner");
        Lesson("rail_switcher",15,16,"powder_sapper",false,"packbeetle");
        Lesson("bore_engineer",17,18,"rail_switcher",false,"pickaxe_delver");
        Lesson("seismic_smith",19,20,"bore_engineer",false,"rivet_gunner");
        Lesson("obsidian_sentinel",22,24,"seismic_smith",true,"pickaxe_delver","packbeetle");
        Lesson("grand_delver",28,30,"obsidian_sentinel",true,"pickaxe_delver","rivet_gunner");
        // Repeatable mixed pressure is eligible only after each member was taught.
        Add("Powered cutters",7,2,"ore_hauler","pickaxe_delver","rivet_gunner");
        Add("Mason convoy",7,2,"stonewright","packbeetle","pickaxe_delver");
        Add("Assay crew",13,2,"stonewright","vein_surveyor");
        Add("Powder convoy",15,2,"powder_sapper","packbeetle","rivet_gunner");
        Add("Track crew",17,2,"rail_switcher","stonewright");
        Add("Bore convoy",19,2,"bore_engineer","packbeetle","pickaxe_delver");
        Add("Faultline crew",21,2,"seismic_smith","ore_hauler");
        Add("Deep survey",21,2,"vein_surveyor","rail_switcher","pickaxe_delver");
        Add("Powder and pistons",23,2,"powder_sapper","bore_engineer");
        Add("Mason and hammer",25,2,"stonewright","seismic_smith","rivet_gunner");
        zone.liveEncounters=recipes.ToArray();zone.apexEnemy=E("grand_delver");zone.apexLocalWave=30;
        zone.displayName="Ironvein Excavation";zone.eligibleForLiveTravel=true;
        EditorUtility.SetDirty(zone);AssetDatabase.SaveAssets();
        Debug.Log("Ironvein: "+recipes.Count+" weighted recipes, three milestone windows, four-zone live travel enabled.");
    }
}
