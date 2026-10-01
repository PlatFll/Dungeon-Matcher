using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>Build the licensed Thaleah bitmap atlas and bind game-owned serialized labels.</summary>
public static class HudTypographyImporter
{
    public const string FontPath = "Assets/_Game/Fonts/Thaleah/Thaleah Bitmap.asset";

    [MenuItem("Dungeon Matcher/Art/Import HUD and Thaleah")]
    public static void Run()
    {
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles("ArtSource/HudTypography/UI", "*.png"))
            FinalizedVisualTargetsImporter.ImportSprite(path, "Assets/_Game/Resources/UI/Consumables/" + Path.GetFileName(path), 100, Vector4.zero);
        var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Game/Fonts/Thaleah/ThaleahFat.ttf");
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
        {
            font = TMP_FontAsset.CreateFontAsset(source, 16, 1, GlyphRenderMode.RASTER, 256, 256, AtlasPopulationMode.Dynamic, false);
            font.name = "Thaleah Bitmap";
            string characters = new string(Enumerable.Range(32, 95).Select(x => (char)x).ToArray());
            font.TryAddCharacters(characters, out string missing);
            if (!string.IsNullOrEmpty(missing)) throw new InvalidOperationException("Thaleah ASCII coverage: " + missing);
            // Preserve this face for punctuation used in the English UI without a foreign fallback.
            string aliases = "\u2018\u2019\u201c\u201d\u2013\u2014\u2212\u00d7\u00b7\u2022\u00a0\u2026";
            string ascii = "''\"\"---x** .";
            for (int i = 0; i < aliases.Length; i++)
                font.characterTable.Add(new TMP_Character(aliases[i], font, font.characterLookupTable[ascii[i]].glyph));
            font.ReadFontAssetDefinition();
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            font.fallbackFontAssetTable?.Clear();
            AssetDatabase.CreateAsset(font, FontPath);
            foreach (var atlas in font.atlasTextures)
            {
                atlas.name = "Thaleah Bitmap Atlas"; atlas.filterMode = FilterMode.Point;
                AssetDatabase.AddObjectToAsset(atlas, font);
            }
            font.material.name = "Thaleah Bitmap Material";
            AssetDatabase.AddObjectToAsset(font.material, font);
            EditorUtility.SetDirty(font);
        }
        var typography = Resources.Load<UiTypography>("UI/Typography");
        typography.regularFont = source; typography.textMeshProFont = font; EditorUtility.SetDirty(typography);
        var settings = Resources.Load<TMP_Settings>("TMP Settings");
        var serialized = new SerializedObject(settings);
        serialized.FindProperty("m_defaultFontAsset").objectReferenceValue = font;
        serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(font, out string guid, out long fontId);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(font.material, out _, out long materialId);
        foreach (string file in Directory.GetFiles("Assets/_Game", "*", SearchOption.AllDirectories)
            .Where(p => p.EndsWith(".prefab") || p.EndsWith(".unity")))
        {
            string original = File.ReadAllText(file);
            string updated = Regex.Replace(original, @"(?m)^  m_fontAsset: \{[^\r\n]*\}", "  m_fontAsset: {fileID: " + fontId + ", guid: " + guid + ", type: 2}");
            if (updated == original) continue;
            // Material and font must agree. Limit replacement to TMP component blocks.
            updated = Regex.Replace(updated, @"(?ms)(^--- !u!114 .*?)(?=^--- |\z)", match =>
                match.Value.Contains("  m_fontAsset:")
                ? Regex.Replace(match.Value, @"(?m)^  m_sharedMaterial: \{[^\r\n]*\}", "  m_sharedMaterial: {fileID: " + materialId + ", guid: " + guid + ", type: 2}")
                : match.Value);
            File.WriteAllText(file, updated); AssetDatabase.ImportAsset(file);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("HUD/Thaleah import complete: native bitmap atlas, shared typography, serialized game labels.");
    }
}
