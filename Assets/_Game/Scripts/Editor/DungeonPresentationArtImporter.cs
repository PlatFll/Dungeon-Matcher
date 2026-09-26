using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class DungeonPresentationArtImporter
{
    public static void ImportAll()
    {
        Run();
        TileBurstArtImporter.Run();
        CombatActionImporter.ImportLocalEnemies();
    }
    public const string Source = "ArtSource/Presentation/";
    public const string Root = "Assets/_Game/Resources/UI/DungeonPresentation/";
    private const string PrefabPath = "Assets/_Game/Resources/BattleEnvironments/Dungeon_Default.prefab";

    [MenuItem("Dungeon Matcher/Art/Import Dungeon Presentation")]
    public static void Run()
    {
        Directory.CreateDirectory(Root);
        foreach (string name in new[] { "DungeonBackdropTile", "Torch", "TorchAlt", "RoyalBanner",
            "Barrel", "SkullPile", "DungeonMatcherLogo", "MenuDungeon" }) Import(name, Root);
        Import("Potion", "Assets/_Game/Resources/UI/Consumables/");
        Import("Bomb", "Assets/_Game/Resources/UI/Consumables/");
        DressBattle();
        AssetDatabase.SaveAssets();
        Debug.Log("Imported native dungeon presentation art and dressed the existing battle tilemap prefab.");
    }

    private static void Import(string name, string root)
    {
        File.Copy(Source + name + ".png", root + name + ".png", true);
        AssetDatabase.ImportAsset(root + name + ".png", ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(root + name + ".png");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 64; importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.wrapMode = name == "DungeonBackdropTile" ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static void DressBattle()
    {
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var found = root.transform.Find("AtmosphereProps");
            var go = found != null ? found.gameObject : new GameObject("AtmosphereProps",typeof(Tilemap),typeof(TilemapRenderer));
            go.transform.SetParent(root.transform, false);
            // Cell Y=0 is the controller's shared character-floor anchor.
            go.transform.localPosition = Vector3.zero;
            var map = go.GetComponent<Tilemap>();map.ClearAllTiles();
            var renderer = go.GetComponent<TilemapRenderer>();
            renderer.sortingLayerName = "Default";renderer.sortingOrder = -96;
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            Place(map,"Torch",0,1); Place(map,"Torch",3,1);
            Place(map,"RoyalBanner",-1,1); Place(map,"RoyalBanner",2,1);
            Place(map,"Barrel",-4,0); Place(map,"Barrel",3,0); Place(map,"SkullPile",1,0);
            PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void Place(Tilemap map,string name,int x,int y)
    {
        string path=Root+name+".asset";
        var tile=AssetDatabase.LoadAssetAtPath<Tile>(path);
        if(tile==null){tile=ScriptableObject.CreateInstance<Tile>();AssetDatabase.CreateAsset(tile,path);}
        tile.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+name+".png");
        tile.colliderType=Tile.ColliderType.None;EditorUtility.SetDirty(tile);
        map.SetTile(new Vector3Int(x,y,0),tile);
    }
}
