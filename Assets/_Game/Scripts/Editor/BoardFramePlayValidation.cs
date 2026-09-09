using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Uses the real Game scene and production resolution coroutines. All gem
// fixtures exist only in Play Mode. Screenshots still require visual review.
[InitializeOnLoad]
public static class BoardFramePlayValidation
{
    private const string Pending = "DungeonMatcher.FrameValidation";
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private static BoardController board;
    private static string output;
    private static readonly StringBuilder report = new StringBuilder();
    private static int cascades;

    static BoardFramePlayValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
            SessionState.SetBool(Pending, false);
            board = UnityEngine.Object.FindFirstObjectByType<BoardController>();
            if (board == null) throw new InvalidOperationException("Open the Game scene first.");
            output = Path.GetFullPath($".utmp/FrameVerification/{DateTime.Now:yyyyMMdd-HHmmss}-{Screen.width}x{Screen.height}");
            Directory.CreateDirectory(output);
            report.Clear();
            cascades = 0;
            board.BoardClearResolved += context =>
            {
                if (context.CascadeDepth > 0) cascades++;
                report.AppendLine($"Clear {context.Source}, depth {context.CascadeDepth}, gems {context.GemCount}");
            };
            board.StartCoroutine(RunChecked());
        };
    }

    [MenuItem("Dungeon Matcher/Validation/Board Frame Play Mode %#F8")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        SessionState.SetBool(Pending, true);
        EditorApplication.EnterPlaymode();
    }

    private static IEnumerator RunChecked()
    {
        var stack = new System.Collections.Generic.Stack<IEnumerator>();
        stack.Push(Scenarios());
        while (stack.Count > 0)
        {
            object current;
            try
            {
                var step = stack.Peek();
                if (!step.MoveNext()) { stack.Pop(); continue; }
                current = step.Current;
            }
            catch (Exception error)
            {
                report.AppendLine("FAILED: " + error);
                File.WriteAllText(Path.Combine(output, "report.txt"), report.ToString());
                Debug.LogException(error);
                yield break;
            }
            if (current is IEnumerator nested) stack.Push(nested);
            else yield return current;
        }
        report.AppendLine($"PASSED renderer checks and production swaps. Cascaded clears: {cascades}. Visually review PNGs separately.");
        File.WriteAllText(Path.Combine(output, "report.txt"), report.ToString());
        Debug.Log("Board frame Play Mode validation PASSED. Screenshots and report: " + output);
    }

    private static IEnumerator Scenarios()
    {
        yield return CaptureMotion("entrance", 1.5f);
        yield return Settle();
        Check(board.Width == 8 && board.Height == 8, "Game scene uses the expected 8x8 test fixture");
        Dump();
        yield return Capture("populated");
        // Avoid dying while inspecting; do not change serialized combat data.
        foreach (var attack in UnityEngine.Object.FindObjectsByType<EnemyAutoAttack>(FindObjectsSortMode.None))
            attack.StopAttacking();

        for (int side = 0; side < 4; side++)
        {
            ResetFixture();
            bool horizontal = side < 2;
            int edge = side % 2 == 0 ? 0 : 7;
            Gem a = At(horizontal ? 2 : edge, horizontal ? edge : 2);
            Gem b = At(horizontal ? 2 : (edge == 0 ? 1 : 6), horizontal ? (edge == 0 ? 1 : 6) : 2);
            SetColor(At(horizontal ? 0 : edge, horizontal ? edge : 0), 0);
            SetColor(At(horizontal ? 1 : edge, horizontal ? edge : 1), 0);
            SetColor(a, 1);
            SetColor(b, 0);
            yield return Swap(a, b, "edge-" + side);
        }

        foreach (var special in new[] { GemSpecialType.RowBomb, GemSpecialType.ColumnBomb, GemSpecialType.PoisonBomb, GemSpecialType.ColorCrystal })
        {
            ResetFixture();
            SetColor(At(0, 7), 0); SetColor(At(1, 7), 0); SetColor(At(2, 7), 1); SetColor(At(2, 6), 0);
            At(0, 7).SetSpecialType(special);
            yield return Capture(special + "-ready");
            yield return Swap(special == GemSpecialType.ColorCrystal ? At(0, 7) : At(2, 7),
                special == GemSpecialType.ColorCrystal ? At(1, 7) : At(2, 6), special.ToString());
        }
        for (int i = 0; i < 8; i++)
        {
            Check(board.TryGetRandomHintMove(out Gem a, out Gem b), "legal move available");
            yield return Swap(a, b, "cascade-" + i);
        }
        Check(cascades > 1, "several cascaded clears observed");
        var owner = UnityEngine.Object.FindObjectsByType<EnemyActor>(FindObjectsSortMode.None)
            .FirstOrDefault(enemy => enemy.IsInitialized && !enemy.IsDefeated);
        Check(owner != null, "obstacle test owner exists");
        Check(board.TryQueuePlaceBarricades(owner, 3, 3, 2, (EnemyBarricadeStyle)0), "barricades queued");
        yield return Settle();
        Check(board.TryQueuePinRandomGem(owner, 3), "chain queued");
        yield return Settle();
        Check(board.TryQueueFreezeRandomGem(owner, 3), "ice queued");
        yield return Settle();
        Check(board.TryQueueMineRandomCell(owner, 2), "hole queued");
        yield return CaptureMotion("obstacles", 1.5f);
        yield return Settle();
        yield return Capture("finished");
        Dump();
        AuditUiFrames();
    }

    private static IEnumerator Swap(Gem a, Gem b, string label)
    {
        int moves = board.CompletedValidPlayerMoves;
        board.StartCoroutine((IEnumerator)typeof(BoardController).GetMethod("TrySwap", Flags).Invoke(board, new object[] { a, b }));
        yield return CaptureMotion(label, 1.5f);
        yield return Settle();
        Check(board.CompletedValidPlayerMoves == moves + 1, label + " completed through production pipeline");
        yield return Capture(label + "-settled");
    }

    private static IEnumerator Settle()
    {
        float end = Time.realtimeSinceStartup + 30;
        while (board.IsBusy)
        {
            Check(Time.realtimeSinceStartup < end, "board settles within 30 seconds");
            Audit();
            yield return null;
        }
    }

    private static IEnumerator CaptureMotion(string label, float duration)
    {
        float end = Time.realtimeSinceStartup + duration;
        int i = 0;
        while (Time.realtimeSinceStartup < end)
        {
            yield return Capture(label + "-" + i++);
            yield return new WaitForSecondsRealtime(0.08f);
        }
    }

    private static IEnumerator Capture(string label)
    {
        yield return new WaitForEndOfFrame();
        Audit();
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(Path.Combine(output, label + ".png"), screenshot.EncodeToPNG());
        UnityEngine.Object.Destroy(screenshot);
    }

    private static void Audit()
    {
        Transform frame = board.transform.Find("BoardFrame");
        Check(frame != null, "frame exists");
        var pieces = frame.GetComponentsInChildren<SpriteRenderer>();
        Check(pieces.Length == 28, "all four corners and 24 edges exist");
        foreach (var piece in pieces)
        {
            Check(piece.maskInteraction == SpriteMaskInteraction.None, "frame is unmasked");
            Check(piece.sortingLayerName == "Effects", "frame uses loaded Effects layer");
        }
        int frameLayer = SortingLayer.GetLayerValueFromID(pieces[0].sortingLayerID);
        foreach (var renderer in board.GetComponentsInChildren<Renderer>())
            if (!renderer.transform.IsChildOf(frame))
                Check(SortingLayer.GetLayerValueFromID(renderer.sortingLayerID) < frameLayer,
                    renderer.name + " below frame");
    }

    private static void Dump()
    {
        report.AppendLine($"Game View {Screen.width}x{Screen.height}; layers: " + string.Join(", ", SortingLayer.layers.Select(l => l.name)));
        foreach (var r in board.GetComponentsInChildren<Renderer>(true))
            report.AppendLine($"{r.name}: {r.sortingLayerName}/{r.sortingOrder}, z={r.transform.position.z}, group={r.GetComponentInParent<SortingGroup>()}, shader={r.sharedMaterial?.shader.name}, queue={r.sharedMaterial?.renderQueue}");
        foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            report.AppendLine($"Canvas {canvas.name}: {canvas.renderMode}, scale={canvas.scaleFactor}, pixelPerfect={canvas.pixelPerfect}");
        foreach (var image in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i => i.name.Contains("Edge") || i.name.Contains("Corner")))
            report.AppendLine($"UI {image.transform.parent.name}/{image.name}: rect={image.rectTransform.rect}, position={image.rectTransform.position}, scale={image.transform.lossyScale}, PPU={image.sprite?.pixelsPerUnit}, tile multiplier={image.pixelsPerUnitMultiplier}");
    }

    private static void AuditUiFrames()
    {
        foreach (var fitter in UnityEngine.Object.FindObjectsByType<ResponsiveModularFrameFitter>(FindObjectsSortMode.None))
        {
            var root = (RectTransform)fitter.transform;
            RectTransform Piece(string name) => (RectTransform)root.Find(name);
            var corner = Piece("TopLeftCorner");
            var edge = Piece("TopEdge");
            var cornerImage = corner.GetComponent<Image>();
            var edgeImage = edge.GetComponent<Image>();
            float expectedThickness = corner.rect.width * edgeImage.sprite.rect.height / cornerImage.sprite.rect.width;
            Check(Mathf.Abs(edge.rect.height - expectedThickness) < 0.01f, root.name + " matching border thickness");
            float tileHeight = edgeImage.sprite.rect.height / (edgeImage.pixelsPerUnit * edgeImage.pixelsPerUnitMultiplier);
            Check(Mathf.Abs(tileHeight - edge.rect.height) < 0.01f, root.name + " uncropped tile height");
            Check(Mathf.Abs(edge.rect.width + corner.rect.width * 2 - root.rect.width) < 0.01f, root.name + " horizontal joins");
            Check(Mathf.Abs(Piece("LeftEdge").rect.width + corner.rect.height * 2 - root.rect.height) < 0.01f, root.name + " vertical joins");
            report.AppendLine("UI joins and tile density PASSED: " + root.name);
        }
    }

    private static void ResetFixture()
    {
        Check(!board.IsBusy, "fixture only changes settled board");
        for (int y = 0; y < board.Height; y++) for (int x = 0; x < board.Width; x++)
        { At(x, y).SetSpecialType(GemSpecialType.None); SetColor(At(x, y), (x + y * 2) % 6); }
    }
    private static Gem At(int x, int y) => ((Gem[,])typeof(BoardController).GetField("gems", Flags).GetValue(board))[x, y];
    private static void SetColor(Gem gem, int color) => gem.SetType((GemType)color, ((Sprite[])typeof(BoardController).GetField("gemSprites", Flags).GetValue(board))[color]);
    private static void Check(bool condition, string label) { if (!condition) throw new InvalidOperationException("Frame validation: " + label); }
}
