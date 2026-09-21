using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Explicit production import; no runtime animation or combat system.</summary>
public static class CombatIdleImporter
{
    public const string ArtRoot = "Assets/_Game/Art/CombatIdles";
    public const string AnimationRoot = "Assets/_Game/Animations/CombatIdles";
    public static readonly string[] Characters = { "Rattlebones", "Farmer", "PanVillager", "Bardley" };
    public static readonly string[] LocalEnemies = { "Miner", "BasketVillager", "BarricadeVillager" };
    public static readonly string[] Guards = { "CrossbowGuard", "BarricadeGuard", "SpearGuard", "SiegeSergeant" };

    [Serializable] private sealed class Size { public int w, h; }
    [Serializable] private sealed class Frame { public int duration; public Size sourceSize; }
    [Serializable] private sealed class Sheet { public Frame[] frames; }

    [MenuItem("Dungeon Matcher/Art/Import Combat Idles")]
    public static void Run()
        => Import(Characters);

    public static void ImportLocalEnemies() => Import(LocalEnemies);

    [MenuItem("Dungeon Matcher/Art/Import Restored Guard Idles")]
    public static void ImportGuards() => Import(Guards);

    private static void Import(IEnumerable<string> characters)
    {
        Directory.CreateDirectory(ArtRoot);
        Directory.CreateDirectory(AnimationRoot);
        var definitionTexts = new Dictionary<string, string>();
        foreach (string character in characters)
        {
            string stem = character + "_Idle";
            string source = (Guards.Contains(character) ? "ArtSource/GuardIdles/" :
                LocalEnemies.Contains(character) ? "ArtSource/LocalEnemies/" : "ArtSource/CombatIdles/") + stem;
            string atlasPath = ArtRoot + "/" + stem + ".png";
            File.Copy(source + ".png", atlasPath, true);
            AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(atlasPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 1024;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            var sheet = JsonUtility.FromJson<Sheet>(File.ReadAllText(source + ".json"));
            if (sheet.frames == null || sheet.frames.Length != 9 || sheet.frames.Any(f => f.duration <= 0))
                throw new InvalidDataException("Invalid combat idle timing: " + character);
            int width = sheet.frames[0].sourceSize.w, height = sheet.frames[0].sourceSize.h;
            if (width <= 0 || height <= 0 || sheet.frames.Any(f => f.sourceSize.w != width || f.sourceSize.h != height))
                throw new InvalidDataException("Inconsistent idle canvases: " + character);
            // Extra prop room changes the canvas, never the character's texel scale.
#pragma warning disable CS0618 // Supported TextureImporter authoring API; retain named sprite IDs on reimport.
            importer.spritesheet = Enumerable.Range(0, 9).Select(i => new SpriteMetaData
            {
                name = stem + "_" + i.ToString("00"),
                rect = new Rect(i * width, 0, width, height),
                alignment = (int)SpriteAlignment.BottomCenter,
                pivot = new Vector2(0.5f, 0f)
            }).ToArray();
#pragma warning restore CS0618
            importer.SaveAndReimport();
            Sprite[] sprites = LoadFrames(character);
            if (sprites.Length != 9) throw new InvalidDataException("Missing imported frames: " + character);

            string clipPath = AnimationRoot + "/" + stem + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
            clip.name = stem;
            clip.frameRate = 100; // Exact 10 ms ticks; 130 ms is thirteen ticks.
            clip.ClearCurves();
            int elapsedMs = 0;
            var keys = new ObjectReferenceKeyframe[10];
            for (int i = 0; i < 9; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = elapsedMs / 1000f, value = sprites[i] };
                elapsedMs += sheet.frames[i].duration;
            }
            // Unity includes the final sample's 10ms exposure in clip length.
            // Hold frame nine on that sample instead of adding an extra frame-one tick.
            keys[9] = new ObjectReferenceKeyframe { time = (elapsedMs - 10) / 1000f, value = sprites[8] };
            AnimationUtility.SetObjectReferenceCurve(clip,
                EditorCurveBinding.PPtrCurve("", typeof(Image), "m_Sprite"), keys);
            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            clipSettings.loopTime = true;
            clipSettings.startTime = 0;
            clipSettings.stopTime = elapsedMs / 1000f;
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
            EditorUtility.SetDirty(clip);

            string controllerPath = AnimationRoot + "/" + character + "_Idle.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath)
                ?? AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine = controller.layers[0].stateMachine;
            var idle = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == "Idle")
                ?? machine.AddState("Idle");
            idle.motion = clip;
            idle.speed = 1;
            machine.defaultState = idle;
            EditorUtility.SetDirty(controller);

            bool player = character == "Rattlebones" || character == "Bardley";
            string definitionPath = player
                ? "Assets/_Game/Resources/Players/Player_" + (character == "Rattlebones" ? "Skeleton" : "Bardley") + ".asset"
                : "Assets/_Game/Data/Enemies/Enemy_" + character + ".asset";
            var definition = AssetDatabase.LoadMainAssetAtPath(definitionPath);
            definitionTexts[definitionPath] = File.ReadAllText(definitionPath);
            var serialized = new SerializedObject(definition);
            serialized.FindProperty(player ? "battleCharacterSprite" : "fallbackVisualSprite").objectReferenceValue = sprites[0];
            serialized.FindProperty(player ? "battleAnimatorController" : "animationControllerOverride").objectReferenceValue = controller;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        AssetDatabase.SaveAssets();
        // Unity's serialization/OnValidate can rewrite unrelated historical
        // gameplay fields. Retain only the two requested visual references.
        foreach (var pair in definitionTexts)
        {
            string saved = File.ReadAllText(pair.Key);
            string preserved = Regex.Replace(pair.Value,
                @"(?m)^  (battleCharacterSprite|battleAnimatorController|fallbackVisualSprite|animationControllerOverride):[^\r\n]*",
                match => Regex.Match(saved, @"(?m)^  " + match.Groups[1].Value + @":[^\r\n]*").Value);
            File.WriteAllText(pair.Key, preserved);
            AssetDatabase.ImportAsset(pair.Key, ImportAssetOptions.ForceSynchronousImport);
        }
        Debug.Log("Combat idle import complete: nine frames per controller; source pixels preserved.");
    }

    public static Sprite[] LoadFrames(string character) => AssetDatabase.LoadAllAssetsAtPath(
        ArtRoot + "/" + character + "_Idle.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
}
