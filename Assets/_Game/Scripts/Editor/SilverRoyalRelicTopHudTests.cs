using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class SilverRoyalRelicTopHudTests
{
    const string Root = "ArtSource/SilverRoyalRelicUI/Stage02";
    const string AudioSnapshot = "SilverRoyalRelic.Stage02.AudioMute";
    static SilverRoyalRelicTopHudPreview preview;
    static void Size(int w, int h) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(w, h) });
    static IEnumerator Until(Func<bool> ready)
    {
        float end = Time.realtimeSinceStartup + 60;
        while (!ready()) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(end)); yield return null; }
    }
    static IEnumerator Wait(float seconds)
    { float end = Time.realtimeSinceStartup + seconds; yield return Until(() => Time.realtimeSinceStartup >= end); }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        preview?.Dispose(); preview = null; SceneManager.sceneLoaded -= SeedScene;
        Time.timeScale = 1; RunLaunchOptions.StartingZone = null;
        GameplayPixelLayoutController.ValidationSafeArea = null;
        if (Application.isPlaying) yield return new ExitPlayMode();
        EditorUtility.audioMasterMute = SessionState.GetBool(AudioSnapshot, EditorUtility.audioMasterMute);
        SessionState.EraseBool(AudioSnapshot);
    }
    [UnityTest, Explicit] public IEnumerator Stage02()
    {
        SessionState.SetBool(AudioSnapshot, EditorUtility.audioMasterMute);
        EditorUtility.audioMasterMute = true; Size(720, 1280);
        Directory.CreateDirectory(Root + "/Preview"); Directory.CreateDirectory(Root + "/QA");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        using (CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        foreach (var zone in new[] { "dungeon", "drowned-court" })
        {
            string profile = Path.GetFullPath(".utmp/SilverRelic/" + Guid.NewGuid() + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(profile));
            File.WriteAllText(profile, JsonUtility.ToJson(new AccountSave { potions = 3, bombs = 3, equipPotions = true, equipBombs = true }));
            using (AccountProgression.UseDisposableProfile(profile))
            {
                RunLaunchOptions.StartingZone = zone; SceneManager.sceneLoaded += SeedScene;
                SceneManager.LoadScene("Game"); yield return null; SceneManager.sceneLoaded -= SeedScene;
                yield return Until(() => RunSession.Current?.Continuation?.CanCapture == true && RunSession.Current.Waves.IsWaveActive);
                yield return Until(() => RunSession.Current.Waves.ActiveEnemies.All(e => e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning != true));
                var controls = Object.FindFirstObjectByType<RunControlsUI>();
                var layout = Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                var cases = zone == "dungeon" ? new[] { new Vector3Int(720,1280,0), new Vector3Int(360,640,0), new Vector3Int(1080,1920,0), new Vector3Int(1080,2400,1) }
                    : new[] { new Vector3Int(1080,2340,0) };
                foreach (var size in cases)
                {
                    preview?.Dispose(); preview = null;
                    GameplayPixelLayoutController.ValidationSafeArea = null;
                    Size(size.x, size.y); yield return Until(() => UnityEngine.Device.Screen.width == size.x && UnityEngine.Device.Screen.height == size.y);
                    GameplayPixelLayoutController.ValidationSafeArea = size.z == 1 ? new Rect(24,96,size.x-48,size.y-216) : (Rect?)null;
                    yield return Wait(.5f); Canvas.ForceUpdateCanvases();
                    string key = zone + "-" + size.x + "x" + size.y + (size.z == 1 ? "-safe" : "");
                    var wave = layout.TopHud.Find("WaveTracker").GetComponent<Image>();
                    var label = wave.GetComponentInChildren<TMP_Text>();
                    Assert.That(label.text, Is.EqualTo("WAVE " + RunSession.Current.Waves.CurrentWave));
                    var before = Snapshot(layout, controls);
                    yield return Capture(key + "-before");
                    preview = new SilverRoyalRelicTopHudPreview(layout, controls);
                    yield return Wait(.15f); preview.Apply(); Canvas.ForceUpdateCanvases();
                    var after = Snapshot(layout, controls);
                    Assert.That(after, Is.EqualTo(before), "Existing layout, board, floor line and bottom controls must remain exact");
                    var errors = GameplayPixelLayoutValidator.Validate(layout, out var report);
                    Assert.That(errors, Is.Empty, report);
                    ValidateControls(layout, controls, wave);
                    File.WriteAllText(Root + "/QA/" + key + ".txt", report + "\nLayout/board/floor/bottom controls unchanged. Settings and ornaments use whole native pixels.\n" + after);
                    yield return Capture(key + "-after");
                    string oldText = label.text;
                    foreach (string sample in new[] { "WAVE 1", "WAVE 99999" })
                    {
                        label.text = sample; PixelTextFitter.ApplyNativeMenu(label,48); label.ForceMeshUpdate();
                        Assert.That(label.isTextOverflowing, Is.False, sample);
                        Assert.That(label.textBounds.size.x, Is.LessThanOrEqualTo(label.rectTransform.rect.width + .1f));
                        Assert.That(label.fontSize * layout.Current.Scale / 16,
                            Is.EqualTo(Mathf.Round(label.fontSize * layout.Current.Scale / 16)).Within(.001), "Whole physical glyph pixels");
                    }
                    label.text = oldText; PixelTextFitter.ApplyNativeMenu(label,48);
                    Time.timeScale = 1; controls.SettingsButton.onClick.Invoke(); yield return null;
                    Assert.That(controls.IsModalOpen, Is.True); Assert.That(Time.timeScale, Is.Zero);
                    controls.Close(); yield return null; Assert.That(controls.IsModalOpen, Is.False); Assert.That(Time.timeScale, Is.EqualTo(1));
                    preview.Dispose(); preview = null;
                    Assert.That(AssetDatabase.GetAssetPath(wave.sprite), Does.Contain("/Stone/WaveBody.png"), "Preview is reversible");
                }
            }
        }
        yield return new ExitPlayMode();
    }
    static IEnumerator Capture(string name)
    {
        string path = Path.GetFullPath(Root + "/Preview/" + name + ".png");
        ScreenCapture.CaptureScreenshot(path); yield return Wait(.2f);
        Assert.That(File.Exists(path), Is.True, name);
    }
    static string Snapshot(GameplayPixelLayoutController layout, RunControlsUI controls)
    {
        var feet = layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s => s.CurrentEnemy)
            .Select(s => BattleFloorAnchor.EnemyFeetScreenY(s).ToString("R")).ToArray();
        var cells = RunSession.Current.Board.GetComponentsInChildren<SpriteRenderer>().Where(s => s.name.StartsWith("CellTile"))
            .Select(s => s.name + ":" + s.bounds + ":" + AssetDatabase.GetAssetPath(s.sprite)).OrderBy(s => s).ToArray();
        var bottom = layout.BottomHud.GetComponentsInChildren<Image>().Select(s => s.name + ":" + GameplayPixelLayoutController.ScreenRect(s.rectTransform)
            + ":" + AssetDatabase.GetAssetPath(s.sprite)).OrderBy(s => s).ToArray();
        return JsonUtility.ToJson(layout.Current) + "\n" + string.Join("|", feet) + "\n" + string.Join("|",cells)
            + "\n" + string.Join("|",bottom) + "\n" + GameplayPixelLayoutController.ScreenRect(controls.SettingsButton.image.rectTransform)
            + "\n" + GameplayPixelLayoutController.ScreenRect(layout.TopHud.Find("WaveTracker") as RectTransform);
    }
    static void ValidateControls(GameplayPixelLayoutController layout, RunControlsUI controls, Image wave)
    {
        Assert.That(wave.type, Is.EqualTo(Image.Type.Tiled)); Assert.That(wave.sprite.border, Is.EqualTo(new Vector4(20,10,20,10)));
        Assert.That(wave.rectTransform.rect.height, Is.EqualTo(48)); Assert.That(wave.pixelsPerUnitMultiplier, Is.EqualTo(1));
        Assert.That(controls.SettingsButton.image.rectTransform.rect.size, Is.EqualTo(new Vector2(56,56)));
        Assert.That(controls.SettingsButton.image.sprite.name, Is.EqualTo("SettingsFrame"));
        var pieces = new[] { controls.SettingsButton.image, controls.SettingsButton.transform.Find("RelicPreviewGear").GetComponent<Image>(),
            wave.transform.Find("RelicPreviewDiamond").GetComponent<Image>(), wave.transform.Find("SharedWavePendant").GetComponent<Image>() };
        foreach (var piece in pieces)
        {
            var rect = GameplayPixelLayoutController.ScreenRect(piece.rectTransform);
            Assert.That(rect.width / piece.sprite.rect.width, Is.EqualTo(layout.Current.Scale).Within(.001), piece.name);
            Assert.That(rect.height / piece.sprite.rect.height, Is.EqualTo(layout.Current.Scale).Within(.001), piece.name);
            Assert.That(rect.xMin, Is.EqualTo(Mathf.Round(rect.xMin)).Within(.001), piece.name);
            Assert.That(rect.yMin, Is.EqualTo(Mathf.Round(rect.yMin)).Within(.001), piece.name);
            Assert.That(piece.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(rect.xMin, Is.GreaterThanOrEqualTo(layout.Current.Safe.xMin));
            Assert.That(rect.xMax, Is.LessThanOrEqualTo(layout.Current.Safe.xMax));
            Assert.That(rect.yMax, Is.LessThanOrEqualTo(layout.Current.Safe.yMax));
        }
        Assert.That(pieces.Skip(1).All(im => !im.raycastTarget), Is.True, "Ornaments never intercept input");
        var wr = GameplayPixelLayoutController.ScreenRect(wave.rectTransform);
        Assert.That(wr.Overlaps(GameplayPixelLayoutController.ScreenRect(controls.SettingsButton.image.rectTransform)), Is.False);
        foreach (var actor in RunSession.Current.Waves.ActiveEnemies)
        {
            var view = actor.GetComponent<EnemyUnifiedIntentView>();
            var ar = GameplayPixelLayoutController.ScreenRect(view.ActionRoot);
            foreach (var image in pieces) Assert.That(ar.Overlaps(GameplayPixelLayoutController.ScreenRect(image.rectTransform)), Is.False, "Intent clearance");
        }
    }
    static void SeedScene(Scene scene, LoadSceneMode mode)
    {
        UnityEngine.Random.InitState(3101);
        var waves = Object.FindFirstObjectByType<WaveController>();
        typeof(WaveController).GetField("encounterSeed",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(waves,3101);
        typeof(WaveController).GetField("encounterRandom",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(waves,null);
    }
}
