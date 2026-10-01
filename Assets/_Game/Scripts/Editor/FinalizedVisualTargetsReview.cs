using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

/// <summary>Opt-in art review in the production scene, with disposable player data.</summary>
[InitializeOnLoad]
public static class FinalizedVisualTargetsReview
{
    private const string Key = "DungeonMatcher.FinalizedVisualTargetsReview";
    private static readonly string Output = Path.GetFullPath(".utmp/FinalizedVisualReview");
    private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly System.Text.StringBuilder Report = new System.Text.StringBuilder();
    private static IDisposable profile, selection;
    private static string failure;
    private static int exitCode;

    static FinalizedVisualTargetsReview()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                failure = null; exitCode = 0; Steps.Clear();
                Application.logMessageReceived += Log;
                Steps.Push(Cases()); EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Key, false);
                profile?.Dispose(); selection?.Dispose();
                EditorApplication.Exit(exitCode);
            }
        };
    }

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a graphics-enabled batch editor.");
        Directory.CreateDirectory(Output);
        File.WriteAllText(Path.Combine(Output, "environment-review.txt"), "RUNNING\n");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Key, true); EditorApplication.EnterPlaymode();
    }

    private static IEnumerator Cases()
    {
        Application.runInBackground = true;
        Time.captureDeltaTime = 1f / 60f;
        string path = Path.Combine(Output, "profile-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, JsonUtility.ToJson(new AccountSave { gold = 100, potions = 3, bombs = 3, equipPotions = true, equipBombs = true }));
        profile = AccountProgression.UseDisposableProfile(path);
        selection = CharacterSelectionSettings.UseTemporarySelection("skeleton");
        foreach (int height in new[] { 1920, 2400 })
        {
            SetSize(1080, height); yield return Wait(.3f);
            Time.timeScale = 1; SceneManager.LoadScene("Game");
            yield return Until(() => RunSession.Current != null && RunSession.Current.Waves.IsWaveActive && RunSession.Current.Continuation.CanCapture, "game settles");
            foreach (var enemy in RunSession.Current.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
            var controls = Object.FindFirstObjectByType<RunControlsUI>(); controls.Close();
            yield return Wait(.6f); Time.timeScale = 0;
            Check(Screen.width == 1080 && Screen.height == height, "requested viewport");
            var issues = GameplayPixelLayoutValidator.Validate(Object.FindFirstObjectByType<GameplayPixelLayoutController>(), out string report);
            File.WriteAllText(Path.Combine(Output, height + "-layout.txt"), report + string.Join("\n", issues));
            Check(issues.Count == 0, "production layout: " + string.Join("; ", issues));
            ValidateEnvironment(Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ActiveEnvironment);
            yield return Shot(height + "-game");
            HudTypographyReview.ValidateHud();
            yield return HudTypographyReview.Feedback(height, Shot);
            var abilityEnergy = Object.FindFirstObjectByType<PlayerAbilityEnergy>();
            abilityEnergy.ResetEnergy(); yield return Wait(.8f);
            Check(GameObject.Find("EnergyFillMask").GetComponent<RectTransform>().rect.width == 0, "empty energy is fully cropped");
            yield return Shot(height + "-empty-energy");
            ValidateButtonStates(GameObject.Find("Settings").GetComponent<Button>());

            // Disposable rank fixtures use real EnemyDefinitions, prefabs and slot binding.
            var waves = RunSession.Current.Waves;
            waves.ClearCurrentWave(); yield return Wait(.1f);
            var slots = (EnemySlotUI[])typeof(WaveController).GetField("enemySlots", Flags).GetValue(waves);
            var definitions = AssetDatabase.FindAssets("t:EnemyDefinition").Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<EnemyDefinition>).Where(d => d != null && d.EnemyPrefab != null).ToArray();
            var categories = new[] { EnemyCategory.Normal, EnemyCategory.Special, EnemyCategory.Miniboss, EnemyCategory.Boss };
            for (int batch = 0; batch < 2; batch++)
            {
                waves.ClearCurrentWave(); yield return Wait(.1f);
                int count = batch == 0 ? 3 : 1;
                for (int i = 0; i < count; i++)
                {
                    var category = categories[batch == 0 ? i : 3];
                    var definition = definitions.First(d => d.Category == category);
                    var enemy = (EnemyActor)typeof(WaveController).GetMethod("CreateEnemy", Flags).Invoke(waves, new object[] { definition, slots[i], (GemType)i });
                    Check(enemy != null, "rank fixture " + category);
                    enemy.GetComponent<EnemyAutoAttack>()?.StopAttacking();
                    while (enemy.HasShield) enemy.TryTakeDamageWithoutFeedback(1);
                    if (i > 0) enemy.TryTakeDamageWithoutFeedback(enemy.MaxHealth * i / 3);
                }
                Time.timeScale = 1; yield return Wait(.7f); Time.timeScale = 0; yield return Wait(.3f);
                for (int i = 0; i < count; i++)
                {
                    var badge = slots[i].GetComponentsInChildren<Image>(true).FirstOrDefault(x => x.name == "RankBadge");
                    var expectedBadge = ModularHealthBarUI.LoadRankStyle(slots[i].CurrentEnemy.Definition.Category).Badge;
                    Check(badge != null && badge.sprite == expectedBadge, "independent category badge: " + slots[i].CurrentEnemy.Definition.Category + " actual=" + badge?.sprite?.name + " expected=" + expectedBadge?.name + " bars=" + slots[i].GetComponentsInChildren<ModularHealthBarUI>(true).Length);
                    var bar = slots[i].GetComponentInChildren<ModularHealthBarUI>();
                    var style = ModularHealthBarUI.LoadRankStyle(slots[i].CurrentEnemy.Definition.Category);
                    var generated = (RectTransform)bar.transform.Find("GeneratedModularHealthBar");
                    float capacity = generated.rect.width - style.FillInsetLeft - style.FillInsetRight;
                    float actual = ((RectTransform)generated.Find("CurrentHealthFill")).rect.width;
                    Check(actual == Mathf.Round(capacity * slots[i].CurrentEnemy.HealthNormalized), "independent HP fill tracks its bound actor");
                }
                var energy = Object.FindFirstObjectByType<PlayerAbilityEnergy>();
                energy.ResetEnergy(); if (batch == 0) energy.AddEnergy(energy.MaximumEnergy / 2); else energy.AddEnergy(energy.MaximumEnergy);
                yield return Shot(height + (batch == 0 ? "-ranks-partial" : "-boss-full"));
                float energyWidth = GameObject.Find("EnergyFillMask").GetComponent<RectTransform>().rect.width;
                Check(batch == 0 ? energyWidth > 0 && energyWidth < 107 : energyWidth == 107, "partial/full energy crop");
                if (batch == 1)
                {
                    var enemy = slots[0].CurrentEnemy;
                    enemy.GrantShield(10); yield return Wait(.2f);
                    Check(slots[0].IsShieldPresentationActive, "shield presentation remains separate");
                    var bar = slots[0].GetComponentInChildren<ModularHealthBarUI>();
                    Check(bar.transform.Find("GeneratedModularHealthBar").gameObject.activeSelf, "HP remains visible with shield");
                    Check(bar.GetComponent<ShieldBarUI>().IsVisible, "compact shield track active");
                    yield return Shot(height + "-shield");
                }
            }
            controls.OpenSettings(); yield return Shot(height + "-settings"); controls.Close(); yield return Wait(.1f);
            controls.OpenGuide(CombatGuide.Ability(RunSession.Current.Player)); yield return Shot(height + "-guide"); controls.Close(); yield return Wait(.1f);
            var upgrades = Object.FindFirstObjectByType<UpgradeChoiceUI>(FindObjectsInactive.Include);
            var choices = AssetDatabase.FindAssets("t:RunUpgradeDefinition").Take(3).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<RunUpgradeDefinition>).ToArray();
            if (upgrades != null) { Check(upgrades.Show(choices, _ => true), "upgrade cards open"); yield return Shot(height + "-upgrades"); upgrades.Hide(); }
            if (RunSession.Current.Player.HasShield) RunSession.Current.Player.TryTakeDamage(1000000);
            RunSession.Current.Player.TryTakeDamage(1000000);
            yield return Wait(3); yield return Shot(height + "-gameover");

            Time.timeScale = 1; SceneManager.LoadScene("MainMenu"); yield return Wait(.6f);
            var menu = Object.FindFirstObjectByType<MainMenuController>();
            ValidateButtonStates(Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.name == "PlayButton"));
            ValidateButtonStates(GameObject.Find("PracticeButton").GetComponent<Button>());
            yield return Shot(height + "-mainmenu");
            menu.ShowCharacterSelect(); yield return Shot(height + "-characters"); menu.ShowHome();
            menu.ShowGemMastery(); yield return Shot(height + "-mastery"); menu.ShowHome();
            menu.ShowShop(); yield return Shot(height + "-shop"); menu.ShowHome();
            menu.ShowChallenges(); yield return Shot(height + "-challenges");
        }
        profile.Dispose(); profile = null; selection.Dispose(); selection = null;
        File.WriteAllText(Path.Combine(Output, "environment-review.txt"), "PASS: actual integrated scenes at both viewports. Disposable rank fixtures; production slots, styles and game APIs. Screenshots require visual approval.\n" + Report);
    }

    public static void ValidateEnvironment(BattleEnvironmentRoot environment)
    {
        Check(environment != null && environment.name.StartsWith("Dungeon_Finalized"), "finalized variant selected");
        Check(environment.transform.localScale == Vector3.one, "environment scale one");
        var grid = environment.GetComponent<Grid>();
        Check(grid != null && grid.cellSize == new Vector3(1,1,0) && grid.cellGap == Vector3.zero, "one unit 2D Grid");
        string[] names = { "BackWall", "Architecture", "Floor", "BackDecor" };
        for (int i = 0; i < names.Length; i++)
        {
            var map = environment.transform.Find(names[i]).GetComponent<Tilemap>();
            var renderer = map.GetComponent<TilemapRenderer>();
            Check(map.transform.localScale == Vector3.one && renderer.sortingOrder == -100 + i && renderer.maskInteraction == SpriteMaskInteraction.VisibleInsideMask, "map ownership/order/mask " + names[i]);
            foreach (var tile in map.GetTilesBlock(map.cellBounds).OfType<Tile>().Distinct())
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tile.sprite));
                Check(tile.sprite.pixelsPerUnit == 64 && tile.sprite.vertices.Length == 4 && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "native terrain import");
            }
        }
        for (int row = 0; row < 6; row++) for (int col = 0; col < 8; col++)
        {
            var map = environment.transform.Find(row < 4 ? "BackWall" : "Floor").GetComponent<Tilemap>();
            var tile = map.GetTile<Tile>(new Vector3Int(col - 4, 3 - row));
            Check(tile != null && tile.sprite.name == "Cell" + (char)('A' + row) + (char)('A' + col), "baked tile mapping " + row + "," + col);
        }
        Check(environment.GetComponentsInChildren<Tilemap>().Length == 4, "only four authored tilemaps");
    }

    private static IEnumerator Shot(string name)
    {
        yield return Wait(.4f);
        Canvas.ForceUpdateCanvases();
        HudTypographyReview.ValidateText(name, Output);
        Check(!Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Any(t => t.enabled && !string.IsNullOrEmpty(t.text)), "runtime labels use TMP: " + name);
        foreach (var image in Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i => i.enabled && i.sprite != null && AssetDatabase.GetAssetPath(i.sprite).Contains("/Finalized/")))
            Check(image.sprite.texture.filterMode == FilterMode.Point, "UI Point import");
        ScreenCapture.CaptureScreenshot(Path.Combine(Output, name + ".png"));
        Report.AppendLine(name + ": captured actual scene.");
        yield return Wait(.4f);
    }

    private static void ValidateButtonStates(Button button)
    {
        Check(button != null && button.transition == Selectable.Transition.SpriteSwap, "sprite state control");
        Check(button.image.canvasRenderer.GetColor() == Color.white, "legacy tint does not darken sprite states");
        var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
        button.OnPointerEnter(pointer);
        Check(button.image.overrideSprite == button.spriteState.highlightedSprite, "highlight state");
        button.OnPointerDown(pointer);
        Check(button.image.overrideSprite == button.spriteState.pressedSprite, "pressed state");
        button.OnPointerUp(pointer); button.OnPointerExit(pointer);
        button.interactable = false;
        Check(button.image.overrideSprite == button.spriteState.disabledSprite, "disabled state");
        button.interactable = true; EventSystem.current.SetSelectedGameObject(null);
    }

    private static void Tick()
    {
        try
        {
            Check(failure == null, failure);
            if (Steps.Count == 0) { Finish(); return; }
            var step = Steps.Peek();
            if (!step.MoveNext()) Steps.Pop(); else if (step.Current is IEnumerator nested) Steps.Push(nested);
            EditorApplication.QueuePlayerLoopUpdate();
        }
        catch (Exception error) { exitCode = 1; failure = error.ToString(); Finish(); }
    }
    private static void Finish()
    {
        EditorApplication.update -= Tick; Application.logMessageReceived -= Log; Time.timeScale = 1;
        if (exitCode != 0) File.WriteAllText(Path.Combine(Output, "environment-review.txt"), "FAIL: " + failure);
        EditorApplication.ExitPlaymode();
    }
    private static void Log(string message, string trace, LogType type)
    {
        if (type == LogType.Exception && trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")) return;
        if (message.StartsWith("Pixel layout FAILED:")) { File.AppendAllText(Path.Combine(Output, "startup-layout.txt"), message + "\n"); return; }
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failure = message;
    }
    private static void Check(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); }
    private static IEnumerator Wait(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
    private static IEnumerator Until(Func<bool> test, string label) { float end = Time.realtimeSinceStartup + 40; while (!test()) { Check(Time.realtimeSinceStartup < end, label); yield return null; } }
    private static void SetSize(int width, int height)
    {
        var assembly = typeof(Editor).Assembly; var type = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        var groupType = type.GetProperty("currentGroupType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sizes);
        var group = type.GetMethod("GetGroup").Invoke(sizes, new[] { groupType });
        var size = Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"), new object[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType"), 1), width, height, "Finalized art review" });
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
        int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
        var viewType = assembly.GetType("UnityEditor.GameView"); var view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, count - 1);
        view.Show(); view.Focus(); view.Repaint();
    }
}
