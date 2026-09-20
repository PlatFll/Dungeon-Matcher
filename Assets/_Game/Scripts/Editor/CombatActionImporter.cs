using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Explicit native-sheet import into the existing character controllers.</summary>
public static class CombatActionImporter
{
    public const string ArtRoot = "Assets/_Game/Art/CombatActions";
    public const string AnimationRoot = "Assets/_Game/Animations/CombatActions";
    public static readonly string[] Names = {
        "Farmer_AutoAttack", "PanVillager_AutoAttack", "Rattlebones_Ability", "Bardley_Ability"
    };
    [Serializable] private sealed class Frame { public int duration; }
    [Serializable] private sealed class Sheet { public Frame[] frames; }

    [MenuItem("Dungeon Matcher/Art/Import Combat Actions")]
    public static void Run()
    {
        CombatIdleImporter.Run();
        Directory.CreateDirectory(ArtRoot);
        Directory.CreateDirectory(AnimationRoot);
        foreach (string name in Names)
        {
            bool attack = name.EndsWith("_AutoAttack");
            string character = name.Split('_')[0];
            string action = attack ? "AutoAttack" : "Ability";
            int width = attack ? 96 : 64, count = attack ? 8 : 10;
            int[] expected = attack ? new[] {80,80,120,40,120,80,80,80}
                : new[] {80,80,120,80,120,80,80,80,80,80};
            string source = "ArtSource/CombatActions/" + name;
            var sheet = JsonUtility.FromJson<Sheet>(File.ReadAllText(source + ".json"));
            if (sheet.frames == null || !sheet.frames.Select(f => f.duration).SequenceEqual(expected))
                throw new InvalidDataException("Unexpected native timing for " + name);
            string atlasPath = ArtRoot + "/" + name + ".png";
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
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
#pragma warning disable CS0618
            importer.spritesheet = Enumerable.Range(0, count).Select(i => new SpriteMetaData {
                name = name + "_" + i.ToString("00"), rect = new Rect(i * width, 0, width, 64),
                alignment = (int)SpriteAlignment.BottomCenter, pivot = new Vector2(.5f, 0)
            }).ToArray();
#pragma warning restore CS0618
            importer.SaveAndReimport();
            Sprite[] sprites = LoadFrames(name);
            if (sprites.Length != count) throw new InvalidDataException("Missing frames: " + name);
            string clipPath = AnimationRoot + "/" + name + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
            clip.name = name; clip.frameRate = 100; clip.ClearCurves();
            var keys = new ObjectReferenceKeyframe[count + 1];
            int elapsed = 0;
            for (int i = 0; i < count; i++) {
                keys[i] = new ObjectReferenceKeyframe { time = elapsed / 1000f, value = sprites[i] };
                elapsed += expected[i];
            }
            keys[count] = new ObjectReferenceKeyframe { time = (elapsed - 10) / 1000f, value = sprites[count - 1] };
            AnimationUtility.SetObjectReferenceCurve(clip,
                EditorCurveBinding.PPtrCurve("", typeof(Image), "m_Sprite"), keys);
            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            clipSettings.loopTime = false; clipSettings.startTime = 0; clipSettings.stopTime = elapsed / 1000f;
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
            AnimationUtility.SetAnimationEvents(clip, attack ? new[] {
                new AnimationEvent { time = .32f, functionName = "AutoAttackImpact" },
                new AnimationEvent { time = .67f, functionName = "AutoAttackComplete" }
            } : Array.Empty<AnimationEvent>());
            EditorUtility.SetDirty(clip);

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CombatIdleImporter.AnimationRoot + "/" + character + "_Idle.controller");
            var machine = controller.layers[0].stateMachine;
            var idle = machine.states.Single(s => s.state.name == "Idle").state;
            var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == action)
                ?? machine.AddState(action);
            state.motion = clip; state.speed = 1; state.writeDefaultValues = true;
            if (!controller.parameters.Any(p => p.name == action))
                controller.AddParameter(action, AnimatorControllerParameterType.Trigger);
            foreach (var old in idle.transitions.Where(t => t.destinationState == state).ToArray()) idle.RemoveTransition(old);
            foreach (var old in state.transitions.ToArray()) state.RemoveTransition(old);
            var enter = idle.AddTransition(state);
            enter.hasExitTime = false; enter.duration = 0; enter.hasFixedDuration = true;
            enter.AddCondition(AnimatorConditionMode.If, 0, action);
            var exit = state.AddTransition(idle);
            exit.hasExitTime = true; exit.exitTime = 1; exit.duration = 0; exit.hasFixedDuration = true;
            EditorUtility.SetDirty(controller);
        }
        AssetDatabase.SaveAssets();
        foreach (string character in new[] {"Farmer", "PanVillager"})
        {
            // Patch only the opted-in presentation flags. Do not let OnValidate
            // rewrite historical enemy balance or ability configuration.
            string path = "Assets/_Game/Data/Enemies/Enemy_" + character + ".asset";
            string text = File.ReadAllText(path);
            if (text.Contains("  timeAutoAttackFromAnimation:"))
                text = Regex.Replace(text, @"(?m)^  timeAutoAttackFromAnimation:.*$", "  timeAutoAttackFromAnimation: 1");
            else text = text.TrimEnd() + "\n  timeAutoAttackFromAnimation: 1\n";
            if (text.Contains("  useAuthoredAutoAttackMotion:"))
                text = Regex.Replace(text, @"(?m)^  useAuthoredAutoAttackMotion:.*$", "  useAuthoredAutoAttackMotion: 1");
            else text = text.Replace("  timeAutoAttackFromAnimation: 1", "  timeAutoAttackFromAnimation: 1\n  useAuthoredAutoAttackMotion: 1");
            File.WriteAllText(path, text);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }
        Debug.Log("Combat actions imported: 680ms attacks with 320ms impacts; 880ms ability casts.");
    }

    public static Sprite[] LoadFrames(string name) => AssetDatabase.LoadAllAssetsAtPath(
        ArtRoot + "/" + name + ".png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
}
