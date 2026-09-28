using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Explicit import of the finalized visual review candidate. Keeps original art GUIDs intact.</summary>
public static class FinalizedVisualTargetsImporter
{
    public const string Source = "ArtSource/FinalizedVisuals/";
    public const string Root = "Assets/_Game/Art/FinalizedVisuals/";
    public const string EnvironmentPrefab = "Assets/_Game/Resources/BattleEnvironments/Dungeon_Finalized.prefab";
    private const string OriginalPrefab = "Assets/_Game/Resources/BattleEnvironments/Dungeon_Default.prefab";

    [MenuItem("Dungeon Matcher/Art/Import Finalized Visual Candidate")]
    public static void Run()
    {
        ImportEnvironment();
        ImportTilePalette();
        foreach (string path in Directory.GetFiles(Source + "UI", "*.png"))
            ImportSprite(path, "Assets/_Game/Resources/UI/Finalized/" + Path.GetFileName(path), 64, Border(Path.GetFileNameWithoutExtension(path)));
        ImportHealthStyles();
        if (File.Exists(Source + "UI/DungeonMatcherLogo.png"))
            ImportSprite(Source + "UI/DungeonMatcherLogo.png", "Assets/_Game/Resources/UI/DungeonPresentation/DungeonMatcherLogo.png", 64, Vector4.zero);
        AssetDatabase.SaveAssets();
        Debug.Log("Imported finalized visual candidate. Final screenshot approval remains pending.");
    }

    public static Vector4 Border(string name)
    {
        if (name.StartsWith("ButtonLarge") || name.StartsWith("Panel")) return new Vector4(16,16,16,16);
        if (name.StartsWith("ButtonSmall")) return new Vector4(8,8,8,8);
        if (name == "WavePlaque") return new Vector4(16,8,16,8);
        return Vector4.zero;
    }

    private static void ImportTilePalette()
    {
        string directory = Root + "Environment";
        string path = directory + "/FinalizedDungeonPalette.prefab";
        if (!File.Exists(path))
            UnityEditor.Tilemaps.GridPaletteUtility.CreateNewPalette(directory, "FinalizedDungeonPalette",
                GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Manual,
                new Vector3(1,1,0), GridLayout.CellSwizzle.XYZ);
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var map = root.GetComponentInChildren<Tilemap>(); map.ClearAllTiles();
            string[] names = { "WallA", "WallB", "WallC", "WallD", "WallWarmA", "WallWarmB",
                "FloorA", "FloorB", "FloorC", "FloorD", "FoundationA", "FoundationB", "FoundationC" };
            for (int i = 0; i < names.Length; i++) map.SetTile(new Vector3Int(i % 6, -(i / 6)), Tile("Tiles/" + names[i]));
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void ImportHealthStyles()
    {
        string root = "Assets/_Game/Resources/UI/Finalized/";
        if (!File.Exists(root + "HealthFill.png")) return;
        Directory.CreateDirectory(root + "Health");
        foreach (string rank in new[] { "Player", "Normal", "Special", "Miniboss", "Boss" })
        {
            string path = root + "Health/" + rank + ".asset";
            var style = AssetDatabase.LoadAssetAtPath<ModularHealthBarStyle>(path);
            if (style == null) { style = ScriptableObject.CreateInstance<ModularHealthBarStyle>(); AssetDatabase.CreateAsset(style,path); }
            var serialized = new SerializedObject(style);
            foreach (var field in new[] { ("startPiece", rank + "HealthStart"), ("middlePiece", rank + "HealthMiddle"),
                ("endPiece", rank + "HealthEnd"), ("badge", rank + "Badge"), ("fillStrip", "HealthFill"), ("emptyTrack", "HealthTrack") })
                serialized.FindProperty(field.Item1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(root + field.Item2 + ".png");
            serialized.FindProperty("fillInsetLeft").floatValue = 27;
            serialized.FindProperty("fillInsetRight").floatValue = 13;
            serialized.FindProperty("fillInsetVertical").floatValue = 8;
            serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(style);
        }
    }

    private static void ImportEnvironment()
    {
        foreach (string folder in new[] { "Tiles", "Props", "Baked" })
            foreach (string path in Directory.GetFiles(Source + "Environment/" + folder, "*.png"))
                if (!Path.GetFileName(path).Contains("Review"))
                    ImportSprite(path, Root + "Environment/" + folder + "/" + Path.GetFileName(path), 64, Vector4.zero);
        ImportSprite(Source + "Environment/DungeonMaster.png", Root + "Environment/DungeonMaster.png", 64, Vector4.zero);
        if (!File.Exists(EnvironmentPrefab) && !AssetDatabase.CopyAsset(OriginalPrefab, EnvironmentPrefab))
            throw new InvalidOperationException("Could not duplicate the valid dungeon environment prefab.");
        var root = PrefabUtility.LoadPrefabContents(EnvironmentPrefab);
        try
        {
            root.name = "Dungeon_Finalized";
            var serialized = new SerializedObject(root.GetComponent<BattleEnvironmentRoot>());
            serialized.FindProperty("environmentId").stringValue = "dungeon-finalized-review-candidate";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            foreach (var map in root.GetComponentsInChildren<Tilemap>()) map.ClearAllTiles();
            // The old optional atmosphere layer is baked into the candidate's four authored layers.
            var atmosphere = root.transform.Find("AtmosphereProps");
            if (atmosphere != null) UnityEngine.Object.DestroyImmediate(atmosphere.gameObject);
            var back = root.transform.Find("BackWall").GetComponent<Tilemap>();
            var floor = root.transform.Find("Floor").GetComponent<Tilemap>();
            back.transform.localPosition = floor.transform.localPosition = new Vector3(0, .75f, 0);
            for (int x = -8; x < 8; x++)
            {
                int col = (x + 12) % 8;
                for (int y = 0; y < 8; y++)
                {
                    var tile = y < 4 && x >= -4 && x < 4 ? Baked(3 - y, col) : Tile("Tiles/Wall" + (char)('A' + (x + y + 24) % 4));
                    back.SetTile(new Vector3Int(x,y), tile);
                }
                floor.SetTile(new Vector3Int(x,-1), x >= -4 && x < 4 ? Baked(4,col) : Tile("Tiles/Floor" + (char)('A' + (x+8)%4)));
                for (int y = -2; y >= -6; y--)
                    floor.SetTile(new Vector3Int(x,y), y == -2 && x >= -4 && x < 4 ? Baked(5,col) : Tile("Tiles/Foundation" + (char)('A' + (x-y+24)%3)));
            }
            PrefabUtility.SaveAsPrefabAsset(root, EnvironmentPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        // The serialized Game scene creates its environment through the controller.
        // Set only that presentation reference; do not rewrite actor or layout state.
        string scene = "Assets/_Game/Scenes/Game.unity";
        string contents = File.ReadAllText(scene);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnvironmentPrefab);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string guid, out long fileId);
        string updated = Regex.Replace(contents, @"(m_EditorClassIdentifier: Assembly-CSharp::BattleBackgroundTilemapController[\s\S]*?worldCamera:[^\r\n]+)(?:\r?\n  environmentPrefab:[^\r\n]+)?",
            "$1\n  environmentPrefab: {fileID: " + fileId + ", guid: " + guid + ", type: 3}");
        if (updated != contents) { File.WriteAllText(scene, updated); AssetDatabase.ImportAsset(scene); }
    }

    private static Tile Baked(int row, int col) => Tile("Baked/Cell" + (char)('A' + row) + (char)('A' + col));
    private static Tile Tile(string name)
    {
        string path = Root + "Environment/" + name + ".asset";
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null) { tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile,path); }
        tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Environment/" + name + ".png");
        tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None;
        EditorUtility.SetDirty(tile); return tile;
    }

    public static void ImportSprite(string source, string path, float ppu, Vector4 border)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.Copy(source,path,true);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = ppu; importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
        importer.wrapMode = TextureWrapMode.Clamp; importer.spriteBorder = border;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spritePivot = Vector2.one * .5f; importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }
}
