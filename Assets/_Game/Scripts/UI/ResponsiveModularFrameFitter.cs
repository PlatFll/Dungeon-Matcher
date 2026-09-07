using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ResponsiveModularFrameFitter : MonoBehaviour
{
    private const string GeneratedTileCanvasRootName =
        "GeneratedTileCanvasTiles";

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

    private Sprite tileCanvasCornerSprite;
    private Sprite tileCanvasNormalSprite;
    private Color tileCanvasColor = Color.white;
    private bool useTileCanvasMode;

    private Vector2 lastTargetSize =
        new Vector2(float.NaN, float.NaN);

    private void Awake()
    {
        ResolveReferences();
        RefreshFrame();
    }

    private void OnEnable()
    {
        /*
         * Awake already performs the initial build. Avoid rebuilding a second
         * time during AddComponent's Awake/OnEnable sequence; explicit callers
         * can still request RefreshFrame after configuring the component.
         */
        ResolveReferences();
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

        if (!Approximately(
                framedTarget.rect.size,
                lastTargetSize
            ))
        {
            RefreshFrame();
        }
    }

    public void ConfigureTileCanvas(
        Sprite cornerSprite,
        Sprite normalSprite,
        Color color)
    {
        tileCanvasCornerSprite =
            cornerSprite;

        tileCanvasNormalSprite =
            normalSprite;

        tileCanvasColor =
            color;

        useTileCanvasMode =
            TileCanvasFrameSpriteUtility
                .IsSquareTileCanvasPair(
                    cornerSprite,
                    normalSprite
                );

        ResolveReferences();
        RefreshFrame();
    }

    public void RefreshFrame()
    {
        ResolveReferences();

        if (frameRoot == null ||
            framedTarget == null)
        {
            return;
        }

        TryResolveTileCanvasSpritesFromTemplates();

        if (useTileCanvasMode &&
            TileCanvasFrameSpriteUtility
                .IsSquareTileCanvasPair(
                    tileCanvasCornerSprite,
                    tileCanvasNormalSprite
                ))
        {
            RefreshTileCanvasFrame();

            lastTargetSize =
                framedTarget.rect.size;

            return;
        }

        RefreshLegacyFrame();
    }

    private void TryResolveTileCanvasSpritesFromTemplates()
    {
        if (useTileCanvasMode &&
            TileCanvasFrameSpriteUtility
                .IsSquareTileCanvasPair(
                    tileCanvasCornerSprite,
                    tileCanvasNormalSprite
                ))
        {
            return;
        }

        Sprite cornerSprite =
            GetImageSprite(
                topLeftCorner
            );

        Sprite normalSprite =
            GetImageSprite(
                topEdge
            );

        if (!TileCanvasFrameSpriteUtility
                .IsSquareTileCanvasPair(
                    cornerSprite,
                    normalSprite
                ))
        {
            return;
        }

        tileCanvasCornerSprite =
            cornerSprite;

        tileCanvasNormalSprite =
            normalSprite;

        tileCanvasColor =
            GetTemplateColor();

        useTileCanvasMode = true;
    }

    private Color GetTemplateColor()
    {
        if (topEdge != null &&
            topEdge.TryGetComponent(
                out Image edgeImage
            ))
        {
            return edgeImage.color;
        }

        if (topLeftCorner != null &&
            topLeftCorner.TryGetComponent(
                out Image cornerImage
            ))
        {
            return cornerImage.color;
        }

        return Color.white;
    }

    private static Sprite GetImageSprite(
        RectTransform rect)
    {
        if (rect == null ||
            !rect.TryGetComponent(
                out Image image
            ))
        {
            return null;
        }

        return image.sprite;
    }

    private void RefreshTileCanvasFrame()
    {
        DisableLegacyTemplatePieces();
        RemoveGeneratedTileCanvasRoots();

        RectTransform generatedRoot =
            CreateRectTransform(
                GeneratedTileCanvasRootName,
                frameRoot
            );

        StretchToParent(
            generatedRoot
        );

        generatedRoot.SetAsLastSibling();

        RectMask2D mask =
            generatedRoot.gameObject
                .AddComponent<RectMask2D>();

        mask.padding = Vector4.zero;

        float pixelsPerSpritePixel =
            GetReferencePixelsPerSpritePixel(
                tileCanvasNormalSprite
            );

        float canvasPixels =
            TileCanvasFrameSpriteUtility
                .GetCanvasPixels(
                    tileCanvasNormalSprite
                );

        float canvasSize =
            canvasPixels *
            pixelsPerSpritePixel;

        if (canvasSize <= 0f)
        {
            return;
        }

        Rect targetRect =
            frameRoot.rect;

        CreateTileCanvasEdges(
            generatedRoot,
            targetRect,
            canvasSize,
            pixelsPerSpritePixel
        );

        CreateTileCanvasCorners(
            generatedRoot,
            targetRect,
            pixelsPerSpritePixel
        );
    }

    private void CreateTileCanvasEdges(
        RectTransform parent,
        Rect targetRect,
        float canvasSize,
        float pixelsPerSpritePixel)
    {
        int horizontalTileCount =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    targetRect.width /
                    canvasSize
                )
            );

        int verticalTileCount =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    targetRect.height /
                    canvasSize
                )
            );

        float coveredWidth =
            horizontalTileCount *
            canvasSize;

        float coveredHeight =
            verticalTileCount *
            canvasSize;

        float firstHorizontalCenter =
            targetRect.xMin -
            (coveredWidth -
             targetRect.width) *
            0.5f +
            canvasSize *
            0.5f;

        float firstVerticalCenter =
            targetRect.yMin -
            (coveredHeight -
             targetRect.height) *
            0.5f +
            canvasSize *
            0.5f;

        Rect leftBounds =
            GetNormalBounds(
                0f,
                pixelsPerSpritePixel
            );

        Rect topBounds =
            GetNormalBounds(
                -90f,
                pixelsPerSpritePixel
            );

        Rect rightBounds =
            GetNormalBounds(
                180f,
                pixelsPerSpritePixel
            );

        Rect bottomBounds =
            GetNormalBounds(
                90f,
                pixelsPerSpritePixel
            );

        float leftCanvasCenterX =
            targetRect.xMin -
            leftBounds.xMin;

        float rightCanvasCenterX =
            targetRect.xMax -
            rightBounds.xMax;

        float topCanvasCenterY =
            targetRect.yMax -
            topBounds.yMax;

        float bottomCanvasCenterY =
            targetRect.yMin -
            bottomBounds.yMin;

        for (int index = 0;
             index < horizontalTileCount;
             index++)
        {
            float centerX =
                firstHorizontalCenter +
                index *
                canvasSize;

            CreateTileCanvasImage(
                parent,
                $"TopEdge_{index}",
                tileCanvasNormalSprite,
                new Vector2(
                    centerX,
                    topCanvasCenterY
                ),
                -90f,
                pixelsPerSpritePixel
            );

            CreateTileCanvasImage(
                parent,
                $"BottomEdge_{index}",
                tileCanvasNormalSprite,
                new Vector2(
                    centerX,
                    bottomCanvasCenterY
                ),
                90f,
                pixelsPerSpritePixel
            );
        }

        for (int index = 0;
             index < verticalTileCount;
             index++)
        {
            float centerY =
                firstVerticalCenter +
                index *
                canvasSize;

            CreateTileCanvasImage(
                parent,
                $"LeftEdge_{index}",
                tileCanvasNormalSprite,
                new Vector2(
                    leftCanvasCenterX,
                    centerY
                ),
                0f,
                pixelsPerSpritePixel
            );

            CreateTileCanvasImage(
                parent,
                $"RightEdge_{index}",
                tileCanvasNormalSprite,
                new Vector2(
                    rightCanvasCenterX,
                    centerY
                ),
                180f,
                pixelsPerSpritePixel
            );
        }
    }

    private Rect GetNormalBounds(
        float rotationDegrees,
        float pixelsPerSpritePixel)
    {
        return TileCanvasFrameSpriteUtility
            .GetRotatedVisibleBoundsRelativeToCanvas(
                tileCanvasNormalSprite,
                pixelsPerSpritePixel,
                rotationDegrees
            );
    }

    private void CreateTileCanvasCorners(
        RectTransform parent,
        Rect targetRect,
        float pixelsPerSpritePixel)
    {
        CreateInsideCorner(
            parent,
            "TopLeftCorner",
            new Vector2(
                targetRect.xMin,
                targetRect.yMax
            ),
            90f,
            -1,
            1,
            pixelsPerSpritePixel
        );

        CreateInsideCorner(
            parent,
            "TopRightCorner",
            new Vector2(
                targetRect.xMax,
                targetRect.yMax
            ),
            0f,
            1,
            1,
            pixelsPerSpritePixel
        );

        CreateInsideCorner(
            parent,
            "BottomLeftCorner",
            new Vector2(
                targetRect.xMin,
                targetRect.yMin
            ),
            180f,
            -1,
            -1,
            pixelsPerSpritePixel
        );

        CreateInsideCorner(
            parent,
            "BottomRightCorner",
            new Vector2(
                targetRect.xMax,
                targetRect.yMin
            ),
            -90f,
            1,
            -1,
            pixelsPerSpritePixel
        );
    }

    private void CreateInsideCorner(
        RectTransform parent,
        string objectName,
        Vector2 targetCorner,
        float rotationDegrees,
        int horizontalSide,
        int verticalSide,
        float pixelsPerSpritePixel)
    {
        Rect bounds =
            TileCanvasFrameSpriteUtility
                .GetRotatedVisibleBoundsRelativeToCanvas(
                    tileCanvasCornerSprite,
                    pixelsPerSpritePixel,
                    rotationDegrees
                );

        float centerX =
            horizontalSide < 0
                ? targetCorner.x -
                  bounds.xMin
                : targetCorner.x -
                  bounds.xMax;

        float centerY =
            verticalSide < 0
                ? targetCorner.y -
                  bounds.yMin
                : targetCorner.y -
                  bounds.yMax;

        CreateTileCanvasImage(
            parent,
            objectName,
            tileCanvasCornerSprite,
            new Vector2(
                centerX,
                centerY
            ),
            rotationDegrees,
            pixelsPerSpritePixel
        );
    }

    private void CreateTileCanvasImage(
        RectTransform parent,
        string objectName,
        Sprite sprite,
        Vector2 logicalCanvasCenter,
        float rotationDegrees,
        float pixelsPerSpritePixel)
    {
        Image image =
            CreateImage(
                objectName,
                parent,
                sprite
            );

        RectTransform rect =
            image.rectTransform;

        Vector2 visibleCenterOffset =
            TileCanvasFrameSpriteUtility
                .GetVisibleCenterOffsetPixels(
                    sprite
                ) *
            pixelsPerSpritePixel;

        visibleCenterOffset =
            TileCanvasFrameSpriteUtility
                .Rotate(
                    visibleCenterOffset,
                    rotationDegrees
                );

        Vector2 visibleSize =
            TileCanvasFrameSpriteUtility
                .GetVisibleSizePixels(
                    sprite
                ) *
            pixelsPerSpritePixel;

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);
        rect.anchorMax =
            rect.anchorMin;
        rect.pivot =
            new Vector2(0.5f, 0.5f);
        rect.anchoredPosition =
            logicalCanvasCenter +
            visibleCenterOffset;
        rect.sizeDelta =
            visibleSize;
        rect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                rotationDegrees
            );
        rect.localScale =
            Vector3.one;

        image.type =
            Image.Type.Simple;
        image.color =
            tileCanvasColor;
        image.raycastTarget = false;
    }

    private void DisableLegacyTemplatePieces()
    {
        SetTemplateActive(
            topLeftCorner,
            false
        );
        SetTemplateActive(
            topRightCorner,
            false
        );
        SetTemplateActive(
            bottomLeftCorner,
            false
        );
        SetTemplateActive(
            bottomRightCorner,
            false
        );
        SetTemplateActive(
            topEdge,
            false
        );
        SetTemplateActive(
            bottomEdge,
            false
        );
        SetTemplateActive(
            leftEdge,
            false
        );
        SetTemplateActive(
            rightEdge,
            false
        );
    }

    private static void SetTemplateActive(
        RectTransform template,
        bool active)
    {
        if (template != null)
        {
            template.gameObject.SetActive(
                active
            );
        }
    }

    private void RemoveGeneratedTileCanvasRoots()
    {
        if (frameRoot == null)
        {
            return;
        }

        int removalIndex = 0;

        while (true)
        {
            Transform existing =
                frameRoot.Find(
                    GeneratedTileCanvasRootName
                );

            if (existing == null)
            {
                break;
            }

            existing.name =
                GeneratedTileCanvasRootName +
                "_Removing_" +
                removalIndex;

            removalIndex++;

            existing.gameObject.SetActive(false);

            if (Application.isPlaying)
            {
                Destroy(
                    existing.gameObject
                );
            }
            else
            {
                DestroyImmediate(
                    existing.gameObject
                );
            }
        }
    }

    private void RefreshLegacyFrame()
    {
        if (topLeftCorner == null)
        {
            return;
        }

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

        float maximumCornerSize =
            Mathf.Max(
                1f,
                Mathf.Min(
                    framedTarget.rect.width,
                    framedTarget.rect.height
                ) *
                0.48f
            );

        float cornerSize =
            Mathf.Min(
                nativeCornerSize,
                maximumCornerSize
            );

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
            true,
            cornerSize,
            thickness
        );

        ConfigureHorizontalEdge(
            bottomEdge,
            false,
            cornerSize,
            thickness
        );

        ConfigureVerticalEdge(
            leftEdge,
            true,
            cornerSize,
            thickness
        );

        ConfigureVerticalEdge(
            rightEdge,
            false,
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
            FindDirectChild(
                "TopLeftCorner"
            );
        topRightCorner =
            FindDirectChild(
                "TopRightCorner"
            );
        bottomLeftCorner =
            FindDirectChild(
                "BottomLeftCorner"
            );
        bottomRightCorner =
            FindDirectChild(
                "BottomRightCorner"
            );

        topEdge =
            FindDirectChild(
                "TopEdge"
            );
        bottomEdge =
            FindDirectChild(
                "BottomEdge"
            );
        leftEdge =
            FindDirectChild(
                "LeftEdge"
            );
        rightEdge =
            FindDirectChild(
                "RightEdge"
            );
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
        corner.anchoredPosition =
            position;
        corner.sizeDelta =
            new Vector2(size, size);
    }

    private static void ConfigureHorizontalEdge(
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
                cornerSize * 2f
            );

        edge.sizeDelta =
            new Vector2(
                verticalLength,
                thickness
            );
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
            frameRoot.Find(
                childName
            ) as RectTransform;
    }

    private static RectTransform CreateRectTransform(
        string objectName,
        RectTransform parent)
    {
        GameObject rectObject =
            new GameObject(
                objectName,
                typeof(RectTransform)
            );

        rectObject.layer =
            parent.gameObject.layer;

        RectTransform rectTransform =
            rectObject.GetComponent<RectTransform>();

        rectTransform.SetParent(
            parent,
            false
        );

        return rectTransform;
    }

    private static Image CreateImage(
        string objectName,
        RectTransform parent,
        Sprite sprite)
    {
        GameObject imageObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        imageObject.layer =
            parent.gameObject.layer;

        RectTransform imageRect =
            imageObject.GetComponent<RectTransform>();

        imageRect.SetParent(
            parent,
            false
        );

        Image image =
            imageObject.GetComponent<Image>();

        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = false;
        image.pixelsPerUnitMultiplier = 1f;

        return image;
    }

    private static void StretchToParent(
        RectTransform rectTransform)
    {
        rectTransform.anchorMin =
            Vector2.zero;
        rectTransform.anchorMax =
            Vector2.one;
        rectTransform.offsetMin =
            Vector2.zero;
        rectTransform.offsetMax =
            Vector2.zero;
        rectTransform.localScale =
            Vector3.one;
    }

    private static bool Approximately(
        Vector2 left,
        Vector2 right)
    {
        return
            Mathf.Approximately(
                left.x,
                right.x
            ) &&
            Mathf.Approximately(
                left.y,
                right.y
            );
    }
}
