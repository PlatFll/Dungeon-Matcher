using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public static class GideonGlassImporter
{
    public const string ArtRoot = "Assets/_Game/Art/GideonGlass";
    public const string AnimationRoot = "Assets/_Game/Animations/GideonGlass";
    public const string PlayerPath = "Assets/_Game/Resources/Players/Player_GideonGlass.asset";
    public const string AbilityPath = "Assets/_Game/Data/Player Abilities/Ability_ChronoShutter.asset";
    [Serializable] private sealed class Size { public int w,h; }
    [Serializable] private sealed class Frame { public int duration; public Size sourceSize; }
    [Serializable] private sealed class Sheet { public Frame[] frames; }

    [MenuItem("Dungeon Matcher/Art/Import Gideon Glass")]
    public static void Run()
    {
        Directory.CreateDirectory(ArtRoot); Directory.CreateDirectory(AnimationRoot);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimationRoot + "/Gideon.controller")
            ?? AnimatorController.CreateAnimatorControllerAtPath(AnimationRoot + "/Gideon.controller");
        var machine = controller.layers[0].stateMachine;
        string[] names = { "Idle", "Cast", "Hold", "Recovery" };
        int[][] times = { Enumerable.Repeat(130,9).ToArray(), new[] {80,80,120,80,50,40,80,120,100,100}, new[] {130}, new[] {80,80,80,130} };
        Sprite ready = null;
        for (int i = 0; i < names.Length; i++)
        {
            string name = "Gideon_" + names[i];
            var frames = ImportSheet(name, times[i]);
            if (i == 0) ready = frames[0];
            var clip = ImportClip(name, frames, times[i], i == 0 || i == 2);
            string stateName = i == 1 ? "Ability" : names[i];
            var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == stateName) ?? machine.AddState(stateName);
            state.motion = clip; state.speed = 1;
            foreach (var transition in state.transitions.ToArray()) state.RemoveTransition(transition);
            if (i == 0) machine.defaultState = state;
        }
        EditorUtility.SetDirty(controller);
        var ability = AssetDatabase.LoadAssetAtPath<ChronoShutterAbilityDefinition>(AbilityPath);
        if (ability == null)
        { ability = ScriptableObject.CreateInstance<ChronoShutterAbilityDefinition>(); AssetDatabase.CreateAsset(ability, AbilityPath); }
        var data = new SerializedObject(ability);
        data.FindProperty("abilityId").stringValue = "chronoshutter";
        data.FindProperty("displayName").stringValue = "ChronoShutter";
        data.FindProperty("description").stringValue = "Photograph the board. After five valid manual moves, restore its gems and refill future. Keep all damage, healing, shield and rewards earned along the way.";
        data.FindProperty("buttonLabel").stringValue = "SHUTTER";
        data.FindProperty("icon").objectReferenceValue = ImportAbilityIcon();
        data.FindProperty("meterDisplay").enumValueIndex = 1;
        data.FindProperty("energyCost").intValue = 100;
        data.FindProperty("manualMoves").intValue = 5;
        data.ApplyModifiedPropertiesWithoutUndo();
        var player = AssetDatabase.LoadAssetAtPath<PlayerDefinition>(PlayerPath);
        if (player == null)
        { player = ScriptableObject.CreateInstance<PlayerDefinition>(); AssetDatabase.CreateAsset(player, PlayerPath); }
        data = new SerializedObject(player);
        data.FindProperty("playerId").stringValue = CharacterSelectionSettings.GideonPlayerId;
        data.FindProperty("displayName").stringValue = "Gideon Glass";
        data.FindProperty("description").stringValue = "A theatrical brass automaton whose camera eye preserves a second chance at the board.";
        data.FindProperty("battleCharacterSprite").objectReferenceValue = ready;
        data.FindProperty("menuPortrait").objectReferenceValue = ready;
        data.FindProperty("battleAnimatorController").objectReferenceValue = controller;
        data.FindProperty("activeAbility").objectReferenceValue = ability;
        data.FindProperty("affinityGemType").enumValueIndex = (int)GemType.Sapphire;
        data.FindProperty("baseMaxHealth").intValue = 90;
        data.FindProperty("healthPerLevel").intValue = 9;
        data.FindProperty("baseGemDamage").floatValue = 10.5f;
        data.FindProperty("gemDamagePerLevel").floatValue = .6f;
        data.FindProperty("baseShieldCap").intValue = 40;
        data.FindProperty("shieldCapPerLevel").intValue = 3;
        data.ApplyModifiedPropertiesWithoutUndo();
        ImportCards();
        AssetDatabase.SaveAssets();
        Debug.Log("Gideon Glass imported: approved 64x64 design, idle/cast/hold/recovery, playable definition and ChronoShutter.");
    }

    private static void ImportCards()
    {
        var exposure = ImportCard("LongExposure", "long_exposure", "Long Exposure",
            "ChronoShutter waits for one extra valid manual move before rewinding (6 total).",
            RunUpgradeStat.BoardMemoryMoves, 1);
        var fluid = ImportCard("DevelopingFluid", "developing_fluid", "Developing Fluid",
            "Gain 12 shield when ChronoShutter successfully rewinds. Shield bonuses and your shield cap apply.",
            RunUpgradeStat.BoardMemoryRewindShield, 12);
        var catalog = Resources.Load<RunUpgradeCatalog>("RunUpgrades/PrototypeRunUpgradeCatalog");
        var data = new SerializedObject(catalog);
        var upgrades = data.FindProperty("upgrades");
        foreach (var card in new[] { exposure, fluid })
            if (!catalog.Upgrades.Contains(card))
            { int index = upgrades.arraySize++; upgrades.GetArrayElementAtIndex(index).objectReferenceValue = card; }
        data.ApplyModifiedPropertiesWithoutUndo();
        if (!catalog.ValidateCatalog()) throw new InvalidDataException("Invalid run upgrade catalog.");
    }

    private static Sprite ImportAbilityIcon()
    {
        string path = ArtRoot + "/Gideon_AbilityButton.png";
        File.Copy("ArtSource/GideonGlass/Gideon_AbilityButton.png", path, true);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single; importer.spritePixelsPerUnit = 64;
        importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static RunUpgradeDefinition ImportCard(string name, string id, string title, string description,
        RunUpgradeStat stat, int value)
    {
        string path = "Assets/_Game/Resources/RunUpgrades/RunUpgrade_" + name + ".asset";
        var card = AssetDatabase.LoadAssetAtPath<RunUpgradeDefinition>(path);
        if (card == null) { card = ScriptableObject.CreateInstance<RunUpgradeDefinition>(); AssetDatabase.CreateAsset(card, path); }
        var data = new SerializedObject(card);
        data.FindProperty("upgradeId").stringValue = id;
        data.FindProperty("displayTitle").stringValue = title;
        data.FindProperty("description").stringValue = description;
        data.FindProperty("rarity").enumValueIndex = (int)RunUpgradeRarity.Epic;
        data.FindProperty("theme").enumValueIndex = (int)RunUpgradeTheme.Ability;
        data.FindProperty("maxStacks").intValue = 1;
        data.FindProperty("requiredPlayerId").stringValue = CharacterSelectionSettings.GideonPlayerId;
        data.FindProperty("requiredAbilityId").stringValue = "chronoshutter";
        var modifiers = data.FindProperty("modifiers"); modifiers.arraySize = 1;
        var modifier = modifiers.GetArrayElementAtIndex(0);
        modifier.FindPropertyRelative("stat").enumValueIndex = (int)stat;
        modifier.FindPropertyRelative("operation").enumValueIndex = (int)RunUpgradeModifierOperation.Flat;
        modifier.FindPropertyRelative("value").floatValue = value;
        data.ApplyModifiedPropertiesWithoutUndo();
        return card;
    }

    private static Sprite[] ImportSheet(string name, int[] durations)
    {
        string source = "ArtSource/GideonGlass/" + name;
        var sheet = JsonUtility.FromJson<Sheet>(File.ReadAllText(source + ".json"));
        if (sheet.frames == null || !sheet.frames.Select(f => f.duration).SequenceEqual(durations) ||
            sheet.frames.Any(f => f.sourceSize.w != 64 || f.sourceSize.h != 64))
            throw new InvalidDataException("Invalid Gideon source timing/canvas: " + name);
        string path = ArtRoot + "/" + name + ".png";
        File.Copy(source + ".png", path, true);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 64; importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp; importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 1024;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
#pragma warning disable CS0618
        importer.spritesheet = Enumerable.Range(0,durations.Length).Select(i => new SpriteMetaData {
            name = name + "_" + i.ToString("00"), rect = new Rect(i*64,0,64,64),
            alignment = (int)SpriteAlignment.BottomCenter, pivot = new Vector2(.5f,0)
        }).ToArray();
#pragma warning restore CS0618
        importer.SaveAndReimport();
        var frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (frames.Length != durations.Length) throw new InvalidDataException("Missing Gideon frames: " + name);
        return frames;
    }
    private static AnimationClip ImportClip(string name, Sprite[] frames, int[] durations, bool loop)
    {
        string path = AnimationRoot + "/" + name + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip,path); }
        clip.name = name; clip.frameRate = 100; clip.ClearCurves();
        var keys = new ObjectReferenceKeyframe[frames.Length+1];
        int time = 0;
        for (int i=0;i<frames.Length;i++)
        { keys[i]=new ObjectReferenceKeyframe {time=time/1000f,value=frames[i]};time+=durations[i]; }
        keys[frames.Length]=new ObjectReferenceKeyframe {time=(time-10)/1000f,value=frames[frames.Length-1]};
        AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(Image),"m_Sprite"),keys);
        var settings=AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime=loop;settings.startTime=0;settings.stopTime=time/1000f;
        AnimationUtility.SetAnimationClipSettings(clip,settings);
        EditorUtility.SetDirty(clip); return clip;
    }
}
