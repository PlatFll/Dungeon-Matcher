using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class IronveinProductionExporter
{
    const string Source = "Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision04.unity";
    const string Output = "Assets/_Game/Resources/UI/BattlePresentation/";
    const string Report = "ArtSource/IronveinBelowTopHUD/Integration";
    static Transform root;
    static Rect Bounds(RectTransform r)
    { var c = new Vector3[4]; r.GetWorldCorners(c); return new Rect(c[0].x / 2, (1280 - c[2].y) / 2, (c[2].x-c[0].x)/2, (c[2].y-c[0].y)/2); }
    static string PathOf(Transform t) => t.parent && t.parent != root ? PathOf(t.parent)+"/"+t.name : t.name;
    static T Asset<T>(string name) where T : ScriptableObject
    { string path=Output+name+".asset"; var a=AssetDatabase.LoadAssetAtPath<T>(path); if(!a){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);} return a; }
    public static void Export()
    {
        EditorSceneManager.OpenScene(Source); root=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Single(c=>c.isRootCanvas).transform;
        foreach(var f in root.GetComponentsInChildren<ResponsiveModularFrameFitter>())f.RefreshFrame();
        Canvas.ForceUpdateCanvases(); Directory.CreateDirectory(Report);
        var a=Asset<IronveinBattleAssembly>("IronveinAssembly");a.approvedScene=Source;
        a.sharedControls=Resources.Load<DungeonPixelLabBattleArt>("UI/BattlePresentation/DungeonPixelLab");
        a.portrait=Bounds((RectTransform)root.Find("IronveinNativeFrames/PortraitFrameBounds"));
        a.boardOpening=Bounds((RectTransform)root.Find("PlayableBoardBounds_NoTileReplacement"));
        var frames=Asset<DungeonPixelLabBattleArt>("IronveinFrameModules");var aliases=new List<DungeonPixelLabBattleArt.SpriteAlias>();
        string[] names={"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner","TopEdge","BottomEdge","LeftEdge","RightEdge"};
        string[] suffix={"Corner_TL","Corner_TR","Corner_BL","Corner_BR","Rail_Top","Rail_Bottom","Rail_Left","Rail_Right"};
        foreach(string family in new[]{"Portrait","Board"})
        {
            var frame=root.Find("IronveinNativeFrames/"+family+"FrameBounds/ModularFrame");
            for(int i=0;i<8;i++)aliases.Add(new DungeonPixelLabBattleArt.SpriteAlias{key=family+"_"+suffix[i],sprite=frame.Find(names[i]).GetComponent<Image>().sprite});
            aliases.Add(new DungeonPixelLabBattleArt.SpriteAlias{key=family+"_Clamp",sprite=frame.Find("NativeConnectors").GetChild(0).GetComponent<Image>().sprite});
        }
        frames.modules=Array.Empty<Sprite>();frames.aliases=aliases.ToArray();EditorUtility.SetDirty(frames);a.frames=frames;
        var pieces=new List<IronveinBattleAssembly.Piece>(); var dump=new List<string>();
        foreach(var im in root.GetComponentsInChildren<Image>())
        {
            if(!im.enabled||!im.sprite)continue;
            string path=PathOf(im.transform);var rect=Bounds(im.rectTransform);string asset=AssetDatabase.GetAssetPath(im.sprite);
            dump.Add(path+"\t"+rect+"\t"+im.type+"\t"+asset);
            if(path.StartsWith("Locked")||path.StartsWith("UnchangedShared")||path.StartsWith("Stage04_Drills"))continue;
            if(path.Contains("Stage03_AmberInset")){a.amberInset=im.sprite;continue;}
            if(path.StartsWith("IronveinNativeFrames"))continue;
            if(path.StartsWith("PortraitInterior")){a.recess=im.sprite;continue;}
            string region=path.Split('/')[0];
            var scale=im.transform.lossyScale;
            if(Mathf.Abs(Mathf.DeltaAngle(im.transform.eulerAngles.z,0))>.01f)throw new Exception("Unexpected rotated scenery "+path);
            pieces.Add(new IronveinBattleAssembly.Piece{name=path,region=region,sprite=im.sprite,rect=rect,type=im.type,color=im.color,reflection=new Vector2(Mathf.Sign(scale.x),Mathf.Sign(scale.y))});
        }
        a.pieces=pieces.ToArray();EditorUtility.SetDirty(a);AssetDatabase.SaveAssets();
        File.WriteAllText(Report+"/SourceImages.tsv",string.Join("\n",dump));
        File.WriteAllText(Report+"/Export.json",JsonUtility.ToJson(a,true));
        Debug.Log("IRONVEIN_EXPORT_PASS pieces="+pieces.Count+" portrait="+a.portrait+" board="+a.boardOpening);
    }
}
