using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Promotes approved separate sprites and timing into runtime data, without modifying artwork.</summary>
public static class ForestProductionExporter
{
    const string Source = "Assets/_Game/Scenes/ArtPreview_ForestSurround_Fireflies.unity";
    const string Output = "Assets/_Game/Resources/UI/BattlePresentation/ForestAssembly.asset";
    const string ArtOutput = "Assets/_Game/Resources/UI/BattlePresentation/ForestPixelLab.asset";
    const string Report = "ArtSource/ForestProductionIntegration";
    static RectTransform root;
    static Rect Bounds(RectTransform r)
    {
        var c = new Vector3[4]; r.GetWorldCorners(c);
        var lo = root.InverseTransformPoint(c[0]); var hi = root.InverseTransformPoint(c[2]);
        return new Rect((lo.x-root.rect.xMin)/2,(root.rect.yMax-hi.y)/2,(hi.x-lo.x)/2,(hi.y-lo.y)/2);
    }
    static string PathOf(Transform t) => t.parent ? PathOf(t.parent)+"/"+t.name : t.name;
    public static void Export()
    {
        EditorSceneManager.OpenScene(Source);
        root = (RectTransform)Object.FindFirstObjectByType<Canvas>().transform;
        Canvas.ForceUpdateCanvases();
        foreach(var f in Object.FindObjectsByType<ResponsiveModularFrameFitter>(FindObjectsSortMode.None)) f.RefreshFrame();
        foreach(var m in Object.FindObjectsByType<LivingForestReviewMotion>(FindObjectsSortMode.None))m.Evaluate(0,true);
        Canvas.ForceUpdateCanvases();
        var motion = Object.FindObjectsByType<LivingForestReviewMotion>(FindObjectsSortMode.None)
            .SelectMany(x=>x.tracks).Where(x=>x.image && x.image.gameObject.activeInHierarchy).ToDictionary(x=>x.image);
        var assembly = AssetDatabase.LoadAssetAtPath<ForestBattleAssembly>(Output);
        if(!assembly){assembly=ScriptableObject.CreateInstance<ForestBattleAssembly>();AssetDatabase.CreateAsset(assembly,Output);}
        assembly.approvedScene=Source;
        var portrait=(RectTransform)root.Find("Approved forest top modules 2x/ForestPortrait");
        assembly.portrait=Bounds(portrait);
        assembly.boardOpening=Bounds((RectTransform)root.Find("Board contents native 64px"));
        var modules=new List<ForestBattleAssembly.Module>();
        string[] names={"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner","TopEdge","BottomEdge","LeftEdge","RightEdge"};
        string[] suffix={"Corner_TL","Corner_TR","Corner_BL","Corner_BR","Rail_Top","Rail_Bottom","Rail_Left","Rail_Right"};
        foreach(var family in new[]{"Portrait","Board"})
        {
            var p=family=="Portrait"?portrait:root.Find("Stage04 forest board frame 2x/BoardFrameTarget");
            var frame=p.GetComponentInChildren<ResponsiveModularFrameFitter>().transform;
            for(int i=0;i<8;i++) modules.Add(new ForestBattleAssembly.Module{key=family+"_"+suffix[i],sprite=frame.Find(names[i]).GetComponent<Image>().sprite});
            modules.Add(new ForestBattleAssembly.Module{key=family+"_Clamp",sprite=null});
        }
        modules.Add(new ForestBattleAssembly.Module{key="Recess_Center",sprite=portrait.Find("InteriorMask/OverfilledBacking").GetComponent<Image>().sprite});
        var pieces=new List<ForestBattleAssembly.Piece>();
        foreach(var im in root.GetComponentsInChildren<Image>())
        {
            if(!im.enabled || !im.sprite || !im.gameObject.activeInHierarchy) continue;
            var path=PathOf(im.transform); var bounds=Bounds(im.rectTransform);
            if(path.Contains("/Board contents ") || path.Contains("/Static readability") || path.Contains("/Shared bottom HUD/") ||
                path.Contains("/Shared Wave candidate/") || path.Contains("/Shared Guide candidate/") || path.Contains("/Shared Settings candidate/") ||
                path.Contains("/Existing Forest roster") || path.Contains("/ModularFrame/") || path.Contains("/NativeConnectors/")) continue;
            string region;
            if(path.Contains("/ForestPortrait/"))
            {
                if(path.Contains("OverfilledBacking") || path.Contains("Rattlebones") || path.Contains("affinity gem"))continue;
                region=path.Contains("Antler pennant")?"PortraitBanner":"Portrait";
            }
            else if(path.Contains("/Continuous quiet timber/"))region="Backdrop";
            else if(im.name=="Grove_Background")region="Grove";
            else if(path.Contains("/Approved forest top modules"))
                region=(im.name=="Combat_Ground"||path.Contains("Mudstone separator"))?"Ground":"Top";
            else if(path.Contains("/Stage04 forest board frame"))region="Board";
            else region=bounds.center.y<assembly.boardOpening.yMax?"Board":"Lower";
            // Only artwork survives. No sample counters, buttons, actors, gems or HP bars.
            string asset=AssetDatabase.GetAssetPath(im.sprite);
            if(asset.Contains("/CombatIdles/")||asset.Contains("/Art/Gems/")||asset.Contains("/Art/Forest/"))throw new Exception("Sample leaked: "+path);
            var piece=new ForestBattleAssembly.Piece{name=im.name,region=region,rect=bounds,sprite=im.sprite,type=im.type,color=im.color};
            if(motion.TryGetValue(im,out var track))
            {
                var amplitude=root.InverseTransformVector(im.transform.parent.TransformVector(track.amplitude))/2;
                piece.motion=new ForestAmbientMotion.Track{frames=track.frames,kind=(ForestAmbientMotion.MotionKind)(int)track.kind,
                    amplitude=new Vector2(amplitude.x,amplitude.y),period=track.period,phase=track.phase,onFraction=track.onFraction,peak=track.peak,
                    driftPeriod=track.driftPeriod,frameRate=track.frameRate,gustDuration=track.gustDuration};
                piece.sprite=track.frames[0];piece.color=Color.white;
            }
            pieces.Add(piece);
        }
        assembly.modules=modules.ToArray();assembly.pieces=pieces.ToArray();EditorUtility.SetDirty(assembly);
        var art=AssetDatabase.LoadAssetAtPath<DungeonPixelLabBattleArt>(ArtOutput);
        if(!art){AssetDatabase.CopyAsset("Assets/_Game/Resources/UI/BattlePresentation/DungeonPixelLab.asset",ArtOutput);art=AssetDatabase.LoadAssetAtPath<DungeonPixelLabBattleArt>(ArtOutput);}
        art.forestAssembly=assembly;EditorUtility.SetDirty(art);
        Directory.CreateDirectory(Report);
        File.WriteAllText(Report+"/Export.json",JsonUtility.ToJson(assembly,true));
        File.WriteAllText(Report+"/Pieces.tsv",string.Join("\n",pieces.Select(x=>$"{x.region}\t{x.name}\t{x.rect}\t{AssetDatabase.GetAssetPath(x.sprite)}\t{x.motion?.frames?.Length??0}")));
        AssetDatabase.SaveAssets();
        // Game's runtime-zone selection is authored separately. Do not save Game here:
        // ExecuteAlways legacy layout scripts may otherwise serialize unrelated transforms.
        Debug.Log($"Forest production export: {pieces.Count} separate pieces; {pieces.Count(p=>p.motion!=null)} motion tracks; no source artwork changed.");
    }
}
