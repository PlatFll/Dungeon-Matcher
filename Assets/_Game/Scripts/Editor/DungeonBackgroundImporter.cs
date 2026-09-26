using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Explicit, background-only import; does not invoke the broader presentation importer.</summary>
public static class DungeonBackgroundImporter
{
    public const string Source = "ArtSource/Backgrounds/";
    public const string Root = "Assets/_Game/Art/Backgrounds/BattleArea/BackgroundRefinement/";
    public const string Prefab = "Assets/_Game/Resources/BattleEnvironments/Dungeon_Default.prefab";
    public const string Surround = "Assets/_Game/Resources/UI/DungeonPresentation/DungeonBackdropTile.png";

    [MenuItem("Dungeon Matcher/Art/Import Background Refinement Only")]
    public static void Run()
    {
        Directory.CreateDirectory(Root);
        Import(Source + "BattlegroundWall.png", Root + "BattlegroundWall.png", true);
        Import(Source + "BattlegroundFloor.png", Root + "BattlegroundFloor.png", true);
        Import(Source + "Modules/Foundation.png", Root + "Foundation.png", false);
        Import(Source + "GeneralMasonry.png", Surround, false, true);
        var wall = Tiles("BattlegroundWall"); var floor = Tiles("BattlegroundFloor"); var foundation = Tiles("Foundation")[0];
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            // Keep the existing Grid, viewport mask, sorting and layout ownership.
            var back = root.transform.Find("BackWall").GetComponent<Tilemap>();
            var ground = root.transform.Find("Floor").GetComponent<Tilemap>();
            foreach (var map in root.GetComponentsInChildren<Tilemap>()) map.ClearAllTiles();
            back.transform.localPosition = new Vector3(0, .75f, 0);
            ground.transform.localPosition = new Vector3(0, .75f, 0);
            // Center the 512x256 composition. Quiet modular wall overscan fills tall/wide views.
            for (int y = 0; y < 8; y++) for (int x = -8; x < 8; x++)
            {
                int col = ((x + 4) % 8 + 8) % 8;
                int index = y < 4 && x >= -4 && x < 4 ? y * 8 + col : 3 * 8 + col;
                back.SetTile(new Vector3Int(x, y), wall[index]);
            }
            for (int x = -8; x < 8; x++)
            {
                ground.SetTile(new Vector3Int(x, -1), floor[((x + 4) % 8 + 8) % 8]);
                for (int y = -2; y >= -6; y--) ground.SetTile(new Vector3Int(x, y), foundation);
            }
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Debug.Log("Imported only battleground and surrounding background artwork.");
    }

    private static void Import(string source, string path, bool sliced, bool repeat = false)
    {
        File.Copy(source, path, true);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = sliced ? SpriteImportMode.Multiple : SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 64; importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
        importer.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        if (sliced)
        {
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
#pragma warning disable CS0618
            importer.spritesheet = Enumerable.Range(0, width / 64 * (height / 64)).Select(i => new SpriteMetaData
            {
                name = Path.GetFileNameWithoutExtension(path) + "_" + i.ToString("00"),
                rect = new Rect(i % (width / 64) * 64, i / (width / 64) * 64, 64, 64),
                alignment = (int)SpriteAlignment.Center, pivot = new Vector2(.5f, .5f)
            }).ToArray();
#pragma warning restore CS0618
        }
        importer.SaveAndReimport();
    }

    private static Tile[] Tiles(string name)
    {
        return AssetDatabase.LoadAllAssetsAtPath(Root + name + ".png").OfType<Sprite>().OrderBy(s => s.name).Select(sprite =>
        {
            string path = Root + sprite.name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null) { tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile, path); }
            tile.sprite = sprite; tile.colliderType = Tile.ColliderType.None; EditorUtility.SetDirty(tile); return tile;
        }).ToArray();
    }
}
