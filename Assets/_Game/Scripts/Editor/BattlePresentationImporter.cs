using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Imports only the three-zone presentation exports, leaving approved gameplay art intact.</summary>
public static class BattlePresentationImporter
{
    private const string Target = "Assets/_Game/Resources/UI/BattlePresentation";
    public static void Run()
    {
        EditorUtility.audioMasterMute = true;
        foreach (string file in Directory.GetFiles("ArtSource/BattlePresentation/native", "*.png", SearchOption.AllDirectories))
        {
            string relative = file.Replace('\\', '/').Substring("ArtSource/BattlePresentation/native/".Length);
            string path = Target + "/" + relative;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(File.ReadAllBytes(file))) File.Copy(file, path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.spriteImportMode = SpriteImportMode.Single;
            Vector2Int cell = relative.StartsWith("Scene/") ? new Vector2Int(64,64) : relative switch
            {
                "Motion/Torch.png" => new Vector2Int(24,40), "Motion/Bubble.png" => new Vector2Int(12,12),
                "Motion/Waterfall.png" => new Vector2Int(64,96), _ => Vector2Int.zero
            };
            if (cell != Vector2Int.zero)
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                var slices = new System.Collections.Generic.List<SpriteMetaData>();
                for (int y=0; y<height/cell.y; y++) for (int x=0; x<width/cell.x; x++)
                    slices.Add(new SpriteMetaData { name=$"{Path.GetFileNameWithoutExtension(file)}_{y:00}_{x:00}",
                        rect=new Rect(x*cell.x,height-(y+1)*cell.y,cell.x,cell.y), alignment=9, pivot=Vector2.one*.5f });
#pragma warning disable 618
                importer.spritesheet = slices.ToArray();
#pragma warning restore 618
            }
            importer.SaveAndReimport();
        }
        foreach (string zone in new[]{"Dungeon","Forest","Court"})
        {
            string profilePath = Target+"/"+zone+"/PlayerFrame.asset";
            var profile = AssetDatabase.LoadAssetAtPath<PlayerAreaFrameProfile>(profilePath);
            if(profile==null){profile=ScriptableObject.CreateInstance<PlayerAreaFrameProfile>();AssetDatabase.CreateAsset(profile,profilePath);}
            var data = new SerializedObject(profile);
            data.FindProperty("playerSectionWidth").floatValue=146;
            foreach(string part in new[]{"Top","Middle","Bottom"})
                data.FindProperty(char.ToLowerInvariant(part[0])+part.Substring(1)+"Piece").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<Sprite>(Target+"/"+zone+"/Player"+part+".png");
            data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(profile);
            BuildEnvironment(zone);
        }
        string materialPath=Target+"/Motion/AmbientUnlit.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null)
        {
            var shader=Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if(shader==null)throw new System.InvalidOperationException("Missing project's URP sprite shader.");
            material=new Material(shader);AssetDatabase.CreateAsset(material,materialPath);
        }
        AssetDatabase.SaveAssets(); Debug.Log("THREE ZONE PRESENTATION: native art imported.");
    }
    private static void BuildEnvironment(string zone)
    {
        string folder=Target+"/Scene/"+zone+"Tiles";Directory.CreateDirectory(folder);
        var sprites=AssetDatabase.LoadAllAssetsAtPath(Target+"/Scene/"+zone+".png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
        if(sprites.Length!=48) throw new System.InvalidOperationException("Expected 8 x 6 scenery tiles: "+zone);
        var tiles=new Tile[48];
        for(int i=0;i<tiles.Length;i++)
        {
            string path=folder+"/"+sprites[i].name+".asset";
            tiles[i]=AssetDatabase.LoadAssetAtPath<Tile>(path);
            if(tiles[i]==null){tiles[i]=ScriptableObject.CreateInstance<Tile>();AssetDatabase.CreateAsset(tiles[i],path);}
            tiles[i].sprite=sprites[i];tiles[i].colliderType=Tile.ColliderType.None;EditorUtility.SetDirty(tiles[i]);
        }
        var root=new GameObject("Presentation_"+zone,typeof(Grid),typeof(BattleEnvironmentRoot));
        try
        {
            var marker=new SerializedObject(root.GetComponent<BattleEnvironmentRoot>());
            marker.FindProperty("environmentId").stringValue="presentation-"+zone.ToLowerInvariant();marker.ApplyModifiedPropertiesWithoutUndo();
            var layer=new GameObject("NativeScenery",typeof(Tilemap),typeof(TilemapRenderer));layer.transform.SetParent(root.transform,false);
            layer.transform.localPosition=new Vector3(0,.75f,0);
            var map=layer.GetComponent<Tilemap>(); var renderer=layer.GetComponent<TilemapRenderer>();
            renderer.sortingOrder=-100;renderer.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
            // 256px of architecture above a floor whose rear edge is 48px above the actor baseline.
            for(int y=-6;y<12;y++)for(int x=-12;x<12;x++)
            {
                int sourceY=Mathf.Clamp(3-y,0,5),sourceX=Mathf.Clamp(x+4,0,7);
                map.SetTile(new Vector3Int(x,y,0),tiles[sourceY*8+sourceX]);
            }
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/_Game/Resources/BattleEnvironments/Presentation_"+zone+".prefab");
        }
        finally { Object.DestroyImmediate(root); }
    }
}
