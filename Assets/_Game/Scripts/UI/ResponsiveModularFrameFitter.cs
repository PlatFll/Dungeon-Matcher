using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ResponsiveModularFrameFitter : MonoBehaviour
{
    // Opt-in for independently authored, upright native pieces. Existing production
    // callers retain their original rotated-edge layout unless explicitly configured.
    [SerializeField] private bool nativePieceLayout;
    private bool dungeonPhysicalPixels;
    private float lastPhysicalUnit;
    [SerializeField] private Sprite nativeConnector;
    [SerializeField] private float connectorPitch = 40;

    public void ConfigureNativePieces(Sprite connector, float pitch, float physicalUnit = 0)
    {
        nativePieceLayout = true;
        dungeonPhysicalPixels = physicalUnit > 0;
        nativeConnector = connector;
        connectorPitch = Mathf.Max(1, pitch);
        RefreshFrame();
    }

    [SerializeField] private RectTransform fittedOpening;
    private readonly Vector3[] openingCorners = new Vector3[4];
    public void FollowOpening(RectTransform opening)
    {
        fittedOpening = opening;
        RefreshFrame();
    }

    private void FitOpening(float unit, float corner, float thickness)
    {
        if (!fittedOpening || !framedTarget || !framedTarget.parent) return;
        fittedOpening.GetWorldCorners(openingCorners);
        var parent = (RectTransform)framedTarget.parent;
        Vector2 lo = parent.InverseTransformPoint(openingCorners[0]);
        Vector2 hi = parent.InverseTransformPoint(openingCorners[2]);
        float inset = (corner + thickness) * .5f * unit;
        framedTarget.anchorMin = framedTarget.anchorMax = framedTarget.pivot = Vector2.zero;
        framedTarget.anchoredPosition = lo - parent.rect.min - Vector2.one * inset;
        framedTarget.sizeDelta = hi - lo + Vector2.one * (2 * inset);
    }

    private void RefreshNativePieces()
    {
        float unit = dungeonPhysicalPixels ? DungeonPixelLabBattleArt.Unit(GetComponentInParent<Canvas>()) : GetReferencePixelsPerSpritePixel(topLeftCorner.GetComponent<Image>().sprite);
        lastPhysicalUnit=unit;
        float c = topLeftCorner.GetComponent<Image>().sprite.rect.width;
        float t = topEdge.GetComponent<Image>().sprite.rect.height;
        FitOpening(unit, c, t);
        float w = Mathf.Round(framedTarget.rect.width / unit), h = Mathf.Round(framedTarget.rect.height / unit);
        // Never squash corners to satisfy an impossible size. The parent owns its
        // minimum size; hide the frame only while its layout has no usable bounds.
        bool fits = w >= 2 * c && h >= 2 * c;
        foreach (var image in frameRoot.GetComponentsInChildren<Image>(true)) image.enabled = fits;
        if (!fits) { lastTargetSize = framedTarget.rect.size; return; }
        float center = (c - 1) / 2, railStart = center - (t - 1) / 2;
        NativeRect(topLeftCorner, 0, 0, c, c, unit);
        NativeRect(topRightCorner, w - c, 0, c, c, unit);
        NativeRect(bottomLeftCorner, 0, h - c, c, c, unit);
        NativeRect(bottomRightCorner, w - c, h - c, c, c, unit);
        NativeRect(topEdge, center, railStart, w - 2 * center, t, unit);
        NativeRect(bottomEdge, center, h - railStart - t, w - 2 * center, t, unit);
        NativeRect(leftEdge, railStart, center, t, h - 2 * center, unit);
        NativeRect(rightEdge, w - railStart - t, center, t, h - 2 * center, unit);
        foreach (var edge in new[] { topEdge, bottomEdge, leftEdge, rightEdge })
        {
            var image = edge.GetComponent<Image>();
            image.type = Image.Type.Tiled;
            // Respect the source PPU: native kits include both 64- and 100-PPU imports.
            image.pixelsPerUnitMultiplier = image.canvas.referencePixelsPerUnit / image.sprite.pixelsPerUnit / unit;
            edge.SetAsFirstSibling();
        }
        var connectors = frameRoot.Find("NativeConnectors") as RectTransform;
        if (!connectors)
        {
            connectors = new GameObject("NativeConnectors", typeof(RectTransform)).GetComponent<RectTransform>();
            connectors.SetParent(frameRoot, false);
            connectors.anchorMin = Vector2.zero; connectors.anchorMax = Vector2.one;
            connectors.offsetMin = connectors.offsetMax = Vector2.zero;
        }
        int used = 0;
        if (nativeConnector)
        {
            int nx = ConnectorCount(w, c), ny = ConnectorCount(h, c);
            for (int i = 1; i <= nx; i++)
            {
                float x = Mathf.Round(center + (w - c) * i / (nx + 1));
                NativeConnectorAt(connectors, used++, x, center, unit);
                NativeConnectorAt(connectors, used++, x, h - 1 - center, unit);
            }
            for (int i = 1; i <= ny; i++)
            {
                float y = Mathf.Round(center + (h - c) * i / (ny + 1));
                NativeConnectorAt(connectors, used++, center, y, unit);
                NativeConnectorAt(connectors, used++, w - 1 - center, y, unit);
            }
        }
        for (int i = used; i < connectors.childCount; i++) connectors.GetChild(i).gameObject.SetActive(false);
        connectors.SetAsLastSibling();
        lastTargetSize = framedTarget.rect.size;
    }

    private int ConnectorCount(float length, float corner)
    {
        int n = Mathf.FloorToInt((length - 2 * corner) / connectorPitch);
        return n < 1 ? 0 : n % 2 == 0 ? n - 1 : n;
    }

    private void NativeConnectorAt(RectTransform parent, int index, float x, float y, float unit)
    {
        RectTransform r;
        if (index < parent.childCount) r = (RectTransform)parent.GetChild(index);
        else
        {
            r = new GameObject("Connector_" + index, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            r.SetParent(parent, false);
        }
        r.gameObject.SetActive(true);
        var image = r.GetComponent<Image>(); image.enabled = true; image.sprite = nativeConnector;
        image.type = Image.Type.Simple; image.raycastTarget = false;
        NativeRect(r, x - (nativeConnector.rect.width - 1) / 2,
            y - (nativeConnector.rect.height - 1) / 2, nativeConnector.rect.width, nativeConnector.rect.height, unit);
    }

    private static void NativeRect(RectTransform r, float x, float y, float w, float h, float unit)
    {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.localRotation = Quaternion.identity; r.localScale = Vector3.one;
        r.anchoredPosition = new Vector2(x, -y) * unit; r.sizeDelta = new Vector2(w, h) * unit;
    }

    private RectTransform frameRoot;
    private RectTransform framedTarget;

    private RectTransform topLeftCorner;
    private RectTransform topRightCorner;
    private RectTransform bottomLeftCorner;
    private RectTransform bottomRightCorner;

    private RectTransform topEdge;
    private RectTransform bottomEdge;
    private RectTransform leftEdge;
    private RectTransform rightEdge;

    private Vector2 lastTargetSize =
        new Vector2(float.NaN, float.NaN);

    private void Awake()
    {
        ResolveReferences();
        RefreshFrame();
    }

    private void OnEnable()
    {
        ResolveReferences();
        RefreshFrame();
    }

    private void LateUpdate()
    {
        if (framedTarget == null)
        {
            ResolveReferences();
        }

        if (framedTarget == null)
        {
            return;
        }

        if (fittedOpening || (dungeonPhysicalPixels && !Mathf.Approximately(lastPhysicalUnit,DungeonPixelLabBattleArt.Unit(GetComponentInParent<Canvas>()))) || !Approximately(
                framedTarget.rect.size,
                lastTargetSize
            ))
        {
            RefreshFrame();
        }
    }

    public void RefreshFrame()
    {
        ResolveReferences();

        if (frameRoot == null ||
            framedTarget == null ||
            topLeftCorner == null)
        {
            return;
        }

        if (nativePieceLayout) { RefreshNativePieces(); return; }

        float nativeCornerSize =
            GetNativeUiWidth(
                topLeftCorner
            );

        float nativeThickness =
            GetNativeUiHeight(
                topEdge
            );

        if (nativeCornerSize <= 0f)
        {
            nativeCornerSize =
                Mathf.Abs(
                    topLeftCorner.rect.width
                );
        }

        if (nativeThickness <= 0f)
        {
            nativeThickness =
                topEdge != null
                    ? Mathf.Abs(
                        topEdge.rect.height
                    )
                    : 0f;
        }

        float cornerSize = nativeCornerSize;

        float frameScale =
            nativeCornerSize > 0f
                ? cornerSize /
                  nativeCornerSize
                : 1f;

        float thickness =
            Mathf.Max(
                1f,
                nativeThickness *
                frameScale
            );

        ConfigureCorner(
            topLeftCorner,
            new Vector2(0f, 1f),
            new Vector2(
                cornerSize * 0.5f,
                -cornerSize * 0.5f
            ),
            cornerSize
        );

        ConfigureCorner(
            topRightCorner,
            new Vector2(1f, 1f),
            new Vector2(
                -cornerSize * 0.5f,
                -cornerSize * 0.5f
            ),
            cornerSize
        );

        ConfigureCorner(
            bottomLeftCorner,
            new Vector2(0f, 0f),
            new Vector2(
                cornerSize * 0.5f,
                cornerSize * 0.5f
            ),
            cornerSize
        );

        ConfigureCorner(
            bottomRightCorner,
            new Vector2(1f, 0f),
            new Vector2(
                -cornerSize * 0.5f,
                cornerSize * 0.5f
            ),
            cornerSize
        );

        ConfigureHorizontalEdge(
            topEdge,
            top: true,
            cornerSize,
            thickness
        );

        ConfigureHorizontalEdge(
            bottomEdge,
            top: false,
            cornerSize,
            thickness
        );

        ConfigureVerticalEdge(
            leftEdge,
            left: true,
            cornerSize,
            thickness
        );

        ConfigureVerticalEdge(
            rightEdge,
            left: false,
            cornerSize,
            thickness
        );

        lastTargetSize =
            framedTarget.rect.size;
    }

    private void ResolveReferences()
    {
        frameRoot =
            transform as RectTransform;

        framedTarget =
            transform.parent as RectTransform;

        if (frameRoot == null)
        {
            return;
        }

        topLeftCorner =
            FindDirectChild("TopLeftCorner");
        topRightCorner =
            FindDirectChild("TopRightCorner");
        bottomLeftCorner =
            FindDirectChild("BottomLeftCorner");
        bottomRightCorner =
            FindDirectChild("BottomRightCorner");

        topEdge =
            FindDirectChild("TopEdge");
        bottomEdge =
            FindDirectChild("BottomEdge");
        leftEdge =
            FindDirectChild("LeftEdge");
        rightEdge =
            FindDirectChild("RightEdge");
    }

    private static void ConfigureCorner(
        RectTransform corner,
        Vector2 anchor,
        Vector2 position,
        float size)
    {
        if (corner == null)
        {
            return;
        }

        corner.anchorMin = anchor;
        corner.anchorMax = anchor;
        corner.pivot =
            new Vector2(0.5f, 0.5f);
        corner.anchoredPosition = position;
        corner.sizeDelta =
            new Vector2(size, size);
    }

    private void ConfigureHorizontalEdge(
        RectTransform edge,
        bool top,
        float cornerSize,
        float thickness)
    {
        if (edge == null)
        {
            return;
        }

        float yAnchor =
            top
                ? 1f
                : 0f;

        edge.anchorMin =
            new Vector2(0f, yAnchor);
        edge.anchorMax =
            new Vector2(1f, yAnchor);
        edge.pivot =
            new Vector2(0.5f, 0.5f);
        edge.offsetMin =
            new Vector2(
                cornerSize,
                top
                    ? -thickness
                    : 0f
            );
        edge.offsetMax =
            new Vector2(
                -cornerSize,
                top
                    ? 0f
                    : thickness
            );
        ConfigureTileScale(edge, thickness);
    }

    private void ConfigureVerticalEdge(
        RectTransform edge,
        bool left,
        float cornerSize,
        float thickness)
    {
        if (edge == null)
        {
            return;
        }

        edge.anchorMin =
            new Vector2(
                left ? 0f : 1f,
                0.5f
            );
        edge.anchorMax =
            edge.anchorMin;
        edge.pivot =
            new Vector2(0.5f, 0.5f);
        edge.anchoredPosition =
            new Vector2(
                left
                    ? thickness * 0.5f
                    : -thickness * 0.5f,
                0f
            );

        float verticalLength =
            Mathf.Max(
                0f,
                framedTarget.rect.height -
                Mathf.Min(cornerSize, framedTarget.rect.height < cornerSize * 2 ? thickness : cornerSize) * 2f
            );

        edge.sizeDelta =
            new Vector2(
                verticalLength,
                thickness
            );
        ConfigureTileScale(edge, thickness);
    }

    private void ConfigureTileScale(RectTransform edge, float thickness)
    {
        if (!edge.TryGetComponent(out Image image) || image.sprite == null) return;
        // Image.Type.Tiled crops rather than scales tiles to a smaller rect.
        // Match its native tile height to the corner's derived border height,
        // including Canvas reference PPU, so all source pixels share one scale.
        image.pixelsPerUnitMultiplier = GetNativeUiHeight(edge) / thickness;
    }

    private float GetNativeUiWidth(
        RectTransform piece)
    {
        if (piece == null ||
            !piece.TryGetComponent(
                out Image image
            ) ||
            image.sprite == null)
        {
            return 0f;
        }

        return
            image.sprite.rect.width *
            GetReferencePixelsPerSpritePixel(
                image.sprite
            );
    }

    private float GetNativeUiHeight(
        RectTransform piece)
    {
        if (piece == null ||
            !piece.TryGetComponent(
                out Image image
            ) ||
            image.sprite == null)
        {
            return 0f;
        }

        return
            image.sprite.rect.height *
            GetReferencePixelsPerSpritePixel(
                image.sprite
            );
    }

    private float GetReferencePixelsPerSpritePixel(
        Sprite sprite)
    {
        if (sprite == null ||
            sprite.pixelsPerUnit <= 0f)
        {
            return 1f;
        }

        CanvasScaler scaler =
            GetComponentInParent<CanvasScaler>();

        float referencePixelsPerUnit =
            scaler != null
                ? scaler.referencePixelsPerUnit
                : sprite.pixelsPerUnit;

        return
            referencePixelsPerUnit /
            sprite.pixelsPerUnit;
    }

    private RectTransform FindDirectChild(
        string childName)
    {
        if (frameRoot == null)
        {
            return null;
        }

        return
            frameRoot.Find(childName)
            as RectTransform;
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
