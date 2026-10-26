using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

/// <summary>Extracts only the approved modular environment from Revision04. Never saves the source scene.</summary>
public static class DrownedCourtProductionExporter
{
    const string Source="Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision04.unity";
    const string Output="Assets/_Game/Resources/UI/BattlePresentation/";
    const string Report="ArtSource/DrownedCourtEnvironmentV2/Integration";
    static RectTransform root;
    static Rect Bounds(RectTransform r)
    {
        var c=new Vector3[4];r.GetWorldCorners(c);var lo=root.InverseTransformPoint(c[0]);var hi=root.InverseTransformPoint(c[2]);
        return new Rect((lo.x-root.rect.xMin)/2,(root.rect.yMax-hi.y)/2,(hi.x-lo.x)/2,(hi.y-lo.y)/2);
    }
    static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
    static T Asset<T>(string name) where T:ScriptableObject
    {
        string path=Output+name+".asset";var asset=AssetDatabase.LoadAssetAtPath<T>(path);
        if(!asset){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}return asset;
    }
    public static void Export()
    {
        EditorSceneManager.OpenScene(Source);root=(RectTransform)Object.FindFirstObjectByType<Canvas>().transform;
        var animators=root.GetComponentsInChildren<Animator>();
        foreach(var a in animators)a.runtimeAnimatorController.animationClips.Single().SampleAnimation(a.gameObject,0);
        foreach(var f in root.GetComponentsInChildren<ResponsiveModularFrameFitter>())f.RefreshFrame();
        Canvas.ForceUpdateCanvases();
        var assembly=Asset<DrownedCourtBattleAssembly>("CourtAssembly");assembly.approvedScene=Source;
        assembly.portrait=Bounds((RectTransform)root.Find("Stage04_Portrait_Frame"));
        assembly.boardOpening=Bounds((RectTransform)root.Find("PlayableBoardBounds_NoTileReplacement"));
        var modules=new List<Sprite>();var frames=Asset<DungeonPixelLabBattleArt>("CourtFrameModules");
        // The existing frame API uses semantic names. Keep pixels and GUIDs intact by using module aliases.
        var aliases=new List<DungeonPixelLabBattleArt.SpriteAlias>();
        string[] names={"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner","TopEdge","BottomEdge","LeftEdge","RightEdge"};
        string[] suffix={"Corner_TL","Corner_TR","Corner_BL","Corner_BR","Rail_Top","Rail_Bottom","Rail_Left","Rail_Right"};
        foreach(var family in new[]{"Portrait","Board"})
        {
            var parent=root.Find(family=="Portrait"?"Stage04_Portrait_Frame":"Stage05_BoardReview");
            var frame=parent.GetComponentInChildren<ResponsiveModularFrameFitter>().transform;
            for(int i=0;i<8;i++)aliases.Add(new DungeonPixelLabBattleArt.SpriteAlias{key=family+"_"+suffix[i],sprite=frame.Find(names[i]).GetComponent<Image>().sprite});
            aliases.Add(new DungeonPixelLabBattleArt.SpriteAlias{key=family+"_Clamp",sprite=frame.Find("NativeConnectors").GetChild(0).GetComponent<Image>().sprite});
        }
        frames.modules=Array.Empty<Sprite>();frames.aliases=aliases.ToArray();
        assembly.frames=frames;EditorUtility.SetDirty(frames);
        var pieces=new List<DrownedCourtBattleAssembly.Piece>();
        foreach(var im in root.GetComponentsInChildren<Image>())
        {
            if(!im.enabled||!im.sprite)continue;
            string path=PathOf(im.transform),asset=AssetDatabase.GetAssetPath(im.sprite);
            if(!asset.StartsWith("Assets/_Game/Art/DrownedCourtEnvironmentV2/",StringComparison.Ordinal))continue;
            if(path.Contains("Revision02_QuietMasonry")){assembly.masonry=im.sprite;continue;}
            if(path.Contains("OversizedRecess")){assembly.recess=im.sprite;continue;}
            if(path.Contains("Stage05_BoardReview")){if(im.sprite.name=="InnerShadow"||im.sprite.name.EndsWith("InnerShadow"))assembly.innerShadow=im.sprite;continue;}
            if(path.Contains("/ModularFrame/")||path.Contains("/NativeConnectors/"))continue;
            var rect=Bounds(im.rectTransform);string region;
            if(path.Contains("Stage04_Portrait"))region="Portrait";
            else if(path.Contains("Stage01_Separate"))region="BackgroundTop";
            else if(path.Contains("Stage02_Architecture"))region="Top";
            else if(path.Contains("Stage03_Platform")||path.Contains("Revision03_Rear"))region="Floor";
            else if(path.Contains("Revision01_IntegratedSides"))region="Side";
            else if(path.Contains("Stage07_LowerReef"))region="Lower";
            else if(path.Contains("Stage08_"))region="SideAccent";
            else if(path.Contains("Revision02_Ambient"))region=im.name.StartsWith("Top")||im.name=="ArenaEdge"?"TopAmbient":im.name.StartsWith("Board")?"SideAmbient":"LowerAmbient";
            else throw new Exception("Unclassified approved module: "+path);
            var p=new DrownedCourtBattleAssembly.Piece{name=im.name,region=region,rect=rect,sprite=im.sprite,type=im.type,color=im.color};
            var animator=im.GetComponent<Animator>();
            if(animator)
            {
                var clip=animator.runtimeAnimatorController.animationClips.Single();int count=Mathf.RoundToInt(clip.length*12);
                p.poses=new DrownedCourtBattleAssembly.Pose[count];
                for(int i=0;i<count;i++)
                {
                    clip.SampleAnimation(im.gameObject,i/12f);Canvas.ForceUpdateCanvases();
                    p.poses[i]=new DrownedCourtBattleAssembly.Pose{sprite=im.sprite,offset=Bounds(im.rectTransform).position-rect.position,alpha=im.color.a};
                }
                clip.SampleAnimation(im.gameObject,0);
            }
            if(im.type==Image.Type.Simple && (rect.size-im.sprite.rect.size).sqrMagnitude>.001f)throw new Exception("Non-native approved source: "+path+" "+rect.size+" / "+im.sprite.rect.size);
            pieces.Add(p);
        }
        if(!assembly.masonry||!assembly.recess||!assembly.innerShadow)throw new Exception("Missing required tile/recess/shadow");
        assembly.pieces=pieces.ToArray();EditorUtility.SetDirty(assembly);AssetDatabase.SaveAssets();Directory.CreateDirectory(Report);
        File.WriteAllText(Report+"/Pieces.tsv",string.Join("\n",pieces.Select(p=>$"{p.region}\t{p.name}\t{p.rect}\t{AssetDatabase.GetAssetPath(p.sprite)}\t{p.poses?.Length??0}")));
        File.WriteAllText(Report+"/Export.json",JsonUtility.ToJson(assembly,true));
        Debug.Log("COURT_PRODUCTION_EXPORT_PASS pieces="+pieces.Count+" tracks="+pieces.Count(p=>p.poses?.Length>0));
    }
}
