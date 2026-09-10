using UnityEngine;
using UnityEngine.UI;

/// <summary>Only owner of the safe gameplay root and the three screen sections.</summary>
[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public sealed class GameplayPixelLayoutController : MonoBehaviour
{
    public const int Inset = 4;
    public const int Gap = 6;
    // Two native 80px corners plus a 16px straight segment. The 64px button
    // has 40px clear space above/below it inside the 16px border.
    public const int BottomHeight = 176;
    public const int MinimumBattleHeight = 220;
    public const int PreferredBattleHeight = 290;
    public const int MaximumBattleHeight = 320;
    public const int AssetsPPU = 64;
    // Player section + section margins + enemy content padding + three 80px
    // slot allocations. Each slot includes its two 3px authored side gaps.
    public const int MinimumViewportWidth = 146 + 6 + 12 + 6 + 2 * 19 + 3 * 80;

    public struct Geometry
    {
        public int Scale;
        public Rect Safe, Viewport, Top, Board, Bottom;
        public float BoardTexelRatio;
        public bool FractionalBoardScale;
        public bool Fits;
    }

    public Geometry Current { get; private set; }
    public RectTransform TopHud { get; private set; }
    public RectTransform BoardArea { get; private set; }
    public RectTransform BottomHud { get; private set; }
    private RectTransform safeRoot;
    private Canvas canvas;
    private CanvasScaler scaler;
    private BoardVisuals board;
    private Vector2 lastScreen;
    private Rect lastSafe;
    private Vector2 lastBoard;
    private bool initialized;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static Rect? ValidationSafeArea;
    private int validationFrames;

    private void ValidateRenderedLayout(UnityEngine.Rendering.ScriptableRenderContext context, Camera renderedCamera)
    {
        // Inspect after all LateUpdate presentation and the board/PPC render
        // callbacks, never halfway through an animated frame's Update phase.
        if (renderedCamera != Camera.main || validationFrames <= 0 || --validationFrames != 0) return;
        var errors = GameplayPixelLayoutValidator.Validate(this, out string report);
        if (errors.Count == 0) Debug.Log("Pixel layout validated: " + report, this);
        else Debug.LogError("Pixel layout FAILED: " + report + string.Join("\n", errors), this);
    }
#endif

    public static Geometry Calculate(int width, int height, Rect safe, Vector2 boardPixels)
    {
        safe = Rect.MinMaxRect(Mathf.Ceil(Mathf.Max(0, safe.xMin)),
            Mathf.Ceil(Mathf.Max(0, safe.yMin)), Mathf.Floor(Mathf.Min(width, safe.xMax)),
            Mathf.Floor(Mathf.Min(height, safe.yMax)));
        // Start with the largest integer fitting actual native battle content.
        // Feasibility below includes the board and all vertical requirements.
        int scale = Mathf.Max(1, Mathf.FloorToInt(safe.width / (MinimumViewportWidth + 2 * Inset)));
        Geometry result = default;
        for (; scale >= 1; scale--)
        {
            float x = Mathf.Ceil(safe.xMin / scale) * scale + Inset * scale;
            float y = Mathf.Ceil(safe.yMin / scale) * scale + Inset * scale;
            // Even logical dimensions keep centered even-size art on the grid.
            float w = Mathf.Floor((safe.xMax - Inset * scale - x) / (2 * scale)) * 2;
            float h = Mathf.Floor((safe.yMax - Inset * scale - y) / (2 * scale)) * 2;
            float available = h - BottomHeight - 2 * Gap - MinimumBattleHeight;
            float fit = Mathf.Min(w * scale / boardPixels.x, available * scale / boardPixels.y);
            // Fill available space uniformly; whole texel steps made phone boards too small.
            float ratio = Mathf.Floor(fit * boardPixels.y / scale) * scale / boardPixels.y;
            result = new Geometry { Scale = scale, Safe = safe,
                Viewport = new Rect(x, y, w * scale, h * scale),
                BoardTexelRatio = ratio, FractionalBoardScale = Mathf.Abs(ratio - Mathf.Round(ratio)) > 0.00001f };
            if (w < MinimumViewportWidth || ratio <= 0) continue;
            float boardWidth = Mathf.Ceil(boardPixels.x * ratio / scale - 0.0001f);
            float boardHeight = Mathf.Ceil(boardPixels.y * ratio / scale - 0.0001f);
            float battleHeight = Mathf.Min(MaximumBattleHeight,
                h - BottomHeight - 2 * Gap - boardHeight);
            if (battleHeight < MinimumBattleHeight) continue;
            battleHeight = Mathf.Min(PreferredBattleHeight, battleHeight);
            // Anchor to the bottom safe inset; surplus background belongs above the stack.
            float bottom = 0;
            result.Bottom = new Rect(0, bottom, w, BottomHeight);
            result.Board = new Rect(Mathf.Floor((w - boardWidth) / 2), bottom + BottomHeight + Gap,
                boardWidth, boardHeight);
            result.Top = new Rect(0, result.Board.yMax + Gap, w, battleHeight);
            result.Fits = true;
            return result;
        }
        return result;
    }

    public void Initialize()
    {
        safeRoot = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>().rootCanvas;
        scaler = canvas.GetComponent<CanvasScaler>();
        TopHud = safeRoot.Find("TopHUD") as RectTransform;
        BoardArea = safeRoot.Find("GameArea") as RectTransform;
        BottomHud = safeRoot.Find("BottomHUD") as RectTransform;
        board = FindFirstObjectByType<BoardVisuals>();
        if (TryGetComponent(out SafeAreaFitter safeFitter)) safeFitter.enabled = false;
        initialized = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= ValidateRenderedLayout;
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += ValidateRenderedLayout;
#endif
        Refresh(true);
    }

    private void LateUpdate() { if (initialized) Refresh(false); }

    public void Refresh(bool force)
    {
        if (board == null || TopHud == null || BoardArea == null || BottomHud == null) return;
        Vector2 screen = new Vector2(Screen.width, Screen.height);
        Vector2 source = new Vector2(board.OuterLocalWidth, board.OuterLocalHeight) * AssetsPPU;
        Rect safe = Screen.safeArea;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        safe = ValidationSafeArea ?? safe;
#endif
        if (!force && screen == lastScreen && safe == lastSafe && source == lastBoard) return;
        lastScreen = screen; lastSafe = safe; lastBoard = source;
        Current = Calculate(Screen.width, Screen.height, safe, source);
        if (!Current.Fits)
        {
            Debug.LogError($"Gameplay pixel layout cannot fit screen {screen}, safe {safe}, board {source}.", this);
            return;
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = Current.Scale;
        scaler.referencePixelsPerUnit = AssetsPPU;
        canvas.scaleFactor = Current.Scale;
        canvas.referencePixelsPerUnit = AssetsPPU;
        canvas.pixelPerfect = true;
        // Root anchors describe physical screen edges, independent of scaler timing.
        safeRoot.anchorMin = new Vector2(Current.Viewport.xMin / Screen.width, Current.Viewport.yMin / Screen.height);
        safeRoot.anchorMax = new Vector2(Current.Viewport.xMax / Screen.width, Current.Viewport.yMax / Screen.height);
        safeRoot.offsetMin = safeRoot.offsetMax = Vector2.zero;
        safeRoot.localScale = Vector3.one;
        SetRect(TopHud, Current.Top);
        SetRect(BoardArea, Current.Board);
        SetRect(BottomHud, Current.Bottom);
        // The authored wave tracker was a sibling of TopHUD and otherwise
        // remained at the screen edge when the stack was letterboxed.
        RectTransform wave = safeRoot.Find("WaveTracker") as RectTransform;
        if (wave != null) wave.SetParent(TopHud, false);
        wave = TopHud.Find("WaveTracker") as RectTransform;
        if (wave != null)
        {
            wave.anchorMin = wave.anchorMax = wave.pivot = new Vector2(0.5f, 1);
            wave.anchoredPosition = Vector2.zero;
            wave.localScale = Vector3.one;
            if (wave.TryGetComponent(out Image waveImage)) GameplayPixelGrid.FitImage(waveImage, new Vector2(264,44));
        }
        Canvas.ForceUpdateCanvases();
        if (TopHud.TryGetComponent(out TopBattlePresentationController presentation)) presentation.RefreshPresentation();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        validationFrames = 5;
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= ValidateRenderedLayout;
#endif
    }

    public static void SetRect(RectTransform target, Rect rectangle)
    {
        target.anchorMin = target.anchorMax = target.pivot = Vector2.zero;
        target.anchoredPosition = rectangle.position;
        target.sizeDelta = rectangle.size;
        target.localScale = Vector3.one;
    }

    public static Rect ScreenRect(RectTransform target)
    {
        var corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Canvas owner = target.GetComponentInParent<Canvas>();
        Camera camera = owner.renderMode == RenderMode.ScreenSpaceOverlay ? null : owner.worldCamera;
        Vector2 lo = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 hi = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
        return Rect.MinMaxRect(Mathf.Min(lo.x, hi.x), Mathf.Min(lo.y, hi.y),
            Mathf.Max(lo.x, hi.x), Mathf.Max(lo.y, hi.y));
    }
}
