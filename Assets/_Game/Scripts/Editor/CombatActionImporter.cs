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
    public static readonly string[] LocalEnemyNames = {
        "Miner_AutoAttack", "Miner_Ability", "BasketVillager_AutoAttack",
        "BarricadeVillager_AutoAttack", "BarricadeVillager_Ability"
    };
    [Serializable] private sealed class Size { public int w, h; }
    [Serializable] private sealed class Frame { public int duration; public Size sourceSize; }
    [Serializable] private sealed class Sheet { public Frame[] frames; }
    [Serializable] private sealed class AttackSelection
    {
        public string name, character, state;
        public int[] durations, impactFrames;
        public int impactFrame;
        public bool loop, legacyImpact;
    }
    [Serializable] private sealed class AttackSelections { public AttackSelection[] entries; }

    [MenuItem("Dungeon Matcher/Art/Import PixelLab Enemy Attacks")]
    public static void ImportPixelLabEnemies()
    {
        var selected = JsonUtility.FromJson<AttackSelections>("{\"entries\":" +
            File.ReadAllText("ArtSource/EnemyAttacks/selected.json") + "}").entries;
        foreach (var entry in selected) Import(new[] { entry.name + "_AutoAttack" }, entry);
    }

    [MenuItem("Dungeon Matcher/Art/Import Approved Enemy Abilities")]
    public static void ImportPixelLabAbilities()
    {
        var selected = JsonUtility.FromJson<AttackSelections>("{\"entries\":" +
            File.ReadAllText("ArtSource/EnemyAttacks/abilities.json") + "}").entries;
        foreach (var entry in selected) Import(new[] { entry.name }, entry);
        foreach (string character in selected.Select(e => e.character).Distinct())
        {
            string path = "Assets/_Game/Data/Enemies/Enemy_" + character + ".asset";
            string text = File.ReadAllText(path);
            foreach (string field in new[] { "timeSpecialAbilityFromAnimation", "useAuthoredSpecialAbilityMotion" })
                text = text.Contains("  " + field + ":")
                    ? Regex.Replace(text, @"(?m)^  " + field + @":[^\r\n]*", "  " + field + ": 1")
                    : text.TrimEnd() + "\n  " + field + ": 1\n";
            File.WriteAllText(path, text);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Dungeon Matcher/Art/Import Combat Actions")]
    public static void Run()
    {
        CombatIdleImporter.Run();
        Import(Names);
    }

    [MenuItem("Dungeon Matcher/Art/Import Local Enemy Animations")]
    public static void ImportLocalEnemies()
    {
        CombatIdleImporter.ImportLocalEnemies();
        Import(LocalEnemyNames);
    }

    private static void Import(string[] names, AttackSelection selection = null)
    {
        Directory.CreateDirectory(ArtRoot);
        Directory.CreateDirectory(AnimationRoot);
        foreach (string name in names)
        {
            bool attack = name.EndsWith("_AutoAttack");
            string character = name.Split('_')[0];
            string action = !string.IsNullOrEmpty(selection?.state) ? selection.state : attack ? "AutoAttack" : "Ability";
            bool localEnemy = LocalEnemyNames.Contains(name);
            int[] expected = attack ? new[] {80,80,120,40,120,80,80,80}
                : new[] {80,80,120,80,120,80,80,80,80,80};
            if (selection != null) expected = selection.durations;
            int count = expected.Length;
            float impactTime = selection != null ? expected.Take(selection.impactFrame).Sum() / 1000f : .32f;
            string source = (selection != null ? "ArtSource/EnemyAttacks/" : localEnemy ? "ArtSource/LocalEnemies/" : "ArtSource/CombatActions/") + name;
            var sheet = JsonUtility.FromJson<Sheet>(File.ReadAllText(source + ".json"));
            if (sheet.frames == null || !sheet.frames.Select(f => f.duration).SequenceEqual(expected))
                throw new InvalidDataException("Unexpected native timing for " + name);
            int width = sheet.frames[0].sourceSize.w, height = sheet.frames[0].sourceSize.h;
            if (width <= 0 || height <= 0 || sheet.frames.Any(f => f.sourceSize.w != width || f.sourceSize.h != height))
                throw new InvalidDataException("Inconsistent action canvases: " + name);
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
            importer.maxTextureSize = 4096;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
#pragma warning disable CS0618
            importer.spritesheet = Enumerable.Range(0, count).Select(i => new SpriteMetaData {
                name = name + "_" + i.ToString("00"), rect = new Rect(i * width, 0, width, height),
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
            clipSettings.loopTime = selection != null && selection.loop; clipSettings.startTime = 0; clipSettings.stopTime = elapsed / 1000f;
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
            AnimationEvent[] abilityEvents = selection?.impactFrames == null ? Array.Empty<AnimationEvent>() :
                selection.impactFrames.Select((frame, index) => new AnimationEvent {
                    time = expected.Take(frame).Sum() / 1000f,
                    functionName = selection.legacyImpact ? "AbilityImpact" : "AbilityBeat", intParameter = index + 1
                }).Concat(selection.loop ? Array.Empty<AnimationEvent>() : new[] {
                    new AnimationEvent { time = (elapsed - 10) / 1000f, functionName = "AbilityComplete" }
                }).ToArray();
            AnimationUtility.SetAnimationEvents(clip, attack ? new[] {
                new AnimationEvent { time = impactTime, functionName = "AutoAttackImpact" },
                new AnimationEvent { time = (elapsed - 10) / 1000f, functionName = "AutoAttackComplete" }
            } : localEnemy ? new[] {
                new AnimationEvent { time = .36f, functionName = "AbilityImpact" },
                new AnimationEvent { time = .87f, functionName = "AbilityComplete" }
            } : abilityEvents);
            EditorUtility.SetDirty(clip);

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CombatIdleImporter.AnimationRoot + "/" + ControllerName(character) + "_Idle.controller");
            if (controller == null) throw new InvalidDataException("Missing existing idle controller: " + character);
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
            if (selection == null || !selection.loop)
            {
                var exit = state.AddTransition(idle);
                exit.hasExitTime = true; exit.exitTime = 1; exit.duration = 0; exit.hasFixedDuration = true;
            }
            EditorUtility.SetDirty(controller);
        }
        AssetDatabase.SaveAssets();
        foreach (string character in names.Where(n => n.EndsWith("_AutoAttack")).Select(n => n.Split('_')[0]))
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
            if (names.Contains(character + "_Ability"))
            {
                text = Regex.Replace(text, @"(?m)^  timeSpecialAbilityFromAnimation:[^\r\n]*", "  timeSpecialAbilityFromAnimation: 1");
                if (text.Contains("  useAuthoredSpecialAbilityMotion:"))
                    text = Regex.Replace(text, @"(?m)^  useAuthoredSpecialAbilityMotion:[^\r\n]*", "  useAuthoredSpecialAbilityMotion: 1");
                else text = text.Replace("  timeSpecialAbilityFromAnimation: 1", "  timeSpecialAbilityFromAnimation: 1\n  useAuthoredSpecialAbilityMotion: 1");
            }
            File.WriteAllText(path, text);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }
        Debug.Log("Combat actions imported with native frame exposures and authored impact events: " + string.Join(", ", names));
    }

    private static string ControllerName(string character) => character == "Knight" ? "SwordKnight"
        : character == "CourtMage" ? "RoyalMage" : character == "RoyalArchbishop" ? "RoyalArcanist" : character;

    public static Sprite[] LoadFrames(string name) => AssetDatabase.LoadAllAssetsAtPath(
        ArtRoot + "/" + name + ".png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
}
