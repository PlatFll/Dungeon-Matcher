using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ModularHealthBarUI : MonoBehaviour
{
    private const string DefaultStyleResourcePath =
        "UI/ModularHealthBarStyle";

    private const string GeneratedRootName =
        "GeneratedModularHealthBar";

    [SerializeField]
    [Tooltip(
        "Optional per-bar style override. When empty, the shared Resources/UI/" +
        "ModularHealthBarStyle asset is used."
    )]
    private ModularHealthBarStyle styleOverride;

    private ModularHealthBarStyle style;
    private RectTransform rootRect;
    private Image legacyBackground;
    private Image legacyFill;

    private RectTransform generatedRoot;
    private RectTransform fillRect;
    private Image fillImage;
    private RectTransform startRect;
    private RectTransform middleRect;
    private RectTransform endRect;

    private float targetNormalized = 1f;
    private float displayedNormalized = 1f;
    private float lastRenderedNormalized = float.NaN;
    private Vector2 lastRootSize =
        new Vector2(float.NaN, float.NaN);

    private bool modularVisualBuilt;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallOnGameScene()
    {
        RectTransform[] rects =
            Object.FindObjectsByType<RectTransform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (RectTransform rect in rects)
        {
            if (rect == null ||
                (rect.name != "PlayerHPBarBackground" &&
                 rect.name != "EnemyHPBarBackground"))
            {
                continue;
            }

            if (!rect.TryGetComponent(
                    out ModularHealthBarUI _
                ))
            {
                rect.gameObject.AddComponent<
                    ModularHealthBarUI
                >();
            }
        }
    }

    private void Awake()
    {
        ResolveReferences();
        ResolveStyle();
        CaptureLegacyValue();
        TryBuildModularVisual();
    }

    private void OnEnable()
    {
        ResolveReferences();
        ResolveStyle();
        CaptureLegacyValue();
        TryBuildModularVisual();
        RefreshGeometry();
    }

    private void LateUpdate()
    {
        if (style == null)
        {
            ResolveStyle();
        }

        if (!modularVisualBuilt)
        {
            TryBuildModularVisual();
        }

        CaptureLegacyValue();

        if (!modularVisualBuilt)
        {
            return;
        }

        Vector2 rootSize =
            rootRect != null
                ? rootRect.rect.size
                : Vector2.zero;

        if (!Approximately(
                rootSize,
                lastRootSize
            ))
        {
            RefreshGeometry();
        }

        float speed =
            style != null
                ? style.FillAnimationSpeed
                : 0f;

        if (speed <= 0f)
        {
            displayedNormalized =
                targetNormalized;
        }
        else
        {
            displayedNormalized =
                Mathf.MoveTowards(
                    displayedNormalized,
                    targetNormalized,
                    speed *
                    Time.unscaledDeltaTime
                );
        }

        if (!Mathf.Approximately(
                displayedNormalized,
                lastRenderedNormalized
            ))
        {
            RefreshFillWidth();
        }
    }

    public void RefreshStyle()
    {
        ResolveStyle();

        if (generatedRoot != null)
        {
            Destroy(generatedRoot.gameObject);
            generatedRoot = null;
        }

        modularVisualBuilt = false;
        RestoreLegacyVisuals();
        TryBuildModularVisual();
    }

    private void ResolveStyle()
    {
        style =
            styleOverride != null
                ? styleOverride
                : Resources.Load<ModularHealthBarStyle>(
                    DefaultStyleResourcePath
                );
    }

    private void ResolveReferences()
    {
        rootRect =
            transform as RectTransform;

        legacyBackground =
            GetComponent<Image>();

        if (legacyFill == null)
        {
            legacyFill =
                FindLegacyFillImage(transform);
        }
    }

    private void CaptureLegacyValue()
    {
        if (legacyFill == null)
        {
            legacyFill =
                FindLegacyFillImage(transform);
        }

        if (legacyFill == null)
        {
            return;
        }

        targetNormalized =
            Mathf.Clamp01(
                legacyFill.fillAmount
            );

        if (!modularVisualBuilt)
        {
            displayedNormalized =
                targetNormalized;
        }
    }

    private void TryBuildModularVisual()
    {
        if (modularVisualBuilt ||
            rootRect == null ||
            style == null ||
            !style.HasCompleteFrame)
        {
            return;
        }

        Transform staleRoot =
            transform.Find(
                GeneratedRootName
            );

        if (staleRoot != null)
        {
            staleRoot.gameObject.SetActive(false);
            Destroy(staleRoot.gameObject);
        }

        generatedRoot =
            CreateRectTransform(
                GeneratedRootName,
                rootRect
            );

        StretchToParent(
            generatedRoot
        );

        generatedRoot.SetAsFirstSibling();

        fillImage =
            CreateImage(
                "HealthFill",
                generatedRoot,
                null
            );

        fillRect =
            fillImage.rectTransform;
        fillImage.color =
            style.FillColor;
        fillImage.raycastTarget = false;

        Image startImage =
            CreateImage(
                "StartPiece",
                generatedRoot,
                style.StartPiece
            );

        Image middleImage =
            CreateImage(
                "MiddlePiece",
                generatedRoot,
                style.MiddlePiece
            );

        Image endImage =
            CreateImage(
                "EndPiece",
                generatedRoot,
                style.EndPiece
            );

        startRect =
            startImage.rectTransform;
        middleRect =
            middleImage.rectTransform;
        endRect =
            endImage.rectTransform;

        startImage.type =
            Image.Type.Simple;
        middleImage.type =
            Image.Type.Tiled;
        endImage.type =
            Image.Type.Simple;

        startImage.raycastTarget = false;
        middleImage.raycastTarget = false;
        endImage.raycastTarget = false;

        HideLegacyVisuals();

        modularVisualBuilt = true;
        displayedNormalized = targetNormalized;
        lastRenderedNormalized = float.NaN;

        RefreshGeometry();
    }

    private void RefreshGeometry()
    {
        if (!modularVisualBuilt ||
            rootRect == null ||
            style == null)
        {
            return;
        }

        float rootWidth =
            Mathf.Max(0f, rootRect.rect.width);

        float rootHeight =
            Mathf.Max(0f, rootRect.rect.height);

        float startWidth =
            GetPieceWidthAtHeight(
                style.StartPiece,
                rootHeight
            );

        float endWidth =
            GetPieceWidthAtHeight(
                style.EndPiece,
                rootHeight
            );

        ConfigureCap(
            startRect,
            leftSide: true,
            startWidth,
            rootHeight
        );

        ConfigureCap(
            endRect,
            leftSide: false,
            endWidth,
            rootHeight
        );

        if (middleRect != null)
        {
            middleRect.anchorMin =
                Vector2.zero;
            middleRect.anchorMax =
                Vector2.one;
            middleRect.pivot =
                new Vector2(0.5f, 0.5f);
            middleRect.offsetMin =
                new Vector2(
                    startWidth,
                    0f
                );
            middleRect.offsetMax =
                new Vector2(
                    -endWidth,
                    0f
                );
            middleRect.localScale =
                Vector3.one;
        }

        if (fillRect != null)
        {
            float fillHeight =
                Mathf.Max(
                    0f,
                    rootHeight -
                    style.FillInsetVertical *
                    2f
                );

            fillRect.anchorMin =
                new Vector2(0f, 0.5f);
            fillRect.anchorMax =
                new Vector2(0f, 0.5f);
            fillRect.pivot =
                new Vector2(0f, 0.5f);
            fillRect.anchoredPosition =
                new Vector2(
                    style.FillInsetLeft,
                    0f
                );
            fillRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                fillHeight
            );
            fillRect.localScale =
                Vector3.one;
        }

        lastRootSize =
            new Vector2(
                rootWidth,
                rootHeight
            );

        RefreshFillWidth();
    }

    private void RefreshFillWidth()
    {
        if (!modularVisualBuilt ||
            rootRect == null ||
            fillRect == null ||
            style == null)
        {
            return;
        }

        float availableFillWidth =
            Mathf.Max(
                0f,
                rootRect.rect.width -
                style.FillInsetLeft -
                style.FillInsetRight
            );

        /*
         * Snap the moving edge to reference UI pixels. The CanvasScaler can
         * still scale the complete bar for the device, but the authored health
         * fill never ends on an arbitrary fractional reference pixel.
         */
        float visibleWidth =
            Mathf.Round(
                availableFillWidth *
                Mathf.Clamp01(
                    displayedNormalized
                )
            );

        fillRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            visibleWidth
        );

        lastRenderedNormalized =
            displayedNormalized;
    }

    private void HideLegacyVisuals()
    {
        if (legacyBackground != null)
        {
            legacyBackground.enabled = false;
        }

        if (legacyFill != null)
        {
            legacyFill.enabled = false;
        }
    }

    private void RestoreLegacyVisuals()
    {
        if (legacyBackground != null)
        {
            legacyBackground.enabled = true;
        }

        if (legacyFill != null)
        {
            legacyFill.enabled = true;
        }
    }

    private static void ConfigureCap(
        RectTransform cap,
        bool leftSide,
        float width,
        float height)
    {
        if (cap == null)
        {
            return;
        }

        float xAnchor =
            leftSide
                ? 0f
                : 1f;

        cap.anchorMin =
            new Vector2(xAnchor, 0.5f);
        cap.anchorMax =
            cap.anchorMin;
        cap.pivot =
            new Vector2(
                leftSide
                    ? 0f
                    : 1f,
                0.5f
            );
        cap.anchoredPosition =
            Vector2.zero;
        cap.sizeDelta =
            new Vector2(
                width,
                height
            );
        cap.localScale =
            Vector3.one;
    }

    private static float GetPieceWidthAtHeight(
        Sprite sprite,
        float targetHeight)
    {
        if (sprite == null ||
            sprite.rect.height <= 0f)
        {
            return 0f;
        }

        return
            targetHeight *
            (sprite.rect.width /
             sprite.rect.height);
    }

    private static Image FindLegacyFillImage(
        Transform root)
    {
        if (root == null)
        {
            return null;
        }

        for (int index = 0;
             index < root.childCount;
             index++)
        {
            Transform child =
                root.GetChild(index);

            if (child.name == GeneratedRootName)
            {
                continue;
            }

            if (child.name.Contains("HPBarFill") &&
                child.TryGetComponent(
                    out Image fill
                ))
            {
                return fill;
            }

            Image nested =
                FindLegacyFillImage(child);

            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static RectTransform CreateRectTransform(
        string objectName,
        RectTransform parent)
    {
        GameObject child =
            new GameObject(
                objectName,
                typeof(RectTransform)
            );

        RectTransform rect =
            child.GetComponent<RectTransform>();

        rect.SetParent(
            parent,
            false
        );

        return rect;
    }

    private static Image CreateImage(
        string objectName,
        RectTransform parent,
        Sprite sprite)
    {
        GameObject child =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        RectTransform rect =
            child.GetComponent<RectTransform>();

        rect.SetParent(
            parent,
            false
        );

        Image image =
            child.GetComponent<Image>();

        image.sprite = sprite;
        image.color = Color.white;

        return image;
    }

    private static void StretchToParent(
        RectTransform rect)
    {
        rect.anchorMin =
            Vector2.zero;
        rect.anchorMax =
            Vector2.one;
        rect.pivot =
            new Vector2(0.5f, 0.5f);
        rect.offsetMin =
            Vector2.zero;
        rect.offsetMax =
            Vector2.zero;
        rect.localScale =
            Vector3.one;
    }

    private static bool Approximately(
        Vector2 left,
        Vector2 right)
    {
        return
            Mathf.Approximately(left.x, right.x) &&
            Mathf.Approximately(left.y, right.y);
    }
}
