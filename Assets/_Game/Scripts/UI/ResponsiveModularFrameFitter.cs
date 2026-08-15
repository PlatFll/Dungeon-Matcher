using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ResponsiveModularFrameFitter : MonoBehaviour
{
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

        if (!Approximately(
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
                ) * 0.48f
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
