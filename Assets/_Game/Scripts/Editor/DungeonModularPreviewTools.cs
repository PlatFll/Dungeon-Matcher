using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DungeonModularPreviewTools
{
    public const string Kit = "ArtSource/DungeonEnvironment/ModularRebuild";
    public const string AssetsRoot = "Assets/_Game/Art/DungeonEnvironment/ModularRebuild";
    public const string ScenePath = "Assets/_Game/Scenes/ArtPreview_DungeonModular.unity";

    [MenuItem("Dungeon Matcher/Art Preview/Build Dungeon Modular Scene")]
    public static void BuildMenu()
    {
        if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Build();
    }
    // Suitable for -executeMethod; the CLI controls quitting and log capture.
    public static void BuildBatch() { Build(); Debug.Log("DUNGEON_MODULAR_BUILD_PASSED"); }

    public static DungeonModularArtPreview Build()
    {
        foreach (string file in Directory.GetFiles(Kit+"/Exports","*.png",SearchOption.AllDirectories).OrderBy(x=>x))
        {
            string relative = file.Replace('\\','/').Substring((Kit+"/Exports/").Length);
            string target = AssetsRoot+"/"+relative;
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            if (!File.Exists(target) || !File.ReadAllBytes(target).SequenceEqual(File.ReadAllBytes(file))) File.Copy(file,target,true);
        }
        Directory.CreateDirectory(AssetsRoot);
        File.Copy(Kit+"/AssetManifest.json",AssetsRoot+"/PlacementManifest.json",true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string path in Directory.GetFiles(AssetsRoot,"*.png",SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64; importer.filterMode = FilterMode.Point;
            importer.spriteBorder = Path.GetFileName(path).StartsWith("EmptyCell",StringComparison.Ordinal)
                ? new Vector4(8,8,8,8) : Vector4.zero;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.isReadable = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp; importer.maxTextureSize = 2048;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var preview = new GameObject("Dungeon Modular Art Preview").AddComponent<DungeonModularArtPreview>();
        preview.previewCamera = new GameObject("PreviewCamera").AddComponent<Camera>();
        preview.previewCanvas = new GameObject("PreviewCanvas",typeof(RectTransform)).AddComponent<Canvas>();
        preview.modules = Directory.GetFiles(AssetsRoot,"*.png",SearchOption.AllDirectories).OrderBy(x=>x)
            .Select(path=>AssetDatabase.LoadAssetAtPath<Sprite>(path.Replace('\\','/'))).ToArray();
        if (preview.modules.Length < 80 || preview.modules.Any(s=>s==null)) throw new InvalidOperationException("Incomplete modular sprite import.");
        preview.placementManifest = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetsRoot+"/PlacementManifest.json");
        preview.Rebuild();
        EditorSceneManager.SaveScene(scene,ScenePath);
        AssetDatabase.SaveAssets();
        return preview;
    }

    [MenuItem("Dungeon Matcher/Art Preview/Open Dungeon Modular Scene")]
    public static void Open()
    {
        if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath);
        Selection.activeObject = UnityEngine.Object.FindFirstObjectByType<DungeonModularArtPreview>();
    }
}
