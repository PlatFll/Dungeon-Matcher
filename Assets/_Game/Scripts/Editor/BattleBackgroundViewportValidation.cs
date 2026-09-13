using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

// Run with -executeMethod BattleBackgroundViewportValidation.Run (graphics required).
// Oversized tiles deliberately cover the board, gaps and BottomHUD before masking.
public static class BattleBackgroundViewportValidation
{
    private static IEnumerator routine;
    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Run in an isolated batch editor.");
        routine = Cases();
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        try
        {
            if (routine.MoveNext()) { EditorApplication.QueuePlayerLoopUpdate(); return; }
            EditorApplication.update -= Tick;
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.update -= Tick;
            EditorApplication.Exit(1);
        }
    }

    private static IEnumerator Cases()
    {
        foreach (int height in new[] { 1920, 2400 })
        {
            IEnumerator test = Validate(1080, height);
            while (test.MoveNext()) yield return test.Current;
        }
        Debug.Log("Battle viewport validation PASSED: 1080x1920 and 1080x2400.");
    }

    private static IEnumerator Validate(int width, int height)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Camera").AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = height / 128f;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        var target = new RenderTexture(width, height, 24);
        camera.targetTexture = target;
        var canvas = new GameObject("Canvas", typeof(RectTransform)).AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        canvas.transform.localScale = Vector3.one / 64f;
        var geometry = GameplayPixelLayoutController.Calculate(width, height,
            new Rect(0, 0, width, height), new Vector2(576, 576));
        Check(geometry.Fits, "portrait geometry fits");
        var area = new GameObject("TopHUD", typeof(RectTransform)).GetComponent<RectTransform>();
        area.SetParent(canvas.transform, false);
        area.sizeDelta = geometry.Top.size * geometry.Scale;
        Vector2 center = geometry.Viewport.min + geometry.Top.center * geometry.Scale;
        area.localPosition = center - new Vector2(width, height) / 2;
        // World-space fixture uses physical pixels, so express the same logical inset.
        area.localScale = Vector3.one * geometry.Scale;
        area.sizeDelta = geometry.Top.size;
        var floor = new GameObject("BattleFloorAnchor", typeof(RectTransform)).GetComponent<RectTransform>();
        floor.SetParent(area, false);
        floor.localPosition = new Vector3(0, area.rect.yMin + 58.123f, 0);
        var root = new GameObject("BattleBackground");
        root.SetActive(false);
        var controller = root.AddComponent<BattleBackgroundTilemapController>();
        Set(controller, "battleArea", area);
        Set(controller, "battleFloorAnchor", floor);
        Set(controller, "worldCamera", camera);
        var grid = new GameObject("Grid", typeof(Grid));
        grid.transform.SetParent(root.transform, false);
        var layer = new GameObject("Tiles", typeof(Tilemap), typeof(TilemapRenderer));
        layer.transform.SetParent(grid.transform, false);
        var legacyMap = layer.GetComponent<Tilemap>();
        var legacyRenderer = layer.GetComponent<TilemapRenderer>();
        legacyRenderer.sortingOrder = -100;
        var prefab = Resources.Load<GameObject>("BattleEnvironments/Dungeon_Default");
        Check(prefab != null, "default environment prefab exists");
        var environment = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
        var map = environment.transform.Find("BackWall").GetComponent<Tilemap>();
        var renderer = map.GetComponent<TilemapRenderer>();
        var texture = new Texture2D(64, 64);
        var colors = new Color[64 * 64];
        Array.Fill(colors, Color.red);
        texture.SetPixels(colors);
        texture.Apply();
        var sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one / 2, 64);
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        for (int y = -45; y <= 45; y++)
            for (int x = -20; x <= 20; x++)
            {
                map.SetTile(new Vector3Int(x, y), tile);
                // Keep painted legacy content to detect accidental double rendering.
                legacyMap.SetTile(new Vector3Int(x, y), tile);
            }
        root.SetActive(true);
        Check(controller.TryUseBackground(area), "64 PPU map available");
        Check(controller.ActiveEnvironment == environment.GetComponent<BattleEnvironmentRoot>(),
            "authored prefab selected");
        Check(root.GetComponentsInChildren<BattleEnvironmentRoot>(true).Length == 1,
            "exactly one environment instance");
        Check(legacyRenderer.forceRenderingOff && !renderer.forceRenderingOff,
            "only prefab tiles render");
        typeof(BattleBackgroundTilemapController).GetMethod("Align", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(controller, null);
        Check(root.transform.lossyScale == Vector3.one && map.transform.lossyScale == Vector3.one,
            "world map scale remains one");
        Check(Mathf.Abs(root.transform.position.y - floor.position.y) < 0.00001f, "floor equals tile Y=0");
        Check(Mathf.Abs(root.transform.position.x * 64 - Mathf.Round(root.transform.position.x * 64)) < 0.0001f &&
            Mathf.Abs(root.transform.position.y * 64 - Mathf.Round(root.transform.position.y * 64)) < 0.0001f,
            "map origin snapped to 64 PPU");
        Check(renderer.maskInteraction == SpriteMaskInteraction.VisibleInsideMask,
            "tile renderer masked");
        // Exercise the production board-mask setup, not a duplicate of its settings.
        var boardVisuals = new GameObject("BoardFixture").AddComponent<BoardVisuals>();
        Set(boardVisuals, "board", boardVisuals.GetComponent<BoardController>());
        Set(boardVisuals, "runtimeSquareSprite", sprite);
        typeof(BoardVisuals).GetMethod("CreateBoardMask", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(boardVisuals, null);
        var boardMask = boardVisuals.GetComponentInChildren<SpriteMask>();
        var board = new GameObject("BoardSprite").AddComponent<SpriteRenderer>();
        var boardSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one / 2, 1);
        board.sprite = boardSprite;
        board.color = Color.blue;
        board.sortingLayerName = "Gems";
        board.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        // SpriteMask sorting state is prepared by Unity's frame loop, not Camera.Render.
        for (int frame = 0; frame < 5; frame++) yield return null;
        SortingGroup.UpdateAllSortingGroups();
        camera.Render();
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        var capture = new Texture2D(width, height, TextureFormat.RGB24, false);
        capture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        capture.Apply();
        RenderTexture.active = previous;
        var mask = root.GetComponentInChildren<SpriteMask>();
        Vector3 min = camera.WorldToScreenPoint(mask.transform.position - mask.transform.localScale / 2);
        Vector3 max = camera.WorldToScreenPoint(mask.transform.position + mask.transform.localScale / 2);
        System.IO.Directory.CreateDirectory(".utmp/BattleViewport");
        System.IO.File.WriteAllBytes($".utmp/BattleViewport/{width}x{height}.png", capture.EncodeToPNG());
        int redCount = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Color pixel = capture.GetPixel(x, y);
                if (pixel.r < 0.5f) continue;
                redCount++;
                Check(x + 0.5f >= min.x - 1 && x + 0.5f <= max.x + 1 &&
                    y + 0.5f >= min.y - 1 && y + 0.5f <= max.y + 1,
                    $"no background outside interior: pixel {x},{y}; bounds {min} to {max}");
            }
        Check(redCount > 1000, "background actually rendered");
        Check(capture.GetPixel(width / 2, height / 2).b > 0.5f, "board still visible inside its own mask");
        Check(board.maskInteraction == SpriteMaskInteraction.VisibleInsideMask && board.sortingLayerName == "Gems",
            "board mask and sorting unchanged");
        Debug.Log($"Battle viewport {width}x{height}: {redCount} interior pixels, no leakage; scale, PPU and floor passed.");
        camera.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(capture);
        UnityEngine.Object.DestroyImmediate(tile);
        Set(boardVisuals, "runtimeSquareSprite", null);
        UnityEngine.Object.DestroyImmediate(sprite);
        UnityEngine.Object.DestroyImmediate(boardSprite);
        UnityEngine.Object.DestroyImmediate(texture);
    }

    private static void Set(object target, string field, object value) => target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Battle viewport: " + message);
    }
}
