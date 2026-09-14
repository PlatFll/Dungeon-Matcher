using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Plays the studio ident on startup, then loads the menu.</summary>
[DisallowMultipleComponent]
public sealed class StudioIdentPlayer : MonoBehaviour
{
    public const string ScenePath = "Assets/_Game/Scenes/Startup.unity";
    public const int FrameCount = 30;
    public const int FramesPerSecond = 12;
    public const int FrameWidth = 480;
    public const int FrameHeight = 270;
    public const int AtlasColumns = 5;
    public const int AtlasRows = 6;

    [SerializeField] private Texture2D frameAtlas;
    [SerializeField] private string nextScenePath = "Assets/_Game/Scenes/MainMenu.unity";
    [SerializeField, Min(0f)] private float finalLogoHoldSeconds = 1.5f;

    public float FinalLogoHoldSeconds => finalLogoHoldSeconds;

    private Canvas canvas;
    private RawImage backdrop;
    private RawImage artwork;
    private Rect lastSafeArea;
    private bool paused;
    private bool skipResumeDelta;
    private int currentFrame = -1;

    private void Awake()
    {
        var cameraObject = new GameObject("Startup Camera", typeof(Camera));
        cameraObject.transform.SetParent(transform, false);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(17, 29, 48, 255);
        camera.cullingMask = 0;
        camera.allowHDR = false;
        camera.allowMSAA = false;

        if (!HasValidAtlas()) return;

        var canvasObject = new GameObject("Studio Ident Canvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        canvas.enabled = false;

        backdrop = CreateImage("Sky", canvas.transform);
        backdrop.rectTransform.anchorMax = Vector2.one;
        backdrop.rectTransform.offsetMin = Vector2.zero;
        backdrop.rectTransform.offsetMax = Vector2.zero;
        artwork = CreateImage("SmallHold Games", canvas.transform);
        FitArtwork();
        ShowFrame(0);
    }

    private RawImage CreateImage(string objectName, Transform parent)
    {
        var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(RawImage));
        imageObject.transform.SetParent(parent, false);
        RawImage image = imageObject.GetComponent<RawImage>();
        image.texture = frameAtlas;
        image.raycastTarget = false;
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.zero;
        image.rectTransform.pivot = Vector2.zero;
        return image;
    }

    private bool HasValidAtlas()
    {
        return frameAtlas != null &&
            frameAtlas.width == FrameWidth * AtlasColumns &&
            frameAtlas.height == FrameHeight * AtlasRows;
    }

    private IEnumerator Start()
    {
        if (HasValidAtlas())
        {
            canvas.enabled = true;
            float elapsed = 0f;
            float duration = (float)FrameCount / FramesPerSecond;
            while (elapsed < duration)
            {
                ShowFrame(Mathf.Min((int)(elapsed * FramesPerSecond), FrameCount - 1));
                yield return null;
                if (!paused && !skipResumeDelta) elapsed += Time.unscaledDeltaTime;
                skipResumeDelta = false;
            }

            // Keep the completed mark visible throughout loading, including on a
            // slow frame that might otherwise skip the last authored pose.
            ShowFrame(FrameCount - 1);
            float hold = 0f;
            while (hold < finalLogoHoldSeconds)
            {
                yield return null;
                if (!paused && !skipResumeDelta) hold += Time.unscaledDeltaTime;
                skipResumeDelta = false;
            }
        }
        else
        {
            Debug.LogWarning("SmallHold ident atlas is missing or has the wrong dimensions; continuing to the menu.", this);
        }

        if (!Application.CanStreamedLevelBeLoaded(nextScenePath) || nextScenePath == gameObject.scene.path)
        {
            Debug.LogError($"Startup cannot load '{nextScenePath}'. Enable the MainMenu scene in Build Settings.", this);
            yield break;
        }

        // Do not initialize the menu or gameplay behind the animation.
        yield return SceneManager.LoadSceneAsync(nextScenePath, LoadSceneMode.Single);
    }

    private void Update()
    {
        if (artwork != null && lastSafeArea != UnityEngine.Device.Screen.safeArea)
            FitArtwork();
    }

    private void FitArtwork()
    {
        lastSafeArea = UnityEngine.Device.Screen.safeArea;
        Rect rect = CalculateFrameRect(lastSafeArea);
        artwork.rectTransform.anchoredPosition = rect.position;
        artwork.rectTransform.sizeDelta = rect.size;
    }

    public static Rect CalculateFrameRect(Rect safeArea)
    {
        float fit = Mathf.Min(safeArea.width / FrameWidth, safeArea.height / FrameHeight);
        float scale = fit >= 1f ? Mathf.Floor(fit) : Mathf.Max(0f, fit);
        Vector2 size = new Vector2(FrameWidth, FrameHeight) * scale;
        Vector2 origin = safeArea.center - size * 0.5f;
        origin.x = Mathf.Round(origin.x);
        origin.y = Mathf.Round(origin.y);
        return new Rect(origin, size);
    }

    private void ShowFrame(int frame)
    {
        if (frame == currentFrame) return;
        currentFrame = frame;
        float x = (frame % AtlasColumns) / (float)AtlasColumns;
        float y = 1f - (frame / AtlasColumns + 1f) / AtlasRows;
        artwork.uvRect = new Rect(x, y, 1f / AtlasColumns, 1f / AtlasRows);
        // Every frame has a solid sky border. Sampling one border texel fills
        // portrait/ultrawide margins with exactly the same sky, without stretching
        // the island, changing its aspect ratio, or keeping the atlas CPU-readable.
        backdrop.uvRect = new Rect(x + 0.5f / frameAtlas.width, y + 0.5f / frameAtlas.height, 0f, 0f);
    }

    private void OnApplicationPause(bool pause)
    {
        paused = pause;
        if (!pause) skipResumeDelta = true;
    }
}
