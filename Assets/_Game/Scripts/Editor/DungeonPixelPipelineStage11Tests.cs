using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class DungeonPixelPipelineStage11Tests
{
    const string Root = "ArtSource/DungeonPixelLab";
    const string Art = "Assets/_Game/Art/DungeonPixelLab";
    const string Scene = "Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity";
    static void SetSize(int w, int h) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { new Vector2Int(w, h) });
    static string Hash(string path) { using (var h = System.Security.Cryptography.SHA256.Create()) return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))); }
    static void Prepare(bool board)
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string family in board ? new[] { "BoardFrame" } : new string[0])
        foreach (string file in Directory.GetFiles(Art + "/" + family, "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(file.Replace('\\', '/'));
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp; importer.alphaIsTransparency = false; importer.ClearPlatformTextureSettings("Android");
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        var scene = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
        var p = Object.FindFirstObjectByType<DungeonSideViewArtPreview>();
        p.name = board ? "Dungeon PixelLab Stage 11 board candidate" : "Dungeon PixelLab corrected portrait candidate";
        string[] families = { "Wall", "PortraitRecess", "FloorTop", "Fascia", "Gate", "Support", "Torch", "Hanging", "Clutter", "PortraitFrame" };
        if (board) families = families.Concat(new[] { "BoardFrame" }).ToArray();
        p.modules = families.SelectMany(d => Directory.GetFiles(Art + "/" + d, "*.png")).Select(f => AssetDatabase.LoadAssetAtPath<Sprite>(f.Replace('\\', '/'))).ToArray();
        p.placementManifest = AssetDatabase.LoadAssetAtPath<TextAsset>(Art + (board ? "/Stage11BoardPlacement.json" : "/Stage10CorrectedPlacement.json"));
        p.width = 360; p.height = 640; p.safeArea = new Rect(0, 0, 360, 640); p.Rebuild();
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == Scene), Is.False);
    }
    static IEnumerator Capture(string folder, string name)
    {
        Canvas.ForceUpdateCanvases(); yield return null; yield return null;
        string path = Path.GetFullPath(Root + "/Previews/" + folder + "/" + name + ".png");
        if (File.Exists(path)) File.Delete(path);
        ScreenCapture.CaptureScreenshot(path); float end = Time.realtimeSinceStartup + 20;
        while (!File.Exists(path)) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(end)); yield return null; }
        yield return null; yield return null;
    }
    static RectTransform Portrait(DungeonSideViewArtPreview p) => (RectTransform)p.Content.Find("05 Top UI shells/Portrait panel");
    static void ValidateFrame(RectTransform panel)
    {
        var frame = (RectTransform)panel.Find("ModularFrame");
        var c = frame.Find("TopLeftCorner").GetComponent<Image>().sprite.rect.width;
        float w = panel.rect.width, h = panel.rect.height;
        foreach (var im in frame.GetComponentsInChildren<Image>())
        {
            var r = im.rectTransform;
            Assert.That(r.anchoredPosition.x, Is.EqualTo(Mathf.Round(r.anchoredPosition.x)).Within(.001f));
            Assert.That(r.anchoredPosition.y, Is.EqualTo(Mathf.Round(r.anchoredPosition.y)).Within(.001f));
            Assert.That(r.localScale, Is.EqualTo(Vector3.one)); Assert.That(im.sprite.pixelsPerUnit, Is.EqualTo(64));
            Assert.That(im.color, Is.EqualTo(Color.white));
            if (im.type == Image.Type.Simple) Assert.That(r.rect.size, Is.EqualTo(im.sprite.rect.size));
            else Assert.That(im.pixelsPerUnitMultiplier, Is.EqualTo(1));
            Assert.That(r.anchoredPosition.x, Is.GreaterThanOrEqualTo(0)); Assert.That(-r.anchoredPosition.y, Is.GreaterThanOrEqualTo(0));
            Assert.That(r.anchoredPosition.x + r.rect.width, Is.LessThanOrEqualTo(w));
            Assert.That(-r.anchoredPosition.y + r.rect.height, Is.LessThanOrEqualTo(h));
        }
        var con = frame.Find("NativeConnectors");
        var active = con.GetComponentsInChildren<Image>();
        float left = (c - 1) / 2, right = w - 1 - left;
        var leftRows = active.Where(i => Mathf.Approximately(i.rectTransform.anchoredPosition.x + (i.sprite.rect.width - 1) / 2, left)).ToArray();
        var rightRows = active.Where(i => Mathf.Approximately(i.rectTransform.anchoredPosition.x + (i.sprite.rect.width - 1) / 2, right)).ToArray();
        Assert.That(leftRows.Select(i => i.rectTransform.anchoredPosition.y), Is.EqualTo(rightRows.Select(i => i.rectTransform.anchoredPosition.y)));
        if (leftRows.Length > 0)
        {
            var middle = leftRows[leftRows.Length / 2].rectTransform;
            float middleCenter = -middle.anchoredPosition.y + (middle.rect.height - 1) / 2;
            Assert.That(middleCenter, Is.EqualTo((h - 1) / 2).Within(.5f));
        }
        var mask = (RectTransform)panel.Find("InteriorMask");
        var fill = mask.Find("OverfilledBacking") as RectTransform;
        if (fill)
        {
            Assert.That(fill.offsetMin.x, Is.LessThan(0)); Assert.That(fill.offsetMin.y, Is.LessThan(0));
            Assert.That(fill.offsetMax.x, Is.GreaterThan(0)); Assert.That(fill.offsetMax.y, Is.GreaterThan(0));
            Assert.That(mask.GetComponent<RectMask2D>(), Is.Not.Null);
            Assert.That(mask.rect.size, Is.EqualTo(new Vector2(w - 20, h - 20)));
        }
    }
    static IEnumerator CoverageProbe(DungeonSideViewArtPreview p, RectTransform panel, string name)
    {
        // Isolate the panel so every interior pixel can be checked for coverage and
        // every outside pixel for leakage, including the former uncovered bottom strip.
        var disabled = p.Content.Cast<Transform>().Where(t => t.name != "05 Top UI shells").Concat(panel.parent.Cast<Transform>().Where(t => t != panel)).ToArray();
        foreach (var t in disabled) t.gameObject.SetActive(false);
        var fill = panel.Find("InteriorMask/OverfilledBacking").GetComponent<Image>();
        Sprite original = fill.sprite; fill.sprite = null; fill.type = Image.Type.Simple; fill.color = Color.cyan;
        yield return Capture("Stage10Correction", name);
        var texture = new Texture2D(2, 2); texture.LoadImage(File.ReadAllBytes(Root + "/Previews/Stage10Correction/" + name + ".png"));
        var bounds = GameplayPixelLayoutController.ScreenRect((RectTransform)panel.Find("InteriorMask"));
        int cyan = 0;
        var pixels = texture.GetPixels32();
        for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
        {
            Color32 color = pixels[y * texture.width + x]; bool isCyan = color.r == 0 && color.g == 255 && color.b == 255;
            bool inside = bounds.Contains(new Vector2(x + .5f, y + .5f));
            Assert.That(isCyan, Is.EqualTo(inside), "Coverage/leak at " + x + "," + y); if (isCyan) cyan++;
        }
        Assert.That(cyan, Is.EqualTo(Mathf.RoundToInt(bounds.width * bounds.height)));
        Object.Destroy(texture); fill.sprite = original; fill.type = Image.Type.Tiled; fill.color = Color.white;
        foreach (var t in disabled) t.gameObject.SetActive(true);
    }
    static IEnumerator CharacterPreview(DungeonSideViewArtPreview p, string folder, string name)
    {
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false); tex.LoadImage(File.ReadAllBytes("Docs/ArtDirection/Audit2026-10/media/skeleton-still.png")); tex.filterMode = FilterMode.Point;
        var sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64, 0, SpriteMeshType.FullRect);
        var go = new GameObject("Temporary unchanged Rattlebones readability specimen", typeof(RectTransform), typeof(Image));
        var r = (RectTransform)go.transform; r.SetParent(p.Content, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(26, -116); r.sizeDelta = new Vector2(64, 64); go.GetComponent<Image>().sprite = sprite;
        yield return Capture(folder, name); go.SetActive(false); Object.Destroy(go); Object.Destroy(sprite); Object.Destroy(tex);
    }
    [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }
    [UnityTest, Explicit("Corrected portrait geometry, actual masked fill coverage and resize regression.")]
    public IEnumerator PortraitCorrection()
    {
        Prepare(false); SetSize(360, 640); yield return new EnterPlayMode();
        foreach (int scale in new[] { 1, 3 })
        {
            SetSize(360 * scale, 640 * scale); yield return new WaitForSecondsRealtime(.4f);
            var p = Object.FindFirstObjectByType<DungeonSideViewArtPreview>(); p.width = Screen.width; p.height = Screen.height; p.safeArea = new Rect(0, 0, p.width, p.height); p.Rebuild(); yield return null;
            var panel = Portrait(p); ValidateFrame(panel);
            yield return CharacterPreview(p, "Stage10Correction", "Portrait-" + scale + "x");
            foreach (var size in new[] { new Vector2(99, 190), new Vector2(100, 191), new Vector2(119, 231), new Vector2(60, 90) })
            {
                panel.sizeDelta = size; yield return null; yield return null; Canvas.ForceUpdateCanvases(); ValidateFrame(panel);
                yield return CoverageProbe(p, panel, "Coverage-" + size.x + "x" + size.y + "-" + scale + "x");
            }
            panel.sizeDelta = new Vector2(20, 20); yield return null; yield return null;
            Assert.That(panel.Find("ModularFrame").GetComponentsInChildren<Image>().All(i => !i.enabled), Is.True);
            panel.sizeDelta = new Vector2(99, 190); yield return null; yield return null; ValidateFrame(panel);
            Assert.That(panel.Find("ModularFrame").GetComponentsInChildren<Image>().All(i => i.enabled), Is.True);
            yield return Capture("Stage10Correction", "Restored-" + scale + "x");
        }
        yield return new ExitPlayMode();
    }
    [UnityTest, Explicit("Stage11 empty board and all prior approved art in the isolated scene.")]
    public IEnumerator BoardPreview()
    {
        File.WriteAllText(Root + "/Diagnostics/Stage11/GameScene.sha256", Hash("Assets/_Game/Scenes/Game.unity"));
        File.WriteAllText(Root + "/Diagnostics/Stage11/BuildSettings.sha256", Hash("ProjectSettings/EditorBuildSettings.asset"));
        Prepare(true); SetSize(360, 640); yield return new EnterPlayMode();
        foreach (var screen in new[] { new Vector2Int(360, 640), new Vector2Int(1080, 1920), new Vector2Int(1080, 2400) })
        {
            SetSize(screen.x, screen.y); yield return new WaitForSecondsRealtime(.4f);
            var p = Object.FindFirstObjectByType<DungeonSideViewArtPreview>(); p.width = Screen.width; p.height = Screen.height;
            p.safeArea = screen.y == 2400 ? new Rect(0, 90, 1080, 2220) : new Rect(0, 0, p.width, p.height); p.Rebuild(); yield return null;
            ValidateFrame(Portrait(p));
            var board = (RectTransform)p.Content.Find("05 Top UI shells/Board panel"); ValidateFrame(board);
            var cells = board.Find("InteriorMask").GetComponentsInChildren<Image>(); Assert.That(cells.Length, Is.EqualTo(64));
            Assert.That(board.anchoredPosition.y, Is.LessThan(-214));
            foreach (var cell in cells)
            {
                Assert.That(cell.rectTransform.rect.size, Is.EqualTo(new Vector2(35, 35)));
                var b = GameplayPixelLayoutController.ScreenRect(cell.rectTransform);
                Assert.That(b.width, Is.EqualTo(35 * p.PixelScale).Within(.01));
                Assert.That(b.x, Is.EqualTo(Mathf.Round(b.x)).Within(.01)); Assert.That(b.y, Is.EqualTo(Mathf.Round(b.y)).Within(.01));
            }
            Assert.That(cells.Select(i => i.rectTransform.anchoredPosition.x).Distinct().Count(), Is.EqualTo(8));
            Assert.That(cells.Select(i => i.rectTransform.anchoredPosition.y).Distinct().Count(), Is.EqualTo(8));
            string[] forbidden = { "BoardController", "EnemyActor", "PlayerActor", "RunSession", "EventSystem" };
            Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None).Any(c => c && forbidden.Contains(c.GetType().Name)), Is.False);
            foreach (var r in p.Pieces)
            {
                var i = r.GetComponent<Image>(); var b = GameplayPixelLayoutController.ScreenRect(r);
                Assert.That(b.width, Is.EqualTo(i.sprite.rect.width * p.PixelScale).Within(.01)); Assert.That(b.height, Is.EqualTo(i.sprite.rect.height * p.PixelScale).Within(.01));
            }
            yield return Capture("Stage11", "EmptyBoard-" + screen.x + "x" + screen.y);
            yield return CharacterPreview(p, "Stage11", "Readability-" + screen.x + "x" + screen.y);
        }
        yield return new ExitPlayMode();
        Assert.That(Hash("Assets/_Game/Scenes/Game.unity"), Is.EqualTo(File.ReadAllText(Root + "/Diagnostics/Stage11/GameScene.sha256"))); Assert.That(Hash("ProjectSettings/EditorBuildSettings.asset"), Is.EqualTo(File.ReadAllText(Root + "/Diagnostics/Stage11/BuildSettings.sha256")));
    }
}
