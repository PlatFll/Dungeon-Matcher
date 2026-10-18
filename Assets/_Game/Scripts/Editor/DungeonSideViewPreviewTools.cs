using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DungeonSideViewPreviewTools
{
    public const string Kit = "ArtSource/DungeonEnvironment/SideViewRebuild";
    public const string AssetsRoot = "Assets/_Game/Art/DungeonEnvironment/SideViewRebuild";
    public const string ScenePath = "Assets/_Game/Scenes/ArtPreview_DungeonSideView.unity";
    public const string PrefabPath = "Assets/_Game/Prefabs/ArtPreview/DungeonSideView.prefab";
    [MenuItem("Dungeon Matcher/Art Preview/Build Reference Side-View Scene")]
    public static void BuildMenu()
    {
        if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildBatch();
    }
    public static void BuildBatch()
    {
        foreach (var file in Directory.GetFiles(Kit + "/Exports", "*.png", SearchOption.AllDirectories))
        {
            string target = AssetsRoot + "/" + file.Replace('\\','/').Substring((Kit + "/Exports/").Length);
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            if (!File.Exists(target) || !File.ReadAllBytes(target).SequenceEqual(File.ReadAllBytes(file))) File.Copy(file, target, true);
        }
        File.Copy(Kit + "/AssetManifest.json", AssetsRoot + "/PlacementManifest.json", true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (var file in Directory.GetFiles(AssetsRoot, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64; importer.spriteBorder = Vector4.zero;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None; importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteExtrude = 0; importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("Dungeon Side-View Art Preview");
        var preview = root.AddComponent<DungeonSideViewArtPreview>();
        preview.previewCamera = new GameObject("Preview Camera").AddComponent<Camera>();
        preview.previewCanvas = new GameObject("Preview Canvas", typeof(RectTransform)).AddComponent<Canvas>();
        preview.previewCamera.transform.SetParent(root.transform, false); preview.previewCanvas.transform.SetParent(root.transform, false);
        preview.modules = Directory.GetFiles(AssetsRoot, "*.png", SearchOption.AllDirectories).OrderBy(x=>x)
            .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\','/'))).ToArray();
        if (preview.modules.Any(s=>!s)) throw new InvalidOperationException("A native sprite failed to import.");
        preview.placementManifest = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetsRoot + "/PlacementManifest.json");
        preview.Rebuild();
        Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath)); AssetDatabase.Refresh();
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.AutomatedAction);
        EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
        Debug.Log("DUNGEON_SIDEVIEW_BUILD_PASSED");
    }
    [MenuItem("Dungeon Matcher/Art Preview/Open Reference Side-View Scene")]
    public static void Open()
    {
        if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath);
        Selection.activeObject = UnityEngine.Object.FindFirstObjectByType<DungeonSideViewArtPreview>();
    }
}
