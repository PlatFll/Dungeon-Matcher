using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ApprovedDungeonBattleImporter
{
    public static void Build()
    {
        const string path="Assets/_Game/Resources/UI/BattlePresentation/ApprovedDungeon.asset";
        var set=AssetDatabase.LoadAssetAtPath<ApprovedDungeonBattleArt>(path);
        if(set==null){set=ScriptableObject.CreateInstance<ApprovedDungeonBattleArt>();AssetDatabase.CreateAsset(set,path);}
        set.placementManifest=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/PlacementManifest.json");
        set.modules=Directory.GetFiles("Assets/_Game/Art/DungeonEnvironment/SideViewRebuild","*.png",SearchOption.AllDirectories)
            .OrderBy(p=>p).Select(p=>AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\','/'))).ToArray();
        if(set.placementManifest==null||set.modules.Any(s=>s==null)||set.modules.Length!=119)throw new System.InvalidOperationException("Approved native art library is incomplete.");
        EditorUtility.SetDirty(set);AssetDatabase.SaveAssets();Debug.Log("APPROVED_DUNGEON_BINDING_COMPLETE: 119 original modules; no artwork copied or resampled.");
    }
}
