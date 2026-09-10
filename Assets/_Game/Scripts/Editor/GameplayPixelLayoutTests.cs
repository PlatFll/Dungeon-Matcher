using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class GameplayPixelLayoutTests
{
    private const string Pending = "DungeonMatcher.PixelLayoutTests";
    private const string Output = ".utmp/PixelLayout";
    private static readonly Vector2Int[] Sizes = {
        new(540,960), new(720,1280), new(1080,1920), new(1080,2160),
        new(1080,2340), new(1080,2400), new(1080,2460), new(1440,3200) };
    private static int index;
    private static double deadline;
    private static int phase;
    private static int exitCode;
    private static int reloadStage;
    static GameplayPixelLayoutTests()
    {
        EditorApplication.playModeStateChanged += state => {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
            {
                index = 0; phase = 0; reloadStage = 0; deadline = EditorApplication.timeSinceStartup + 3;
                EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Pending, false))
            {
                SessionState.SetBool(Pending, false);
                if (Application.isBatchMode) EditorApplication.Exit(exitCode);
            }
        };
    }

    [MenuItem("Dungeon Matcher/Validation/Pixel Layout Math")]
    public static void MathTests()
    {
        int count = 0;
        foreach (Vector2Int size in Sizes)
        foreach (Vector4 insets in new[] { Vector4.zero, new Vector4(0,0,73,0), new Vector4(0,0,97,41), new Vector4(3,5,99,43) })
        {
            Rect safe = new Rect(insets.x, insets.w, size.x - insets.x - insets.y, size.y - insets.z - insets.w);
            var g = GameplayPixelLayoutController.Calculate(size.x, size.y, safe, new Vector2(544,544));
            Check(g.Fits, $"Fits {size} {safe}");
            Check(g.Scale >= 1 && GameplayPixelLayoutValidator.Integral(g.Scale), "Integer UI scale");
            Check(g.Viewport.xMin >= safe.xMin + 4*g.Scale && g.Viewport.yMin >= safe.yMin + 4*g.Scale &&
                g.Viewport.xMax <= safe.xMax - 4*g.Scale && g.Viewport.yMax <= safe.yMax - 4*g.Scale, "Safe insets");
            Check(g.Bottom.height == 176, "Native bottom height");
            Check(g.Top.height >= 220 && g.Top.height <= 320, "Bounded battle");
            float upperGap = g.Top.yMin - g.Board.yMax, lowerGap = g.Board.yMin - g.Bottom.yMax;
            Check(upperGap >= 6 && lowerGap >= 6 && Mathf.Abs(upperGap - lowerGap) <= 1, "Balanced minimum gaps");
            Check(g.Top.yMax == g.Viewport.height / g.Scale, "Top anchored below safe edge");
            Check(g.Board.xMin >= 0 && g.Board.xMax <= g.Viewport.width / g.Scale, "Board rectangle stays inside viewport");
            Check(g.Bottom.yMin == 0, "Bottom anchored to viewport");
            float maximum = Mathf.Min(g.Viewport.width, g.Viewport.height - (176 + 12 + 220)*g.Scale);
            Check(Mathf.Abs(g.BoardTexelRatio * 544 - maximum) < g.Scale + 0.01f, "Board fills available space");
            Check(g.BoardTexelRatio * 544 <= g.Board.width*g.Scale + 0.01f, "Board fits assigned width");
            count++;
        }
        Check(!GameplayPixelLayoutValidator.Integral(1.137f), "Intentional fractional ratio rejected");
        Debug.Log($"Pixel layout math PASSED: {count} screen/safe-area combinations.");
    }

    [MenuItem("Dungeon Matcher/Validation/Pixel Layout Play Mode")]
    public static void Run()
    {
        MathTests();
        Directory.CreateDirectory(Output);
        File.WriteAllText(Output + "/report.txt", "Pixel layout scene validation\n");
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/Game.unity");
        SetGameViewSize(Sizes[0]);
        SessionState.SetBool(Pending, true);
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < deadline) return;
        try
        {
            if (index == Sizes.Length * 4) { TestReloads(); return; }
            Vector2Int size = Sizes[index / 4];
            if (phase == 0)
            {
                SetGameViewSize(size);
                Vector4 inset = (index % 4) switch { 1 => new Vector4(0,0,73,0), 2 => new Vector4(0,0,97,41), 3 => new Vector4(3,5,99,43), _ => Vector4.zero };
                GameplayPixelLayoutController.ValidationSafeArea = new Rect(inset.x, inset.w,
                    size.x-inset.x-inset.y, size.y-inset.z-inset.w);
                if (index == 4)
                {
                    var waves = UnityEngine.Object.FindFirstObjectByType<WaveController>();
                    var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_Farmer.asset");
                    if (definition == null)
                    {
                        foreach (string guid in AssetDatabase.FindAssets("t:EnemyDefinition"))
                        {
                            definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                            if (definition != null && definition.EnemyPrefab != null) break;
                        }
                    }
                    Check(definition != null, "Multiple-enemy fixture definition exists");
                    Check(waves.TrySummonEnemy(definition, out _), "Second enemy spawned through production service");
                    Check(waves.TrySummonEnemy(definition, out _), "Third enemy spawned through production service");
                }
                phase = 1; deadline = EditorApplication.timeSinceStartup + 1;
                return;
            }
            Check(Screen.width == size.x && Screen.height == size.y, $"Native render size {Screen.width}x{Screen.height}, requested {size}");
            var layout = UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
            Check(layout != null, "Bootstrap installed layout");
            var errors = GameplayPixelLayoutValidator.Validate(layout, out string report);
            File.AppendAllText(Output + "/report.txt", $"Case {index}: {report}\n" + string.Join("\n", errors) + "\n");
            ScreenCapture.CaptureScreenshot($"{Output}/{size.x}x{size.y}-safe{index % 4}.png");
            Check(errors.Count == 0, string.Join("\n", errors));
            if (index == 4) { TestViolations(layout); AuditImports(layout); }
            foreach (var attack in UnityEngine.Object.FindObjectsByType<EnemyAutoAttack>(FindObjectsSortMode.None)) attack.StopAttacking();
            index++; phase = 0; deadline = EditorApplication.timeSinceStartup + 0.25;
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            File.AppendAllText(Output + "/report.txt", "FAILED: " + error);
            Finish(1);
        }
    }

    private static void TestReloads()
    {
        if (reloadStage == 0)
        {
            var gameOver = UnityEngine.Object.FindFirstObjectByType<GameOverPresentationController>();
            typeof(GameOverPresentationController).GetMethod("RetryCurrentGame", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(gameOver, null);
        }
        else if (reloadStage == 1)
        {
            CheckScene("Retry");
        }
        else if (reloadStage == 2) UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        else if (reloadStage == 3) UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        else if (reloadStage == 4) CheckScene("MainMenu to Game");
        else { Finish(0); return; }
        reloadStage++;
        deadline = EditorApplication.timeSinceStartup + 3;
    }

    private static void CheckScene(string label)
    {
        var layout = UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        Check(layout != null, label + " bootstrap");
        var errors = GameplayPixelLayoutValidator.Validate(layout, out string report);
        Check(errors.Count == 0, label + ": " + string.Join("\n", errors));
        File.AppendAllText(Output + "/report.txt", label + " PASSED\n" + report);
        ScreenCapture.CaptureScreenshot(Output + "/" + label.Replace(" ", "-") + ".png");
    }

    private static void TestViolations(GameplayPixelLayoutController layout)
    {
        Vector2 position = layout.BottomHud.anchoredPosition;
        layout.BottomHud.anchoredPosition += Vector2.down * 1000;
        Check(GameplayPixelLayoutValidator.Validate(layout, out _).Count > 0, "Outside-safe-area mutation detected");
        layout.BottomHud.anchoredPosition = position;
        layout.BottomHud.localScale = new Vector3(0.8f,0.7f,1);
        Check(GameplayPixelLayoutValidator.Validate(layout, out _).Count > 0, "Fractional transform mutation detected");
        layout.BottomHud.localScale = Vector3.one;
        var conflict = layout.BottomHud.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        Check(GameplayPixelLayoutValidator.Validate(layout, out _).Count > 0, "Duplicate ownership mutation detected");
        UnityEngine.Object.DestroyImmediate(conflict);
        RectTransform corner = (RectTransform)layout.BottomHud.Find("GeneratedBottomHudFrame/TopLeftCorner");
        Vector2 cornerPosition = corner.anchoredPosition;
        corner.anchoredPosition += Vector2.right * (0.25f / layout.Current.Scale);
        Check(GameplayPixelLayoutValidator.Validate(layout, out _).Exists(e => e.Contains("Fractional frame vertices")), "Off-grid corner detected");
        corner.anchoredPosition = cornerPosition;
        var cornerImage = corner.GetComponent<UnityEngine.UI.Image>();
        Sprite savedSprite = cornerImage.sprite;
        cornerImage.sprite = null;
        Check(GameplayPixelLayoutValidator.Validate(layout, out _).Exists(e => e.Contains("Missing frame piece")), "Missing sprite detected");
        cornerImage.sprite = savedSprite;
        RectTransform edge = (RectTransform)layout.BottomHud.Find("GeneratedBottomHudFrame/TopEdge");
        edge.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 15);
        Check(GameplayPixelLayoutValidator.Validate(layout, out _).Exists(e => e.Contains("Mismatched native frame thickness")), "Wrong edge thickness detected");
        edge.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 16);
        var board = UnityEngine.Object.FindFirstObjectByType<BoardLayoutController>();
        PropertyInfo ratio = typeof(BoardLayoutController).GetProperty("PhysicalTexelRatio");
        float savedRatio = board.PhysicalTexelRatio;
        ratio.SetValue(board, 1.137f);
        Check(GameplayPixelLayoutValidator.Validate(layout, out _).Exists(e => e.Contains("Board scale differs")), "Incorrect board ratio detected");
        ratio.SetValue(board, savedRatio);
        Check(GameplayPixelLayoutValidator.Validate(layout, out _).Count == 0, "All intentional violations restored");
        File.AppendAllText(Output + "/report.txt", "Intentional violations detected and restored.\n");
    }

    private static void Finish(int code)
    {
        exitCode = code;
        EditorApplication.update -= Tick;
        GameplayPixelLayoutController.ValidationSafeArea = null;
        File.AppendAllText(Output + "/report.txt", code == 0 ? "PASSED\n" : "FAILED\n");
        EditorApplication.ExitPlaymode();
    }

    private static void AuditImports(GameplayPixelLayoutController layout)
    {
        var paths = new System.Collections.Generic.HashSet<string>();
        foreach (UnityEngine.UI.Image image in layout.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            if (image.sprite != null) paths.Add(AssetDatabase.GetAssetPath(image.sprite.texture));
        foreach (SpriteRenderer renderer in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            if (renderer.sprite != null) paths.Add(AssetDatabase.GetAssetPath(renderer.sprite.texture));
        var report = new System.Text.StringBuilder();
        foreach (string path in paths)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;
            var android = importer.GetPlatformTextureSettings("Android");
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            bool compression = android.overridden ? android.textureCompression != TextureImporterCompression.Uncompressed : importer.textureCompression != TextureImporterCompression.Uncompressed;
            report.AppendLine($"{path}: filter={importer.filterMode}, mipmaps={importer.mipmapEnabled}, PPU={importer.spritePixelsPerUnit}, mesh={settings.spriteMeshType}, compression={compression}, AndroidOverride={android.overridden}, maxSize={(android.overridden ? android.maxTextureSize : importer.maxTextureSize)}");
            if (importer.filterMode != FilterMode.Point || importer.mipmapEnabled || compression || importer.spritePixelsPerUnit != 64)
                report.AppendLine("REVIEW: import differs from the default pixel-art policy; inspect relevance before editing.");
        }
        File.WriteAllText(Output + "/art-imports.txt", report.ToString());
    }

    private static void SetGameViewSize(Vector2Int size)
    {
        Assembly assembly = typeof(Editor).Assembly;
        Type sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        Type singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singleton.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        object currentGroup = sizesType.GetProperty("currentGroupType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sizes);
        object group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { currentGroup });
        Type sizeType = assembly.GetType("UnityEditor.GameViewSize");
        Type kind = assembly.GetType("UnityEditor.GameViewSizeType");
        object entry = Activator.CreateInstance(sizeType, new object[] { Enum.ToObject(kind, 1), size.x, size.y, "Pixel layout validation" });
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { entry });
        int total = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
        Type viewType = assembly.GetType("UnityEditor.GameView");
        EditorWindow view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, total-1);
        view.Show();
        view.Focus();
        view.Repaint();
    }
    private static void Check(bool success, string message) { if (!success) throw new Exception(message); }
}
