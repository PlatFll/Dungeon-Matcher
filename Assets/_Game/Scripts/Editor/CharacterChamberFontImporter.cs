using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class CharacterChamberFontImporter
{
    private const string Path = "Assets/_Game/Resources/UI/CharacterChamber/PixelifyReading.asset";

    [MenuItem("Dungeon Matcher/Art/Bake character chamber reading font")]
    public static void Bake()
    {
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Path) != null) return;
        var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Game/Art/UI/Fonts/PixelifySans-Bold.ttf");
        var font = TMP_FontAsset.CreateFontAsset(source,32,4,GlyphRenderMode.SDFAA,512,512,AtlasPopulationMode.Dynamic,false);
        font.name = "Pixelify Reading";
        string ascii = new string(Enumerable.Range(32,95).Select(c=>(char)c).ToArray());
        if (!font.TryAddCharacters(ascii,out string missing)) throw new InvalidOperationException(missing);
        font.TryAddCharacters("’‘–—×…",out _);
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(font,Path);
        foreach (var texture in font.atlasTextures) AssetDatabase.AddObjectToAsset(texture,font);
        AssetDatabase.AddObjectToAsset(font.material,font);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
    }
}
