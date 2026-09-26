using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Imports the native LibreSprite exports without resampling their pixels.</summary>
public static class TileBurstArtImporter
{
    public const string SourceRoot = "ArtSource/TileVfx";
    public const string ArtRoot = "Assets/_Game/Art/VFX/TileBursts";
    public const string LibraryPath = "Assets/_Game/Resources/VFX/TileBursts.asset";
    private static readonly string[] Names = { "Explosion", "PoisonExplosion", "ShieldExplosion", "HealingExplosion" };
    [Serializable] private sealed class Size { public int w, h; }
    [Serializable] private sealed class Frame { public int duration; public Size sourceSize; }
    [Serializable] private sealed class Sheet { public Frame[] frames; }

    [MenuItem("Dungeon Matcher/Art/Import Tile Bursts")]
    public static void Run()
    {
        var sheets = new Sheet[Names.Length];
        for (int i = 0; i < Names.Length; i++)
        {
            string source = SourceRoot + "/" + Names[i];
            if (!File.Exists(source + ".png") || !File.Exists(source + ".json"))
                throw new FileNotFoundException("Export the native PNG and json-array timing file first: " + source);
            var sheet = JsonUtility.FromJson<Sheet>(File.ReadAllText(source + ".json"));
            if (sheet?.frames == null || sheet.frames.Length < 2 || sheet.frames.Length > 32 ||
                sheet.frames.Any(f => f == null || f.sourceSize == null || f.sourceSize.w != 64 ||
                    f.sourceSize.h != 64 || f.duration < 10 || f.duration > 300))
                throw new InvalidDataException("Expected ordered 64x64 native burst frames with valid exposures: " + source);
            sheets[i] = sheet;
        }

        Directory.CreateDirectory(ArtRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(LibraryPath));
        AssetDatabase.Refresh();
        var library = AssetDatabase.LoadAssetAtPath<TileBurstLibrary>(LibraryPath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<TileBurstLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
        }
        var sequences = new TileBurstLibrary.Sequence[Names.Length];
        for (int i = 0; i < Names.Length; i++)
        {
            string name = Names[i];
            string path = ArtRoot + "/" + name + ".png";
            File.Copy(SourceRoot + "/" + name + ".png", path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
#pragma warning disable CS0618
            importer.spritesheet = Enumerable.Range(0, sheets[i].frames.Length).Select(f => new SpriteMetaData {
                name = name + "_" + f.ToString("00"), rect = new Rect(f * 64, 0, 64, 64),
                alignment = (int)SpriteAlignment.Center, pivot = new Vector2(.5f, .5f)
            }).ToArray();
#pragma warning restore CS0618
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture.width != sheets[i].frames.Length * 64 || texture.height != 64)
                throw new InvalidDataException("Expected a horizontal untrimmed sheet: " + path);
            var frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (frames.Length != sheets[i].frames.Length) throw new InvalidDataException("Missing sliced frames: " + name);
            sequences[i] = new TileBurstLibrary.Sequence {
                kind = (TileBurstKind)i, frames = frames,
                durations = sheets[i].frames.Select(f => f.duration / 1000f).ToArray()
            };
        }
        library.sequences = sequences;
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        Debug.Log("Imported four native tile burst families, centered at 64 PPU with exact authored exposure timing.");
    }
}
